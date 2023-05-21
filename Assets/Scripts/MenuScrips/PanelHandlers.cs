using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelHandlers : MonoBehaviour
{
    [Header("PANELS -----")]
    [SerializeField] GameObject UserProfilePanel;
    [SerializeField] GameObject LeaderBoardPanel;
    [SerializeField] GameObject FriendsPanel;
    [SerializeField] GameObject AdsPanel;
    [SerializeField] GameObject NotificationsPanel;
    [SerializeField] GameObject SettingPanel;
    [SerializeField] GameObject CollectBonusPanel;
    [SerializeField] GameObject IAPPanel;
    [SerializeField] GameObject NoEnoughCoinPanel;
    [SerializeField] GameObject SharePanel;
    [SerializeField] GameObject BackgammonPanel;
    [SerializeField] GameObject SelectionPanel;
    [SerializeField] GameObject ChessPanel;


    [SerializeField] GameObject SelectTimePanel;
    public float TimePanelSmoothTime = 0.05F;
    private Vector3 TimePanelVelocity = Vector3.one;
    Vector3 TimePaneltarget;

    private void Start()
    {
        TimePaneltarget = SelectTimePanel.transform.localPosition;

    }

    private void Update()
    {
        // SelectTimePanel.transform.localPosition = Vector3.SmoothDamp(SelectTimePanel.transform.localPosition, TimePaneltarget, ref TimePanelVelocity, TimePanelSmoothTime);
    }

    public void selectTime()
    {
         TimePaneltarget = new Vector3(SelectTimePanel.transform.localPosition.x, -120 , SelectTimePanel.transform.localPosition.z);
    }

    public void OpenSharePanel()
    {
        LeaderBoardPanel.SetActive(false);
        FriendsPanel.SetActive(false);
        AdsPanel.SetActive(false);
        NotificationsPanel.SetActive(false);
        CollectBonusPanel.SetActive(false);
        IAPPanel.SetActive(false);
        SettingPanel.SetActive(false);
        UserProfilePanel.SetActive(false);
        SharePanel.SetActive(true);
    }



    public void CloseSharePanel()
    {
        SharePanel.SetActive(false);
    }



    public void OpenProfilePanel()
    {
        LeaderBoardPanel.SetActive(false);
        FriendsPanel.SetActive(false);
        AdsPanel.SetActive(false);
        NotificationsPanel.SetActive(false);
        CollectBonusPanel.SetActive(false);
        IAPPanel.SetActive(false);
        SettingPanel.SetActive(false);
        UserProfilePanel.SetActive(true);
    }

    public void OpenLeaderBoardPanel()
    {
        LeaderBoardPanel.SetActive(true);
        FriendsPanel.SetActive(false);
        AdsPanel.SetActive(false);
        NotificationsPanel.SetActive(false);
        CollectBonusPanel.SetActive(false);
        IAPPanel.SetActive(false);
        SettingPanel.SetActive(false);
        UserProfilePanel.SetActive(false);
    }

    public void OpenFriendsPanel()
    {
        LeaderBoardPanel.SetActive(false);
        FriendsPanel.SetActive(true);
        AdsPanel.SetActive(false);
        NotificationsPanel.SetActive(false);
        CollectBonusPanel.SetActive(false);
        IAPPanel.SetActive(false);
        SettingPanel.SetActive(false);
        UserProfilePanel.SetActive(false);
    }

    public void OpenAdsPanel()
    {
        LeaderBoardPanel.SetActive(false);
        FriendsPanel.SetActive(false);
        AdsPanel.SetActive(true);
        NotificationsPanel.SetActive(false);
        CollectBonusPanel.SetActive(false);
        IAPPanel.SetActive(false);
        SettingPanel.SetActive(false);
        UserProfilePanel.SetActive(false);
    }

    public void OpenNotificationPanel()
    {
        LeaderBoardPanel.SetActive(false);
        FriendsPanel.SetActive(false);
        AdsPanel.SetActive(false);
        NotificationsPanel.SetActive(true);
        CollectBonusPanel.SetActive(false);
        IAPPanel.SetActive(false);
        SettingPanel.SetActive(false);
        UserProfilePanel.SetActive(false);
    }

    public void OpenSettingsPanel()
    {
        LeaderBoardPanel.SetActive(false);
        FriendsPanel.SetActive(false);
        AdsPanel.SetActive(false);
        NotificationsPanel.SetActive(false);
        CollectBonusPanel.SetActive(false);
        IAPPanel.SetActive(false);
        SettingPanel.SetActive(true);
        UserProfilePanel.SetActive(false);
    }

    public void OpenCollectBonus()
    {
        LeaderBoardPanel.SetActive(false);
        FriendsPanel.SetActive(false);
        AdsPanel.SetActive(false);
        NotificationsPanel.SetActive(false);
        CollectBonusPanel.SetActive(true);
        IAPPanel.SetActive(false);
        SettingPanel.SetActive(false);
        UserProfilePanel.SetActive(false);
    }

    public void OpenIAPPanel()
    {
        LeaderBoardPanel.SetActive(false);
        FriendsPanel.SetActive(false);
        AdsPanel.SetActive(false);
        NotificationsPanel.SetActive(false);
        CollectBonusPanel.SetActive(false);
        IAPPanel.SetActive(true);
        SettingPanel.SetActive(false);
        UserProfilePanel.SetActive(false);
    }

    public void NoEnoughMoneyClose()
    {
        NoEnoughCoinPanel.SetActive(false);
    }

    public void OpenBackgammonPanel()
    {
        SelectionPanel.SetActive(false);
        BackgammonPanel.SetActive(true);
    }

    public void OpenChessPanel()
    {
        SelectionPanel.SetActive(false);
        ChessPanel.SetActive(true);
    }

    public void BackToSelectionPanel()
    {
        SelectionPanel.SetActive(true);
        ChessPanel.SetActive(false);
        BackgammonPanel.SetActive(false);
        TimePaneltarget = new Vector3(SelectTimePanel.transform.localPosition.x, -200, SelectTimePanel.transform.localPosition.z);
    }
 
 

}
