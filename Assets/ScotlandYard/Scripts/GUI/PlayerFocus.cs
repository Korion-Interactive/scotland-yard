using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerFocus : MonoBehaviour, IClickConsumable
{
    public int PlayerID;

    public int ConsumeOrder
    {
        get { return -10; }
    }

    public virtual bool TryClick()
    {
        PlayerBase p = GameState.Instance.PlayerIterator().FirstOrDefault(o => o.PlayerId == this.PlayerID);
        
        if (p != null)
	    {
            ClickedOnPlayer(p);

            if (p.IsDetective)
            {
                this.Broadcast(GameGuiEvents.FocusPosition, p.gameObject);
                this.Broadcast(GameGuiEvents.PlayerFocusClicked);
                return true;
            }
            else
            {
                if (GameState.Instance.MrX.HasAlreadyAppeared())
                {
                    this.Broadcast(GameGuiEvents.FocusPosition, GameState.Instance.MrX.LastAppearance.gameObject);
                }
                return true;
            }
	    }
        else
        {
            this.LogError("Player not found!");
        }
        return false;
    }

    protected virtual void ClickedOnPlayer(PlayerBase player)
    {
        // intentionally left blank
    }


    public void ClickStart()
    {

    }
}
