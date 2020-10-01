using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Firebase;
using Firebase.Auth;
using Google;

public class MainMenuHandler : MonoBehaviour
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
    public GameObject emailGO;
    public GameObject passwordGO;
    public GameObject usernameGO;
    public GameObject bLogin;
    public GameObject bEnter;
    public GameObject bLoginGoogle;
    public GameObject loginBakground;
    public GameObject warningText;

    //[Header("Login Google")]
    private string webClientId = "727163640903-bvdehltbo0ss6hduucltetb8nsfudscl.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    [Header("Username")]
    public TMP_InputField usernameText;
    public TMP_Text warningUsernameText;

    [Header("Loading Screen")]
    public float animDuration = 0.8f;

    public GameObject lBG;
    public GameObject lLogo;
    public GameObject lStatus;
    public GameObject lBrBG;
    public GameObject lBrMask;
    public GameObject lBrVoid;

    private float timer = 0f;

    private bool loginregisterState = false;

    private bool loadingScreenLoaded = false;

    private bool canSwapLoginMode = true;

    public const int maxUsernameLength = 16;
    private float updateStatusTimer = 0f;
    

    void Start(){
        passwordGO.GetComponent<TMP_InputField>().asteriskChar = '•';
    }

    void Update()
    {
        updateStatusTimer += Time.deltaTime;
        timer += Time.deltaTime;

        if(updateStatusTimer >= 1f){
            updateStatusTimer = 0f;

            if (auth.CurrentUser != null) {
                Debug.Log("CONECTADO");
            // User is signed in.
            } else {
                Debug.Log("DESCONECTADO");
            // No user is signed in.
            }
        }

        if(!loadingScreenLoaded && timer > 0.2f){
            loadingScreenLoaded = true;

            Color colorLStatus = lStatus.GetComponent<TextMeshProUGUI>().color; 
            Color colorLBG = lBG.GetComponent<Image>().color; 
            Color colorLLogo = lLogo.GetComponent<Image>().color; 
            Color colorLBrBG = lBrBG.GetComponent<Image>().color; 
            Color colorLBrMask = lBrMask.GetComponent<Image>().color; 
            Color colorLBrVoid = lBrVoid.GetComponent<Image>().color; 

            DOTween.To(()=> lStatus.GetComponent<TextMeshProUGUI>().color, x=>lStatus.GetComponent<TextMeshProUGUI>().color = x, new Color(colorLBG.r, colorLBG.g, colorLBG.b, 1f), animDuration).SetEase(Ease.OutExpo);
            DOTween.To(()=> lBG.GetComponent<Image>().color, x=>lBG.GetComponent<Image>().color = x, new Color(colorLLogo.r, colorLLogo.g, colorLLogo.b, 1f), animDuration).SetEase(Ease.OutExpo);
            DOTween.To(()=> lLogo.GetComponent<Image>().color, x=>lLogo.GetComponent<Image>().color = x, new Color(colorLStatus.r, colorLStatus.g, colorLStatus.b, 1f), animDuration).SetEase(Ease.OutExpo);
            DOTween.To(()=> lBrBG.GetComponent<Image>().color, x=>lBrBG.GetComponent<Image>().color = x, new Color(colorLBrBG.r, colorLBrBG.g, colorLBrBG.b, 1f), animDuration).SetEase(Ease.OutExpo);
            DOTween.To(()=> lBrMask.GetComponent<Image>().color, x=>lBrMask.GetComponent<Image>().color = x, new Color(colorLBrMask.r, colorLBrMask.g, colorLBrMask.b, 1f), animDuration).SetEase(Ease.OutExpo);
            DOTween.To(()=> lBrVoid.GetComponent<Image>().color, x=>lBrVoid.GetComponent<Image>().color = x, new Color(colorLBrVoid.r, colorLBrVoid.g, colorLBrVoid.b, 1f), animDuration).SetEase(Ease.OutExpo);
        }
        
    }

    void Awake()
    {
        configuration = new GoogleSignInConfiguration {
            WebClientId = webClientId,
            RequestEmail = true,
            RequestIdToken = false
        };
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
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
            }
        }
    }

    //Function for the login button
    public void LoginButton()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;
        
        if(loginregisterState == false){
            //Call the login coroutine passing the email and password
            StartCoroutine(Login(emailText.text, passwordText.text));
        } else {
            ChangeWindowState();
        }
        //Login(emailText.text, passwordText.text);
    }

    public void LoginWithGoogleButton(){
        OnGoogleSignIn();
    }
    //Function for the register button
    public void RegisterButton()
    {
        
        if(loginregisterState == true){
            //Call the register coroutine passing the email, password, and username
            StartCoroutine(Register(emailText.text, passwordText.text, usernameText.text));
        } else {
            ChangeWindowState();
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

                string message = "Login Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WrongPassword:
                        message = "Wrong Password";
                        break;
                    case AuthError.InvalidEmail:
                        message = "Invalid Email";
                        break;
                    case AuthError.UserNotFound:
                        message = "Account does not exist";
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
                warningLoginText.text = "Logged In";
            }
        }

    }

    private void SignInWithGoogleInFirebase(string idToken){
        Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
            newUser.DisplayName, newUser.UserId);
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
                        warningLoginText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        //UIManager.instance.LoginScreen();
                        warningLoginText.text = "Registrado correctamente";
                        //warningUsernameText.text = "Logueado con éxito";
                    }
                }
            }
        }
    }

    void ChangeWindowState(){
        float windowAnimDuration = 0.7f;

        if(canSwapLoginMode){
            if(loginregisterState == false){
                canSwapLoginMode = false;

                loginregisterState = true;
                usernameGO.SetActive(true);
                warningLoginText.text = "";

                RectTransform logBG = loginBakground.GetComponent<RectTransform>(); 
                RectTransform emailInpt = emailGO.GetComponent<RectTransform>();
                RectTransform passInpt = passwordGO.GetComponent<RectTransform>();
                RectTransform loginBtn = bLogin.GetComponent<RectTransform>();
                RectTransform enterBtn = bEnter.GetComponent<RectTransform>();
                RectTransform loginGBtn = bLoginGoogle.GetComponent<RectTransform>();
                RectTransform warningTxt = warningText.GetComponent<RectTransform>();

                DOTween.To(()=> loginBakground.GetComponent<RectTransform>().sizeDelta, x=>loginBakground.GetComponent<RectTransform>().sizeDelta = x, new Vector2(logBG.sizeDelta.x, 1320f), windowAnimDuration).SetEase(Ease.OutExpo);
                DOTween.To(()=> emailGO.GetComponent<RectTransform>().localPosition, x=>emailGO.GetComponent<RectTransform>().localPosition = x, new Vector3(emailInpt.localPosition.x, emailInpt.localPosition.y + 70f, emailInpt.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo);
                DOTween.To(()=> passwordGO.GetComponent<RectTransform>().localPosition, x=>passwordGO.GetComponent<RectTransform>().localPosition = x, new Vector3(passInpt.localPosition.x, passInpt.localPosition.y + 70f, passInpt.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo);
                DOTween.To(()=> bLogin.GetComponent<RectTransform>().localPosition, x=>bLogin.GetComponent<RectTransform>().localPosition = x, new Vector3(loginBtn.localPosition.x, loginBtn.localPosition.y - 70f, loginBtn.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo);
                DOTween.To(()=> bEnter.GetComponent<RectTransform>().localPosition, x=>bEnter.GetComponent<RectTransform>().localPosition = x, new Vector3(enterBtn.localPosition.x, enterBtn.localPosition.y - 70f, enterBtn.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo);
                DOTween.To(()=> bLoginGoogle.GetComponent<RectTransform>().localPosition, x=>bLoginGoogle.GetComponent<RectTransform>().localPosition = x, new Vector3(loginGBtn.localPosition.x, loginGBtn.localPosition.y - 70f, loginGBtn.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo);
                DOTween.To(()=> warningText.GetComponent<RectTransform>().localPosition, x=>warningText.GetComponent<RectTransform>().localPosition = x, new Vector3(warningTxt.localPosition.x, warningTxt.localPosition.y - 70f, warningTxt.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo);

                Color colorUsername = usernameGO.GetComponent<Image>().color;
                Color colorPlaceholder = usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color;
                Color colorUsernameText = usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color;

                Sequence seq = DOTween.Sequence();
                seq.Append(DOTween.To(()=> usernameGO.GetComponent<Image>().color, x=>usernameGO.GetComponent<Image>().color = x, new Color(colorUsername.r, colorUsername.g, colorUsername.b, 1f), windowAnimDuration).SetEase(Ease.OutExpo));
                seq.Join(DOTween.To(()=> usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color, x=>usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorPlaceholder.r, colorPlaceholder.g, colorPlaceholder.b, 0.5f), windowAnimDuration).SetEase(Ease.OutExpo));
                seq.Join(DOTween.To(()=> usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color, x=>usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorUsernameText.r, colorUsernameText.g, colorUsernameText.b, 1f), windowAnimDuration).SetEase(Ease.OutExpo));
                seq.PrependInterval(1f*windowAnimDuration/4f);
                
                usernameGO.GetComponent<TMP_InputField>().interactable = true;
                StartCoroutine(enableSwapIn(5f*windowAnimDuration/4f));

            } else {
                canSwapLoginMode = false;
                loginregisterState = false;
                warningLoginText.text = "";

                usernameGO.GetComponent<TMP_InputField>().interactable = false;

                Color colorUsername = usernameGO.GetComponent<Image>().color;
                Color colorPlaceholder = usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color;
                Color colorUsernameText = usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color;

                DOTween.To(()=> usernameGO.GetComponent<Image>().color, x=>usernameGO.GetComponent<Image>().color = x, new Color(colorUsername.r, colorUsername.g, colorUsername.b, 0f), windowAnimDuration).SetEase(Ease.OutExpo);
                DOTween.To(()=> usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color, x=>usernameGO.transform.Find("UsernameArea").Find("UsernamePlaceholder").GetComponent<TextMeshProUGUI>().color = x, new Color(colorPlaceholder.r, colorPlaceholder.g, colorPlaceholder.b, 0f), windowAnimDuration).SetEase(Ease.OutExpo);
                DOTween.To(()=> usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color, x=>usernameGO.transform.Find("UsernameArea").Find("UsernameText").GetComponent<TextMeshProUGUI>().color = x, new Color(colorUsernameText.r, colorUsernameText.g, colorUsernameText.b, 0f), windowAnimDuration).SetEase(Ease.OutExpo);

                RectTransform logBG = loginBakground.GetComponent<RectTransform>(); 
                RectTransform emailInpt = emailGO.GetComponent<RectTransform>();
                RectTransform passInpt = passwordGO.GetComponent<RectTransform>();
                RectTransform loginBtn = bLogin.GetComponent<RectTransform>();
                RectTransform enterBtn = bEnter.GetComponent<RectTransform>();
                RectTransform loginGBtn = bLoginGoogle.GetComponent<RectTransform>();
                RectTransform warningTxt = warningText.GetComponent<RectTransform>();

                Sequence seq = DOTween.Sequence();
                seq.Append(DOTween.To(()=> loginBakground.GetComponent<RectTransform>().sizeDelta, x=>loginBakground.GetComponent<RectTransform>().sizeDelta = x, new Vector2(logBG.sizeDelta.x, 1180f), windowAnimDuration).SetEase(Ease.OutExpo));
                seq.Join(DOTween.To(()=> emailGO.GetComponent<RectTransform>().localPosition, x=>emailGO.GetComponent<RectTransform>().localPosition = x, new Vector3(emailInpt.localPosition.x, emailInpt.localPosition.y - 70f, emailInpt.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo));
                seq.Join(DOTween.To(()=> passwordGO.GetComponent<RectTransform>().localPosition, x=>passwordGO.GetComponent<RectTransform>().localPosition = x, new Vector3(passInpt.localPosition.x, passInpt.localPosition.y - 70f, passInpt.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo));
                seq.Join(DOTween.To(()=> bLogin.GetComponent<RectTransform>().localPosition, x=>bLogin.GetComponent<RectTransform>().localPosition = x, new Vector3(loginBtn.localPosition.x, loginBtn.localPosition.y + 70f, loginBtn.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo));
                seq.Join(DOTween.To(()=> bEnter.GetComponent<RectTransform>().localPosition, x=>bEnter.GetComponent<RectTransform>().localPosition = x, new Vector3(enterBtn.localPosition.x, enterBtn.localPosition.y + 70f, enterBtn.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo));
                seq.Join(DOTween.To(()=> bLoginGoogle.GetComponent<RectTransform>().localPosition, x=>bLoginGoogle.GetComponent<RectTransform>().localPosition = x, new Vector3(loginGBtn.localPosition.x, loginGBtn.localPosition.y + 70f, loginGBtn.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo));
                seq.Join(DOTween.To(()=> warningText.GetComponent<RectTransform>().localPosition, x=>warningText.GetComponent<RectTransform>().localPosition = x, new Vector3(warningTxt.localPosition.x, warningTxt.localPosition.y + 70f, warningTxt.localPosition.z), windowAnimDuration).SetEase(Ease.OutExpo));
                seq.PrependInterval(1f*windowAnimDuration/4f);

                StartCoroutine(deactivateUsername(5f*windowAnimDuration/4f));
                StartCoroutine(enableSwapIn(5f*windowAnimDuration/4f));
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

    IEnumerator enableSwapIn(float time)
    {
        yield return new WaitForSeconds(time);
        canSwapLoginMode = true;
    }

    void OnDestroy() {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
}
