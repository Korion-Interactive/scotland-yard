using UnityEngine;
using System.Collections;

public class StatisticsTickets : StatisticsGUI {

	// Use this for initialization
	void Start () {
	
        GetStatistics(false, true, Stats.TaxiRides, Stats.BusRides, Stats.MetroRides);
	}

    protected override void ResetStatistics()
    {
        GetStatistics(false, true, Stats.TaxiRides, Stats.BusRides, Stats.MetroRides);
    }
}
