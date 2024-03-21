using System.Collections.Generic;

public static class Stats
{

    public static bool CompletedTutorial1 { get => Get("tut1", false); set { bool before = CompletedTutorial1; Set("tut1", value); TutorialAchivementCheck(before, value); } }
    public static bool CompletedTutorial2 { get => Get("tut1", false); set { bool before = CompletedTutorial2; Set("tut2", value); TutorialAchivementCheck(before, value); } }
    public static bool CompletedTutorial3 { get => Get("tut1", false); set { bool before = CompletedTutorial3; Set("tut3", value); TutorialAchivementCheck(before, value); } }

    public static int TotalGamesPlayed => GamesPlayedAsMrX + GamesPlayedAsDetective;

    public static int GamesPlayedAsMrX { get => Get("played_mrx", 0); set => Set("played_mrx", value); }
    public static int GamesWonAsMrX { get => Get("won_mrx", 0); set => Set("won_mrx", value); }

    public static int GamesPlayedAsDetective { get => Get("played_det", 0); set => Set("played_det", value); }
    public static int GamesWonAsDetective { get => Get("won_det", 0); set => Set("won_det", value); }

    public static int TaxiRides { get => Get("taxi_ticket", 0); set => Set("taxi_ticket", value); }
    public static int BusRides { get => Get("bus_ticket", 0); set => Set("bus_ticket", value); }
    public static int MetroRides { get => Get("metro_ticket", 0); set => Set("metro_ticket", value); }
    public static int BlackTicketsUsed { get => Get("black_ticket", 0); set => Set("black_ticket", value); }
    public static int DoubleTicketsUsed { get => Get("double_ticket", 0); set => Set("double_ticket", value); }

    public static int PlayedWithYellow { get => Get("color_yellow", 0); set => Set("color_yellow", value); }
    public static int PlayedWithRed { get => Get("color_red", 0); set => Set("color_red", value); }
    public static int PlayedWithBlack { get => Get("color_black", 0); set => Set("color_black", value); }
    public static int PlayedWithBlue { get => Get("color_blue", 0); set => Set("color_blue", value); }
    public static int PlayedWithGreen { get => Get("color_green", 0); set => Set("color_green", value); }

    public static bool IsInSociety { get => Get("society_unlocked", false); set => Set("society_unlocked", value); }

    static T Get<T>(string id, T defaultValue)
    {
        T val;
        if(!AppSetup.Instance.StatsTable.ContainsRow(id) || !AppSetup.Instance.StatsTable.TryGet<T>("value", id, out val))
        {
            return defaultValue;
        }

        return val;
    }

    static void Set<T>(string id, T val)
    {
        if (!AppSetup.Instance.StatsTable.ContainsRow(id))
            AppSetup.Instance.StatsTable.AppendRow(id, val.ToString());
        else
            AppSetup.Instance.StatsTable["value", id] = val.ToString();

        Save();
    }

    static void TutorialAchivementCheck(bool before, bool after)
    {
        //TODO KORION Achievements
        if (before == false && after == true)
            //Achievements.Progress(Achievements.rookie, 1);

        if(Stats.CompletedTutorial1 && Stats.CompletedTutorial2 && Stats.CompletedTutorial3)
        {
            // just to be sure... maybe it couldn't access achievements before
            //TODO KORION Achievements
            //Achievements.Progress(Achievements.rookie, 3);
        }
    }

    public static void Reload()
    {
        AppSetup.Instance.LoadOrCreateStatsTable();
    }

    public static void Save()
    {
        AppSetup.Instance.StatsTable.Save();
    }

    public static void TrackColor(PlayerColor playerColor)
    {
        switch(playerColor)
        {
            case PlayerColor.Black:
                PlayedWithBlack++;
                break;
            case PlayerColor.Blue:
                PlayedWithBlue++;
                break;
            case PlayerColor.Green:
                PlayedWithGreen++;
                break;
            case PlayerColor.Red:
                PlayedWithRed++;
                break;
            case PlayerColor.Yellow:
                PlayedWithYellow++;
                break;
        }
    }

    public static PlayerColor GetFavouriteColor()
    {
        List<Tuple<PlayerColor, int>> tmp = new List<Tuple<PlayerColor, int>>();
        tmp.Add(new Tuple<PlayerColor, int>(PlayerColor.Black, PlayedWithBlack));
        tmp.Add(new Tuple<PlayerColor, int>(PlayerColor.Blue, PlayedWithBlue));
        tmp.Add(new Tuple<PlayerColor, int>(PlayerColor.Green, PlayedWithGreen));
        tmp.Add(new Tuple<PlayerColor, int>(PlayerColor.Red, PlayedWithRed));
        tmp.Add(new Tuple<PlayerColor, int>(PlayerColor.Yellow, PlayedWithYellow));

        tmp.Sort((a, b) => b.B.CompareTo(a.B));

        return tmp[0].A;
    }

}
