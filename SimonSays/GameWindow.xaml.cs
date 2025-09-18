using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SimonSays;

public partial class GameWindow : UserControl
{
    public event EventHandler? BackToMenuRequested;

    public static readonly List<string> Words = new() { "WELCOME", "123HI", "CYBERPUNK", "MATRIX", "ALGORITHM", "SEQUENCE" };

    private string _currentWord = "";
    private int _currentLetterIndex = 0;
    private List<char> _uniqueLetters = new();
    private DispatcherTimer _flashTimer = new();
    private int _flashIndex = 0;
    private bool _gameActive = false;
    private List<Button> _choiceButtons = new();
    private DispatcherTimer _particleTimer = new();

    public GameWindow()
    {
        InitializeComponent();
        _flashTimer.Tick += FlashTimer_Tick;
        _flashTimer.Interval = TimeSpan.FromMilliseconds(500);
        _particleTimer.Tick += ParticleTimer_Tick;
        _particleTimer.Interval = TimeSpan.FromMilliseconds(800);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var fadeIn = Resources["FadeInAnimation"] as Storyboard;
        fadeIn?.Begin(this);

        CreateFloatingGrid();
        StartParticleSystem();
        StartPulseAnimations();
        StartGame();
    }

    private void CreateFloatingGrid()
    {
        var gridSize = 40;
        var canvasWidth = Math.Max(1200, ActualWidth + 200);
        var canvasHeight = Math.Max(800, ActualHeight + 200);

        // Create main grid
        for (int x = -gridSize * 2; x <= canvasWidth + gridSize; x += gridSize)
        {
            var verticalLine = new Line
            {
                X1 = x,
                Y1 = -gridSize * 2,
                X2 = x,
                Y2 = canvasHeight + gridSize,
                Stroke = new SolidColorBrush(Color.FromArgb(60, 0, 255, 65)),
                StrokeThickness = 1
            };
            GridCanvas.Children.Add(verticalLine);
        }

        for (int y = -gridSize * 2; y <= canvasHeight + gridSize; y += gridSize)
        {
            var horizontalLine = new Line
            {
                X1 = -gridSize * 2,
                Y1 = y,
                X2 = canvasWidth + gridSize,
                Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(60, 0, 255, 65)),
                StrokeThickness = 1
            };
            GridCanvas.Children.Add(horizontalLine);
        }

        // Create accent lines every 5th line
        for (int x = -gridSize * 2; x <= canvasWidth + gridSize; x += gridSize * 5)
        {
            var accentLine = new Line
            {
                X1 = x,
                Y1 = -gridSize * 2,
                X2 = x,
                Y2 = canvasHeight + gridSize,
                Stroke = new SolidColorBrush(Color.FromArgb(120, 0, 255, 65)),
                StrokeThickness = 2
            };
            GridCanvas.Children.Add(accentLine);
        }

