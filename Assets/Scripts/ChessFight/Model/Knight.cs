using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece {
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY) {
        List<Vector2Int> r = new List<Vector2Int>();
        
        // L shapes: 2x, 1y and 1x, 2y
        int[] dx = { 1, 1, 2, 2, -1, -1, -2, -2 };
        int[] dy = { 2, -2, 1, -1, 2, -2, 1, -1 };
        
        for (int i = 0; i < 8; i++) {
            int x = currentX + dx[i];
            int y = currentY + dy[i];
            
            if (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY) {
                if (board[x, y] == null || board[x, y].team != team) {
                    r.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return r;
    }
}
