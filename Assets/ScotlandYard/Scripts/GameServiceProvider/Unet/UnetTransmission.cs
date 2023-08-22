using System.Collections.Generic;
using LostPolygon.AndroidBluetoothMultiplayer;
using UnityEngine;
using UnityEngine.Networking;

// The high level API classes are deprecated and will be removed in the future.
#pragma warning disable CS0618
public class UnetTransmission : AndroidBluetoothNetworkManager // replacement for RpcSync
{
    [SerializeField] public AndroidBluetoothNetworkManagerHelper NetworkManagerHelper;

#if UNITY_ANDROID
    private static UnetTransmission _instance = null;
    public static UnetTransmission Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("UnetTransmission");
                _instance = GameObject.Instantiate(prefab).GetComponent<UnetTransmission>();
            }
            return _instance;
        }
    }
    
    #region Server
    public override void OnStartServer()
    {
        base.OnStartServer();
        this.LogInfo("UnetTransmission.OnStartServer");
        // Register the handler for the UnetByteArrayMessage that is sent from client to server
        NetworkServer.RegisterHandler(UnetByteArrayMessage.MessageType, OnServerReceivedMessage);
    }

    private void OnServerReceivedMessage(NetworkMessage netMsg)
    {
        UnetByteArrayMessage message = netMsg.ReadMessage<UnetByteArrayMessage>();
        GSP.MultiplayerRTEvents.ReceiveMessage(message.Bytes);
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        this.LogInfo("UnetTransmission.OnServerReady");
    }
    #endregion
    
    #region Client
    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
        this.LogInfo("UnetTransmission.OnStartClient");

        if (UnetNetworking.IsServer)
        {
            return;
        }
        // Register the handler for UnetByteArrayMessage that is sent from server to clients
        client.RegisterHandler(UnetByteArrayMessage.MessageType,OnClientReceivedMessage);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        this.LogInfo("UnetTransmission.OnClientConnect");
        
        // this will trigger player ready event
        UnetNetworking.OnCreatedPlayer();
    }

    private void OnClientReceivedMessage(NetworkMessage netMsg)
    {
        UnetByteArrayMessage message = netMsg.ReadMessage<UnetByteArrayMessage>();
        GSP.MultiplayerRTEvents.ReceiveMessage(message.Bytes);
    }
    #endregion
    
    private static readonly MessageHandler MessageHandler = new MessageHandler(255, new PlayerReady(), new ServerToClientMatchReady());
    public void SendInternalNetworkMessage(IMessage msg)
    {
        this.LogInfo("SendInternalNetworkMessage... " + msg);
        SendNetworkMessageToAll(MessageHandler.Pack(msg));
    }
    
    public void SendNetworkMessageToAll(byte[] msg)
    {
        this.LogDebug("Sending Network Message... byte length: " + msg.Length);
        SendMessage(msg);
    }

    public void SendNetworkMessageToServer(byte[] msg)
    {
        if (UnetNetworking.IsServer)
        {
            return;
        }

        this.LogDebug("Sending Network Message to Server... byte length: " + msg.Length);
        SendMessage(msg);
    }
    
    private void SendMessage(byte[] msg)
    {
        UnetByteArrayMessage message = new UnetByteArrayMessage
        {
            Bytes = msg
        };

        if (UnetNetworking.IsServer)
        {
            SendMessageToConnections(message, NetworkServer.connections);
        }
        else
        {
            if (!NetworkManager.singleton.isNetworkActive)
            {
                // this is called on the client, if the server quits the game => notify that the server quit the game
                Bluetooth.Android.MultiplayerRT bluetoothMultiplayer = (Bluetooth.Android.MultiplayerRT) GSP.MultiplayerRT;
                string participant = bluetoothMultiplayer.GetNonLocalParticipantID();
                GSP.MultiplayerRTEvents.CallPeerLeft(participant);
            }
            else
            {
                NetworkManager.singleton.client.Send(UnetByteArrayMessage.MessageType, message);
            }
        }
    }

    private void SendMessageToConnections(UnetByteArrayMessage message, IEnumerable<NetworkConnection> connections)
    {
        //string debug = "Sending to connections = ";
        foreach (NetworkConnection connection in connections)
        {
            if (connection == null)
            {
                continue;
            }

            // Sending to connections =  (localClient,-1,0) (::ffff:127.0.0.1,  0,     1)
            //debug += " (" + connection.address + "," + connection.hostId + "," + connection.connectionId + ")";
    
            if (connection.connectionId == 0)
            {
                continue; // skip sending from server to server
            }

            connection.Send(UnetByteArrayMessage.MessageType, message);
        }
        //Debug.Log(debug);
    }
    #endif
}
