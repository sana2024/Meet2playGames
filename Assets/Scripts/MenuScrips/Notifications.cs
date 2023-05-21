using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using Nakama.TinyJson;
using System.Linq;

public class Notifications : MonoBehaviour
{
    IClient client;
    ISession session;
    ISocket isocket;
    static IMatch currentMatch;
    IMatchmakerMatched MatchmakerMatched;


    [SerializeField] Text NotificationAmountText;
    [SerializeField] RawImage userAvatar;
    [SerializeField] RawImage OpponentAvatar;

    [SerializeField] GameObject NotificationPanel;
    [SerializeField] GameObject notificationPrefab;
    [SerializeField] Transform notificationParent;
    [SerializeField] FriendSystem friendsSystem;
    [SerializeField] GameObject FriendRequestPanel;
    [SerializeField] Button ButtonPlay;
    [SerializeField] GameObject ChallengeFriendPanell;
    [SerializeField] Button ButtonPlayRequest;
    UserProfile profile;
    public static Notifications Instance;

    int NotificationAmount = 0;
    string SenderUsername = "";
    string reciverId = "";
    string subject = "";
    string message = "";
    string sendId = "";

    string messageAccept;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    async void Start()
    {
        client = PassData.iClient;
        session = PassData.isession;


        isocket = PassData.isocket;


        var mainThread = UnityMainThreadDispatcher.Instance();


        var result = await client.ListNotificationsAsync(session, 4 , PlayerPrefs.GetString("nakama.notificationsCacheableCursor"));

        NotificationAmount = result.Notifications.Count();
        NotificationAmountText.text = NotificationAmount.ToString();


        isocket.ReceivedNotification += notification => mainThread.Enqueue(() => onRecivedNotification(notification));
 

    }


    private void onRecivedNotification(IApiNotification notification)
    {
        switch (notification.Code)
        {
            case -2:

                NotificationAmount++;
                NotificationAmountText.text = NotificationAmount.ToString();

                break;

            case 102:
 
                subject = notification.Subject;
                sendId = notification.SenderId;
                PassData.BoardPrice = 100;
                profile = new UserProfile();
                profile.AddXP(5);
                profile.updateWallet(100);
 
                NotificationAmount++;
                NotificationAmountText.text = NotificationAmount.ToString();

                
                break;

            case 103:
 
                message = notification.Subject;
                sendId = notification.SenderId;
                NotificationAmount++;
                NotificationAmountText.text = NotificationAmount.ToString();
                ChallengeFriendPanell.SetActive(false);
                break;
                

            case 104:

                Debug.Log("subject " + notification.Subject);
                messageAccept = notification.Subject;
                sendId = notification.SenderId;
                NotificationAmount++;
                NotificationAmountText.text = NotificationAmount.ToString();
                ButtonPlayRequest.gameObject.SetActive(true);
                break;



        }

    }


    public async void ListNotifications()
    {

        foreach (Transform item in notificationParent)
        {
            Destroy(item.gameObject);
        }

        var cacheableCursor = PlayerPrefs.GetString("nakama.notificationsCacheableCursor", null);


        var result = await client.ListNotificationsAsync(session , 4 , cacheableCursor);
        PlayerPrefs.SetString("nakama.notificationsCacheableCursor", result.CacheableCursor);

 


 

            foreach (var n in result.Notifications)
            {
                 

            if (NotificationPanel.active)
               {
                Debug.Log("sender "+ n.SenderId);
                GameObject Notification = Instantiate(notificationPrefab, notificationParent);
                Notification.transform.parent = notificationParent.transform;
                ArabicText [] Message = Notification.GetComponentsInChildren<ArabicText>();
                Button[] ButtonResponse = Notification.GetComponentsInChildren<Button>();


                string[] SenderProfile = { n.SenderId };


                var senderaacount = await PassData.iClient.GetUsersAsync(PassData.isession, SenderProfile);
                foreach (var sender in senderaacount.Users)
                 {

                     if (n.Code == 102)
                     {
                        subject = n.Subject;
                        ButtonResponse[0].onClick.AddListener(() => { JoinedPlayers(sender.Id, messageAccept, subject);
                        StartCoroutine(ButtonClickedDesign(ButtonResponse[0]));
                        });
              

                        ButtonResponse[1].onClick.AddListener(() => { SendNotificationRpcReject(sender.Id, message);
                        StartCoroutine(ButtonClickedDesign(ButtonResponse[1]));
                        });


                        // SenderProfile(n.SenderId);
                        Message[0].Text = sender.Username;
                        Message[1].Text = "Challenged You For A game";

                    }

 
                    if (n.Code == -2)
                    {

                        ButtonResponse[0].onClick.AddListener(() => { AcceptRequest(sender.Id);

                            StartCoroutine(ButtonClickedDesign(ButtonResponse[0]));
                        });
                
                        ButtonResponse[1].gameObject.SetActive(false);

                        Message[0].Text = sender.Username;
                        Message[1].Text ="Sent you friend request";

                    }

                    if (n.Code == -3)
                    {
                        ButtonResponse[0].gameObject.SetActive(false);
                        ButtonResponse[1].gameObject.SetActive(false);

                        Message[0].Text = sender.Username;
                        Message[1].Text = "Accepted your friend request";

                    }


                    if (n.Code == 104)
                    {
                        ButtonResponse[0].gameObject.SetActive(false);
                        ButtonResponse[1].gameObject.SetActive(false);

                        Message[0].Text = sender.Username;
                        Message[1].Text = "Accepted your challange";

                    }

                    if (n.Code == 103)
                    {
                        ButtonResponse[0].gameObject.SetActive(false);
                        ButtonResponse[1].gameObject.SetActive(false);

                        Message[0].Text = sender.Username;
                        Message[1].Text = "Rejected your challange";

                    }
                }
            }

        }
 
        }
    



