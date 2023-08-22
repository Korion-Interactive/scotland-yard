using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_IOS
//namespace GSP
//{
    public class ReliableNetworkLayer
    {
        RealtimeMultiplayerBaseProvider multiplayerRT;
        MessageHandler messageHandler { get { return multiplayerRT.InternalMessageHandler; } }

        HashSet<string> receivedMessages = new HashSet<string>();

        Dictionary<string, ResponseCollector> deliveringMessages = new Dictionary<string, ResponseCollector>();

        public ReliableNetworkLayer(RealtimeMultiplayerBaseProvider multiplayerProvider)
        {
            this.multiplayerRT = multiplayerProvider;

            this.messageHandler.AddMessageTypes(
                new ReliableMessage(), 
                new ReliableMessageReceivedResponse());
        }

        public void Activate()
        {
            Cleanup();
            GSP.Coroutiner.StartCoroutine(CoUpdate());
        }

        public void Deactivate()
        {
            Cleanup();
            GSP.Coroutiner.StopCoroutine(CoUpdate());
        }

        void Cleanup()
        {
            receivedMessages.Clear();
            deliveringMessages.Clear();

            ReliableMessage.Cleanup();
        }

        public void SendMessageToOthers(byte[] message)
        {
            ReliableMessage msg = new ReliableMessage(multiplayerRT.OwnParticipantId, message);
            SendMessage(msg, false);
        }

        public void SendMessageToHost(byte[] message)
        {
            ReliableMessage msg = new ReliableMessage(multiplayerRT.OwnParticipantId, message);
            SendMessage(msg, true);
        }

        void SendMessage(ReliableMessage msg, bool toHostOnly)
        {
            byte[] reliableMessage = messageHandler.Pack(msg);

            if(toHostOnly)
                multiplayerRT.SendMessageToHost(reliableMessage, false);
            else
                multiplayerRT.SendMessageToOthers(reliableMessage, false);

			if(!deliveringMessages.ContainsKey(msg.ReliableID))
            	deliveringMessages.Add(msg.ReliableID, new ResponseCollector(msg, toHostOnly));
        }

        public void InternalMessageReceived(IMessage message)
        {
            switch((InternalMessageID)message.MessageTypeId)
            {
                case InternalMessageID.ReliableMessage:

                    ReliableMessage rel = message as ReliableMessage;
                    if(rel != null)
                    {
                        // send a response to notify the other side that the message was received successfully.
                        // do this even we already did this before to ensure the message was received by the other side.
                        var response = new ReliableMessageReceivedResponse(rel.ReliableID, multiplayerRT.OwnParticipantId);
                        byte[] re = messageHandler.Pack(response);
                        multiplayerRT.SendMessageToPeers(re, false, rel.SenderID);

                        // check if we received a message we already received earlier
                        if (receivedMessages.Contains(rel.ReliableID))
                            return;

                        receivedMessages.Add(rel.ReliableID);

                        GSP.MultiplayerRTEvents.ReceiveMessage(rel.InnerMessage);
                    }
                    break;

                case InternalMessageID.ReliableMessageResponse:

                    ReliableMessageReceivedResponse res = message as ReliableMessageReceivedResponse;
                    if(res != null && deliveringMessages.ContainsKey(res.ReliableMessageID))
                    {
                        ResponseCollector rc = deliveringMessages[res.ReliableMessageID];
                        rc.ResponseReceived(res.ParticipantID, multiplayerRT);

                        if(rc.MessageDeliveredSuccessfully)
                        {
                            deliveringMessages.Remove(res.ReliableMessageID);
                        }
                    }
                    break;
            }
        }

        IEnumerator CoUpdate()
        {
            while(true)
			{
				yield return new WaitForSeconds(0.2f);

                foreach (var val in deliveringMessages.Values.ToArray())
                {
                    this.LogInfo("Resending Message");

                    // check again before resending. maybe a peer has disconnected...
                    val.EvaluateMessageDelivery(multiplayerRT);

                    if (val.MessageDeliveredSuccessfully)
                    {
                        // message delivered to all still connected peers already
                        deliveringMessages.Remove(val.Message.ReliableID);
                    }
                    else
                    {
                        // resend
                        SendMessage(val.Message, val.HostOnlyMessage);
                    }
                }

            }
        }
    }
//}

#endif