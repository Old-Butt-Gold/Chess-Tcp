using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;
using ChessLogic.CoordinateClasses;
using ChessLogic.Moves;
using ChessLogic.Pieces;
using Color = System.Windows.Media.Color;
using Move = ChessLogic.Moves.Move;

namespace ChessUI;

public partial class MainWindow
{
    readonly Image[,] _pieceImages = new Image[8, 8];
    readonly Shape[,] _highLights = new Shape[8, 8];
    readonly Dictionary<Position, Move> _movesCache = new();

    Position? _selectedPosition;
    Position? _kingPosition;
    GameState _gameState;

    bool _isEnemyThinking;
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeBoard();
        
        ReloadBoard();
    }

    void ReloadBoard()
    {
        _selectedPosition = null;
        HideHighlights();
        _movesCache.Clear();
        _gameState = new GameState(Player.White, Board.Initial());
        
        DrawBoard(_gameState.Board);
        SetCursor(_gameState.CurrentPlayer);
        
        _kingPosition = null;
        foreach (var highLight in _highLights)
        {
            highLight.Fill = Brushes.Transparent;
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
    
    void InitializeBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Image image = new Image();
                _pieceImages[i, j] = image;
                
                PieceGrid.Children.Add(image);

                Ellipse ellipse = new Ellipse
                {
                    Height = 45,
                    Width = 45
                };

                _highLights[i, j] = ellipse;
                HighLightGrid.Children.Add(ellipse);
                
            }
        }
    }

    async void BoardGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (IsMenuOnScreen())
        {
            return;
        }

        if (_isEnemyThinking)
        {
            return;
        }
        
        Point point = e.GetPosition(PieceGrid);
        var position = ToSquarePosition(point);

        if (_selectedPosition is null)
        {
            IEnumerable<Move> moves = _gameState.LegalMovesForPieces(position);

            if (moves.Any())
            {
                _selectedPosition = position;
                
                _movesCache.Clear();
                foreach (var move in moves)
                {
                    _movesCache[move.ToPos] = move;
                }
                
                ShowHighlights();
            }
        }
        else
        {
            _selectedPosition = null;
            HideHighlights();
            
            if (_movesCache.TryGetValue(position, out var move))
            {
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotionMove(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }
            }
        }

        DrawKingCheck();
        
        if (_gameState.CurrentPlayer == Player.Black)
        {
            await HandleBotMoveAsync();
        }
    }
    
    async Task HandleBotMoveAsync()
    {
        _isEnemyThinking = true;
        await Task.Delay(1000);
        var result = await _gameState.GetBestMoveAsync();
        var moveFinal = result.move;
        var pieceType = result.pieceType;
        if (moveFinal != null)
        {
            if (moveFinal.Type == MoveType.PawnPromotion)
            {
                moveFinal = new PawnPromotion(moveFinal.FromPos, moveFinal.ToPos, pieceType!.Value);
            }
            HandleMove(moveFinal);
        }
        DrawKingCheck();
        _isEnemyThinking = false;
    }

    void DrawKingCheck()
    {
        if (_gameState.Board.IsInCheck(_gameState.CurrentPlayer))
        {
            _kingPosition = _gameState.Board.PiecePositionsFor(_gameState.CurrentPlayer)
                .First(x => _gameState.Board[x].Type == PieceType.King);
            Draw(_kingPosition, new SolidColorBrush(Color.FromArgb(100, 245, 39, 65)));
        }
        else
        {
            Draw(_kingPosition, Brushes.Transparent);
            _kingPosition = null;
        }

        void Draw(Position kingPos, Brush brush)
        {
            if (kingPos != null)
            {
                _highLights[kingPos.Row, kingPos.Column].Fill = brush;
            }
        }
    }

    void HandlePromotionMove(Position from, Position to)
    {
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
                ReloadBoard();
                MenuContainer.Content = null;
            }
            else
            {
                Application.Current.Shutdown();
            }
        };
        
    }

    void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsMenuOnScreen() && e.Key == Key.Escape)
        {
            ShowPauseMenu();
        } 
    }

    void ShowPauseMenu()
    {
        PauseMenu pauseMenu = new();
        MenuContainer.Content = pauseMenu;

        pauseMenu.OptionSelected += option =>
        {
            MenuContainer.Content = null;
            if (option == Option.Restart)
            {
                ReloadBoard();
            }
        };
    }
}

