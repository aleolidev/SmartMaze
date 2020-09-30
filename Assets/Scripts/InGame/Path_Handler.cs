using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Path_Handler : MonoBehaviour
{
    [Range(0f, 2f)]public float animDuration = 0.5f;
    [Range(0.2f, 0.5f)]public float CornerNormalRadius = 0.4f;
    public Material pathColor;
    public Material pathShadow;
    public Material finalColor;
    public Material finalShadow;

    public GameObject UIHandler;

    float CornerRoundness = 0.1f;
    int RoundPoints = 6;
    float ShadowDisplacement = 0.1f;
    float MarginBetweenSquares = 0.1f;
    float sideMargins = 1f;
    [HideInInspector]public int[,] map;
    int[,] pathMap;
    [HideInInspector]public Vector2 initialTile;
    [HideInInspector]public Vector2 finalTile;
    [HideInInspector]public bool passedValues = false;
    bool PathGenerated = false;
    bool moving = false;

    [HideInInspector]public bool moveLeft = false;
    [HideInInspector]public bool moveRight = false;
    [HideInInspector]public bool moveUp = false;
    [HideInInspector]public bool moveDown = false;

    Vector2 moveIn = new Vector2(0, 0);
    Vector2 lastMove = new Vector2(0, 0);
    Vector2 currentPosition = new Vector2(0, 0);

    int footstepSlabs = 2;

    int tiles_generated = 0;

    bool goBack = false;
    string lastMoveDirection, moveToDirection;

    Mesh SquareMesh;
    Mesh ShadowMesh;
    Transform movableSquare;

    List<Vector3> SquareVertices;
    List<int> SquareTriangles;
    List<Vector3> ShadowVertices;
    List<int> ShadowTriangles;

    Vector3 squareTarget;
    
    void Start()
    {
        GetMazeHandlerVarValues();
        StartCoroutine(GeneratePath());
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMesh();
        CheckIfMoving();

        bool usingUI = UIHandler.GetComponent<UI_Handler>().usingInterface; 

        if(PathGenerated && !usingUI){
            if((Input.GetKeyDown(KeyCode.D) || moveRight) && !moving){
                if(moveRight){
                    moveRight = false;
                }
                if((int)currentPosition.y >= 0 && (int)currentPosition.y < pathMap.GetLength(0)){
                    if(((int)currentPosition.x + 1) >= 0 && ((int)currentPosition.x + 1) < pathMap.GetLength(1)){
                        int pathMapValue = pathMap[(int)currentPosition.y, (int)currentPosition.x + 1];
                        int aux = (int)currentPosition.x + 1;
                        if(pathMapValue == 0 || (pathMapValue == (footstepSlabs - 1) && pathMap[(int)currentPosition.y, (int)currentPosition.x] != 2) || pathMapValue == CountSlabs()+2){
                            lastMove = moveIn;
                            moveIn.x = 1;
                            moveIn.y = 0;

                            movePath();
                        }
                    }
                }
            } else if((Input.GetKeyDown(KeyCode.A) || moveLeft) && !moving){
                if(moveLeft){
                    moveLeft = false;
                }
                if((int)currentPosition.y >= 0 && (int)currentPosition.y < pathMap.GetLength(0)){
                    if(((int)currentPosition.x - 1) >= 0 && ((int)currentPosition.x - 1) < pathMap.GetLength(1)){
                        int pathMapValue = pathMap[(int)currentPosition.y, (int)currentPosition.x - 1];
                        if(pathMapValue == 0 || (pathMapValue == (footstepSlabs - 1) && pathMap[(int)currentPosition.y, (int)currentPosition.x] != 2) || pathMapValue == CountSlabs()+2){
                            lastMove = moveIn;
                            moveIn.x = -1;
                            moveIn.y = 0;

                            movePath();
                        }
                    }
                }
            } else if((Input.GetKeyDown(KeyCode.W) || moveUp) && !moving){
                if(moveUp){
                    moveUp = false;
                }
                if(((int)currentPosition.y - 1) >= 0 && ((int)currentPosition.y - 1) < pathMap.GetLength(0)){
                    if((int)currentPosition.x >= 0 && (int)currentPosition.x < pathMap.GetLength(1)){
                        int pathMapValue = pathMap[(int)currentPosition.y - 1, (int)currentPosition.x];
                        if(pathMapValue == 0 || (pathMapValue == (footstepSlabs - 1) && pathMap[(int)currentPosition.y, (int)currentPosition.x] != 2) || pathMapValue == CountSlabs()+2){
                            lastMove = moveIn;
                            moveIn.x = 0;
                            moveIn.y = 1;

                            movePath();
                        }
                    }
                }
            } else if ((Input.GetKeyDown(KeyCode.S) || moveDown) && !moving){
                if(moveDown){
                    moveDown = false;
                }
                if(((int)currentPosition.y + 1) >= 0 && ((int)currentPosition.y + 1) < pathMap.GetLength(0)){
                    if((int)currentPosition.x >= 0 && (int)currentPosition.x < pathMap.GetLength(1)){
                        int pathMapValue = pathMap[(int)currentPosition.y + 1, (int)currentPosition.x];
                        if(pathMapValue == 0 || (pathMapValue == (footstepSlabs - 1) && pathMap[(int)currentPosition.y, (int)currentPosition.x] != 2) || pathMapValue == CountSlabs()+2){
                            lastMove = moveIn;
                            moveIn.x = 0;
                            moveIn.y = -1;

                            movePath();
                        }
                    }
                }
            }
        }
    }

    IEnumerator GeneratePath(){  
        while(passedValues == false){
            yield return null;
        }
        GenerateFinalSquare();
        GenerateMovableSquare(initialTile);
        SetupPathMap();
        PathGenerated = true;
    }

    void SetupPathMap(){
        pathMap = (int[,])map.Clone();
        pathMap[(int)initialTile.y, (int)initialTile.x] = 2;
        pathMap[(int)finalTile.y, (int)finalTile.x] = CountSlabs() + 2;
        currentPosition = initialTile;
        //DebugMap(pathMap);
    }

    void movePath(){
        moving = true;
        goBack = false;

        Transform deformSquare = this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundlessSquare").transform;
        movableSquare = this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare1").transform;

        Vector3 initSquareTruePosition = this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare0").transform.Find("Square").transform.Find("Corner 1").transform.GetComponent<MeshFilter>().mesh.vertices[0] + this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare0").transform.position;
        Vector3 squareTruePosition = this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare1").transform.Find("Square").transform.Find("Corner 1").transform.GetComponent<MeshFilter>().mesh.vertices[0] + this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare1").transform.position;
        

        if(lastMove.x == 1 || lastMove.x == -1){
            lastMoveDirection = "Horizontal";
        } else if(lastMove.y == 1 || lastMove.y == -1){
            lastMoveDirection = "Vertical";
        } else {
            lastMoveDirection = "Null";
        }
        if(moveIn.x == 1 || moveIn.x == -1){
            moveToDirection = "Horizontal";
        } else if(moveIn.y == 1 || moveIn.y == -1){
            moveToDirection = "Vertical";
        } else {
            moveToDirection = "Null";
        }

        if(lastMoveDirection == moveToDirection){
            if(moveToDirection == "Horizontal"){
                if(squareTruePosition.x > (initSquareTruePosition.x + 0.05f)){
                    if(moveIn.x == -1){
                        goBack = true;
                    } else {
                        goBack = false;
                    }
                } else if(squareTruePosition.x < (initSquareTruePosition.x - 0.05f)){
                    if(moveIn.x == 1){
                        goBack = true;
                    } else {
                        goBack = false;
                    }
                } else{
                    goBack = false;
                }
            } else if (moveToDirection == "Vertical"){
                if(squareTruePosition.y > (initSquareTruePosition.y + 0.05f)){
                    if(moveIn.y == -1){
                        goBack = true;
                    } else {
                        goBack = false;
                    }
                } else if(squareTruePosition.y < (initSquareTruePosition.y - 0.05f)){
                    if(moveIn.y == 1){
                        goBack = true;
                    } else {
                        goBack = false;
                    }
                } else {
                    goBack = false;
                }
            } else {
                goBack = false;
            }
        }

        if(goBack){
            pathMap[(int)currentPosition.y, (int)currentPosition.x] = 0;
            footstepSlabs = footstepSlabs - 1;
            currentPosition = searchSlab(footstepSlabs);
        } else {
            footstepSlabs++;
            currentPosition = currentPosition + new Vector2(moveIn.x, -moveIn.y);
            pathMap[(int)currentPosition.y, (int)currentPosition.x] = footstepSlabs;
        }

        //Si hay un cambio de dirección, generar un nuevo cuadrado movible
        if((lastMoveDirection != moveToDirection) && (lastMoveDirection != "Null") && (moveToDirection != "Null")){
            Vector2 newPos = new Vector2(Mathf.Round(squareTruePosition.x/(CornerNormalRadius*2f+MarginBetweenSquares)), Mathf.Round((float)map.GetLength(0) - squareTruePosition.y/(CornerNormalRadius*2f + ShadowDisplacement + MarginBetweenSquares) - 1));
            GenerateMovableSquare(newPos);
            lastMove = new Vector2(0, 0);
        }
        

        deformSquare = this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundlessSquare").transform;
        movableSquare = this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare1").transform;
        SquareMesh = deformSquare.Find("Square").transform.GetComponent<MeshFilter>().mesh;
        ShadowMesh = deformSquare.Find("Shadow").transform.GetComponent<MeshFilter>().mesh;
        float verticalDisplacement = CornerNormalRadius*2 + ShadowDisplacement + MarginBetweenSquares;
        float horizontalDisplacement = CornerNormalRadius*2 + MarginBetweenSquares;

        SquareVertices = new List<Vector3>(SquareMesh.vertices);
        SquareTriangles = new List<int>(SquareMesh.triangles);
        ShadowVertices = new List<Vector3>(ShadowMesh.vertices);
        ShadowTriangles = new List<int>(ShadowMesh.triangles);

        //En primer lugar se comprueba hacia qué dirección se mueve para ampliar en esa dirección el cuadrado
        if(moveToDirection == "Horizontal"){
            if(SquareVertices[0] == SquareVertices[2]){
                //Se cambia la dirección en la que se moverá el cuadrado a deformar
                SquareVertices[0] = new Vector3(0,-CornerNormalRadius,0) + SquareVertices[0];
                SquareVertices[1] = new Vector3(0,-CornerNormalRadius,0) + SquareVertices[1];
                SquareVertices[2] = new Vector3(0,CornerNormalRadius,0) + SquareVertices[2];
                SquareVertices[3] = new Vector3(0,CornerNormalRadius,0) + SquareVertices[3];
                ShadowVertices[0] = new Vector3(0,-CornerNormalRadius,0) + ShadowVertices[0];
                ShadowVertices[1] = new Vector3(0,-CornerNormalRadius,0) + ShadowVertices[1];
                ShadowVertices[2] = new Vector3(0,CornerNormalRadius,0) + ShadowVertices[2];
                ShadowVertices[3] = new Vector3(0,CornerNormalRadius,0) + ShadowVertices[3];
            }  

            if(moveIn.x == 1){
                
                if(!goBack){    
                    DOTween.To(()=> SquareVertices[1], x=>SquareVertices[1] = x, new Vector3(horizontalDisplacement,0,0) + SquareVertices[1], animDuration);
                    DOTween.To(()=> SquareVertices[3], x=>SquareVertices[3] = x, new Vector3(horizontalDisplacement,0,0) + SquareVertices[3], animDuration);
                    DOTween.To(()=> ShadowVertices[1], x=>ShadowVertices[1] = x, new Vector3(horizontalDisplacement,0,0) + ShadowVertices[1], animDuration);
                    DOTween.To(()=> ShadowVertices[3], x=>ShadowVertices[3] = x, new Vector3(horizontalDisplacement,0,0) + ShadowVertices[3], animDuration);
                } else {
                    DOTween.To(()=> SquareVertices[0], x=>SquareVertices[0] = x, new Vector3(horizontalDisplacement,0,0) + SquareVertices[0], animDuration);
                    DOTween.To(()=> SquareVertices[2], x=>SquareVertices[2] = x, new Vector3(horizontalDisplacement,0,0) + SquareVertices[2], animDuration);
                    DOTween.To(()=> ShadowVertices[0], x=>ShadowVertices[0] = x, new Vector3(horizontalDisplacement,0,0) + ShadowVertices[0], animDuration);
                    DOTween.To(()=> ShadowVertices[2], x=>ShadowVertices[2] = x, new Vector3(horizontalDisplacement,0,0) + ShadowVertices[2], animDuration);
                }

                squareTarget = movableSquare.transform.position + new Vector3(horizontalDisplacement,0,0);
                movableSquare.transform.DOMoveX(squareTarget.x, animDuration);
            } else if (moveIn.x == -1){
                if(!goBack){
                    DOTween.To(()=> SquareVertices[0], x=>SquareVertices[0] = x, new Vector3(-horizontalDisplacement,0,0) + SquareVertices[0], animDuration);
                    DOTween.To(()=> SquareVertices[2], x=>SquareVertices[2] = x, new Vector3(-horizontalDisplacement,0,0) + SquareVertices[2], animDuration);
                    DOTween.To(()=> ShadowVertices[0], x=>ShadowVertices[0] = x, new Vector3(-horizontalDisplacement,0,0) + ShadowVertices[0], animDuration);
                    DOTween.To(()=> ShadowVertices[2], x=>ShadowVertices[2] = x, new Vector3(-horizontalDisplacement,0,0) + ShadowVertices[2], animDuration); 
                } else {
                    DOTween.To(()=> SquareVertices[1], x=>SquareVertices[1] = x, new Vector3(-horizontalDisplacement,0,0) + SquareVertices[1], animDuration);
                    DOTween.To(()=> SquareVertices[3], x=>SquareVertices[3] = x, new Vector3(-horizontalDisplacement,0,0) + SquareVertices[3], animDuration);
                    DOTween.To(()=> ShadowVertices[1], x=>ShadowVertices[1] = x, new Vector3(-horizontalDisplacement,0,0) + ShadowVertices[1], animDuration);
                    DOTween.To(()=> ShadowVertices[3], x=>ShadowVertices[3] = x, new Vector3(-horizontalDisplacement,0,0) + ShadowVertices[3], animDuration);
                }
                     
                squareTarget = movableSquare.transform.position - new Vector3(horizontalDisplacement,0,0);
                movableSquare.transform.DOMoveX(squareTarget.x, animDuration);

            }

            //Se aplican los nuevos cambios
        } else if(moveToDirection == "Vertical"){
            if(SquareVertices[0] == SquareVertices[1]){
                //Se cambia la dirección en la que se moverá el cuadrado a deformar
                SquareVertices[0] = new Vector3(-CornerNormalRadius,0,0) + SquareVertices[0];
                SquareVertices[1] = new Vector3(CornerNormalRadius,0,0) + SquareVertices[1];
                SquareVertices[2] = new Vector3(-CornerNormalRadius,0,0) + SquareVertices[2];
                SquareVertices[3] = new Vector3(CornerNormalRadius,0,0) + SquareVertices[3];
                ShadowVertices[0] = new Vector3(-CornerNormalRadius,0,0) + ShadowVertices[0];
                ShadowVertices[1] = new Vector3(CornerNormalRadius,0,0) + ShadowVertices[1];
                ShadowVertices[2] = new Vector3(-CornerNormalRadius,0,0) + ShadowVertices[2];
                ShadowVertices[3] = new Vector3(CornerNormalRadius,0,0) + ShadowVertices[3];
            }
            
            //Se aplican los nuevos cambios
            if(moveIn.y == 1){
                
                if(!goBack){
                    DOTween.To(()=> SquareVertices[2], x=>SquareVertices[2] = x, new Vector3(0,verticalDisplacement,0) + SquareVertices[2], animDuration);
                    DOTween.To(()=> SquareVertices[3], x=>SquareVertices[3] = x, new Vector3(0,verticalDisplacement,0) + SquareVertices[3], animDuration);
                    DOTween.To(()=> ShadowVertices[2], x=>ShadowVertices[2] = x, new Vector3(0,verticalDisplacement,0) + ShadowVertices[2], animDuration);
                    DOTween.To(()=> ShadowVertices[3], x=>ShadowVertices[3] = x, new Vector3(0,verticalDisplacement,0) + ShadowVertices[3], animDuration);
                } else {
                    DOTween.To(()=> SquareVertices[0], x=>SquareVertices[0] = x, new Vector3(0,verticalDisplacement,0) + SquareVertices[0], animDuration);
                    DOTween.To(()=> SquareVertices[1], x=>SquareVertices[1] = x, new Vector3(0,verticalDisplacement,0) + SquareVertices[1], animDuration);
                    DOTween.To(()=> ShadowVertices[0], x=>ShadowVertices[0] = x, new Vector3(0,verticalDisplacement,0) + ShadowVertices[0], animDuration);
                    DOTween.To(()=> ShadowVertices[1], x=>ShadowVertices[1] = x, new Vector3(0,verticalDisplacement,0) + ShadowVertices[1], animDuration);
                }
                
                squareTarget = movableSquare.transform.position + new Vector3(0,verticalDisplacement,0);
                movableSquare.transform.DOMoveY(squareTarget.y, animDuration);
            } else if (moveIn.y == -1){

                if(!goBack){
                    DOTween.To(()=> SquareVertices[0], x=>SquareVertices[0] = x, new Vector3(0,-verticalDisplacement,0) + SquareVertices[0], animDuration);
                    DOTween.To(()=> SquareVertices[1], x=>SquareVertices[1] = x, new Vector3(0,-verticalDisplacement,0) + SquareVertices[1], animDuration);
                    DOTween.To(()=> ShadowVertices[0], x=>ShadowVertices[0] = x, new Vector3(0,-verticalDisplacement,0) + ShadowVertices[0], animDuration);
                    DOTween.To(()=> ShadowVertices[1], x=>ShadowVertices[1] = x, new Vector3(0,-verticalDisplacement,0) + ShadowVertices[1], animDuration);     
                } else {
                    DOTween.To(()=> SquareVertices[2], x=>SquareVertices[2] = x, new Vector3(0,-verticalDisplacement,0) + SquareVertices[2], animDuration);
                    DOTween.To(()=> SquareVertices[3], x=>SquareVertices[3] = x, new Vector3(0,-verticalDisplacement,0) + SquareVertices[3], animDuration);
                    DOTween.To(()=> ShadowVertices[2], x=>ShadowVertices[2] = x, new Vector3(0,-verticalDisplacement,0) + ShadowVertices[2], animDuration);
                    DOTween.To(()=> ShadowVertices[3], x=>ShadowVertices[3] = x, new Vector3(0,-verticalDisplacement,0) + ShadowVertices[3], animDuration);  
                }

                squareTarget = movableSquare.transform.position - new Vector3(0,verticalDisplacement,0);
                movableSquare.transform.DOMoveY(squareTarget.y, animDuration);
            }
            
        }
    }
    
    void UpdateMesh(){
        if(SquareMesh != null && ShadowMesh != null && SquareVertices != null && SquareTriangles != null && ShadowVertices != null && ShadowTriangles != null){
            SquareMesh.Clear();
            SquareMesh.vertices = SquareVertices.ToArray();
            SquareMesh.triangles = SquareTriangles.ToArray();
            ShadowMesh.Clear();
            ShadowMesh.vertices = ShadowVertices.ToArray();
            ShadowMesh.triangles = ShadowTriangles.ToArray();
        }
    }

    void CheckIfMoving(){
        if(squareTarget != null && movableSquare != null){
            if(squareTarget == movableSquare.transform.position && moving == true){
                RemoveIfUndo();
                moving = false;
            }
        }
    }

    void RemoveIfUndo(){
        Vector3 newSquare0TruePos = this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare0").transform.Find("Square").transform.Find("Corner 1").transform.GetComponent<MeshFilter>().mesh.vertices[0] + this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare0").transform.position;
        Vector3 newSquare1TruePos = this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare1").transform.Find("Square").transform.Find("Corner 1").transform.GetComponent<MeshFilter>().mesh.vertices[0] + this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare1").transform.position;

        if(goBack && (CountSquares() + 1) > 2){
            if((Mathf.Round(newSquare0TruePos.x*100f))/100f == (Mathf.Round(newSquare1TruePos.x*100f))/100f && (Mathf.Round(newSquare0TruePos.y*100f))/100f == (Mathf.Round(newSquare1TruePos.y*100f))/100f){
                DestroyImmediate(this.transform.Find("MovableSquare" + (CountSquares())).transform.gameObject);

                newSquare0TruePos = this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare0").transform.Find("Square").transform.Find("Corner 1").transform.GetComponent<MeshFilter>().mesh.vertices[0] + this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare0").transform.position;
                newSquare1TruePos = this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare1").transform.Find("Square").transform.Find("Corner 1").transform.GetComponent<MeshFilter>().mesh.vertices[0] + this.transform.Find("MovableSquare" + (CountSquares())).transform.Find("RoundSquare1").transform.position;
                
                lastMoveDirection = moveToDirection;
                if(moveToDirection == "Horizontal"){
                    moveToDirection = "Vertical";
                } else if(moveToDirection == "Vertical"){
                    moveToDirection = "Horizontal";
                }

                lastMove = moveIn;

                switch(moveToDirection){
                    case "Horizontal":
                        if(newSquare0TruePos.x > newSquare1TruePos.x){
                            moveIn = new Vector2(-1, 0);
                        } else if(newSquare0TruePos.x < newSquare1TruePos.x){
                            moveIn = new Vector2(1, 0);
                        }
                    break;
                    case "Vertical":
                        if(newSquare0TruePos.y > newSquare1TruePos.y){
                            moveIn = new Vector2(0, -1);
                        } else if(newSquare0TruePos.y < newSquare1TruePos.y){
                            moveIn = new Vector2(0, 1);
                        }
                    break;
                }
            }
        }
    }

    public void ResetPath(){
        
        //V1:
        PathGenerated = false;
        for(int i = (CountSquares()); i > 0; i--){
            DestroyImmediate(this.transform.Find("MovableSquare" + (i)).transform.gameObject);
        }
        moveLeft = false;
        moveRight = false;
        moveUp = false;
        moveDown = false;
        moveIn = new Vector2(0,0);
        lastMove = new Vector2(0,0);
        currentPosition = initialTile;
        footstepSlabs = 2;
        tiles_generated = 0;
        goBack = false;
        lastMoveDirection = null; 
        moveToDirection = null;
        

        // SquareVertices.Clear();
        // SquareTriangles.Clear();
        // ShadowVertices.Clear();
        // ShadowTriangles.Clear();

        //squareTarget = new Vector3;
        //Destroy(this.transform.Find("LastSquare").transform.gameObject);
        //GenerateFinalSquare();
        GenerateMovableSquare(initialTile);
        SetupPathMap();
        PathGenerated = true;
        

        //V2:
        

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    void GenerateMovableSquare(Vector2 position){
        GameObject movSquare = new GameObject("MovableSquare" + (CountSquares()+1));

        int yAxis = map.GetLength(0) - (int)position.y - 1;
        Vector3 squarePosition = new Vector3(position.x*(CornerNormalRadius*2 + MarginBetweenSquares), yAxis*(CornerNormalRadius*2f + ShadowDisplacement + MarginBetweenSquares), -0.5f);
        GameObject square0 = GenerateRoundedSquare(squarePosition, pathColor, pathShadow, "RoundSquare0", (int)position.x, (int)position.y, CornerNormalRadius);
        GameObject roundlesssquare = GenerateSquare(squarePosition, pathColor, pathShadow, "RoundlessSquare", (int)position.x, (int)position.y);
        GameObject square1 = GenerateRoundedSquare(squarePosition, pathColor, pathShadow, "RoundSquare1", (int)position.x, (int)position.y, CornerNormalRadius);

        square0.transform.parent = movSquare.transform;
        roundlesssquare.transform.parent = movSquare.transform;
        square1.transform.parent = movSquare.transform;

        movSquare.transform.parent = this.transform;
    }

    void GenerateFinalSquare(){
        /*
        V1:

        int yAxis = map.GetLength(0) - (int)finalTile.y - 1;
        Vector3 squarePosition = new Vector3(finalTile.x*(CornerNormalRadius*2 + MarginBetweenSquares), yAxis*(CornerNormalRadius*2f + ShadowDisplacement + MarginBetweenSquares), -0.25f);
        GameObject finalSquare = GenerateRoundedSquare(squarePosition, finalColor, finalShadow, "LastSquare", (int)finalTile.x, (int)finalTile.y);
        final.transform.parent = this.transform;


        V2:
        
        int yAxis = map.GetLength(0) - (int)finalTile.y - 1;
        Vector3 squarePosition = new Vector3(finalTile.x*(CornerNormalRadius*2 + MarginBetweenSquares), yAxis*(CornerNormalRadius*2f + ShadowDisplacement + MarginBetweenSquares), -0.25f);
        //GameObject finalSquare = GenerateRoundedSquare(squarePosition, finalColor, finalShadow, "LastSquare", (int)finalTile.x, (int)finalTile.y);
        GameObject simpleSquare = GenerateSimpleSquare(squarePosition, pathColor);
        simpleSquare.transform.parent = this.transform;
        //simpleSquare.transform.parent = finalSquare.transform;
        
        
        V3:
        */
        int yAxis = map.GetLength(0) - (int)finalTile.y - 1;
        Vector3 squarePosition = new Vector3(finalTile.x*(CornerNormalRadius*2 + MarginBetweenSquares), yAxis*(CornerNormalRadius*2f + ShadowDisplacement + MarginBetweenSquares), -0.3f);
        //GameObject finalSquare = GenerateRoundedSquare(squarePosition, finalColor, finalShadow, "LastSquare", (int)finalTile.x, (int)finalTile.y);
        
        GameObject finalSquare;
        finalSquare = new GameObject(name);
        //square.transform.parent = this.transform;

        finalSquare = new GameObject("LastSquare");
        for(int i = 0; i < 4; i++){
            GenerateCorner(squarePosition, i, pathColor, ref finalSquare, CornerNormalRadius*(5f/7f));
        }
        
        finalSquare.transform.parent = this.transform;
        //simpleSquare.transform.parent = finalSquare.transform;
    }

    GameObject GenerateSimpleSquare(Vector3 spawnPosition, Material color){
        Mesh m = new Mesh();
        GameObject simpleSquare;
        List<Vector3> squareVertex = new List<Vector3>();
        List<int> squareTriangles = new List<int>();

        simpleSquare = new GameObject("Simple Square");
        simpleSquare.AddComponent<MeshFilter>();
        simpleSquare.AddComponent<MeshRenderer>().material = color;
        
        squareVertex.Add(new Vector3(spawnPosition.x - (CornerNormalRadius - (1f/2f)*CornerNormalRadius), spawnPosition.y - (CornerNormalRadius - (1f/2f)*CornerNormalRadius), spawnPosition.z + -0.1f));
        squareVertex.Add(new Vector3(spawnPosition.x + (CornerNormalRadius - (1f/2f)*CornerNormalRadius), spawnPosition.y - (CornerNormalRadius - (1f/2f)*CornerNormalRadius), spawnPosition.z + -0.1f));
        squareVertex.Add(new Vector3(spawnPosition.x - (CornerNormalRadius - (1f/2f)*CornerNormalRadius), spawnPosition.y + (CornerNormalRadius - (1f/2f)*CornerNormalRadius), spawnPosition.z + -0.1f));
        squareVertex.Add(new Vector3(spawnPosition.x + (CornerNormalRadius - (1f/2f)*CornerNormalRadius), spawnPosition.y + (CornerNormalRadius - (1f/2f)*CornerNormalRadius), spawnPosition.z + -0.1f));

        squareTriangles.Add(0);
        squareTriangles.Add(2);
        squareTriangles.Add(1);

        squareTriangles.Add(2);
        squareTriangles.Add(3);
        squareTriangles.Add(1);

        m.Clear();
        m.vertices = squareVertex.ToArray();
        m.triangles = squareTriangles.ToArray();

        simpleSquare.GetComponent<MeshFilter>().mesh = m;

        return simpleSquare;
    }

    GameObject GenerateRoundedSquare(Vector3 spawnPosition, Material color, Material shadow, string name, int x, int y, float radius){
        //Instantiate Square
        GameObject square;
        square = new GameObject(name);
        //square.transform.parent = this.transform;

        GameObject topSquare;
        topSquare = new GameObject("Square");
        for(int i = 0; i < 4; i++){
            GenerateCorner(spawnPosition, i, color, ref topSquare, radius);
        }
        topSquare.transform.parent = square.transform;

        //Instantiate Shadow
        GameObject squareShadow;
        squareShadow = new GameObject("Shadow");
        for(int i = 0; i < 4; i++){
            GenerateCorner(spawnPosition + new Vector3(0f, -ShadowDisplacement, 0.1f), i, shadow, ref squareShadow, radius);
        }
        squareShadow.transform.parent = square.transform;

        return square;
    }

    GameObject GenerateSquare(Vector3 spawnPosition, Material color, Material shadow, string name, int x, int y){
        GameObject square;
        square = new GameObject(name);
        //square.transform.parent = this.transform;

        Mesh m = new Mesh();
        List<Vector3> squareVertex = new List<Vector3>();
        List<int> squareTriangles = new List<int>();

        Vector3 newPoint = spawnPosition;
        /*
        3----4
        |    |
        1----2
        */
        squareVertex.Add(newPoint);
        squareVertex.Add(newPoint);
        squareVertex.Add(newPoint);
        squareVertex.Add(newPoint);

        //Primer triángulo
        squareTriangles.Add(0);
        squareTriangles.Add(2);
        squareTriangles.Add(1);
        //Segundo triángulo
        squareTriangles.Add(1);
        squareTriangles.Add(2);
        squareTriangles.Add(3);

        m.Clear();
        m.vertices = squareVertex.ToArray();
        m.triangles = squareTriangles.ToArray();

        GameObject topSquare;
        topSquare = new GameObject("Square");
        //Generar cuadrado superior

        /*for(int i = 0; i < 4; i++){
            GenerateCorner(spawnPosition, i, color, ref topSquare);
        }*/
        topSquare.AddComponent<MeshFilter>();
        topSquare.AddComponent<MeshRenderer>().material = color;

        topSquare.transform.parent = square.transform;
        topSquare.GetComponent<MeshFilter>().mesh = m;

        //Instantiate Shadow
        Mesh mShadow = new Mesh();
        List<Vector3> shadowVertex = new List<Vector3>();
        List<int> shadowTriangles = new List<int>();

        newPoint = spawnPosition + new Vector3(0f, -ShadowDisplacement, 0.1f);
        /*
        3----4
        |    |
        1----2
        */
        shadowVertex.Add(newPoint);
        shadowVertex.Add(newPoint);
        shadowVertex.Add(newPoint);
        shadowVertex.Add(newPoint);

        //Primer triángulo
        shadowTriangles.Add(0);
        shadowTriangles.Add(2);
        shadowTriangles.Add(1);
        //Segundo triángulo
        shadowTriangles.Add(1);
        shadowTriangles.Add(2);
        shadowTriangles.Add(3);

        mShadow.Clear();
        mShadow.vertices = shadowVertex.ToArray();
        mShadow.triangles = shadowTriangles.ToArray();

        GameObject squareShadow;
        squareShadow = new GameObject("Shadow");
        /*for(int i = 0; i < 4; i++){
            GenerateCorner(spawnPosition + new Vector3(0f, -ShadowDisplacement, 0.1f), i, shadow, ref squareShadow);
        }*/
        squareShadow.AddComponent<MeshFilter>();
        squareShadow.AddComponent<MeshRenderer>().material = shadow;

        squareShadow.transform.parent = square.transform;
        squareShadow.GetComponent<MeshFilter>().mesh = mShadow;

        return square;
    }

    void GenerateCorner(Vector3 spawnPosition, int flips, Material color, ref GameObject parent, float radius){
        Mesh m = new Mesh();
        GameObject corner;
        corner = new GameObject("Corner " + ((tiles_generated % 4) + 1));
        corner.AddComponent<MeshFilter>();
        corner.AddComponent<MeshRenderer>().material = color;

        DrawCorner(spawnPosition, flips, ref m, radius);

        tiles_generated++;
        
        corner.GetComponent<MeshFilter>().mesh = m;
        corner.transform.parent = parent.transform;
    }

    void DrawCorner(Vector3 spawnPosition, int flip_amount, ref Mesh m, float radius){
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
            Vector3 circleCenter = new Vector3(radius - CornerRoundness, radius - CornerRoundness, 0.0f);
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
                    sumY1 = radius;
                    sumX2 = (radius - CornerRoundness);
                    sumY2 = radius;
                break;
                case 1:
                    sumX1 = radius;
                    sumY1 = (radius - CornerRoundness);
                    sumX2 = radius;
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

    Vector2 searchSlab(int slabValue){
        Vector2 returnVector = new Vector2(0,0);

        for(int y = 0; y < (pathMap.GetLength(0)); y++){
            for(int x = 0; x < (pathMap.GetLength(1)); x++){
                if(pathMap[y,x] == slabValue){
                    returnVector = new Vector2(x, y);
                }
            }
        }
        return returnVector;
    }

    int CountSlabs(){
        int amountOfSlabs = 0;
        for(int y = 0; y < (map.GetLength(0)); y++){
            for(int x = 0; x < (map.GetLength(1)); x++){
                if(map[y,x] == 0){
                    amountOfSlabs++;
                }
            }
        }

        return amountOfSlabs;
    }

    void DebugMap(int[,] mapToDebug){
        string toDebug = "";
        for(int y = 0; y < (mapToDebug.GetLength(0)); y++){
            for(int x = 0; x < (mapToDebug.GetLength(1)); x++){
                toDebug += mapToDebug[y,x] + " ";
            }
            toDebug += "\n";
        }

        Debug.Log(toDebug);
    }

    void GetMazeHandlerVarValues(){
        Maze_Handler maze_handler = this.transform.parent.GetComponent<Maze_Handler>();
        CornerRoundness = maze_handler.CornerRoundness;
        RoundPoints = maze_handler.RoundPoints;
        ShadowDisplacement = maze_handler.ShadowDisplacement;
        CornerNormalRadius = maze_handler.CornerNormalRadius;
        MarginBetweenSquares = maze_handler.MarginBetweenSquares;
        sideMargins = maze_handler.sideMargins;
    }

    int CountSquares(){
        return (this.transform.childCount - 1);
    }
}
