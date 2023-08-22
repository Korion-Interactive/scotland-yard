using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ChatArgs : ConnectionArgs
{
    public string ColorCode;
    public string PlayerName;
    public string ChatMessage;

    public override void Serialize(System.IO.BinaryWriter writer)
    {
        base.Serialize(writer);

        writer.Write(ColorCode);
        writer.Write(PlayerName);
        writer.Write(ChatMessage);
    }

    public override void Deserialize(System.IO.BinaryReader reader)
    {
        base.Deserialize(reader);

        ColorCode = reader.ReadString();
        PlayerName = reader.ReadString();
        ChatMessage = reader.ReadString();
    }
}
