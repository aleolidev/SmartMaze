using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Swipe_Detector : MonoBehaviour
{

    private Vector2 firstPosition;
    private Vector2 lastPosition;

    public Slider sensibility;
    public Slider moveOnRelease;

    public bool someInterfaceEnabled = false;

    public float minDistanceForSwipe = 30f;
    [SerializeField]
    public float initTimeBetweenDisplacements = 0.5f;
    [SerializeField]
    public float minTimeBetweenDisplacements = 0.25f;
    public float extraFirstInRowDisplacement = 0.2f;

    float currentTimeBetweenDisplacements;

    int inRowDisplacements = 0;

    public Path_Handler path_handler;
    public bool moveJustOnRelease = false;
    string direction = "None";

    bool isFirstTouch = true;
    bool isFirstInRowMove = true;
    bool ableToSendInfo = true;
    Vector2 firstTouch;

    float timer = 0f;

    void Start(){
        currentTimeBetweenDisplacements = initTimeBetweenDisplacements + extraFirstInRowDisplacement;

        if(PlayerPrefs.GetInt("despSostenido") == 0){
            moveJustOnRelease = true;
        } else {
            moveJustOnRelease = false;
        }

        minDistanceForSwipe = PlayerPrefs.GetFloat("sensibilidad");
    }

    void Update()
    {
        if(!someInterfaceEnabled){

            /*if(moveOnRelease.value == 0){
                moveJustOnRelease = true;
            } else {
                moveJustOnRelease = false;
            }

            minDistanceForSwipe = ((1f - sensibility.value) * 300f) + 0.15f;*/

            foreach(Touch touch in Input.touches){
                //Debug.Log("Procesando touch");
                if(isFirstTouch){
                        isFirstTouch = false;
                        firstTouch = touch.position;
                        if(firstTouch.x > Screen.width*0.05f && firstTouch.x < Screen.width*0.95f && firstTouch.y > Screen.height*0.05f && firstTouch.y < Screen.height*0.97f){
                            ableToSendInfo = true;
                        } else {
                            ableToSendInfo = false;
                        }
                }


                if(touch.phase == TouchPhase.Began){
                    firstPosition = touch.position;
                    lastPosition = touch.position;
                }

                if(!moveJustOnRelease && touch.phase == TouchPhase.Moved){
                    lastPosition = touch.position;
                    SendData();
                }

                if(touch.phase == TouchPhase.Ended){
                    lastPosition = touch.position;
                    SendData();

                    if(!moveJustOnRelease){
                        direction = "None";
                        resetMovements();
                    }
                    isFirstTouch = true;
                }
                
            }

            timer += Time.deltaTime;

            if(timer >= currentTimeBetweenDisplacements && direction != "None" && !isFirstInRowMove){
                inRowDisplacements++;
                if(inRowDisplacements < 5){
                    currentTimeBetweenDisplacements = (inRowDisplacements/5f)*(initTimeBetweenDisplacements-minTimeBetweenDisplacements) + minTimeBetweenDisplacements;
                } else {
                    currentTimeBetweenDisplacements = minTimeBetweenDisplacements;
                }
                
                switch(direction){
                    case "Down":
                        path_handler.moveDown = true;  
                    break;
                    case "Up":
                        path_handler.moveUp = true;
                    break;
                    case "Left":
                        path_handler.moveLeft = true;
                    break;
                    case "Right":
                        path_handler.moveRight = true;
                    break;
                }

                if(moveJustOnRelease){
                    direction = "None";
                }

                timer = 0f;
            } else if (direction != "None" && isFirstInRowMove){
                inRowDisplacements++;

                switch(direction){
                    case "Down":
                        path_handler.moveDown = true;
                        isFirstInRowMove = false;  
                    break;
                    case "Up":
                        path_handler.moveUp = true;
                        isFirstInRowMove = false; 
                    break;
                    case "Left":
                        path_handler.moveLeft = true;
                        isFirstInRowMove = false; 
                    break;
                    case "Right":
                        path_handler.moveRight = true;
                        isFirstInRowMove = false; 
                    break;
                }

                if(moveJustOnRelease){
                    direction = "None";
                }

                timer = 0f;
            }

        }
        
    }

    void SendData(){
        float verticalDifference = firstPosition.y - lastPosition.y;
        float horizontalDifference = firstPosition.x - lastPosition.x;

        if(Mathf.Abs(verticalDifference) >= minDistanceForSwipe || Mathf.Abs(horizontalDifference) >= minDistanceForSwipe){
            if(Mathf.Abs(verticalDifference) > Mathf.Abs(horizontalDifference)){
                if(verticalDifference >= 0 && direction != "Down"){
                    //Go Down
                    if(ableToSendInfo){
                        direction = "Down";
                        resetMovements();
                    }
                } else if (verticalDifference < 0 && direction != "Up"){
                    //Go Up
                    if(ableToSendInfo){
                        direction = "Up";
                        resetMovements();
                    }
                }
                firstPosition = lastPosition;
            } else{
                if(horizontalDifference >= 0 && direction != "Left"){
                    //Go Left
                    if(ableToSendInfo){
                        direction = "Left";
                        resetMovements();
                    }
                } else if(horizontalDifference < 0 && direction != "Right"){
                    //Go Right
                    if(ableToSendInfo){
                        direction = "Right";
                        resetMovements();
                    }
                }
                firstPosition = lastPosition;
            }
        }
    }

    void resetMovements(){
        timer = 0f;
        isFirstInRowMove = true;
        inRowDisplacements = 0;
        currentTimeBetweenDisplacements = initTimeBetweenDisplacements + extraFirstInRowDisplacement;
    }
}
