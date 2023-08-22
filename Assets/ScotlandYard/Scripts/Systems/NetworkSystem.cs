using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public abstract class NetworkSystem<TEvent, TSystem> : BaseSystem<TEvent, TSystem>
    where TEvent : struct, IComparable, IConvertible, IFormattable // = enum
    where TSystem : BaseSystem<TSystem>
{

    Tuple<TEvent, BaseArgs> currentNetworkMessage; // TODO: kick this and check sender
    protected abstract byte Context { get; }

    public bool RequireOrderedMessages { get; protected set; }
    uint lastMessageIndex = 0;

    protected override void Start()
    {
        base.Start();

        GSP.MultiplayerRTEvents.MessageReceived += ReceiveMessage;
    }

    protected override void OnDestroy()
    {
        GSP.MultiplayerRTEvents.MessageReceived -= ReceiveMessage;
        base.OnDestroy();
    }

    protected void SendEvent(TEvent eventType, BaseArgs args, bool sendToHost = false, bool appendMessageId = true)
    {
        if (!GSP.IsMultiplayerRTAvailable)
        {
            this.LogDebug("Send Event: No Multiplayer Available... skip!");
            return;
        }

        // don't send events which we just received
        if (currentNetworkMessage != null && currentNetworkMessage.A.Equals(eventType) && currentNetworkMessage.B.GetHashCode() == args.GetHashCode())
            return;

        appendMessageId = appendMessageId && RequireOrderedMessages;

        if(appendMessageId)
            lastMessageIndex++;

        uint msgId = (appendMessageId) ? lastMessageIndex : 0;

        this.LogInfo(string.Format("send event {0} (id: {2}) - {1}", eventType, args.ToString(), msgId));


        byte[] msg;

        // Create Message
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Context);

                writer.Write(Convert.ToByte(eventType));
                args.Serialize(writer);

                if (RequireOrderedMessages)
                {
                    writer.Write(msgId);
                }

                writer.Close();
            }

            stream.Close();
            msg = stream.ToArray();
        }

        // Send it
        if (sendToHost)
            GSP.MultiplayerRT.SendMessageToHost(msg, true);
        else
            GSP.MultiplayerRT.SendMessageToOthers(msg, true);
    }

    private bool ReceiveMessage(byte[] msg)
    {
        if(msg[0] != this.Context)
        {
            this.LogDebug("Receive message - wrong context: " + msg[0]);
            return false;
        }

        bool success = true; 
        TEvent eventType = new TEvent(); 
        BaseArgs args = null;

        using (MemoryStream stream = new MemoryStream(msg))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    reader.ReadByte(); // consume context (checked at start of method)


                    eventType = (TEvent)(object)reader.ReadByte();
                    this.LogInfo("receive message: event type: " + eventType.ToString());
                    args = ArgsFactory(eventType);
                    args.Deserialize(reader);


                }
                catch(Exception ex)
                {
                    this.LogError("Receive Message Failed", ex);
                    success = false;
                }

                if (RequireOrderedMessages)
                {
                    try
                    {
                        uint msgIdx = reader.ReadUInt32();

                        if (msgIdx != lastMessageIndex + 1 
                            && msgIdx != 0)
                        {
                            this.LogWarn(string.Format("Waiting for message id {0}. received message with id {1}.", (lastMessageIndex + 1), msgIdx));
                            success = false;
                        }
                        else
                        {
                            if (msgIdx != 0)
                                lastMessageIndex = msgIdx;
                        } 
                    }
                    catch (Exception ex)
                    {
                        this.LogError("problem reading msg index. probably playing with device with older version. ", ex);
                    }
                }
               

                reader.Close();
            }
            stream.Close();
        }

        success = success && CanHandleMessage(eventType, args);

        if (success)
        {
            currentNetworkMessage = new Tuple<TEvent, BaseArgs>(eventType, args);
            success = MessageReceivedSuccessfully(eventType, args);
            currentNetworkMessage = null;
        }

        return success;
    }

    protected virtual bool CanHandleMessage(TEvent eventType, BaseArgs args)
    {
        return true;
    }

    protected abstract BaseArgs ArgsFactory(TEvent eventType);

    protected abstract bool MessageReceivedSuccessfully(TEvent eventType, BaseArgs args);
}