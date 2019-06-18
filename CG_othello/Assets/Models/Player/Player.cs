using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Models.Board;
using UnityEngine;

namespace Models.Player
{
    public abstract class Player : MonoBehaviour
    {
        public abstract Tuple<int, int, PlayerColor> GetNextMove();
        public abstract List<Move> GetPotentialMoves();

        public abstract void SetNextMove(int x, int z);

        protected static List<Move> CalculatePotentialMoves(IReadOnlyList<LogicalPiece> state)
        {
            List<Move> result = new List<Move>();

            foreach (LogicalPiece alreadyPlacedPiece in state)
            {
                List<LogicalPiece> directions = GetOpposingAdjacentsOf(alreadyPlacedPiece, state);
                List<Move> moves = GetMovesFrom(alreadyPlacedPiece, directions, state);
                
                result.AddRange(moves);
            }

            return result;
        }

        private static List<LogicalPiece> GetOpposingAdjacentsOf(LogicalPiece existingPiece, IReadOnlyList<LogicalPiece> state)
        {
            IEnumerable<int> Range(int pos) => Enumerable.Range(pos - 1, pos); // you would think that this is one too little but its not
            LogicalPiece OpposingPieceAt(int x, int z) => new LogicalPiece(x, z, existingPiece.Color.Opposing());

            return Range(existingPiece.X)
                .SelectMany(row => 
                    Range(existingPiece.Z), (row, col) => OpposingPieceAt(row, col))
                .Where(state.Contains)
                .ToList();
        }

        private static List<Move> GetMovesFrom(LogicalPiece bound, List<LogicalPiece> directions, IReadOnlyList<LogicalPiece> state)
        {
            List<Move> moves = new List<Move>();
            
            foreach (LogicalPiece direction in directions)
            {
                var (possiblePieceForMove, flippedPieces) = 
                    GetPieceAtEndOfLineAndDistanceFrom(bound, direction, state);

                if (
                    !state.Contains(possiblePieceForMove) && 
                    InsideBounds(possiblePieceForMove) && 
                    flippedPieces > 0
                    ) moves.Add(new Move(possiblePieceForMove, bound, flippedPieces));
            }

            return moves;
        }

        private static Tuple<LogicalPiece, int> GetPieceAtEndOfLineAndDistanceFrom(
            LogicalPiece origin, 
            LogicalPiece direction, 
            IReadOnlyList<LogicalPiece> state)
        {
            int AdvanceOneStepFrom(int i) => i + i.CompareTo(0); // increment or decrement depending on the direction
            LogicalPiece NewLogicalPieceAt(int row, int column) => 
                new LogicalPiece(origin.X + row, origin.Z + column, origin.Color.Opposing());
            
            var (rowDirection, colDirection) = GetSlopeFrom(origin, direction);
            int flipped = 0;
            LogicalPiece next = NewLogicalPieceAt(rowDirection, colDirection);
            
            while (state.Contains(next))
            {
                rowDirection = AdvanceOneStepFrom(rowDirection);
                colDirection = AdvanceOneStepFrom(colDirection);
                next = NewLogicalPieceAt(rowDirection, colDirection);
                flipped++;
            }

            return new Tuple<LogicalPiece, int>(new LogicalPiece(next.X, next.Z, origin.Color), flipped);
        }

        private static Tuple<int, int> GetSlopeFrom(LogicalPiece origin, LogicalPiece direction)
        {
            // this works because we know that destination and adjacent will always be at most one step apart
            int GetDirectionFrom(int start, int end) => end.CompareTo(start);
            return new Tuple<int, int>(GetDirectionFrom(origin.X, direction.X), GetDirectionFrom(origin.Z, direction.Z));
        }

        private static bool InsideBounds(LogicalPiece piece)
        {
            bool InsideBounds(int pos) { return pos >= 0 && pos < Game.Instance.BoardLength; }
            
            return InsideBounds(piece.X) && InsideBounds(piece.Z);
        }
    }
}