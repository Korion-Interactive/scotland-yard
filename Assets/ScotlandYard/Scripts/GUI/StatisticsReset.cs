using System;
using UnityEngine;
using System.Collections;

public class StatisticsReset : MonoBehaviour {

    [SerializeField]
    public ChangeActionMap changeActionMap;

    private GameObject cachedSelectedObject;
    private string cachedHorizontalAxisName;

    public void Reset()
    {
        //PopupManager.ShowQuestion("statistics_reset", "statistics_reset_text", YesCallback, null );

        //KORION
        cachedSelectedObject = UICamera.selectedObject;
        PopupManager.ShowQuestion("statistics_reset", "statistics_reset_text",
            (o) =>
            {
                YesCallback(null);
                ActivateActionMap(true); //activates proper 
                UICamera.currentCamera.GetComponent<UICamera>().horizontalAxisName = cachedHorizontalAxisName;
                
                UICamera.ForceSetSelection(cachedSelectedObject);
                cachedSelectedObject = null;
            }, OnClick);

        PopupManager.Instance.CachedButton = PopupManager.Instance.CurrentPopup.noButton; //used to activate when receiving uiCancelAction //popupkill

        //should be firstcamera
        cachedHorizontalAxisName = UICamera.currentCamera.GetComponent<UICamera>().horizontalAxisName;
        UICamera.currentCamera.GetComponent<UICamera>().horizontalAxisName = "HorizontalPopUp";

        ActivateActionMap(false);
    }



    public void ActivateActionMap(bool isSet)
    {
        //opposite as expected --> since used by a different variable
        if (!isSet)
            changeActionMap.SetControllerMapState();
        else
            changeActionMap.ResetControllerMaps();
    }

    //KORION
    private void OnClick(GameObject go)
    {
        ActivateActionMap(true);

        PopupManager.Instance.CachedButton = null;

        //changed ja aber nicht
        UICamera.ForceSetSelection(cachedSelectedObject);

        cachedSelectedObject = null;

        UICamera.currentCamera.GetComponent<UICamera>().horizontalAxisName = cachedHorizontalAxisName;
    }

    private void YesCallback(GameObject go)
    {
        foreach (var key in AppSetup.Instance.StatsTable.GetColumnCellIterator("id", false))
        {
            AppSetup.Instance.StatsTable["value", key] = "0";
        }

        AppSetup.Instance.StatsTable.Save();

        Stats.Reload();
        
        StatisticsGUI.ReloadAllStatistics();
    }
}
