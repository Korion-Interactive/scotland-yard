using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MoveHistorySystem : BaseSystem<GameEvents, GameGuiEvents, MoveHistorySystem>
{
    public UISprite PlayerTypeSprite, TicketTypeSprite;
    public UILabel LabelMoveFrom, LabelMoveTo;
    private List<GameObject> historyGrid = new List<GameObject>();
    public GameObject HistoryGridPrefab;
    public GameObject HistoryGridDoubleTicket;
    public GameObject HistoryNewRound;
    public GameObject HistoryGridParent;
    public UIScrollView HistoryScrollView;
    public GameObject CloseHistoryPanelButton;

    private int[] mrXAppearance = new int[] { Globals.MrX_Appear_1st_Time, Globals.MrX_Appear_2nd_Time, Globals.MrX_Appear_3rd_Time, Globals.MrX_Appear_4th_Time, Globals.MrX_Appear_Last_Time };


    protected override void RegisterEvents()
    {
        ListenTo(GameGuiEvents.MoveHistoryEntryAdded, OnHistoryEntryAdded);
        ListenTo(GameGuiEvents.MoveHistoryPanelClick, ClosePanel);
        ListenTo(GameGuiEvents.MoveHistoryPanelMouseDown, StartDragPanel);
        ListenTo(GameGuiEvents.MouseUp, StopDragging);

        ListenTo(GameEvents.GameLoaded, GameLoaded, Globals.Listen_Early);
    }

    private void StopDragging(BaseArgs obj)
    {
        GameBoardAnimationSystem.Instance.CamSubSystem.IsGesturesEnabled = true;
    }


    private void StartDragPanel(BaseArgs obj)
    {
        GameBoardAnimationSystem.Instance.CamSubSystem.IsGesturesEnabled = false;
    }

    private void ClosePanel(BaseArgs obj)
    {
        if(HistoryGridParent.activeInHierarchy)
            CloseHistoryPanelButton.BroadcastMessage("OnClick", SendMessageOptions.DontRequireReceiver);
    }

    private void GameLoaded(BaseArgs obj)
    {
        for(int i = 0; i < GameState.Instance.MoveHistory.EntryCount; i++)
        {
            AddEntry(i, GameState.Instance.MoveHistory.Entries[i]);
        }
    }

    private void OnHistoryEntryAdded(BaseArgs obj)
    {
        int counter = GameState.Instance.MoveHistory.Entries.Count - 1;
        AddEntry(counter, GameState.Instance.MoveHistory.Entries.Last());
    }


    private void OnPlayerMove(MoveHistoryEntry entry)
    {

        PlayerTypeSprite.spriteName = "pawn_symbol_" + entry.MoveHistoryArgs.MovingPlayer.PlayerInfo.Color.GetColorName();
        TicketTypeSprite.spriteName = entry.MoveHistoryArgs.Ticket.GetTransportSpriteName();

        string[] fromTo = GetFromToStrings(entry);
        LabelMoveFrom.text = fromTo[0];
        LabelMoveTo.text = fromTo[1];

    }

    private void AddEntry(int counter, MoveHistoryEntry lastAddedEntry)
    {

        float scrollValue = HistoryScrollView.verticalScrollBar.value;

        for(int i = counter; i >= 0; i--)
        {
            var item = GameState.Instance.MoveHistory.Entries[i];
            bool createEntry = item == lastAddedEntry;
               

            switch (item.EntryType)
            {
                case MoveHistoryEntryType.Move:
                    if (createEntry)
                    {
                        historyGrid.Add(NGUITools.AddChild(HistoryGridParent, HistoryGridPrefab));
                        OnPlayerMove(item);
                    }
                    string[] fromTo = GetFromToStrings(item);
                    historyGrid[i].transform.Find("LabelFrom").GetComponent<UILabel>().text = fromTo[0];
                    historyGrid[i].transform.Find("LabelTo").GetComponent<UILabel>().text = fromTo[1];

                    historyGrid[i].transform.Find("PlayerTypeSprite").GetComponent<UISprite>().spriteName = "pawn_symbol_" + item.MoveHistoryArgs.MovingPlayer.PlayerInfo.Color.GetColorName();
                    historyGrid[i].transform.Find("TicketTypeSprite").GetComponent<UISprite>().spriteName = item.MoveHistoryArgs.Ticket.GetTransportSpriteName();
                    break;
                case MoveHistoryEntryType.DoubleTicket:
                    if (createEntry)
                    {
                        historyGrid.Add(NGUITools.AddChild(HistoryGridParent, HistoryGridDoubleTicket));
                    }
                    break;
                case MoveHistoryEntryType.MrXTurnStart:
                    if (createEntry)
                    {
                        historyGrid.Add(NGUITools.AddChild(HistoryGridParent, HistoryNewRound));
                    }

                    Transform tempChild = historyGrid[i].transform.Find("RoundLabel");
                    UILabel tempLabel = tempChild.GetComponent<UILabel>();
                    tempLabel.text = item.MrXMoves.ToString();
                    break;
                default:
                    break;
            }
            
        }

        HistoryGridParent.GetComponent<UIGrid>().repositionNow = true;
        HistoryScrollView.UpdateScrollbars();
        if (scrollValue >= 0.95f)
        {
           StartCoroutine(WaitForUpdate());
        }
     
    }

    IEnumerator WaitForUpdate()
    {
        yield return new WaitForEndOfFrame();
        HistoryScrollView.InvalidateBounds();
        
        yield return new WaitForEndOfFrame();
        HistoryScrollView.UpdatePosition();
        HistoryScrollView.Scroll(-0.5f);

    }

    private string[] GetFromToStrings(MoveHistoryEntry entry)
    {
        if (entry.MoveHistoryArgs.MovingPlayer.IsMrX)
        {
            for (int i = 0; i < mrXAppearance.Length - 1; i++)
            {
                if (entry.MrXMoves == mrXAppearance[i])
                {
                    return new string[] { "???", entry.MoveHistoryArgs.To.Id.ToString() };
                }
                else if (entry.MrXMoves == mrXAppearance[i] + 1)
                {
                    return new string[] { entry.MoveHistoryArgs.From.Id.ToString(), "???" };
                }
            }
            
            return new string[] { "???", "???" };
        }

        return new string[] { entry.MoveHistoryArgs.From.Id.ToString(), entry.MoveHistoryArgs.To.Id.ToString() };
    }

}