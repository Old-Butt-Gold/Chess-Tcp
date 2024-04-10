﻿using ChessLogic.Moves;
using ChessLogic.Pieces;

namespace ChessLogic;

public class GameState
{
    public Board Board { get; }
    public Player CurrentPlayer { get; private set; }

    public GameState(Player player, Board board)
    {
        CurrentPlayer = player;
        Board = board;
    }

    public IEnumerable<Move> LegalMovesForPieces(Position position)
    {
        if (Board.IsEmpty(position) || Board[position].Color != CurrentPlayer)
        {
            return Enumerable.Empty<Move>();
        }

        Piece piece = Board[position];
        return piece.GetMoves(position, Board);
    }

    public void MakeMove(Move move)
    {
        move.Execute(Board);
        CurrentPlayer = CurrentPlayer.Opponent();
    }
}