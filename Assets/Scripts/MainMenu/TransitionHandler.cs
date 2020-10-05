using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TransitionHandler : MonoBehaviour
{
    [Header("Transition Objects")]
    public GameObject wrap;
    [Header("Canvas")]
    public GameObject mainCanvas;
    public GameObject loadingScene;
    public IEnumerator LoadMainMenuFromLoading(){
        wrap.GetComponent<RectTransform>().localPosition = new Vector3(-18f, -2600f, 0f);
        DOTween.To(()=> wrap.GetComponent<RectTransform>().localPosition, x=> wrap.GetComponent<RectTransform>().localPosition = x, new Vector3(-18f, 0f, 0f), 0.7f).SetEase(Ease.OutSine);

        yield return new WaitForSecondsRealtime(1.2f);

        wrap.GetComponent<RectTransform>().localPosition = new Vector3(18f, 0f, 0f);
        mainCanvas.SetActive(true);
        loadingScene.SetActive(false);

        DOTween.To(()=> wrap.GetComponent<RectTransform>().localPosition, x=> wrap.GetComponent<RectTransform>().localPosition = x, new Vector3(18f, 2600f, 0f), 0.7f).SetEase(Ease.OutSine);
    }
}
