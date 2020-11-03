using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeDetector : MonoBehaviour
{
    private Vector2 fingerDown;
    private Vector2 fingerUp;
    public bool detectAfterRelease = true;
    public UIHandler scriptUI;

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
        switch(scriptUI.sceneStat){
            case  UIHandler.SceneStatus.WorldLevelSelector:
                if(validDisplacement()){
                    if(HorizontalDisplacement() > VerticalDisplacement()){
                        if(HorizontalDirection() == SwipeDirection.Right){
                            scriptUI.moveWorldLeft();
                        } else if (HorizontalDirection() == SwipeDirection.Left){
                            scriptUI.moveWorldRight();
                        }  
                    }
                }
            break;
        }
        
    }

    bool validDisplacement(){
        return VerticalDisplacement() > minDistanceSwipe || HorizontalDisplacement() > minDistanceSwipe;
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
