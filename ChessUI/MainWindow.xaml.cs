using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;
using ChessLogic.Moves;

namespace ChessUI;

public partial class MainWindow
{
    readonly Image[,] _pieceImages = new Image[8, 8];
    readonly Rectangle[,] _highLights = new Rectangle[8, 8];
    readonly Dictionary<Position, Move> _movesCache = new();

    Position? _selectedPosition;
    GameState _gameState = new(Player.White, Board.Initial());
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeBoard();

        DrawBoard(_gameState.Board);
        SetCursor(_gameState.CurrentPlayer);
    }

    void InitializeBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Image image = new Image();
                _pieceImages[i, j] = image;
                
                /*LinearGradientBrush gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new Point(0, 0);
                gradientBrush.EndPoint = new Point(1, 1);

                if ((i + j) % 2 == 0)
                {
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(230, 179, 134), 0)); // Светло-коричневый
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(139, 87, 42), 0.5)); // Темно-коричневый
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 228, 196), 1)); // Песочный
                }
                else
                {
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(139, 87, 42), 0)); // Темно-коричневый
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(230, 179, 134), 0.5)); // Светло-коричневый
                    gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 228, 196), 1)); // Песочный
                }
                
                StackPanel stackPanel = new StackPanel
                {
                    //Background = (i + j) % 2 == 0 ? Brushes.Black :  Brushes.White,
                    Background = gradientBrush,
                };
                stackPanel.Children.Add(image);
                PieceGrid.Children.Add(stackPanel);*/ //цвета своего стиля, ZIndex перекрывает как надо
                
                PieceGrid.Children.Add(image);

                Rectangle rectangle = new Rectangle();
                _highLights[i, j] = rectangle;
                HighLightGrid.Children.Add(rectangle);
            }
        }
    }

    void DrawBoard(Board board)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                _pieceImages[i, j].Source = Images.GetImage(board[i, j]);
            }
        }
    }

    void BoardGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Point point = e.GetPosition(BoardGrid);
        var position = ToSquarePosition(point);

        if (_selectedPosition is null)
        {
            IEnumerable<Move> moves = _gameState.LegalMovesForPieces(position);

            if (moves.Any())
            {
                _selectedPosition = position;
                CacheMoves(moves);
                ShowHighlights();
            }
        }
        else
        {
            /*var moves = _gameState.LegalMovesForPieces(position);
            if (moves.Any())
            {
                HideHighlights();
                _selectedPosition = position;
                CacheMoves(moves);
                ShowHighlights();
                return;
            }*/
            
            _selectedPosition = null;
            HideHighlights();

            if (_movesCache.TryGetValue(position, out var move))
            {
                HandleMove(move);
            }
        }
    }

    void HandleMove(Move move)
    {
        _gameState.MakeMove(move);
        DrawBoard(_gameState.Board);
        SetCursor(_gameState.CurrentPlayer);
    }

    Position ToSquarePosition(Point point)
    {
        double squareSize = BoardGrid.ActualWidth / 8;
        int row = (int)(point.Y / squareSize);
        int column = (int)(point.X / squareSize);
        return new Position(row, column);
    }

    void CacheMoves(IEnumerable<Move> moves)
    {
        _movesCache.Clear();
        foreach (var move in moves)
        {
            _movesCache[move.ToPos] = move;
        }
    }

    void ShowHighlights()
    {
        Color color = Color.FromArgb(150, 125, 255, 125); //в глоб цвет

        foreach (var to in _movesCache.Keys)
        {
            _highLights[to.Row, to.Column].Fill = new SolidColorBrush(color);
        }
    }

    void HideHighlights()
    {
        foreach (var to in _movesCache.Keys)
        {
            _highLights[to.Row, to.Column].Fill = Brushes.Transparent;
        }
    }

    void SetCursor(Player player)
    {
        Cursor = player == Player.White ? ChessCursors.WhiteCursor : ChessCursors.BlackCursor;
    }
}
