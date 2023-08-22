using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UIGrid))]
public class MrXGameStepSystem : BaseSystem<GameEvents, MrXGameStepSystem>
{

    protected override void RegisterEvents()
    {
        ListenTo<MoveArgs>(GameEvents.MrXMove, SetMrXGameStep, Globals.Listen_Late);
        ListenTo(GameEvents.GameLoaded, GameLoaded);
    }

    private void GameLoaded(BaseArgs obj)
    {
        int idx = 0;

        foreach(MoveHistoryEntry entry in GameState.Instance.MoveHistory.Entries)
        {
            if(entry.EntryType == MoveHistoryEntryType.Move && entry.MoveHistoryArgs.MovingPlayer.IsMrX)
            {
                SetStep(idx, entry.MoveHistoryArgs.Ticket);
                idx++;
            }
        }
    }

    private void SetMrXGameStep(MoveArgs args)
    {

        var mrX = args.MovingPlayer as MrX;
        if (mrX != null)
        {
            SetStep(mrX.Moves - 1, args.Ticket);
        }
        else
        {
            Log.error(this, "args.MovingPlayer in SetMrXGameStep is null");
        }
    }

    public void SetStep(int index, TransportationType transport)
    {
        UIGrid grid = GetComponent<UIGrid>();
        if (index < grid.transform.childCount)
        {
            MrXGameStep gameStep = grid.GetChild(index).GetComponent<MrXGameStep>();
            gameStep.SetMrXMoveStep(transport.GetTransportSpriteName());
        }
    }
}
