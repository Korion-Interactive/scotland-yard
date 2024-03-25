using Newtonsoft.Json;
using Sunbow.Util.IO;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

#if UNITY_PS5
using UnityEngine.PS5;
using Korion.IO;
using System.Threading;
#elif UNITY_PS4
using Sony.NP;
using UnityEngine.PS4;
using Korion.IO;
using System.Threading;
#endif

public class AppSetup : MonoBehaviour
{
    static AppSetup instance;
    public static AppSetup Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("AppSetup");
                go.AddComponent<AppSetup>();
                DontDestroyOnLoad(go);
                // awake is setting the instance
            }
            return instance;
        }
    }

    public Table AchievementTable;
    public Table StatsTable;
    public Table LocaTable;
    Table SettingsTable;


    public bool IsMusicEnabled { get => Get("music", true); set { Set("music", value); AudioSystem.Instance.EnableMusic(value); } }
    public bool IsSfxEnabled { get => Get("sfx", true); set { Set("sfx", value); AudioSystem.Instance.EnableSfx(value); } }
    public bool IsStatusBarVisible { get => Get("status_bar", false); set => Set("status_bar", value); }
    public bool IsPostEffectEnabled { get => Get("post_effect", false); set => Set("post_effect", value); }

    bool isVoiceChatEnabled;
    public bool IsVoiceChatEnabled
    {
        get => isVoiceChatEnabled;
        set
        {
            isVoiceChatEnabled = value;
            GSP.EnableVoiceChatIfPossible = value;
        }
    }


    private void Set(string settingName, bool value)
    {
        try
        {
            Ensure(settingName, value);

            SettingsTable["value", settingName] = value.ToString();
            SettingsTable.Save();
        }
        catch (Exception ex)
        {
            this.LogError(ex);
        }
    }

    private bool Get(string settingsName, bool defaultValue)
    {
        try
        {
            Ensure(settingsName, defaultValue);
            bool result = SettingsTable.Get<bool>("value", settingsName);
            return result;
        }
        catch (Exception ex)
        {
            this.LogError(ex);
            return false;
        }
    }


    private void Ensure(string settingsName, bool value)
    {
        if (!SettingsTable.Contains("value", settingsName))
        {
            SettingsTable.AppendRow(settingsName, value.ToString());
        }
    }

    private void Awake()
    {
        // set instance
        instance = this;

        // destroy any old object if there is any.
        var inits = FindObjectsOfType<AppSetup>();
        foreach (AppSetup init in inits)
        {
            if (init != this)
            {
                // there can be only one!
                this.LogWarn("found an old AppSetup: destroying it!");
                Destroy(this.gameObject);
                return;
            }
        }

        DontDestroyOnLoad(this.gameObject);

        // Load localization
        TextAsset locaTxt = Resources.Load("Loca") as TextAsset;
        LocaTable = new Table(locaTxt.text, "Loca", new CSVSetting(true, true) { ColumnSeparator = '\t', });
        Loc.Language = Application.systemLanguage;

        // load achievement data
        TextAsset achvTxt = Resources.Load("achievements") as TextAsset;
        AchievementTable = new Table(achvTxt.text, "achievements", new CSVSetting(true, true) { ColumnSeparator = '\t', });

        // stats
        LoadOrCreateStatsTable();

        // Load or create settings table

#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        //TODO KORION IO
        string settingsPath = "settings.txt";
        SettingsTable = new Table(settingsPath, 2, 1);
        SettingsTable[0, 0] = "id";
        SettingsTable[1, 0] = "value";
#else
        string settingsPath = Path.Combine(Application.persistentDataPath, "settings.txt");
        if (!File.Exists(settingsPath))
        {
            SettingsTable = new Table(settingsPath, 2, 1);
            SettingsTable[0, 0] = "id";
            SettingsTable[1, 0] = "value";
        }
        else
        {
            SettingsTable = new Table(settingsPath);
        }
#endif
    }

    public UniTask WriteDataAsync(string id, string data, CancellationToken cancellationToken = default)
    {
        var writer = IOSystem.Instance.GetWriter();
        //string json = JsonUtility.ToJson(savedSettings, prettyPrint: true); //now data

        Debug.Log("Writing Korion IO");

        return writer.WriteAsync(id, data, cancellationToken);
    }

    public async UniTask<string> ReadDataAsync(string id, CancellationToken cancellationToken = default)
    {
        var reader = IOSystem.Instance.GetReader();

        string stringData = await reader.Read<string>(id, cancellationToken);
        if (string.IsNullOrEmpty(stringData))
        {
            return null; //json
        }

        Debug.Log("Reading Korion IO: " + stringData);
        return stringData;
    }

    public void LoadOrCreateStatsTable()
    {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5

        string statsPath = "stats.txt";
        StatsTable = new Table(statsPath, 2, 1);
        StatsTable[0, 0] = "id";
        StatsTable[1, 0] = "value";

        //save
        //string json = JsonUtility.ToJson(StatsTable, prettyPrint: true); //now data

        //Test
        //KORION SAVE DATA
        //WriteDataAsync(id, savestring).Forget();
        //read data
        //KORION LOAD DATA
        //string configData = await ReadDataAsync(ConfigKeyIdentifier);

        //if (configData != null)


        ////TODO KORION IO
        ////TryLoad
        //if (false)
        //{

        //}
        //else
        //{
        //    string statsPath = "stats.txt";
        //    StatsTable = new Table(statsPath, 2, 1);
        //    StatsTable[0, 0] = "id";
        //    StatsTable[1, 0] = "value";
        //}
        ////how do i convert to binary --> then to a table again //json? memorrha
#else
        string statsPath = Path.Combine(Application.persistentDataPath, "stats.txt");
        if (!File.Exists(statsPath))
        {
            StatsTable = new Table(statsPath, 2, 1);
            StatsTable[0, 0] = "id";
            StatsTable[1, 0] = "value";
        }
        else
        {
            StatsTable = new Table(statsPath);
        }
#endif
    }


