using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIHandler : MonoBehaviour
{
    public Canvas canvas;
    public GameObject wrap;
    public GameObject BattleBackground;
    public GameObject BattleObject;

    private Vector2 screenSize;
    private Vector2 baseScreenSize = new Vector2(1080f, 1920f);
    private Vector2 baseWrapSize = new Vector2(1115f, 2488f);

    // Start is called before the first frame update
    void Start()
    {
        screenSize = BattleBackground.GetComponent<RectTransform>().sizeDelta;
        resizeTransitions();
    }

    // Update is called once per frame
    void Update()
    {
        if(!screenSize.Equals(canvas.GetComponent<RectTransform>().sizeDelta)){
            screenSize = canvas.GetComponent<RectTransform>().sizeDelta;
            resizeTransitions();
        }

        checkBattleSize();   
    }

    void resizeTransitions(){
        //Wrap

        wrap.GetComponent<RectTransform>().sizeDelta = new Vector2(baseWrapSize.x * (screenSize.x/baseScreenSize.x) + 2, baseWrapSize.y * (screenSize.y/baseScreenSize.y) + 2);
        wrap.GetComponent<RectTransform>().localPosition = new Vector2(-18f * (screenSize.x / baseScreenSize.x), wrap.GetComponent<RectTransform>().localPosition.y);
    }

    void checkBattleSize() {
        if(!screenSize.Equals(BattleBackground.GetComponent<RectTransform>().sizeDelta)){
            BattleBackground.GetComponent<RectTransform>().sizeDelta = screenSize;
            BattleBackground.GetComponent<RectTransform>().localPosition = new Vector2(screenSize.x/2f, 0f);
        }

        if(!screenSize.Equals(BattleObject.GetComponent<RectTransform>().sizeDelta)){
            BattleObject.GetComponent<RectTransform>().sizeDelta = screenSize;
        }
    }
}