        for (int y = -gridSize * 2; y <= canvasHeight + gridSize; y += gridSize * 5)
        {
            var accentLine = new Line
            {
                X1 = -gridSize * 2,
                Y1 = y,
                X2 = canvasWidth + gridSize,
                Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(120, 0, 255, 65)),
                StrokeThickness = 2
            };
            GridCanvas.Children.Add(accentLine);
        }

        CreateCircuitPattern();
        CreateDataNodes();
        StartGridAnimation();
    }

    private void CreateCircuitPattern()
    {
        var random = new Random();
        var canvasWidth = Math.Max(1200, ActualWidth + 200);
        var canvasHeight = Math.Max(800, ActualHeight + 200);

        for (int i = 0; i < 15; i++)
        {
            var x = random.Next(0, (int)canvasWidth);
            var y = random.Next(0, (int)canvasHeight);
            var size = random.Next(20, 80);

            var circuit = new Rectangle
            {
                Width = size,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromArgb(40, 0, 255, 65)),
                RenderTransform = new RotateTransform(random.Next(0, 360))
            };

            Canvas.SetLeft(circuit, x);
            Canvas.SetTop(circuit, y);
            CircuitCanvas.Children.Add(circuit);

            // Add perpendicular connections
            var connection = new Rectangle
            {
                Width = 4,
                Height = size / 2,
                Fill = new SolidColorBrush(Color.FromArgb(30, 0, 255, 65))
            };

            Canvas.SetLeft(connection, x + size / 2);
            Canvas.SetTop(connection, y - size / 4);
            CircuitCanvas.Children.Add(connection);
        }
    }

    private void CreateDataNodes()
    {
        var random = new Random();
        var canvasWidth = Math.Max(1200, ActualWidth + 200);
        var canvasHeight = Math.Max(800, ActualHeight + 200);

        // Create glowing data nodes
        for (int i = 0; i < 25; i++)
        {
            var x = random.Next(50, (int)canvasWidth - 50);
            var y = random.Next(50, (int)canvasHeight - 50);

            var node = new Ellipse
            {
                Width = random.Next(3, 8),
                Height = random.Next(3, 8),
                Fill = new SolidColorBrush(Color.FromArgb(120, 0, 255, 65)),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromArgb(255, 0, 255, 65),
                    BlurRadius = 6,
                    ShadowDepth = 0
                }
            };

            Canvas.SetLeft(node, x);
            Canvas.SetTop(node, y);
            GridCanvas.Children.Add(node);

            // Add pulsing animation to nodes
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(random.NextDouble() * 3 + 1),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            node.BeginAnimation(UIElement.OpacityProperty, pulseAnimation);
        }

        // Create connecting lines between some nodes
        var nodeElements = GridCanvas.Children.OfType<Ellipse>().Take(10).ToArray();
        for (int i = 0; i < nodeElements.Length - 1; i += 2)
        {
            var node1 = nodeElements[i];
            var node2 = nodeElements[i + 1];

            var x1 = Canvas.GetLeft(node1) + node1.Width / 2;
            var y1 = Canvas.GetTop(node1) + node1.Height / 2;
            var x2 = Canvas.GetLeft(node2) + node2.Width / 2;
            var y2 = Canvas.GetTop(node2) + node2.Height / 2;

            var connection = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(Color.FromArgb(40, 0, 255, 65)),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 2, 4 }
            };

            GridCanvas.Children.Insert(0, connection); // Add behind nodes
        }
    }

    private void StartGridAnimation()
    {
        var gridAnimation = new DoubleAnimation
        {
            From = 0,
            To = 40,
            Duration = TimeSpan.FromSeconds(20),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };

        var gridTransform = GridCanvas.RenderTransform as TranslateTransform;
        if (gridTransform != null)
        {
            gridTransform.BeginAnimation(TranslateTransform.XProperty, gridAnimation);

            var yAnimation = new DoubleAnimation
            {
                From = 0,
                To = 40,
                Duration = TimeSpan.FromSeconds(25),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            gridTransform.BeginAnimation(TranslateTransform.YProperty, yAnimation);
        }
    }

    private void StartParticleSystem()
    {
        _particleTimer.Start();
    }

    private void StartPulseAnimations()
    {
        var statusPulse = Resources["StatusTextPulse"] as Storyboard;
        statusPulse?.Begin(StatusText);

        var centerBoxPulse = Resources["CenterBoxPulse"] as Storyboard;
        centerBoxPulse?.Begin(CenterBox);
    }

    private void ParticleTimer_Tick(object? sender, EventArgs e)
    {
        CreateParticle();
        CleanupOldParticles();
    }

    private void CreateParticle()
    {
        var particle = new Ellipse
        {
            Width = Random.Shared.Next(2, 6),
            Height = Random.Shared.Next(2, 6),
            Fill = new SolidColorBrush(Color.FromArgb(100, 0, 255, 65)),
            RenderTransform = new TranslateTransform()
        };

        var x = Random.Shared.Next(0, (int)ActualWidth);
        Canvas.SetLeft(particle, x);
        Canvas.SetTop(particle, 800);

        ParticleCanvas.Children.Add(particle);

        var particleAnimation = Resources["ParticleFloat"] as Storyboard;
        if (particleAnimation != null)
        {
            var clonedStoryboard = particleAnimation.Clone();
            Storyboard.SetTarget(clonedStoryboard, particle);
            clonedStoryboard.Begin();
        }
    }

    private void CleanupOldParticles()
    {
        var particlesToRemove = new List<UIElement>();
        foreach (UIElement child in ParticleCanvas.Children)
        {
            if (child is Ellipse particle)
            {
                var top = Canvas.GetTop(particle);
                if (top < -100)
                {
                    particlesToRemove.Add(particle);
                }
            }
        }

        foreach (var particle in particlesToRemove)
        {
            ParticleCanvas.Children.Remove(particle);
        }
    }

    private void StartGame()
    {
        _currentWord = Words[Random.Shared.Next(Words.Count)].ToUpper().Replace(" ", "");
        _currentLetterIndex = 0;
        _gameActive = false;

        StatusText.Text = "MEMORIZE THE SEQUENCE...";
        CenterText.Text = "";
        HideAllChoices();

        Task.Delay(1000).ContinueWith(_ => Dispatcher.Invoke(StartFlashing));
    }

    private void StartFlashing()
    {
        _flashIndex = 0;
        _flashTimer.Start();
    }

    private void FlashTimer_Tick(object? sender, EventArgs e)
    {
        if (_flashIndex < _currentWord.Length)
        {
            CenterText.Text = _currentWord[_flashIndex].ToString();

            var flashAnimation = Resources["CharacterFlashAnimation"] as Storyboard;
            flashAnimation?.Begin(CenterText);

            _flashIndex++;
        }
        else
        {
            _flashTimer.Stop();
            CenterText.Text = "";
            ShowChoices();
        }
    }

    private void ShowChoices()
    {
        _uniqueLetters = _currentWord.Distinct().ToList();
        _gameActive = true;
        StatusText.Text = $"CLICK THE LETTERS IN ORDER: {_currentWord}";

        ClearChoiceButtons();
        CreateChoiceButtons();
        AnimateChoiceButtons();
    }

    private void ClearChoiceButtons()
    {
        TopChoicesPanel.Children.Clear();
        BottomChoicesPanel.Children.Clear();
        LeftChoicesPanel.Children.Clear();
        RightChoicesPanel.Children.Clear();
        _choiceButtons.Clear();
    }

    private void CreateChoiceButtons()
    {
        var panels = new[] { TopChoicesPanel, RightChoicesPanel, BottomChoicesPanel, LeftChoicesPanel };

        // Distribute letters evenly across all panels, ensuring each panel gets at least one if possible
        var lettersRemaining = _uniqueLetters.Count;
        var panelsToUse = Math.Min(4, lettersRemaining);
        int letterIndex = 0;

        // Calculate distribution to ensure all 4 sides are used when possible
        var distribution = new int[4];
        var baseLetters = lettersRemaining / panelsToUse;
        var extraLetters = lettersRemaining % panelsToUse;

        for (int i = 0; i < panelsToUse; i++)
        {
            distribution[i] = baseLetters + (i < extraLetters ? 1 : 0);
        }

        for (int panelIndex = 0; panelIndex < 4 && letterIndex < _uniqueLetters.Count; panelIndex++)
        {
            var panel = panels[panelIndex];
            var lettersForThisPanel = distribution[panelIndex];

            for (int i = 0; i < lettersForThisPanel && letterIndex < _uniqueLetters.Count; i++)
            {
                var button = new Button
                {
                    Content = _uniqueLetters[letterIndex].ToString(),
                    Tag = _uniqueLetters[letterIndex],
                    Width = 100,
                    Height = 100,
                    Style = Resources["GameButtonStyle"] as Style,
                    Margin = new Thickness(6),
                    Opacity = 1
                };

                button.Click += ChoiceButton_Click;
                _choiceButtons.Add(button);
                panel.Children.Add(button);
                letterIndex++;
            }

            if (panel.Children.Count > 0)
            {
                panel.Visibility = Visibility.Visible;
            }
        }
    }

    private void AnimateChoiceButtons()
    {
        var panels = new[] { TopChoicesPanel, RightChoicesPanel, BottomChoicesPanel, LeftChoicesPanel };
        var initialMargins = new[]
        {
            new Thickness(0, -100, 0, 0), // Top
            new Thickness(100, 0, 0, 0),  // Right
            new Thickness(0, 0, 0, -100), // Bottom
            new Thickness(-100, 0, 0, 0)  // Left
        };
        var targetMargins = new[]
        {
            new Thickness(0, 20, 0, 0),   // Top
            new Thickness(0, 0, 20, 0),   // Right
            new Thickness(0, 0, 0, 20),   // Bottom
            new Thickness(20, 0, 0, 0)    // Left
        };

        for (int i = 0; i < panels.Length; i++)
        {
            var panel = panels[i];
            if (panel.Children.Count > 0)
            {
                panel.Opacity = 0;
                panel.Margin = initialMargins[i];

                var slideAnimation = new ThicknessAnimation
                {
                    From = initialMargins[i],
                    To = targetMargins[i],
                    Duration = TimeSpan.FromSeconds(0.6),
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
                };

                var fadeAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.4)
                };

                var storyboard = new Storyboard();
                storyboard.Children.Add(slideAnimation);
                storyboard.Children.Add(fadeAnimation);

                Storyboard.SetTarget(slideAnimation, panel);
                Storyboard.SetTargetProperty(slideAnimation, new PropertyPath(FrameworkElement.MarginProperty));

                Storyboard.SetTarget(fadeAnimation, panel);
                Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));

                Task.Delay(i * 150).ContinueWith(_ => Dispatcher.Invoke(() => storyboard.Begin()));
            }
        }
    }

    private void HideAllChoices()
    {
        TopChoicesPanel.Visibility = Visibility.Collapsed;
        BottomChoicesPanel.Visibility = Visibility.Collapsed;
        LeftChoicesPanel.Visibility = Visibility.Collapsed;
        RightChoicesPanel.Visibility = Visibility.Collapsed;
        ClearChoiceButtons();
    }

    private void ChoiceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_gameActive || sender is not Button button || button.Tag is not char letter)
            return;

        if (letter == _currentWord[_currentLetterIndex])
        {
            _currentLetterIndex++;
            CenterText.Text = letter.ToString();

            var flashAnimation = Resources["CharacterFlashAnimation"] as Storyboard;
            flashAnimation?.Begin(CenterText);

            if (_currentLetterIndex >= _currentWord.Length)
            {
                StatusText.Text = "SUCCESS! NEXT SEQUENCE...";
                _gameActive = false;
                Task.Delay(2000).ContinueWith(_ => Dispatcher.Invoke(StartGame));
            }
        }
        else
        {
            ShowFailureAnimation();
        }
    }

    private void ShowFailureAnimation()
    {
        _gameActive = false;
        StatusText.Text = "SEQUENCE BREACH! RESTARTING...";

        var failShake = Resources["FailureShakeAnimation"] as Storyboard;
        if (failShake != null)
        {
            var translateAnimation = failShake.Children.OfType<DoubleAnimationUsingKeyFrames>().FirstOrDefault();
            if (translateAnimation != null)
            {
                Storyboard.SetTarget(translateAnimation, MainGameGrid);
            }

            var colorAnimation = failShake.Children.OfType<ColorAnimation>().FirstOrDefault();
            if (colorAnimation != null)
            {
                Storyboard.SetTarget(colorAnimation, MainGameGrid);
            }

            failShake.Begin();
        }

        Task.Delay(2000).ContinueWith(_ => Dispatcher.Invoke(StartGame));
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        BackToMenuRequested?.Invoke(this, EventArgs.Empty);
    }
}