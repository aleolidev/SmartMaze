using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SlideButton : MonoBehaviour
{
    public GameObject slider;

    public void executeSlide(){

        float sliderValue = slider.GetComponent<Slider>().value;

        if(sliderValue == 0f){
            DOTween.To(()=> slider.GetComponent<Slider>().value, x=>slider.GetComponent<Slider>().value = x, 1f, 0.3f).SetEase(Ease.OutExpo);
        } else {
            DOTween.To(()=> slider.GetComponent<Slider>().value, x=>slider.GetComponent<Slider>().value = x, 0f, 0.3f).SetEase(Ease.OutExpo);
        }
    }
}
