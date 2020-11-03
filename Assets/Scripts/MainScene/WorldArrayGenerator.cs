using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldArrayGenerator : MonoBehaviour
{
    private enum Direction{
        NULL,
        DOT,
        UP,
        DOWN,
        LEFT,
        RIGHT
    }

    private enum levelStatus{
        NULL,
        COMPLETED,
        ACCESSIBLE,
        UNACCESSIBLE
    }
    // Start is called before the first frame update

    private Direction[,] BlueWorld = 
    {
        {Direction.NULL, Direction.DOT, Direction.NULL},
        {Direction.NULL, Direction.UP, Direction.NULL}
    };

    private levelStatus[,] BlueWorldStatus = 
    {
        {levelStatus.NULL, levelStatus.ACCESSIBLE, levelStatus.NULL},
        {levelStatus.NULL, levelStatus.COMPLETED, levelStatus.NULL}
    };
    void Start()
    {
        //GenerateWorld();   
    }
}
