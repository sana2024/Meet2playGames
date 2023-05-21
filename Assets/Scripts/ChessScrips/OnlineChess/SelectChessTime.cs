using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectChessTime : MonoBehaviour
{
    [SerializeField] Button Min5Button;
    [SerializeField] Button Min10Button;
    [SerializeField] Button Min30Button;


    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("time")== 5)
        {
            Min5Button.interactable = false;
            Min10Button.interactable = true;
            Min30Button.interactable = true;
        }
        if (PlayerPrefs.GetInt("time") == 10)
        {
            PlayerPrefs.SetInt("time", 10);
            Min5Button.interactable = true;
            Min10Button.interactable = false;
            Min30Button.interactable = true;
        }
        if (PlayerPrefs.GetInt("time") == 30)
        {
            PlayerPrefs.SetInt("time", 30);
            Min5Button.interactable = true;
            Min10Button.interactable = true;
            Min30Button.interactable = false;
        }
    }

 public void Selected5Min()
    {
        PlayerPrefs.SetInt("time", 5);
        Min5Button.interactable = false;
        Min10Button.interactable = true;
        Min30Button.interactable = true;

    }

    public void Selected10Min()
    {
        PlayerPrefs.SetInt("time", 10);
        Min5Button.interactable = true;
        Min10Button.interactable = false;
        Min30Button.interactable = true;
    }

    public void Selected30Min()
    {

        PlayerPrefs.SetInt("time", 30);
        Min5Button.interactable = true;
        Min10Button.interactable = true;
        Min30Button.interactable = false;

    }


}
