using System;
using TreeEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChessBoard : MonoBehaviour {

    [Header("Art stuff")]
    [SerializeField] private Material trasparentMaterial; 
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.65f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;

    public static ChessBoard Instance { set; get; }

    [Header("Prefabs and Materials")]
    [SerializeField] private GameObject[] prefabs;    
    [SerializeField] private Material[] teamMaterials;    


    // LOGIC
    private ChessPiece[,] chessPieces;   
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    
    // GAME STATE
    private ChessPiece currentlySelectedPiece;
    private System.Collections.Generic.List<Vector2Int> availableMoves = new System.Collections.Generic.List<Vector2Int>();
    private bool isWhiteTurn = true;
    
    // ADVANCED STATE
    public Vector2Int[] lastMove { set; get; } = new Vector2Int[2];
    private System.Collections.Generic.List<ChessPiece> deadWhites = new System.Collections.Generic.List<ChessPiece>();
    private System.Collections.Generic.List<ChessPiece> deadBlacks = new System.Collections.Generic.List<ChessPiece>();
    private int victoryTeam = -1;

    private void Awake() {
        Instance = this;
        lastMove[0] = -Vector2Int.one;
        lastMove[1] = -Vector2Int.one;
        GenerateTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnPieces();
        PositionPieces();
    }
    private void Update() {
        if (victoryTeam != -1) return; // GAME OVER

        if (!currentCamera) {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = currentCamera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0));

        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile","Hover"))) {

            //Get the tile index from the hit object
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            //if we are hovering a tile after not hovering one
            if (currentHover == -Vector2Int.one) {
                currentHover = hitPosition;
                if (!availableMoves.Contains(currentHover)) {
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                    tiles[hitPosition.x, hitPosition.y].GetComponent<MeshRenderer>().material = hoverMaterial;
                }
            }

            //if we are hovering a tile and the hit tile is different from the current one
            if (currentHover != hitPosition) {
                if (!availableMoves.Contains(currentHover)) {
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                    tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = trasparentMaterial;
                }
                
                currentHover = hitPosition;
                
                if (!availableMoves.Contains(currentHover)) {
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                    tiles[hitPosition.x, hitPosition.y].GetComponent<MeshRenderer>().material = hoverMaterial;
                }
            }

            // Click Logic
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                if (currentlySelectedPiece != null) {
                    if (availableMoves.Contains(currentHover)) {
                        MovePieceTo(currentHover.x, currentHover.y);
                    } else {
                        // Clicked somewhere else
                        ChessPiece cp = chessPieces[currentHover.x, currentHover.y];
                        if (cp != null && cp.team == (isWhiteTurn ? 0 : 1)) {
                            SelectPiece(currentHover.x, currentHover.y);
                        } else {
                            DeselectPiece();
                        }
                    }
                } else {
                    ChessPiece cp = chessPieces[currentHover.x, currentHover.y];
                    if (cp != null && cp.team == (isWhiteTurn ? 0 : 1)) {
                        SelectPiece(currentHover.x, currentHover.y);
                    }
                }
            }

        }
        else {
            if (currentHover != -Vector2Int.one) { 
                if (!availableMoves.Contains(currentHover)) {
                    tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                    tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = trasparentMaterial;
                }
                currentHover = -Vector2Int.one;
            }
            
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                DeselectPiece();
            }
        }
    }

    // Generates a grid of tiles
    private void GenerateTiles(float tilesSize, int tilesCountX, int tilesCountY) {

        yOffset += transform.position.y;
        bounds = new Vector3((tilesCountX / 2) * tileSize, 0, (tilesCountY / 2) * tileSize) + boardCenter;
        
        tiles = new GameObject[tilesCountX, tilesCountY];
        
        for (int x = 0; x < tilesCountX; x++) {
            for (int y = 0; y < tilesCountY; y++) {
                tiles[x, y] = GenerateSingleTile(tilesSize, x, y);
            }
        }
    }

    private GameObject GenerateSingleTile(float tilesSize, int x, int y) {
        GameObject tileObject = new GameObject($"Tile_{x}_{y}");
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = trasparentMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tilesSize, yOffset, y * tilesSize) - bounds;
        vertices[1] = new Vector3(x * tilesSize, yOffset, (y+1) * tilesSize) - bounds;
        vertices[2] = new Vector3((x+1) * tilesSize, yOffset, y * tilesSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tilesSize, yOffset, (y + 1) * tilesSize) - bounds;

        int[] triangles = new int[6] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;


        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>().size = new Vector3(tileSize, 0.1f,tileSize);


        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return tileObject;
    }

    // Spawing pieces
    private void SpawnPieces() {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        // Spawna i pezzi per entrambe le squadre
        SpawnTeamPieces(0, 1, 0); // Squadra bianca
        SpawnTeamPieces(7, 6, 1); // Squadra nera
    }

    private void SpawnTeamPieces(int mainRow, int pawnRow, int team) {
        // Spawna i pezzi principali (torri, cavalli, alfieri, re e regina)
        chessPieces[0, mainRow] = SpawnSinglePiece(ChessPieceEnum.Rook, team);
        chessPieces[1, mainRow] = SpawnSinglePiece(ChessPieceEnum.Knight, team);
        chessPieces[2, mainRow] = SpawnSinglePiece(ChessPieceEnum.Bishop, team);
        chessPieces[3, mainRow] = SpawnSinglePiece(ChessPieceEnum.Queen, team);
        chessPieces[4, mainRow] = SpawnSinglePiece(ChessPieceEnum.King, team);
        chessPieces[5, mainRow] = SpawnSinglePiece(ChessPieceEnum.Bishop, team);
        chessPieces[6, mainRow] = SpawnSinglePiece(ChessPieceEnum.Knight, team);
        chessPieces[7, mainRow] = SpawnSinglePiece(ChessPieceEnum.Rook, team);

        // Spawna i pedoni
        for (int i = 0; i < TILE_COUNT_X; i++) {
            chessPieces[i, pawnRow] = SpawnSinglePiece(ChessPieceEnum.Pawn, team);
        }
    }

    private ChessPiece SpawnSinglePiece(ChessPieceEnum pieceEnum ,int team) {
        ChessPiece cp = Instantiate(prefabs[(int)pieceEnum - 1], transform).GetComponent<ChessPiece>();

        cp.pieceType = pieceEnum;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return cp;
    }

    // Positioning pieces
    private void PositionPieces() {
        for (int x = 0; x < TILE_COUNT_X; x++) {
            for (int y = 0; y < TILE_COUNT_Y; y++) {
                if (chessPieces[x,y] != null) {
                    PositionSinglePiece(x, y, true);
                }
  
            }
        }
    }

    private void PositionSinglePiece(int x, int y, bool force = false) {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].desiredPosition = GetTilePosition(x, y);
        if (force) {
            chessPieces[x, y].transform.localPosition = chessPieces[x, y].desiredPosition;
        }
    }

    private Vector3 GetTilePosition(int x, int y) {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }
    
    // Game Flow & Logic
    private void SelectPiece(int x, int y) {
        DeselectPiece();
        
        currentlySelectedPiece = chessPieces[x, y];
        System.Collections.Generic.List<Vector2Int> pseudoMoves = currentlySelectedPiece.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
        
        availableMoves = new System.Collections.Generic.List<Vector2Int>();
        for (int i = 0; i < pseudoMoves.Count; i++) {
            Vector2Int move = pseudoMoves[i];
            
            // Regola ferrea: Il Re non può mai essere "mangiato" fisicamente.
            ChessPiece target = chessPieces[move.x, move.y];
            if (target != null && target.pieceType == ChessPieceEnum.King) {
                continue;
            }

            if (ContainsValidMove(ref chessPieces, currentlySelectedPiece, move.x, move.y)) {
                availableMoves.Add(move);
            }
        }
        
        HighlightTiles();
    }

    private void DeselectPiece() {
        RemoveHighlightTiles();
        currentlySelectedPiece = null;
        availableMoves.Clear();
    }

    private void MovePieceTo(int x, int y) {
        Vector2Int previousPosition = new Vector2Int(currentlySelectedPiece.currentX, currentlySelectedPiece.currentY);
        ChessPiece targetPiece = chessPieces[x, y];
        
        // En Passant Capture Logic
        if (currentlySelectedPiece.pieceType == ChessPieceEnum.Pawn) {
            if (previousPosition.x != x && targetPiece == null) {
                int enemyDirection = (currentlySelectedPiece.team == 0) ? -1 : 1;
                targetPiece = chessPieces[x, y + enemyDirection];
                chessPieces[x, y + enemyDirection] = null;
            }
        }

        // Handle combat if target tile is occupied by an enemy
        if (targetPiece != null && targetPiece.team != currentlySelectedPiece.team) {
            HandleCombat(currentlySelectedPiece, targetPiece);
        }

        // Update Matrix
        chessPieces[previousPosition.x, previousPosition.y] = null;
        currentlySelectedPiece.currentX = x;
        currentlySelectedPiece.currentY = y;
        currentlySelectedPiece.hasMoved = true;
        chessPieces[x, y] = currentlySelectedPiece;

        PositionSinglePiece(x, y);
        
        // Castling Movement Logic
        if (currentlySelectedPiece.pieceType == ChessPieceEnum.King) {
            if (Mathf.Abs(x - previousPosition.x) == 2) {
                int rookX = (x > previousPosition.x) ? 7 : 0;
                ChessPiece rook = chessPieces[rookX, y];
                if (rook != null) {
                    chessPieces[rookX, y] = null;
                    int newRookX = (x > previousPosition.x) ? x - 1 : x + 1;
                    rook.currentX = newRookX;
                    rook.currentY = y;
                    rook.hasMoved = true;
                    chessPieces[newRookX, y] = rook;
                    PositionSinglePiece(newRookX, y);
                }
            }
        }
        
        // Promotion Logic
        if (currentlySelectedPiece.pieceType == ChessPieceEnum.Pawn) {
            if ((currentlySelectedPiece.team == 0 && y == 7) || (currentlySelectedPiece.team == 1 && y == 0)) {
                currentlySelectedPiece.gameObject.SetActive(false);
                ChessPiece newQueen = SpawnSinglePiece(ChessPieceEnum.Queen, currentlySelectedPiece.team);
                chessPieces[x, y] = newQueen;
                newQueen.hasMoved = true;
                PositionSinglePiece(x, y, true);
                Destroy(currentlySelectedPiece.gameObject); 
                currentlySelectedPiece = newQueen;
            }
        }
        
        lastMove[0] = previousPosition;
        lastMove[1] = new Vector2Int(x, y);

        // Pass Turn
        isWhiteTurn = !isWhiteTurn;
        DeselectPiece();
        CheckForCheckmate();
    }

    private void HandleCombat(ChessPiece attacker, ChessPiece defender) {
        if (defender.team == 0) {
            deadWhites.Add(defender);
            defender.desiredScale = Vector3.one * 0.5f;
            defender.desiredPosition = GetTilePosition(-2, -1) + (Vector3.forward * (deadWhites.Count * tileSize / 2));
        } else {
            deadBlacks.Add(defender);
            defender.desiredScale = Vector3.one * 0.5f;
            defender.desiredPosition = GetTilePosition(9, 8) + (Vector3.back * (deadBlacks.Count * tileSize / 2));
        }
    }

    private void HighlightTiles() {
        for (int i = 0; i < availableMoves.Count; i++) {
            Vector2Int move = availableMoves[i];
            tiles[move.x, move.y].GetComponent<MeshRenderer>().material = hoverMaterial; // Using hover material for now to show valid moves
        }
    }

    private void RemoveHighlightTiles() {
        for (int i = 0; i < availableMoves.Count; i++) {
            Vector2Int move = availableMoves[i];
            tiles[move.x, move.y].GetComponent<MeshRenderer>().material = trasparentMaterial;
        }
    }


    // Operations
    private Vector2Int LookupTileIndex(GameObject hitInfo) {
        for (int x = 0; x < TILE_COUNT_X; x++) {
            for (int y = 0; y < TILE_COUNT_Y; y++) {
                if (tiles[x, y] == hitInfo) {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one; // INVALID
    }

    // Validations & Rules
    private bool ContainsValidMove(ref ChessPiece[,] board, ChessPiece piece, int targetX, int targetY) {
        int startX = piece.currentX;
        int startY = piece.currentY;
        ChessPiece targetPiece = board[targetX, targetY];

        // Simulate
        board[startX, startY] = null;
        piece.currentX = targetX;
        piece.currentY = targetY;
        board[targetX, targetY] = piece;

        bool isValid = !IsTeamInCheck(ref board, piece.team);

        // Undo
        board[targetX, targetY] = targetPiece;
        piece.currentX = startX;
        piece.currentY = startY;
        board[startX, startY] = piece;

        return isValid;
    }

    private bool IsTeamInCheck(ref ChessPiece[,] board, int team) {
        int kingX = -1, kingY = -1;
        for (int x = 0; x < TILE_COUNT_X; x++) {
            for (int y = 0; y < TILE_COUNT_Y; y++) {
                if (board[x, y] != null && board[x, y].team == team && board[x, y].pieceType == ChessPieceEnum.King) {
                    kingX = x;
                    kingY = y;
                    break;
                }
            }
        }

        if (kingX == -1) return false;

        for (int x = 0; x < TILE_COUNT_X; x++) {
            for (int y = 0; y < TILE_COUNT_Y; y++) {
                if (board[x, y] != null && board[x, y].team != team) {
                    System.Collections.Generic.List<Vector2Int> enemyMoves = board[x, y].GetAvailableMoves(ref board, TILE_COUNT_X, TILE_COUNT_Y);
                    for (int i = 0; i < enemyMoves.Count; i++) {
                        if (enemyMoves[i].x == kingX && enemyMoves[i].y == kingY) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private void CheckForCheckmate() {
        int currentTeam = isWhiteTurn ? 0 : 1;
        bool hasAnyValidMove = false;
        
        for (int x = 0; x < TILE_COUNT_X; x++) {
            for (int y = 0; y < TILE_COUNT_Y; y++) {
                ChessPiece piece = chessPieces[x, y];
                if (piece != null && piece.team == currentTeam) {
                    System.Collections.Generic.List<Vector2Int> moves = piece.GetAvailableMoves(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                    for (int i = 0; i < moves.Count; i++) {
                        if (ContainsValidMove(ref chessPieces, piece, moves[i].x, moves[i].y)) {
                            hasAnyValidMove = true;
                            break;
                        }
                    }
                }
                if (hasAnyValidMove) break;
            }
            if (hasAnyValidMove) break;
        }

        if (!hasAnyValidMove) {
            victoryTeam = isWhiteTurn ? 1 : 0;
        }
    }

    // UI
    private void OnGUI() {
        if (victoryTeam != -1) {
            GUI.color = Color.red;
            GUIStyle style = new GUIStyle();
            style.fontSize = 50;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.red;
            style.alignment = TextAnchor.MiddleCenter;
            string winner = victoryTeam == 0 ? "BIANCO" : "NERO";
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), $"SCACCO MATTO!\nVINCE IL {winner}!", style);
        }
    }
}
