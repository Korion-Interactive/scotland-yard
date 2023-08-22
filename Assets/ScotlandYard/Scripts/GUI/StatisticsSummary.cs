using UnityEngine;
using System.Collections;

public class StatisticsSummary : StatisticsGUI {

	// Use this for initialization
	void Start () {
        GetStatisticsValuesOnly(Stats.TotalGamesPlayed, Stats.DoubleTicketsUsed, Stats.BlackTicketsUsed);
	}

    protected override void ResetStatistics()
    {
        GetStatisticsValuesOnly(Stats.TotalGamesPlayed, Stats.DoubleTicketsUsed, Stats.BlackTicketsUsed);
    }
}
