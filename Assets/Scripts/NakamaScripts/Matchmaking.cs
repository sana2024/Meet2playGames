using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;
using System.Threading.Tasks;
using Nakama.TinyJson;
using UnityEngine.Networking;
using Nakama.Ninja.WebSockets;
using Random = UnityEngine.Random;
using ByteBrewSDK;

public class Matchmaking : MonoBehaviour
{
    [SerializeField] NakamaConnection Nconnect;
    ISocket isocket;
    IClient iclient;
    ISession isession;
    private string ticket;
    static IMatch currentMatch;
    string OtherId;

    IUserPresence hostPresence;
    IUserPresence SecondPresence;
    string wating = "true";

    bool MatchFound;

    //to recive notification when no one was found
    float FindTimer;
    bool EnteredWaitingPhase = false;
    bool EnteredGame = false;
    bool waitedMoreThan30s = false;

    IMatch match;


    [SerializeField] GameObject SearchingPanel;
    [SerializeField] GameObject SearchingBackground;
    [SerializeField] GameObject SearingTitle;
    [SerializeField] UserProfile userProfile;
    [SerializeField] GameObject NoEnoughCoinPanel;
    [SerializeField] GameObject RejectPanel;
    [SerializeField] RawImage OpponentImage;
    [SerializeField] ArabicText OponentUsername;
    [SerializeField] GameObject ProgressBar;
    [SerializeField] GameObject HiddenUser;
    [SerializeField] GameObject PlayButton;
    [SerializeField] Button CancleButton;
    [SerializeField] GameObject LostconnectionPanel;


    UnityWebRequest www;

    string query;
    Dictionary<string, string> properity;

    public string matchType;


    //if chess didnt connect to a real person then load AI scene
    bool enteredWaitingPhase = false;
    float waitingTime = 0;


    //creating face personality for AI
    string FakeAIUsername;
    [SerializeField] GameObject FakeAIProfile;


    //cancling ticket if the game connected to AI
    bool isTicketCancled = false;
    int ChessPrice;

    // Start is called before the first frame update
    public async void Start()
    {
 
        isession = PassData.isession;
        isession = await PassData.iClient.SessionRefreshAsync(isession);
        iclient = PassData.iClient;
        isocket = PassData.isocket;
        var mainThread = UnityMainThreadDispatcher.Instance();
        isocket.ReceivedMatchmakerMatched += match => mainThread.Enqueue(() => OnReceivedMatchmakerMatched(match));
        isocket.ReceivedMatchPresence += m => mainThread.Enqueue(() => OnRecivedMatchPresence(m));
        isocket.ReceivedMatchState += m => mainThread.Enqueue(async () => await OnReceivedMatchState(m));
        isocket.Closed += () => ReConnect();

        ByteBrew.NewCustomEvent("OpenedMenu", "Username=" + isession.Username + ";");

        CancleButton.onClick.AddListener(RemoveTicket);
 

    }

    public async void Update()
    {

        if (enteredWaitingPhase)
        {
            waitingTime += Time.deltaTime;
            isTicketCancled = true;

            if(int.Parse(UserProfile.instance.OnlineCounterText.text) <= 1)
            {
                Invoke("ConnectToAI", 7);
            }
        }



        if (waitingTime > 12)
        {
            ConnectToAI();

        }

        // Debug.Log(FindTimer);
        if (EnteredWaitingPhase)
        {
            FindTimer += Time.deltaTime;
        }

        if (FindTimer > 4)
        {
            waitedMoreThan30s = true;
        }

        if (Camera.main.aspect <= 1.5f)
        {
            SearchingBackground.transform.localScale = new Vector3(1.002772f, 1.12765f, 0.2982222f);
            RectTransform titleRect = SearingTitle.GetComponent<RectTransform>();
            RectTransform CancleRect = CancleButton.gameObject.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, -114);
            titleRect.localScale = new Vector3(0.9f, 0.8f, 0.8f);

            CancleRect.anchoredPosition = new Vector2(-93, -7);
            CancleRect.localScale = new Vector3(0.40f, 0.79f, 0.79f);
        }

    }


#if !UNITY_EDITOR
    private async void OnApplicationFocus(bool focus)
    {
        if(focus == false)
        {
        if (waitedMoreThan30s == true && EnteredGame == false)
        {
            userProfile.InsertIntoWaiting(isession.UserId);
        }
        }
            
            iclient = Nconnect.client();
            var keepAliveIntervalSec = 30;
          //  isocket = Socket.From(iclient, new WebSocketAdapter());
            await isocket.ConnectAsync(isession, true, keepAliveIntervalSec);

 
        if (SearchingPanel.activeSelf == true)
        {
            var matchmakingTickets = await isocket.AddMatchmakerAsync(query, 2, 2, properity);

            ticket = matchmakingTickets.Ticket;
        }

       
    }
