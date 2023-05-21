using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Facebook.Unity;
using UnityEngine.Networking;
using Nakama;
using Nakama.TinyJson;
using System;
using System.Linq;
using ByteBrewSDK;

public class GameEndResult : MonoBehaviour
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
    [SerializeField] Text OtherWnnerScore;
    [SerializeField] GameObject DrawType;
    [SerializeField] Text DrawTypeText;
    [SerializeField] Button LeaveButton;
    public static GameEndResult Instance;
    [SerializeField] GameObject FreezeGame;
    [SerializeField] Button RematchButton;
    [SerializeField] GameObject LeavePanel;
    [SerializeField] Sprite LooserSprite;
    [SerializeField] public GameObject OfferDrawPanel;
    [SerializeField] public GameObject RequestedDrawPanel;
    [SerializeField] public GameObject RejectDrawPanel;

    [SerializeField] Button DrawButton;
    [SerializeField] Button CancleEndScrrenButton;
    [SerializeField] Button RequestDrawButton;
    [SerializeField] Button AddFriendButton;
    [SerializeField] Sprite FriendAddedSprite;
 
    


    bool IsLooseRunned = false;
    bool IsDrawRunned = false;


    //nakama variables
    ISocket isocket;
    ISession isession;
    IClient iclient;


    int OppoenentELO;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
       // LeaveButton.onClick.AddListener(OpenLeavePanel);

        isession = PassData.isession;
        isocket = PassData.isocket;
        iclient = PassData.iClient;

        Scene currentScene = SceneManager.GetActiveScene();
        LeaveButton.onClick.AddListener(OpenLeavePanel);

        if (currentScene.name == "Chess")
        {

            StartCoroutine(GetTexture(PassData.MyURL, MyAvatar));

            OpponentAvatar.texture = PassData.OtherPlayerTexture;

        }

        GetOppoenetELO();
        MyRatingText.text = PassData.ChessELO.ToString();
        MyLevelText.text = PassData.ChessLevel.ToString();
    }

    public void WinnerResult()
    {
        ByteBrew.NewCustomEvent("Won", "Game=Chess; Type=Online; Username=" + isession.Username + ";");
        ChessTimer.Instance.gameEnded = true;
        HelloVideoAgora.instance.OnApplicationQuit();
        EndScreenPanel.SetActive(true);
        RequestDrawButton.interactable = false;
        Reward.SetActive(true);
        ResultImage.sprite = WinnerImage;
        EndScreenBackground.sprite = GreenBackground;
        PassData.ChessWins++;
        if (PassData.BotLeveling != 20)
        {
            PassData.BotLeveling++;
        }
        Rewardamount.text = "200";
        updateWallet(200 , 25);
        //UpdateXP(25);
        PassData.ChessELO = calculateEloChess(1);
        WriteData();
        MyWinnerScore.text = "1";
        LeaveButton.onClick.RemoveAllListeners();
        LeaveButton.onClick.AddListener(LeaveScene);

        string Rating = PlayerPrefs.GetString("progress");
        Rating = Rating + " " + PassData.ChessELO;
        PlayerPrefs.SetString("progress", Rating);
      //  ChessTimer.Instance.OpponentTimerText.text = "00:00";
       // ChessTimer.Instance.MyTimerText.text = "00:00";
    }

    public void LooserResult(Sprite LooserSprite)
    {
        ByteBrew.NewCustomEvent("Lost", "Game=Chess; Type=Online; Username=" + isession.Username + ";");
        LeavePanel.SetActive(false);
        if (IsLooseRunned == false)
        {
            ChessTimer.Instance.gameEnded = true;
            HelloVideoAgora.instance.OnApplicationQuit();
            EndScreenPanel.SetActive(true);
            RequestDrawButton.interactable = false;
            EndScreenBackground.sprite = RedBackground;
            PassData.ChessLooses++;
            PassData.ChessELO = calculateEloChess(0);
            WriteData();
            OtherWnnerScore.text = "1";
          
            ResultImage.sprite = LooserSprite;
          
            if (PassData.BotLeveling != 1)
            {
                PassData.BotLeveling--;
            }

            LeaveButton.onClick.RemoveAllListeners();
            LeaveButton.onClick.AddListener(LeaveScene);

            string Rating = PlayerPrefs.GetString("progress");
            Rating = Rating + " " + PassData.ChessELO;
            PlayerPrefs.SetString("progress", Rating);
          //  ChessTimer.Instance.OpponentTimerText.text = "00:00";
          //  ChessTimer.Instance.MyTimerText.text = "00:00";
            IsLooseRunned = false;
        }
    }

    public void DrawResult(string DrawString)
    {
        ByteBrew.NewCustomEvent("Draw", "Game=Chess; Type=Online; Username=" + isession.Username + ";");
        if (IsDrawRunned == false)
        {
            MyWinnerScore.text = "1";
            OtherWnnerScore.text = "1";

            ChessTimer.Instance.gameEnded = true;
            Reward.SetActive(true);
            RequestDrawButton.interactable = false;
            ResultImage.sprite = DrawImage;
            EndScreenBackground.sprite = YellowBackground;
            PassData.ChessDraws++;
            if (PassData.BotLeveling != 20)
            {
                PassData.BotLeveling++;
            }
            Debug.Log("end screen true");
            EndScreenPanel.SetActive(true);
            WriteData();
            updateWallet(100 , 15);
            //UpdateXP(15);
            Rewardamount.text = "100";
            DrawType.SetActive(true);
            DrawTypeText.text = DrawString;
            LeaveButton.onClick.RemoveAllListeners();
            LeaveButton.onClick.AddListener(LeaveScene);
            CancleEndScrrenButton.onClick.AddListener(CloseEndPanel);
          //  ChessTimer.Instance.OpponentTimerText.text = "00:00";
          //  ChessTimer.Instance.MyTimerText.text = "00:00";

            IsDrawRunned = true;

        }
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
        ByteBrew.NewCustomEvent("LeftGame", "Username=" + PassData.isession.Username + ";");
        GameOver();
    }


    public void GameOver()
    {
        var state = MatchDataJson.SetLeaveMatch("leave");
        DataSync.Instance.SendMatchState(OpCodes.Leave_match, state);
        LooserResult(LooserSprite);
        HelloVideoAgora.instance.OnApplicationQuit();
        
    }

    public async void LeaveScene()
    {
        try
        {
            await PassData.isocket.LeaveMatchAsync(PassData.Match.Id);
            SceneManager.LoadScene("Menu");

        }
        catch (TaskCanceledException)
        {
            SceneManager.LoadScene("Menu");
        }
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
        var state = MatchDataJson.SetRematch("RequestReplay");
        DataSync.Instance.SendMatchState(OpCodes.Play_Again, state);
        Debug.Log("requested rematch");
    }

    public void AcceptRematch()
    {
 
        var state = MatchDataJson.SetRematch("AcceptReplay");
        DataSync.Instance.SendMatchState(OpCodes.Play_Again , state);
        DataSync.Instance.RematchDialog.SetActive(false);
        SceneManager.LoadScene("Chess");
    }


    public void RejectRematch()
    {
        var state = MatchDataJson.SetRematch("RejectPlayAgain");
        DataSync.Instance.SendMatchState(OpCodes.Play_Again, state);
        DataSync.Instance.RematchDialog.SetActive(false);
    }


    public async void UpdateXP(int xp)
    {
        var payload = JsonWriter.ToJson(new { xp = xp });
        var rpcid = "Update_XP";
        var WalletRPC = await PassData.iClient.RpcAsync(PassData.isession, rpcid, payload);
  
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


    public int calculateEloChess(int Result)
    {

        var Expected = 1.0f / (1 + (Mathf.Pow(10, 1.0f * (OppoenentELO - PassData.ChessELO) / 400)));
        PassData.ChessELO = (int)(PassData.ChessELO + 30 * (Result - Expected));

        return PassData.ChessELO;

    }


    public async void GetOppoenetELO()
    {

        var result = await iclient.ReadStorageObjectsAsync(isession, new[] {
  new StorageObjectId {
    Collection = "ChessDatabase",
    Key = "Data",
    UserId = PassData.OtherUserId
  }
});

        var ChessDatabase = result.Objects.First();
        var datas = JsonParser.FromJson<ChessDataObj>(ChessDatabase.Value);
         
        OppoenentELO = int.Parse(datas.ChessElo);
        OpponentRatingText.text = OppoenentELO.ToString();
        OppoenentLevelText.text = int.Parse(datas.ChessLevel).ToString();
 
    }

    public void CloseEndPanel()
    {

        Debug.Log("is end screen active " + EndScreenPanel.activeSelf);
        EndScreenPanel.SetActive(false);
        FreezeGame.SetActive(true);
      
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
        var state = MatchDataJson.SetOfferDraw("O");
        DataSync.Instance.SendMatchState(OpCode.OfferDraw, state);
    }

    public void RejectDrawOffer()
    {
        RequestedDrawPanel.SetActive(false);
        var state = MatchDataJson.SetOfferDraw("R");
        DataSync.Instance.SendMatchState(OpCode.OfferDraw, state);
    }

    public void AcceptDrawOffer()
    {
        RequestedDrawPanel.SetActive(false);
        DrawResult("Draw By Agreement");
        var state = MatchDataJson.SetOfferDraw("A");
        DataSync.Instance.SendMatchState(OpCode.OfferDraw, state);
    }
    public IEnumerator DrawRejected()
    {
        RejectDrawPanel.SetActive(true);
        yield return new WaitForSeconds(2);
        RejectDrawPanel.SetActive(false);

    }
    public async void AddFriend()
    {
        AddFriendButton.image.sprite = FriendAddedSprite;
        AddFriendButton.interactable = false;
        var id = new[] { PassData.OtherUserId };
        await PassData.iClient.AddFriendsAsync(PassData.isession, id);
        Debug.Log(" you added " + PassData.OtherUserId);

    }





}
