using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze_Handler : MonoBehaviour
{

    public GameObject cam;
    public Path_Handler path_handler;
    public Transform floor_object;
    public RectTransform canvas;
    public RectTransform pauseButton;
    public RectTransform reloadButton;
    [Range(0.0f, 0.5f)]public float CornerRoundness = 0.1f;
    [Range(1, 30)]public int RoundPoints = 6;
    [Range(0f, 0.8f)]public float ShadowDisplacement = 0.1f;
    [Range(0.2f, 0.5f)]public float CornerNormalRadius = 0.4f;
    [Range(0f, 1f)]public float MarginBetweenSquares = 0.1f;
    [Range(0f, 10f)]public float sideMargins = 1f;
    [Range(0f, 2f)]public float uiMargin = 2f;
    public Vector2 initialTile;
    public Vector2 finalTile;
    public Material upSquareColor;
    public Material downSquareColor;

    int[,] map;

    private DeviceOrientation devOrientation;

    int tiles_generated = 0;

    //Para comprobar si cambia el tamaño de la pantalla y redimensionar todo
    float xScreenSize;
    float yScreenSize;

    void Start()
    {
        xScreenSize = Screen.width;
        yScreenSize = Screen.height;

        GenerateMap();
        devOrientation = Input.deviceOrientation;
        SetCameraSize();
        SpawnMap(); 
    }
    
    void Update()
    {
        if(xScreenSize != Screen.width || yScreenSize != Screen.height){
            //Debug.Log("Tamaño recalculado");
            SetCameraSize();
            xScreenSize = Screen.width;
            yScreenSize = Screen.height;
        }
    }

    void GenerateMap(){
        //Provisional, en un futuro, generación lógica aleatoria.
        map = new int[,]{
            {1, 1, 1, 1, 0, 1},
            {0, 0, 0, 1, 0, 0},
            {0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 1},
            {0, 1, 0, 1, 0, 0},
            {0, 0, 0, 0, 0, 0},
            {0, 0, 0, 0, 0, 0},
            {1, 1, 0, 1, 1, 1}
        };

        initialTile = new Vector2(2,7);
        finalTile = new Vector2(4, 0);
        path_handler.map = (int[,])map.Clone();
        path_handler.initialTile = initialTile;
        path_handler.finalTile = finalTile;        
        path_handler.passedValues = true;
    }

    void SetCameraSize(){
        float screenWidth = map.GetLength(1) / (float)Screen.width;
        float screenHeight = map.GetLength(0) / (float)Screen.height;
        float camSize, xCenter, yCenter;
        bool addUIUpDisplacement = false;

        if(Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight){
            if(screenHeight > screenWidth){
                camSize = ((map.GetLength(0) * (CornerNormalRadius*2f + ShadowDisplacement)) + ((map.GetLength(0) - 1f) * MarginBetweenSquares) + sideMargins)/ 2f ;
            } else {
                camSize = ((map.GetLength(0) * (CornerNormalRadius*2f + ShadowDisplacement)) + ((map.GetLength(0) - 1f) * MarginBetweenSquares) + sideMargins)/ 2f ;
            }
        } else {
            if(screenHeight > screenWidth){
                camSize = ((map.GetLength(0) * (CornerNormalRadius*2f + ShadowDisplacement)) + ((map.GetLength(0) - 1f) * MarginBetweenSquares) + sideMargins)/ 2f ;       
            } else {
                camSize = ((map.GetLength(1) * (CornerNormalRadius*2f) + ((map.GetLength(1) - 1f) * MarginBetweenSquares) + sideMargins) / 2f) * ((float)Screen.height/(float)Screen.width);
            }
        }
        
        float uiHorizontalAspectRatio = (canvas.rect.width - (pauseButton.rect.width + reloadButton.rect.width + Mathf.Abs(pauseButton.rect.position.x)*2 + Mathf.Abs(reloadButton.rect.position.x)*2))/canvas.rect.width;
        float mapHorizontalAspectRatio = ((map.GetLength(1) * (CornerNormalRadius*2f) + ((map.GetLength(1) - 1f) * MarginBetweenSquares)) / 2f) / (camSize * ((float)Screen.width/(float)Screen.height));
        float uiVerticalAspectRatio = (canvas.rect.height - (pauseButton.rect.height + Mathf.Abs(pauseButton.rect.position.y)*2))/canvas.rect.height;
        float mapVerticalAspectRatio = ((map.GetLength(0) * (CornerNormalRadius*2f + ShadowDisplacement) + ((map.GetLength(0) - 1f) * MarginBetweenSquares)) / 2f) / camSize;

        if(uiHorizontalAspectRatio < (mapHorizontalAspectRatio + 0.01f) || uiVerticalAspectRatio < (mapVerticalAspectRatio + 0.01f)){
            if(Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight){
                if(uiHorizontalAspectRatio < (mapHorizontalAspectRatio + 0.01f)){
                    uiMargin = ((mapHorizontalAspectRatio + 0.01f) - uiHorizontalAspectRatio) * (camSize * ((float)Screen.width/(float)Screen.height));
                    camSize = camSize + uiMargin/2f;
                    addUIUpDisplacement = true;
                }
            } else {
                if(uiVerticalAspectRatio < (mapVerticalAspectRatio + 0.01f)){
                    uiMargin = ((mapVerticalAspectRatio + 0.01f) - uiVerticalAspectRatio) * camSize;
                    camSize = camSize + uiMargin/2f;
                    addUIUpDisplacement = true;
                }
            }
        } else {
            addUIUpDisplacement = false;
        }

        cam.GetComponent<Camera>().orthographicSize = camSize;

        xCenter = (map.GetLength(1) * (CornerNormalRadius*2f + MarginBetweenSquares) - MarginBetweenSquares)/2f - CornerNormalRadius;
        yCenter = (map.GetLength(0) * (CornerNormalRadius*2f + ShadowDisplacement + MarginBetweenSquares) - MarginBetweenSquares)/2f - CornerNormalRadius - ShadowDisplacement;
        if(addUIUpDisplacement){
            if(Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight){             
                xCenter = xCenter + uiMargin;
            } else {
                yCenter = yCenter + uiMargin;
            }
        }

        cam.transform.localPosition = new Vector3(xCenter, yCenter, -2);
    }

    void SpawnMap()
    {
        if(CornerRoundness > CornerNormalRadius){
            CornerRoundness = CornerNormalRadius;
        }

        for(int x = 0; x < map.GetLength(0); x++){
            for(int y = 0; y < map.GetLength(1); y++){
                if(map[x,y] == 0){
                    int xAxis = map.GetLength(0) - x - 1;
                    int yAxis = map.GetLength(1) - y - 1;
                    Vector3 squarePosition = new Vector3(y*(CornerNormalRadius*2f + MarginBetweenSquares), xAxis*(CornerNormalRadius*2f + ShadowDisplacement + MarginBetweenSquares), 0);
                    GenerateSquare(squarePosition, upSquareColor, downSquareColor, x, y);
                }
            }
        }

        //GenerateSquare(new Vector3(0f, 0f, 0f), upSquareColor, downSquareColor);
    }

    void GenerateSquare(Vector3 spawnPosition, Material color, Material shadow, int x, int y){
        //Instantiate Square
        GameObject square;
        square = new GameObject("Square [" + x + "][" + y + "]");
        for(int i = 0; i < 4; i++){
            GenerateCorner(spawnPosition, i, color, ref square);
        }
        square.transform.parent = floor_object;

        //Instantiate Shadow
        GameObject squareShadow;
        squareShadow = new GameObject("Shadow [" + x + "][" + y + "]");
        for(int i = 0; i < 4; i++){
            GenerateCorner(spawnPosition + new Vector3(0f, -ShadowDisplacement, 0.1f), i, shadow, ref squareShadow);
        }
        squareShadow.transform.parent = floor_object;
    }

    void GenerateCorner(Vector3 spawnPosition, int flips, Material color, ref GameObject parent){
        Mesh m = new Mesh();
        GameObject corner;
        corner = new GameObject("Corner " + ((tiles_generated % 4) + 1));
        corner.AddComponent<MeshFilter>();
        corner.AddComponent<MeshRenderer>().material = color;

        DrawCorner(spawnPosition, flips, ref m);

        tiles_generated++;
        
        corner.GetComponent<MeshFilter>().mesh = m;
        corner.transform.parent = parent.transform;
    }

    void DrawCorner(Vector3 spawnPosition, int flip_amount, ref Mesh m){
        List<Vector3> squareVertex = new List<Vector3>();
        List<int> squareTriangles = new List<int>();

        Vector3 newPoint = new Vector3();
        int fixed_flip = flip_amount % 4;

        float val = Mathf.PI / 180f;
        float x1 = 0.0f, y1 = 0.0f, x2 = 0.0f, y2 = 0.0f;
        float sumX1 = 0.0f, sumY1 = 0.0f, sumX2 = 0.0f, sumY2 = 0.0f;
        float auxSumX1, auxSumX2;
        int triangle_counter = 0;  

        //Primero generamos los triángulos que conformarán la esquina
        for(int i = 0; i < RoundPoints; i++){
            Vector3 circleCenter = new Vector3(CornerNormalRadius - CornerRoundness, CornerNormalRadius - CornerRoundness, 0.0f);
            sumX1 = CornerRoundness * Mathf.Cos((90f - (float)i*(90f/RoundPoints)) * val) + circleCenter.x;
            sumY1 = CornerRoundness * Mathf.Sin((90f - (float)i*(90f/RoundPoints)) * val)  + circleCenter.y;
            sumX2 = CornerRoundness * Mathf.Cos((90f - (float)(i+1)*(90f/RoundPoints)) * val)  + circleCenter.x;
            sumY2 = CornerRoundness * Mathf.Sin((90f - (float)(i+1)*(90f/RoundPoints)) * val)  + circleCenter.y;

            auxSumX1 = sumX1;
            auxSumX2 = sumX2;

            switch(fixed_flip){
            case 1:
                sumX1 = sumY1;
                sumY1 = -auxSumX1;
                sumX2 = sumY2;
                sumY2 = -auxSumX2;
            break;
            case 2:
                sumX1 = -sumX1;
                sumX2 = -sumX2;
                sumY1 = -sumY1;
                sumY2 = -sumY2;
            break;
            case 3:
                sumX1 = -sumY1;
                sumY1 = auxSumX1;
                sumX2 = -sumY2;
                sumY2 = auxSumX2;
            break;
            }

            x1 = spawnPosition.x + sumX1;
            y1 = spawnPosition.y + sumY1;
            x2 = spawnPosition.x + sumX2;
            y2 = spawnPosition.y + sumY2;
            newPoint = new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z);
            squareVertex.Add(newPoint);
            newPoint = new Vector3(x1, y1, spawnPosition.z);
            squareVertex.Add(newPoint);
            newPoint = new Vector3(x2, y2, spawnPosition.z);
            squareVertex.Add(newPoint);

            squareTriangles.Add(triangle_counter);
            triangle_counter++;
            squareTriangles.Add(triangle_counter);
            triangle_counter++;
            squareTriangles.Add(triangle_counter);
            triangle_counter++;

            m.Clear();
            m.vertices = squareVertex.ToArray();
            m.triangles = squareTriangles.ToArray();
        }

        //Posteriormente, los triángulos más grandes
        for(int i = 0; i < 2; i++){
            switch(i){
                case 0:
                    sumX1 = 0.0f;
                    sumY1 = CornerNormalRadius;
                    sumX2 = (CornerNormalRadius - CornerRoundness);
                    sumY2 = CornerNormalRadius;
                break;
                case 1:
                    sumX1 = CornerNormalRadius;
                    sumY1 = (CornerNormalRadius - CornerRoundness);
                    sumX2 = CornerNormalRadius;
                    sumY2 = 0.0f;
                break;
            }     

            auxSumX1 = sumX1;
            auxSumX2 = sumX2;

            switch(fixed_flip){
            case 1:
                sumX1 = sumY1;
                sumY1 = -auxSumX1;
                sumX2 = sumY2;
                sumY2 = -auxSumX2;
            break;
            case 2:
                sumX1 = -sumX1;
                sumX2 = -sumX2;
                sumY1 = -sumY1;
                sumY2 = -sumY2;
            break;
            case 3:
                sumX1 = -sumY1;
                sumY1 = auxSumX1;
                sumX2 = -sumY2;
                sumY2 = auxSumX2;
            break;
            }

            x1 = spawnPosition.x + sumX1;
            y1 = spawnPosition.y + sumY1;
            x2 = spawnPosition.x + sumX2;
            y2 = spawnPosition.y + sumY2;
            newPoint = new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z);
            squareVertex.Add(newPoint);
            newPoint = new Vector3(x1, y1, spawnPosition.z);
            squareVertex.Add(newPoint);
            newPoint = new Vector3(x2, y2, spawnPosition.z);
            squareVertex.Add(newPoint);

            squareTriangles.Add(triangle_counter);
            triangle_counter++;
            squareTriangles.Add(triangle_counter);
            triangle_counter++;
            squareTriangles.Add(triangle_counter);
            triangle_counter++;

            m.Clear();
            m.vertices = squareVertex.ToArray();
            m.triangles = squareTriangles.ToArray();

        }
    }

}
