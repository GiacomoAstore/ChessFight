using UnityEngine;

public class ChessPiece : MonoBehaviour {
    
    public ChessPieceEnum pieceType;
    public int team;
    public int currentX;
    public int currentY;

    public Vector3 desiredPosition;
    public Vector3 desiredScale = Vector3.one;
    public bool hasMoved = false;

    private void Start() {
        desiredPosition = transform.localPosition;
        desiredScale = transform.localScale;
    }

    private void Update() {
        transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual System.Collections.Generic.List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY) {
        return new System.Collections.Generic.List<Vector2Int>();
    }


}
