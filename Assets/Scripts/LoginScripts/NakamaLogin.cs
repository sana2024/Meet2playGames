using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ByteBrewSDK;
using Nakama.TinyJson;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif
#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif

public class NakamaLogin : MonoBehaviour
{
    //--------------
    // Nakama Connections and sessions
    //--------------

    [SerializeField] NakamaConnection Nconnect;
    public IClient iclient;
    public ISession isession;
    public ISocket isocket;
    

    //-------------
    // UI
    //-------------

    [SerializeField] Button PlatformBtn;
    [SerializeField] Sprite GoogleIcon;
    [SerializeField] Sprite AppleIcon;
    [SerializeField] Sprite EditorIcon;
    [SerializeField] public GameObject LoadingPanel;
 
    [SerializeField] GameObject ConnectionPanel;
    [SerializeField] GameObject DiceRotate;
    [SerializeField] GSPManager gspManager;

    public static NakamaLogin Instance;

    bool facebookcheck = false;

    [System.Obsolete]
    private void Start()
    {
        ByteBrew.NewCustomEvent("OpenedApp");

        // Initialize ByteBrew
        ByteBrew.InitializeByteBrew();
        ByteBrew.GetUserID();


        var IsTimerIntialized = PlayerPrefs.HasKey("time");

        if (IsTimerIntialized == false)
        {
 
            PlayerPrefs.SetInt("time", 10);
        }


       // iclient = Nconnect.client();
 
 
        if (Instance == null)
        {
            Instance = this;
        }

#if UNITY_ANDROID
        

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        PlatformBtn.image.sprite = GoogleIcon;
        
        PlatformBtn.onClick.AddListener(()=> { gspManager.GoogleSigin(); });

 
#endif

#if UNITY_IOS

        PlatformBtn.image.sprite = AppleIcon;
        PlatformBtn.onClick.AddListener(GamecenterLogin);


#endif

#if UNITY_EDITOR

        PlatformBtn.image.sprite = EditorIcon;

#endif


    }


    public void AutomaticLogins()
    {
 

        if (PlayerPrefs.GetString("login") == "guest")
        {
            OnGuestLogin();

        }
        else if (PlayerPrefs.GetString("login") == "gameCenter")
        {
#if UNITY_IOS

            GamecenterLogin();


#endif
        }
        else if (PlayerPrefs.GetString("login") == "googlePlay")
        {
#if UNITY_ANDROID


            GSPManager.Instance.GoogleSigin();

#endif
        }


        if (PlayerPrefs.GetString("login") == "facebook")

        {

            FacebookLogin.Instance.OnfacebookInit();

        }

    }

    public async void OnGuestLogin()
    {

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ConnectionPanel.SetActive(true);


            }
        else
        {

            ConnectionPanel.SetActive(false);
            string displayName = "";
            string username = "";
            string avatarUrl = "";

            var vars = new Dictionary<string, string>();
            vars["key"] = "value";
            vars["key2"] = "value2";

 
            LoadingPanel.SetActive(true);
 
            isession = await iclient.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier, create: true);
 

            var keepAliveIntervalSec = 30;
            isocket = Socket.From(iclient, new WebSocketAdapter(30));
            await isocket.ConnectAsync(isession, true , keepAliveIntervalSec);



