using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SimonSays;

public partial class MainMenu : UserControl
{
    public event EventHandler? StartGameRequested;

    private DispatcherTimer _matrixTimer = new();
    private DispatcherTimer _scanTimer = new();
    private List<MatrixColumn> _matrixColumns = new();
    private Random _random = new();

    private class MatrixColumn
    {
        public List<TextBlock> Characters = new();
        public double X;
        public double Speed;
        public int Length;
        public int CurrentIndex;
        public bool IsActive;
    }

    public MainMenu()
    {
        InitializeComponent();
        _matrixTimer.Interval = TimeSpan.FromMilliseconds(50);
        _matrixTimer.Tick += MatrixTimer_Tick;

        _scanTimer.Interval = TimeSpan.FromMilliseconds(16);
        _scanTimer.Tick += ScanTimer_Tick;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeMatrixRain();
        CreateHexagonalPattern();
        StartAnimations();
    }

    private void MainMenuGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.PreviousSize.Width != 0 && e.PreviousSize.Height != 0)
        {
            ReinitializeMatrixRain();
        }
    }

    private void InitializeMatrixRain()
    {
        MatrixCanvas.Children.Clear();
        _matrixColumns.Clear();

        if (ActualWidth <= 0 || ActualHeight <= 0) return;

        var columnWidth = 12; // Much tighter spacing
        var columnCount = (int)(ActualWidth / columnWidth) + 5;

        for (int i = 0; i < columnCount; i++)
        {
            var column = new MatrixColumn
            {
                X = i * columnWidth,
                Speed = _random.Next(30, 150),
                Length = _random.Next(15, 35),
                IsActive = _random.NextDouble() > 0.4 // More active columns
            };

            for (int j = 0; j < column.Length; j++)
            {
                var textBlock = new TextBlock
                {
                    Text = GetRandomMatrixCharacter(),
                    Foreground = GetMatrixBrush(j, column.Length),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = _random.Next(9, 13),
                    Opacity = 0
                };

                Canvas.SetLeft(textBlock, column.X);
                Canvas.SetTop(textBlock, -100 - (j * 16));

                column.Characters.Add(textBlock);
                MatrixCanvas.Children.Add(textBlock);
            }

            _matrixColumns.Add(column);
        }
    }

    private string GetRandomMatrixCharacter()
    {
        var characters = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                                "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                                "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
                                "ア", "イ", "ウ", "エ", "オ", "カ", "キ", "ク", "ケ", "コ",
                                "サ", "シ", "ス", "セ", "ソ", "タ", "チ", "ツ", "テ", "ト",
                                "ナ", "ニ", "ヌ", "ネ", "ノ", "ハ", "ヒ", "フ", "ヘ", "ホ"};
        return characters[_random.Next(characters.Length)];
    }

    private void ReinitializeMatrixRain()
    {
        _matrixTimer.Stop();
        InitializeMatrixRain();
        CreateHexagonalPattern();
        _matrixTimer.Start();
    }

    private void CreateHexagonalPattern()
    {
        HexPatternCanvas.Children.Clear();

        if (ActualWidth <= 0 || ActualHeight <= 0) return;

        var hexSize = 30;
        var hexSpacing = hexSize * 1.8;

        for (double y = -hexSize; y < ActualHeight + hexSize; y += hexSpacing * 0.75)
        {
            for (double x = -hexSize; x < ActualWidth + hexSize; x += hexSpacing)
            {
                var offsetX = ((int)(y / (hexSpacing * 0.75)) % 2) * (hexSpacing * 0.5);

                var hexagon = CreateHexagon(hexSize);
                Canvas.SetLeft(hexagon, x + offsetX);
                Canvas.SetTop(hexagon, y);
                HexPatternCanvas.Children.Add(hexagon);
            }
        }
    }

    private Polygon CreateHexagon(double size)
    {
        var hexagon = new Polygon();
        var points = new PointCollection();

        for (int i = 0; i < 6; i++)
        {
            var angle = i * Math.PI / 3;
            var x = size * Math.Cos(angle);
            var y = size * Math.Sin(angle);
            points.Add(new Point(x, y));
        }

        hexagon.Points = points;
        hexagon.Stroke = new SolidColorBrush(Color.FromArgb(30, 0, 255, 65));
        hexagon.StrokeThickness = 1;
        hexagon.Fill = new SolidColorBrush(Color.FromArgb(5, 0, 255, 65));

        return hexagon;
    }

    private Brush GetMatrixBrush(int index, int totalLength)
    {
        byte opacity;

        if (index == 0) // Head of the column - black flash
        {
            opacity = 255;
            return new SolidColorBrush(Color.FromArgb(opacity, 0, 0, 0));
        }
        else if (index == 1) // Second character - brightest green
        {
            opacity = 255;
            return new SolidColorBrush(Color.FromArgb(opacity, 0, 255, 65));
        }
        else if (index < 4) // Near head - bright green
        {
            opacity = (byte)(220 - (index * 25));
            return new SolidColorBrush(Color.FromArgb(opacity, 0, 255, 65));
        }
        else if (index < 8) // Mid section - medium green
        {
            opacity = (byte)(180 - (index * 15));
            return new SolidColorBrush(Color.FromArgb(opacity, 0, (byte)(220 - index * 10), 50));
        }
        else // Tail - fade to darker green
        {
            opacity = (byte)Math.Max(20, 150 - (index * 12));
            return new SolidColorBrush(Color.FromArgb(opacity, 0, (byte)(180 - index * 8), 40));
        }
    }

    private void StartAnimations()
    {
        _matrixTimer.Start();
        _scanTimer.Start();

        // Title pulse animation
        var titlePulse = new DoubleAnimation
        {
            From = 0.7,
            To = 1.0,
            Duration = TimeSpan.FromSeconds(2),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };
        TitleText.BeginAnimation(OpacityProperty, titlePulse);

        // Button glow animation
        var buttonGlow = new DoubleAnimation
        {
            From = 20,
            To = 35,
            Duration = TimeSpan.FromSeconds(1.5),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };
        if (StartButton.Effect is DropShadowEffect buttonEffect)
        {
            buttonEffect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, buttonGlow);
        }
    }

    private void MatrixTimer_Tick(object? sender, EventArgs e)
    {
        foreach (var column in _matrixColumns)
        {
            if (!column.IsActive && _random.NextDouble() > 0.98)
            {
                column.IsActive = true;
                column.CurrentIndex = 0;

                // Randomly change some characters
                for (int i = 0; i < column.Characters.Count; i++)
                {
                    if (_random.NextDouble() > 0.5)
                    {
                        column.Characters[i].Text = GetRandomMatrixCharacter();
                    }
                }
            }

            if (column.IsActive)
            {
                // Move existing characters down
                for (int i = 0; i < column.Characters.Count; i++)
                {
                    var textBlock = column.Characters[i];
                    var currentTop = Canvas.GetTop(textBlock);
                    var newTop = currentTop + (column.Speed * 0.05);

                    Canvas.SetTop(textBlock, newTop);

                    // Fade in/out logic
                    if (newTop > -50 && newTop < ActualHeight + 50)
                    {
                        var distanceFromHead = Math.Abs(i - column.CurrentIndex);
                        if (distanceFromHead < 3)
                        {
                            textBlock.Opacity = Math.Max(0.3, 1.0 - (distanceFromHead * 0.3));
                        }
                        else
                        {
                            textBlock.Opacity = Math.Max(0, textBlock.Opacity - 0.02);
                        }
                    }
                    else
                    {
                        textBlock.Opacity = 0;
                    }

                    // Reset when off screen
                    if (newTop > ActualHeight + 100)
                    {
                        Canvas.SetTop(textBlock, -100 - (i * 20));
                        textBlock.Opacity = 0;
                    }
                }

                column.CurrentIndex++;
                if (column.CurrentIndex > column.Length + 10)
                {
                    column.IsActive = false;
                    column.CurrentIndex = 0;
                }
            }
        }
    }

    private double _scanPosition1 = 0;
    private double _scanPosition2 = 0;

    private void ScanTimer_Tick(object? sender, EventArgs e)
    {
        if (ActualWidth <= 0) return;

        // First scan line
        _scanPosition1 += 2;
        if (_scanPosition1 > ActualWidth + 10)
        {
            _scanPosition1 = -10;
        }
        ScanTransform1.X = _scanPosition1;

        // Second scan line (slower)
        _scanPosition2 += 1.2;
        if (_scanPosition2 > ActualWidth + 5)
        {
            _scanPosition2 = -5;
        }
        ScanTransform2.X = _scanPosition2;
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        _matrixTimer.Stop();
        _scanTimer.Stop();
        StartGameRequested?.Invoke(this, EventArgs.Empty);
    }
}