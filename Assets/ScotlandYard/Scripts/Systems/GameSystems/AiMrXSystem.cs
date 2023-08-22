using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AiMrXSystem : BaseSystem<GameEvents, AiMrXSystem>
{
    private class WayPointConsideration
    {
        internal Station Location;
        internal WayPartAI Close, Sum;    
        internal WayPointConsideration(Station location, WayPartAI close, WayPartAI sum)
        {
            this.Location = location;
            this.Close = close;
            this.Sum = sum;
        }
    }


    private AiDifficulty difficulty { get { return GameState.Instance.MrX.PlayerInfo.Difficulty; } }

    bool usingDoubleTicket;

    protected override void RegisterEvents()
    {
        ListenTo(GameEvents.TurnStart, StartThinking);
    }


    private void StartThinking(BaseArgs args)
    {
        var mrX = args.RelatedObject.GetComponent<MrX>();
        if (mrX == null || mrX.PlayerInfo.Controller != PlayerController.Ai || !GameSetupBehaviour.Instance.LocalPlayer.IsResponsibleFor(mrX))
            return;

        if (GameState.Instance.CurrentPlayer != mrX)
            return;

        if (GameSetupBehaviour.Instance.Setup.Mode == GameMode.TutorialBasics)
        {
            mrX.Moves++;
            Broadcast(GameEvents.TurnEnd, mrX.gameObject);
            GameState.Instance.NextPlayer();
            Broadcast(GameEvents.TurnStart, GameState.Instance.CurrentPlayer.gameObject);
            return;
        }

        StartCoroutine(Think(mrX));
    }

    IEnumerator Think(MrX mrX)
    {

        while (AiDetectiveSystem.Instance.IsThinking)
            yield return new WaitForEndOfFrame();

        var go = FindBestLocationToGo(mrX, mrX.Location);

        usingDoubleTicket = ConsiderUsingDoubleTicket(mrX, go);

        yield return new WaitForEndOfFrame();


        if (GameState.Instance.CurrentPlayer != mrX) // on game load the player may change during thinking
            yield break;

        if (usingDoubleTicket)
            mrX.UseDoubleTicket();
        
        mrX.GoStep(FindBestTicket(mrX, mrX.Location, go.Location), go.Location);

    }

    private WayPointConsideration FindBestLocationToGo(MrX mrX, Station currentLocation)
    {
        var neighbours = mrX.GetReachableNeighbours().ToList();//currentLocation.GetStationNeighbours(mrX.GetAvailableTransportTypes()).ToList();
        List<WayPointConsideration> options = new List<WayPointConsideration>();

        for (int i = 0; i < neighbours.Count; i++)
        {
            var location = neighbours[i];

            WayPartAI low, sum;
            bool occupied;

            switch(difficulty)
            { 
                case AiDifficulty.Easy:
                    GetLocationWeights(location, out low, out sum, out occupied);
                    break;

                case AiDifficulty.Hard:
                    GetLocationWeightsRecursive(location, 3, 3, out low, out sum, out occupied);
                    break;

                default:
                    GetLocationWeightsRecursive(location, 1, 1, out low, out sum, out occupied);
                    break;
            }

            this.LogDebug(string.Format("WayInfo: {0} -> {1} / {2} [{3}]", location.Id, low.Step, sum.Step, (occupied ? "occupied" : "free")));

            if (!occupied)
            {
                options.Add(new WayPointConsideration(location, low, sum));
            }
        }

        options.Sort(SortMethod);
        //options.Reverse();
        return options[0];
    }

    private TransportationType FindBestTicket(MrX mrX, Station from, Station to)
    {
        List<TransportationType> transport = from.GetAllTransportationsTo(to).Where((o) => mrX.PlayerState.Tickets.CanUse(o)).ToList();

        if (transport.Contains(TransportationType.Ferry))
            return TransportationType.Any;

        bool hiddenTravel = mrX.AppearedXMovesAgo() <= 1 && mrX.PlayerState.Tickets.BlackTickets.TicketsLeft > 0
            && (transport.Contains(TransportationType.Metro) || transport.Contains(TransportationType.Ferry));

        TransportationType bestFittingTicket = TransportationType.Any;

        while (transport.Count > 0)
        {
            if (mrX.PlayerState.Tickets.GetTicketPoolCopy(transport[0]).TicketsLeft > 0)
                bestFittingTicket = transport[0];

            transport.RemoveAt(0);
        }

        this.Assert(bestFittingTicket != TransportationType.Any || mrX.PlayerState.Tickets.BlackTickets.TicketsLeft > 0);

        switch(mrX.PlayerInfo.Difficulty)
        {
            case AiDifficulty.Easy:

                return bestFittingTicket;

            case AiDifficulty.Medium:

                if (hiddenTravel && UnityEngine.Random.Range(0, 3) < 2)
                    return TransportationType.Any;

                return bestFittingTicket;

            case AiDifficulty.Hard:
                if (hiddenTravel)
                    return TransportationType.Any;

                return bestFittingTicket;
        }

        return TransportationType.Any;
    }

    private bool ConsiderUsingDoubleTicket(MrX mrX, WayPointConsideration wp)
    {
        int doubleTickets = mrX.PlayerState.Tickets.DoubleTickets;

        if (doubleTickets <= 0)
            return false;


        if (HasDetectiveAsNeighbour(wp.Location)) // has detective as neighbour
        {
            if (mrX.AppearedXMovesAgo() == 0)
                return true;

            switch (difficulty)
            {
                case AiDifficulty.Easy:
                    return UnityEngine.Random.Range(0, 3) < 2; // probability of 66 % 

                case AiDifficulty.Medium:
                    return UnityEngine.Random.Range(0, 4) < 3; // probability of 75 % 

                case AiDifficulty.Hard:
                    return true; // probability of 100 % 
            }
        }

        if (mrX.AppearsInXMoves() <= 2)
            return false;

        // maybe use double ticket if many possible mrX locations have a detective as neighbour
        int cnt = 0;
        int detCnt = 0;
        foreach(Detective d in GameState.Instance.DetectivesIterator())
        {
            foreach(Station neighbour in d.Location.GetAllStationNeighbours().Select(o => o.B))
            {
                if(GameState.Instance.PossibleMrXLocations.Contains(neighbour))
                {
                    cnt++;
                    break;
                }
            }

            detCnt++;
        }

        return UnityEngine.Random.Range(0, (Globals.MrX_Appear_Last_Time - GameState.Instance.Round) * detCnt) < cnt;
    }

    private bool HasDetectiveAsNeighbour(Station station)
    {
        foreach(Station n in station.GetStationNeighbours(TransportationType.Any))
        {
            var s = GameState.Instance.DetectivesIterator().FirstOrDefault((o) => o.Location == n);
            if (s != null)
                return true;
        }

        return false;
    }
    
    private void GetLocationWeightsRecursive(Station location, int initialRecursiveSteps, int recursiveSteps, out WayPartAI smallestWeight, out WayPartAI accumulatedWeight, out bool isOccupied)
    {
        GetLocationWeights(location, out smallestWeight, out accumulatedWeight, out isOccupied);

        if (isOccupied)
            return;

        foreach(Station neighbour in location.GetAllStationNeighbours().Select(o => o.B))
        {
            if(recursiveSteps > 0)
            {
                WayPartAI small, sum;
                bool occupied;

                GetLocationWeightsRecursive(neighbour, initialRecursiveSteps, recursiveSteps - 1, out small, out sum, out occupied);

                if(!occupied)
                {
                    WayPartAI add = ((float)recursiveSteps / initialRecursiveSteps) * small;
                    smallestWeight += add;
                    accumulatedWeight += add;
                }
            }
        }
    }

    private void GetLocationWeights(Station location, out WayPartAI smallestWeight, out WayPartAI accumulatedWeight, out bool isOccupied)
    {
        WayPartAI small = new WayPartAI();
        WayPartAI sum = new WayPartAI() { Target = location };
        isOccupied = false;

        foreach (Detective d in GameState.Instance.DetectivesIterator())
        {
            WayPartAI weight = d.ConnectionWeight[location.Id];
            isOccupied = isOccupied || d.Location == location;

            if (weight.Invalid)
                continue;

            if (small.Invalid || small.Step > weight.Step)
                small = weight;

            sum += weight;
        }

        smallestWeight = small;
        accumulatedWeight = sum;

    }

    private int SortMethod(WayPointConsideration a, WayPointConsideration b)
    {
        int neighbourCheck = 0;
        if (HasDetectiveAsNeighbour(a.Location))
        {
            if (!HasDetectiveAsNeighbour(b.Location))
                neighbourCheck = +1;
        }
        else if(HasDetectiveAsNeighbour(b.Location))
        {
            neighbourCheck = -1;
        }

        int fastTravel = 0;
        if (GameState.Instance.MrX.AppearsInXMoves() <= 2)
        {
            if (a.Location.HasAnyTransportationOption(TransportationType.Metro | TransportationType.Ferry))
                fastTravel = +1;
            else if (b.Location.HasAnyTransportationOption(TransportationType.Metro | TransportationType.Ferry))
                fastTravel = -1;
        }

        switch (difficulty)
        {
            case AiDifficulty.Easy:

                if (neighbourCheck != 0 && UnityEngine.Random.Range(0, 3) < 2)
                    return neighbourCheck;

                return Math.Sign(
                       (b.Close.Step + UnityEngine.Random.Range(-1f, +1f))
                     - (a.Close.Step + UnityEngine.Random.Range(-1f, +1f)));

            case AiDifficulty.Medium:

                if (neighbourCheck != 0)
                    return neighbourCheck;

                if(fastTravel != 0 && UnityEngine.Random.Range(0, 3) < 2)
                    return fastTravel;

                return b.Close.Step - a.Close.Step;

            case AiDifficulty.Hard:

                if (neighbourCheck != 0)
                    return neighbourCheck;

                if (fastTravel != 0)
                    return fastTravel;

                int result = b.Close.Step - a.Close.Step;
                if (result == 0)
                {
                    result = Math.Sign(b.Sum.WayWeightFromStart - a.Sum.WayWeightFromStart);
                    if (result == 0)
                    {
                        result = Math.Sign(b.Close.WayWeightFromStart - a.Close.WayWeightFromStart);

                        if (result == 0)
                        {
                            result = Math.Sign(b.Sum.Step - a.Sum.Step);
                        }
                    }
                }
                return result;

            default:

                this.LogError("Unexpected Difficulty");
                return 0;
        }
    }

}
