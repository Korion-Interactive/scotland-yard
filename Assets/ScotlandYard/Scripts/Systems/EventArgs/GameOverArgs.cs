using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum GameOverReason
{
    EscapeOfMrX = 0,
    MrXCaught = 1,
    MrXSurrounded = 2,
    NobodyCanMove = 3,
}
public class GameOverArgs : BaseArgs
{
    public GameOverReason Reason;
    public GameOverArgs()
    { }
    public GameOverArgs(GameOverReason reason)
    {
        this.Reason = reason;
    }

    public override void Serialize(System.IO.BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write((int)Reason);
    }

    public override void Deserialize(System.IO.BinaryReader reader)
    {
        base.Deserialize(reader);
        Reason = (GameOverReason)reader.ReadInt32();
    }

    public override string ToString()
    {
        return string.Format("GameOverArgs [Reason: {0}]", Reason);
    }
}