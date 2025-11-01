using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Nexus_Omok_Game
{
    public partial class MainWindow : Window
    {
        private const int BOARD_SIZE = 15;
        private const double CELL_SIZE = 40.0;
        private const double STONE_RADIUS = 15.0;
        private const double BOARD_MARGIN = 20.0;

        private int[,] board = new int[BOARD_SIZE, BOARD_SIZE];
        private int currentPlayer = 1;
        private bool gameOver = false;
        private Ellipse? hoverStone = null;
        private Line? winningLine = null;

        // AI fields
        private GameSettings gameSettings;
        private IAIPlayer? aiPlayer;
        private int aiPlayerNumber;
        private bool isAIThinking = false;

        // 기본 생성자 (XAML 디자이너용 - 사용되지 않음)
        public MainWindow() : this(new GameSettings())
        {
        }

        // 파라미터 있는 생성자 (실제 사용)
        public MainWindow(GameSettings settings)
        {
            InitializeComponent();
            gameSettings = settings;

            if (gameSettings.Mode == GameMode.VsAI)
            {
                aiPlayer = AIPlayerFactory.Create(gameSettings.AIDifficulty);
                aiPlayerNumber = gameSettings.IsPlayerBlack ? 2 : 1;
            }

            InitializeGame();
        }

        private void InitializeGame()
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    board[row, col] = 0;
                }
            }

            currentPlayer = 1;
            gameOver = false;
            hoverStone = null;
            winningLine = null;
            isAIThinking = false;

            GameCanvas.Children.Clear();
            DrawBoard();

            UpdateTurnDisplay();

            if (gameSettings.Mode == GameMode.VsAI)
            {
                string diffText = gameSettings.AIDifficulty == AIDifficulty.Easy ? "Easy" :
           gameSettings.AIDifficulty == AIDifficulty.Normal ? "Normal" : "Hard";
                StatusText.Text = $"AI Mode - Difficulty: {diffText}";

                if (aiPlayerNumber == 1)
                {
                    _ = ExecuteAIMove();
                }
            }
            else
            {
                StatusText.Text = "Welcome to Nexus Omok Game! Black plays first.";
            }
        }

        private void DrawBoard()
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                Line vLine = new Line
                {
                    X1 = BOARD_MARGIN + i * CELL_SIZE,
                    Y1 = BOARD_MARGIN,
                    X2 = BOARD_MARGIN + i * CELL_SIZE,
                    Y2 = BOARD_MARGIN + (BOARD_SIZE - 1) * CELL_SIZE,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                GameCanvas.Children.Add(vLine);

                Line hLine = new Line
                {
                    X1 = BOARD_MARGIN,
                    Y1 = BOARD_MARGIN + i * CELL_SIZE,
                    X2 = BOARD_MARGIN + (BOARD_SIZE - 1) * CELL_SIZE,
                    Y2 = BOARD_MARGIN + i * CELL_SIZE,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                GameCanvas.Children.Add(hLine);
            }

            int[] starPoints = { 3, 7, 11 };
            foreach (int row in starPoints)
            {
                foreach (int col in starPoints)
                {
                    Ellipse starPoint = new Ellipse
                    {
                        Width = 8,
                        Height = 8,
                        Fill = Brushes.Black
                    };
                    Canvas.SetLeft(starPoint, BOARD_MARGIN + col * CELL_SIZE - 4);
                    Canvas.SetTop(starPoint, BOARD_MARGIN + row * CELL_SIZE - 4);
                    GameCanvas.Children.Add(starPoint);
                }
            }
        }

        private void GameCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (gameOver)
            {
                StatusText.Text = "Game is over! Click 'New Game' to play again.";
                return;
            }

            if (isAIThinking)
            {
                StatusText.Text = "AI is thinking... Please wait.";
                return;
            }

            if (gameSettings.Mode == GameMode.VsAI && currentPlayer == aiPlayerNumber)
            {
                StatusText.Text = "It's AI's turn. Please wait.";
                return;
            }

            Point clickPoint = e.GetPosition(GameCanvas);
            var (row, col) = GetNearestIntersection(clickPoint);

            if (row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE)
            {
                if (board[row, col] == 0)
                {
                    if (currentPlayer == 1 && IsDoubleThree(row, col))
                    {
                        StatusText.Text = "Forbidden move: Double Three (3-3) is not allowed for Black!";
                        StatusText.Foreground = Brushes.Orange;
                        PlayErrorSound();
                        return;
                    }

                    PlaceStoneAndCheckGame(row, col);
                }
                else
                {
                    StatusText.Text = "This position is already occupied!";
                }
            }
        }

        private void PlaceStoneAndCheckGame(int row, int col)
        {
            board[row, col] = currentPlayer;
            DrawStone(row, col, currentPlayer);
            PlaySound();

            if (CheckWin(row, col, currentPlayer))
            {
                gameOver = true;
                string winner = GetPlayerName(currentPlayer);
                StatusText.Text = $"{winner} wins! Congratulations!";
                StatusText.FontSize = 24;
                StatusText.Foreground = currentPlayer == 1 ? Brushes.DarkSlateGray : Brushes.WhiteSmoke;
                return;
            }

            if (IsBoardFull())
            {
                gameOver = true;
                StatusText.Text = "Draw! The board is full.";
                return;
            }

            currentPlayer = currentPlayer == 1 ? 2 : 1;
            UpdateTurnDisplay();

            if (gameSettings.Mode == GameMode.VsAI && currentPlayer == aiPlayerNumber)
            {
                _ = ExecuteAIMove();
            }
        }

        private string GetPlayerName(int player)
        {
            if (gameSettings.Mode == GameMode.VsAI)
            {
                if (player == aiPlayerNumber)
                    return "AI";
                else
                    return "You";
            }
            else
            {
                return player == 1 ? "Black" : "White";
            }
        }

        private async Task ExecuteAIMove()
        {
            isAIThinking = true;
            StatusText.Text = "AI is thinking...";
            StatusText.Foreground = Brushes.Orange;
            GameCanvas.IsEnabled = false;

            try
            {
                var (row, col) = await aiPlayer!.GetNextMoveAsync(board, aiPlayerNumber);
                await Task.Delay(new Random().Next(500, 1500));

                PlaceStoneAndCheckGame(row, col);

                StatusText.Text = "AI placed a stone!";
                StatusText.Foreground = Brushes.White;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"AI Error: {ex.Message}";
                StatusText.Foreground = Brushes.Red;
            }
            finally
            {
                isAIThinking = false;
                GameCanvas.IsEnabled = true;
            }
        }

        private void GameCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (gameOver) return;
            if (isAIThinking) return;

            if (gameSettings.Mode == GameMode.VsAI && currentPlayer == aiPlayerNumber)
                return;

            Point mousePoint = e.GetPosition(GameCanvas);
            var (row, col) = GetNearestIntersection(mousePoint);

            if (row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE && board[row, col] == 0)
            {
                if (hoverStone != null)
                {
                    GameCanvas.Children.Remove(hoverStone);
                }

                hoverStone = new Ellipse
                {
                    Width = STONE_RADIUS * 2,
                    Height = STONE_RADIUS * 2,
                    Fill = currentPlayer == 1 ?
new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)) :
 new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1,
                    IsHitTestVisible = false
                };

                double x = BOARD_MARGIN + col * CELL_SIZE - STONE_RADIUS;
                double y = BOARD_MARGIN + row * CELL_SIZE - STONE_RADIUS;
                Canvas.SetLeft(hoverStone, x);
                Canvas.SetTop(hoverStone, y);
                GameCanvas.Children.Add(hoverStone);
            }
            else
            {
                if (hoverStone != null)
                {
                    GameCanvas.Children.Remove(hoverStone);
                    hoverStone = null;
                }
            }
        }

        private void GameCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (hoverStone != null)
            {
                GameCanvas.Children.Remove(hoverStone);
                hoverStone = null;
            }
        }

        private (int row, int col) GetNearestIntersection(Point point)
        {
            int col = (int)Math.Round((point.X - BOARD_MARGIN) / CELL_SIZE);
            int row = (int)Math.Round((point.Y - BOARD_MARGIN) / CELL_SIZE);
            return (row, col);
        }

        private void DrawStone(int row, int col, int player)
        {
            Ellipse stone = new Ellipse
            {
                Width = STONE_RADIUS * 2,
                Height = STONE_RADIUS * 2,
                Fill = player == 1 ? Brushes.Black : Brushes.White,
                Stroke = player == 1 ? Brushes.DarkGray : Brushes.Black,
                StrokeThickness = 2
            };

            if (player == 1)
            {
                RadialGradientBrush gradient = new RadialGradientBrush();
                gradient.GradientStops.Add(new GradientStop(Colors.DarkGray, 0));
                gradient.GradientStops.Add(new GradientStop(Colors.Black, 1));
                stone.Fill = gradient;
            }
            else
            {
                RadialGradientBrush gradient = new RadialGradientBrush();
                gradient.GradientStops.Add(new GradientStop(Colors.White, 0));
                gradient.GradientStops.Add(new GradientStop(Colors.LightGray, 1));
                stone.Fill = gradient;
            }

            double x = BOARD_MARGIN + col * CELL_SIZE - STONE_RADIUS;
            double y = BOARD_MARGIN + row * CELL_SIZE - STONE_RADIUS;
            Canvas.SetLeft(stone, x);
            Canvas.SetTop(stone, y);
            GameCanvas.Children.Add(stone);
        }

        private bool CheckWin(int lastRow, int lastCol, int player)
        {
            int[][] directions = new int[][]
      {
                new int[] { 0, 1 },
       new int[] { 1, 0 },
        new int[] { 1, 1 },
     new int[] { 1, -1 }
        };

            foreach (var dir in directions)
            {
                int count = 1;
                int startRow = lastRow;
                int startCol = lastCol;
                int endRow = lastRow;
                int endCol = lastCol;

                int r = lastRow + dir[0];
                int c = lastCol + dir[1];
                while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
                {
                    count++;
                    endRow = r;
                    endCol = c;
                    r += dir[0];
                    c += dir[1];
                }

                r = lastRow - dir[0];
                c = lastCol - dir[1];
                while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == player)
                {
                    count++;
                    startRow = r;
                    startCol = c;
                    r -= dir[0];
                    c -= dir[1];
                }

                if (count == 5)
                {
                    DrawWinningLine(startRow, startCol, endRow, endCol);
                    return true;
                }
            }

            return false;
        }

        private void DrawWinningLine(int startRow, int startCol, int endRow, int endCol)
        {
            winningLine = new Line
            {
                X1 = BOARD_MARGIN + startCol * CELL_SIZE,
                Y1 = BOARD_MARGIN + startRow * CELL_SIZE,
                X2 = BOARD_MARGIN + endCol * CELL_SIZE,
                Y2 = BOARD_MARGIN + endRow * CELL_SIZE,
                Stroke = Brushes.Red,
                StrokeThickness = 4,
                Opacity = 0.7
            };
            GameCanvas.Children.Add(winningLine);
        }

        private bool IsBoardFull()
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    if (board[row, col] == 0)
                        return false;
                }
            }
            return true;
        }

        private void UpdateTurnDisplay()
        {
            string turnText;
            string statusText;

            if (gameSettings.Mode == GameMode.VsAI)
            {
                if (currentPlayer == aiPlayerNumber)
                {
                    string aiColor = aiPlayerNumber == 1 ? "Black" : "White";
                    turnText = $"Current Turn: AI ({aiColor})";
                    statusText = "AI's turn to play.";
                }
                else
                {
                    string playerColor = aiPlayerNumber == 1 ? "White" : "Black";
                    turnText = $"Current Turn: You ({playerColor})";
                    statusText = "Your turn to play.";
                }
            }
            else
            {
                if (currentPlayer == 1)
                {
                    turnText = "Current Turn: Black";
                    statusText = "Black's turn to play.";
                }
                else
                {
                    turnText = "Current Turn: White";
                    statusText = "White's turn to play.";
                }
            }

            CurrentTurnText.Text = turnText;
            StatusText.Text = statusText;
            StatusText.FontSize = 20;
            StatusText.Foreground = Brushes.White;
        }

        private void PlaySound()
        {
            try
            {
                SystemSounds.Beep.Play();
            }
            catch
            {
            }
        }

        private void PlayErrorSound()
        {
            try
            {
                SystemSounds.Hand.Play();
            }
            catch
            {
            }
        }

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
           "Are you sure you want to start a new game?",
          "New Game",
                    MessageBoxButton.YesNo,
        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                InitializeGame();
            }
        }

        private bool IsDoubleThree(int row, int col)
        {
            int openThreeCount = 0;
            int[][] directions = new int[][]
       {
                new int[] { 0, 1 },
       new int[] { 1, 0 },
     new int[] { 1, 1 },
       new int[] { 1, -1 }
       };

            board[row, col] = 1;

            foreach (var dir in directions)
            {
                if (CountOpenThree(row, col, dir[0], dir[1]))
                {
                    openThreeCount++;
                }
            }

            board[row, col] = 0;

            return openThreeCount >= 2;
        }

        private bool CountOpenThree(int row, int col, int dRow, int dCol)
        {
            int posCount = 0;
            int negCount = 0;

            int r = row + dRow;
            int c = col + dCol;
            while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == 1)
            {
                posCount++;
                r += dRow;
                c += dCol;
            }
            bool posOpen = (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == 0);

            r = row - dRow;
            c = col - dCol;
            while (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == 1)
            {
                negCount++;
                r -= dRow;
                c -= dCol;
            }
            bool negOpen = (r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE && board[r, c] == 0);

            int count = 1 + posCount + negCount;

            return count == 3 && posOpen && negOpen;
        }
    }
}
