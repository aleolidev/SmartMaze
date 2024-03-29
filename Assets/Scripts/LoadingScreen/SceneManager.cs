﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SceneManager : MonoBehaviour
{

    public float animDuration = 0.4f;
    public float afterAnimDelay = 1f;
    public Camera cam;
    public GameObject ggLetter;
    public GameObject ggLight;
    public GameObject ggSquare;


    private float timer = 0;
    private float initAnim = 0;

    private float totalAnimDuration = 2f + 1.6f;
    

    void Start()
    {
        //Execute first intro
        Image letterImg = ggLetter.GetComponent<Image>();
        Image lightImg = ggLight.GetComponent<Image>();
        //letterImg.color = new Color(1f, 1f, 1f, 0f);
        lightImg.color = new Color(1f, 1f, 1f, 0f);

        RectTransform letterRT = ggLetter.GetComponent<RectTransform>();
        RectTransform lightRT = ggLight.GetComponent<RectTransform>();
        RectTransform squareRT = ggSquare.GetComponent<RectTransform>();


        //DOTween.To(()=> letterImg.color, x=>letterImg.color = x, new Color(letterImg.color.r, letterImg.color.g, letterImg.color.b, 1f), animDuration + 1f).SetEase(Ease.OutExpo);
        initAnim = timer;
        
        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(()=> lightImg.color, x=>lightImg.color = x, new Color(lightImg.color.r, lightImg.color.g, lightImg.color.b, 1f), 0.4f).SetEase(Ease.OutExpo));
    
        seq.Join(DOTween.To(()=> letterRT.localPosition, x=>letterRT.localPosition = x, new Vector3(letterRT.localPosition.x, -50f, letterRT.localPosition.z), animDuration).SetEase(Ease.OutExpo));
        seq.Join(DOTween.To(()=> lightRT.localPosition, x=>lightRT.localPosition = x, new Vector3(lightRT.localPosition.x, 170f, lightRT.localPosition.z), animDuration).SetEase(Ease.OutExpo));
        seq.Join(DOTween.To(()=> squareRT.localPosition, x=>squareRT.localPosition = x, new Vector3(squareRT.localPosition.x, 500f, squareRT.localPosition.z), animDuration).SetEase(Ease.OutExpo));
        
        seq.Join(DOTween.To(()=> letterRT.sizeDelta, x=>letterRT.sizeDelta = x, new Vector2(753f, 950f), animDuration).SetEase(Ease.OutExpo));
        seq.Join(DOTween.To(()=> lightRT.sizeDelta, x=>lightRT.sizeDelta = x, new Vector2(710f, 190f), animDuration).SetEase(Ease.OutExpo));
        
        //Sequence seq = DOTween.Sequence());
        seq.Join(DOTween.To(()=> lightRT.localScale, x=>lightRT.localScale = x, new Vector3(0.4f, 0.4f, lightRT.localScale.z), animDuration).SetEase(Ease.OutCirc));


        seq.AppendInterval(1f);
        seq.Append(DOTween.To(()=> letterImg.color, x=>letterImg.color = x, new Color(letterImg.color.r, letterImg.color.g, letterImg.color.b, 0f), 0.6f).SetEase(Ease.OutExpo));
        seq.Join(DOTween.To(()=> lightImg.color, x=>lightImg.color = x, new Color(lightImg.color.r, lightImg.color.g, lightImg.color.b, 0f), 0.6f).SetEase(Ease.OutExpo)); 
        //seq.Append(DOTween.To(()=> lightRT.localScale, x=>lightRT.localScale = x, new Vector3(0.8f, 0.8f, lightRT.localScale.z), 3f*animDuration/4f).SetEase(Ease.OutExpo));
        //seq.PrependInterval(animDuration/4f);
        //seq.Insert(0, transform.DOScale(new Vector3(3,3,3), seq.Duration()));
    }

    void Update() {
        timer += Time.deltaTime;

        if(timer > (initAnim + totalAnimDuration)){
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
