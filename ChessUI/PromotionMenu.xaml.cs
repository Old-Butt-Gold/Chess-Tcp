using System.Windows.Input;
using ChessLogic;
using ChessLogic.Pieces;

namespace ChessUI;

public partial class PromotionMenu
{
    public event Action<PieceType>? PieceSelected;
    
    public PromotionMenu(Player player)
    {
        InitializeComponent();

        QueenImg.Source = Images.GetImage(player, PieceType.Queen);
        BishopImg.Source = Images.GetImage(player, PieceType.Bishop);
        RookImg.Source = Images.GetImage(player, PieceType.Rook);
        KnightImg.Source = Images.GetImage(player, PieceType.Knight);
    }

    private void QueenImg_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        PieceSelected?.Invoke(PieceType.Queen);
    }

    private void BishopImg_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        PieceSelected?.Invoke(PieceType.Bishop);
    }

    private void RookImg_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        PieceSelected?.Invoke(PieceType.Rook);
    }

    private void KnightImg_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        PieceSelected?.Invoke(PieceType.Knight);
    }
}