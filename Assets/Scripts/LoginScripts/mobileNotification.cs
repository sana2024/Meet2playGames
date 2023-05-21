using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID
using Unity.Notifications.Android;
# endif
using UnityEngine;


public class mobileNotification : MonoBehaviour
{
  #if UNITY_ANDROID
    public void Start()
    {
    
        // AndroidNotificationCenter.CancelAllDisplayedNotifications();
        var channel = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Notification Channel",
            Importance = Importance.Default,
            Description = "reminder notifications",
        };


        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        var notification = new AndroidNotification();
        /*
        notification.Title = "Meet2Play";
        notification.Text = "Backgammon lovers are waiting for you! Play now 🎲";
        notification.LargeIcon = "m2p";
        notification.FireTime = System.DateTime.Now.AddDays(2);
        var id = AndroidNotificationCenter.SendNotification(notification, "channel_id");
        */
    }

#endif

}
