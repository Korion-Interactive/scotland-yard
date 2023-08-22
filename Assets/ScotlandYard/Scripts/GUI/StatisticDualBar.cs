using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatisticDualBar : StatisticsGUI
{

    public enum GetStatisticFor
    {
        GAMES_WON,
        SUCCESS,
        PLAYED_FIGURE
    }

    public GetStatisticFor GetStatistic;

	// Use this for initialization
	void Start () {

	    switch (GetStatistic)
	    {
	        case GetStatisticFor.GAMES_WON:
                GetStatistics(false, true, Stats.GamesWonAsMrX, Stats.GamesWonAsDetective);
	            break;
	        case GetStatisticFor.SUCCESS:
                GetStatistics( true, true, Stats.GamesWonAsMrX, Stats.GamesWonAsDetective);                
	            break;
	        case GetStatisticFor.PLAYED_FIGURE:
                GetStatistics(false, true, Stats.GamesPlayedAsMrX, Stats.GamesPlayedAsDetective);
	            break;
	        default:
	            throw new ArgumentOutOfRangeException();
	    }
	}

    protected override void ResetStatistics()
    {
        switch (GetStatistic)
        {
            case GetStatisticFor.GAMES_WON:
                GetStatistics(false, true, Stats.GamesWonAsMrX, Stats.GamesWonAsDetective);
                break;
            case GetStatisticFor.SUCCESS:
                GetStatistics(true, true, Stats.GamesWonAsMrX, Stats.GamesWonAsDetective);
                break;
            case GetStatisticFor.PLAYED_FIGURE:
                GetStatistics(false, true, Stats.GamesPlayedAsMrX, Stats.GamesPlayedAsDetective);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
