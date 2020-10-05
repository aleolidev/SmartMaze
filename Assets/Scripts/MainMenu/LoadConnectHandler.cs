using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Firebase;
using Firebase.Auth;
using Google;

public class LoadConnectHandler : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;    
    public FirebaseUser user;

    [Header("Login/Register")]
    public GameObject loginBox;
    public TMP_InputField emailText;
    public TMP_InputField passwordText;
    public TMP_Text warningLoginText;

    [Header("Login/Register GameObjects")]
    public GameObject CompleteBoxGO;
    public GameObject emailGO;
    public GameObject passwordGO;
    public GameObject usernameGO;
    public GameObject bLogin;
    public GameObject bEnter;
    public GameObject bLoginGoogle;
    public GameObject bForgottenPassword;
    public GameObject bResetPassword;
    public GameObject bBack;
    public GameObject loginBakground;
    public GameObject skipLogin;
    public GameObject warningText;


    //[Header("Login Google")]
    private string webClientId = "727163640903-bvdehltbo0ss6hduucltetb8nsfudscl.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    [Header("Username")]
    public TMP_InputField usernameText;
    public TMP_Text warningUsernameText;

    [Header("Loading Screen")]
    public float animDuration = 0.8f;
    [Range(0, 100)]
    public int loadinStatus = 0;
    public GameObject loadingBar;
    public AnimationCurve barMoving;
    public GameObject completeLoadingGO;
    public GameObject lBG;
    public GameObject lLogo;
    public GameObject lStatus;
    public GameObject lBrBG;
    public GameObject lBrMask;
    public GameObject lBrVoid;
    public GameObject barFill;
    public GameObject barBorder;

    [Header("Scripts")]
    public TransitionHandler TransitionHndl;

    private float timer = 0f;
    private bool loadingScreenLoaded = false;
    public const int maxUsernameLength = 16;
    private float updateStatusTimer = 0f;

    private bool isOnline = false;

    enum States {
        Init,
        Login,
        Register,
        ForgottenPassword
    }

    private States sceneState = States.Init;
    

    void Start(){
        passwordGO.GetComponent<TMP_InputField>().asteriskChar = '•';
        loginBakground.GetComponent<Image>().color = new Color(loginBakground.GetComponent<Image>().color.r, loginBakground.GetComponent<Image>().color.g, loginBakground.GetComponent<Image>().color.b, 0f);
        HideAllSubElements(0f);
        Vector3 barFillPos = barFill.GetComponent<RectTransform>().localPosition;
        DOTween.To(()=> barFill.GetComponent<RectTransform>().localPosition, x=>barFill.GetComponent<RectTransform>().localPosition = x, new Vector3(47.55f, barFillPos.y, barFillPos.z), 1f).SetEase(barMoving).SetLoops(-1, LoopType.Restart);
        //StartCoroutine(loadInit());
    }

    void Update()
    {
        updateStatusTimer += Time.deltaTime;
        timer += Time.deltaTime;

        if(updateStatusTimer >= 1f){
            updateStatusTimer = 0f;

            /*
            if (auth.CurrentUser != null) {
                //Debug.Log("CONECTADO");
            // User is signed in.
            } else {
                //Debug.Log("DESCONECTADO");
            // No user is signed in.
            }*/
        }

        if(!loadingScreenLoaded && timer > 0.2f){
            loadingScreenLoaded = true;

            Color colorLBG = lBG.GetComponent<Image>().color; 
            Color colorLStatus = lStatus.GetComponent<TextMeshProUGUI>().color; 
            Color colorLLogo = lLogo.GetComponent<Image>().color; 
            Color colorLBrBG = lBrBG.GetComponent<Image>().color; 
            Color colorLBrMask = lBrMask.GetComponent<Image>().color; 
            Color colorLBrVoid = lBrVoid.GetComponent<Image>().color; 

            DOTween.To(()=> lStatus.GetComponent<TextMeshProUGUI>().color, x=>lStatus.GetComponent<TextMeshProUGUI>().color = x, new Color(colorLStatus.r, colorLStatus.g, colorLStatus.b, 1f), animDuration).SetEase(Ease.OutExpo);
            DOTween.To(()=> lBG.GetComponent<Image>().color, x=>lBG.GetComponent<Image>().color = x, new Color(colorLBG.r, colorLBG.g, colorLBG.b, 1f), animDuration).SetEase(Ease.OutExpo);
            DOTween.To(()=> lLogo.GetComponent<Image>().color, x=>lLogo.GetComponent<Image>().color = x, new Color(colorLLogo.r, colorLLogo.g, colorLLogo.b, 1f), animDuration).SetEase(Ease.OutExpo);
            DOTween.To(()=> lBrBG.GetComponent<Image>().color, x=>lBrBG.GetComponent<Image>().color = x, new Color(colorLBrBG.r, colorLBrBG.g, colorLBrBG.b, 1f), animDuration).SetEase(Ease.OutExpo);
            DOTween.To(()=> lBrMask.GetComponent<Image>().color, x=>lBrMask.GetComponent<Image>().color = x, new Color(colorLBrMask.r, colorLBrMask.g, colorLBrMask.b, 1f), animDuration).SetEase(Ease.OutExpo);
            DOTween.To(()=> lBrVoid.GetComponent<Image>().color, x=>lBrVoid.GetComponent<Image>().color = x, new Color(colorLBrVoid.r, colorLBrVoid.g, colorLBrVoid.b, 1f), animDuration).SetEase(Ease.OutExpo);
        }
        
    }

    // ANIMACIONES UI

    private void HideAllSubElements(float disableDuration){
        
        bLogin.GetComponent<Button>().interactable = false;
        bLoginGoogle.GetComponent<Button>().interactable = false;
        bEnter.GetComponent<Button>().interactable = false;
        bForgottenPassword.GetComponent<Button>().interactable = false;
        bResetPassword.GetComponent<Button>().interactable = false;
        bBack.GetComponent<Button>().interactable = false;
        skipLogin.GetComponent<Button>().interactable = false;
        emailGO.GetComponent<TMP_InputField>().interactable = false;
        passwordGO.GetComponent<TMP_InputField>().interactable = false;
        usernameGO.GetComponent<TMP_InputField>().interactable = false;

        Color colorLogin = bLogin.GetComponent<Image>().color;
        Color colorLoginGoogle = bLoginGoogle.GetComponent<Image>().color;
        Color colorRegister = bEnter.GetComponent<Image>().color;
        Color colorSkip = skipLogin.GetComponent<Image>().color;
        Color colorForgottenPassword = bForgottenPassword.GetComponent<Image>().color;
        Color colorResetPassword = bResetPassword.GetComponent<Image>().color;
        Color colorBack = bBack.GetComponent<Image>().color;
        Color colorEmail = emailGO.GetComponent<Image>().color;
        Color colorPassword = passwordGO.GetComponent<Image>().color;
        Color colorUsername = usernameGO.GetComponent<Image>().color;
        Color colorLoginText = bLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorLoginGoogleText = bLoginGoogle.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorRegisterText = bEnter.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorSkipText = skipLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorForgottenPasswordText = bForgottenPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorResetPasswordText = bResetPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorBackText = bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorEmailPlaceholderText = emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color;
        Color colorEmailText = emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color;
        Color colorPasswordPlaceholderText = passwordGO.transform.Find("PasswordArea").Find("PasswordPlaceholder").GetComponent<TextMeshProUGUI>().color;
        Color colorPasswordText = passwordGO.transform.Find("PasswordArea").Find("PasswordText").GetComponent<TextMeshProUGUI>().color;
        Color colorUsernamePlaceholderText = usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color;
        Color colorUsernameText = usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color;

        DOTween.To(()=> bLogin.GetComponent<Image>().color, x=> bLogin.GetComponent<Image>().color = x, new Color(colorLogin.r, colorLogin.g, colorLogin.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bLoginGoogle.GetComponent<Image>().color, x=> bLoginGoogle.GetComponent<Image>().color = x, new Color(colorLoginGoogle.r, colorLoginGoogle.g, colorLoginGoogle.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bEnter.GetComponent<Image>().color, x=> bEnter.GetComponent<Image>().color = x, new Color(colorRegister.r, colorRegister.g, colorRegister.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bForgottenPassword.GetComponent<Image>().color, x=> bForgottenPassword.GetComponent<Image>().color = x, new Color(colorForgottenPassword.r, colorForgottenPassword.g, colorForgottenPassword.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bResetPassword.GetComponent<Image>().color, x=> bResetPassword.GetComponent<Image>().color = x, new Color(colorResetPassword.r, colorResetPassword.g, colorResetPassword.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bBack.GetComponent<Image>().color, x=> bBack.GetComponent<Image>().color = x, new Color(colorBack.r, colorBack.g, colorBack.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> skipLogin.GetComponent<Image>().color, x=> skipLogin.GetComponent<Image>().color = x, new Color(colorSkip.r, colorSkip.g, colorSkip.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> emailGO.GetComponent<Image>().color, x=> emailGO.GetComponent<Image>().color = x, new Color(colorEmail.r, colorEmail.g, colorEmail.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> passwordGO.GetComponent<Image>().color, x=> passwordGO.GetComponent<Image>().color = x, new Color(colorPassword.r, colorPassword.g, colorPassword.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> usernameGO.GetComponent<Image>().color, x=> usernameGO.GetComponent<Image>().color = x, new Color(colorUsername.r, colorUsername.g, colorUsername.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorLoginText.r, colorLoginText.g, colorLoginText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bLoginGoogle.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bLoginGoogle.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorLoginGoogleText.r, colorLoginGoogleText.g, colorLoginGoogleText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bEnter.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bEnter.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorRegisterText.r, colorRegisterText.g, colorRegisterText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bForgottenPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bForgottenPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorForgottenPasswordText.r, colorForgottenPasswordText.g, colorForgottenPasswordText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bResetPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bResetPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorResetPasswordText.r, colorResetPasswordText.g, colorResetPasswordText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorBackText.r, colorBackText.g, colorBackText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> skipLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> skipLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorSkipText.r, colorSkipText.g, colorSkipText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color, x=> emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorEmailPlaceholderText.r, colorEmailPlaceholderText.g, colorEmailPlaceholderText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color, x=> emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorEmailText.r, colorEmailText.g, colorEmailText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> passwordGO.transform.Find("PasswordArea").Find("PasswordPlaceholder").GetComponent<TextMeshProUGUI>().color, x=> passwordGO.transform.Find("PasswordArea").Find("PasswordPlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorPasswordPlaceholderText.r, colorPasswordPlaceholderText.g, colorPasswordPlaceholderText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> passwordGO.transform.Find("PasswordArea").Find("PasswordText").GetComponent<TextMeshProUGUI>().color, x=> passwordGO.transform.Find("PasswordArea").Find("PasswordText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorPasswordText.r, colorPasswordText.g, colorPasswordText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color, x=> usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorUsernamePlaceholderText.r, colorUsernamePlaceholderText.g, colorUsernamePlaceholderText.b, 0f), disableDuration).SetEase(Ease.OutSine);
        DOTween.To(()=> usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color, x=> usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorUsernameText.r, colorUsernameText.g, colorUsernameText.b, 0f), disableDuration).SetEase(Ease.OutSine);
    }

    private IEnumerator loadInit(){
        //Login, Register, Login With Google, Skip
        sceneState = States.Init;

        CompleteBoxGO.SetActive(true);
        Color lbgColor = loginBakground.GetComponent<Image>().color;


        warningLoginText.text = "";

        //Espera que desaparezcan
        yield return new WaitForSecondsRealtime(0.15f);
        
        Vector2 logBG = loginBakground.GetComponent<RectTransform>().sizeDelta; 
        DOTween.To(()=> loginBakground.GetComponent<RectTransform>().sizeDelta, x=> loginBakground.GetComponent<RectTransform>().sizeDelta = x, new Vector2(logBG.x, 980), 1f).SetEase(Ease.OutExpo);

        yield return new WaitForSecondsRealtime(0.25f);

        bLogin.SetActive(true);
        bEnter.SetActive(true);
        bLoginGoogle.SetActive(true);
        bForgottenPassword.SetActive(false);
        bResetPassword.SetActive(false);
        bBack.SetActive(false);
        skipLogin.SetActive(true);
        emailGO.SetActive(false);
        passwordGO.SetActive(false);
        usernameGO.SetActive(false);

        //yield return new WaitForSecondsRealtime(0.25f);

        bLogin.GetComponent<RectTransform>().localPosition = new Vector3(bLogin.GetComponent<RectTransform>().localPosition.x, 210f, bLogin.GetComponent<RectTransform>().localPosition.z);
        bEnter.GetComponent<RectTransform>().localPosition = new Vector3(bEnter.GetComponent<RectTransform>().localPosition.x, 70f, bEnter.GetComponent<RectTransform>().localPosition.z);
        bLoginGoogle.GetComponent<RectTransform>().localPosition = new Vector3(bLoginGoogle.GetComponent<RectTransform>().localPosition.x, -70f, bLoginGoogle.GetComponent<RectTransform>().localPosition.z);
        skipLogin.GetComponent<RectTransform>().localPosition = new Vector3(skipLogin.GetComponent<RectTransform>().localPosition.x, -210f, skipLogin.GetComponent<RectTransform>().localPosition.z);

        warningLoginText.GetComponent<RectTransform>().localPosition = new Vector3(warningLoginText.GetComponent<RectTransform>().localPosition.x, -620f, warningLoginText.GetComponent<RectTransform>().localPosition.z);

        yield return new WaitForSecondsRealtime(0.05f);

        bLogin.GetComponent<Button>().interactable = true;
        bEnter.GetComponent<Button>().interactable = true;
        bLoginGoogle.GetComponent<Button>().interactable = true;
        skipLogin.GetComponent<Button>().interactable = true;

        Color colorBgBox = loginBakground.GetComponent<Image>().color;
        Color colorLogin = bLogin.GetComponent<Image>().color;
        Color colorLoginGoogle = bLoginGoogle.GetComponent<Image>().color;
        Color colorRegister = bEnter.GetComponent<Image>().color;
        Color colorSkip = skipLogin.GetComponent<Image>().color;
        Color colorLoginText = bLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorLoginGoogleText = bLoginGoogle.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorRegisterText = bEnter.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorSkipText = skipLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;

        DOTween.To(()=> loginBakground.GetComponent<Image>().color, x=> loginBakground.GetComponent<Image>().color = x, new Color(lbgColor.r, lbgColor.g, lbgColor.b, 1), 0.25f).SetEase(Ease.OutExpo);
        DOTween.To(()=> loginBakground.GetComponent<Image>().color, x=> loginBakground.GetComponent<Image>().color = x, new Color(colorBgBox.r, colorBgBox.g, colorBgBox.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bLogin.GetComponent<Image>().color, x=> bLogin.GetComponent<Image>().color = x, new Color(colorLogin.r, colorLogin.g, colorLogin.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bLoginGoogle.GetComponent<Image>().color, x=> bLoginGoogle.GetComponent<Image>().color = x, new Color(colorLoginGoogle.r, colorLoginGoogle.g, colorLoginGoogle.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bEnter.GetComponent<Image>().color, x=> bEnter.GetComponent<Image>().color = x, new Color(colorRegister.r, colorRegister.g, colorRegister.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> skipLogin.GetComponent<Image>().color, x=> skipLogin.GetComponent<Image>().color = x, new Color(colorSkip.r, colorSkip.g, colorSkip.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorLoginText.r, colorLoginText.g, colorLoginText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bLoginGoogle.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bLoginGoogle.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorLoginGoogleText.r, colorLoginGoogleText.g, colorLoginGoogleText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bEnter.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bEnter.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorRegisterText.r, colorRegisterText.g, colorRegisterText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> skipLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> skipLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorSkipText.r, colorSkipText.g, colorSkipText.b, 1f), 0.25f).SetEase(Ease.OutSine);

    
    }
    private IEnumerator loadLogin(){
        //Correo electrónico, contraseña, Iniciar sesión, He olvidado mi contraseña, Volver
        sceneState = States.Login;

        warningLoginText.text = "";

        //Espera que desaparezcan
        yield return new WaitForSecondsRealtime(0.15f);

        Vector2 logBG = loginBakground.GetComponent<RectTransform>().sizeDelta; 
        DOTween.To(()=> loginBakground.GetComponent<RectTransform>().sizeDelta, x=> loginBakground.GetComponent<RectTransform>().sizeDelta = x, new Vector2(logBG.x, 1170), 1f).SetEase(Ease.OutExpo);
        
        yield return new WaitForSecondsRealtime(0.25f);

        bLogin.SetActive(true);
        bEnter.SetActive(false);
        bLoginGoogle.SetActive(false);
        bForgottenPassword.SetActive(true);
        bResetPassword.SetActive(false);
        bBack.SetActive(true);
        skipLogin.SetActive(false);
        emailGO.SetActive(true);
        passwordGO.SetActive(true);
        usernameGO.SetActive(false);

        //yield return new WaitForSecondsRealtime(0.25f);

        emailGO.GetComponent<RectTransform>().localPosition = new Vector3(emailGO.GetComponent<RectTransform>().localPosition.x, 280f, emailGO.GetComponent<RectTransform>().localPosition.z);
        passwordGO.GetComponent<RectTransform>().localPosition = new Vector3(passwordGO.GetComponent<RectTransform>().localPosition.x, 140f, passwordGO.GetComponent<RectTransform>().localPosition.z);
        bLogin.GetComponent<RectTransform>().localPosition = new Vector3(bLogin.GetComponent<RectTransform>().localPosition.x, 0f, bLogin.GetComponent<RectTransform>().localPosition.z);
        bBack.GetComponent<RectTransform>().localPosition = new Vector3(bBack.GetComponent<RectTransform>().localPosition.x, -140f, bBack.GetComponent<RectTransform>().localPosition.z);
        bForgottenPassword.GetComponent<RectTransform>().localPosition = new Vector3(bForgottenPassword.GetComponent<RectTransform>().localPosition.x, -280f, bForgottenPassword.GetComponent<RectTransform>().localPosition.z);

        warningLoginText.GetComponent<RectTransform>().localPosition = new Vector3(warningLoginText.GetComponent<RectTransform>().localPosition.x, -700f, warningLoginText.GetComponent<RectTransform>().localPosition.z);

        yield return new WaitForSecondsRealtime(0.05f);

        bLogin.GetComponent<Button>().interactable = true;
        bForgottenPassword.GetComponent<Button>().interactable = true;
        bBack.GetComponent<Button>().interactable = true;
        emailGO.GetComponent<TMP_InputField>().interactable = true;
        passwordGO.GetComponent<TMP_InputField>().interactable = true;

        Color colorLogin = bLogin.GetComponent<Image>().color;
        Color colorForgottenPassword = bForgottenPassword.GetComponent<Image>().color;
        Color colorBack = bBack.GetComponent<Image>().color;
        Color colorEmail = emailGO.GetComponent<Image>().color;
        Color colorPassword = passwordGO.GetComponent<Image>().color;
        Color colorLoginText = bLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorForgottenPasswordText = bForgottenPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorBackText = bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorEmailPlaceholderText = emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color;
        Color colorEmailText = emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color;
        Color colorPasswordPlaceholderText = passwordGO.transform.Find("PasswordArea").Find("PasswordPlaceholder").GetComponent<TextMeshProUGUI>().color;
        Color colorPasswordText = passwordGO.transform.Find("PasswordArea").Find("PasswordText").GetComponent<TextMeshProUGUI>().color;

        DOTween.To(()=> bLogin.GetComponent<Image>().color, x=> bLogin.GetComponent<Image>().color = x, new Color(colorLogin.r, colorLogin.g, colorLogin.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bForgottenPassword.GetComponent<Image>().color, x=> bForgottenPassword.GetComponent<Image>().color = x, new Color(colorForgottenPassword.r, colorForgottenPassword.g, colorForgottenPassword.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bBack.GetComponent<Image>().color, x=> bBack.GetComponent<Image>().color = x, new Color(colorBack.r, colorBack.g, colorBack.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bLogin.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorLoginText.r, colorLoginText.g, colorLoginText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bForgottenPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bForgottenPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorForgottenPasswordText.r, colorForgottenPasswordText.g, colorForgottenPasswordText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorBackText.r, colorBackText.g, colorBackText.b, 1f), 0.25f).SetEase(Ease.OutSine);

        DOTween.To(()=> emailGO.GetComponent<Image>().color, x=> emailGO.GetComponent<Image>().color = x, new Color(colorEmail.r, colorEmail.g, colorEmail.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> passwordGO.GetComponent<Image>().color, x=> passwordGO.GetComponent<Image>().color = x, new Color(colorPassword.r, colorPassword.g, colorPassword.b, 1f), 0.25f).SetEase(Ease.OutSine);

        DOTween.To(()=> emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color, x=> emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorEmailPlaceholderText.r, colorEmailPlaceholderText.g, colorEmailPlaceholderText.b, 0.5f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color, x=> emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorEmailText.r, colorEmailText.g, colorEmailText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> passwordGO.transform.Find("PasswordArea").Find("PasswordPlaceholder").GetComponent<TextMeshProUGUI>().color, x=> passwordGO.transform.Find("PasswordArea").Find("PasswordPlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorPasswordPlaceholderText.r, colorPasswordPlaceholderText.g, colorPasswordPlaceholderText.b, 0.5f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> passwordGO.transform.Find("PasswordArea").Find("PasswordText").GetComponent<TextMeshProUGUI>().color, x=> passwordGO.transform.Find("PasswordArea").Find("PasswordText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorPasswordText.r, colorPasswordText.g, colorPasswordText.b, 1f), 0.25f).SetEase(Ease.OutSine);

    }

    private IEnumerator loadForgottenPassword(){
        //Correo electrónico, recuperar contraseña, volver
        sceneState = States.ForgottenPassword;

        warningLoginText.text = "";

        //Espera que desaparezcan
        yield return new WaitForSecondsRealtime(0.15f);

        Vector2 logBG = loginBakground.GetComponent<RectTransform>().sizeDelta; 
        DOTween.To(()=> loginBakground.GetComponent<RectTransform>().sizeDelta, x=> loginBakground.GetComponent<RectTransform>().sizeDelta = x, new Vector2(logBG.x, 800), 1f).SetEase(Ease.OutExpo);
        
        yield return new WaitForSecondsRealtime(0.25f);

        bLogin.SetActive(false);
        bEnter.SetActive(false);
        bLoginGoogle.SetActive(false);
        bForgottenPassword.SetActive(false);
        bResetPassword.SetActive(true);
        bBack.SetActive(true);
        skipLogin.SetActive(false);
        emailGO.SetActive(true);
        passwordGO.SetActive(false);
        usernameGO.SetActive(false);

        //yield return new WaitForSecondsRealtime(0.25f);

        emailGO.GetComponent<RectTransform>().localPosition = new Vector3(emailGO.GetComponent<RectTransform>().localPosition.x, 140f, emailGO.GetComponent<RectTransform>().localPosition.z);
        bResetPassword.GetComponent<RectTransform>().localPosition = new Vector3(bResetPassword.GetComponent<RectTransform>().localPosition.x, 0f, bResetPassword.GetComponent<RectTransform>().localPosition.z);
        bBack.GetComponent<RectTransform>().localPosition = new Vector3(bBack.GetComponent<RectTransform>().localPosition.x, -140f, bBack.GetComponent<RectTransform>().localPosition.z);
        
        warningLoginText.GetComponent<RectTransform>().localPosition = new Vector3(warningLoginText.GetComponent<RectTransform>().localPosition.x, -550f, warningLoginText.GetComponent<RectTransform>().localPosition.z);

        yield return new WaitForSecondsRealtime(0.05f);

        emailGO.GetComponent<TMP_InputField>().interactable = true;
        bResetPassword.GetComponent<Button>().interactable = true;
        bBack.GetComponent<Button>().interactable = true;

        Color colorEmail = emailGO.GetComponent<Image>().color;
        Color colorEmailPlaceholderText = emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color;
        Color colorEmailText = emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color;
        Color colorResetPassword = bResetPassword.GetComponent<Image>().color;
        Color colorResetPasswordText = bResetPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorBack = bBack.GetComponent<Image>().color;
        Color colorBackText = bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;

        DOTween.To(()=> emailGO.GetComponent<Image>().color, x=> emailGO.GetComponent<Image>().color = x, new Color(colorEmail.r, colorEmail.g, colorEmail.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color, x=> emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorEmailPlaceholderText.r, colorEmailPlaceholderText.g, colorEmailPlaceholderText.b, 0.5f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color, x=> emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorEmailText.r, colorEmailText.g, colorEmailText.b, 1f), 0.25f).SetEase(Ease.OutSine);

        DOTween.To(()=> bResetPassword.GetComponent<Image>().color, x=> bResetPassword.GetComponent<Image>().color = x, new Color(colorResetPassword.r, colorResetPassword.g, colorResetPassword.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bResetPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bResetPassword.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorResetPasswordText.r, colorResetPasswordText.g, colorResetPasswordText.b, 1f), 0.25f).SetEase(Ease.OutSine);

        DOTween.To(()=> bBack.GetComponent<Image>().color, x=> bBack.GetComponent<Image>().color = x, new Color(colorBack.r, colorBack.g, colorBack.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorBackText.r, colorBackText.g, colorBackText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        
    }

    private IEnumerator loadRegister(){
        //Correo electrónico, contraseña, usuario, registrarse, volver

        sceneState = States.Register;

        warningLoginText.text = "";

        //Espera que desaparezcan
        yield return new WaitForSecondsRealtime(0.15f);

        Vector2 logBG = loginBakground.GetComponent<RectTransform>().sizeDelta; 
        DOTween.To(()=> loginBakground.GetComponent<RectTransform>().sizeDelta, x=> loginBakground.GetComponent<RectTransform>().sizeDelta = x, new Vector2(logBG.x, 1230), 1f).SetEase(Ease.OutExpo);
        
        yield return new WaitForSecondsRealtime(0.25f);

        bLogin.SetActive(false);
        bEnter.SetActive(true);
        bLoginGoogle.SetActive(false);
        bForgottenPassword.SetActive(false);
        bResetPassword.SetActive(false);
        bBack.SetActive(true);
        skipLogin.SetActive(false);
        emailGO.SetActive(true);
        passwordGO.SetActive(true);
        usernameGO.SetActive(true);

        //yield return new WaitForSecondsRealtime(0.25f);

        emailGO.GetComponent<RectTransform>().localPosition = new Vector3(emailGO.GetComponent<RectTransform>().localPosition.x, 280f, emailGO.GetComponent<RectTransform>().localPosition.z);
        passwordGO.GetComponent<RectTransform>().localPosition = new Vector3(passwordGO.GetComponent<RectTransform>().localPosition.x, 140f, passwordGO.GetComponent<RectTransform>().localPosition.z);
        usernameGO.GetComponent<RectTransform>().localPosition = new Vector3(usernameGO.GetComponent<RectTransform>().localPosition.x, 0, usernameGO.GetComponent<RectTransform>().localPosition.z);
        bEnter.GetComponent<RectTransform>().localPosition = new Vector3(bEnter.GetComponent<RectTransform>().localPosition.x, -140f, bEnter.GetComponent<RectTransform>().localPosition.z);
        bBack.GetComponent<RectTransform>().localPosition = new Vector3(bBack.GetComponent<RectTransform>().localPosition.x, -280f, bBack.GetComponent<RectTransform>().localPosition.z);

        warningLoginText.GetComponent<RectTransform>().localPosition = new Vector3(warningLoginText.GetComponent<RectTransform>().localPosition.x, -745f, warningLoginText.GetComponent<RectTransform>().localPosition.z);

        yield return new WaitForSecondsRealtime(0.05f);

        emailGO.GetComponent<TMP_InputField>().interactable = true;
        passwordGO.GetComponent<TMP_InputField>().interactable = true;
        usernameGO.GetComponent<TMP_InputField>().interactable = true;
        bEnter.GetComponent<Button>().interactable = true;
        bBack.GetComponent<Button>().interactable = true;

        Color colorRegister = bEnter.GetComponent<Image>().color;
        Color colorBack = bBack.GetComponent<Image>().color;
        Color colorEmail = emailGO.GetComponent<Image>().color;
        Color colorPassword = passwordGO.GetComponent<Image>().color;
        Color colorUsername = usernameGO.GetComponent<Image>().color;
        Color colorRegisterText = bEnter.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorBackText = bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color;
        Color colorEmailPlaceholderText = emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color;
        Color colorEmailText = emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color;
        Color colorPasswordPlaceholderText = passwordGO.transform.Find("PasswordArea").Find("PasswordPlaceholder").GetComponent<TextMeshProUGUI>().color;
        Color colorPasswordText = passwordGO.transform.Find("PasswordArea").Find("PasswordText").GetComponent<TextMeshProUGUI>().color;
        Color colorUsernamePlaceholderText = usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color;
        Color colorUsernameText = usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color;

        DOTween.To(()=> bEnter.GetComponent<Image>().color, x=> bEnter.GetComponent<Image>().color = x, new Color(colorRegister.r, colorRegister.g, colorRegister.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bBack.GetComponent<Image>().color, x=> bBack.GetComponent<Image>().color = x, new Color(colorBack.r, colorBack.g, colorBack.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> emailGO.GetComponent<Image>().color, x=> emailGO.GetComponent<Image>().color = x, new Color(colorEmail.r, colorEmail.g, colorEmail.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> passwordGO.GetComponent<Image>().color, x=> passwordGO.GetComponent<Image>().color = x, new Color(colorPassword.r, colorPassword.g, colorPassword.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> usernameGO.GetComponent<Image>().color, x=> usernameGO.GetComponent<Image>().color = x, new Color(colorUsername.r, colorUsername.g, colorUsername.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bEnter.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bEnter.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorRegisterText.r, colorRegisterText.g, colorRegisterText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color, x=> bBack.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = x, new Color(colorBackText.r, colorBackText.g, colorBackText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color, x=> emailGO.transform.Find("EmailArea").Find("EmailPlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorEmailPlaceholderText.r, colorEmailPlaceholderText.g, colorEmailPlaceholderText.b, 0.5f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color, x=> emailGO.transform.Find("EmailArea").Find("EmailText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorEmailText.r, colorEmailText.g, colorEmailText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> passwordGO.transform.Find("PasswordArea").Find("PasswordPlaceholder").GetComponent<TextMeshProUGUI>().color, x=> passwordGO.transform.Find("PasswordArea").Find("PasswordPlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorPasswordPlaceholderText.r, colorPasswordPlaceholderText.g, colorPasswordPlaceholderText.b, 0.5f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> passwordGO.transform.Find("PasswordArea").Find("PasswordText").GetComponent<TextMeshProUGUI>().color, x=> passwordGO.transform.Find("PasswordArea").Find("PasswordText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorPasswordText.r, colorPasswordText.g, colorPasswordText.b, 1f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color, x=> usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorUsernamePlaceholderText.r, colorUsernamePlaceholderText.g, colorUsernamePlaceholderText.b, 0.5f), 0.25f).SetEase(Ease.OutSine);
        DOTween.To(()=> usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color, x=> usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorUsernameText.r, colorUsernameText.g, colorUsernameText.b, 1f), 0.25f).SetEase(Ease.OutSine);

    }
    
    public IEnumerator hideLoading(){

        Color colorLBG = loadingBar.GetComponent<Image>().color; 
        Color colorLStatus = lStatus.GetComponent<TextMeshProUGUI>().color; 
        Color colorLLogo = lLogo.GetComponent<Image>().color; 
        Color colorLBrBG = lBrBG.GetComponent<Image>().color; 
        Color colorLBrMask = lBrMask.GetComponent<Image>().color; 
        Color colorLBrVoid = lBrVoid.GetComponent<Image>().color;
        Color colorBarFill = barFill.GetComponent<Image>().color;
        Color colorBorderBar = barBorder.GetComponent<Image>().color;

        yield return new WaitForSecondsRealtime(0.05f);

        DOTween.To(()=> lStatus.GetComponent<TextMeshProUGUI>().color, x=>lStatus.GetComponent<TextMeshProUGUI>().color = x, new Color(colorLStatus.r, colorLStatus.g, colorLStatus.b, 0f), 0.85f).SetEase(Ease.OutExpo);
        DOTween.To(()=> loadingBar.GetComponent<Image>().color, x=>loadingBar.GetComponent<Image>().color = x, new Color(colorLBG.r, colorLBG.g, colorLBG.b, 0f), 0.85f).SetEase(Ease.OutExpo);
        DOTween.To(()=> lLogo.GetComponent<Image>().color, x=>lLogo.GetComponent<Image>().color = x, new Color(colorLLogo.r, colorLLogo.g, colorLLogo.b, 0f), 0.85f).SetEase(Ease.OutExpo);
        DOTween.To(()=> lBrBG.GetComponent<Image>().color, x=>lBrBG.GetComponent<Image>().color = x, new Color(colorLBrBG.r, colorLBrBG.g, colorLBrBG.b, 0f), 0.85f).SetEase(Ease.OutExpo);
        DOTween.To(()=> lBrMask.GetComponent<Image>().color, x=>lBrMask.GetComponent<Image>().color = x, new Color(colorLBrMask.r, colorLBrMask.g, colorLBrMask.b, 0f), 0.85f).SetEase(Ease.OutExpo);
        DOTween.To(()=> lBrVoid.GetComponent<Image>().color, x=>lBrVoid.GetComponent<Image>().color = x, new Color(colorLBrVoid.r, colorLBrVoid.g, colorLBrVoid.b, 0f), 0.85f).SetEase(Ease.OutExpo);
        DOTween.To(()=> barFill.GetComponent<Image>().color, x=>barFill.GetComponent<Image>().color = x, new Color(colorBarFill.r, colorBarFill.g, colorBarFill.b, 0f), 0.85f).SetEase(Ease.OutExpo);
        DOTween.To(()=> barBorder.GetComponent<Image>().color, x=>barBorder.GetComponent<Image>().color = x, new Color(colorBorderBar.r, colorBorderBar.g, colorBorderBar.b, 0f), 0.85f).SetEase(Ease.OutExpo);
    

        yield return new WaitForSecondsRealtime(0.85f);

        completeLoadingGO.SetActive(false);
    }

    public void backButtonPressed(){
        switch(sceneState){
            case States.Login:
                HideAllSubElements(0.4f);
                StartCoroutine(loadInit());
            break;
            case States.ForgottenPassword:
                HideAllSubElements(0.4f);
                StartCoroutine(loadLogin());
            break;
            case States.Register:
                HideAllSubElements(0.4f);
                StartCoroutine(loadInit());
            break;
        }
    }



    // FIN ANIMACIONES UI
    private void setBarStatus(int percent, string message){
        loadinStatus = percent;
        Vector3 loadingBarPosition = loadingBar.GetComponent<RectTransform>().localPosition;
        DOTween.To(()=> loadingBar.GetComponent<RectTransform>().localPosition, x=> loadingBar.GetComponent<RectTransform>().localPosition = x, new Vector3((100f-(float)percent)*0.01f*(-712f), loadingBarPosition.y, loadingBarPosition.y), 0.8f).SetEase(Ease.OutCirc);
        Debug.Log("Bar at " + percent + " at second " + timer);
        lStatus.GetComponent<TextMeshProUGUI>().text = message;
    }

    public IEnumerator connectionManager(){
        yield return StartCoroutine(tryConnection());

        if(isOnline){
            if (auth.CurrentUser != null) {
                //Está conectado
                //Debug.Log("CONECTADO");
                // User is signed in.
            } else {
                yield return StartCoroutine(hideLoading());
                yield return StartCoroutine(loadInit());
                //Debug.Log("DESCONECTADO");
            // No user is signed in.
            }
        }
    }

    public IEnumerator tryConnection(){
        yield return new WaitForSecondsRealtime(2f);
        setBarStatus(32, "CARGANDO...");
        yield return new WaitForSecondsRealtime(1.5f);
        setBarStatus(67, "CONECTANDO CON EL SERVIDOR");
        if(checkInternetConnection()){
            yield return new WaitForSecondsRealtime(0.8f);
            setBarStatus(83, "CONEXIÓN ESTABLECIDA");
            yield return new WaitForSecondsRealtime(0.4f);
            setBarStatus(100, "CONEXIÓN ESTABLECIDA");
            isOnline = true;
            Debug.Log("Success pinging Google");
            InitializeFirebase();
        } else{
            yield return new WaitForSecondsRealtime(5f);
            setBarStatus(79, "CONECTANDO CON EL SERVIDOR");
            //Try with Adobe
            Debug.Log("Error pinging Google");
            if(checkInternetConnection()){
                yield return new WaitForSecondsRealtime(0.8f);
                setBarStatus(83, "CONEXIÓN ESTABLECIDA");
                yield return new WaitForSecondsRealtime(0.4f);
                setBarStatus(100, "CONEXIÓN ESTABLECIDA");
                Debug.Log("Success pinging Google 2nd time");
                isOnline = true;
                InitializeFirebase();
            } else {
                //Insertar animación de fallo wifi


                yield return new WaitForSecondsRealtime(0.7f);
                isOnline = false;
                Debug.Log("Error pinging Google 2nd time");

                setBarStatus(100, "ERROR DE CONEXIÓN");
            }
        }
    }

    void Awake()
    {
        configuration = new GoogleSignInConfiguration {
            WebClientId = webClientId,
            RequestEmail = true,
            RequestIdToken = false
        };

        StartCoroutine(connectionManager());


        //Check that all of the necessary dependencies for Firebase are present on the system
        /*FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                Debug.Log("llega");
                if(isOnline){
                    InitializeFirebase();
                }
                //StartCoroutine(tryConnection());
                Debug.Log("llega2");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
                setBarStatus(60);
            }
        });*/
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);

        if (auth.CurrentUser != null) {
            StartCoroutine(TransitionHndl.LoadMainMenuFromLoading());
        }
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs) {
        if (auth.CurrentUser != user) {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null) {
                Debug.Log("Signed out " + user.DisplayName);
            }
            user = auth.CurrentUser;
            if (signedIn) {
                Debug.Log("Signed in " + user.DisplayName);
                //StartCoroutine(TransitionHndl.LoadMainMenuFromLoading());
            }
        }
    }

    //Function for the login button
    public void LoginButton()
    {
        if(sceneState == States.Init){
            HideAllSubElements(0.4f);
            StartCoroutine(loadLogin());
        } else if (sceneState == States.Login) {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = true;
            GoogleSignIn.Configuration.RequestIdToken = false;
            
            StartCoroutine(Login(emailText.text, passwordText.text));
        }
        //Login(emailText.text, passwordText.text);
    }

    public void LoginWithGoogleButton(){
        OnGoogleSignIn();
    }
    //Function for the register button
    public void RegisterButton()
    {
        if(sceneState == States.Init){
            HideAllSubElements(0.4f);
            StartCoroutine(loadRegister());
        } else {
            StartCoroutine(Register(emailText.text, passwordText.text, usernameText.text));
        }
    }

    public void skipRegisterButton(){
        StartCoroutine(TransitionHndl.LoadMainMenuFromLoading());
    }

    public void forgottenPasswordButton(){
        if(sceneState == States.Login){
            HideAllSubElements(0.4f);
            StartCoroutine(loadForgottenPassword());
        }
    }

    public void resetPasswordButton(){
        /*Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null && ) {
            user.SendEmailVerificationAsync().ContinueWith(task => {
                if (task.IsCanceled) {
                    Debug.LogError("SendEmailVerificationAsync was canceled.");
                    warningLoginText.text = "Se ha cancelado la recuperación de la contraseña.";
                return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("SendEmailVerificationAsync encountered an error: " + task.Exception);
                    warningLoginText.text = "No se ha podido enviar un correo de verificación para reestablecer la contraseña.";
                return;
                }
                Debug.Log("Email sent successfully.");
                warningLoginText.text = "Se te ha enviado un mensaje a tu correo electrónico para reestablecer tu contraseña.";
            });
        }*/
        string emailAddress = emailText.text;
        if (!String.IsNullOrEmpty(emailAddress) && !String.IsNullOrWhiteSpace(emailAddress)) {
            auth.SendPasswordResetEmailAsync(emailAddress).ContinueWith(task => {
                if (task.IsCanceled) {
                    Debug.LogError("SendPasswordResetEmailAsync was canceled.");
                    warningLoginText.text = "Se ha cancelado la recuperación de la contraseña.";
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
                    warningLoginText.text = "No se ha podido enviar un correo de verificación para reestablecer la contraseña.";
                    return;
                }

                Debug.Log("Password reset email sent successfully.");
                warningLoginText.text = "Se ha enviado un mensaje a tu correo electrónico para reestablecer la contraseña.";
            });
        } else {
            warningLoginText.text = "Introduce un email válido";
        }

    }

    public void LogOutButton(){
        Debug.Log("Logout");
        auth.SignOut();
    }

    private IEnumerator Login(string _email, string _password)
    {
        //warningLoginText.text = "INIT\n";
        
        if(_password == ""){
            warningLoginText.text = "Rellena el campo contraseña\n";
        }
        if(_email == ""){
            warningLoginText.text = "Rellena el campo email\n";
        }
        if(_email != "" && _password != "") 
        {
            var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

            if (LoginTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
                FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Error intentando iniciar sesisón";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Email inválido";
                        break;
                    case AuthError.MissingPassword:
                        message = "Contraseña incorrecta";
                        break;
                    case AuthError.WrongPassword:
                        message = "Contraseña incorrecta";
                        break;
                    case AuthError.InvalidEmail:
                        message = "Email inválido";
                        break;
                    case AuthError.UserNotFound:
                        message = "La cuenta especificada no existe";
                        break;
                }
                warningLoginText.text = message;
            }
            else
            {
                //User is now logged in
                //Now get the result
                user = LoginTask.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
                warningLoginText.text = "Inicio de sesión exitoso";
                StartCoroutine(TransitionHndl.LoadMainMenuFromLoading());
            }
        }

    }

    private void SignInWithGoogleInFirebase(string idToken){
        Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                warningLoginText.text = "Se ha cancelado el inicio de sesión.";
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                warningLoginText.text = "Error durante el inicio de sesión con una cuenta de Google.";
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            StartCoroutine(TransitionHndl.LoadMainMenuFromLoading());
        });
    }

    internal void OnGoogleAuthenticationFinished(Task<GoogleSignInUser> task) {
        if (task.IsFaulted) {
            using (IEnumerator<System.Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator()) {
                if (enumerator.MoveNext()) {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.LogError("Got Error: " + error.Status + " " + error.Message);
                } else {
                    Debug.LogWarning("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        } else if(task.IsCanceled) {
            Debug.LogWarning("Canceled");
        } else  {
            Debug.Log("Welcome: " + task.Result.DisplayName + "!");
            SignInWithGoogleInFirebase(task.Result.IdToken);
            StartCoroutine(TransitionHndl.LoadMainMenuFromLoading());
        }
    }

    public void OnGoogleSignIn() {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticationFinished);
    }

    public void OnGoogleSignInSilently() {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnGoogleAuthenticationFinished);
    }

    public void OnGoogleSignOut() {
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnGoogleDisconnect() {
      GoogleSignIn.DefaultInstance.Disconnect();
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;
        
        if(_username.Length >= maxUsernameLength){
            warningLoginText.text = "El nombre de usuario es demasiado largo\n";
        }
        if (_username == ""){
            warningLoginText.text = "Rellena el campo usuario\n";
        }
        if(_password == ""){
            warningLoginText.text = "Rellena el campo contraseña\n";
        }
        if(_email == ""){
            warningLoginText.text = "Rellena el campo email\n";
        }
        if(_email != "" && _password != "" && _username != "" && _username.Length < maxUsernameLength)
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "¡Error en el registro!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Introduce un email válido.";
                        break;
                    case AuthError.MissingPassword:
                        message = "Introduce una contraseña válida";
                        break;
                    case AuthError.WeakPassword:
                        message = "Esta contraseña es demasiado débil.";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Ya existe una cuenta con este email.";
                        break;
                }
                warningLoginText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                user = RegisterTask.Result;

                if (user != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile{DisplayName = _username};

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = user.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningLoginText.text = "¡Error con el nombre de usuario!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        //UIManager.instance.LoginScreen();
                        warningLoginText.text = "Registrado correctamente";
                        StartCoroutine(TransitionHndl.LoadMainMenuFromLoading());
                        //warningUsernameText.text = "Logueado con éxito";
                    }
                }
            }
        }
    }

    public void onFocusInputFiled(){
        Vector3 rt = loginBox.GetComponent<RectTransform>().localPosition;
        DOTween.To(()=> loginBox.GetComponent<RectTransform>().localPosition, x=>loginBox.GetComponent<RectTransform>().localPosition = x, new Vector3(rt.x, 400f, rt.z), 0.8f).SetEase(Ease.OutExpo);
    }

    public void onLoseFocusInputField(){
        Vector3 rt = loginBox.GetComponent<RectTransform>().localPosition;
        DOTween.To(()=> loginBox.GetComponent<RectTransform>().localPosition, x=>loginBox.GetComponent<RectTransform>().localPosition = x, new Vector3(rt.x, 0f, rt.z), 0.8f).SetEase(Ease.OutExpo);
    }

    IEnumerator deactivateUsername(float time)
    {
        yield return new WaitForSeconds(time);
        usernameGO.SetActive(false);
    }

    void OnDestroy() {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    private bool checkInternetConnection(){
        string HtmlText = GetHtmlFromUri("http://google.com");
        if(string.IsNullOrEmpty(HtmlText))
        {
            return false;
            //No connection
        }
        else if(!HtmlText.Contains("schema.org/WebPage"))
        {
            //Redirecting since the beginning of googles html contains that 
            //phrase and it was not found
            return false;
        }
        else
        {
            //success
            return true;
        }
    } 

    public string GetHtmlFromUri(string resource){
        string html = string.Empty;
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(resource);
        try
        {
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                if (isSuccess)
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        //We are limiting the array to 80 so we don't have
                        //to parse the entire html document feel free to 
                        //adjust (probably stay under 300)
                        char[] cs = new char[80];
                        reader.Read(cs, 0, cs.Length);
                        foreach(char ch in cs)
                        {
                            html +=ch;
                        }
                    }
                }
            }
        } catch
        {
            return "";
        }
        return html;
    }
}
