using System;
using TreeEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChessBoard : MonoBehaviour {

    [Header("Art stuff")]
    [SerializeField] private Material trasparentMaterial; 
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;


    // LOGIC
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;


    private void Awake() {
        GenerateTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
    }
    private void Update() {
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
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitPosition.x, hitPosition.y].GetComponent<MeshRenderer>().material = hoverMaterial;
            }

            //if we are hovering a tile and the hit tile is different from the current one
            if (currentHover != hitPosition) {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = trasparentMaterial;
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                tiles[hitPosition.x, hitPosition.y].GetComponent<MeshRenderer>().material = hoverMaterial;
            }
        }
        else {
            if (currentHover != -Vector2Int.one) { 
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                tiles[currentHover.x, currentHover.y].GetComponent<MeshRenderer>().material = trasparentMaterial;
                currentHover = -Vector2Int.one;
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
}
