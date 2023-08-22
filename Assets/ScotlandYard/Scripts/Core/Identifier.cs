using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Identifier : MonoBehaviour
{
    static Dictionary<int, Identifier> mapping = new Dictionary<int, Identifier>();

    public static Identifier Find(int id)
    {
        //if (mapping.ContainsKey(id))
            return mapping[id]; // we want an exception here if not found!

        //Log.warn("Identifier", "couldn't find object with id " + id);
        //return null;
    }


    [SerializeField]
    int id;
    public int GameID 
    { 
        get { return id; } 
        set
        {
            mapping.Remove(id);

            id = value;

            AddToMapping();
        }
    }

    public int UniqueID
    {
        get { return id + IdShift; }
    }

    public int IdShift = 0;

    void Start()
    {
        AddToMapping();
    }

    void OnDestroy()
    {
        mapping.Remove(UniqueID);
        this.LogInfo(string.Format("Destroy(): Id {0} removed", UniqueID));

    }

    public void GenerateIDFromHashcode()
    {
        GameID = name.GetHashCode();
    }

    public void SetToNextValidId()
    {
        mapping.Remove(id + IdShift);

        for(int i = 1; i < int.MaxValue; i++)
        {
            if (!mapping.ContainsKey(i + IdShift))
            {
                GameID = i;
                break;
            }
        }
    }

    void AddToMapping()
    {
        int key = id + IdShift;

        if (key != 0 && mapping != null)
        {
            if (!mapping.ContainsKey(key))
            {
                mapping.Add(key, this);
                this.LogDebug(string.Format("{0}: Added identifier with id {1} ({2} + {3})", this.name, key, id, IdShift));
            }
            else if (mapping[key] != this)
            {
                this.LogError(string.Format("ID is used more than once: {0} ({1} + {2} / {3} & {4})", key, id, IdShift, this.name, mapping[key].name));
            }
        }
        else
        {
            this.LogError(string.Format("Do not add identifier because 0 is not allowed: {0} ({1} + {2} / {3})", key, id, IdShift, this.name));
        }
    }
}
