using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using UnityEngine.UI;
using Nakama.TinyJson;
using UnityEngine.SceneManagement;
using Facebook.Unity;
using UnityEngine.Networking;
using ByteBrewSDK;

public class AIGameResult : MonoBehaviour
{
    [SerializeField] GameObject EndScreenPanel;
    [SerializeField] Image ResultImage;
    [SerializeField] Sprite WinnerImage;
    [SerializeField] Sprite WhiteLooserImage;
    [SerializeField] Sprite BlackLooserImage;
    [SerializeField] Sprite DrawImage;
    [SerializeField] Sprite RedBackground;
    [SerializeField] Sprite GreenBackground;
    [SerializeField] Sprite YellowBackground;
    [SerializeField] GameObject Reward;
    [SerializeField] Image EndScreenBackground;
    [SerializeField] RawImage MyAvatar;
    [SerializeField] RawImage OpponentAvatar;
    [SerializeField] Text Rewardamount;

    [SerializeField] Text MyRatingText;
    [SerializeField] Text OpponentRatingText;
    [SerializeField] Text MyLevelText;
    [SerializeField] Text OppoenentLevelText;
    [SerializeField] Text MyWinnerScore;
    [SerializeField] Text AIWinnerScore;
    public static  AIGameResult Instance;
    [SerializeField] Text DrawTypeText;
    [SerializeField] GameObject DrawType;
    [SerializeField] Button LeaveButton;
    [SerializeField] GameObject FreezeGame;
    [SerializeField] Button RematchButton;
    [SerializeField] GameObject LeavePanel;
    [SerializeField] GameObject OfferDrawPanel;
    [SerializeField] Button DrawButton;
    [SerializeField] Button RequestDrawButton;
    [SerializeField] GameObject RejectDrawPanel;
    [SerializeField] GameObject RejectRematchPanel;
    [SerializeField] Button AddFriendButton;
    [SerializeField] Sprite FriendAddedSprite;


    bool IsLooseRunned = false;
    bool IsDrawRunned = false;


    [SerializeField] GameObject NoConnectionPanel;

    //nakama variables
    ISocket isocket;
    ISession isession;
    IClient iclient;

    // Start is called before the first frame update
    void Start()
    {
       // LeaveButton.onClick.AddListener(AILooserResult);
        iclient = PassData.iClient;
        isession = PassData.isession;
        isocket = PassData.isocket;

        LeaveButton.onClick.AddListener(OpenLeavePanel);

        if (Instance == null)
        {
            Instance = this;
        }


        MyRatingText.text = PassData.ChessELO.ToString();
        MyLevelText.text = PassData.ChessLevel.ToString();
        OpponentRatingText.text = PassData.AIRating.ToString();
        OppoenentLevelText.text = PassData.AIlevel.ToString();


        StartCoroutine(GetTexture(PassData.MyURL , MyAvatar));
        StartCoroutine(GetTexture("https://i.pinimg.com/564x/bc/7f/80/bc7f8058b40eaf9118e762830db84e3e.jpg", MyAvatar));

    }

    public void AIWinnerResult()
    {
        ByteBrew.NewCustomEvent("Won", "Game=Chess; Type=AI; Username=" + isession.Username + ";");
        huggingFaceStock.Instance.skill = "";
        ChessTimer.Instance.gameEnded = true;
        ChessBoardAI.instance.isWhiteTurn = true;
        EndScreenPanel.SetActive(true);
        ResultImage.sprite = WinnerImage;
        EndScreenBackground.sprite = GreenBackground;
        PassData.ChessWins++;
        Rewardamount.text = "200";
        PassData.ChessELO = calculateEloChess(1);
        PassData.AIRating = CalculateAIRating(0);
        if(PassData.BotLeveling != 20)
        {
          PassData.BotLeveling++;
        }

        MyWinnerScore.text = "1";
        LeaveButton.onClick.RemoveAllListeners();
        LeaveButton.onClick.AddListener(LeaveGame);
        RequestDrawButton.interactable = false;
        string Rating = PlayerPrefs.GetString("progress");
        Rating = Rating + " " + PassData.ChessELO;
        PlayerPrefs.SetString("progress", Rating);
        //ChessTimer.Instance.OpponentTimerText.text = "00:00";
        //ChessTimer.Instance.MyTimerText.text = "00:00";
        updateWallet(200 , 25);
        //UpdateXP(25);
        WriteData();
        AIfakeCam.Instance.MuteLocalVideo();
        AIfakeCam.Instance.StopWebCam();
        //PassData.AIlevel++;
        //PassData.AIRating += 50;

    }

