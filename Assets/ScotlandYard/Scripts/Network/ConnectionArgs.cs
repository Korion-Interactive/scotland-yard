using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class ConnectionArgs : BaseArgs
{
    public virtual string ParticipantId { get { return participantId; } set { participantId = value; } }
    protected string participantId;

    public ConnectionArgs()
        : base()
    {
        if (GSP.IsMultiplayerRTAvailable)
        {
            this.participantId = GSP.MultiplayerRT.OwnParticipantId;
        }
        else
        {
            this.participantId = string.Empty;
			this.LogInfo("could not set sender participant ID: multiplayer is not available.");
        }
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(this.participantId);
    }
    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);
        this.participantId = reader.ReadString();
    }

    public override string ToString()
    {
        return "ConnectionArgs: " + participantId;
    }
}

public class RollHostDiceArgs : ConnectionArgs
{
    public string PlayerName;
    public int Number;


    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(this.PlayerName);
        writer.Write(this.Number);
    }
    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);
        this.PlayerName = reader.ReadString();
        this.Number = reader.ReadInt32();
    }
    public override string ToString()
    {
        return string.Format("Roll Host Dice - Num: {0}, Participant: {1} (ID: {2})", Number, PlayerName, participantId);
    }
}
