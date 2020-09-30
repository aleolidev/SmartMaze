using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DetectSettingsButtonChecked : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool buttonPressed;
 
    public void OnPointerDown(PointerEventData eventData){
        RectTransform image = this.transform.Find("Image").GetComponent<RectTransform>();
        image.localPosition = new Vector3(image.localPosition.x,image.localPosition.y - 20f,image.localPosition.z);

        buttonPressed = true;
    }
    
    public void OnPointerUp(PointerEventData eventData){
        RectTransform image = this.transform.Find("Image").GetComponent<RectTransform>();
        image.localPosition = new Vector3(image.localPosition.x,image.localPosition.y + 20f,image.localPosition.z);

        buttonPressed = false;
    }
}
