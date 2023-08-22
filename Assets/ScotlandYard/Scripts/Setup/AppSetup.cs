using Newtonsoft.Json;
using Sunbow.Util.IO;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppSetup : MonoBehaviour
{
    static AppSetup instance;
    public static AppSetup Instance
    {
        get
        {
            if(instance == null)
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
        catch(Exception ex)
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
        catch(Exception ex)
        {
            this.LogError(ex);
            return false;
        }
    }


    private void Ensure(string settingsName, bool value)
    {
        if(!SettingsTable.Contains("value", settingsName))
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
    }

    public void LoadOrCreateStatsTable()
    {
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
        return File.Exists(Globals.LastGameSetupPath) && File.Exists(Globals.LastGameStatePath);
    }

    public void SaveGame()
    {
        string gameSetup = JsonConvert.SerializeObject(GameSetupBehaviour.Instance.Setup);
        SaveData(ref gameSetup, Globals.LastGameSetupPath);

        string gameState = JsonConvert.SerializeObject(GameState.Instance);
        SaveData(ref gameState, Globals.LastGameStatePath);

    }

    void SaveData(ref string data, string filePath)
    {
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
    }


    internal void LoadLastGame()
    {
        StartCoroutine(CoLoadLastGame());
    }

    IEnumerator CoLoadLastGame()
    {
        if (!TryLoad(Globals.LastGameSetupPath, out GameSetup setup))
        {
            yield break;
        }

        GameSetupBehaviour.Instance.Setup = setup;

        yield return new WaitForEndOfFrame();

        this.Broadcast(GameGuiEvents.LoadingScene);
        SceneManager.LoadSceneAsync("Game");
        
        yield return new WaitForEndOfFrame();

        // --- DON'T SIMPLY REMOVE THIS! ---
        // this looks very stupid, but the deserializing constructor assigns the fresh GameState to the singleton immediately
        if (!TryLoad(Globals.LastGameStatePath, out GameState _))
        {
            yield break;
        }
    }

    private bool TryLoad<T>(string filePath, out T result)
    {
        try
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(stream))
                {
                    result = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                }
            }
        }
        catch(Exception ex)
        {
            this.LogError($"Error at TryLoad({filePath})", ex);
            PopupManager.ShowPrompt("error", "something_went_wrong");
            result = default;
            DeleteSavegame();
            return false;
        }

        return true;
    }

    internal void DeleteSavegame()
    {
        if (File.Exists(Globals.LastGameSetupPath))
        {
            File.Delete(Globals.LastGameSetupPath);
        }

        if (File.Exists(Globals.LastGameStatePath))
        {
            File.Delete(Globals.LastGameStatePath);
        }
    }
}
