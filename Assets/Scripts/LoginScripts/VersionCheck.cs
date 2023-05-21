using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;
 
using UnityEngine.UI;


public class VersionCheck : MonoBehaviour
{

    public string MyVersion;

    public string MyIOSVersion;
    public string MyAndroidVersion;

    private string IOSVersion;
    private string AndroidVersion;

    public struct userAttributes { }
    public struct appAttributes { }
    private string Version;
    [SerializeField] GameObject VersionCheckPanel;
    [SerializeField] Text ThisVersion;
    [SerializeField] Text LatestVersion;
    [SerializeField] Sprite AppStoreSprite;
    [SerializeField] Sprite GoggleSprite;
    [SerializeField] Button RedirectButton;
    [SerializeField] GameObject NoInternetPanel;
    public static VersionCheck Instance;

    public bool IsUpToDate = false;


    string AppStoreURL = "https://apps.apple.com/us/app/meet-2-play/id1594208918";
    string GooglePlayStoreURL = "https://play.google.com/store/apps/details?id=com.meet2play.v1.com";

    [System.Obsolete]
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        GetVersion();

    }

    [System.Obsolete]
     public void GetVersion()
    {

       
      ConfigManager.FetchCompleted += SetValues;
        ConfigManager.FetchConfigs<userAttributes, appAttributes>
                 (new userAttributes(), new appAttributes());

    

    }


    [System.Obsolete]
    private void Start()
    {
 

#if UNITY_IOS

        RedirectButton.image.sprite = AppStoreSprite;

#endif

#if UNITY_ANDROID
        RedirectButton.image.sprite = GoggleSprite;

#endif

    }

    [System.Obsolete]
    void SetValues(ConfigResponse response)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            NoInternetPanel.SetActive(true);
        }
        else
        {

            Version = ConfigManager.appConfig.GetString("Version");
            IOSVersion = ConfigManager.appConfig.GetString("IOSVersion");
            AndroidVersion = ConfigManager.appConfig.GetString("AndroidVersion");


#if UNITY_IOS

            ThisVersion.text = MyIOSVersion;
            LatestVersion.text = IOSVersion;
            Debug.Log("os version " + IOSVersion);
            if (MyIOSVersion == IOSVersion)
            {
                IsUpToDate = true;
                VersionCheckPanel.SetActive(false);
                Debug.Log("UP TO DATE");
                NakamaLogin.Instance.AutomaticLogins();

            }
            else
            {
                IsUpToDate = false;
                VersionCheckPanel.SetActive(true);
                Debug.Log("UPDATE NEEDED");
            }
#endif


#if UNITY_ANDROID

        ThisVersion.text = MyAndroidVersion;
        LatestVersion.text = AndroidVersion;

        if (MyAndroidVersion == AndroidVersion)
        {
            VersionCheckPanel.SetActive(false);
            Debug.Log("UP TO DATE");
            NakamaLogin.Instance.AutomaticLogins();
        }
        else
        {
            IsUpToDate = false;
            VersionCheckPanel.SetActive(true);
            Debug.Log("UPDATE NEEDED");
        }

#endif
        }
 
    }

    public void UpdateClicked()
    {
#if UNITY_IOS

        Application.OpenURL(AppStoreURL);

#endif


#if UNITY_ANDROID

      Application.OpenURL(GooglePlayStoreURL);

#endif
    }

    public void CloseTheApp()
    {
        Application.Quit();
    }
}
