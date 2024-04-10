using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;
using ChessLogic.Moves;
using ChessLogic.Pieces;

namespace ChessUI;

public partial class MainWindow
{
    readonly Image[,] _pieceImages = new Image[8, 8];
    readonly Rectangle[,] _highLights = new Rectangle[8, 8];
    readonly Dictionary<Position, Move> _movesCache = new();

    Position? _selectedPosition;
    GameState _gameState;
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeBoard();
        
        PrepareBoard();
    }

    void PrepareBoard()
    {
        HideHighlights();
        _movesCache.Clear();
        _gameState = new GameState(Player.White, Board.Initial());
        
        DrawBoard(_gameState.Board);
        SetCursor(_gameState.CurrentPlayer);
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

    void InitializeBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Image image = new Image();
                _pieceImages[i, j] = image;
                
                
                /*StackPanel stackPanel = new StackPanel
                {
                    Background = (i + j) % 2 == 0 ? Brushes.Black :  Brushes.White,
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

    void BoardGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (IsMenuOnScreen())
        {
            return;
        }
        
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
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }
                
            }
        }
    }

    void HandlePromotion(Position from, Position to)
    {
        //TODO убрать?
        _pieceImages[to.Row, to.Column].Source = Images.GetImage(_gameState.CurrentPlayer, PieceType.Pawn);
        _pieceImages[from.Row, from.Column].Source = null;

        PromotionMenu promotionMenu = new PromotionMenu(_gameState.CurrentPlayer);
        MenuContainer.Content = promotionMenu;

        promotionMenu.PieceSelected += type =>
        {
            MenuContainer.Content = null;
            Move promMove = new PawnPromotion(from, to, type);
            HandleMove(promMove);
        };
    }

    void HandleMove(Move move)
    {
        _gameState.MakeMove(move);
        
        DrawBoard(_gameState.Board);
        SetCursor(_gameState.CurrentPlayer);
        
        if (_gameState.IsGameOver())
        {
            ShowGameOver();
        }
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

    bool IsMenuOnScreen() => MenuContainer.Content != null;

    void ShowGameOver()
    {
        GameOverMenu gameOverMenu = new GameOverMenu(_gameState);
        MenuContainer.Content = gameOverMenu;
        gameOverMenu.OptionSelected += option =>
        {
            if (option == Option.Restart)
            {
                PrepareBoard();
                MenuContainer.Content = null;
            }
            else
            {
                Application.Current.Shutdown();
            }
        };
        
    }
}
