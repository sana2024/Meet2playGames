using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeepLink : MonoBehaviour
{
    public string deeplinkURL;
    public static DeepLink Instance { get; private set; }
    private void Awake()
    {
 
        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += onDeepLinkActivated;
                    Debug.Log("application url "+ Application.absoluteURL);
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else deeplinkURL = "[none]";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void onDeepLinkActivated(string url)
    {
       
        // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
        deeplinkURL = url;

        // Decode the URL to determine action. 
        // In this example, the app expects a link formatted like this:
        // unitydl://mylink?scene1

   
        string PlayerId = url.Split('+')[1];

        var matchId = url.Substring(url.IndexOf("?"), url.IndexOf("+") - url.IndexOf("?"));
        var Match = matchId.Split('?')[1];

        Notifications.Instance.JoinedPlayers(PlayerId , "Challange Accepted", Match);

        Debug.Log(" Match "+Match+"  ------------------------");
        Debug.Log("User ID " +PlayerId+"  --------------------------");
        }

    
}