#endif


    async void ConnectToAI()
    {
 
        matchType = "chessAI";
        PlayButton.SetActive(true);
        OponentUsername.Text = FakeAIUsername;
        PassData.otherUsername = FakeAIUsername;
        SimpleLoading.Instance.rectComponent.gameObject.SetActive(false);
        CalculateAIrating();
        FakeAIProfile.SetActive(true);



        if (isTicketCancled == true)
        {
            enteredWaitingPhase = false;
            isTicketCancled = false;
            Debug.Log("is ticked canceled");
            await isocket.RemoveMatchmakerAsync(ticket);


        }
    }

    private async void ReConnect()
    {
        try
        {
            if (!isocket.IsConnected)
            {
                await isocket.ConnectAsync(isession, true, 30);
                PassData.isocket = isocket;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error connecting socket: " + e.Message);
        }
    }


    public async void SendMatchState(long opCode, string state)
    {
        await isocket.SendMatchStateAsync(PassData.Match.Id, opCode, state);
    }

    private async Task OnReceivedMatchState(IMatchState matchState)
    {
        var state = matchState.State.Length > 0 ? System.Text.Encoding.UTF8.GetString(matchState.State).FromJson<Dictionary<string, string>>() : null;

        switch (matchState.OpCode)
        {
            case 16:

                if (state["Leave"] == "Left")
                {
                    SearchingPanel.SetActive(false);
                    StartCoroutine(DisplayRejectPanel());
                    await PassData.isocket.LeaveMatchAsync(PassData.Match.Id);


                    HiddenUser.SetActive(true);
                    ProgressBar.SetActive(true);
                    StartCoroutine(GetTexture(""));
                    PlayButton.SetActive(false);
                    SearchingPanel.SetActive(false);
                }


                if (state["Leave"] == "Join")
                {
                    PassData.JoinedPlayers = 2;
                    Debug.Log("other player joined");
                }




                break;

            case 11:

                PassData.RecivedLevel = state["Level"];
                break;

        }
    }


    IEnumerator DisplayRejectPanel()
    {
        RejectPanel.SetActive(true);
        yield return new WaitForSeconds(3);
        RejectPanel.SetActive(false);
    }

    public async void FindBackgammonMatch(string BoardName)
    {
        ByteBrew.NewCustomEvent("SrearchedForGame", "type=Backgammon; Username=" + isession.Username + ";");
        matchType = "backgammon";
        var result1 = await iclient.ListUsersStorageObjectsAsync(isession, "wait", "00000000-0000-0000-0000-000000000000", 1);

        foreach (var userid in result1.Objects)
        {

            if (userid.Value.Count() > 30)
            {
                var id = userid.Value.Remove(0, 12);


                id = id.Remove(36, 2);
                if (id != PassData.isession.UserId)
                {
                    userProfile.SendNotificationWaiting(id, BoardName);
                    userProfile.InsertIntoWaiting("");

                }
            }
            else
            {
                Debug.Log("no user is wating");
            }


        }


        EnteredWaitingPhase = true;
        OponentUsername.Text = "";
        PassData.BoardType = BoardName;

        int wallet = Math.Abs(PassData.WalletMoney);
        int boardPrice = Math.Abs(PassData.BoardPrice);


        if (wallet >= boardPrice)
        {
           // ChessUserDatas.Instance.UpdateXP(5);
            SearchingPanel.SetActive(true);
            LostconnectionPanel.SetActive(false);

            properity = new Dictionary<string, string>() {
          {"board", BoardName}
         };
            query = "+properties.board:" + BoardName;


            var matchmakingTickets = await isocket.AddMatchmakerAsync(query, 2, 2, properity);


            ticket = matchmakingTickets.Ticket;

            wating = "true";
            PassData.Queue = wating;

            PassData.Board = PassData.BoardType;
            var level = PlayerPrefs.GetInt("level");
            userProfile.WriteData(level, PassData.wins, PassData.losses, PassData.Queue, PassData.Board);
            Debug.Log(level);

            userProfile.Storageojectcounter();

            isession = await iclient.SessionRefreshAsync(isession);
            var account = await iclient.GetAccountAsync(isession);

        }
        else
        {
            NoMoneyPanelTimer();
            Debug.Log("you dont have enough money");
        }

    }

    public async void FindChessMatch(int BoardPrice)
    {
        ByteBrew.NewCustomEvent("SrearchedForGame", "type=Chess; Username=" + isession.Username + ";");
        FakeAIUsername = "Player" + UnityEngine.Random.Range(0, 5000);
        if(FakeAIUsername == PassData.Username)
        {
            FakeAIUsername = "Player" + UnityEngine.Random.Range(0, 5000);
        }
        int wallet = Math.Abs(PassData.WalletMoney);
        if (wallet >= BoardPrice)
        {
           // ChessUserDatas.Instance.UpdateXP(5);
            ChessPrice = BoardPrice;
            OponentUsername.Text = "";
            enteredWaitingPhase = true;
            matchType = "chess";
            PlayButton.SetActive(false);
            SearchingPanel.SetActive(true);
            SimpleLoading.Instance.rectComponent.gameObject.SetActive(true);
            var time = PlayerPrefs.GetInt("time");
            Debug.Log("time " + time);
            Debug.Log("skill " + PassData.SkillLevel);
            //enteredWaitingPhase = true;
            var stringProperties = new Dictionary<string, string>() {
    {"skill", GetPlayerChessSkill(PassData.ChessELO)}
};

            var numericProperties = new Dictionary<string, double>() {
    {"time", time}
};
            var query = "+properties.skill:" + GetPlayerChessSkill(PassData.ChessELO) + " +properties.time:" + time;

            var matchmakingTickets = await PassData.isocket.AddMatchmakerAsync(query, 2, 2, stringProperties, numericProperties);
            ticket = matchmakingTickets.Ticket;

 

        }
        else
        {
            NoMoneyPanelTimer();
        }
    }

    void NoMoneyPanelTimer()
    {
        NoEnoughCoinPanel.SetActive(true);

    }




    public async void RemoveTicket()
    {
        ByteBrew.NewProgressionEvent(ByteBrewProgressionTypes.Failed, "Chess", "ChessAI");
        enteredWaitingPhase = false;
        waitingTime = 0;
        FakeAIProfile.SetActive(false);
        FindTimer = 0;
        UserProfile.instance.AddXP(-5);
        if (MatchFound == false)
        {
            SearchingPanel.SetActive(false);
            LostconnectionPanel.SetActive(false);
            Debug.Log("tiketck " + ticket);

            if (matchType != "chessAI")
            {
                await isocket.RemoveMatchmakerAsync(ticket);
            }
            if (matchType == "chessAI" || matchType == "chess")
            {
                ByteBrew.NewCustomEvent("CanceledGame", "type=Chess; Username=" + isession.Username + ";");
                UserProfile.instance.BonusWallet(ChessPrice);
            }

            if (matchType == "backgammon")
            {
                ByteBrew.NewCustomEvent("CanceledGame", "type=Backgammon; Username=" + isession.Username + ";");
                int boardPrice = Math.Abs(PassData.BoardPrice);
                UserProfile.instance.BonusWallet(boardPrice);
            }

       
          //  ChessUserDatas.Instance.UpdateXP(-5);

        }
        matchType = "";
        wating = "false";
        PassData.Queue = wating;
        PassData.BoardType = " ";
        userProfile.WriteData(PassData.level, PassData.wins, PassData.losses, PassData.Queue, PassData.BoardType);

    }

    public async void OnApplicationQuit()
    {
        ByteBrew.NewCustomEvent("ClosedApp", "Username=" + PassData.isession.Username + ";");
        if (waitedMoreThan30s == true && EnteredGame == false)
        {
            userProfile.InsertIntoWaiting(isession.UserId);

        }

        if (PassData.BoardType != null)
        {
            wating = "false";
            var level = PlayerPrefs.GetInt("level");
            var win = PlayerPrefs.GetInt("wins");
            var losses = PlayerPrefs.GetInt("looses");
            var board = "paris";
            userProfile.WriteData(level, win, losses, wating, board);


        }
        await isocket.RemoveMatchmakerAsync(ticket);


    }

    public async void OnApplicationPause(bool pause)
    {
        ByteBrew.NewCustomEvent("ClosedApp", "Username=" + PassData.isession.Username + ";");
        /*
           if (waitedMoreThan30s == true && EnteredGame == false)
           {
               userProfile.InsertIntoWaiting(isession.UserId);
           }

         */
        if (PassData.BoardType != null)
        {
            wating = "false";
            var level = PlayerPrefs.GetInt("level");
            var win = PlayerPrefs.GetInt("wins");
            var losses = PlayerPrefs.GetInt("looses");
            var board = PassData.BoardType;

            userProfile.WriteData(level, win, losses, wating, board);

        }

        await isocket.RemoveMatchmakerAsync(ticket);


    }

    private async void OnReceivedMatchmakerMatched(IMatchmakerMatched matchmakerMatched)
    {
        ByteBrew.NewProgressionEvent(ByteBrewProgressionTypes.Started, "Chess", "ChessOnline");
        //when a chess match was found cancle the waiting face of the AI
        enteredWaitingPhase = false;
        waitingTime = 0;

        //these variables are specific for backagmmon, if a game was found you will be kicked out of wating list and you will not recieve notification based on that
        if (matchType == "backgammon")
        {
            EnteredGame = true;
            EnteredWaitingPhase = false;
            FindTimer = 0;

            MatchFound = true;

            wating = "false";

            PassData.Queue = wating;
            userProfile.WriteData(PassData.level, PassData.wins, PassData.losses, PassData.Queue, PassData.BoardType);

        }

        var users = matchmakerMatched.Users;
        try
        {
            match = await isocket.JoinMatchAsync(matchmakerMatched);


        }
        catch (TaskCanceledException ex)
        {
            LostconnectionPanel.SetActive(true);
            Debug.Log("match canceled " + ex);
            var keepAliveIntervalSec = 30;
            //  isocket = Socket.From(iclient, new WebSocketAdapter());
            await isocket.ConnectAsync(isession, true, keepAliveIntervalSec);
            match = await isocket.JoinMatchAsync(matchmakerMatched);

        }

        hostPresence = matchmakerMatched.Users.OrderBy(x => x.Presence.SessionId).First().Presence;
        SecondPresence = matchmakerMatched.Users.OrderBy(x => x.Presence.SessionId).Last().Presence;

        PassData.hostPresence = hostPresence.UserId;
        PassData.SecondPresence = SecondPresence.UserId;


        currentMatch = match;
        PassData.Match = currentMatch;
        PlayerPrefs.SetString("matchID", currentMatch.Id);

        PlayerPrefs.SetString("MatchSelf", currentMatch.Self.UserId);
        foreach (var presence in match.Presences)
        {

            CancleButton.onClick.AddListener(RejectGame);

            if (presence.UserId != match.Self.UserId)
            {
                PassData.OtherUserId = presence.UserId;

                PassData.OtherPresence = presence;

                isession = await iclient.SessionRefreshAsync(isession);
                var ids = new[] { presence.UserId };
                var result = await iclient.GetUsersAsync(isession, ids);



                foreach (var u in result.Users)
                {


                    if (u.Username != match.Self.Username)
                    {
                        PassData.OpponentURL = u.AvatarUrl;

                        OponentUsername.Text = u.Username;
                        PassData.otherUsername = u.Username;
                        StartCoroutine(GetTexture(u.AvatarUrl));
                    }
                }

            }

            if (presence.UserId == match.Self.UserId)
            {
                PassData.MyPresense = presence;
            }


        }

        var state = MatchDataJson.SetLevel(PassData.level.ToString());
        SendMatchState(OpCodes.Player_Level, state);

    }

    private async void OnRecivedMatchPresence(IMatchPresenceEvent matchPresenceEvent)
    {
        MatchFound = true;

        foreach (var user in matchPresenceEvent.Joins)
        {

            CancleButton.onClick.AddListener(RejectGame);

            var UserId = PlayerPrefs.GetString("MatchSelf");

            if (user.UserId != UserId)
            {
                PassData.OtherUserId = user.UserId;

                PassData.OtherPresence = user;
                isession = await iclient.SessionRefreshAsync(isession);
                var ids = new[] { user.UserId };
                var result = await iclient.GetUsersAsync(isession, ids);

                foreach (var u in result.Users)
                {
                    // Debug.Log(u.Username + "  " + u.AvatarUrl);
                    PassData.OpponentURL = u.AvatarUrl;


                    if (u.Username != match.Self.Username)
                    {
                        OponentUsername.Text = u.Username;
                        PassData.otherUsername = u.Username;
                    }

                    StartCoroutine(GetTexture(u.AvatarUrl));
                }

            }

        }

    }

    public void AcceptGame()
    {

        if (matchType == "backgammon")
        {
            ByteBrew.NewCustomEvent("FoundGame", "type=Backgammon; Username=" + isession.Username + ";");
            var state = MatchDataJson.SetLeaveMatch("Join");
            SendMatchState(OpCodes.Reject_Match, state);

            SceneManager.LoadScene("GameScene");
            PassData.IsFirstRound = true;
        }

        if (matchType == "chess")
        {
            ByteBrew.NewCustomEvent("FoundGame", "type=ChessOnline; Username=" + isession.Username + ";");
            var state = MatchDataJson.SetLeaveMatch("Join");
            SendMatchState(OpCodes.Reject_Match, state);
            SceneManager.LoadScene("Chess");
        }

        if (matchType == "chessAI")
        {
            ByteBrew.NewCustomEvent("FoundGame", "type=ChessAI; Username=" + isession.Username + ";");
            SceneManager.LoadScene("ChessAI");
        }
    }


    public async void RejectGame()
    {
        SearchingPanel.SetActive(false);
        MatchFound = false;
        var state = MatchDataJson.SetLeaveMatch("Left");
        SendMatchState(OpCodes.Reject_Match, state);

        await PassData.isocket.LeaveMatchAsync(PassData.Match.Id);


        HiddenUser.SetActive(true);
        ProgressBar.SetActive(true);
        StartCoroutine(GetTexture(""));
        PlayButton.SetActive(false);




    }


    IEnumerator GetTexture(string url)
    {


        www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            if (PassData.FacebookID != null)
            {

                if (url != PassData.MyURL)
                {
                    OpponentImage.texture = myTexture;
                    PassData.OtherPlayerTexture = myTexture;
                }
                else
                {
                    Debug.Log("the same");
                }

            }
            else
            {
                if (PassData.otherUsername.Contains("Player") && url == "https://www.nicepng.com/png/detail/232-2323319_gamecenter-icon-game-center-app.png")
                {
                }
                else if (PassData.otherUsername.Contains("Player") && url == "https://play-lh.googleusercontent.com/szHQCpMAb0MikYIhvNG1MlruXFUggd6DJHXkMPG1H4lJPB7Lee_BkODfwxpQazxfO9mA")
                {
                }
                else
                {
                    OpponentImage.texture = myTexture;
                    PassData.OtherPlayerTexture = myTexture;
                }



            }
        }

    }

    public String GetPlayerChessSkill(int Rating)
    {
        if (Rating <= 1200)
        {
            PassData.SkillLevel = "Beginner";
            return "Beginner";
 
        }

        if (Rating > 1200 && Rating < 2000)
        {
            PassData.SkillLevel = "Intermediate";
            return "Intermediate";
        }

        if (Rating >= 2000)
        {
            PassData.SkillLevel = "Advanced";
            return "Advanced";
        }
        else
        {
            return "";
        }
    }

    public void CalculateAIrating()
    {
        switch (PassData.BotLeveling)
        {
            case <= 1:
                PassData.AIRating = Random.Range(950,1001);
                PassData.AIlevel = 1;
                break;

            case 2:
                PassData.AIRating = 1050;
                PassData.AIlevel = 2;
                break;

            case 3:
                PassData.AIRating = 1100;
                PassData.AIlevel = 3;
                break;


            case 4:
                PassData.AIRating = 1150;
                PassData.AIlevel = 4;
                break;

            case 5:
                PassData.AIRating = 1200;
                PassData.AIlevel = 5;
                break;

            case 6:
                PassData.AIRating = 1250;
                PassData.AIlevel = 6;
                break;

            case 7:
                PassData.AIRating = 1300;
                PassData.AIlevel = 7;
                break;

            case 8:
                PassData.AIRating = 1350;
                PassData.AIlevel = 8;
                break;

            case 9:
                PassData.AIRating = 1400;
                PassData.AIlevel = 9;
                break;

            case 10:
                PassData.AIRating = 1450;
                PassData.AIlevel = 10;
                break;


            case 11:
                PassData.AIRating = 1500;
                PassData.AIlevel = 11;
                break;


            case 12:
                PassData.AIRating = 1550;
                PassData.AIlevel = 12;
                break;


            case 13:
                PassData.AIRating = 1600;
                PassData.AIlevel = 13;
                break;


            case 14:
                PassData.AIRating = 1650;
                PassData.AIlevel = 14;
                break;


            case 15:
                PassData.AIRating = 1700;
                PassData.AIlevel = 15;
                break;


            case 16:
                PassData.AIRating = 1750;
                PassData.AIlevel = 16;
                break;


            case 17:
                PassData.AIRating = 1800;
                PassData.AIlevel = 17;
                break;

            case 18:
                PassData.AIRating = 1850;
                PassData.AIlevel = 18;
                break;

            case 19:
                PassData.AIRating = 1900;
                PassData.AIlevel = 19;
                break;


            case >= 20:
                PassData.AIRating = 2000;
                PassData.AIlevel = 20;
                break;

        }

    }


}