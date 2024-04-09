using System.Windows;
using System.Windows.Controls;
using ChessLogic;
using ChessLogic.Pieces;

namespace ChessUI;

public partial class MainWindow : Window
{
    private readonly Image[,] _pieceImages = new Image[8, 8];

    GameState _gameState = new(Player.White, Board.Initial());
    
    public MainWindow()
    {
        InitializeComponent();
        InitializeBoard();

        DrawBoard(_gameState.Board);
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
            }
        }
    }

    void DrawBoard(Board board)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piece piece = board[i, j];
                _pieceImages[i, j].Source = Images.GetImage(piece);
            }
        }
    }
}