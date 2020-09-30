using System.Collections;
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

    private bool firstAnimEnded = false;
    private bool startedSecondAnim = false;
    private bool secondAnimEnded = false;
    

    void Start()
    {
        //Execute first intro
        //Image letterImg = ggLetter.GetComponent<Image>();
        Image lightImg = ggLight.GetComponent<Image>();
        //letterImg.color = new Color(1f, 1f, 1f, 0f);
        lightImg.color = new Color(1f, 1f, 1f, 0f);
        //DOTween.To(()=> letterImg.color, x=>letterImg.color = x, new Color(letterImg.color.r, letterImg.color.g, letterImg.color.b, 1f), animDuration + 1f).SetEase(Ease.OutExpo);
        DOTween.To(()=> lightImg.color, x=>lightImg.color = x, new Color(lightImg.color.r, lightImg.color.g, lightImg.color.b, 1f), 0.4f).SetEase(Ease.OutExpo);
        
        RectTransform letterRT = ggLetter.GetComponent<RectTransform>();
        RectTransform lightRT = ggLight.GetComponent<RectTransform>();
        RectTransform squareRT = ggSquare.GetComponent<RectTransform>();

        DOTween.To(()=> letterRT.localPosition, x=>letterRT.localPosition = x, new Vector3(letterRT.localPosition.x, -50f, letterRT.localPosition.z), animDuration).SetEase(Ease.OutExpo);
        DOTween.To(()=> lightRT.localPosition, x=>lightRT.localPosition = x, new Vector3(lightRT.localPosition.x, 170f, lightRT.localPosition.z), animDuration).SetEase(Ease.OutExpo);
        DOTween.To(()=> squareRT.localPosition, x=>squareRT.localPosition = x, new Vector3(squareRT.localPosition.x, 500f, squareRT.localPosition.z), animDuration).SetEase(Ease.OutExpo);
        
        DOTween.To(()=> letterRT.sizeDelta, x=>letterRT.sizeDelta = x, new Vector2(753f, 950f), animDuration).SetEase(Ease.OutExpo);
        DOTween.To(()=> lightRT.sizeDelta, x=>lightRT.sizeDelta = x, new Vector2(710f, 190f), animDuration).SetEase(Ease.OutExpo);
        
        //Sequence seq = DOTween.Sequence();
        DOTween.To(()=> lightRT.localScale, x=>lightRT.localScale = x, new Vector3(0.4f, 0.4f, lightRT.localScale.z), animDuration).SetEase(Ease.OutCirc);
        //seq.Append(DOTween.To(()=> lightRT.localScale, x=>lightRT.localScale = x, new Vector3(0.8f, 0.8f, lightRT.localScale.z), 3f*animDuration/4f).SetEase(Ease.OutExpo));
        //seq.PrependInterval(animDuration/4f);
        //seq.Insert(0, transform.DOScale(new Vector3(3,3,3), seq.Duration()));
    }

    void Update() {
        timer += Time.deltaTime;

        if(timer > (afterAnimDelay + animDuration) && !firstAnimEnded){
            firstAnimEnded = true;
        }

        if(timer > (afterAnimDelay + animDuration + 0.7f)){
            secondAnimEnded = true;
        }

        if (firstAnimEnded && !startedSecondAnim && timer <= (afterAnimDelay + animDuration + 0.7f)){
            startedSecondAnim = true;

            //Hide firstScene
            Image letterImg = ggLetter.GetComponent<Image>();
            Image lightImg = ggLight.GetComponent<Image>();

            DOTween.To(()=> letterImg.color, x=>letterImg.color = x, new Color(letterImg.color.r, letterImg.color.g, letterImg.color.b, 0f), 0.6f).SetEase(Ease.OutExpo);
            DOTween.To(()=> lightImg.color, x=>lightImg.color = x, new Color(lightImg.color.r, lightImg.color.g, lightImg.color.b, 0f), 0.6f).SetEase(Ease.OutExpo); 

        }

        if(secondAnimEnded){
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
