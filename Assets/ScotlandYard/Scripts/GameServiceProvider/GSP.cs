using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// GSP stands for "Game Service Provider".
/// </summary>
public static class GSP
{
    internal static EventProvider Coroutiner { get; private set; }


    #region Status

    public static bool IsStatusAvailable { get { return status != null && status.IsAvailable; } }
    /// <summary>
    /// Status handles the connection to a game service (GC / GPG / ?) as well as the achievements and leaderboards.
    /// </summary>
    public static SocialStatus Status { get { return status; } }
    static SocialStatus status;

    #endregion

    #region Multiplayer

    public static bool AllowInvites { get; set; }

    public static bool IsMultiplayerRTAvailable { get { return multiplayerRT != null && multiplayerRT.IsAvailable; } }

    /// <summary>
    /// RT stands for "Realtime", so it is the Realtime Multiplayer Provider.
    /// </summary>
    public static RealtimeMultiplayerBaseProvider MultiplayerRT { get { return (multiplayerRT != null) ? multiplayerRT : fallbackMultiplayerRT; } }
    static RealtimeMultiplayerBaseProvider multiplayerRT, fallbackMultiplayerRT;

    /// <summary>
    /// RT stands for "Realtime", so here are the events called by the Realtime Multiplayer Provider.
    /// </summary>
    public static RealtimeMultiplayerEvents MultiplayerRTEvents { get { return multiplayerRTEvents; } }
    static RealtimeMultiplayerEvents multiplayerRTEvents = new RealtimeMultiplayerEvents();

    static RealtimeMultiplayerBaseProvider[] allMuliplayerRTProviders;

    #endregion

    /// <summary>
    /// This variable should be set before a game is started. It might not work with all Multiplayer types and also not the same way.
    /// </summary>
    public static bool EnableVoiceChatIfPossible { get { return enableVoiceChat; }
        set
        {
            enableVoiceChat = value; 
            if(GSP.IsMultiplayerRTAvailable && GSP.multiplayerRT.VoiceChat.IsAvailable)
            {
                if (value)
                    GSP.MultiplayerRT.VoiceChat.StartChat();
                else
                    GSP.MultiplayerRT.VoiceChat.StopChat();
            }
        } 
    }
    static bool enableVoiceChat;

    static GSP()
    {
        Log.info("GSP", "Static Constructor Called!");

        if (GSP.Coroutiner == null)
        {
            GameObject old = GameObject.Find("__Coroutiner");
            if (old != null)
                GameObject.Destroy(old);


            var go = new GameObject("__Coroutiner");
            
            var identifier = go.AddComponent<Identifier>();
            identifier.GameID = int.MinValue;

            var evtProvider = go.AddComponent<EventProvider>();

            GSP.Coroutiner = evtProvider;
            GameObject.DontDestroyOnLoad(go);
        }
    }

    /// <summary>
    /// Initializes a Game Service Provider (like Google Play Game Services / Game Center) and all supported multiplayer providers for the given Platform.
    /// </summary>
    /// <param name="status">the Status Game Service Provider for achievements and leaderboards</param>
    /// <param name="multiplayerRTProviders">all supported Realtime Multiplayer options (like GPG or GC multiplayer and Bluetooth)</param>
    public static void Init(SocialStatus status, params RealtimeMultiplayerBaseProvider[] multiplayerRTProviders)
    {
        Log.info("GSP", "Initialize");

        GSP.status = status;
        GSP.allMuliplayerRTProviders = multiplayerRTProviders;
        GSP.status.Initialize();

        foreach(RealtimeMultiplayerBaseProvider provider in GSP.allMuliplayerRTProviders)
        {
            if (provider.AlwaysActive)
                provider.Init();
        }

        fallbackMultiplayerRT = allMuliplayerRTProviders.FirstOrDefault(o => o.AlwaysActive);

        AllowInvites = true;
    }


    /// <summary>
    /// Activates a Realtime Multiplayer Provider which has been declared in the init function.
    /// </summary>
    /// <typeparam name="T">The type of the Realtime Multiplayer Provider to use.</typeparam>
    public static void ActivateMultiplayerRT<T>()
        where T : RealtimeMultiplayerBaseProvider
    {
        var m = allMuliplayerRTProviders.SingleOrDefault((o) => o.GetType() == typeof(T));

        if (m != null)
        {
            // always activate the events to clear old messages 
            // which where not processed so far.
            multiplayerRTEvents.Activate();

            if (m != multiplayerRT)
            {
                if (multiplayerRT != null && !multiplayerRT.AlwaysActive)
                    multiplayerRT.CleanUp();

                multiplayerRT = m;

                if(!multiplayerRT.AlwaysActive)
                    multiplayerRT.Init();
            }
        }
        else
        {
            Log.error("GSP", string.Format("ActivateMultiplayerRT<{0}>(): MultiplayerRTProvider of type {0} was not declared on Init() once", typeof(T).Name));
        }
    }

    /// <summary>
    /// Deactivates the current Realtime Multiplayer Provider:
    /// - unregisters all internal event connection
    /// - clears the currently pending messages
    /// - IsMultiplayerRTAvailable is false after calling this
    /// </summary>
    public static void DeactivateMultiplayerRT()
    {
        multiplayerRTEvents.ClearSession();

        if (multiplayerRT == null)
            return;

        if(!multiplayerRT.AlwaysActive)
            multiplayerRT.CleanUp();


        multiplayerRT = null;
    }

}
