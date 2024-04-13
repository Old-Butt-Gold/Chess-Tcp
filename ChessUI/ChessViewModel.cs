using System.Windows;
using System.Windows.Input;
using ChessLogic;
using ChessLogic.Bot;
using ChessLogic.CoordinateClasses;
using ChessLogic.Moves;
using ChessLogic.Pieces;
using ChessUI.Commands;
using ChessUI.ContentControls;

namespace ChessUI;

public class ChessViewModel : IDisposable
{
    public ICommand EscapeMenuCommand => new RelayCommand(EscapeMenu);
    public ICommand MouseDownCommand { get; private set; }
    
    public UIChessManager UiChessManager { get; private set; } = new();
    public BoardDrawer BoardDrawer { get; private set; } = new();
    public GameManager GameManager { get; private set; }
    public BotManager BotManager { get; private set; }

    bool _isBotThinking;
    
    Player _startPlayer;
    
    public ChessViewModel(Player startPlayer, GameType gameType, BotDifficulty botDifficulty)
    {
        _startPlayer = startPlayer;
        
        if (gameType == GameType.PlayerVersusPlayer)
        {
            Board.IsBoardReversed = _startPlayer == Player.Black;
        }
        else
        {
            BotManager = new(botDifficulty);
        }
     
        switch (gameType)
        {
            case GameType.BotVersusBot:
                MouseDownCommand = new RelayCommand(MouseDownBotVBot, _ => !UiChessManager.IsMenuOnScreen());
                break;
            case GameType.PlayerVersusBot:
                MouseDownCommand = new RelayCommand(MouseDownPlayerVBot, _ => !UiChessManager.IsMenuOnScreen() && !_isBotThinking);
                break;
            case GameType.PlayerVersusPlayer:
                MouseDownCommand = new RelayCommand(MouseDownPvp, _ => !UiChessManager.IsMenuOnScreen());
                break;
        }
    }

    public void Start()
    {
        BoardDrawer.SetHighLights(UiChessManager.HighLightGrid);
        BoardDrawer.SetPieceImages(UiChessManager.PieceGrid);
        Reload();
    }

    void Reload()
    {
        GameManager = new GameManager(Player.White, Board.Initial());
        BoardDrawer.ReloadBoard(GameManager.Board);
        UiChessManager.SetCursor(GameManager.CurrentPlayer);
    }
    
    void HandlePromotionMove(Position from, Position to)
    {
        BoardDrawer.MoveImagePiece(from, to, GameManager.CurrentPlayer, PieceType.Pawn);

        PromotionMenu promotionMenu = new PromotionMenu(GameManager.CurrentPlayer);
        UiChessManager.MenuContainer.Content = promotionMenu;

        promotionMenu.PieceSelected += type =>
        {
            UiChessManager.MenuContainer.Content = null;
            Move promMove = new PawnPromotion(from, to, type);
            HandleMove(promMove);
        };
    }

    void HandleMove(Move move)
    {
        GameManager.MakeMove(move);
        
        BoardDrawer.DrawBoard(GameManager.Board);
        UiChessManager.SetCursor(GameManager.CurrentPlayer);
        
        if (GameManager.IsGameOver())
        {
            ShowGameOver();
        }
    }

    void ShowGameOver()
    {
        GameOverMenu gameOverMenu = new GameOverMenu(GameManager);
        UiChessManager.MenuContainer.Content = gameOverMenu;
        gameOverMenu.OptionSelected += option =>
        {
            if (option == Option.Restart)
            {
                Reload();
                UiChessManager.MenuContainer.Content = null;
            }
            else
            {
                Application.Current.Shutdown();
            }
        };
        
    }
    
    void EscapeMenu(object obj)
    {
        if (!UiChessManager.IsMenuOnScreen())
        {
            PauseMenu pauseMenu = new();
            UiChessManager.MenuContainer.Content = pauseMenu;

            pauseMenu.OptionSelected += option =>
            {
                UiChessManager.MenuContainer.Content = null;
                if (option == Option.Restart)
                {
                    Reload();
                }
            };
        } 
    }
    
    async void MouseDownPlayerVBot(object obj)
    {
        if (obj is MouseButtonEventArgs e)
        {
            var position = UiChessManager.ToSquarePosition(e);

            if (BoardDrawer.SelectedPosition is null)
            {
                IEnumerable<Move> moves = GameManager.LegalMovesForPieces(position);
                BoardDrawer.ShowPossibleMoves(position, moves);
            }
            else
            {
                var move = BoardDrawer.TryToGetMove(position);
                if (move != null)
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

            BoardDrawer.DrawKingCheck(GameManager.Board, GameManager.CurrentPlayer);
        
            if (GameManager.CurrentPlayer == _startPlayer.Opponent())
            {
                await HandleBotMoveAsync();
            }
        }
    }

    async void MouseDownBotVBot(object obj)
    {
        if (obj is MouseButtonEventArgs e)
        {
            while (!UiChessManager.IsMenuOnScreen())
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
            var position = UiChessManager.ToSquarePosition(e);

            if (BoardDrawer.SelectedPosition is null)
            {
                IEnumerable<Move> moves = GameManager.LegalMovesForPieces(position);
                BoardDrawer.ShowPossibleMoves(position, moves);
            }
            else
            {
                var move = BoardDrawer.TryToGetMove(position);
                if (move != null)
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

            BoardDrawer.DrawKingCheck(GameManager.Board, GameManager.CurrentPlayer);
        }
    }

    async Task HandleBotMoveAsync()
    {
        _isBotThinking = true;
        await Task.Delay(1000);
        var (moveFinal, pieceType, moves) = BotManager.GetBestMove(GameManager);
        if (moveFinal != null)
        {
            BoardDrawer.ShowPossibleMoves(moveFinal.FromPos, moves);
            await Task.Delay(1000);
            if (moveFinal.Type == MoveType.PawnPromotion)
            {
                moveFinal = new PawnPromotion(moveFinal.FromPos, moveFinal.ToPos, pieceType!.Value);
            }
            BoardDrawer.TryToGetMove(moveFinal.ToPos);
            HandleMove(moveFinal);
        }
        BoardDrawer.DrawKingCheck(GameManager.Board, GameManager.CurrentPlayer);
        _isBotThinking = false;
    }

    public void Dispose()
    {
        BotManager?.Dispose();
    }
}