    public void AILooserResult(Sprite LooserSprite)
    {
        ByteBrew.NewCustomEvent("Lost", "Game=Chess; Type=AI; Username=" + isession.Username + ";");
        LeavePanel.SetActive(false);
        huggingFaceStock.Instance.skill = "";
        ChessTimer.Instance.gameEnded = true;
        ChessBoardAI.instance.isWhiteTurn = true;
        if (IsLooseRunned == false)
        {
            Debug.Log("looser result");
            EndScreenPanel.SetActive(true);
            EndScreenBackground.sprite = RedBackground;
            ResultImage.sprite = LooserSprite;
            PassData.ChessLooses++;
            PassData.ChessELO = calculateEloChess(0);
            PassData.AIRating = CalculateAIRating(1);
            RequestDrawButton.interactable = false;
            if (PassData.BotLeveling != 1)
            {
              PassData.BotLeveling--;
            }
            AIWinnerScore.text = "1";
            LeaveButton.onClick.RemoveAllListeners();
            LeaveButton.onClick.AddListener(LeaveGame);
            string Rating = PlayerPrefs.GetString("progress");
            Rating = Rating + " " + PassData.ChessELO;
            PlayerPrefs.SetString("progress", Rating);
           // ChessTimer.Instance.OpponentTimerText.text = "00:00";
           // ChessTimer.Instance.MyTimerText.text = "00:00";

            WriteData();
            AIfakeCam.Instance.MuteLocalVideo();
            AIfakeCam.Instance.StopWebCam();
            IsLooseRunned = true;
            //PassData.AIlevel--;
            //PassData.AIRating -= 50;
        }

    }

    public void DrawResult(string DrawString)
    {
        ByteBrew.NewCustomEvent("Draw", "Game=Chess; Type=AI; Username=" + isession.Username + ";");
        if (IsDrawRunned == false)
        {
            huggingFaceStock.Instance.skill = "";
            ChessTimer.Instance.gameEnded = true;
            ChessBoardAI.instance.isWhiteTurn = true;
            Reward.SetActive(true);
            ResultImage.sprite = DrawImage;
            EndScreenBackground.sprite = YellowBackground;
            EndScreenPanel.SetActive(true);
            AIWinnerScore.text = "1";
            MyWinnerScore.text = "1";
            PassData.ChessDraws++;
            if (PassData.BotLeveling != 20)
            {
                PassData.BotLeveling++;
            }
            Rewardamount.text = "100";
            DrawType.SetActive(true);
            DrawTypeText.text = DrawString;
            RequestDrawButton.interactable = false;
            LeaveButton.onClick.RemoveAllListeners();
            LeaveButton.onClick.AddListener(LeaveGame);
           // ChessTimer.Instance.OpponentTimerText.text = "00:00";
           // ChessTimer.Instance.MyTimerText.text = "00:00";
            updateWallet(100 , 15);
           // UpdateXP(15);
            WriteData();
            IsDrawRunned = true;
            //PassData.AIlevel++;
            //PassData.AIRating += 50;
        }

    }

    public async void updateWallet(int coins , int xp)
    {


        var payload = JsonWriter.ToJson(new { coins = coins , xp = xp});
        var rpcid = "Update_Wallet";
        var WalletRPC = await PassData.iClient.RpcAsync(PassData.isession, rpcid, payload);



    }


    public async void WriteData()
    {
        var Datas = new ChessDataObj
        {
            description = PassData.PlayerDesc,
            flag = "",
            ChessLevel = PassData.ChessLevel.ToString(),
            ChessElo = PassData.ChessELO.ToString(),
            chesswin = PassData.ChessWins.ToString(),
            chessloses = PassData.ChessLooses.ToString(),
            chessDraw = PassData.ChessDraws.ToString(),
            AILeveling = PassData.BotLeveling



        };


        var Sendata = await iclient.WriteStorageObjectsAsync(isession, new[] {
        new WriteStorageObject
       {
      Collection = "ChessDatabase",
      Key = "Data",
      Value = JsonWriter.ToJson(Datas),
      Version = PassData.version,
      PermissionRead=2,

       }
});

    }


