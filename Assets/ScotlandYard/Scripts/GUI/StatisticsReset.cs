using System;
using UnityEngine;
using System.Collections;

public class StatisticsReset : MonoBehaviour {

    public void Reset()
    {
        PopupManager.ShowQuestion("statistics_reset", "statistics_reset_text", YesCallback, null );
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
