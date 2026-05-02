using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece {
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY) {
        List<Vector2Int> r = new List<Vector2Int>();
        
        // 4 directions: (1,1), (1,-1), (-1,1), (-1,-1)
        int[] dx = { 1, 1, -1, -1 };
        int[] dy = { 1, -1, 1, -1 };
        
        for (int i = 0; i < 4; i++) {
            int x = currentX + dx[i];
            int y = currentY + dy[i];
            
            while (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY) {
                if (board[x, y] == null) {
                    r.Add(new Vector2Int(x, y));
                } else {
                    if (board[x, y].team != team) {
                        r.Add(new Vector2Int(x, y)); // Can kill
                    }
                    break; // Blocked by piece
                }
                x += dx[i];
                y += dy[i];
            }
        }
        
        return r;
    }
}
