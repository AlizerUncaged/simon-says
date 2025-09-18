using System.Windows;

namespace SimonSays;

public partial class MainWindow : Window
{
    private MainMenu? _currentMainMenu;
    private GameWindow? _currentGameWindow;

    public MainWindow()
    {
        InitializeComponent();
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        // Dispose current game if exists
        if (_currentGameWindow != null)
        {
            _currentGameWindow.BackToMenuRequested -= OnBackToMenuRequested;
            MainContainer.Children.Remove(_currentGameWindow);
            _currentGameWindow = null;
        }

        // Create new main menu
        _currentMainMenu = new MainMenu();
        _currentMainMenu.StartGameRequested += OnStartGameRequested;
        MainContainer.Children.Add(_currentMainMenu);
    }

    private void ShowGameWindow()
    {
        // Dispose current main menu
        if (_currentMainMenu != null)
        {
            _currentMainMenu.StartGameRequested -= OnStartGameRequested;
            MainContainer.Children.Remove(_currentMainMenu);
            _currentMainMenu = null;
        }

        // Create new game window
        _currentGameWindow = new GameWindow();
        _currentGameWindow.BackToMenuRequested += OnBackToMenuRequested;
        MainContainer.Children.Add(_currentGameWindow);
    }

    private void OnStartGameRequested(object? sender, EventArgs e)
    {
        ShowGameWindow();
    }

    private void OnBackToMenuRequested(object? sender, EventArgs e)
    {
        ShowMainMenu();
    }
}