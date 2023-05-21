using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Nakama;
using UnityEngine.UI;
using UnityEngine.Networking;
using Nakama.TinyJson;

public class SystemSettings : MonoBehaviour
{

    [SerializeField] GameObject SettingPanel;
    [SerializeField] public GameObject ConnectionPanel;

    [SerializeField] Button MusicToggle;
    [SerializeField] Button SoundToggle;

    [SerializeField] Sprite ToggleOn;
    [SerializeField] Sprite ToggleOff;

    [SerializeField] AudioSource sound;
    [SerializeField] AudioSource Music;

    [SerializeField] GameObject AdsPanel;

    [SerializeField] GameObject TutorialPanel;
    [SerializeField] GameObject DeleteComformation;

    public static SystemSettings instance;
    float timeToEndGame = 0;
 


    bool MusicMute = false;
    bool SoundMute = false;
    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void Start()
    {
        string muteSound = PlayerPrefs.GetString("muteSound");

        if (muteSound == "True")
        {
            SoundToggle.image.sprite = ToggleOff;
            sound.mute = true;
        }
        if (muteSound == "False")
        {
            SoundToggle.image.sprite = ToggleOn;
            sound.mute = false;
        }


 
        string muteMusic = PlayerPrefs.GetString("muteMusic");


        if (muteMusic == "True")
        {
            MusicToggle.image.sprite = ToggleOff;
            Music.mute = true;
        }
        if (muteMusic == "False")
        {
            MusicToggle.image.sprite = ToggleOn;
            Music.mute = false;
        }

    }

    public async void OnLeaveClicked()
    {
        PlayerPrefs.SetString("login", "");
        await PassData.iClient.SessionLogoutAsync(PassData.isession);
        SceneManager.LoadScene("Login");
    }

    public void OpenSettings()
    {
        SettingPanel.SetActive(true);

    }

    public void CloseSettings()
    {
        SettingPanel.SetActive(false);

    }

    public void Update()
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                ConnectionPanel.SetActive(true);

                timeToEndGame += Time.deltaTime;

                if (timeToEndGame > 16)
                {

                }

            }
            else
            {

            }
        }
 
    }


    public void onSoundClicked()
    {
        SoundMute = !SoundMute;

        PlayerPrefs.SetString("muteSound", SoundMute.ToString());
        string muteSound = PlayerPrefs.GetString("muteSound");

 
        sound.mute = SoundMute;

        if (muteSound == "True")
        {
            SoundToggle.image.sprite = ToggleOff;
 
        }
        if(muteSound == "False")
        {
            SoundToggle.image.sprite = ToggleOn;
 
        }
    }


    public void onMusicClicked()
    {

        MusicMute = !MusicMute;


        PlayerPrefs.SetString("muteMusic", MusicMute.ToString());
        string muteMusic = PlayerPrefs.GetString("muteMusic");

       
        Music.mute = MusicMute;

        if (muteMusic == "True")
        {
            MusicToggle.image.sprite = ToggleOff;
 
        }
        if (muteMusic == "False")
        {
            MusicToggle.image.sprite = ToggleOn;
  
        }
    }

    public void openAdsPanel()
    {
        AdsPanel.SetActive(true);

    }

    public void CloseAdsPanel()
    {
        AdsPanel.SetActive(false);
    }

    public void OpenTutorialPanel()
    {
        TutorialPanel.SetActive(true);
    }

    public void CloseTutorialPanel()
    {
        TutorialPanel.SetActive(false);
    }

    public void cancleDeletation()
    {
        DeleteComformation.SetActive(false);
    }

    public void OpenDeletePanel()
    {
        SettingPanel.SetActive(false);
        DeleteComformation.SetActive(true);

    }

    public void CloseTheApp()
    {
        Application.Quit();
    }


}
