using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reselution : MonoBehaviour
{
    [SerializeField] RectTransform MyBackground;
    [SerializeField] RectTransform OpponentBackground;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("main aspect "+Camera.main.aspect);
        if (Camera.main.aspect <= 1.5)
        {
            Camera.main.orthographicSize = 8.16144f;
        }

        if(Camera.main.aspect > 1.4 && Camera.main.aspect < 1.5)
        {
            Debug.Log("aspect " + Camera.main.aspect);
            MyBackground.anchoredPosition= new Vector2(MyBackground.anchoredPosition.x , 160);
            OpponentBackground.anchoredPosition = new Vector2(OpponentBackground.anchoredPosition.x, 160);
        }

        if (Camera.main.aspect > 1.3  && Camera.main.aspect < 1.4)
        {
            Debug.Log("aspect " + Camera.main.aspect);
            MyBackground.anchoredPosition = new Vector2(MyBackground.anchoredPosition.x, 174);
            OpponentBackground.anchoredPosition = new Vector2(OpponentBackground.anchoredPosition.x, 174);
        }

        if (Camera.main.aspect > 1.7 && Camera.main.aspect < 1.9)
        {
            Debug.Log("aspect " + Camera.main.aspect);
            MyBackground.anchoredPosition = new Vector2(MyBackground.anchoredPosition.x, 121);
            OpponentBackground.anchoredPosition = new Vector2(OpponentBackground.anchoredPosition.x, 121);
        }

    }
}
