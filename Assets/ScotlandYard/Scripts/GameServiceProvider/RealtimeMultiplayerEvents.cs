using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public sealed class RealtimeMultiplayerEvents
{

    static Queue<byte[]> pendingMessages = new Queue<byte[]>();

    // MESSAGE
    public bool IsMessageReceiverRegistered { get { return MessageReceived != null; } }

    public void Activate()
    {
        ClearSession();

        this.LogDebug("Activate");

        lock (pendingMessages)
        {
            GSP.Coroutiner.StartCoroutine(coCallReceivedMessages());
        }
    }

    public void ClearSession()
    {
        this.LogDebug("ClearSession");

        lock (pendingMessages)
        {

            pendingMessages.Clear();
            GSP.Coroutiner.StopCoroutine("coCallReceivedMessages");
        }
    }

    #region RECEIVE MESSAGE

    public void ReceiveMessage(byte[] m)
    {

        lock (pendingMessages)
        {
            if (!pendingMessages.Contains(m))
                pendingMessages.Enqueue(m);
        }

    }

    IEnumerator coCallReceivedMessages()
    {
        yield return new WaitForEndOfFrame(); // the coroutine is started before multiplayer is available -> wait a frame

        this.LogDebug("coCallReceivedMessages() START");
        while (GSP.IsMultiplayerRTAvailable)
        {
            if (!IsMessageReceiverRegistered)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            lock (pendingMessages)
            {
                int cnt = pendingMessages.Count;

                for (int i = 0; i < cnt; i++)
                {
                    byte[] msg = pendingMessages.Dequeue();
                    this.LogInfo("receive message - byte length: " + msg.Length);

                    if (GSP.MultiplayerRT.InternalMessageHandler != null) // Internal Message?
                    {
                        var m = GSP.MultiplayerRT.InternalMessageHandler.Unpack(msg);
                        if (m != null)
                        {
                            GSP.MultiplayerRT.ProcessInternalMessage(m);
                            continue;
                        }
                    }
                    
                    // external Message
                    {
                        if (!CallMessageReceived(msg))
                            pendingMessages.Enqueue(msg);
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        }
        this.LogDebug("coCallReceivedMessages() END");
    }

    public event Func<byte[], bool> MessageReceived;
    private bool CallMessageReceived(byte[] message)
    {
        bool consumed = false;

        if (MessageReceived != null)
        {
            foreach (var m in MessageReceived.GetInvocationList())
            {
                try
                {
                    consumed = (bool)m.DynamicInvoke(message) || consumed;
                }
                catch (Exception ex)
                {
                    consumed = true; // prevent spamming log with exception every frame
                    this.LogError(ex.InnerException);
                }
            }
        }
        return consumed;
    }

    #endregion

    #region MATCHMAKER

    public event Action MatchMakerCancelled;
    internal void CallMatchMakerCancelled()
    {
        if (MatchMakerCancelled != null)
            MatchMakerCancelled();
    }

    //public event Action<string> MatchMakingFailed;
    //internal void CallMatchMakingFailed(string msg)
    //{
    //    if (MatchMakingFailed != null)
    //        MatchMakingFailed(msg);
    //}

    public event Action MatchMakerFoundMatch;
    internal void CallMatchMakerFoundMatch()
    {
        if (MatchMakerFoundMatch != null)
            MatchMakerFoundMatch();
    }

    #endregion

    #region CONNECTION STATUS

    public event Action<string> PeerLeft;
    internal void CallPeerLeft(string participantID)
    {
        if (participantID == GSP.MultiplayerRT.OwnParticipantId)
        {
            CallSessionLeft();
        }
        else
        {
            if (PeerLeft != null)
                PeerLeft(participantID);

            if (GSP.MultiplayerRT.GetNumberOfParticipants(false) == 0)
            {
                GSP.Coroutiner.WaitAndDo(new WaitForSeconds(3), null, () =>
                {
                    if (GSP.MultiplayerRT != null && GSP.MultiplayerRT.IsConnected)
                        GSP.MultiplayerRT.Disconnect();
                });
            }
        }
    }

    public event Action SessionLeft;
    internal void CallSessionLeft()
    {
        if (SessionLeft != null)
            SessionLeft();
    }

    #endregion
}