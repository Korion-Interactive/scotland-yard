using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public interface IMessage
{
    byte MessageTypeId { get; }
    void Serialize(BinaryWriter writer);
    void Deserialize(BinaryReader reader);

    IMessage CreateEmptyInstance();
}