using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using Nakama.TinyJson;
using System.Threading.Tasks;
using ByteBrewSDK;

public class Connections : MonoBehaviour
{
    [SerializeField] GameObject NoConnectionPanel;
    [SerializeField] GameObject WeakConnection;
    [SerializeField] GameObject OppoenentOffile;
    [SerializeField] GameObject UserOffline;
    [SerializeField] Sprite LooserSprite;
    bool Reconnected = false;

    // Start is called before the first frame update
    void Start()
    {
        var mainThread = UnityMainThreadDispatcher.Instance();
        PassData.isocket.Closed += () => Connect();
        PassData.isocket.Connected += () => Connect();
        PassData.isocket.ReceivedMatchPresence += m => mainThread.Enqueue(() => CheckPresence(m));



    }


   public async void CheckPresence(IMatchPresenceEvent matchPresenceEvent)
    {
        foreach (var user in matchPresenceEvent.Leaves)
        {

            if(user.UserId != PassData.Match.Self.UserId)
            {
                Debug.Log(user.Username + " left the game ");

                if (ChessBoard.Instance.victoryScreen.activeSelf == false)
                {
                    OppoenentOffile.SetActive(true);
                    ByteBrew.NewCustomEvent("GameInterrupted", "Username=" + PassData.isession.Username + ";");
                    Invoke("WinGame", 15f);

                }
            }
  
        }


        foreach (var user in matchPresenceEvent.Joins)
        {
            Debug.Log(user.Username + " joined the game ");
            CancelInvoke();
 

        }


    }

    public void WinGame()
    {
        GameEndResult.Instance.WinnerResult();
    }

    public void LooseGame()
    {
        GameEndResult.Instance.LooserResult(LooserSprite);
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            NoConnectionPanel.SetActive(true);
            ByteBrew.NewCustomEvent("GameInterrupted", "Username=" + PassData.isession.Username + ";");
            Invoke("LooseGame", 15f);
        }
        else
        {
             
            NoConnectionPanel.SetActive(false);
            if(Reconnected == false)
            {
                Connect();
                Reconnected = true;
            }

        }
    }

    private async void Connect()
    {
     
        try
        {
            var keepAliveIntervalSec = 30;
            await PassData.isocket.ConnectAsync(PassData.isession, true, keepAliveIntervalSec);
            if (PassData.isocket.IsConnected)
            {
                await PassData.isocket.JoinMatchAsync(PassData.Match.Id);
                Debug.Log("socket reconnected ");
                UserOffline.SetActive(false);

            }
        }


        catch (TaskCanceledException e)
        {

            Debug.Log("task canceled " + e.Task);
            var retryConfiguration = new Nakama.RetryConfiguration(1, 5, delegate { });

            // Configure the retry configuration globally.
            PassData.iClient.GlobalRetryConfiguration = retryConfiguration;
            await PassData.isocket.JoinMatchAsync(PassData.Match.Id);

        }
    }

    public void CloseTheApp()
    {
        Application.Quit();
    }
}
