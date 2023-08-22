using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_IOS
public class ResponseCollector
{

    HashSet<string> respondedParticipants = new HashSet<string>();
    public ReliableMessage Message { get; private set; }

    public bool MessageDeliveredSuccessfully { get; private set; }

    public bool HostOnlyMessage { get; private set; }

    public ResponseCollector(ReliableMessage message, bool hostOnlyMessage)
    {
        this.Message = message;
        this.HostOnlyMessage = hostOnlyMessage;
        this.MessageDeliveredSuccessfully = false;
    }


    public void ResponseReceived(string participantId, RealtimeMultiplayerBaseProvider multiplayer)
    {
        if (MessageDeliveredSuccessfully || respondedParticipants.Contains(participantId))
            return;

        respondedParticipants.Add(participantId);

        if (HostOnlyMessage)
        {
            // A message to the host can be answered by the host only.
            // So we safely can mark the message as delivered when any response is received.
            MessageDeliveredSuccessfully = true;
        }
        else
        {

            bool allReceived = true;
            foreach (string p in multiplayer.AllParticipantIds(false))
            {
                if (!respondedParticipants.Contains(p))
                {
                    allReceived = false;
                    break;
                }
            }

            if (allReceived)
                MessageDeliveredSuccessfully = true;
        }
    }

    public void EvaluateMessageDelivery(RealtimeMultiplayerBaseProvider multiplayer)
    {
        if (MessageDeliveredSuccessfully)
            return;

        if(HostOnlyMessage)
        {
            MessageDeliveredSuccessfully = respondedParticipants.Count > 0;
        }
        else
        {
            bool result = true;
            foreach(string p in multiplayer.AllParticipantIds(false))
            {
                if(!respondedParticipants.Contains(p))
                {
                    result = false;
                    break;
                }
            }

            MessageDeliveredSuccessfully = result;
        }
    }

}

#endif