#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
        {
            Loc.Table.Save("Assets/Resources/Loca_Temp.txt");
        }
    }
#endif

    void OnApplicationFocus(bool focus)
    {
        if (!focus
            && GameState.HasInstance
            && !GameState.Instance.IsGameOver
            && GameSetupBehaviour.Instance.Setup.Mode == GameMode.HotSeat)
        {
            SaveGame();
        }
    }

    public static bool HasOpenGame()
    {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        IOSystem.Instance.GetReader().Read //@ james? oder doku? gibts so nen check?
        return false;//TODO KORION IO
#else
        return File.Exists(Globals.LastGameSetupPath) && File.Exists(Globals.LastGameStatePath);
#endif
    }

    public void SaveGame()
    {
        //TODO KORION IO
        string gameSetup = JsonConvert.SerializeObject(GameSetupBehaviour.Instance.Setup);
        SaveData(ref gameSetup, Globals.LastGameSetupPath);

        string gameState = JsonConvert.SerializeObject(GameState.Instance);
        SaveData(ref gameState, Globals.LastGameStatePath);
    }

    void SaveData(ref string data, string filePath)
    {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        //TODO KORION IO
        WriteDataAsync(filePath, data).Forget();
#else
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (FileStream stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(data);

                    writer.Flush();
                }
            }
        }
        catch(Exception ex)
        {
            this.LogError($"Error at SaveData(xxx, {filePath})", ex);
            PopupManager.ShowPrompt("error", "something_went_wrong");
        }
#endif
    }


    internal void LoadLastGame()
    {
        //in theory i could load it here and on finish --> call this load coroutine // less fuck up no need for await nor change coroutine logic //implement callback
        AsyncLoadLastGame().Forget();
    }

    private async UniTaskVoid AsyncLoadLastGame()
    {
        GameSetup setup;
        setup = await TryLoad(Globals.LastGameSetupPath);
        if (setup == null)
            return;

        GameSetupBehaviour.Instance.Setup = setup;

        await UniTask.DelayFrame(1);// or await UniTask.Yield() or await Task.Delay(150);

        this.Broadcast(GameGuiEvents.LoadingScene);
        SceneManager.LoadSceneAsync("Game"); //maybe use this sometime else

        //// --- DON'T SIMPLY REMOVE THIS! ---
        //// this looks very stupid, but the deserializing constructor assigns the fresh GameState to the singleton immediately
        //if (TryLoad(Globals.LastGameStatePath))
        //{
        //    yield break;
        //}
    }

    private async UniTask<GameSetup> TryLoad(string filePath)
    {
        GameSetup gameSetup;

#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        //TODO KORION IO
        var result = await ReadDataAsync(filePath);

        if (result != null)
        {
            Debug.Log("Loaded Config");

            gameSetup = JsonConvert.DeserializeObject<GameSetup>(result);
            //OnLoadSettingsCompleted(true, configData);
        }
        else
        {
            PopupManager.ShowPrompt("error", "something_went_wrong");
            Debug.Log("loading failed");
            DeleteSavegame();
            return null;
        }
#else
        try
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    gameSetup = JsonConvert.DeserializeObject<GameSetup>(reader.ReadToEnd());
                }
            }
        }
        catch (Exception ex)
        {
            this.LogError($"Error at TryLoad({filePath})", ex);
            PopupManager.ShowPrompt("error", "something_went_wrong");
            result = default;
            DeleteSavegame();
            return null;
        }
#endif
        return gameSetup;
    }

    internal void DeleteSavegame()
    {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        //TODO KORION IO // CancellationToken cancellationToken = default 
        IOSystem.Instance.RemoveData(Globals.LastGameSetupPath, new CancellationTokenSource().Token).Forget();
#else
        if (File.Exists(Globals.LastGameSetupPath))
        {
            File.Delete(Globals.LastGameSetupPath);
        }

        if (File.Exists(Globals.LastGameStatePath))
        {
            File.Delete(Globals.LastGameStatePath);
        }
#endif
    }
}
