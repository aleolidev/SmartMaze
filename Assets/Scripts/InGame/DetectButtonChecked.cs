using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DetectButtonChecked : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool buttonPressed;
 
    public void OnPointerDown(PointerEventData eventData){
        RectTransform buttonText = this.transform.Find("Text").GetComponent<RectTransform>();
        buttonText.localPosition = new Vector3(buttonText.localPosition.x,buttonText.localPosition.y - 20f,buttonText.localPosition.z);

        buttonPressed = true;
    }
    
    public void OnPointerUp(PointerEventData eventData){
        RectTransform buttonText = this.transform.Find("Text").GetComponent<RectTransform>();
        buttonText.localPosition = new Vector3(buttonText.localPosition.x,buttonText.localPosition.y + 20f,buttonText.localPosition.z);

        buttonPressed = false;
    }
}
