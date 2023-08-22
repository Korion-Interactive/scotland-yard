using UnityEngine;
using System.Collections;

public class StatisticsColor : StatisticsGUI 
{

	// Use this for initialization
	void Start () {	
        GetStatistics(false, true, Stats.PlayedWithRed, Stats.PlayedWithYellow, Stats.PlayedWithGreen, Stats.PlayedWithBlue, Stats.PlayedWithBlack);
	}

    protected override void ResetStatistics()
    {
        GetStatistics(false, true, Stats.PlayedWithRed, Stats.PlayedWithYellow, Stats.PlayedWithGreen, Stats.PlayedWithBlue, Stats.PlayedWithBlack);
    }
	

}
