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

    private DispatcherTimer _animationTimer = new();
    private List<MatrixColumn> _columns = new();
    private Random _random = new();

    private class MatrixColumn
    {
        public double X;
        public double Speed;
        public int Length;
        public double Position;
        public string[] Characters = Array.Empty<string>();
        public bool IsActive;
    }

    public MainMenu()
    {
        InitializeComponent();
        _animationTimer.Interval = TimeSpan.FromMilliseconds(100); // Much slower update rate
        _animationTimer.Tick += AnimationTimer_Tick;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitializeSimpleMatrix();
        StartAnimations();
    }

    private void MainMenuGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.PreviousSize.Width != 0 && e.PreviousSize.Height != 0)
        {
            ReinitializeMatrix();
        }
    }

    private void InitializeSimpleMatrix()
    {
        MatrixColumns.Items.Clear();
        _columns.Clear();

        if (ActualWidth <= 0 || ActualHeight <= 0) return;

        // Create only a few columns for better performance
        var columnWidth = 50; // Much wider spacing
        var columnCount = Math.Max(5, (int)(ActualWidth / columnWidth));

        for (int i = 0; i < columnCount; i++)
        {
            var column = new MatrixColumn
            {
                X = i * columnWidth,
                Speed = _random.Next(1, 4), // Much slower
                Length = _random.Next(8, 15),
                Position = _random.Next(0, (int)ActualHeight),
                IsActive = _random.NextDouble() > 0.6
            };

            // Pre-generate characters for this column
            column.Characters = new string[column.Length];
            for (int j = 0; j < column.Length; j++)
            {
                column.Characters[j] = GetRandomChar();
            }

            CreateColumnVisual(column);
            _columns.Add(column);
        }
    }

    private void CreateColumnVisual(MatrixColumn column)
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Background = Brushes.Transparent
        };

        for (int i = 0; i < column.Length; i++)
        {
            var textBlock = new TextBlock
            {
                Text = column.Characters[i],
                Foreground = GetMatrixBrush(i),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                TextAlignment = TextAlignment.Center,
                Width = 20,
                Margin = new Thickness(0, 2, 0, 0)
            };
            stackPanel.Children.Add(textBlock);
        }

        Canvas.SetLeft(stackPanel, column.X);
        Canvas.SetTop(stackPanel, column.Position);
        stackPanel.Opacity = column.IsActive ? 0.8 : 0.2;

        MatrixColumns.Items.Add(stackPanel);
    }

    private string GetRandomChar()
    {
        var chars = new[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
                           "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                           "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        return chars[_random.Next(chars.Length)];
    }

    private Brush GetMatrixBrush(int index)
    {
        if (index == 0)
            return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)); // White head
        else if (index < 3)
            return new SolidColorBrush(Color.FromArgb(200, 0, 255, 65)); // Bright green
        else
            return new SolidColorBrush(Color.FromArgb((byte)(150 - index * 10), 0, (byte)(200 - index * 15), 45)); // Fade
    }

    private void ReinitializeMatrix()
    {
        _animationTimer.Stop();
        InitializeSimpleMatrix();
        _animationTimer.Start();
    }

    private void StartAnimations()
    {
        _animationTimer.Start();

        // Simple scanning line animation
        var scanAnimation = new DoubleAnimation
        {
            From = -10,
            To = ActualWidth + 10,
            Duration = TimeSpan.FromSeconds(4),
            RepeatBehavior = RepeatBehavior.Forever
        };
        ScanTransform.BeginAnimation(TranslateTransform.XProperty, scanAnimation);

        // Simple title pulse
        var titlePulse = new DoubleAnimation
        {
            From = 0.8,
            To = 1.0,
            Duration = TimeSpan.FromSeconds(2),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };
        TitleText.BeginAnimation(OpacityProperty, titlePulse);
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        // Very simple animation - just move columns down slowly
        for (int i = 0; i < _columns.Count && i < MatrixColumns.Items.Count; i++)
        {
            var column = _columns[i];
            if (MatrixColumns.Items[i] is StackPanel stackPanel)
            {
                var currentTop = Canvas.GetTop(stackPanel);
                var newTop = currentTop + column.Speed;

                if (newTop > ActualHeight)
                {
                    newTop = -stackPanel.ActualHeight;
                    // Randomly change some characters
                    if (_random.NextDouble() > 0.7)
                    {
                        for (int j = 0; j < stackPanel.Children.Count; j++)
                        {
                            if (stackPanel.Children[j] is TextBlock tb && _random.NextDouble() > 0.8)
                            {
                                tb.Text = GetRandomChar();
                            }
                        }
                    }
                }

                Canvas.SetTop(stackPanel, newTop);
            }
        }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        _animationTimer.Stop();
        StartGameRequested?.Invoke(this, EventArgs.Empty);
    }
}