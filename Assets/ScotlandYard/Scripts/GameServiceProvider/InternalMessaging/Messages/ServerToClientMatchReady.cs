using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

public class ServerToClientMatchReady : IMessage
{
    public byte MessageTypeId { get { return (byte)InternalMessageID.ServerToClientMatchReady; } }

    public ReadOnlyCollection<string> Participants { get { return participants.AsReadOnly(); } }
    List<string> participants = new List<string>();

    internal ServerToClientMatchReady()
    { }
    public ServerToClientMatchReady(string[] participants)
    {
        this.LogDebug("ServerToClientMatchReady -> ctor");
        this.participants = participants.ToList();
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(participants.Count);

        foreach (string p in participants)
            writer.Write(p);
    }

    public void Deserialize(BinaryReader reader)
    {
        int cnt = reader.ReadInt32();

        for(int i = 0; i < cnt; i++)
        {
            participants.Add(reader.ReadString());
        }
    }

    public IMessage CreateEmptyInstance()
    {
        return new ServerToClientMatchReady();
    }
}