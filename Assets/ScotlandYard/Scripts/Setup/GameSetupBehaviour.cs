using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum GameMode
{
    Undefined = 0,

    // Tutorials
    TutorialBasics = 1,
    TutorialDetective = 2,
    TutorialMrX = 3,

    // Single Player, Multiplayer on one device
    HotSeat = 4,

    // GameCenter, Google Play Game Servicers or Bluetooth
    Network = 5,

    // KORION: Multiplayer with more than one controller
    MultiController = 6,
}

public static class GameModeExtensions
{
    public static bool IsTutorial(this GameMode self)
    {
        return self == GameMode.TutorialBasics || self == GameMode.TutorialDetective || self == GameMode.TutorialMrX;
    }
}

[Serializable]
public class GameSetup
{
    public float RoundTime;
    public GameMode Mode;
    public PlayerSetup MrXSetup = new PlayerSetup(PlayerController.Human, 0);
    public PlayerSetup[] DetectiveSetups =
    { 
        new PlayerSetup(PlayerController.Ai, 1),
        new PlayerSetup(PlayerController.Ai, 2),
        new PlayerSetup(PlayerController.Ai, 3),
        new PlayerSetup(PlayerController.Ai, 4),
        new PlayerSetup(PlayerController.Ai, 5),
    };
}

[AddComponentMenu("Scripts/GameSetup")]
public class GameSetupBehaviour : MonoBehaviour
{
    public static GameSetupBehaviour Instance { get; private set; }
    
    public GameSetup Setup;

    public bool IsNetworkGame => Setup.Mode == GameMode.Network;

    public GameObject PrefabMrX, PrefabDetective;

    public Font StationFont;

    public LocalPlayerController LocalPlayer = new LocalPlayerController();

    private int _humanPlayers = 0;
    public int HumanPlayers => _humanPlayers;

    public PlayerSetup GetPlayer(int id)
    {
        if (id == 0)
        {
            return Setup.MrXSetup;
        }
        
        return Setup.DetectiveSetups[id - 1];
    }

    public IEnumerable<PlayerSetup> IterateAllPlayers(bool includeNonSet)
    {
        for (int i = 0; i <= Setup.DetectiveSetups.Length; i++)
            if(includeNonSet || GetPlayer(i).Controller != PlayerController.None)
                yield return GetPlayer(i);
    }


    public int LastPlayerID()
    {
        int result = 1;

        for (int i = Setup.DetectiveSetups.Length - 1; i >= 0; i--)
        {
            if (Setup.DetectiveSetups[i].Controller != PlayerController.None)
            {
                return Setup.DetectiveSetups[i].PlayerId;
            }
        }

        return result;
    }

    public void OnPlayerSetupFinalized()
    {
        _humanPlayers = Setup.MrXSetup.Controller == PlayerController.Human ? 1 : 0;

        foreach (var player in Setup.DetectiveSetups)
        {
            if (player.Controller == PlayerController.Human)
                ++_humanPlayers;
        }
    }
        
    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        if (Instance == null)
            Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        Reset();        
    }

    public void CreatePlayers()
    {
        this.LogDebug("CreatePlayer()");
        var parent = GameObject.Find("Players");
        
        // Create Mr. X
        var goX = NGUITools.AddChild(parent, PrefabMrX);// GameObject.(PrefabMrX) as GameObject;//new GameObject("MrX").AddComponent<MrX>();

        var mrX = goX.GetComponent<MrX>();
        mrX.PlayerInfo = Setup.MrXSetup;
        var idx = goX.GetComponent<Identifier>();
        idx.IdShift = 100;
        idx.GameID = 0;

        // Create All Detectives
        foreach (var d in Setup.DetectiveSetups)
        {
            if (d.Controller == PlayerController.None)
                continue;

            var go = NGUITools.AddChild(parent, PrefabDetective); //GameObject.Instantiate(PrefabDetective) as GameObject;
            go.name = "Detective_" + d.PlayerId;

            var detective = go.GetComponent<Detective>();//new GameObject("Detective_" + d.PlayerId).AddComponent<Detective>();
            detective.PlayerInfo = d;

            var id = detective.GetComponent<Identifier>();
            id.IdShift = 100;
            id.GameID = detective.PlayerId;

            var sprite = go.GetComponentInChildren<UISprite>();
            sprite.spriteName = d.Color.GetActorSpriteName();

            Debug.Log("Detective created with id " + id);
        }

    }

    public int CountDetectiveControlTypes(PlayerController controlType)
    {
        int cnt = Setup.DetectiveSetups.Count((o) => o.Controller == controlType);
        this.LogInfo(string.Format("Detective Count of Controller Type {0}: {1}", controlType, cnt));
        return cnt;
    }


    public void Reset()
    {
        foreach(PlayerSetup d in Setup.DetectiveSetups)
        {
            d.DisplayName = "Detective";
            ResetPlayer(d);
        }

        Setup.MrXSetup.DisplayName = "Mr X";
        ResetPlayer(Setup.MrXSetup);
    }

    void ResetPlayer(PlayerSetup player)
    {
        player.Controller = player.PlayerId == 0 ? PlayerController.Human : PlayerController.Ai;
        player.Difficulty = AiDifficulty.Medium;
        player.StartAtStationId = 0;
        player.ControllingParticipantID = string.Empty;
    }

    public string PrintSettings()
    {
        StringBuilder sb = new StringBuilder("Round Time: ").Append((int)Setup.RoundTime).AppendLine()
            .Append("Mr X: ").Append(Setup.MrXSetup.Controller).Append(" - ").Append(Setup.MrXSetup.Difficulty);

        foreach(var det in Setup.DetectiveSetups)
            sb.AppendLine().Append("Detective ").Append(det.PlayerId).Append(": ").Append(det.Controller).Append(" - ").Append(det.Difficulty);

        return sb.ToString();
    }

    public bool IsMultiControllerGame()
    {
        return Setup.Mode == GameMode.MultiController;
    }
}
