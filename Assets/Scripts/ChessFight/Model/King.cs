using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece {
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY) {
        List<Vector2Int> r = new List<Vector2Int>();
        
        // 8 directions: (1,0), (-1,0), (0,1), (0,-1), (1,1), (1,-1), (-1,1), (-1,-1)
        int[] dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
        int[] dy = { 0, 0, 1, -1, 1, -1, 1, -1 };
        
        for (int i = 0; i < 8; i++) {
            int x = currentX + dx[i];
            int y = currentY + dy[i];
            
            if (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY) {
                if (board[x, y] == null || board[x, y].team != team) {
                    r.Add(new Vector2Int(x, y));
                }
            }
        }

        // Castling
        if (!hasMoved) {
            // Right Rook
            ChessPiece rightRook = board[7, currentY];
            if (rightRook != null && rightRook.pieceType == ChessPieceEnum.Rook && !rightRook.hasMoved) {
                // Check if path is clear
                if (board[5, currentY] == null && board[6, currentY] == null) {
                    r.Add(new Vector2Int(6, currentY));
                }
            }
            // Left Rook
            ChessPiece leftRook = board[0, currentY];
            if (leftRook != null && leftRook.pieceType == ChessPieceEnum.Rook && !leftRook.hasMoved) {
                // Check if path is clear
                if (board[1, currentY] == null && board[2, currentY] == null && board[3, currentY] == null) {
                    r.Add(new Vector2Int(2, currentY));
                }
            }
        }
        
        return r;
    }
}