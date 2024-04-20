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
    public ICommand? MouseDownCommand { get; private set; }
    public UIChessManager UiChessManager { get; private set; } = new();
    public BoardDrawer BoardDrawer { get; private set; } = new();
    public GameManager GameManager { get; private set; }
    public BotManager? BotManager { get; private set; }

    bool IsBotThinking { get; set; }
    Player StartPlayer { get; set; }
    
    public void Start(Player startPlayer, GameType gameType, BotDifficulty botDifficulty)
    {
        StopBot();
        
        StartPlayer = startPlayer;
        
        if (gameType == GameType.PlayerVersusPlayer)
        {
            Board.IsBoardReversed = StartPlayer == Player.Black;
        }
        else
        {
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
            BotManager = new(botDifficulty);
        }

        MouseDownCommand = gameType == GameType.PlayerVersusBot
            ? new RelayCommand(MouseDownPlayerVBot, _ => !UiChessManager.IsMenuOnScreen() && !IsBotThinking) 
            : new RelayCommand(MouseDownPvp, _ => !UiChessManager.IsMenuOnScreen());
        
        Reload();
    }

    void Reload()
    {
        BoardDrawer.InitializeHighLights(UiChessManager.HighLightGrid);
        BoardDrawer.InitializePieceImages(UiChessManager.PieceGrid);
        GameManager = new GameManager(Player.White);
        BoardDrawer.ReloadBoard(GameManager.Board);
        UiChessManager.SetCursor(GameManager.CurrentPlayer);
    }

    public void StopBot() //Для остановки и смены режима игры
    {
        CancellationTokenSource?.Cancel();
        CancellationTokenSource?.Dispose();
        //BoardDrawer.ClearHighLights(UiChessManager.HighLightGrid);
        //BoardDrawer.ClearPieceImages(UiChessManager.PieceGrid);
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
    }

    #region Bots game type

    CancellationTokenSource? CancellationTokenSource { get; set; }
    CancellationToken CancellationToken { get; set; }
    
    async void MouseDownPlayerVBot(object obj)
    {
        if (obj is MouseButtonEventArgs e)
        {
            if (StartPlayer == GameManager.CurrentPlayer)
            {
                HandlePlayerMove(e);
            }
            
            if (GameManager.CurrentPlayer == StartPlayer.Opponent())
            {
                IsBotThinking = true;
                await HandleBotMoveAsync(CancellationToken);
                IsBotThinking = false;
            }
        }
    }

    async Task HandleBotMoveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(750, cancellationToken);

            var (moveFinal, pieceType, moves) = BotManager!.GetBestMove(GameManager);
            if (moveFinal != null)
            {
                BoardDrawer.ShowPossibleMoves(moveFinal.FromPos, moves!);

                await Task.Delay(750, cancellationToken);

                if (moveFinal.Type == MoveType.PawnPromotion)
                {
                    moveFinal = new PawnPromotion(moveFinal.FromPos, moveFinal.ToPos, pieceType!.Value);
                }

                BoardDrawer.TryToGetMove(moveFinal.ToPos); //Для удаления предыдущего highlight'а
                HandleMove(moveFinal);
            }

            BoardDrawer.DrawKingCheck(GameManager.Board, GameManager.CurrentPlayer);
        } catch { }
    }

    #endregion 
    
    //TODO сети
    void MouseDownPvp(object obj)
    {
        if (obj is MouseButtonEventArgs e)
        {
            HandlePlayerMove(e);
        }
    }

    void HandlePlayerMove(MouseButtonEventArgs e)
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
    
    public void Dispose()
    {
        CancellationTokenSource?.Dispose();
        BotManager?.Dispose();
    }
}