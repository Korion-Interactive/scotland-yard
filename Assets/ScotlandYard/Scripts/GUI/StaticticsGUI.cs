using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class StatisticsGUI : MonoBehaviour
{
    [Serializable]
    public struct Bar
    {
        [SerializeField]
        public UIProgressBar ProgressBar;
        [SerializeField]
        public UILabel BarLabel;
    }

    public List<Bar> Bars = new List<Bar>();

    static public void ReloadAllStatistics()
    {
        GameObject.FindObjectsOfType<StatisticsGUI>().ToList().ForEach(x => x.ResetStatistics());
    }

    protected abstract void ResetStatistics();

    // calculate progress bar
    void CalculateProgressBars(bool animateBars, params int[] vals)
    {
        // get sum of all values
        var sum = vals.Sum();

        // if sum is equal 0 return
        // if (sum == 0) return;

        // get percentage for each value in vals
        var percents = new List<float>();
        vals.ToList().ForEach(x => percents.Add((sum == 0) ? 0 :(1f / (float)sum) * (float)x));


        if (!animateBars)
        {
            Bars.ForEach(x => x.ProgressBar.value = percents[Bars.IndexOf(x)]);
        }
        else
        {
            Bars.ForEach(x => AnimateProgressBar(x.ProgressBar.value, percents[Bars.IndexOf(x)], 1.5f, x.ProgressBar));
        }
    }

    // set statistics labels for white and black
    void SetStatisticsLabels(bool showValueInpercent, params int[] vals)
    {
        if (!showValueInpercent)
        {
            Bars.ForEach(x => x.BarLabel.text = vals[Bars.IndexOf(x)].ToString());
        }
        else
        {
            var sum = vals.Sum();

            //if (sum == 0) return;

            // get percentage for each value in vals
            var percents = new List<int>();
            vals.ToList().ForEach(x => percents.Add((int)Math.Round((sum == 0) ? 0 : (100f / (float)sum) * (float)x)));

            Bars.ForEach(x => x.BarLabel.text = percents[Bars.IndexOf(x)].ToString());
        }
    }

    // Get statistics from file
    public void GetStatistics(bool showValueInPercent, bool animateBars, params int[] vals)
    {
        SetStatisticsLabels(showValueInPercent, vals);

        CalculateProgressBars(animateBars, vals);
    }

    public void GetStatisticsValuesOnly(params int[] vals)
    {
        SetStatisticsLabels(false, vals);
    }

    void AnimateProgressBar(float from, float to, float time, UIProgressBar progressBar)
    {
        iTween.ValueTo(gameObject, iTween.Hash(
                "from", from,
                "to", to,
                "time", time,
                "onUpdate", (Action<object>)((x) => { progressBar.value = (float)x; })
                ));
    }

    // update bar
    void UpdateBar(float val, object progressBar)
    {
        var uiProgressBar = progressBar as UIProgressBar;
        if (uiProgressBar != null) uiProgressBar.value = val;
    }

}