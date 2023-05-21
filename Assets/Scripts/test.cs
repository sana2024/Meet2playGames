using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Nakama;
using System.Linq;
using System.Collections.Generic;
using Unity.RemoteConfig;
using Random = UnityEngine.Random;

public class test : MonoBehaviour
{
 
    public void Start()
    {
        var pick = Random.Range(0, 2);
        Debug.Log("bool " + pick);
    }

 
}
