using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MazeSystem : MonoBehaviour
{
    [Header("Maze")]
    public int[,] maze = new int[,]{
        {0, 0, 0, 0, 1, 0},
        {1, 1, 1, 0, 1, 1},
        {1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 0},
        {1, 0, 1, 0, 1, 1},
        {1, 1, 1, 1, 1, 1},
        {1, 1, 1, 1, 1, 1},
        {0, 0, 1, 0, 0, 0}
    };

    private int[,] mazeCopy;

    public bool addShades = false;
    [Range(0f, 0.5f)]
    public float shadePercent = 0.2f;
    [Range(0f, 0.15f)]
    public float spaceBetweenTiles = 0.01f;
    public Vector2 startPosition = new Vector2(2, 7);
    public Vector2 endPosition = new Vector2(4, 0);

    [Range(0f, 1f)]
    public float dashAnimationDuration = 0.05f;

    public int cp_Counter = 0;
    public int cp_GlobalSteps = 0;
    public int cp_Steps = 0;
    public Vector2 cp_Position;
    public Direction cp_Direction = Direction.Up;

    [Header("Scene Resources")]
    public UIHandler uiHandler;
    public GameObject tilePrefab;
    public GameObject framePrefab;
    public tileSprites sprites;
    public GameObject mazeBase;
    public GameObject mazeShade;
    public GameObject path;
    public GameObject end;
    public GameObject levelsButton;
    public GameObject configurationButton;
    public GameObject hintButton;
    [Space(5)]
    public MazeColors colors;
    public MazeColors darkmodeColors;
    [Space(10)]

    [Range(0f, 0.25f)]
    public float percentWidthMargin = 0.1f;
    [Range(0f, 0.25f)]
    public float percentHeightMargin = 0.1f;

    private const int MIN_WIDTH_MATRIX_SIZE = 5;
    private const int MIN_HEIGHT_MATRIX_SIZE = 9;

    private const float TILE_DEFAULT_SIZE = 976f;

    public bool movingPathState = false;
    public bool waitingForRemove = false;
    public bool destroyCurrentPath = false;

    public List<Direction> dirBuffer = new List<Direction>();

    [HideInInspector]
    public enum Direction{
        Up,
        Down,
        Left,
        Right
    }

    void Start()
    {
        //idk why but with 0.69 works fine
        percentHeightMargin = (0.69f)*percentWidthMargin;
        mazeCopy = maze.Clone() as int[,];

        //spawnFrame();
        spawnBaseMap();
        spawnPath();
        spawnEndIndicator();
        if(addShades){
            spawnBaseShade();
        } else {
            mazeShade.SetActive(false);
        }

        cp_Position = startPosition;
    }

    void Update()
    {
        if(!movingPathState && !destroyCurrentPath && dirBuffer.Count > 0){
            managePath(dirBuffer[0]);
            dirBuffer.RemoveAt(0);
        }
    }

    void spawnPathBlock(Vector2 position){
        //GameObject tile = new GameObject("PathShade".ToString(), typeof(RectTransform));
        //tile.transform.SetParent(path.transform);
        /*if(cp_Direction != Direction.Down && !lastPathBlockWasDown){
            tile.transform.SetAsFirstSibling(); //Lo pone en primera posición
        } else {
            if(cp_Direction == Direction.Down){
                lastPathBlockWasDown = true;
            } else {
                lastPathBlockWasDown = false;
            }
            tile.transform.SetAsLastSibling(); //Lo pone en última posición
        }
        tile.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
        tile.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
        tile.GetComponent<RectTransform>().localPosition = new Vector3 (0f, 0f, 0f);
        tile.GetComponent<RectTransform>().sizeDelta = new Vector3 (0f, 0f, 0f);
        tile.GetComponent<RectTransform>().localScale = new Vector3 (1f, 1f, 1f);*/

        if(maze[(int)position.y, (int)position.x] >= 1){
            GameObject baseTile = Instantiate(tilePrefab, path.transform.Find("PathBase"));
            baseTile.name = ("Base " + cp_Counter.ToString());
            baseTile.GetComponent<Image>().sprite = sprites.filledTile;
            baseTile.GetComponent<RectTransform>().anchorMin = getCenterPosition(position);
            baseTile.GetComponent<RectTransform>().anchorMax = getCenterPosition(position);
            baseTile.GetComponent<RectTransform>().sizeDelta = new Vector2(TILE_DEFAULT_SIZE, TILE_DEFAULT_SIZE);
            baseTile.transform.localScale = new Vector3(calculateTileSizeInPixels().x/TILE_DEFAULT_SIZE, calculateTileSizeInPixels().y/TILE_DEFAULT_SIZE, 1f);
            baseTile.GetComponent<Image>().color = getColorSet().pathColor;
            if(addShades){
                GameObject shadeTile = Instantiate(tilePrefab, path.transform.Find("PathShade"));
                shadeTile.name = ("Shade " + cp_Counter.ToString());
                shadeTile.GetComponent<Image>().sprite = sprites.filledTile;
                shadeTile.GetComponent<RectTransform>().anchorMin = new Vector2(getCenterPosition(position).x, getCenterPosition(position).y - calculateShadeSize());
                shadeTile.GetComponent<RectTransform>().anchorMax = new Vector2(getCenterPosition(position).x, getCenterPosition(position).y - calculateShadeSize());
                shadeTile.GetComponent<RectTransform>().sizeDelta = new Vector2(TILE_DEFAULT_SIZE, TILE_DEFAULT_SIZE);
                shadeTile.transform.localScale = new Vector3(calculateTileSizeInPixels().x/TILE_DEFAULT_SIZE, calculateTileSizeInPixels().y/TILE_DEFAULT_SIZE, 1f);
                shadeTile.GetComponent<Image>().color = getColorSet().pathShadeColor;
            }
        }
    }

    public void managePath(Direction dir){
        Vector2 nextPosition = getNextPosition(dir);

        if(nextPosition.x >= 0 && nextPosition.x < getMazeUnitWidth(maze) && nextPosition.y >= 0 && nextPosition.y < getMazeUnitHeight(maze)){
            if(maze[(int)nextPosition.y, (int)nextPosition.x] == 1 || maze[(int)nextPosition.y, (int)nextPosition.x] == cp_GlobalSteps + 1){
                
                if(!movingPathState){
                    movingPathState = true;
                    if(cp_GlobalSteps > 0){

                        if(changesDirection(cp_Direction, dir)){
                            cp_Counter++;
                            cp_GlobalSteps++;
                            cp_Direction = dir;
                            cp_Steps = 1;
                            spawnPathBlock(cp_Position);
                            cp_Position = nextPosition;
                            movePath(dir);
                            maze[(int)cp_Position.y, (int)cp_Position.x] = cp_GlobalSteps + 2;
                        } else {
                            if(cp_Direction == dir){
                                //cp_Direction = dir; //No se actualiza porque si al subir y bajar varias veces consecutivas perdería el ritmo  
                                cp_Position = nextPosition;
                                cp_GlobalSteps++;
                                cp_Steps++;
                                movePath(dir);
                                maze[(int)cp_Position.y, (int)cp_Position.x] = cp_GlobalSteps + 2;
                            } else {
                                maze[(int)cp_Position.y, (int)cp_Position.x] = 1;

                                cp_Position = nextPosition;
                                cp_GlobalSteps = cp_GlobalSteps - 1;
                                cp_Steps = cp_Steps - 1;
                                if(cp_Steps <= 0 && cp_GlobalSteps > 0){
                                    //Eliminar 
                                    destroyCurrentPath = true;
                                }
                                movePath(dir);
                                maze[(int)cp_Position.y, (int)cp_Position.x] = cp_GlobalSteps + 2;
                            }
                        }
                    } else {
                        //Si es el primer movimiento
                        cp_GlobalSteps++;
                        cp_Direction = dir;
                        cp_Steps++;
                        cp_Position = nextPosition;
                        movePath(dir);
                        maze[(int)cp_Position.y, (int)cp_Position.x] = cp_GlobalSteps + 2;
                    }
                } else {
                    dirBuffer.Add(dir);
                }
            } else {
                //Si en la matriz no hay ni un 1 ni un 2 en esa posición
                Debug.LogError("Intentando desplazarse a una posición inválida");
            }
        } else {
            Debug.LogError("Coordenada inválida");
            //Coordenada inválida
        }
    }

    void movePath(Direction dir){

        if(cp_Steps < 0){
            Debug.LogError("Error durante el cálculo de los caminos");
        }
        bool substract = isOppositeDirection(dir);
        RectTransform currentObject = path.transform.Find("PathBase").Find("Base " + cp_Counter).GetComponent<RectTransform>();

        if(currentObject.pivot == new Vector2(0.5f, 0.5f)){
            switch(dir){
                case Direction.Up:
                    currentObject.pivot = new Vector2(0.5f, 0f);
                    currentObject.localPosition = currentObject.localPosition - new Vector3(0f, (TILE_DEFAULT_SIZE*currentObject.localScale.y)/2f, 0f);
                    if(addShades) {
                            RectTransform shadeObject = path.transform.Find("PathShade").Find("Shade " + cp_Counter).GetComponent<RectTransform>(); 
                            //currentObject.localPosition = currentObject.localPosition - new Vector3(0f, (shadePercent*TILE_DEFAULT_SIZE*currentObject.localScale.y)/2f, 0f);
                            shadeObject.pivot = new Vector2(0.5f, 0f);
                            shadeObject.localPosition = shadeObject.localPosition - new Vector3(0f, (TILE_DEFAULT_SIZE*shadeObject.localScale.y)/2f, 0f);
                        
                        }
                break;
                case Direction.Down:
                    currentObject.pivot = new Vector2(0.5f, 1f);
                    currentObject.localPosition = currentObject.localPosition + new Vector3(0f, (TILE_DEFAULT_SIZE*currentObject.localScale.y)/2f, 0f);
                    if(addShades) {
                            RectTransform shadeObject = path.transform.Find("PathShade").Find("Shade " + cp_Counter).GetComponent<RectTransform>(); 
                            //currentObject.localPosition = currentObject.localPosition + new Vector3(0f, (shadePercent*TILE_DEFAULT_SIZE*currentObject.localScale.y)/2f, 0f);
                            shadeObject.pivot = new Vector2(0.5f, 1f);
                            shadeObject.localPosition = shadeObject.localPosition + new Vector3(0f, (TILE_DEFAULT_SIZE*shadeObject.localScale.y)/2f, 0f);
                        
                        }
                break;
                case Direction.Left:
                    currentObject.pivot = new Vector2(1f, 0.5f);
                    currentObject.localPosition = currentObject.localPosition + new Vector3((TILE_DEFAULT_SIZE*currentObject.localScale.y)/2f, 0f, 0f);
                    if(addShades) {
                            RectTransform shadeObject = path.transform.Find("PathShade").Find("Shade " + cp_Counter).GetComponent<RectTransform>(); 
                            shadeObject.pivot = new Vector2(1f, 0.5f);
                            shadeObject.localPosition = shadeObject.localPosition + new Vector3((TILE_DEFAULT_SIZE*shadeObject.localScale.y)/2f, 0f, 0f);
                        
                        }
                break;
                case Direction.Right:
                    currentObject.pivot = new Vector2(0f, 0.5f);
                    currentObject.localPosition = currentObject.localPosition - new Vector3((TILE_DEFAULT_SIZE*currentObject.localScale.y)/2f, 0f, 0f);
                    if(addShades) {
                            RectTransform shadeObject = path.transform.Find("PathShade").Find("Shade " + cp_Counter).GetComponent<RectTransform>(); 
                            shadeObject.pivot = new Vector2(0f, 0.5f);
                            shadeObject.localPosition = shadeObject.localPosition - new Vector3((TILE_DEFAULT_SIZE*shadeObject.localScale.y)/2f, 0f, 0f);
                        
                        }
                break;
                default: 
                break;
            }
        }
    
        if(dir == Direction.Up || dir == Direction.Down){
            float displacement;
            if(substract){
                displacement = -getHeightBetweenSlabsCenters()/currentObject.localScale.y;
            } else {
                displacement = getHeightBetweenSlabsCenters()/currentObject.localScale.y;
            }
            if(path.transform.Find("PathBase").Find("Base " + cp_Counter) != null){
                if(addShades){
                    DOTween.To(()=> path.transform.Find("PathShade").Find("Shade " + cp_Counter).GetComponent<RectTransform>().sizeDelta, x=> path.transform.Find("PathShade").Find("Shade " + cp_Counter).GetComponent<RectTransform>().sizeDelta = x, new Vector2(currentObject.sizeDelta.x, currentObject.sizeDelta.y + displacement), dashAnimationDuration).SetEase(Ease.OutQuint);
                }
                currentObject = path.transform.Find("PathBase").Find("Base " + cp_Counter).GetComponent<RectTransform>();
                DOTween.To(()=> path.transform.Find("PathBase").Find("Base " + cp_Counter).GetComponent<RectTransform>().sizeDelta, x=> path.transform.Find("PathBase").Find("Base " + cp_Counter).GetComponent<RectTransform>().sizeDelta = x, new Vector2(currentObject.sizeDelta.x, currentObject.sizeDelta.y + displacement), dashAnimationDuration).SetEase(Ease.OutQuint).OnComplete(() => { 
                    if(destroyCurrentPath){
                        destroyThisPath();
                    }
                    movingPathState = false;
                });
            }
        } else if (dir == Direction.Left || dir == Direction.Right){
            float displacement;
            if(substract){
                displacement = -getWidthBetweenSlabsCenters()/currentObject.localScale.x;
            } else {
                displacement = getWidthBetweenSlabsCenters()/currentObject.localScale.x;
            }
            if(path.transform.Find("PathBase").Find("Base " + cp_Counter) != null){
                //Debug.Log("Intentando mover a: PathBase > Base " + cp_Counter.ToString() + "\nExiste: " + (currentObject != null).ToString());
                if(addShades){
                    DOTween.To(()=> path.transform.Find("PathShade").Find("Shade " + cp_Counter).GetComponent<RectTransform>().sizeDelta, x=> path.transform.Find("PathShade").Find("Shade " + cp_Counter).GetComponent<RectTransform>().sizeDelta = x, new Vector2(currentObject.sizeDelta.x + displacement, currentObject.sizeDelta.y), dashAnimationDuration).SetEase(Ease.OutQuint);
                }
                currentObject = path.transform.Find("PathBase").Find("Base " + cp_Counter).GetComponent<RectTransform>();
                DOTween.To(()=> path.transform.Find("PathBase").Find("Base " + cp_Counter).GetComponent<RectTransform>().sizeDelta, x=> path.transform.Find("PathBase").Find("Base " + cp_Counter).GetComponent<RectTransform>().sizeDelta = x, new Vector2(currentObject.sizeDelta.x + displacement, currentObject.sizeDelta.y), dashAnimationDuration).SetEase(Ease.OutQuint).OnComplete(() => { 
                    if(destroyCurrentPath){
                        destroyThisPath();
                    }
                    movingPathState = false;
                });
            }
        }
    }

    void spawnBaseMap(){
        maze = recalculateMaze(maze);

        for(int y = 0; y < getMazeUnitHeight(maze); y++){
            for(int x = 0; x < getMazeUnitWidth(maze); x++){
                //if(maze[y, x] == 1 && new Vector2(x, y) != endPosition && new Vector2(x, y) != startPosition){
                if(maze[y, x] == 1){
                    if(new Vector2(x, y) != endPosition || uiHandler.darkmode){
                        spawnBaseTile(new Vector3(x, y, -2f), getColorSet().baseColor);
                    }
                }
            }
        }
    }

    void spawnBaseShade(){
        mazeShade.SetActive(true);

        maze = recalculateMaze(maze);

        for(int y = 0; y < getMazeUnitHeight(maze); y++){
            for(int x = 0; x < getMazeUnitWidth(maze); x++){
                //if(maze[y, x] == 1 && new Vector2(x, y) != endPosition && new Vector2(x, y) != startPosition){
                if(maze[y, x] == 1){
                    spawnShadeTile(new Vector3(x, y, -2.01f), getColorSet().shadeColor);
                }
            }
        }
    }

    void spawnPath(){
        /*GameObject tile = new GameObject("Path_0", typeof(RectTransform));
        tile.transform.SetParent(path.transform);
        tile.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
        tile.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
        tile.GetComponent<RectTransform>().localPosition = new Vector3 (0f, 0f, 0f);
        tile.GetComponent<RectTransform>().sizeDelta = new Vector3 (0f, 0f, 0f);
        tile.GetComponent<RectTransform>().localScale = new Vector3 (1f, 1f, 1f);*/

        if(maze[(int)startPosition.y, (int)startPosition.x] == 1){
            if(addShades){
                GameObject shadeTile = Instantiate(tilePrefab, path.transform.Find("PathShade"));
                shadeTile.GetComponent<Image>().sprite = sprites.filledTile;
                shadeTile.GetComponent<RectTransform>().anchorMin = new Vector2(getCenterPosition(startPosition).x, getCenterPosition(startPosition).y - calculateShadeSize());
                shadeTile.GetComponent<RectTransform>().anchorMax = new Vector2(getCenterPosition(startPosition).x, getCenterPosition(startPosition).y - calculateShadeSize());
                shadeTile.GetComponent<RectTransform>().sizeDelta = new Vector2(TILE_DEFAULT_SIZE, TILE_DEFAULT_SIZE);
                shadeTile.transform.localScale = new Vector3(calculateTileSizeInPixels().x/TILE_DEFAULT_SIZE, calculateTileSizeInPixels().y/TILE_DEFAULT_SIZE, 1f);
                shadeTile.GetComponent<Image>().color = getColorSet().pathShadeColor;
                shadeTile.name = ("Shade 0");
            }
            GameObject baseTile = Instantiate(tilePrefab, path.transform.Find("PathBase"));
            baseTile.GetComponent<Image>().sprite = sprites.filledTile;
            baseTile.GetComponent<RectTransform>().anchorMin = getCenterPosition(startPosition);
            baseTile.GetComponent<RectTransform>().anchorMax = getCenterPosition(startPosition);
            baseTile.GetComponent<RectTransform>().sizeDelta = new Vector2(TILE_DEFAULT_SIZE, TILE_DEFAULT_SIZE);
            baseTile.transform.localScale = new Vector3(calculateTileSizeInPixels().x/TILE_DEFAULT_SIZE, calculateTileSizeInPixels().y/TILE_DEFAULT_SIZE, 1f);
            baseTile.GetComponent<Image>().color = getColorSet().pathColor;
            baseTile.name = ("Base 0");
        }
    }

    void spawnEndIndicator(){
        float minimizeFactor = 0.6f;

        GameObject tile = new GameObject("End_Object", typeof(RectTransform));
        tile.transform.SetParent(end.transform);
        tile.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0f);
        tile.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
        tile.GetComponent<RectTransform>().localPosition = new Vector3 (0f, 0f, 0f);
        tile.GetComponent<RectTransform>().sizeDelta = new Vector3 (0f, 0f, 0f);
        tile.GetComponent<RectTransform>().localScale = new Vector3 (1f, 1f, 1f);
        
        if(maze[(int)endPosition.y, (int)endPosition.x] == 1){
            /*if(addShades){
                GameObject shadeTile = Instantiate(tilePrefab, tile.transform);
                shadeTile.GetComponent<Image>().sprite = sprites.goalTile;
                shadeTile.GetComponent<RectTransform>().anchorMin = new Vector2(getCenterPosition(endPosition).x, getCenterPosition(endPosition).y - calculateShadeSize());
                shadeTile.GetComponent<RectTransform>().anchorMax = new Vector2(getCenterPosition(endPosition).x, getCenterPosition(endPosition).y - calculateShadeSize());
                shadeTile.GetComponent<RectTransform>().sizeDelta = new Vector2(TILE_DEFAULT_SIZE, TILE_DEFAULT_SIZE);
                shadeTile.transform.localScale = new Vector3(calculateTileSizeInPixels().x/TILE_DEFAULT_SIZE, calculateTileSizeInPixels().y/TILE_DEFAULT_SIZE, 1f);
                shadeTile.GetComponent<Image>().color = getColorSet().endShadeColor;
                shadeTile.name = ("End Shade");
            }*/
            GameObject baseTile = Instantiate(tilePrefab, tile.transform);
            baseTile.GetComponent<Image>().sprite = sprites.goalTile;
            baseTile.GetComponent<RectTransform>().anchorMin = getCenterPosition(endPosition);
            baseTile.GetComponent<RectTransform>().anchorMax = getCenterPosition(endPosition);
            baseTile.GetComponent<RectTransform>().sizeDelta = new Vector2(TILE_DEFAULT_SIZE, TILE_DEFAULT_SIZE);
            if(uiHandler.darkmode){
                baseTile.transform.localScale = new Vector3((minimizeFactor*calculateTileSizeInPixels().x)/TILE_DEFAULT_SIZE, (minimizeFactor*calculateTileSizeInPixels().y)/TILE_DEFAULT_SIZE, 1f);
            } else {
                baseTile.transform.localScale = new Vector3((calculateTileSizeInPixels().x)/TILE_DEFAULT_SIZE, (calculateTileSizeInPixels().y)/TILE_DEFAULT_SIZE, 1f);
            }
            baseTile.GetComponent<Image>().color = getColorSet().endColor;
            baseTile.name = ("End Base");
        }
    }

    void spawnFrame(){
        GameObject frame = framePrefab;
        frame.GetComponent<RectTransform>().anchorMin = new Vector2(getZeroZeroAvailablePosition().y, getZeroZeroAvailablePosition().x - calculateTrueCanvasHeight()/getCanvasHeight());
        frame.GetComponent<RectTransform>().anchorMax = new Vector2(getZeroZeroAvailablePosition().y + calculateTrueCanvasWidth()/getCanvasWidth(), getZeroZeroAvailablePosition().x);
        frame.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        GameObject tileInstantiated = Instantiate(frame, mazeBase.transform);
        tileInstantiated.name = ("FRAME");
    }

    /*
    ====================
    FUNCIONES AUXILIARES
    ====================
    */

    
    int[,] recalculateMaze(int[,] matrix){
        int tileDifference;
        int[,] recalcMatrix = matrix;

        if(fitToWidth(matrix)){
            if(getMazeUnitWidth(matrix) < MIN_WIDTH_MATRIX_SIZE){
                tileDifference = MIN_WIDTH_MATRIX_SIZE - getMazeUnitWidth(matrix);
                
                if(tileDifference % 2 != 0){
                    tileDifference++;
                }

                for(int i = 0; i < tileDifference; i++){
                    if(i < (tileDifference/2)){
                        recalcMatrix = addColumnToMatrix(recalcMatrix, 0, true);
                    } else {
                        recalcMatrix = addColumnToMatrix(recalcMatrix, 0, false);
                    }
                }
            }
        } else {
            if(getMazeUnitHeight(matrix) < MIN_HEIGHT_MATRIX_SIZE){
                tileDifference = MIN_HEIGHT_MATRIX_SIZE - getMazeUnitHeight(matrix);
                
                if(tileDifference % 2 != 0){
                    tileDifference++;
                }

                for(int i = 0; i < tileDifference; i++){
                    if(i < (tileDifference/2)){
                        recalcMatrix = addRowToMatrix(recalcMatrix, 0, true);
                    } else {
                        recalcMatrix = addRowToMatrix(recalcMatrix, 0, false);
                    }
                }
            }
        }

        return recalcMatrix;
    }

    //Añade una fila al principio o al final de la matriz pasada como parámetro 
    int[,] addRowToMatrix(int[,] matrix, int value, bool addOnTop){
        
        int[,] newMatrix = new int[getMazeUnitHeight(matrix) + 1, getMazeUnitWidth(matrix)];

        for(int y = 0; y < getMazeUnitHeight(newMatrix); y++){
            for(int x = 0; x < getMazeUnitWidth(newMatrix); x++){
                if(addOnTop){
                    if(y == 0){
                        newMatrix[y,x] = value;
                    } else {
                        newMatrix[y,x] = matrix[y-1,x];
                    }
                } else {
                    if(y < getMazeUnitHeight(matrix)){
                        newMatrix[y,x] = matrix[y,x];
                    } else {
                        newMatrix[y,x] = value;
                    }
                }
            }
        }

        return newMatrix;
    }   

    //Añade una columna al principio o al final de la matriz pasada como parámetro 
    int[,] addColumnToMatrix(int[,] matrix, int value, bool addOnLeft){

        int[,] newMatrix = new int[getMazeUnitHeight(matrix), getMazeUnitWidth(matrix) + 1];

        for(int y = 0; y < getMazeUnitHeight(newMatrix); y++){
            for(int x = 0; x < getMazeUnitWidth(newMatrix); x++){
                if(addOnLeft){
                    if(x == 0){
                        newMatrix[y,x] = value;
                    } else {
                        newMatrix[y,x] = matrix[y,x-1];
                    }
                } else {
                    if(x < getMazeUnitWidth(matrix)){
                        newMatrix[y,x] = matrix[y,x];
                    } else {
                        newMatrix[y,x] = value;
                    }
                }
            }
        }

        return newMatrix;
    }

    //Calcula si hay una cantidad par de columnas (eje horizontal)
    bool evenHorizontalValue(int[,] matrix){
        if((getMazeUnitWidth(matrix)%2) == 0){
            return true;
        } else {
            return false;
        }
    }

    //Calcula si hay una cantidad par de filas (eje vertical)
    bool evenVerticalValue(int[,] matrix){
        if((getMazeUnitHeight(matrix)%2) == 0){
            return true;
        } else {
            return false;
        }
    }

    //Función para saber si el laberinto se ajustará al ancho o al alto
    bool fitToWidth(int[,] matrix){
        float mazeAspectRatio = (float)getMazeUnitHeight(matrix)/(float)getMazeUnitWidth(matrix);
        float availableScreenSpaceAspectRatio = calculateTrueCanvasHeight()/calculateTrueCanvasWidth();

        if(mazeAspectRatio > availableScreenSpaceAspectRatio){
            return false;
        } else {
            return true;
        }
    }

    Vector2 getNextPosition(Direction dir){
        switch(dir){
            case Direction.Up:
                return new Vector2(cp_Position.x, cp_Position.y - 1);
            case Direction.Down:
                return new Vector2(cp_Position.x, cp_Position.y + 1);
            case Direction.Left:
                return new Vector2(cp_Position.x - 1, cp_Position.y);
            case Direction.Right:
                return new Vector2(cp_Position.x + 1, cp_Position.y);
            default:
                return new Vector2(cp_Position.x, cp_Position.y);
        }
    }

    void updateCP(){
        if(cp_GlobalSteps > 0){
            cp_Direction = getCurrentDirection();
            cp_Steps = getStepsByCP();
            //cp_Position = getCurrentPositionByCP();
        } else {
            cp_Direction = Direction.Up;
            cp_Steps = 0;
            //cp_Position = startPosition;
        }
    }

    int getStepsByCP(){
        RectTransform currentPathBlock = path.transform.Find("PathBase").Find("Base " + cp_Counter).GetComponent<RectTransform>();
        int steps = 0;

        if(cp_Direction == Direction.Left || cp_Direction == Direction.Right){
            steps = Mathf.Abs(Mathf.RoundToInt((currentPathBlock.sizeDelta.x - TILE_DEFAULT_SIZE)/(getWidthBetweenSlabsCenters()/currentPathBlock.localScale.x)));
        } else if(cp_Direction == Direction.Up || cp_Direction == Direction.Down){
            steps = Mathf.Abs(Mathf.RoundToInt((currentPathBlock.sizeDelta.y - (TILE_DEFAULT_SIZE+(TILE_DEFAULT_SIZE*shadePercent)))/(getHeightBetweenSlabsCenters()/currentPathBlock.localScale.y)));
        }

        return steps;
    }

    Direction getCurrentDirection(){
        RectTransform currentPathBlock = path.transform.Find("PathBase").Find("Base " + cp_Counter).GetComponent<RectTransform>();
        if(currentPathBlock.pivot.x == 1){
            return Direction.Left;
        } else if(currentPathBlock.pivot.x == 0){
            return Direction.Right;
        } else if(currentPathBlock.pivot.y == 1){
            return Direction.Down;
        } else if(currentPathBlock.pivot.y == 0){
            return Direction.Up;
        }

        //Default
        return Direction.Up;
    }

    bool isOppositeDirection(Direction dir){
        switch(cp_Direction){
            case Direction.Up:
                if(dir == Direction.Down){
                    return true;
                }
            break;
            case Direction.Down:
                if(dir == Direction.Up){
                    return true;
                }
            break;
            case Direction.Left:
                if(dir == Direction.Right){
                    return true;
                }
            break;
            case Direction.Right:
                if(dir == Direction.Left){
                    return true;
                }
            break;
        }

        return false;     
    }

    Vector2 getPreviousPositionByCP(){
        if(maze[(int)cp_Position.y - 1, (int)cp_Position.x] == (cp_GlobalSteps - 1)){
            return new Vector2(cp_Position.x, cp_Position.y - 1);
        } else if(maze[(int)cp_Position.y + 1, (int)cp_Position.x] == (cp_GlobalSteps - 1)){
            return new Vector2(cp_Position.x, cp_Position.y + 1);
        } else if(maze[(int)cp_Position.y, (int)cp_Position.x - 1] == (cp_GlobalSteps - 1)){
            return new Vector2(cp_Position.x - 1, cp_Position.y);
        } else if(maze[(int)cp_Position.y - 1, (int)cp_Position.x + 1] == (cp_GlobalSteps - 1)){
            return new Vector2(cp_Position.x + 1, cp_Position.y);
        }

        return new Vector2(cp_Position.x, cp_Position.y);
    }

    Vector2 getCurrentPositionByCP(){
        for(int y = 0; y < getMazeUnitHeight(maze); y++){
            for(int x = 0; x < getMazeUnitWidth(maze); x++){
                if(maze[y, x] == cp_GlobalSteps + 2){
                    return new Vector2(x, y);
                }
            }
        }

        return startPosition;
    }

    bool changesDirection(Direction dir1, Direction dir2){
        bool dir1IsUpOrDown = true, dir2IsUpOrDown = true;

        if(dir1 == Direction.Left || dir1 == Direction.Right){
            dir1IsUpOrDown = false;
        }
        if(dir2 == Direction.Left || dir2 == Direction.Right){
            dir2IsUpOrDown = false;
        }

        return (dir1IsUpOrDown != dir2IsUpOrDown);
    }

    float getWidthBetweenSlabsCenters(){
        Vector2 zero = getCenterPosition(new Vector2(0, 0));
        Vector2 down = getCenterPosition(new Vector2(1, 0));
        return (down.x - zero.x)*getCanvasWidth();
    }

    float getHeightBetweenSlabsCenters(){
        Vector2 zero = getCenterPosition(new Vector2(0, 0));
        Vector2 right = getCenterPosition(new Vector2(0, 1));
        return (zero.y - right.y)*getCanvasHeight();
    }

    void spawnBaseTile(Vector3 pos, Color tileColor){
        GameObject tile = Instantiate(tilePrefab, mazeBase.transform);
        if(uiHandler.darkmode){
            tile.GetComponent<Image>().sprite = sprites.filledTile;
        } else {
            tile.GetComponent<Image>().sprite = sprites.voidTile;
        }
        tile.GetComponent<RectTransform>().anchorMin = getCenterPosition(pos);
        tile.GetComponent<RectTransform>().anchorMax = getCenterPosition(pos);
        tile.GetComponent<RectTransform>().sizeDelta = new Vector2(TILE_DEFAULT_SIZE, TILE_DEFAULT_SIZE);
        tile.transform.localScale = new Vector3(calculateTileSizeInPixels().x/TILE_DEFAULT_SIZE, calculateTileSizeInPixels().y/TILE_DEFAULT_SIZE, 1f);
        tile.GetComponent<Image>().color = tileColor;
        tile.name = ("Tile [" + pos.x.ToString() + "][" + pos.y.ToString() + "]");
    }

    void spawnShadeTile(Vector3 pos, Color tileColor){
        GameObject tile = Instantiate(tilePrefab, mazeShade.transform);
        tile.GetComponent<Image>().sprite = sprites.filledTile;
        tile.GetComponent<RectTransform>().anchorMin = new Vector2(getCenterPosition(pos).x, getCenterPosition(pos).y - calculateShadeSize());
        tile.GetComponent<RectTransform>().anchorMax = new Vector2(getCenterPosition(pos).x, getCenterPosition(pos).y - calculateShadeSize());
        tile.GetComponent<RectTransform>().sizeDelta = new Vector2(TILE_DEFAULT_SIZE, TILE_DEFAULT_SIZE);
        tile.transform.localScale = new Vector3(calculateTileSizeInPixels().x/TILE_DEFAULT_SIZE, calculateTileSizeInPixels().y/TILE_DEFAULT_SIZE, 1f);
        tile.GetComponent<Image>().color = tileColor;
        tile.name = ("Shade [" + pos.x.ToString() + "][" + pos.y.ToString() + "]");
    }

    float getScaleFactor(){
        if(fitToWidth(maze)){
            return calculateTileSize().x/calculateBaseTileSize().x;
        } else {
            return calculateTileSize().y/calculateBaseTileSize().y;
        }
    }

    Vector2 getCenterPosition(Vector2 pos){
        return calculatePercentPosition(pos, true);
        
    }

    Vector2 getMinPosition(Vector2 pos){
        Vector2 minPos = calculatePercentPosition(pos, true);
        minPos = new Vector2(minPos.x - calculateBaseTileSize().x/2f, minPos.y - calculateBaseTileSize().y/2f);

        return minPos;
    }

    Vector2 getMaxPosition(Vector2 pos){
        Vector2 maxPos = calculatePercentPosition(pos, true);
        maxPos = new Vector2(maxPos.x + calculateBaseTileSize().x/2f, maxPos.y + calculateBaseTileSize().y/2f);

        return maxPos;
    }

    void destroyThisPath(){
        DestroyImmediate(path.transform.Find("PathBase").Find("Base " + cp_Counter).gameObject);
        if(addShades){
            DestroyImmediate(path.transform.Find("PathShade").Find("Shade " + cp_Counter).gameObject);
        }
        cp_Counter = cp_Counter - 1;
        updateCP();
        destroyCurrentPath = false;
    }

    //Calcula la posición en porcentaje de una coordenada
    Vector2 calculatePercentPosition(Vector2 pos, bool checkIfValid){

        float horizontalSpacing, verticalSpacing, shadeSpacing;
        Vector2 positionPadding = getPositionPadding();

        if(addShades){
            shadeSpacing = calculateShadeSize();
        } else {
            shadeSpacing = 0f;
        }

        horizontalSpacing = spaceBetweenTiles;
        verticalSpacing = (calculateTrueCanvasWidth()/calculateTrueCanvasHeight()) * horizontalSpacing;

        Vector2 calculatedPosition = new Vector2(getZeroZeroAvailablePosition().y, getZeroZeroAvailablePosition().x);
        Vector2 basePos = new Vector2(getZeroZeroAvailablePosition().y, getZeroZeroAvailablePosition().x);

        if(checkIfValid){
            if(pos.x >= 0 && (int)pos.x < getMazeUnitWidth(maze)){
                if(pos.y >= 0 && (int)pos.y < getMazeUnitHeight(maze)) {
                    calculatedPosition = new Vector2((basePos.x + ((calculateTileSize().x + horizontalSpacing)*(float)pos.x) + calculateTileSize().x/2f) + positionPadding.x, (basePos.y - ((calculateTileSize().y+verticalSpacing+shadeSpacing)*(float)pos.y) - (calculateTileSize().y+shadeSpacing)/2f) - positionPadding.y);
                } else {
                    Debug.LogError("Error intentando calcular la posición de una baldosa\nPosición Y fuera de rango (" + pos.y.ToString() + ").");
                }
            } else {
                Debug.LogError("Error intentando calcular la posición de una baldosa\nPosición X fuera de rango (" + pos.x.ToString() + ").");
            }
        } else {
            calculatedPosition = new Vector2((basePos.x + ((calculateTileSize().x + horizontalSpacing)*(float)pos.x) + calculateTileSize().x/2f) + positionPadding.x, (basePos.y - ((calculateTileSize().y+verticalSpacing+shadeSpacing)*(float)pos.y) - (calculateTileSize().y+shadeSpacing)/2f) - positionPadding.y);
        }

        return calculatedPosition;
    }

    //Calcula el espacio porcentual de las baldosas
    Vector2 calculateTileSize(){
        float width, height;
        
        if(fitToWidth(maze)){
            width = (returnPercentOfAvailableScreenSpace().y - (((float)getMazeUnitWidth(maze) - 1f)*spaceBetweenTiles))/(float)getMazeUnitWidth(maze);
            height = (calculateTrueCanvasWidth()/calculateTrueCanvasHeight()) * width;
        } else {
            height = (returnPercentOfAvailableScreenSpace().x - (((float)getMazeUnitHeight(maze) - 1f)*((calculateTrueCanvasWidth()/calculateTrueCanvasHeight()) * spaceBetweenTiles)))/(float)getMazeUnitHeight(maze);
            width = (calculateTrueCanvasHeight()/calculateTrueCanvasWidth()) * height;
        }

        return new Vector2(width, height);
    }

    /*Vector2 calculateTileSizeInPixels(){
        Vector2 percentualSize = calculateTileSize();
        if(fitToWidth(maze)){
            return new Vector2(percentualSize.x*calculateTrueCanvasWidth(), percentualSize.y*calculateTrueCanvasHeight());
        } else {
            return new Vector2(percentualSize.x*calculateTrueCanvasHeight(), percentualSize.y*calculateTrueCanvasWidth());
        }
    }*/

    Vector2 calculateTileSizeInPixels(){
        Vector2 percentualSize = new Vector2(calculateTrueCanvasWidth()*(1f/((float)getMazeUnitWidth(maze)+(((float)getMazeUnitWidth(maze)-1f)*spaceBetweenTiles))), calculateTrueCanvasHeight()*(1f/((float)getMazeUnitHeight(maze)+(((float)getMazeUnitHeight(maze)-1f)*spaceBetweenTiles))));
        if(fitToWidth(maze)){
            return new Vector2(percentualSize.x, percentualSize.x);
        } else {
            return new Vector2(percentualSize.y, percentualSize.y);
        }
    }

    float calculateShadeSize(){
        return (calculateTileSizeInPixels().y/getCanvasHeight())*shadePercent;
    }

    Vector2 calculateBaseTileSize(){
        float width, height;
        
        if(fitToWidth(maze)){
            width = (returnPercentOfAvailableScreenSpace().y - ((MIN_WIDTH_MATRIX_SIZE - 1f)*spaceBetweenTiles))/MIN_WIDTH_MATRIX_SIZE;
            height = (calculateTrueCanvasWidth()/calculateTrueCanvasHeight()) * width;
        } else {
            height = (returnPercentOfAvailableScreenSpace().x - ((MIN_HEIGHT_MATRIX_SIZE - 1f)*((calculateTrueCanvasWidth()/calculateTrueCanvasHeight()) * spaceBetweenTiles)))/MIN_HEIGHT_MATRIX_SIZE;
            width = (calculateTrueCanvasHeight()/calculateTrueCanvasWidth()) * height;
        }

        return new Vector2(width, height);
    }

    //Devuelve un vector para sumar a las posiciones y centrarlas
    Vector2 getPositionPadding(){
        float freeSpace, shadeSpacing;

        if(addShades){
            shadeSpacing = calculateShadeSize();
        } else {
            shadeSpacing = 0f;
        }

        if(fitToWidth(maze)){
            freeSpace = calculateTrueCanvasHeight()/getCanvasHeight() - (((calculateTileSize().y + shadeSpacing)*(float)getMazeUnitHeight(maze) + (calculateTrueCanvasWidth()/calculateTrueCanvasHeight() * spaceBetweenTiles)*((float)getMazeUnitHeight(maze) - 1f)));
            return new Vector2(0f, freeSpace/2f);
        } else {
            freeSpace = calculateTrueCanvasWidth()/getCanvasWidth() - ((calculateTileSize().x*(float)getMazeUnitWidth(maze) + (spaceBetweenTiles)*((float)getMazeUnitWidth(maze) - 1f)));
            return new Vector2(freeSpace/2f, 0f);
        }
    }

    //Devuelve la posición porcentual del punto de inicio
    //0 = Height; 1 = Width
    Vector2 getZeroZeroAvailablePosition(){
        return new Vector2(1f - getUpperElementsSize()/getCanvasHeight() - percentHeightMargin/2f, percentWidthMargin/2f);
    }

    //Devuelve el porcentaje de espacio real usable para el tablero
    //0 = Height; 1 = Width
    Vector2 returnPercentOfAvailableScreenSpace(){
        return new Vector2(calculateTrueCanvasHeight()/getCanvasHeight(), calculateTrueCanvasWidth()/getCanvasWidth());

    }

    //Devuelve la cantidad máxima de columnas/cuadrados en el eje horizontal
    int getMazeUnitWidth(int[,] matrix){
        return matrix.GetLength(1);
    }

    //Devuelve la cantidad máxima de filas/cuadrados en el eje vertical
    int getMazeUnitHeight(int[,] matrix){
        return matrix.GetLength(0);
    }

    //Devuelve el ancho total en pixeles que está usando el Canvas
    float getCanvasWidth(){
        return mazeBase.transform.GetComponent<RectTransform>().rect.width;
    }

    //Devuelve el alto total en pixeles que está usando el Canvas
    float getCanvasHeight(){
        return mazeBase.transform.GetComponent<RectTransform>().rect.height;
    }

    //Calcula la cantidad real disponible para implementar baldosas teniendo en cuenta
    //elementos de la UI, márgenes...
    float calculateTrueCanvasWidth(){
        return getCanvasWidth()*(1f - percentWidthMargin);
    }

    //Calcula la cantidad real disponible para implementar baldosas teniendo en cuenta
    //elementos de la UI, márgenes...
    float calculateTrueCanvasHeight(){
        return canvasHeightSubstractTopElementsSize()*(1f - percentHeightMargin);
    }

    //Devuelve el espacio vertical ocupado por los elementos superiores
    float getUpperElementsSize(){

        float configMaxYPoint, hintMaxYPoint, levelMaxYPoint, overallMaxYPoint;
        configMaxYPoint = (1f-configurationButton.GetComponent<RectTransform>().anchorMin.y)*(getCanvasHeight()) + configurationButton.GetComponent<RectTransform>().rect.height/2f;
        hintMaxYPoint = (1f-hintButton.GetComponent<RectTransform>().anchorMin.y)*(getCanvasHeight()) + hintButton.GetComponent<RectTransform>().rect.height/2f;
        levelMaxYPoint = (1f-levelsButton.GetComponent<RectTransform>().anchorMin.y)*(getCanvasHeight()) + levelsButton.GetComponent<RectTransform>().rect.height/2f;
        if(configMaxYPoint >= hintMaxYPoint){
            if(configMaxYPoint >= levelMaxYPoint){
                overallMaxYPoint = configMaxYPoint;
            } else {
                overallMaxYPoint = levelMaxYPoint;
            }
        } else if(hintMaxYPoint >= levelMaxYPoint){
            overallMaxYPoint = hintMaxYPoint;
        } else {
            overallMaxYPoint = levelMaxYPoint;
        }

        return overallMaxYPoint;
    }

    //Resta al total de altura, el espacio ocupado por los elementos
    //de la parte superior de la interfaz (sin contar los márgenes)
    float canvasHeightSubstractTopElementsSize(){
        return getCanvasHeight() - getUpperElementsSize();
    }

    private MazeColors getColorSet(){
        if(uiHandler.darkmode){
            return darkmodeColors;
        } else {
            return colors;
        }
    }

    private Sprite getSpriteSet(){
        if(uiHandler.darkmode){
            return sprites.filledTile;
        } else {
            return sprites.voidTile;
        }
    }

    public void switchMazeColorMode(){
        float minimizeFactor = 0.6f;

        if(uiHandler.darkmode){
            spawnBaseTile(new Vector3(endPosition.x, endPosition.y, -2f), getColorSet().baseColor);
        }
        
        foreach(Transform children in mazeBase.transform){
            if(children.name != ("Tile [" + endPosition.x + "][" + endPosition.y + "]")){
                children.GetComponent<Image>().sprite = getSpriteSet();
                children.GetComponent<Image>().color = getColorSet().baseColor;
            } else {
                if(!uiHandler.darkmode){
                    Destroy(children.gameObject);
                }
            }
        }
        //Base
        GameObject endGO = end.transform.Find("End_Object").Find("End Base").gameObject;
        
        if(uiHandler.darkmode){
            //End
            endGO.GetComponent<RectTransform>().localScale = new Vector3((minimizeFactor*calculateTileSizeInPixels().x)/TILE_DEFAULT_SIZE, (minimizeFactor*calculateTileSizeInPixels().y)/TILE_DEFAULT_SIZE, 1f);
            
        } else {
            endGO.GetComponent<RectTransform>().localScale = new Vector3((calculateTileSizeInPixels().x)/TILE_DEFAULT_SIZE, (calculateTileSizeInPixels().y)/TILE_DEFAULT_SIZE, 1f);
        }

        endGO.GetComponent<Image>().color = getColorSet().endColor;

        //Path
        foreach(Transform children in path.transform.Find("PathBase")){
            children.GetComponent<Image>().color = getColorSet().pathColor;
        }

        //Background gradient
        uiHandler.switchBGState();
    }

    public void resetPathLevel(){
        foreach(Transform children in path.transform.Find("PathBase")){
            GameObject.Destroy(children.gameObject);
        }

        cp_Counter = 0;
        cp_GlobalSteps = 0;
        cp_Steps = 0;
        cp_Position = startPosition;
        cp_Direction = Direction.Up;
        movingPathState = false;
        waitingForRemove = false;
        destroyCurrentPath = false;
        dirBuffer = new List<Direction>();
        maze = mazeCopy.Clone() as int[,];

        spawnPath();

        uiHandler.returnToGame();

    }

}
