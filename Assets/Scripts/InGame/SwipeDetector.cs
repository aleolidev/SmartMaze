using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    private Vector2 fingerDown;
    private Vector2 fingerUp;
    private bool ableToSendInfo = true;
    public bool detectAfterRelease = true;
    public UIHandler scriptUI;
    public MazeSystem mazeSystem;

    [SerializeField]
    private float minDistanceSwipe = 20f;

    public enum SwipeDirection{
        Up,
        Down,
        Left,
        Right
    }

    void Update()
    {
        
        foreach(Touch touch in Input.touches){
            if(touch.phase ==  TouchPhase.Began){
                fingerDown = touch.position;
                fingerUp = touch.position;
                if(fingerDown.x > Screen.width*0.05f && fingerDown.x < Screen.width*0.95f && fingerDown.y > Screen.height*0.05f && fingerDown.y < Screen.height*0.97f){
                    ableToSendInfo = true;
                } else {
                    ableToSendInfo = false;
                }
            }

            if(!detectAfterRelease && touch.phase == TouchPhase.Moved){
                fingerDown = touch.position;
                DetectSwipe();
            }

            if(touch.phase == TouchPhase.Ended){
                fingerDown = touch.position;
                DetectSwipe();
            }
            
        }
    }

    void DetectSwipe(){
        
        if(validDisplacement()){
            switch(scriptUI.sceneStat){
                case UIHandler.SceneStatus.WorldLevelSelector:
                    if(HorizontalDisplacement() > VerticalDisplacement()){
                        if(HorizontalDirection() == SwipeDirection.Right){
                            scriptUI.moveWorldLeft();
                        } else if (HorizontalDirection() == SwipeDirection.Left){
                            scriptUI.moveWorldRight();
                        }  
                    }
                break;
                case UIHandler.SceneStatus.InGame:
                    if(HorizontalDisplacement() > VerticalDisplacement()){
                        if(HorizontalDirection() == SwipeDirection.Right){
                            mazeSystem.managePath(MazeSystem.Direction.Right);
                        } else if (HorizontalDirection() == SwipeDirection.Left){
                            mazeSystem.managePath(MazeSystem.Direction.Left);
                        }  
                    } else {
                        if(VerticalDirection() == SwipeDirection.Up){
                            mazeSystem.managePath(MazeSystem.Direction.Up);
                        } else if (VerticalDirection() == SwipeDirection.Down){
                            mazeSystem.managePath(MazeSystem.Direction.Down);
                        }  
                    }
                break;
            }
        }
        
    }

    bool validDisplacement(){
        return (VerticalDisplacement() > minDistanceSwipe || HorizontalDisplacement() > minDistanceSwipe) && ableToSendInfo;
    }

    private float VerticalDisplacement(){
        return Mathf.Abs(fingerDown.y - fingerUp.y);
    }

    private float HorizontalDisplacement(){
        return Mathf.Abs(fingerDown.x - fingerUp.x);
    }

    private SwipeDirection HorizontalDirection(){
        if((fingerUp.x - fingerDown.x) > 0){
            return SwipeDirection.Left;
        } else {
            return SwipeDirection.Right;
        }
    }

    private SwipeDirection VerticalDirection(){
        if((fingerUp.y - fingerDown.y) > 0){
            return SwipeDirection.Down;
        } else {
            return SwipeDirection.Up;
        }
    }
}
