using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
        int _maxAttempts = 15;
        int _maxColumns;
        bool isPaused = false;
        bool _isSolutionVisible = false;
        bool _isDuplicates = true;
        Ellipse[,] _ellipses;
        Color[] _allColors = new Color[6] { Colors.Red, Colors.Orange, Colors.Gray, Colors.Violet, Colors.Blue, Colors.Green };
        int tries = 0;
        Random rnd;
        Color[] _currentCode;
        TextBlock[] _textboxes1;
        TextBlock[] _textboxes2;
        public MainPage()
        {
            this.InitializeComponent();
            rnd = new Random();
            Loaded += MainPage_Loaded;
            _maxColumns = _maxAttempts + 1;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _textboxes1 = new TextBlock[_maxAttempts];
            _textboxes2 = new TextBlock[_maxAttempts];
            _ellipses = new Ellipse[4, _maxColumns];
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
            for (int row = 0; row < 6; row++)
            {
                CircleGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int col = 0; col < _maxColumns; col++)
            {
                CircleGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < _maxColumns; col++)
                {
                    Ellipse ellipse = new Ellipse();
                    Grid.SetRow(ellipse, row);
                    Grid.SetColumn(ellipse, col);
                    ellipse.Height = 50;
                    ellipse.Width = 50;
                    ellipse.HorizontalAlignment = HorizontalAlignment.Center;
                    _ellipses[row, col] = ellipse;
                    CircleGrid.Children.Add(ellipse);
                }
            }
            for (int col = 0; col < _maxColumns - 1; col++)
            {
                TextBlock textbox = new TextBlock();
                Grid.SetRow(textbox, 4);
                Grid.SetColumn(textbox, col);
                textbox.HorizontalAlignment = HorizontalAlignment.Center;
                textbox.FontSize = 20;
                CircleGrid.Children.Add(textbox);
                _textboxes1[col] = textbox;
            }
            for (int col = 0; col < _maxAttempts - 1; col++)
            {
                TextBlock textbox = new TextBlock();
                Grid.SetRow(textbox, 5);
                Grid.SetColumn(textbox, col);
                textbox.HorizontalAlignment = HorizontalAlignment.Center;
                textbox.FontSize = 20;
                CircleGrid.Children.Add(textbox);
                _textboxes2[col] = textbox;
            }
            InitializeLayout();
            Header1.TextWrapping = TextWrapping.Wrap;
            Header1.Text = "Correct Positions";
            Header2.TextWrapping = TextWrapping.Wrap;
            Header2.Text = "Wrong Positions";
            GameGrid.UpdateLayout();
            CircleGrid.UpdateLayout();
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
                    e.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)0, (byte)0, (byte)0));
                    e.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)255, (byte)255, (byte)255));
                    e.StrokeThickness = 5;
                    int row = Grid.GetRow(e);
                    int column = Grid.GetColumn(e);
                    if (row == 0 && column == 0)
                    { 
                        e.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)128, (byte)0, (byte)0));
                    }
                }
            }
        }
        private void OnColorClick(object sender, RoutedEventArgs e)
        {
            if (!_isSolutionVisible)
            {
                var button = sender as Button;
                Ellipse ellipse = _ellipses[_pegRow, _pegCol];
                ellipse.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)128, (byte)0, (byte)0));
                ellipse.Fill = button.Background;
                _pegRow += 1;
                if (_pegRow == 4)
                {
                    _pegRow = 0;
                    _pegCol += 1;
                    Ellipse ellipse_new = _ellipses[_pegRow, _pegCol];
                    ellipse.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)0, (byte)0, (byte)0));
                    Feedback(_pegCol - 1, _currentCode);
                    ellipse_new.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)128, (byte)0, (byte)0));
                }
                else
                {
                    Ellipse ellipse_new = _ellipses[_pegRow, _pegCol];
                    ellipse.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)0, (byte)0, (byte)0));
                    ellipse_new.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, (byte)128, (byte)0, (byte)0));
                }
            }
        }
        private void Feedback(int col, Color[] _currentCode)
        {
            tries += 1;
            Ellipse ellipse1 = _ellipses[0, col];
            Color color1 = ((SolidColorBrush)ellipse1.Fill).Color;
            Ellipse ellipse2 = _ellipses[1, col];
            Color color2 = ((SolidColorBrush)ellipse2.Fill).Color;
            Ellipse ellipse3 = _ellipses[2, col];
            Color color3 = ((SolidColorBrush)ellipse3.Fill).Color;
            Ellipse ellipse4 = _ellipses[3, col];
            Color color4 = ((SolidColorBrush)ellipse4.Fill).Color;

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
            TextBlock textbox1 = _textboxes1[tries - 1];
            TextBlock textbox2 = _textboxes2[tries - 1];
            textbox1.Text = rightColorAndPlace.ToString();
            textbox2.Text = rightColorButWrongPlace.ToString();
            if (rightColorAndPlace == 4)
            {
                EndScreen(true);
            }
            else if (tries == _maxAttempts)
            {
                StopGame();
                SolidColorBrush failureRedColor = new SolidColorBrush(Colors.Red);
                for (int row = 0; row <= 3; row++)
                {
                    for (int column = 0; column < _maxColumns; column++)
                    {
                        Ellipse ellipse = _ellipses[row, column];
                        ellipse.Stroke = failureRedColor;
                    }
                }
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
        
        private void EndScreen(bool isVictory)
        {
            if (isVictory)
            {
                if (tries == 1)
                {
                    CongratsTextBlock.Text = "Good job! You got it in " + tries.ToString() + " try!";
                }
                else
                {
                    CongratsTextBlock.Text = "Good job! You got it in " + tries.ToString() + " tries!";
                }
            }
            else
            {
                CongratsTextBlock.Text = "You didn't crack the code.";
            }
        }
        private void StopGame()
        {
            Red.IsEnabled = false;
            Orange.IsEnabled = false;
            Gray.IsEnabled = false;
            Violet.IsEnabled = false;
            Blue.IsEnabled = false;
            Green.IsEnabled = false;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                PauseButton.Content = "\uE768";
                foreach (var child in ColorButtonGrid.Children)
                {
                    Button button = child as Button;
                    button.IsEnabled = false;
                }
                BacktoStartButton.IsEnabled = false;
                ShowSolutionButton.IsEnabled = false;
            }
            else
            {
                PauseButton.Content = "\uE769";
                foreach (var child in ColorButtonGrid.Children)
                {
                    Button button = child as Button;
                    button.IsEnabled = true;
                }
                BacktoStartButton.IsEnabled = true;
                ShowSolutionButton.IsEnabled = true;
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

            _isSolutionVisible = true;
            EndScreen(false);
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            StartGrid.Visibility = Visibility.Collapsed;
            GameGrid.Visibility = Visibility.Visible;
            ResetVariables();
            CreateBoard();
            _currentCode = CreateCode();
        }

        private void BacktoStartButton_Click(object sender, RoutedEventArgs e)
        {
            GameGrid.Visibility = Visibility.Collapsed;
            StartGrid.Visibility = Visibility.Visible;
        }
        
        private void ResetVariables()
        {
            _pegRow = 0;
            _pegCol = 0;
            isPaused = false;
            _isSolutionVisible = false;
            _ellipses = new Ellipse[4, _maxColumns];
            _textboxes1 = new TextBlock[_maxAttempts];
            _textboxes2 = new TextBlock[_maxAttempts];
            _allColors = new Color[6] { Colors.Red, Colors.Orange, Colors.Gray, Colors.Violet, Colors.Blue, Colors.Green };
            tries = 0;
            CongratsTextBlock.Text = "";
            ResetColorButtons();
            InitializeLayout();
        }

        private void ResetColorButtons()
        {
            Red.IsEnabled = true;
            Orange.IsEnabled = true;
            Gray.IsEnabled = true;
            Violet.IsEnabled = true;
            Blue.IsEnabled = true;
            Green.IsEnabled = true;
        }

        private void Duplicates_Click(object sender, RoutedEventArgs e)
        {
            if (_isDuplicates)
            {
                Duplicates.Content = "Duplicates: Off";
                _maxAttempts = 10;
            }
            else
            {
                Duplicates.Content = "Duplicates: On";
                _maxAttempts = 15;
            }
            _maxColumns = _maxAttempts + 1;
            _isDuplicates = !_isDuplicates;
        }
    }
}