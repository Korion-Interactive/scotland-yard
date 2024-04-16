using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AchievementStatsSystem : BaseSystem<GameEvents, AchievementStatsSystem>
{
    List<Station> lastMrXStations = new List<Station>();
    bool gameEndAlreadyTracked = false;
    protected override void RegisterEvents()
    {
        gameEndAlreadyTracked = false;

        ListenTo<GameOverArgs>(GameEvents.GameEnd, GameEnd);
        ListenTo<MoveArgs>(GameEvents.PlayerMoveFinished, PlayerMoved);
        ListenTo(GameEvents.MrXUseDoubleTicket, DoubleTicketUsed);
        ListenTo(GameEvents.GameStart, GameStart);

        //TODO KORION Achievements //Cleaned
        //if(GSP.IsStatusAvailable)
        //    GSP.Status.AchievementUnlockedSuccessfully += AnyAchievementUnlocked;

    //    StartCoroutine(LoadAllAchievementsFromServer());
    //}

    //IEnumerator LoadAllAchievementsFromServer()
    //{
    //    yield return new WaitForSeconds(1f);

        //KORION Achievements setup //Cleaned
        //AnyAchievementUnlocked(null); // Check achievements on startup to get the right data of previous iOS version of the game

    }

    protected override void OnDestroy()
    {
        //KORION Achievements setup //Cleaned
        //if (GSP.IsStatusAvailable)
        //    GSP.Status.AchievementUnlockedSuccessfully -= AnyAchievementUnlocked;

        base.OnDestroy();
    }

    bool IsTrackingAllowed()
    {
        if (GameSetupBehaviour.Instance.Setup.Mode != GameMode.HotSeat && GameSetupBehaviour.Instance.Setup.Mode != GameMode.Network)
            return false;

        bool isMrXHuman = GameState.Instance.MrX.PlayerInfo.Controller == PlayerController.Human;
        bool isAnyDetectiveHuman = GameSetupBehaviour.Instance.CountDetectiveControlTypes(PlayerController.Human) > 0;

        return (isMrXHuman && !isAnyDetectiveHuman) || (!isMrXHuman && isAnyDetectiveHuman);
    }

    private void GameStart(BaseArgs obj)
    {
        if (!IsTrackingAllowed())
            return;

        foreach(var d in GameSetupBehaviour.Instance.Setup.DetectiveSetups)
        {
            if (d.Controller == PlayerController.Human)
                Stats.TrackColor(d.Color);
        }

        lastMrXStations.Add(GameState.Instance.MrX.Location);
    }
    private void DoubleTicketUsed(BaseArgs args)
    {
        if (!IsTrackingAllowed())
            return;

        if(GameState.Instance.MrX.PlayerInfo.Controller == PlayerController.Human)
        {
            Stats.DoubleTicketsUsed++;
            //TODO KORION Achievements
            
            if(Stats.DoubleTicketsUsed > 100)
            Achievements.Unlock(Achievements.life_in_a_fast_lane);
        }
    }

    private void PlayerMoved(MoveArgs args)
    {
        if (!IsTrackingAllowed())
            return;

        PlayerBase player = args.MovingPlayer;

        if (player == null || player.PlayerInfo.Controller != PlayerController.Human)
            return;

        switch (args.Ticket)
        {
            case TransportationType.Taxi:
                Stats.TaxiRides++;
                Debug.Log("Stats.TaxiRides: " + Stats.TaxiRides);
                //TODO KORION Achievements
                if(Stats.TaxiRides >= 100)
                    Achievements.Unlock(Achievements.driver);
                break;
            case TransportationType.Bus:
                Stats.BusRides++;

                Debug.Log("Stats.BusRides: " + Stats.BusRides);
                //TODO KORION Achievements
                if (Stats.BusRides >= 100)
                    Achievements.Unlock(Achievements.sightseeing_tour);
                    break;
            case TransportationType.Metro:
                Stats.MetroRides++;

                Debug.Log("Stats.MetroRides: " + Stats.MetroRides);
                //TODO KORION Achievements
                if (Stats.MetroRides >= 100)
                    Achievements.Unlock(Achievements.nothing_better_than_tube);
                break;
            case TransportationType.Any:
                Stats.BlackTicketsUsed++;

                Debug.Log("Stats.BlackTicketsUsed: " + Stats.BlackTicketsUsed);
                //TODO KORION Achievements
                if (Stats.BlackTicketsUsed > 100)
                    Achievements.Unlock(Achievements.specialist);
                    break;
        }

        if (player.IsMrX)
        {
            // THE SHADOW (Achievement check) "Stay within 2 taxi stations for 3 turns as Mr. X"
            lastMrXStations.Add(args.To);

            if(lastMrXStations.Count >= 3)
            {
                var a = lastMrXStations[lastMrXStations.Count - 1];
                var b = lastMrXStations[lastMrXStations.Count - 2];
                var c = lastMrXStations[lastMrXStations.Count - 3];

                bool twoStations = (a.Id == b.Id) || (a.Id == c.Id) || (b.Id == c.Id); // actually only the middle one can happen
                bool taxiOnly = !a.HasAnyTransportationOption(TransportationType.Bus | TransportationType.Metro | TransportationType.Ferry)
                             && !b.HasAnyTransportationOption(TransportationType.Bus | TransportationType.Metro | TransportationType.Ferry)
                             && !c.HasAnyTransportationOption(TransportationType.Bus | TransportationType.Metro | TransportationType.Ferry);

                if(twoStations && taxiOnly)
                {
                    //TODO KORION Achievements
                    Achievements.Unlock(Achievements.the_shadow);
                }
            }
        }
    }

    private void GameEnd(GameOverArgs args)
    {
        if (!IsTrackingAllowed() || gameEndAlreadyTracked)
            return;

        gameEndAlreadyTracked = true;

        bool humanDetectiveExists = GameState.Instance.DetectivesIterator().FirstOrDefault((o) => o.PlayerInfo.Controller == PlayerController.Human) != null;
        bool mrXIsHuman = GameState.Instance.MrX.PlayerInfo.Controller == PlayerController.Human;

        if(humanDetectiveExists)
        {
            Stats.GamesPlayedAsDetective++;
        }
        else if(mrXIsHuman)
        {
            Stats.GamesPlayedAsMrX++;
        }

        switch(args.Reason)
        {
            case GameOverReason.EscapeOfMrX:
                if (mrXIsHuman)
                {
                    Stats.GamesWonAsMrX++;
                    //TODO KORION Achievements
                    Achievements.Unlock(Achievements.mister_x);

                    if (Stats.GamesWonAsMrX >= 10)
                        Achievements.Unlock(Achievements.master_x);
                }
                break;
            case GameOverReason.MrXCaught:
            case GameOverReason.MrXSurrounded:
                if (humanDetectiveExists)
                {
                    Stats.GamesWonAsDetective++;
                    //TODO KORION Achievements
                    Achievements.Unlock(Achievements.the_detective);

                    if(Stats.GamesWonAsDetective >= 10)
                        Achievements.Unlock(Achievements.bames_jond);

                    if (GameState.Instance.MrX.Moves <= 8)
                    {
                        //TODO KORION Achievements
                        Achievements.Unlock(Achievements.undercover_agent);
                    }
                }
                break;
        }

        if(args.Reason == GameOverReason.MrXCaught && GameState.Instance.CurrentPlayer.PlayerInfo.Controller == PlayerController.Human)
        {
            //TODO KORION Achievements
            Achievements.Unlock(Achievements.gotcha);
        }

        Stats.Save();
    }


    void AnyAchievementUnlocked(string achievementId)
    {
        if (achievementId == Achievements.GetId(Achievements.mi_5))
            return;

        StopCoroutine(CheckMI5AndSociety());
        StartCoroutine(CheckMI5AndSociety());
    }

    IEnumerator CheckMI5AndSociety() // all other achievements unlocked? -- am i in society?
    {
        while (!GSP.IsStatusAvailable)
            yield return new WaitForSeconds(1);

        bool ready = false;
        IEnumerable<Achievement> data = null;

        Action<IEnumerable<Achievement>> success = (o) => { ready = true; data = o; };
        GSP.Status.AchievementsRetrievedEvt += success;

        GSP.Status.RetrieveAchievementData();

        while (!ready)
            yield return new WaitForSeconds(0.2f);

        GSP.Status.AchievementsRetrievedEvt -= success;


        if (data != null && data.Count() > 0)
        {
            // society
            Achievement society = data.FirstOrDefault((o) => o.Identifier == Achievements.GetId(Achievements.society));
            if(society != null)
            {
                if(society.Completed)
                    Stats.IsInSociety = true;
            }
            else
            {
                this.LogError("Society achievement not in retrieved list of achievements.");
            }


            // MI-5
            int uncompletedCount = data.Count((o) => o.Completed == false && o.Identifier != Achievements.GetId(Achievements.mi_5));

            if (uncompletedCount == 0)
            {
                //KORION Achievements //Cleaned
                //Achievements.Unlock(Achievements.mi_5);
            }
        }
    }
}
