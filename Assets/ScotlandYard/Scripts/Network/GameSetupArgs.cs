using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class SetRoundTimeArgs : BaseArgs
{
    public byte RoundTime;

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(RoundTime);
    }
    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);
        RoundTime = reader.ReadByte();
    }

    public override string ToString()
    {
        return string.Format("SetRoundTimeArgs: {0}", RoundTime);
    }
}

public abstract class SetPlayerInfoArgs : ConnectionArgs
{
    byte playerIndex;
    public bool IsMrX { get { return playerIndex == 0; } }
    public int DetectiveIndex { get { return (int)playerIndex - 1; } }
    public int PlayerSetupIndex { get { return (int)playerIndex; } }
    protected SetPlayerInfoArgs() { }
    protected SetPlayerInfoArgs(bool isMrX, int playerIndex)
    {
        this.playerIndex = (byte)playerIndex;
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(playerIndex);
    }
    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);
        playerIndex = reader.ReadByte();
    }
    public override string ToString()
    {
        return string.Format("SetPlayerInfoArgs: {0} ", playerIndex);
    }

}

// disabled warning "Overriding Object.Equals(object) but not Object.GetHashCode()"
#pragma warning disable 659
public class SetPlayerArgs : SetPlayerInfoArgs, IEquatable<SetPlayerArgs>, IComparable<SetPlayerArgs>
{
    public PlayerController Controller { 
        get { return controller; }
        set { if (controller == value) return; controller = value; IsDirty = true; }
    }

    public string Name {
        get { return name; }
        set { if (name == value) return; name = value; IsDirty = true; }
    }

    public AiDifficulty Difficulty {
        get { return difficulty; }
        set { if (difficulty == value) return; difficulty = value; IsDirty = true; }
    }

    public override string ParticipantId
    {
        get { return participantId; }
        set { if (participantId == value) return; participantId = value; IsDirty = true; }
    }

    PlayerController controller;
    string name;
    AiDifficulty difficulty;

    public bool IsDirty { get; set; }

    public SetPlayerArgs() : base() { }
    public SetPlayerArgs(PlayerController controller, string name, AiDifficulty difficulty, bool isMrX, int playerIndex = 0)
        : base(isMrX, playerIndex) 
    {
        this.controller = controller;
        this.name = name;
        this.difficulty = difficulty;
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write((byte)controller);
        writer.Write(name);
        writer.Write((byte)difficulty);
    }

    public override void Deserialize(BinaryReader reader)
    {
        base.Deserialize(reader);
        this.controller = (PlayerController)reader.ReadByte();
        this.name = reader.ReadString();
        this.difficulty = (AiDifficulty)reader.ReadByte();
    }


    public int CompareTo(SetPlayerArgs other)
    {
        return this.PlayerSetupIndex.CompareTo(other.PlayerSetupIndex);
    }

    public bool Equals(SetPlayerArgs other)
    {
        return this.controller == other.controller
            && this.difficulty == other.difficulty
            && this.name == other.name
            && this.participantId == other.participantId
            && this.PlayerSetupIndex == other.PlayerSetupIndex;
    }

    public override bool Equals(object obj)
    {
        if (obj is SetPlayerArgs)
            return this.Equals(obj as SetPlayerArgs);

        return base.Equals(obj);
    }

    public override string ToString()
    {
        return string.Format("SetPlayerArgs - id: {0}, controller: {1}, name: {2}, difficulty: {3}, participant: {4}", 
                                        PlayerSetupIndex, controller, name, difficulty, participantId);
    }

}
#pragma warning restore 659
