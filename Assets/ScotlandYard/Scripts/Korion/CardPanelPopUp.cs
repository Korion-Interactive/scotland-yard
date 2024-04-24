using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPanelPopUp : MonoBehaviour
{
    [SerializeField]
    private ChangeActionMap changeActionMap;

    [SerializeField]
    private List<BoxCollider2D> boxCollider2Ds = new List<BoxCollider2D>();
    
    private GameObject cachedSelectedObject;

    public void SetButtonColliders(bool isActive)
    {
        for (int i = 0; i < boxCollider2Ds.Count; i++)
        {
            boxCollider2Ds[i].enabled = isActive;
        }
    }

    public void SetProperActionMap(bool isSet)
    {
        //opposite as expected --> since used by a different variable
        if (!isSet)
            changeActionMap.SetControllerMapState();
        else
            changeActionMap.ResetControllerMaps();
    }

    public void PopUp()
    {
        if (!(GameSetupBehaviour.Instance.Setup.MrXSetup.Controller == PlayerController.Human))
            return;

        if ((GameSetupBehaviour.Instance.Setup.MrXSetup.Controller == PlayerController.Human))
        {
            bool hasHumanDetectives = false;
            for (int i = 0; i < GameSetupBehaviour.Instance.Setup.DetectiveSetups.Length; i++)
            {
                if (GameSetupBehaviour.Instance.Setup.DetectiveSetups[i].Controller == PlayerController.Human)
                {
                    hasHumanDetectives = true;
                    break;
                }
            }

            if(!hasHumanDetectives)
            {
                return;
            }
        }

        cachedSelectedObject = UICamera.selectedObject;

        Debug.Log("triggered PopUp in Cards Panel");
        PopupManager.ShowQuestion("pass_device", "MrX_turn", (o) => { SetProperActionMap(true); SetButtonColliders(true); PopupManager.Instance.CachedButton = null; UICamera.ForceSetSelection(cachedSelectedObject); cachedSelectedObject = null; }, null);
        PopupManager.Instance.CachedButton = PopupManager.Instance.CurrentPopup.yesButton; //used to activate when receiving uiCancelAction //popupkill
        //PopupManager.Instance.CurrentPopup.text.GetComponent<UILabel>().text = string.Format(PopupManager.Instance.CurrentPopup.text.GetComponent<UILabel>().text, GameState.Instance.MrX.PlayerDisplayName); //KORION IMPROVE --> NEXT LINE IN BETWEEN
        PopupManager.Instance.CurrentPopup.noButton.SetActive(false);
        SetProperActionMap(true);
        SetButtonColliders(false);
    }
}
