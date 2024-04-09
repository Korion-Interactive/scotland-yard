using Newtonsoft.Json;
using Sunbow.Util.IO;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using Korion.IO;
using System.Threading;

#if UNITY_PS5
using UnityEngine.PS5;
#elif UNITY_PS4
using Sony.NP;
using UnityEngine.PS4;
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

    //KORION //to be beautified!
    public bool IsSettingTableInit
    {
        get => _isSettingTableInit;
    
        set
        {
            if (_isSettingTableInitEventInvokedOnce)
                return;
            else
            {
                _isSettingTableInit = value;
                m_isSettingTableInit.Invoke();
                _isSettingTableInitEventInvokedOnce = true;
            }
        } 
    }
    private bool _isSettingTableInit = false;

    private bool _isSettingTableInitEventInvokedOnce = false;

    public UnityEvent m_isSettingTableInit = new UnityEvent();

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

    private async UniTaskVoid Awake()
    {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        await IOSystem.Instance.InitializeAsync(destroyCancellationToken);
#endif

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
        Loc.Language = Application.systemLanguage; //KORION TODO SYSTEM LANGUAGE as in Arcade Fishing for example


        // load achievement data
        TextAsset achvTxt = Resources.Load("achievements") as TextAsset;
        AchievementTable = new Table(achvTxt.text, "achievements", new CSVSetting(true, true) { ColumnSeparator = '\t', });

#if (UNITY_SWITCH || UNITY_PS4 || UNITY_PS5)// && !UNITY_EDITOR //nicht editor?! sollte schon wd woerken und ps4/ps5 weil safe load net läuft
        //TODO KORION IO
        string settingsPath = "settings.txt"; //id

        string data = await ReadDataAsync<string>(settingsPath);

        if (data == null)
        {
            SettingsTable = new Table(settingsPath, 2, 1);
            SettingsTable[0, 0] = "id";
            SettingsTable[1, 0] = "value";
            IsSettingTableInit = true;
        }
        else //load
        {
            SettingsTable = new Table(settingsPath);
            IsSettingTableInit = true;
        }
#else
        string settingsPath = Path.Combine(Application.persistentDataPath, "settings.txt");
        if (!File.Exists(settingsPath))
        {
            SettingsTable = new Table(settingsPath, 2, 1);
            SettingsTable[0, 0] = "id";
            SettingsTable[1, 0] = "value";
            IsSettingTableInit = true;
        }
        else
        {
            SettingsTable = new Table(settingsPath);
            IsSettingTableInit = true;
        }
#endif

        // stats
        LoadOrCreateStatsTable().Forget();
    }

#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5

    public UniTask WriteDataAsync<T>(string id, T data, CancellationToken cancellationToken = default)
    {
        Debug.Log("KORION: Start Writing Data");
        var writer = IOSystem.Instance.GetWriter();
        //string json = JsonUtility.ToJson(savedSettings, prettyPrint: true); //now data

        Debug.Log("Writing Korion IO");

        return writer.WriteAsync(id, data, cancellationToken);
    }

    public async UniTask<T> ReadDataAsync<T>(string id, CancellationToken cancellationToken = default)
    {
        Debug.Log("KORION: Start Reading Data");
        var reader = IOSystem.Instance.GetReader();

        T data = await reader.Read<T>(id, cancellationToken);

        Debug.Log("Reading Korion IO: " + data);
        return data;
    }

#endif

    public async UniTaskVoid LoadOrCreateStatsTable()
    {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        //TODO KORION IO
        string statsPath = "stats.txt"; //id
        Debug.Log("KORION: STart reading or creating stats table");
        string data = await ReadDataAsync<string>(statsPath);
        if (data == null)
        {
            StatsTable = new Table(statsPath, 2, 1);
            StatsTable[0, 0] = "id";
            StatsTable[1, 0] = "value";
        }
        else //load
        {
            StatsTable = new Table(statsPath);
        }
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

    //KORION why was this even static?
    public async UniTask<bool> HasOpenGame()
    {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        //TODO KORION IO
        Debug.Log("KORION: Checking if has open game");
        Debug.Log("Last Savegamepath: " + Globals.LastGameSetupPath);
        string data = await ReadDataAsync<string>(Globals.LastGameSetupPath);
        Debug.Log("KORION: Data read");
        return (data != null);
#else
        return File.Exists(Globals.LastGameSetupPath) && File.Exists(Globals.LastGameStatePath);
#endif
    }

    public void SaveGame()
    {
        Debug.Log("KORION: Start Saving game");
        string gameSetup = JsonConvert.SerializeObject(GameSetupBehaviour.Instance.Setup);
        SaveData(ref gameSetup, Globals.LastGameSetupPath);

        string gameState = JsonConvert.SerializeObject(GameState.Instance);
        SaveData(ref gameState, Globals.LastGameStatePath);
        Debug.Log("KORION: Game Saved");
    }

    void SaveData(ref string data, string filePath)
    {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
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
        Debug.Log("KORION: Start Loading last game");
        AsyncLoadLastGame().Forget();
    }

    private async UniTaskVoid AsyncLoadLastGame()
    {
        GameSetup setup;
        setup = await LoadGameSetup(Globals.LastGameSetupPath);
        Debug.Log("setup: " + setup);
        if (setup == null)
            return;

        GameSetupBehaviour.Instance.Setup = setup;

        await UniTask.DelayFrame(1);// or await UniTask.Yield() or await Task.Delay(150);

        this.Broadcast(GameGuiEvents.LoadingScene);

        SceneManager.LoadSceneAsync("Game"); //maybe use this sometime else

        await UniTask.DelayFrame(1);

        await LoadGameState(Globals.LastGameStatePath);
    }

    private async UniTask LoadGameState(string filePath)
    {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        //TODO KORION IO
        string result = await ReadDataAsync<string>(filePath);
        if (result != null)
        {
            GameState gameState = JsonConvert.DeserializeObject<GameState>(result); //this also sets the singleton
            Debug.Log("Loaded GameState");
        }
        else
        {
            //KORION IO - what happens if we simply dont have a game to load --> prompts? xD
            PopupManager.ShowPrompt("error", "something_went_wrong");
            Debug.Log("loading GameState failed");
            DeleteSavegame();
        }
#else
        try
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    GameState gameState = JsonConvert.DeserializeObject<GameState>(reader.ReadToEnd());
                }
            }
        }
        catch (Exception ex)
        {
            this.LogError($"Error at TryLoad({filePath})", ex);
            PopupManager.ShowPrompt("error", "something_went_wrong");
            DeleteSavegame();
        }
#endif
    }

    private async UniTask<GameSetup> LoadGameSetup(string filePath)
    {
        GameSetup gameSetup;

#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        //TODO KORION IO
        Debug.Log("KORION: Sart loading Game setup");
        string result = await ReadDataAsync<string>(filePath);
        if (result != null)
        {
            Debug.Log("Loaded Setup");
            gameSetup = JsonConvert.DeserializeObject<GameSetup>(result); //Korion does string work? result = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        }
        else
        {
            //KORION IO - what happens if we simply dont have a game to load --> prompts? xD
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
            DeleteSavegame();
            return null;
        }
#endif
        return gameSetup;
    }

    internal void DeleteSavegame()
    {
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        Debug.Log("KORION: Deleting Savegame");
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
