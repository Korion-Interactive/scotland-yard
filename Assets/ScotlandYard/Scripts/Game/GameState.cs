using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class GameState : ISerializable
{
    static GameState instance;
    public static GameState Instance
    {
        get
        {
            if (instance == null)
                instance = new GameState();

            return instance;
        }
    }

    public static bool HasInstance { get { return instance != null; } }
    public static void ReleaseInstance()
    {
        instance = null;
    }

    int round = 0;
    public int Round { get{ return round; } }

    public PlayerBase CurrentPlayer
    {
        get
        {
            if (players.Count <= currentPlayerIndex)
                return null;

            return players[currentPlayerIndex];
        }
    }

    public MrX MrX { get { return players[0] as MrX; } }

    public bool IsGamePaused
    { 
        get { return isGamePaused || isTutorialPaused; } 
        //set 
        //{
        //    if (isGamePaused != value)
        //    {
        //        isGamePaused = value;
        //        this.Broadcast<GameEvents>(GameEvents.ChangeGamePausing, null, new BaseArgs());
        //    }
        //}
    }
    public bool IsTutorialPause { get { return isTutorialPaused; } }
    public bool IsMenuPause { get { return isGamePaused; } }
    bool isGamePaused = false;
    bool isTutorialPaused = false;

    public void SetPausing(bool pause, bool tutorial)
    {
        if(tutorial)
        {
            if (isTutorialPaused != pause)
            {
                isTutorialPaused = pause;
                this.Broadcast<GameEvents>(GameEvents.ChangeGamePausing, null, new BaseArgs());
            }
        }
        else
        {
            if (isGamePaused != pause)
            {
                isGamePaused = pause;
                this.Broadcast<GameEvents>(GameEvents.ChangeGamePausing, null, new BaseArgs());
            }
        }

    }

    public bool IsGameOver { get; set; }

    public bool HasBeenLoadedFromFile;
    public bool IsInitialized { get; private set; }

    int currentPlayerIndex = 0;

    List<PlayerBase> players = new List<PlayerBase>();
    
    private int _humanPlayers = 0;
    
    public int HumanPlayers => _humanPlayers;

    [HideInInspector]
    public HashSet<Station> PossibleMrXLocations = new HashSet<Station>();

    public MoveHistory MoveHistory = new MoveHistory();

    public void Init()
    {
        HasBeenLoadedFromFile = false;
        PossibleMrXLocations = new HashSet<Station>();
        round = 1;

        MoveHistory.Entries.Clear();
        players.Clear();

        // COLLECT PLAYERS
        var raw = GameObject.FindObjectsOfType<PlayerBase>();
        foreach (var p in raw)
        {
            if (p.enabled && p.gameObject.activeInHierarchy)
                players.Add(p);
        }
        this.Assert(players.Count > 1);

        players.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));

        // MR X
        var mrX = GameObject.FindObjectOfType<MrX>();
        this.Assert(mrX != null);

        mrX.BlackTicketStartAmount = players.Count - 1;

        // INIT PLAYERS
        foreach (var p in players)
        {
            if (p.PlayerInfo.Controller == PlayerController.Human)
            {
                _humanPlayers++;
            }
            p.Initialize();
        }

        currentPlayerIndex = 0;

        SetPausing(false, false);
        SetPausing(false, true);

        IsInitialized = true;
    }


    public void NextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        if (currentPlayerIndex == 0)
        {
            round++;
        }
    }

    public IEnumerable<Detective> DetectivesIterator()
    {
        for (int i = 1; i < players.Count; i++) // player 0 is always mrX
            yield return players[i] as Detective;
    }

    public IEnumerable<PlayerBase> PlayerIterator()
    {
        for (int i = 0; i < players.Count; i++)
            yield return players[i];
    }

    //public static GameState LoadLastGame()
    //{
    //    GameState result;
    //    using (FileStream stream = new FileStream(Application.persistentDataPath + "/gamestate.json", FileMode.Open, FileAccess.Read))
    //    {
    //        using (var reader = new StreamReader(stream))
    //        {
    //            result = JsonConvert.DeserializeObject<GameState>(reader.ReadToEnd());
    //            reader.Close();
    //        }
    //        stream.Close();
    //    }

    //    result.Broadcast<GameEvents>(GameEvents.GameLoaded, null, new BaseArgs());

    //    return result;
    //}

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        //info.AddValue("GameSetup", GameSetup.Instance, typeof(GameSetup));

        info.AddValue("Round", round);
        info.AddValue("CurrentPlayerIdx", currentPlayerIndex);

        info.AddValue("MrXSpecialState", MrX.MrXState);
        info.AddValue("MrXPlayerState", MrX.PlayerState);

        info.AddValue("Detectives", DetectivesIterator().Select(o => o.PlayerState).ToArray());

        info.AddValue("MoveHistory", MoveHistory);
        info.AddValue("PossibleMrXLocations", PossibleMrXLocations.Select(o => o.Id).ToArray());
    }

    public GameState() { }
    public GameState(SerializationInfo info, StreamingContext context)
    {
        instance = this;

        // Wait for GameStarter, then initialize ... little hacky
        GameSetupBehaviour.Instance.WaitAndDo(new WaitForEndOfFrame(), () => players != null && players.Count > 0, () =>
            {
                HasBeenLoadedFromFile = true;

                this.round = (int)info.GetValue("Round", typeof(int));
                this.currentPlayerIndex = (int)info.GetValue("CurrentPlayerIdx", typeof(int));

                MrXState mrXSpecialState = (MrXState)info.GetValue("MrXSpecialState", typeof(MrXState));
                PlayerState mrXPlayerState = (PlayerState)info.GetValue("MrXPlayerState", typeof(PlayerState));

                PlayerState[] dets = (PlayerState[])info.GetValue("Detectives", typeof(PlayerState[]));

                MrX.MrXState = mrXSpecialState;
                MrX.PlayerState = mrXPlayerState;

                for (int i = 1; i < players.Count; i++)
                {
                    players[i].PlayerState = dets[i - 1];
                }

                this.MoveHistory = (MoveHistory)info.GetValue("MoveHistory", typeof(MoveHistory));
                int[] mrXLocations = (int[])info.GetValue("PossibleMrXLocations", typeof(int[]));

                foreach (int id in mrXLocations)
                    PossibleMrXLocations.Add(Station.FindStation(id));


                IsInitialized = true;

                this.Broadcast<GameEvents>(GameEvents.GameLoaded, null, new BaseArgs());
            });
    }

}
