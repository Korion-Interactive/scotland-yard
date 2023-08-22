using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class VectorEventArgs : BaseArgs
{
    public Vector3 Vector;

    public override void Serialize(System.IO.BinaryWriter writer)
    {
        base.Serialize(writer);

        writer.Write(Vector.x);
        writer.Write(Vector.y);
        writer.Write(Vector.z);
    }
    public override void Deserialize(System.IO.BinaryReader reader)
    {
        base.Deserialize(reader);

        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();

        Vector = new Vector3(x, y, z);
    }
}