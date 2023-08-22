using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class BaseArgs
{
    public GameObject RelatedObject;

    public virtual void Serialize(BinaryWriter writer)
    {
        if(RelatedObject == null)
        {
            this.LogError("Serialize(): Related Object is null");
            writer.Write(0);
            return;
        }

        Identifier identifier = RelatedObject.GetComponent<Identifier>();
        if (identifier != null)
        {
            writer.Write(identifier.UniqueID);
        }
        else
        {
            this.LogError(string.Format("Serialize(): Related Object \"{0}\" has no Identifier", RelatedObject.name));
            writer.Write(0);
        }
    }

    public virtual void Deserialize(BinaryReader reader)
    {
        int id = reader.ReadInt32();

        if (id != 0)
        {
            Identifier identifier = Identifier.Find(id);
            RelatedObject = identifier.gameObject;
        }
        else
        {
            this.LogWarn(string.Format("Deserialize(): No Related Object."));
        }
    }
    
}
