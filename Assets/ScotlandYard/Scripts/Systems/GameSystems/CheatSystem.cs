using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CheatSystem : BaseSystem<GameGuiEvents, CheatSystem>
{
    List<int> lastClickedStations = new List<int>();
    List<Cheat> cheats = new List<Cheat>() { new CheatAchievementSociety() };
    bool ignoreNextAnywhereClick;

    protected override void RegisterEvents()
    {
        ListenTo(GameGuiEvents.StationClicked, StationClicked);
        ListenTo(GameGuiEvents.ClickedAnywhere, CickedAnywhere);
    }

    private void CickedAnywhere(BaseArgs obj)
    {
        if(ignoreNextAnywhereClick)
        {
            ignoreNextAnywhereClick = false;
            return;
        }

        lastClickedStations.Clear();
    }

    private void StationClicked(BaseArgs obj)
    {
        var station = obj.RelatedObject.GetComponent<Station>();

        if(station != null)
        {
            lastClickedStations.Add(station.Id);
            CheckCheats();
        }

        ignoreNextAnywhereClick = true;
    }

    private void CheckCheats()
    {
        foreach(Cheat cheat in cheats)
        {
            if (cheat.StationsToClick.Length > lastClickedStations.Count)
                continue;

            bool success = true;
            for(int i = 0; i < cheat.StationsToClick.Length; i++)
            {
                int idx = i + (lastClickedStations.Count - cheat.StationsToClick.Length);
                if(cheat.StationsToClick[i] != lastClickedStations[idx])
                {
                    success = false;
                    break;
                }
            }

            if(success)
            {
                cheat.Activate();
            }
        }
    }
}

public abstract class Cheat
{
    public abstract int[] StationsToClick { get; }

    public abstract void Activate();
}

public class CheatAchievementSociety : Cheat
{
    static readonly int[] CHEAT_CODE = new int[] { 50, 43, 7, 12 };
    public override int[] StationsToClick
    {
        get { return CHEAT_CODE; }
    }

    public override void Activate()
    {
        //PopupManager.ShowPrompt("CHEAT ACTIVATED", "\n\nYou found a cheat code:\nAchievement 'Society'\n\n");
        //TODO KORION Achievements //Cleaned
        //Achievements.Unlock(Achievements.society);
    }
}
