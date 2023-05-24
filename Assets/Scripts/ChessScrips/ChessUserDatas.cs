using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nakama;
using Nakama.TinyJson;
using System.Threading.Tasks;
using System.Linq;

public class ChessUserDatas : MonoBehaviour
{
    [SerializeField] Text ProfileLextText;
    [SerializeField] Text MenuLevelText;
    [SerializeField] Text PlayerDescText;
    [SerializeField] Text WinsText;
    [SerializeField] Text LoosesText;
    [SerializeField] Text DrawText;
    [SerializeField] Text CurrentXPText;
    [SerializeField] Text RequiredXPText;
    [SerializeField] Text ELOText;
    [SerializeField] Image XPSlider;
    [SerializeField] Image MenuLevelSlider;
    [SerializeField] GameObject StatisticPanel;

    public int ChessLevel = 1;
    public int ChessElo = 1000;
    public int RequiredXP = 83;
    public int CurrentXP = 0;
    public int ChessWins = 0;
    public int ChessLosses = 0;
    public int ChessDraw = 0;
    public int AIleveling = 1;
    public string PlayerDescription = "";
    public string PlayerFlag = "";
    public string ChessSkill = "Beginner";


    //User Description / About me
    [SerializeField] InputField AboutMeInput;
    [SerializeField] Text AboutMeText;
    [SerializeField] Text EditButtonText;
    bool Edited = true;


    IClient iclient;
    ISession isession;
    ISocket isocket;

    //to calculate XP amount based on level
    public float AdditionMultiplier = 300;
    public float PowerMutiplier = 2;
    public float DivisionMultipler = 7;




    bool LeveledUp = false;

    public static ChessUserDatas Instance;

    // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        iclient = PassData.iClient;
        isession = PassData.isession;
        isocket = PassData.isocket;


