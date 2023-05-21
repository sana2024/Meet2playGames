using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using Nakama.TinyJson;
using System.Linq;

public class PlayerList : MonoBehaviour
{

    ISession session;
    ISocket socket;
    IClient client;
    string hostPresence;
    string SecondPresence;
    static IMatch currentMatch;

    public GameObject Prefab;
    public Transform PrarentRow;

    [SerializeField] GameObject ChallengeFriendPanell;
    [SerializeField] GameObject FreindPanel;

    [SerializeField] RawImage myProfileImg;
    [SerializeField] RawImage Opponent;
    [SerializeField] Button ButtonPlay;

    [SerializeField] Sprite FriendAdded;

    List<string> friendsList = new List<string>();

    RawImage image;

    UserProfile profile;


    public string matchID = "";
    public static PlayerList instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UserListRPC();
    }

 

    async void UserListRPC()
    {
        var rpcid = "users";
        // var pokemonInfo = await PassData.iClient.RpcAsync(PassData.isession, rpcid);
        var pokemonInfo = await PassData.iClient.RpcAsync(PassData.isession,rpcid);

        string TrimedJson = pokemonInfo.Payload.Remove(11, 1);

        var data = JsonUtility.FromJson<PersonData>(TrimedJson);

        List<String> termsList = new List<String>();


      
            foreach(var id in data.client)
            {
               if(data.client.IndexOf(id) < 30)
            {
                termsList.Add(id.id);
            }

            }            


      

        var result2 = await PassData.iClient.GetUsersAsync(PassData.isession, termsList);

        foreach (var player in result2.Users)
        {
             
           
            if (player.Id != "00000000-0000-0000-0000-000000000000" && player.Id != PassData.isession.UserId)
            {


                GameObject game = Instantiate(Prefab, PrarentRow);
                Button[] addButton = game.GetComponentsInChildren<Button>();
                addButton[0].onClick.AddListener(delegate {

                    AddFriend(player.Username);
                    StartCoroutine(SendButton(addButton[0]));
                    addButton[0].image.sprite = FriendAdded;


                });

                addButton[1].onClick.AddListener(delegate {

 
                    SendNotificationRpc(player.Id , player.Username ,player.AvatarUrl);
                    //SendPushNotification(player.Id, player.Username);
                    StartCoroutine(SendButton(addButton[1]));


                });

 
                // addbuton=AddFriend(player.Username);

                ArabicText text = game.GetComponentInChildren<ArabicText>();
                image = game.GetComponentInChildren<RawImage>();


                text.Text = player.DisplayName;
                var name = text.Text;


                Image[] UserImages = game.GetComponentsInChildren<Image>();
                if (player.Online)
                {
                    UserImages[4].color = Color.green;
                }
                else
                {
                    UserImages[4].color = Color.grey;
                }

                StartCoroutine(GetTexture(image, player.AvatarUrl));
                StartCoroutine(GetTexture());


                if (friendsList.Contains(player.Id))
                {
                    addButton[0].image.sprite = FriendAdded;
                    addButton[0].interactable = false;
                }


            }

        }

        var FriendList = await  PassData.iClient.ListFriendsAsync(PassData.isession);


        foreach (var f in FriendList.Friends)
        {
 
                if (f.State == 0 || f.State == 1)
                {
                     friendsList.Add(f.User.Id);
                }
                else
                {
                    
                }
            }


        
    }
    //to design the buttun after it's clicked 
   public IEnumerator SendButton(Button button)
    {
        button.interactable = false;
        yield return new WaitForSeconds(3f);

        button.interactable = true;
    }

    // Update is called once per frame


    IEnumerator GetTexture(RawImage image, string url)
    {

        // fetchdata();
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        Debug.Log("image url " + www);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            image.texture = myTexture;

        }

    }

    public async void AddFriend(string user)
    {
        var usernames = new[] { user };
        await PassData.iClient.AddFriendsAsync(PassData.isession, null, usernames);
        UserProfile.instance.AddXP(5);
        UserProfile.instance.updateWallet(0);
        //ChessUserDatas.Instance.UpdateXP(5);

        Debug.Log("it is working Properly");


    }

    public async void SendPushNotification(string UserID, string Username , string matchId) {

        Debug.Log("match id "+matchId);
        var payload = JsonWriter.ToJson(new { userid = UserID , username = Username , matchid = matchId});
        var rpcid = "one_signal";
        await PassData.isocket.RpcAsync(rpcid, payload);
    }


    public async void SendNotificationRpc(string Userid , string username ,string OpponentURL)

    {
        var match = await PassData.isocket.CreateMatchAsync();

        matchID = match.Id;
        var payload = JsonWriter.ToJson(new { userid = Userid, matchid = match.Id });
        var rpcid = "custom_rpc_func_id";
        // var Notification = await PassData.iClient.RpcAsync(PassData.isession, rpcid, payload);
        var Notification1 = await PassData.isocket.RpcAsync(rpcid, payload);
 
        ChallengeFriendPanell.SetActive(true);
        StartCoroutine(GetTexture());
        StartCoroutine(GetTexture(Opponent, OpponentURL));

        var matchjoin = await PassData.isocket.JoinMatchAsync(match.Id);

        PassData.MyPresense = match.Self;



        foreach (var user in match.Presences)
        {
            if (user.UserId != match.Self.UserId)
            {
                PassData.OtherPresence = user;
            }
        }

        PassData.betAmount = 100;

        PassData.BoardPrice = 100;
        profile = new UserProfile();
        profile.AddXP(5);
        profile.updateWallet(100);


        hostPresence = match.Self.UserId;

        SecondPresence = Userid;


        PassData.hostPresence = hostPresence;
        PassData.SecondPresence = SecondPresence;




        // PassData.hostPresence = hostPresence;
        // PassData.SecondPresence = SecondPresence;

        Debug.Log("hostPlayer " + hostPresence);
        Debug.Log("SecondPresence" + Userid);


        Debug.Log("Our Match ID: " + match.Self.SessionId);
        currentMatch = match;
        PassData.Match = currentMatch;
        PlayerPrefs.SetString("matchID", currentMatch.Id);


        SendPushNotification(Userid, username, match.Id);
 
    }






    IEnumerator GetTexture()
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(PassData.MyURL);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            myProfileImg.texture = myTexture;

        }

    }

}






