using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class MessageHandler
{
    byte context;
    Dictionary<byte, IMessage> messageFactory = new Dictionary<byte, IMessage>();

    public MessageHandler(byte context, params IMessage[] messageTemplates)
    {
        this.context = context;

        AddMessageTypes(messageTemplates);
    }

    public void AddMessageTypes(params IMessage[] messageTemplates)
    {
        foreach (IMessage msg in messageTemplates)
        {
            if(!messageFactory.ContainsKey(msg.MessageTypeId))
                messageFactory.Add(msg.MessageTypeId, msg);
        }
    }

    public byte[] Pack(IMessage message)
    {
        using(MemoryStream stream = new MemoryStream())
        {
            using(BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(this.context);
                writer.Write(message.MessageTypeId);

                message.Serialize(writer);
            }

            return stream.ToArray();
        }
    }

    public IMessage Unpack(byte[] input)
    {
        if (input[0] != this.context)
            return null;

        IMessage result = null;
        using (MemoryStream stream = new MemoryStream(input))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                reader.ReadByte(); // consume context

                byte msgID = reader.ReadByte();

                if (!messageFactory.ContainsKey(msgID))
                {
                    this.LogError(string.Format("No message with ID {0} found.", msgID));
                }
                else
                {
                    result = messageFactory[msgID].CreateEmptyInstance();
                    result.Deserialize(reader);
                }
            }
        }


        return result;
    }
}