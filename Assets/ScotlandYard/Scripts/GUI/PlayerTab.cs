using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerTab : PlayerFocus
{

    public string PlayerName;
    public Color PlayerColor;

    public UISprite bgSprite;
    public UILabel label;

    public PlayerBase Player { get { return PlayerTabSystem.Instance.Player; } set { PlayerTabSystem.Instance.Player = value; } }

    private GameObject detectiveTicketsLeft;
    private GameObject mrXTicketsLeft;

    private bool isActive;
    private bool isDisabled = false;

    public void DisablePlayerTab()
    {
        isDisabled = true;
        PlayerColor = Color.black;
        bgSprite.SetDirty();
        PlayerName = "";
    }

    void Start()
    {
        detectiveTicketsLeft = PlayerTabSystem.Instance.DetectivesTicketsLeft.gameObject;
        mrXTicketsLeft = PlayerTabSystem.Instance.MrXTicketsLeft.gameObject;


        bgSprite.color = PlayerColor;

        if (string.IsNullOrEmpty(PlayerName) && !isDisabled)
        {
            label.text = "Player " + PlayerID;
        }
        else
        {
            label.text = PlayerName;            
        }

    }

    public void SetTabActive(bool setActive)
    {
        if (setActive && isActive) return;

        if (setActive)
        {
            bgSprite.spriteName = "player_tab_a";
            bgSprite.color = PlayerColor;
            isActive = true;
        }
        else
        {
            bgSprite.spriteName = "player_tab_b";
            isActive = false;
        }

        bgSprite.SetDirty();
    }

    protected override void ClickedOnPlayer(PlayerBase p)
    {
        if (p == Player)
        {
            detectiveTicketsLeft.SetActive(false);
            mrXTicketsLeft.SetActive(false);
            Player = null;
        }
        else
        {
            Player = p;
            mrXTicketsLeft.SetActive(p.IsMrX);
            detectiveTicketsLeft.SetActive(p.IsDetective);
            GameObject ticketsLeft = (p.IsDetective) ? detectiveTicketsLeft : mrXTicketsLeft;
            ticketsLeft.GetComponent<ShowTicketsLeft>().ShowRemainigTickets(p);
        }

    }
}
