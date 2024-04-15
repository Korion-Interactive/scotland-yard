using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ravity;
using Korion.Achievements;

public static class Achievements
{
    public const string the_detective = "the_detective";
    public const string mister_x = "mister_x";
    public const string undercover_agent = "undercover_agent";
    public const string the_shadow = "the_shadow";
    public const string gotcha = "gotcha";
    public const string driver = "driver";
    public const string sightseeing_tour = "sightseeing_tour";
    public const string nothing_better_than_tube = "nothing_better_than_tube";
    public const string rookie = "rookie";
    public const string society = "society"; //KORION WE DONT SUPPORT this CHEAT that cause an avalanche
    public const string life_in_a_fast_lane = "life_in_a_fast_lane";
    public const string specialist = "specialist";
    public const string master_x = "master_x";
    public const string bames_jond = "bames_jond";
    public const string mi_5 = "mi_5"; //KORION NOT NECCESSARY

    public static void Unlock(string lookupId)
    {
        //KORION ACHIEVEMENTS //Cleaned
   //     if (!GSP.IsStatusAvailable)
   //     {
			//Log.info("achievements", "couldn't unlock achievement because GSP status is not available.");
   //         return;
   //     }
        //string id = GetId(lookupId);
        //GSP.Status.UnlockAchievement(id);

        AchievementFacade.Unlock(lookupId);
    }

    public static void Progress(string lookupId, int stepIncrement)
    {
        if (!GSP.IsStatusAvailable)
        {
            Log.info("achievements", "couldn't progress achievement because GSP status is not available.");
            return;
        }

        string id = GetId(lookupId);
        int max = GetMaxSteps(lookupId);
        GSP.Status.ProgressAchievement(id, stepIncrement: stepIncrement, totalSteps: max);
    }

    public static string GetId(string lookupId)
    {
        string column = null;

        if (HardwareUtils.IsiOS) column = "game_center_id";
        if (HardwareUtils.IsAndroid) column = "google_play_id";
        
        //KORION TODO: Column is game center or google play ID. This must be changed. Is this even easily possible to use?
        return AppSetup.Instance.AchievementTable[column, lookupId];
    }

    public static int GetMaxSteps(string lookupId)
    {
        string column = "steps";
        AppSetup.Instance.AchievementTable.TryGet(column, lookupId, out int result);
        return result;
    }
}
