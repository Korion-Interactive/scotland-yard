using System;
using UnityEngine;

#if false // replaced with UnetNetworking & UnetTransmission

[RequireComponent(typeof(NetworkView))]
public class RpcSynch : MonoBehaviour
{
    public static event Action<NetworkPlayer> PlayerDisconnectedEvent;
    public static event Action<NetworkConnectionError> FailedToConnectEvent;
    public static event Action ConnectedToServerEvent;
    public static event Action DisconnectedFromServerEvent;
    public static event Action ServerInitializedEvent;
    public static event Action SynchCreatedEvent;

    public static MessageHandler MessageHandler = new MessageHandler(255, new PlayerReady(), new ServerToClientMatchReady());

    void Awake()
    {
        this.LogInfo("Network Communication object instanciated! " + this.name);
    }

    void Start()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);

        if (SynchCreatedEvent != null)
            SynchCreatedEvent();
    }

	// disabled deprecation warning concerning UnityEngine.RPC
	#pragma warning disable 618

    [RPC]
    public void ReceiveMessage(byte[] msg)
    {
        RpcSynch.ReceiveRawMessage(msg);
    }

    public static void ReceiveRawMessage(byte[] msg)
    {
        Log.info("RpcSynch", "Receiving Network Message... byte length: " + msg.Length);
        GSP.MultiplayerRTEvents.ReceiveMessage(msg);
    }

    internal void SendInternalNetworkMessage(IMessage msg)
    {
        SendNetworkMessageToAll(MessageHandler.Pack(msg));
    }

    public void SendNetworkMessageToAll(byte[] msg)
    {
        this.LogInfo("Sending Network Message... byte length: " + msg.Length);
        GetComponent<NetworkView>().RPC("ReceiveMessage", RPCMode.Others, msg);
    }

    public void SendNetworkMessageToServer(byte[] msg)
    {
        this.LogInfo("Sending Network Message to Server... byte length: " + msg.Length);
        GetComponent<NetworkView>().RPC("ReceiveMessage", RPCMode.Server, msg);
    }

	#pragma warning restore 618

    public void SendNetworkMessageToIds(byte[] msg, params string[] participantIds)
    {
        this.LogError("Sending Network Message to Ids: not supported. sending to all others");
        SendNetworkMessageToAll(msg);
    }

    void OnDestroy()
    {
        this.LogInfo("Destroyed");
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        this.LogInfo("Player Disconnected");
        if (PlayerDisconnectedEvent != null)
            PlayerDisconnectedEvent(player);
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        this.LogInfo("Failed to Connect");
        if (FailedToConnectEvent != null)
            FailedToConnectEvent(error);
    }

    void OnDisconnectedFromServer()
    {
        this.LogInfo("Disconnected from Server");
        if (DisconnectedFromServerEvent != null)
            DisconnectedFromServerEvent();
    }

    void OnConnectedToServer()
    {
        this.LogInfo("Connected to Server");
        if (ConnectedToServerEvent != null)
            ConnectedToServerEvent();
    }

    void OnServerInitialized()
    {
        this.LogInfo("Server Initialized");
        if (ServerInitializedEvent != null)
            ServerInitializedEvent();
    }

}

#endif