            if (isession.Created)
            {
                
                
                
 
                displayName = "Player" + Random.Range(5000, 100000);
                username = displayName;
                avatarUrl = "https://i.pinimg.com/564x/70/5e/fb/705efbcfb56e45f52636d4e9f441a369.jpg";
                
                try
                {
                    var retryConfiguration = new Nakama.RetryConfiguration( 1,  5, delegate { System.Console.WriteLine("about to retry."); });

                    iclient.GlobalRetryConfiguration = retryConfiguration;

                    await iclient.UpdateAccountAsync(isession, username, displayName, avatarUrl, null, null, null , retryConfiguration);
                    isession = await iclient.SessionRefreshAsync(isession);
 

            }
                catch (Nakama.ApiResponseException ex)
                {
 
                    if (ex.Message == "Username is already in use.")
                    {
                        displayName = "Player" + Random.Range(5000, 100000);
                        username = displayName;
                        await iclient.UpdateAccountAsync(isession, username, displayName, avatarUrl, null, null);
                        isession = await iclient.SessionRefreshAsync(isession);
                    }
                }
 
                PassData.isocket = isocket;
                PassData.Username = isession.Username;
                PassData.DateAndTime = isession.CreateTime.ToString();
                PassData.MyURL = avatarUrl;
                PassData.iClient = iclient;
                PassData.isession = isession;
                PassData.ImageURL = avatarUrl;

                ByteBrew.NewCustomEvent("Registered", "type=Guest; Username=" + isession.Username + ";");
            }
            else
            {
                var account = await iclient.GetAccountAsync(isession);
                var user = account.User;
                displayName = user.DisplayName;
                username = displayName;
                Debug.Log("time " + isession.CreateTime);
                PassData.DateAndTime = user.CreateTime.ToString().Substring(0, 10);
                avatarUrl = "https://i.pinimg.com/564x/70/5e/fb/705efbcfb56e45f52636d4e9f441a369.jpg";
                await iclient.UpdateAccountAsync(isession, username, displayName, avatarUrl, null, null);

                PassData.isocket = isocket;
                PassData.Username = username;
                PassData.MyURL = avatarUrl;
                PassData.iClient = iclient;
                PassData.isession = isession;
                ByteBrew.NewCustomEvent("LoggedIn", "type=Guest; Username=" + isession.Username + ";");


            }

  
            PlayerPrefs.SetString("login", "guest");
            ChangeScene();
            LoadingPanel.SetActive(false);
 

        }
    }



#if UNITY_IOS
    public void GamecenterLogin()
    {

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ConnectionPanel.SetActive(true);


        }
        else
        {
            ConnectionPanel.SetActive(false);

            LoadingPanel.SetActive(true);
            GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
            Social.localUser.Authenticate(success =>
            {
                if (success)
                {
                    PlayerPrefs.SetString("login", "gameCenter");
                    var avatarUrl = "https://upload.wikimedia.org/wikipedia/de/8/83/Game_Center_Logo_iOS_7.png";
                    EmailLogin(Social.localUser.userName + "@gmail.com", Social.localUser.id, Social.localUser.userName, avatarUrl);
                }


                else
                    Debug.Log("Failed to authenticate");
            });



        }
    }

#endif

    public async void EmailLogin(string email, string password , string name , string avatarUrl)
        {
  
        isession = await iclient.AuthenticateEmailAsync(email, password);
        if (isession.Created)
        {
            ByteBrew.NewCustomEvent("Registered", "type=SocialPlatform; Username=" + isession.Username + ";");
        }
        else
        {
            ByteBrew.NewCustomEvent("LoggedIn", "type=SocialPlatform; Username=" + isession.Username + ";");
        }

 
            isession = await iclient.SessionRefreshAsync(isession);
 
        var keepAliveIntervalSec = 30;
            isocket = Socket.From(iclient, new WebSocketAdapter(keepAliveIntervalSec));
            await isocket.ConnectAsync(isession, true, keepAliveIntervalSec);

 
           var displayName = name;
           var username = displayName;
            await iclient.UpdateAccountAsync(isession, username, displayName, avatarUrl, null, null);

            PassData.isocket = isocket;
            PassData.Username = username;
            PassData.MyURL = avatarUrl;
            PassData.iClient = iclient;
            PassData.isession = isession;
            PassData.DateAndTime = isession.CreateTime.ToString();
 
            ByteBrew.NewCustomEvent("Login", "type=GameCenter_GooglePlay; Username=" + isession.Username + ";");
            ChangeScene();
            LoadingPanel.SetActive(false);


    }

 


    private void ChangeScene()
    {
        SceneManager.LoadScene("Menu");
    }


    public void Update()
    {
 

  #if UNITY_ANDROID
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
#endif


        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            ConnectionPanel.SetActive(false);
        }


        var speed = 1;

 
 
        

    }

    private void FixedUpdate()
    {
        var speed = 1;
         DiceRotate.transform.Rotate(Vector3.forward * speed);
    }





}