        ReadChessData();
        StartCoroutine(WaitForRequiredXp());
    }

    IEnumerator WaitForRequiredXp()
    {
        yield return new WaitUntil(() => PassData.RequiredXP != 0);
        ReadXP();
    }

    // Update is called once per frame
    void Update()
    {
         if(LeveledUp == true)
        {
            LevelUp();
            LeveledUp = false;
        }
    }


    public async void ReadChessData()
    {
        var ChessProfile = await iclient.ReadStorageObjectsAsync(isession, new[] {
        new StorageObjectId {
        Collection = "ChessDatabase",
        Key = "Data",
        UserId = isession.UserId
  }
});



        if (ChessProfile.Objects.Any())
        {
            var ChessDatabase = ChessProfile.Objects.First();
            var datas = JsonParser.FromJson<ChessDataObj>(ChessDatabase.Value);

            //ADD IT TO THE CLASSES VARIABLES
            PlayerDescription = datas.description;
            PlayerFlag = datas.flag;
            ChessLevel = int.Parse(datas.ChessLevel);
            
            RequiredXP = CalculateRequiredXp();
            Debug.Log("required xp read " + RequiredXP);

            ChessWins = int.Parse(datas.chesswin);
            ChessLosses = int.Parse(datas.chessloses);
            ChessDraw = int.Parse(datas.chessDraw);
            ChessSkill = datas.ChessSkill;
            ChessElo = int.Parse(datas.ChessElo);
            AIleveling = datas.AILeveling;

            // DISPLAY UI OF THE DATAS

            AboutMeText.text = datas.description;
            ProfileLextText.text =  datas.ChessLevel;
            MenuLevelText.text = datas.ChessLevel;
            WinsText.text = datas.chesswin;
            LoosesText.text = datas.chessloses;
            DrawText.text = datas.chessDraw;
            RequiredXPText.text = CalculateRequiredXp().ToString();
            ELOText.text = datas.ChessElo;

            // SEND IT TO PASS DATA
            PassData.PlayerDesc = datas.description;
            PassData.ChessLevel = int.Parse(datas.ChessLevel);
            PassData.ChessWins = int.Parse(datas.chesswin);
            PassData.ChessLooses = int.Parse(datas.chessloses);
            PassData.ChessDraws = int.Parse(datas.chessDraw);
            PassData.RequiredXP = CalculateRequiredXp();
            PassData.ChessELO = int.Parse(datas.ChessElo);
            PassData.BotLeveling = datas.AILeveling;

        }
        else
        {
            UserProfile.instance.AddXP(0);
            UserProfile.instance.updateWallet(0);
            WriteData(); 
        }
    }

    // ----------------------------- XP SECTION ----------------------------- 
    public async void ReadXP()
    {
        var account = await iclient.GetAccountAsync(isession);
        var wallet = JsonParser.FromJson<Dictionary<string, int>>(account.Wallet);
 
        CurrentXP = wallet.Values.First();
        CurrentXPText.text = wallet.Values.First().ToString();
        PassData.CurrentXP = wallet.Values.First();

        updateXPUi();


    }

    public void UpdateXP(int xp)
    {
        CurrentXP += xp;
        //var payload = JsonWriter.ToJson(new { xp = xp });
        //var rpcid = "Update_XP";
        //var WalletRPC = await iclient.RpcAsync(isession, rpcid, payload);
        if (CurrentXP >= PassData.RequiredXP)
        {
            LeveledUp = true;
        }

        PassData.CurrentXP = CurrentXP;
        ReadXP();
        updateXPUi();
        Debug.Log("Update XP" + CurrentXP);
    }

  
    private int CalculateRequiredXp()
    {

        var Required = 0;
        for (int i = 1; i <= ChessLevel; i++)
        {
            Required += (int)Mathf.Floor(i + AdditionMultiplier * Mathf.Pow(PowerMutiplier, i / DivisionMultipler));

        }
        return Required / 4;
    }


    public void updateXPUi()
    {
      
 
        var fillAmount = ((CurrentXP * 100) / PassData.RequiredXP) * 0.01f;
        XPSlider.fillAmount = fillAmount;
        MenuLevelSlider.fillAmount = fillAmount;
    }




    // -------------------------- LEVEL SESION -----------------------------
 


    public void LevelUp()
    {
        ChessLevel++;
       // UpdateXP(-PassData.RequiredXP);
        UserProfile.instance.AddXP(-PassData.RequiredXP);
        UserProfile.instance.updateWallet(0);
        PassData.RequiredXP = CalculateRequiredXp();
        Debug.Log("required xp level up " + RequiredXP);
        RequiredXPText.text = RequiredXP.ToString();
        ProfileLextText.text = ChessLevel.ToString();
        MenuLevelText.text = ChessLevel.ToString();
        WriteData();

        Debug.Log("Level up 2 " + CurrentXP);
    }



    public async void WriteData()
    {
        var Datas = new ChessDataObj
        {
            description = PlayerDescription,
            flag = PlayerFlag,
            ChessLevel = ChessLevel.ToString(),
            ChessElo = ChessElo.ToString(),
            ChessSkill = ChessSkill,
            chesswin = ChessWins.ToString(),
            chessloses = ChessLosses.ToString(),
            chessDraw = ChessDraw.ToString(),
            AILeveling = AIleveling
             
 
        };


        PassData.PlayerDesc = PlayerDescription;
        PassData.ChessLevel = ChessLevel;
        PassData.RequiredXP = CalculateRequiredXp();
        Debug.Log("Required xp pass data  " + PassData.RequiredXP);
        PassData.ChessWins = ChessWins;
        PassData.ChessLooses = ChessLosses;
        PassData.ChessDraws = ChessDraw;
        PassData.ChessELO = ChessElo;
        PassData.BotLeveling = AIleveling;
        


        AboutMeText.text = PlayerDescription;
        ProfileLextText.text = ChessLevel.ToString();
        MenuLevelText.text = ChessLevel.ToString();
        RequiredXPText.text = CalculateRequiredXp().ToString();
        Debug.Log("Required xp ui " + CalculateRequiredXp().ToString());
        WinsText.text = ChessWins.ToString();
        LoosesText.text = ChessLosses.ToString();
        DrawText.text = ChessDraw.ToString();
        ELOText.text = ChessElo.ToString();


        var Sendata = await iclient.WriteStorageObjectsAsync(isession, new[] {
        new WriteStorageObject
       {
      Collection = "ChessDatabase",
      Key = "Data",
      Value = JsonWriter.ToJson(Datas),
      Version = PassData.version,
      PermissionRead=2,

       }
});

    }

    public void EditClicked()
    {
        Edited = !Edited;

        if(Edited == false)
        {
            EditButtonText.text = "Submit";
            AboutMeInput.text = AboutMeText.text;
            AboutMeText.gameObject.SetActive(false);
            AboutMeInput.gameObject.SetActive(true);
 
        }
        if (Edited == true)
        {
            EditButtonText.text = "Edit";
            AboutMeText.text = AboutMeInput.text;

            AboutMeText.gameObject.SetActive(true);
            AboutMeInput.gameObject.SetActive(false);
            PlayerDescription = AboutMeText.text;
            WriteData();
        }
    }


    public void OpenStatisticPanel()
    {
        StatisticPanel.SetActive(true);
    }

    public void CloseStatisticPanel()
    {
        StatisticPanel.SetActive(false);
    }



 
}