    public async void AcceptRequest(string UserId)
    {

        var id = new[] { UserId };
        await client.AddFriendsAsync(session, id, null);

    }



    public async void JoinedPlayers(string UserId, string messageAccept, string matchmakerMatched)
    {
        Debug.Log("match ID " + matchmakerMatched);

        NotificationPanel.SetActive(false);
        SendAcceptRequest(UserId, messageAccept);

        var match = await PassData.isocket.JoinMatchAsync(matchmakerMatched);

        PassData.betAmount = 100;
        PassData.BoardPrice = 100;
        profile = new UserProfile();
        profile.AddXP(5);
        profile.updateWallet(100);
        PassData.hostPresence = sendId;
        PassData.SecondPresence = match.Self.UserId;


        currentMatch = match;
        PassData.Match = currentMatch;
        PlayerPrefs.SetString("matchID", currentMatch.Id);




        Debug.Log(PassData.hostPresence);

        var otheruserid = PassData.SecondPresence;


        foreach (var presence in match.Presences)
        {
 
            if (presence.UserId != match.Self.UserId)
            {
                PassData.OtherUserId = presence.UserId;
                PassData.otherUsername = presence.Username;
                PassData.OtherPresence = presence;

                PassData.isession = await client.SessionRefreshAsync(session);
                var ids = new[] { presence.UserId };
                var result = await PassData.iClient.GetUsersAsync(PassData.isession, ids);

                foreach (var u in result.Users)
                {
                    PassData.OpponentURL = u.AvatarUrl;
                    StartCoroutine(GetTexture(u.AvatarUrl, OpponentAvatar));
                    StartCoroutine(GetTexture());

                    ButtonPlay.gameObject.SetActive(true);
                    FriendRequestPanel.SetActive(true);
                }

            }


        }




        //   StartCoroutine(GetTexture(user.AvatarUrl,userAvatar));




        // }

        Debug.Log(matchmakerMatched);




    }

    public async void SendAcceptRequest(string Userid, string message)
    {

        var payload = JsonWriter.ToJson(new { userid = Userid, messageAccept = message });
        var rpcid = "custom_rpc_func_Accept";
        // var Notification = await PassData.iClient.RpcAsync(PassData.isession, rpcid, payload);
        var Notification1 = await PassData.isocket.RpcAsync(rpcid, payload);




    }

    public async void SendNotificationRpcReject(string Userid, string message)

    {

        var payload = JsonWriter.ToJson(new { userid = Userid, message = message });
        var rpcid = "custom_rpc_func_reject";
        // var Notification = await PassData.iClient.RpcAsync(PassData.isession, rpcid, payload);
        var Notification1 = await PassData.isocket.RpcAsync(rpcid, payload);




    }


    public async void RemoveNotification(string NotificationIds, GameObject notification)
    {
        var notificationIds = new[] { NotificationIds };
        await client.DeleteNotificationsAsync(session, notificationIds);
        notification.SetActive(false);
    }



    public async void SenderProfile(string SenderId)
    {
        var id = new[] { SenderId };
        var result = await client.GetUsersAsync(session, id, null);

        foreach (var user in result.Users)
        {
            SenderUsername = user.Username;
        }
    }


    IEnumerator GetTexture(string uri, RawImage rawImage)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            rawImage.texture = myTexture;

        }

    }


    public void OnNotificationClicked()
    {
        NotificationPanel.SetActive(true);
        ListNotifications();
    }

    public void OnCancelClicked()
    {
        NotificationPanel.SetActive(false);
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

            userAvatar.texture = myTexture;

        }

    }


    IEnumerator ButtonClickedDesign(Button button)
    {
        button.interactable = false;
        yield return new WaitForSeconds(3f);
        button.interactable = true;
         
    }

    private void Update()
    {
        if (NotificationPanel.activeSelf)
        {
            NotificationAmount = 0;
            NotificationAmountText.text = NotificationAmount.ToString();
        }
    }

}



 

