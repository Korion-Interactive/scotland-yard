
using UnityEngine;
public interface IClickConsumable
{
    int ConsumeOrder { get; }
    bool TryClick();

    void ClickStart();
}

public class ClickConsumable : MonoBehaviour, IClickConsumable
{
    public GameGuiEvents TriggerEvent = GameGuiEvents.Undefined;
    public GameGuiEvents ClickStartEvent = GameGuiEvents.Undefined;

    [SerializeField]
    int consumeOrder;
    public int ConsumeOrder
    {
        get { return consumeOrder; }
    }

    public bool ConsumeClick = true;

    public bool TryClick()
    {
        if (TriggerEvent != GameGuiEvents.Undefined)
            this.Broadcast(TriggerEvent);

        return ConsumeClick;
    }


    public void ClickStart()
    {
        if (ClickStartEvent != GameGuiEvents.Undefined)
            this.Broadcast(ClickStartEvent);
    }
}