    public async void UpdateXP(int xp)
    {
        var payload = JsonWriter.ToJson(new { xp = xp });
        var rpcid = "Update_XP";
        var WalletRPC = await PassData.iClient.RpcAsync(PassData.isession, rpcid, payload);

    }

    public int calculateEloChess(int Result)
    {

        var Expected = 1.0f / (1 + (Mathf.Pow(10, 1.0f * (PassData.AIRating - PassData.ChessELO) / 400)));
        PassData.ChessELO = (int)(PassData.ChessELO + 30 * (Result - Expected));

        return PassData.ChessELO;

    }

    public void OpenLeavePanel()
    {
        LeavePanel.SetActive(true);
    }

    public void CloseLeavePanel()
    {
        LeavePanel.SetActive(false);
    }

    public void LeaveMatch()
    {
        LeaveGame();
    }


    public void LeaveGame()
    {
        SceneManager.LoadScene("Menu");
    }

    public void FacebookShareLink()
    {
        FB.Mobile.ShareDialogMode = ShareDialogMode.NATIVE;
        FB.ShareLink(
                new System.Uri("https://www.meet2play.app/download"),
                callback: ShareCallback);

    }

    private void ShareCallback(IShareResult result)
    {
        if (result.Cancelled || !string.IsNullOrEmpty(result.Error))
        {
            Debug.Log("ShareLink Error: " + result.Error);
        }
        else if (!string.IsNullOrEmpty(result.PostId))
        {
            // Print post identifier of the shared content
            Debug.Log(result.PostId);
        }
        else
        {
            // Share succeeded without postID

            updateWallet(50 , 5);
           // UpdateXP(5);
            PlayerPrefs.SetString("sharesplash", "true");
            SceneManager.LoadScene("Menu");


            Debug.Log("ShareLink success!");
        }
    }


    IEnumerator GetTexture(string URL, RawImage avatar)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(URL);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            avatar.texture = myTexture;
        }

    }

    public void Rematch()
    {
        RematchButton.interactable = false;
        StartCoroutine(ResetScene());

    }


    IEnumerator ResetScene()
    {
        yield return new WaitForSeconds(3);

        var pick = Random.Range(0, 2);


        if(pick == 1)
        {
        SceneManager.LoadScene("ChessAI");
        }

        if(pick == 0)
        {
            RejectRematchPanel.SetActive(true);
            yield return new WaitForSeconds(5);
            RejectRematchPanel.SetActive(false);
        }


    }

    // Update is called once per frame
    void Update()
    {

       


        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            NoConnectionPanel.SetActive(true);
        }
        else
        {

            NoConnectionPanel.SetActive(false);

        }
    }

    public void CloseTheApp()
    {
        Application.Quit();
    }

    public void CloseEndPanel()
    {
        EndScreenPanel.SetActive(false);
        FreezeGame.SetActive(true);
 
    }

    public void MonitorLeave()
    {
        ByteBrew.NewCustomEvent("LeftGame", "Username=" + PassData.isession.Username + ";");
    }

    public int CalculateAIRating(int Result)
    {
        var Expected = 1.0f / (1 + (Mathf.Pow(10, 1.0f * (PassData.ChessELO - PassData.AIRating) / 400)));
        PassData.AIRating = (int)(PassData.AIRating + 30 * (Result - Expected));

        return PassData.AIRating;
    }

    public void OpenDrawOfferPanel()
    {
        OfferDrawPanel.SetActive(true);
    }

    public void CloseDrawOfferPanel()
    {
        OfferDrawPanel.SetActive(false);
    }

    public void SendDrawRequest()
    {
        DrawButton.interactable = false;
        RequestDrawButton.interactable = false;
        StartCoroutine(DrawGame());
    }

    IEnumerator DrawGame()
    {
        yield return new WaitForSeconds(3);

        var pick = Random.Range(0, 2);

        if(pick == 1)
        {
        OfferDrawPanel.SetActive(false);
        DrawResult("Draw By Agreement");
        }

        if(pick== 0)
        {
            OfferDrawPanel.SetActive(false);
            RejectDrawPanel.SetActive(true);

            yield return new WaitForSeconds(2);

            RejectDrawPanel.SetActive(false);

        }


    }

    public  void AddFriend()
    {
        AddFriendButton.image.sprite = FriendAddedSprite;
        AddFriendButton.interactable = false;
 

    }



}
