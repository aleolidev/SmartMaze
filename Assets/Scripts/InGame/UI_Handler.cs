using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_Handler : MonoBehaviour
{
    public float openDialogDuration = 0.4f;
    public float closeDialogDuration = 0.4f;
    public GameObject semitransparentPlane;
    public GameObject path;
    public GameObject pauseBox;
    public GameObject reloadDialog;

    public Button pauseButton;
    public Button reloadButton;

    public Button reloadYesButton;
    public Button reloadNoButton;

    [SerializeField]
    public bool usingInterface = false;
    public GameObject SwipeHandler;

    public Slider sensibility;

    public Slider despSostenido;

    Swipe_Detector swipeDetector;

    float lastSensibility;

    float lastDespSost;

    // Start is called before the first frame update
    void Start()
    {  
        swipeDetector = SwipeHandler.GetComponent<Swipe_Detector>();

        sensibility.value = PlayerPrefs.GetFloat("sensibilidad");

        if(PlayerPrefs.GetInt("despSostenido") == 0){
            despSostenido.value = 0;
        } else {
            despSostenido.value = 1;
        }

        lastSensibility = sensibility.value;
        lastDespSost = despSostenido.value;

    }

    // Update is called once per frame
    void Update()
    {
        if(sensibility.value != lastSensibility){
            lastSensibility = sensibility.value;
            PlayerPrefs.SetFloat("sensibilidad", lastSensibility);
            swipeDetector.minDistanceForSwipe = lastSensibility;
        }

        if(lastDespSost != despSostenido.value){
            if(despSostenido.value == 0){
                PlayerPrefs.SetInt("despSostenido", 0);
                swipeDetector.moveJustOnRelease = true;
            } else {
                PlayerPrefs.SetInt("despSostenido", 1);
                swipeDetector.moveJustOnRelease = false;
            }
        }
        
    }

    public void reloadClicked(){

        if(semitransparentPlane.activeSelf == false && reloadDialog.activeSelf == false){
            usingInterface = true;
            SwipeHandler.GetComponent<Swipe_Detector>().someInterfaceEnabled = true;
            semitransparentPlane.SetActive(true);
            deactivateGroundButtons();
            openReloadConfirmation();
        }
    }

    public void pauseClicked(){

        if(semitransparentPlane.activeSelf == false && pauseBox.activeSelf == false){
            usingInterface = true;
            SwipeHandler.GetComponent<Swipe_Detector>().someInterfaceEnabled = true;
            semitransparentPlane.SetActive(true);
            deactivateGroundButtons();
            openMenuBox();
        }
    }

    void openReloadConfirmation(){
        //Animación del diálogo

        reloadDialog.transform.localScale = new Vector3(0.7f,0.7f,0.7f);
        reloadDialog.SetActive(true);
        DOTween.To(()=> reloadDialog.transform.localScale, x=>reloadDialog.transform.localScale = x, new Vector3(1,1,1), openDialogDuration).SetEase(Ease.OutElastic);
    }

    public void confirmReload(){
        path.GetComponent<Path_Handler>().ResetPath();
        closeMenuBox();
        //closeReloadConfirmation();
    }

    public void closeReloadConfirmation(){
        DOTween.To(()=> reloadDialog.transform.localScale, x=>reloadDialog.transform.localScale = x, new Vector3(0,0,0), closeDialogDuration).SetEase(Ease.OutCirc).OnComplete(() => {
            reloadDialog.SetActive(false);
            semitransparentPlane.SetActive(false);
            activateGroundButtons();
            usingInterface = false;
            SwipeHandler.GetComponent<Swipe_Detector>().someInterfaceEnabled = false;
        });
    }

    void openMenuBox(){
        //Animación de la caja
        pauseBox.transform.localScale = new Vector3(0.7f,0.7f,0.7f);
        pauseBox.SetActive(true);
        DOTween.To(()=> pauseBox.transform.localScale, x=>pauseBox.transform.localScale = x, new Vector3(1,1,1), openDialogDuration).SetEase(Ease.OutElastic);
    }

    public void closeMenuBox(){
        DOTween.To(()=> pauseBox.transform.localScale, x=>pauseBox.transform.localScale = x, new Vector3(0,0,0), closeDialogDuration).SetEase(Ease.OutCirc).OnComplete(() => {
            pauseBox.SetActive(false);
            semitransparentPlane.SetActive(false);
            activateGroundButtons();
            usingInterface = false;
            SwipeHandler.GetComponent<Swipe_Detector>().someInterfaceEnabled = false;
        });
    }

    void deactivateGroundButtons(){
        pauseButton.interactable = false;
        reloadButton.interactable = false;
    }

    void activateGroundButtons(){
        pauseButton.interactable = true;
        reloadButton.interactable = true;
    }

    public void setSizeOfButtonClicked(GameObject button){
        Debug.Log("PULSADO!");
        RectTransform rt = button.GetComponent<RectTransform>();
        Transform buttonText = button.transform.Find("Text");
        rt.localPosition = new Vector3(rt.localPosition.x, -60, rt.localPosition.z);
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 100);
    }
}
