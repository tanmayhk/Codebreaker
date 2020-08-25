using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Codebreaker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int _pegRow = 0;
        int _pegCol = 0;
        int _maxAttempts = 10;
        int _maxColumns;
        int _tries = 0;
        bool isPaused = false;
        bool _isSolutionVisibleOnBoard = false;
        bool _isDuplicates;
        bool _moveOnToNextGuess = false;
        bool _isCircleClicked = false;
        bool _isGameDone = false;
        bool _isSolved = false;
        Ellipse[,] _ellipses;
        Color _selectionButtonGray = Windows.UI.Color.FromArgb((byte)255, (byte)231, (byte)231, (byte)231);
        Color[] _allColors = new Color[6] { Colors.Red, Colors.Orange, Colors.MediumPurple, Colors.SaddleBrown, Colors.Navy, Colors.Green };
        Color[] _allColorsWithBlank;
        
        Random rnd;
        Color[] _currentCode;
        TextBlock[] _textboxes1;
        TextBlock[] _textboxes2;
        Button _selectedButton;

        Button _ConfirmGuessButton = new Button();
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            rnd = new Random();
            Loaded += MainPage_Loaded;
            _maxColumns = _maxAttempts + 1;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _textboxes1 = new TextBlock[_maxAttempts];
            _textboxes2 = new TextBlock[_maxAttempts];
            _ellipses = new Ellipse[4, _maxColumns];
            _allColorsWithBlank = new Color[7] { _selectionButtonGray, Colors.Red, Colors.Orange, Colors.MediumPurple, Colors.SaddleBrown, Colors.Navy, Colors.Green };
        }

        private Color[] CreateCode()
        {
            if (_isDuplicates)
            {
                Color[] code = new Color[4];
                for (int i = 0; i < 4; i++)
                {
                    int index = rnd.Next(0, 6);
                    code[i] = _allColors[index];
                }
                return code;
            }
            else
            {
                Color[] code = new Color[4];
                for (int i = 0; i < 4; i++)
                {
                    bool x = true;
                    while (x)
                    {
                        int index = rnd.Next(0, 6);
                        if (!code.Contains(_allColors[index]))
                        {
                            x = false;
                            code[i] = _allColors[index];
                        }

                    }
                }
                return code;
            }
        }
        private void CreateBoard()
        {
            CircleGrid.ColumnDefinitions.Clear();
            CircleGrid.RowDefinitions.Clear();
            CircleGrid.Children.Clear();
            int[] rows = new int[5] { 3, 3, 3, 3, 4 };
            for (int row = 0; row < 5; row++)
            {
                RowDefinition k = new RowDefinition();
                k.Height = new GridLength(rows[row], GridUnitType.Star);
                CircleGrid.RowDefinitions.Add(k);
            }
            Grid FeedbackGrid = new Grid();
            Grid.SetRow(FeedbackGrid, 4);
            Grid.SetColumnSpan(FeedbackGrid, _maxColumns);
            FeedbackGrid.RowDefinitions.Add(new RowDefinition());
            FeedbackGrid.RowDefinitions.Add(new RowDefinition());
            FeedbackGrid.BorderThickness = new Thickness(0, 5, 0, 0);
            FeedbackGrid.BorderBrush = new SolidColorBrush(Colors.LightSlateGray);
            CircleGrid.Children.Add(FeedbackGrid);
            for (int col = 0; col < _maxColumns; col++)
            {
                CircleGrid.ColumnDefinitions.Add(new ColumnDefinition());
                FeedbackGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int col = 0; col < _maxColumns - 1; col++)
            {
                AddButtonsOnCurrentColumn(col);
            }
            for (int i = 0; i < 4; i++)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Height = 50;
                ellipse.Width = 50;
                Grid.SetRow(ellipse, i);
                Grid.SetColumn(ellipse, _maxColumns - 1);
                _ellipses[i, _maxColumns - 1] = ellipse;
                CircleGrid.Children.Add(ellipse);
            }
            for (int col = 0; col < _maxColumns - 1; col++)
            {
                TextBlock textbox = new TextBlock();
                Grid.SetRow(textbox, 0);
                Grid.SetColumn(textbox, col);
                textbox.HorizontalAlignment = HorizontalAlignment.Center;
                textbox.VerticalAlignment = VerticalAlignment.Center;
                textbox.FontFamily = new FontFamily("Segoe MDL2 Assets");
                textbox.FontSize = 20;
                textbox.Foreground = new SolidColorBrush(Colors.Black);
                FeedbackGrid.Children.Add(textbox);
                _textboxes1[col] = textbox;
            }
            for (int col = 0; col < _maxColumns - 1; col++)
            {
                TextBlock textbox = new TextBlock();
                Grid.SetRow(textbox, 1);
                Grid.SetColumn(textbox, col);
                textbox.HorizontalAlignment = HorizontalAlignment.Center;
                textbox.VerticalAlignment = VerticalAlignment.Center;
                textbox.FontFamily = new FontFamily("Segoe MDL2 Assets");
                textbox.FontSize = 20;
                textbox.Foreground = new SolidColorBrush(Colors.Black);
                FeedbackGrid.Children.Add(textbox);
                _textboxes2[col] = textbox;
            }

            Rectangle rectLeft = new Rectangle();
            rectLeft.Width = 4;
            Grid.SetRow(rectLeft, 0);
            Grid.SetColumn(rectLeft, _maxColumns - 1);
            Grid.SetRowSpan(rectLeft, 4);
            rectLeft.Fill = new SolidColorBrush(Colors.LightSlateGray);
            rectLeft.HorizontalAlignment = HorizontalAlignment.Left;
            CircleGrid.Children.Add(rectLeft);

            Rectangle rectRight = new Rectangle();
            rectRight.Width = 4;
            Grid.SetRow(rectRight, 0);
            Grid.SetColumn(rectRight, _maxColumns - 2);
            Grid.SetRowSpan(rectRight, 4);
            rectRight.Fill = new SolidColorBrush(Colors.LightSlateGray);
            rectRight.HorizontalAlignment = HorizontalAlignment.Right;
            CircleGrid.Children.Add(rectRight);

            _ConfirmGuessButton.Click += ConfirmGuessButton_Click;
            _ConfirmGuessButton.Width = 130;
            _ConfirmGuessButton.Height = 130;
            _ConfirmGuessButton.HorizontalAlignment = HorizontalAlignment.Center;
            _ConfirmGuessButton.FontFamily = new FontFamily("Segoe MDL2 Assets");
            _ConfirmGuessButton.FontSize = 40;
            _ConfirmGuessButton.Content = "\uE73E";
            _ConfirmGuessButton.Background = new SolidColorBrush(Colors.LightSlateGray);
            _ConfirmGuessButton.Foreground = new SolidColorBrush(Colors.White);
            Grid.SetRow(_ConfirmGuessButton, 0);
            Grid.SetRowSpan(_ConfirmGuessButton, 2);
            Grid.SetColumn(_ConfirmGuessButton, 0);
            FeedbackGrid.Children.Add(_ConfirmGuessButton);

            InitializeLayout();
        }

        private void InitializeLayout()
        {
            foreach (var child in CircleGrid.Children)
            {
                Ellipse e = child as Ellipse;
                if (e == null)
                {
                    TextBlock t = child as TextBlock;
                    if (t != null)
                    {
                        t.Text = "";
                    }
                }
                else
                {
                    e.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)65, (byte)64, (byte)66));
                    e.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)255, (byte)255, (byte)255));
                    e.StrokeThickness = 2;
                    int row = Grid.GetRow(e);
                    int column = Grid.GetColumn(e);
                }
            }

            GameGrid.UpdateLayout();
            FeedbackGrid.UpdateLayout();
            CircleGrid.UpdateLayout();
        }

        private void OnCircleClick(object sender, RoutedEventArgs e)
        {
            if (!_isCircleClicked)
            {
                Button button = sender as Button;
                button.BorderThickness = new Thickness(3);
                button.BorderBrush = new SolidColorBrush(Colors.Black);
                foreach (var child in PaletteGrid.Children)
                {
                    Button b = child as Button;
                    b.IsEnabled = true;
                }
                if (_selectedButton == new Button())
                {
                    _selectedButton = button;
                    _isCircleClicked = !_isCircleClicked;
                }
                else
                {
                    _selectedButton.BorderThickness = new Thickness(0);
                    _selectedButton.BorderBrush = new SolidColorBrush(Colors.Black);
                    button.BorderThickness = new Thickness(3);
                    button.BorderBrush = new SolidColorBrush(Colors.Black);
                    _selectedButton = button;
                }
            }
            else
            {
                Button button = sender as Button;
                button.BorderThickness = new Thickness(0);
                button.BorderBrush = new SolidColorBrush(Colors.Black);
                foreach (var child in PaletteGrid.Children)
                {
                    Button b = child as Button;
                    b.IsEnabled = false;
                }
                _selectedButton = new Button();
                _isCircleClicked = !_isCircleClicked;
            }
        }
        private void OnColorClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            SolidColorBrush backgroundBrush = (SolidColorBrush)button.Background;

            int selectedRow = Grid.GetRow(_selectedButton);
            int selectedColumn = Grid.GetColumn(_selectedButton);

            Ellipse selectedEllipse = _ellipses[selectedRow, selectedColumn];
            selectedEllipse.Fill = backgroundBrush;

            foreach (var child in PaletteGrid.Children)
            {
                Button b = child as Button;
                b.IsEnabled = false;
            }
            
            _selectedButton.BorderThickness = new Thickness(0);
            _selectedButton = new Button();
        }
        private void Feedback(int col, Color[] _currentCode)
        {
            Ellipse ellipse1 = _ellipses[0, col];
            Color color1 = ((SolidColorBrush)ellipse1.Fill).Color;
            Ellipse ellipse2 = _ellipses[1, col];
            Color color2 = ((SolidColorBrush)ellipse2.Fill).Color;
            Ellipse ellipse3 = _ellipses[2, col];
            Color color3 = ((SolidColorBrush)ellipse3.Fill).Color;
            Ellipse ellipse4 = _ellipses[3, col];
            Color color4 = ((SolidColorBrush)ellipse4.Fill).Color;
            
            if (color1 != _selectionButtonGray && color2 != _selectionButtonGray && color3 != _selectionButtonGray && color4 != _selectionButtonGray)
            {
                _tries += 1;
                _moveOnToNextGuess = true;
                Color[] attempt = new Color[4] { color1, color2, color3, color4 };
                Color[] duplicateAttempt = new Color[4];
                attempt.CopyTo(duplicateAttempt, 0);

                Color[] duplicateCurrentCode = new Color[4];
                _currentCode.CopyTo(duplicateCurrentCode, 0);

                int rightColorAndPlace = 0;
                int rightColorButWrongPlace = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (duplicateAttempt[i] == duplicateCurrentCode[i])
                    {
                        rightColorAndPlace += 1;
                        duplicateCurrentCode[i] = Colors.White;
                        duplicateAttempt[i] = Colors.White;
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    if (duplicateAttempt[i] != Colors.White)
                    {
                        if (duplicateCurrentCode.Contains(duplicateAttempt[i]))
                        {
                            int codeIndex = Array.IndexOf(duplicateCurrentCode, duplicateAttempt[i]);
                            duplicateAttempt[i] = Colors.White;
                            duplicateCurrentCode[codeIndex] = Colors.White;
                            rightColorButWrongPlace += 1;
                        }
                    }
                }
                TextBlock textbox1 = _textboxes1[_tries - 1];
                TextBlock textbox2 = _textboxes2[_tries - 1];
                textbox1.VerticalAlignment = VerticalAlignment.Center;
                textbox2.VerticalAlignment = VerticalAlignment.Center;

                string rightColorWrongPlaceString = "";
                string rightColorPlaceString = "";
                for (int i = 0; i < rightColorButWrongPlace; i++)
                {
                    rightColorWrongPlaceString += "\uE734 ";
                }
                for (int i = 0; i < rightColorAndPlace; i++)
                {
                    rightColorPlaceString += "\uE735 ";
                }

                textbox1.Text = rightColorPlaceString;
                textbox2.Text = rightColorWrongPlaceString;

                if (rightColorAndPlace == 4)
                {
                    _isSolved = true;
                    EndScreen(true);

                    Ellipse e1 = _ellipses[0, _maxAttempts];
                    SolidColorBrush CodePeg1 = new SolidColorBrush(_currentCode[0]);
                    e1.Fill = CodePeg1;

                    Ellipse e2 = _ellipses[1, _maxAttempts];
                    SolidColorBrush CodePeg2 = new SolidColorBrush(_currentCode[1]);
                    e2.Fill = CodePeg2;

                    Ellipse e3 = _ellipses[2, _maxAttempts];
                    SolidColorBrush CodePeg3 = new SolidColorBrush(_currentCode[2]);
                    e3.Fill = CodePeg3;

                    Ellipse e4 = _ellipses[3, _maxAttempts];
                    SolidColorBrush CodePeg4 = new SolidColorBrush(_currentCode[3]);
                    e4.Fill = CodePeg4;

                    _isSolutionVisibleOnBoard = true;
                }
                else if (_tries == _maxAttempts)
                {
                    _isSolved = true;
                    Ellipse e1 = _ellipses[0, _maxAttempts];
                    SolidColorBrush CodePeg1 = new SolidColorBrush(_currentCode[0]);
                    e1.Fill = CodePeg1;

                    Ellipse e2 = _ellipses[1, _maxAttempts];
                    SolidColorBrush CodePeg2 = new SolidColorBrush(_currentCode[1]);
                    e2.Fill = CodePeg2;

                    Ellipse e3 = _ellipses[2, _maxAttempts];
                    SolidColorBrush CodePeg3 = new SolidColorBrush(_currentCode[2]);
                    e3.Fill = CodePeg3;

                    Ellipse e4 = _ellipses[3, _maxAttempts];
                    SolidColorBrush CodePeg4 = new SolidColorBrush(_currentCode[3]);
                    e4.Fill = CodePeg4;
                    EndScreen(false);
                }
            }
        }

        private void AddButtonsOnCurrentColumn(int col)
        {
            for (int i = 0; i < 4; i++)
            {
                Button b = new Button();
                Grid.SetRow(b, i);
                Grid.SetColumn(b, col);
                b.Width = 130;
                b.Height = 100;
                b.HorizontalAlignment = HorizontalAlignment.Center;
                b.Background = new SolidColorBrush(_selectionButtonGray);
                b.Click += OnCircleClick;

                if (col == 0)
                {
                    b.Visibility = Visibility.Visible;
                }
                else
                {
                    b.Visibility = Visibility.Collapsed;
                }
                CircleGrid.Children.Add(b);

                Ellipse ellipse = new Ellipse();
                ellipse.Height = 50;
                ellipse.Width = 50;

                ellipse.StrokeThickness = 2;
                ellipse.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)65, (byte)64, (byte)66));
                ellipse.Fill = new SolidColorBrush(_selectionButtonGray);

                if (col == 0)
                {
                    ellipse.Visibility = Visibility.Visible;
                }
                else
                {
                    ellipse.Visibility = Visibility.Collapsed;
                }
                ellipse.HorizontalAlignment = HorizontalAlignment.Center;
                _ellipses[i, col] = ellipse;

                if (col < _maxColumns - 1)
                {
                    b.Content = ellipse;
                }
            }
        }

        private void MoveToNextColumnOfButtons(int col)
        {
            foreach (var child in CircleGrid.Children)
            {
                Button b = child as Button;
                if (b != null)
                {
                    int c = Grid.GetColumn(b);
                    if (c == col)
                    {
                        b.Visibility = Visibility.Visible;
                    }
                    if (c == col - 1)
                    {
                        b.Click -= OnCircleClick;
                        GazeInput.SetInteraction(b, Interaction.Disabled);
                    }
                }
            }
            int confirmGuessButtonColumn = Grid.GetColumn(_ConfirmGuessButton);
            Grid.SetColumn(_ConfirmGuessButton, confirmGuessButtonColumn + 1);
        }
        private void EndScreen(bool isVictory)
        {
            CongratsGrid.Visibility = Visibility.Visible;
            if (isVictory)
            {
                if (_tries == 1)
                {
                    CongratsTextBlock.Text = "Good job! You got it in " + _tries.ToString() + " try!";
                }
                else
                {
                    CongratsTextBlock.Text = "Good job! You got it in " + _tries.ToString() + " tries!";
                }
            }
            else
            {
                CongratsTextBlock.Text = "Sorry - you didn't crack the code. Please try again!";
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                PauseButton.Content = "\uE768";
                BacktoStartButton.IsEnabled = false;
                ShowSolutionButton.IsEnabled = false;
                _ConfirmGuessButton.IsEnabled = false;

                GazeInput.SetInteraction(PaletteGrid, Interaction.Disabled);
                GazeInput.SetInteraction(CircleGrid, Interaction.Disabled);
                foreach (var child in CircleGrid.Children)
                {
                    Button b = child as Button;
                    if (b != null)
                    {
                        b.IsEnabled = false;
                    }
                }
                foreach (var child in PaletteGrid.Children)
                {
                    Button b = child as Button;
                    b.IsEnabled = false;
                }
            }
            else
            {
                PauseButton.Content = "\uE769";
                BacktoStartButton.IsEnabled = true;
                ShowSolutionButton.IsEnabled = true;
                _ConfirmGuessButton.IsEnabled = true;

                GazeInput.SetInteraction(PaletteGrid, Interaction.Enabled);
                GazeInput.SetInteraction(CircleGrid, Interaction.Enabled);
                foreach (var child in CircleGrid.Children)
                {
                    Button b = child as Button;
                    if (b != null)
                    {
                        b.IsEnabled = true;
                    }
                }
                foreach (var child in PaletteGrid.Children)
                {
                    Button b = child as Button;
                    b.IsEnabled = true;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
        
        private void ShowSolutionButton_Click(object sender, RoutedEventArgs e)
        {
            Ellipse e1 = _ellipses[0, _maxAttempts];
            SolidColorBrush CodePeg1 = new SolidColorBrush(_currentCode[0]);
            e1.Fill = CodePeg1;

            Ellipse e2 = _ellipses[1, _maxAttempts];
            SolidColorBrush CodePeg2 = new SolidColorBrush(_currentCode[1]);
            e2.Fill = CodePeg2;

            Ellipse e3 = _ellipses[2, _maxAttempts];
            SolidColorBrush CodePeg3 = new SolidColorBrush(_currentCode[2]);
            e3.Fill = CodePeg3;

            Ellipse e4 = _ellipses[3, _maxAttempts];
            SolidColorBrush CodePeg4 = new SolidColorBrush(_currentCode[3]);
            e4.Fill = CodePeg4;

            GazeInput.SetInteraction(CircleGrid, Interaction.Disabled);

            foreach (var child in CircleGrid.Children)
            {
                Button b = child as Button;
                if (b != null)
                {
                    b.IsEnabled = false;
                    GazeInput.SetInteraction(b, Interaction.Disabled);
                }
            }
            _isSolutionVisibleOnBoard = true;
            EndScreen(false);
        }

        private void StartGame()
        {
            StartGrid.Visibility = Visibility.Collapsed;
            GameGrid.Visibility = Visibility.Visible;
            ResetVariables();
            CreateBoard();
            _currentCode = CreateCode();
        }

        private void BacktoStartButton_Click(object sender, RoutedEventArgs e)
        {
            CongratsGrid.Visibility = Visibility.Collapsed;
            GameGrid.Visibility = Visibility.Collapsed;
            StartGrid.Visibility = Visibility.Visible;
        }
        
        private void ResetVariables()
        {
            _pegRow = 0;
            _pegCol = 0;
            isPaused = false; 
            _isSolutionVisibleOnBoard = false;
            _ellipses = new Ellipse[4, _maxColumns];
            _textboxes1 = new TextBlock[_maxAttempts];
            _textboxes2 = new TextBlock[_maxAttempts];
            _allColors = new Color[6] { Colors.Red, Colors.Orange, Colors.MediumPurple, Colors.SaddleBrown, Colors.Navy, Colors.Green };
            _allColorsWithBlank = new Color[7] { _selectionButtonGray, Colors.Red, Colors.Orange, Colors.MediumPurple, Colors.SaddleBrown, Colors.Navy, Colors.Green };
            _tries = 0;
            CongratsTextBlock.Text = "";
            _selectedButton = new Button();
            _isCircleClicked = false;
            _ConfirmGuessButton = new Button();
            foreach (var child in PaletteGrid.Children)
            {
                Button b = child as Button;
                b.IsEnabled = false;
            }
            _isSolved = false;
            InitializeLayout();
        }

        private void ConfirmGuessButton_Click(object sender, RoutedEventArgs e)
        {
            if (_tries < _maxAttempts)
            {
                Feedback(_tries, _currentCode);
                if (_moveOnToNextGuess)
                {
                    _pegCol += 1;
                    for (int i = 0; i < 4; i++)
                    {
                        Ellipse circle = _ellipses[i, _pegCol];
                        circle.Visibility = Visibility.Visible;
                    }
                    _moveOnToNextGuess = false;
                    if (! _isSolved)
                    {
                        MoveToNextColumnOfButtons(_pegCol);
                    }
                    else
                    {
                        _ConfirmGuessButton.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void EasyGameStart_Click(object sender, RoutedEventArgs e)
        {
            _isDuplicates = false;
            StartGame();
        }

        private void HardGameStart_Click(object sender, RoutedEventArgs e)
        {
            _isDuplicates = true;
            StartGame();
        }

        private void HowToPlayButton_Click(object sender, RoutedEventArgs e)
        {
            HelpGrid.Visibility = Visibility.Visible;
        }
        private void CloseHowToPlayButton_Click(object sender, RoutedEventArgs e)
        {
            HelpGrid.Visibility = Visibility.Collapsed;
        }

        private void CloseCongratsButton_Click(object sender, RoutedEventArgs e)
        {
            CongratsGrid.Visibility = Visibility.Collapsed;
        }
    }
}