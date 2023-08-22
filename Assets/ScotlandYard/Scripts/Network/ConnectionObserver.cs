using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class ConnectionObserver : NetworkSystem<ConnectionEvent, ConnectionObserver>
{
    protected override string expectedParentName { get { return null; } }

    public static void CreateInstance()
    {
        if(ConnectionObserver.instance != null)
        {
            Log.info("PlayerConnectionObserver.CreateInstance()", "Instance already exists - skip");
            return;
        }

        GameObject go = new GameObject("__ConnectionObserver");
        instance = go.AddComponent<ConnectionObserver>();
        go.AddComponent<Identifier>().GenerateIDFromHashcode();

        go.SetActive(true);
        GameObject.DontDestroyOnLoad(go);
    }

    protected override byte Context { get { return Globals.Net_Context_Connection; } }

    List<RollHostDiceArgs> roomParticipantHostOrder = new List<RollHostDiceArgs>();
    //public string HostID { get { return (roomParticipantHostOrder.Count > 0) ? roomParticipantHostOrder[0].ParticipantId : null; } }
    public bool IsHost { get { return !GSP.IsMultiplayerRTAvailable || GSP.MultiplayerRT.IsHost;/*HostID == GSP.MultiplayerRT.OwnParticipantId*/; } }
    
    protected override void RegisterEvents()
    {
        //ListenTo(ConnectionEvent.ClientServerMatchStarted, (args) => { if (GSP.MultiplayerRT.IsHost) SendEvent(ConnectionEvent.ClientServerMatchStarted, args); });

        GSP.MultiplayerRTEvents.MatchMakerFoundMatch += MatchFound;
        GSP.MultiplayerRTEvents.PeerLeft += PeerLeft;
        GSP.MultiplayerRTEvents.SessionLeft += SessionLeft;
    }

    protected override void OnDestroy()
    {
        this.LogInfo("Connection Observer destroyed!");
        base.OnDestroy();
        GSP.MultiplayerRTEvents.MatchMakerFoundMatch -= MatchFound;
        GSP.MultiplayerRTEvents.PeerLeft -= PeerLeft;
        GSP.MultiplayerRTEvents.SessionLeft -= SessionLeft;
    }

    private void SessionLeft()
    {
        this.LogInfo("Session Left");
        StartCoroutine(coSessionLeft());
    }
    IEnumerator coSessionLeft()
    {
        var conArgs = new ConnectionArgs();

        GameSetupBehaviour.Instance.Setup.Mode = GameMode.Undefined;

        if(GSP.MultiplayerRT != null)
            GSP.MultiplayerRT.Disconnect();

        GSP.DeactivateMultiplayerRT();

        // HACK
        // Wait a bit... if last peer disconnects we want to track it before we autodisconnect due to lack of players
        yield return new WaitForSeconds(0.5f);


        roomParticipantHostOrder.Clear();
        Broadcast(ConnectionEvent.SessionLeft, conArgs);
    }

    private void PeerLeft(string participantID)
    {
        RollHostDiceArgs args = roomParticipantHostOrder.FirstOrDefault((o) => o.ParticipantId == participantID);

        if(args == null)
        {
            this.LogWarn("Unknown peer left: " + participantID);
            return;
        }
        this.LogInfo("Peer Left: " + participantID);

        int idx = roomParticipantHostOrder.IndexOf(args);
        roomParticipantHostOrder.RemoveAt(idx);

        Broadcast(ConnectionEvent.PeerLeft, new ConnectionArgs() { ParticipantId = participantID });

        if(idx == 0) // host has left
        {
            UpdateHost();
        }
    }

    private void MatchFound()
    {
        this.LogInfo("Found Match");
        roomParticipantHostOrder.Clear();

        int randomJoinNumber = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        RollHostDiceArgs args = new RollHostDiceArgs()
        {
            PlayerName = (GSP.IsStatusAvailable) ? GSP.Status.PlayerName : "Mr Incognito",
            Number = randomJoinNumber, 
            RelatedObject = this.gameObject,
        };
        PlayerJoin(args);

        SendEvent(ConnectionEvent.FindHost, args);
    }

    protected override BaseArgs ArgsFactory(ConnectionEvent eventType)
    {
        switch(eventType)
        {
            case ConnectionEvent.FindHost:
                return new RollHostDiceArgs();
            default:
                return new BaseArgs();
        }
    }

    protected override bool MessageReceivedSuccessfully(ConnectionEvent eventType, BaseArgs args)
    {
        this.LogInfo("message received successfully " + eventType);

        switch(eventType)
        {
            case ConnectionEvent.FindHost:
                PlayerJoin(args as RollHostDiceArgs);
                break;
            //case ConnectionEvent.ClientServerMatchStarted:
            //    Broadcast(ConnectionEvent.ClientServerMatchStarted);
            //    break;
        }

        return true;
    }

    private void PlayerJoin(RollHostDiceArgs args)
    {
        if (roomParticipantHostOrder.FirstOrDefault((o) => o.ParticipantId == args.ParticipantId) != null)
        {
            this.LogWarn("PlayerJoin: participant already registered: " + args.ParticipantId);
            return;
        }

        roomParticipantHostOrder.Add(args);

        if(roomParticipantHostOrder.Count >= GSP.MultiplayerRT.GetNumberOfParticipants(true))
        {
            roomParticipantHostOrder.Sort((a, b) =>
                {
                    if (a.Number == b.Number)
                        return a.ParticipantId.CompareTo(b.ParticipantId);

                    return a.Number.CompareTo(b.Number);
                });

            UpdateHost();
        }
    }

    void UpdateHost()
    {
        if (roomParticipantHostOrder.Count == 0)
            return;

        if(GSP.IsMultiplayerRTAvailable)
            GSP.MultiplayerRT.SetHost(roomParticipantHostOrder[0].ParticipantId);

        Broadcast(ConnectionEvent.HostChanged, roomParticipantHostOrder[0]);
    }

    public IEnumerable<RollHostDiceArgs> GetParticipantsOrdered()
    {
        return roomParticipantHostOrder;//.Select((o) => o.ParticipantId);
    }
}
