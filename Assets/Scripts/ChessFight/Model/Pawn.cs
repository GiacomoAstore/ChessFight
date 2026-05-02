using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece {
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY) {
        List<Vector2Int> r = new List<Vector2Int>();
        
        int direction = (team == 0) ? 1 : -1; // White moves up (y+1), Black moves down (y-1)
        
        // One in front
        if (currentY + direction >= 0 && currentY + direction < tileCountY) {
            if (board[currentX, currentY + direction] == null) {
                r.Add(new Vector2Int(currentX, currentY + direction));
                
                // Two in front
                if ((team == 0 && currentY == 1) || (team == 1 && currentY == 6)) {
                    if (board[currentX, currentY + (direction * 2)] == null) {
                        r.Add(new Vector2Int(currentX, currentY + (direction * 2)));
                    }
                }
            }
            
            // Kill move (Diagonal left)
            if (currentX != 0) {
                if (board[currentX - 1, currentY + direction] != null && board[currentX - 1, currentY + direction].team != team) {
                    r.Add(new Vector2Int(currentX - 1, currentY + direction));
                }
            }
            // Kill move (Diagonal right)
            if (currentX != tileCountX - 1) {
                if (board[currentX + 1, currentY + direction] != null && board[currentX + 1, currentY + direction].team != team) {
                    r.Add(new Vector2Int(currentX + 1, currentY + direction));
                }
            }
        }

        // En Passant
        if (ChessBoard.Instance != null && ChessBoard.Instance.lastMove[0] != -Vector2Int.one) {
            Vector2Int lastMoveFrom = ChessBoard.Instance.lastMove[0];
            Vector2Int lastMoveTo = ChessBoard.Instance.lastMove[1];
            
            // If the last move was an enemy pawn moving 2 squares
            if (Mathf.Abs(lastMoveFrom.y - lastMoveTo.y) == 2) {
                if (board[lastMoveTo.x, lastMoveTo.y] != null && board[lastMoveTo.x, lastMoveTo.y].pieceType == ChessPieceEnum.Pawn) {
                    // And it's exactly to the left or right of our pawn
                    if (lastMoveTo.y == currentY) {
                        if (lastMoveTo.x == currentX - 1) {
                            r.Add(new Vector2Int(currentX - 1, currentY + direction));
                        } else if (lastMoveTo.x == currentX + 1) {
                            r.Add(new Vector2Int(currentX + 1, currentY + direction));
                        }
                    }
                }
            }
        }
        
        return r;
    }
}