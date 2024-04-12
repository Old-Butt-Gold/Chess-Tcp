using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;
using ChessLogic.Bot;
using ChessLogic.CoordinateClasses;
using ChessLogic.Moves;
using ChessLogic.Pieces;
using ChessUI.Commands;

namespace ChessUI;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private Grid _boardGrid;
    private UniformGrid _highlightGrid;
    private UniformGrid _pieceGrid;
    private ContentControl _menuContainer;
    private Cursor _cursor;

    public Grid BoardGrid
    {
        get => _boardGrid;
        set
        {
            if (_boardGrid != value)
            {
                _boardGrid = value;
                OnPropertyChanged(nameof(BoardGrid));
            }
        }
    }
    
    public UniformGrid HighLightGrid
    {
        get => _highlightGrid;
        set
        {
            if (_highlightGrid != value)
            {
                _highlightGrid = value;
                OnPropertyChanged(nameof(HighLightGrid));
            }
        }
    }

    public UniformGrid PieceGrid
    {
        get => _pieceGrid;
        set
        {
            if (_pieceGrid != value)
            {
                _pieceGrid = value;
                OnPropertyChanged(nameof(PieceGrid));
            }
        }
    }
    
    public ContentControl MenuContainer
    {
        get => _menuContainer;
        set
        {
            if (_menuContainer != value)
            {
                _menuContainer = value;
                OnPropertyChanged(nameof(MenuContainer));
            }
        }
    }
    
    public ICommand EscapeMenuCommand { get; private set; }
    public ICommand MouseDownCommand { get; private set; }
    
    readonly Image[,] _pieceImages = new Image[8, 8];
    readonly Shape[,] _highLights = new Shape[8, 8];
    readonly Dictionary<Position, Move> _movesCache = new();

    Position? _selectedPosition;
    Position? _kingPosition;
    public GameState GameState { get; private set; }

    bool _isEnemyThinking;
    
    GameType _gameType;
    BotDifficulty _botDifficulty;
    Player _startPlayer;

    Brush _dangerBrush = new SolidColorBrush(Color.FromArgb(100, 245, 39, 65));
    Brush _legalBrush = new SolidColorBrush(Color.FromArgb(150, 125, 255, 125));
    
    public MainWindowViewModel(Player startPlayer, GameType gameType, BotDifficulty botDifficulty)
    {
        EscapeMenuCommand = new RelayCommand(EscapeMenu);

        _gameType = gameType;
        _startPlayer = startPlayer;
        _botDifficulty = botDifficulty;
        
        CreateGameByType(gameType);
    }

    public void Start()
    {
        InitializeBoard();
        ReloadBoard();
    }

    void ReloadBoard()
    {
        _selectedPosition = null;
        _movesCache.Clear();
        //TODO переворот доски для пользователя
        GameState = _gameType != GameType.PlayerVersusPlayer 
            ? new GameState(Player.White, Board.Initial(), _botDifficulty) 
            : new GameState(Player.White, Board.Initial());

        DrawBoard(GameState.Board);
        SetCursor(GameState.CurrentPlayer);
        
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
    
    void DrawKingCheck()
    {
        if (GameState.Board.IsInCheck(GameState.CurrentPlayer))
        {
            _kingPosition = GameState.Board.PiecePositionsFor(GameState.CurrentPlayer)
                .First(x => GameState.Board[x].Type == PieceType.King);
            Draw(_kingPosition, _dangerBrush);
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
        _pieceImages[to.Row, to.Column].Source = Images.GetImage(GameState.CurrentPlayer, PieceType.Pawn);
        _pieceImages[from.Row, from.Column].Source = null;

        PromotionMenu promotionMenu = new PromotionMenu(GameState.CurrentPlayer);
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
        GameState.MakeMove(move);
        
        DrawBoard(GameState.Board);
        SetCursor(GameState.CurrentPlayer);
        
        if (GameState.IsGameOver())
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
        foreach (var to in _movesCache.Keys)
        {
            _highLights[to.Row, to.Column].Fill = _legalBrush;
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
        BoardGrid.Cursor = player == Player.White ? ChessCursors.WhiteCursor : ChessCursors.BlackCursor;
    }

    bool IsMenuOnScreen() => MenuContainer.Content != null;

    void ShowGameOver()
    {
        GameOverMenu gameOverMenu = new GameOverMenu(GameState);
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

    void EscapeMenu(object obj)
    {
        if (!IsMenuOnScreen())
        {
            ShowPauseMenu();
        } 
    }
    
    void CreateGameByType(GameType gameType)
    {
        switch (gameType)
        {
            case GameType.BotVersusBot:
                MouseDownCommand = new RelayCommand(MouseDownBotVBot, _ => !IsMenuOnScreen());
                break;
            case GameType.PlayerVersusBot:
                MouseDownCommand = new RelayCommand(MouseDownPlayerVBot, _ => !IsMenuOnScreen() || !_isEnemyThinking);
                break;
            case GameType.PlayerVersusPlayer:
                MouseDownCommand = new RelayCommand(MouseDownPvp, _ => !IsMenuOnScreen());
                break;
        }
    }

    async void MouseDownPlayerVBot(object obj)
    {
        if (obj is MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(PieceGrid);
            var position = ToSquarePosition(point);

            if (_selectedPosition is null)
            {
                IEnumerable<Move> moves = GameState.LegalMovesForPieces(position);

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
        
            if (GameState.CurrentPlayer == _startPlayer.Opponent())
            {
                await HandleBotMoveAsync();
            }
        }
    }

    async void MouseDownBotVBot(object obj)
    {
        if (obj is MouseButtonEventArgs e)
        {
            while (!IsMenuOnScreen())
            {
                await HandleBotMoveAsync();
            }
        }
    }
    
    //TODO сети
    void MouseDownPvp(object obj)
    {
        if (obj is MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(PieceGrid);
            var position = ToSquarePosition(point);

            if (_selectedPosition is null)
            {
                IEnumerable<Move> moves = GameState.LegalMovesForPieces(position);

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
        }
    }

    async Task HandleBotMoveAsync()
    {
        _isEnemyThinking = true;
        await Task.Delay(1000);
        var result = await GameState.GetBestMoveAsync();
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}