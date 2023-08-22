using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum MoveHistoryEntryType { Move, DoubleTicket, MrXTurnStart }

public class MoveHistoryEntry
{
    public MoveHistoryEntryType EntryType;
    public MoveArgs MoveHistoryArgs;
    public int MrXMoves;
    public MoveHistoryEntry(MoveHistoryEntryType type)
    {
        this.EntryType = type;
        this.MoveHistoryArgs = null;
    }

    public MoveHistoryEntry() { }
}

[Serializable]
public class MoveHistory
{

    public int EntryCount { get { return Entries.Count; } }

    public List<MoveHistoryEntry> Entries = new List<MoveHistoryEntry>();

    public void AddMove(MoveArgs args)
    {
        MoveHistoryEntry entry = new MoveHistoryEntry(MoveHistoryEntryType.Move);
        entry.MoveHistoryArgs = args;
        entry.MrXMoves = GameState.Instance.MrX.Moves + 1;

        Entries.Add(entry);
        this.Broadcast(GameGuiEvents.MoveHistoryEntryAdded, null, new BaseArgs());
    }

    public void UsingDoubleTicket()
    {
        MoveHistoryEntry entry = new MoveHistoryEntry(MoveHistoryEntryType.DoubleTicket);
        Entries.Add(entry);
        this.Broadcast(GameGuiEvents.MoveHistoryEntryAdded, null, new BaseArgs());

    }

    public void MrXTurnStart()
    {
        MoveHistoryEntry entry = new MoveHistoryEntry(MoveHistoryEntryType.MrXTurnStart);
        entry.MrXMoves = GameState.Instance.MrX.Moves + 1;
        Entries.Add(entry);
        this.Broadcast(GameGuiEvents.MoveHistoryEntryAdded, null, new BaseArgs());

    }

    //public IEnumerable<MoveHistoryEntry> IterateLastMoves()
    //{
    //    //for (int i = entries.Count - 1; i >= 0; i--)
    //    for (int i = 0; i < entries.Count; i++)
    //    {
    //        yield return entries[i];            
    //    }
    //}


    //public void Serialize(JsonSerializer serializer, JsonWriter writer)
    //{
    //   // writer.WriteStartArray();
    //    foreach (var e in Entries)
    //    {
    //        serializer.Serialize(writer, e.EntryType);
    //        serializer.Serialize(writer, e.MrXMoves);
    //        serializer.Serialize(writer, e.MoveHistoryArgs.MovingPlayer.PlayerId);
    //        serializer.Serialize(writer, e.MoveHistoryArgs.Ticket);
    //        serializer.Serialize(writer, e.MoveHistoryArgs.From.Id);
    //        serializer.Serialize(writer, e.MoveHistoryArgs.To.Id);
    //    }
    //    //writer.WriteEndArray();
    //}

    //public void Deserialize(JsonSerializer serializer, JsonReader reader)
    //{
    //    // TODO
    //}

}
