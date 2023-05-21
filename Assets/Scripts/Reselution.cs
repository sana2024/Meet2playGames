using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reselution : MonoBehaviour
{
    [SerializeField] GameObject MyBackground;
    [SerializeField] GameObject OpponentBackground;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main.aspect <= 1.5)
        {
            Camera.main.orthographicSize = 8.16144f;
        }

        if(Camera.main.aspect == 1.4)
        {
            MyBackground.transform.position = new Vector3(MyBackground.transform.position.x , 160 , MyBackground.transform.position.z);
            OpponentBackground.transform.position = new Vector3(OpponentBackground.transform.position.x, 160, OpponentBackground.transform.position.z);
        }

        if (Camera.main.aspect == 1.3)
        {
            MyBackground.transform.position = new Vector3(MyBackground.transform.position.x, 174, MyBackground.transform.position.z);
            OpponentBackground.transform.position = new Vector3(OpponentBackground.transform.position.x, 174, OpponentBackground.transform.position.z);
        }
    }
}
