using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum MultiplayerMode
{
    PeerToPeer = 0,
    ServerClient = 1,
}

public abstract class RealtimeMultiplayerBaseProvider
{
    protected virtual bool PreventScreenSleepDuringPlay { get { return true; } }

    protected RealtimeMultiplayerEvents events { get { return GSP.MultiplayerRTEvents; } }

    public abstract VoiceChatBaseProvider VoiceChat { get; }

    internal virtual MessageHandler InternalMessageHandler { get { return null; } }

    public virtual MultiplayerMode Mode { get { return MultiplayerMode.PeerToPeer; } }

    /// <summary>
    /// When true the provider is initialized on startup and never cleaned up
    /// </summary>
    public virtual bool AlwaysActive { get { return false; } }
    public abstract bool IsHost { get; }

    protected bool isInitialized;
    public virtual bool IsAvailable { get { return isInitialized; } }

    private bool isConnected;
    public bool IsConnected { get { return isConnected; } }

    public abstract string OwnParticipantId { get; }
    
    public abstract int MaxAllowedConnections { get; }

    public abstract void SetHost(object host);


    internal virtual void Init()
    {
        this.LogInfo("Init");

        isInitialized = true;
    }

    internal void CleanUp()
    {
        if (!isInitialized) // probably a recursive call...
            return;

        this.LogInfo("CleanUp()");

        isInitialized = false;
        DoCleanUp();
        DisconnectFromSession();

        if (PreventScreenSleepDuringPlay)
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }
    protected abstract void DoCleanUp();

    public virtual int GetNumberOfParticipants(bool includeSelf)
    {
        return AllParticipantIds(includeSelf).Count();
    }

    public abstract IEnumerable<string> AllParticipantIds(bool includeSelf);

    public abstract void ShowMatchMaker(int minPlayerCount, int maxPlayerCount);

    public abstract void StartQuickMatch(int playerCount);

    public abstract void SendMessageToOthers(byte[] message, bool reliable);
    public abstract void SendMessageToHost(byte[] message, bool reliable);
    public abstract void SendMessageToPeers(byte[] message, bool reliable, params string[] participantIds);

    public void Disconnect()
    {
        if (VoiceChat.IsAvailable)
            VoiceChat.StopChat();

        if (isConnected)
        {
            DisconnectFromSession();
            events.CallSessionLeft();
        }

        GSP.DeactivateMultiplayerRT();
    }
    protected virtual void DisconnectFromSession()
    {
        this.LogDebug("Disconnect From Session!");
            
        isConnected = false;

        if (PreventScreenSleepDuringPlay)
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    protected void ConnectedToMatch()
    {
        if(PreventScreenSleepDuringPlay)
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

        isConnected = true;
        events.CallMatchMakerFoundMatch();
    }

    internal virtual void ProcessInternalMessage(IMessage msg)
    {
        // intentionally left blank
    }

    public virtual void ShowInvitations()
    {
        this.LogWarn("ShowInvitiations: No invitation inbox available for this provider.");
    }
}
