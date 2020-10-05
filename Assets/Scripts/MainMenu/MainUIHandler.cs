using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIHandler : MonoBehaviour
{
    public Canvas canvas;
    public GameObject BattleBackground;
    public GameObject BattleObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(BattleBackground.GetComponent<RectTransform>().sizeDelta.y != canvas.GetComponent<RectTransform>().sizeDelta.y){
            BattleBackground.GetComponent<RectTransform>().sizeDelta = new Vector2(BattleBackground.GetComponent<RectTransform>().sizeDelta.x, canvas.GetComponent<RectTransform>().sizeDelta.y);
        }
        if(BattleObject.GetComponent<RectTransform>().sizeDelta.y != canvas.GetComponent<RectTransform>().sizeDelta.y){
            BattleObject.GetComponent<RectTransform>().sizeDelta = new Vector2(BattleObject.GetComponent<RectTransform>().sizeDelta.x, canvas.GetComponent<RectTransform>().sizeDelta.y);
        }
    }
}
