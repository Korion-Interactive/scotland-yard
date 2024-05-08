using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using System.Collections;

public class AiDetectiveSystem : BaseSystem<GameEvents, AiDetectiveSystem>
{
    const int MAX_STEPS = 9;

    Station[] fallbackStationOfInterest;

    [HideInInspector]
    public HashSet<Station> forbiddenLocations = new HashSet<Station>();

    // weights are set in Inspector!
    public Tuple<float, float> WeightMultiplierRangeEasy;// = new Tuple<float, float>(0.5f, 1.5f);
    public Tuple<float, float> WeightMultiplierRangeMedium;// = new Tuple<float, float>(0.8f, 1.2f);

    internal bool IsThinking;

    protected override void RegisterEvents()
    {
        IsThinking = false;

        ListenTo(GameEvents.GameStart, GameStarts);
        ListenTo(GameEvents.TurnStart, TurnStart);
        ListenTo(GameEvents.MrXAppear, MrXAppears);
        ListenTo<MoveArgs>(GameEvents.MrXMove, MrXMove);
        ListenTo<MoveArgs>(GameEvents.DetectiveMove, DetectiveMove);
        ListenTo<MoveArgs>(GameEvents.PlayerMoveFinished, PlayerMoved);

        ListenTo(GameEvents.GameLoaded, GameStarts);
    }

    private void GameStarts(BaseArgs args)
    {
        fallbackStationOfInterest = GameObject.FindObjectsOfType<Station>()
            .Where((s) => s.HasAnyTransportationOption(TransportationType.Metro)).ToArray();

        foreach(Detective det in GameState.Instance.DetectivesIterator())
        {
            StartThinking(det, false);
        }
    }

    void PlayerMoved(MoveArgs args)
    {
        if (args.MovingPlayer.IsDetective)
        {
            this.LogDebug("Player Moved... Think about it!");
            if(args.MovingPlayer.GetReachableNeighbours().Count() > 0)
                StartThinking(args.MovingPlayer as Detective, false);
        }
    }

    private void DetectiveMove(MoveArgs args)
    {
        GameState.Instance.PossibleMrXLocations.Remove(args.To);

    }

    private void MrXMove(MoveArgs args)
    {
        // early out if mr x just appeared
        if (GameState.Instance.MrX.AppearedXMovesAgo() == 0)
            return;

        HashSet<Station> possibleLocations = new HashSet<Station>();

        foreach (Station s in GameState.Instance.PossibleMrXLocations)
        {
            foreach(var n in s.GetStationNeighbours(args.Ticket))
            {
                possibleLocations.Add(n);
            }
        }

        foreach (PlayerBase p in GameState.Instance.DetectivesIterator())
            possibleLocations.Remove(p.Location);

        GameState.Instance.PossibleMrXLocations = possibleLocations;
    }

    private void MrXAppears(BaseArgs args)
    {
        GameState.Instance.PossibleMrXLocations.Clear();
        GameState.Instance.PossibleMrXLocations.Add(GameState.Instance.MrX.LastAppearance);
    }

    private void TurnStart(BaseArgs args)
    {
        Detective det = args.RelatedObject.GetComponent<Detective>();
        if (det == null || det.PlayerInfo.Controller != PlayerController.Ai || !GameSetupBehaviour.Instance.LocalPlayer.IsResponsibleFor(det))
            return;

        this.LogDebug(det.name + ": Turn Start... Think how to move!");
        StartThinking(det, true);
    }

    private void StartThinking(Detective det, bool movePlayer)
    {
        if (GameState.Instance.CurrentPlayer != det)
            return;


        det.ClearConnectionWeights();

        StartCoroutine(Think(det, movePlayer));
    }

    private IEnumerator Think(Detective detective, bool movePlayer)
    {
        while (IsThinking)
            yield return new WaitForEndOfFrame();

        IsThinking = true;
        this.LogDebug("START THINKING " + detective.name);

        forbiddenLocations =
            GameState.Instance.DetectivesIterator()
            .Where((o) => o != detective)
            .Select((o) => o.Location)
            .Where((o) => detective.Location.GetStationNeighbours(detective.GetAvailableTransportTypes()).Contains(o))
            .ToHashSet();

        // weight the map
        var initialStation = new HashSet<Trio<Station, TicketCollection, int>>();
        initialStation.Add(new Trio<Station, TicketCollection, int>(detective.Location, detective.PlayerState.Tickets, 0));

        yield return StartCoroutine(RecursiveExpand(detective, initialStation, 1));

        // find the best station of interest
        Station target = FindTargetStation(detective, GameState.Instance.PossibleMrXLocations, false);
        if (target == null)
            target = FindTargetStation(detective, fallbackStationOfInterest, false);

        yield return new WaitForEndOfFrame();

        // layaway back from target to start
        FindWay(detective, target);

        yield return new WaitForEndOfFrame();
        
#if UNITY_EDITOR
        DisplayWay(detective);

        yield return new WaitForEndOfFrame();
#endif

        if(movePlayer && GameState.Instance.CurrentPlayer == detective) // after game load the player may change instantly
            detective.GoStep(FindBestTicket(detective, detective.Location, detective.CurrentWayPlan[0]), detective.CurrentWayPlan[0]);

        this.LogDebug("END THINKING " + detective.name);
        IsThinking = false;
    }

    private TransportationType FindBestTicket(Detective det, Station from, Station to)
    {
        List<TransportationType> transport = from.GetAllTransportationsTo(to).Where((o) => det.PlayerState.Tickets.CanUse(o)).ToList();
        this.Assert(transport.Count > 0);

        float biggest = 0;
        TransportationType result = TransportationType.Any;
        foreach(var tr in transport)
        {
            var pool = det.PlayerState.Tickets.GetTicketPoolCopy(tr);
            if(pool.TicketPercentAmount > biggest)
            {
                biggest = pool.TicketPercentAmount;
                result = tr;
            }
        }

        return result;
    }


    private void FindWay(Detective det, Station target)
    {
        List<Station> wayToGo = new List<Station>();
        wayToGo.Add(target);

        UnityEngine.Debug.Log("Trying to find way. Det: " + det + ", target: " + target);
        int step = det.ConnectionWeight[target.Id].Step;

        //if(target.Id == 46) // DEBUG
        //{
        //    int i = step;
        //    i = step;
        //    i = step;
        //}
        int debug = 0;
        while (target != det.Location)
        {
            debug++;
            if (debug > Globals.MrX_Appear_Last_Time)
            {
                this.LogError(det.name + ": Dead Lock detected! jumping out of method");
                break;
            }

            step--;

            var list = target.GetStationNeighbours(TransportationType.Taxi | TransportationType.Bus | TransportationType.Metro)
                .Where(o => !forbiddenLocations.Contains(o) && det.ConnectionWeight[o.Id].Step == step || det.Location == o)
                .ToList();
                
            //    target.GetStationNeighbours(det.GetAvailableTransportTypes(true))
            //    .Where(o => !forbiddenLocations.Contains(o))
            //    .ToList();

            //// check the step only if necessary... sometimes it produces errors.
            //var bestList = list.Where((o) => (det.ConnectionWeight[o.Id].Step == step || det.Location == o)).ToList();
            //if (bestList.Count > 0)
            //    list = bestList;

            Station s = FindTargetStation(det, list, true);

            if (s == null)
            {
                // emergency way
                this.LogWarn(det.name + ": way is corrupt. Trying to find an emergency way...");

                list = target.GetStationNeighbours(det.GetAvailableTransportTypes(true)).Where((o) => !forbiddenLocations.Contains(o)).ToList();
                list.Sort((a, b) => det.ConnectionWeight[a.Id].Step.CompareTo(det.ConnectionWeight[b.Id].Step));
                list = list.Where(o => det.ConnectionWeight[o.Id].Step <= det.ConnectionWeight[list[0].Id].Step).ToList();

                s = FindTargetStation(det, list, true);

                if(s == null)
                {
                    this.LogError(det.name + ": failed to find emergency way :(");
                    break;
                }
            }

            wayToGo.Add(s);
            target = s;

        }

        wayToGo.Remove(det.Location); // remove current location
        wayToGo.Reverse();

        if(!det.Location.GetStationNeighbours(det.GetAvailableTransportTypes()).Contains(wayToGo[0]))
        {
            this.LogError(det.name + ": way to non-neighbour station! this shouldn't happen!");

            wayToGo.Clear();
            wayToGo.Add(det.GetRandomReachableNeighbour());
        }

#if UNITY_EDITOR
        this.LogDebug(string.Format("{0} way: [{1}] {2}", det.name, det.Location.Id, wayToGo.ToString(o => " > " + o.Id)));
#endif
        det.CurrentWayPlan = wayToGo;

    }

    private Station FindTargetStation(Detective det, IEnumerable<Station> stationsToLookAt, bool searchForOwnLocation)
    {
#if UNITY_EDITOR
        if (GameState.Instance.MrX.HasAlreadyAppeared() &&  stationsToLookAt.Count() == 0)
            this.LogError("no stations to look after! (" + det.name + ")");
#endif

        List<Tuple<int, Station>> list = new List<Tuple<int, Station>>();
        foreach(var station in stationsToLookAt)
        {
            int id = station.Id;
            var w = det.ConnectionWeight[id];

            if (det.Location.Id == id)
            {
                if (searchForOwnLocation)
                    return w.Target;
                else
                    continue;
            }

            if (forbiddenLocations.Contains(station) )//|| ai.ConnectionWeight[id].Invalid)
                continue;

            int weight = w.WayWeightFromStart;

            //if (weight <= 0)
            //    continue;

            list.Add(new Tuple<int, Station>(weight, station));
        }
        list.Sort((a, b) => a.A.CompareTo(b.A));

        if(list.Count == 0)
        {
            if (GameState.Instance.Round > 2)
                this.LogError(string.Format("FindTargetStation: nothing in list! \nlookedAt: {0} \nforbidden: {1}",
                    stationsToLookAt.ToString(o => o.Id.ToString()), 
                    forbiddenLocations.ToString(o => o.Id.ToString())));

            return null;
        }

        return list[0].B;
    }

    private IEnumerator RecursiveExpand(Detective det, HashSet<Trio<Station, TicketCollection, int>> wayPoints, int step)
    {
        //this.LogDebug("Step ---------------------- " + step);

        if (step >= MAX_STEPS)
        {
            yield return new WaitForEndOfFrame();
            yield break;
        }

        HashSet<Trio<Station, TicketCollection, int>> nextWayPoints = new HashSet<Trio<Station, TicketCollection, int>>();

        foreach (var w in wayPoints)
        {
            Station location = w.A;


            int weightFromStart = w.C;
            //var tickets = w.B; << it is a struct. save performance by not instancing this unnecessarily.
            //this.LogDebug("Iterate Neighbours...");

            foreach (Tuple<TransportationType, Station> n in location.GetAllStationNeighbours())
            {
                TransportationType tr = n.A;
                Station station = n.B;

               // this.LogDebug("NEIGHBOUR " + n.B.Id);

                if (forbiddenLocations.Contains(station))
                    continue;


                float mul = GetCrossingWeightMultiplier(det, station, step);

                // make metro connections too expensive to not waste them before mr x has appeared
                if (tr == TransportationType.Metro && !GameState.Instance.MrX.HasAlreadyAppeared())
                    mul += 10;

                int weight = GetWeight(tr, mul, ref w.B);
                int totalWeight = weightFromStart + weight;

                // is better way? -> use it.
                if ((det.ConnectionWeight[station.Id].Invalid /*&& weight != WayPartAI.UNREACHABLE*/)
                    || det.ConnectionWeight[station.Id].WayWeightFromStart >= totalWeight)
                {
                    if (det.ConnectionWeight[station.Id].WayWeightFromStart == totalWeight
                        && wayPoints.FirstOrDefault((o) => o.A.Id == station.Id) != null)
                        continue;


                    det.ConnectionWeight[station.Id].Target = station;//(weight < WayPartAI.UNREACHABLE) ? station : null;

                    det.ConnectionWeight[station.Id].BestTransport = tr;

                    det.ConnectionWeight[station.Id].WayWeightFromStart = totalWeight;
                    det.ConnectionWeight[station.Id].Step = step;


                    Trio<Station, TicketCollection, int> next = new Trio<Station, TicketCollection, int>(ref station, ref w.B, ref totalWeight);
                    next.B.UseTicket(tr);

                    var old = nextWayPoints.FirstOrDefault((o) => o.A == station);
                    if (old != null)
                        nextWayPoints.Remove(old);

                    nextWayPoints.Add(next);
                    //this.LogDebug(string.Format("add Station: {0} - weight: {1}", next.A.Id, totalWeight));

                   // yield return new WaitForSeconds(0.1f);
                }
            }
        }

        //this.LogDebug("NEXT");
        yield return StartCoroutine(RecursiveExpand(det, nextWayPoints, step + 1));

        yield return new WaitForEndOfFrame();
    }

    private int GetWeight(TransportationType transport, float multiplier, ref TicketCollection tickets)
    {
        TicketPool pool = tickets.GetTicketPoolCopy(transport);

        if (pool.TicketsLeft <= 0)
            return WayPartAI.UNREACHABLE;

        //float mulitplier = 100 * GetCrossingWeightMultiplier(station, step);

        //int result = (int)(100 * multiplier * ((float)pool.TicketsLeft / pool.StartAmount));
        int result = (int)(100 * multiplier * ((float)pool.StartAmount / pool.TicketsLeft));
        

        return result;
    }

    // AI difficulty: diverse weighting
    float GetCrossingWeightMultiplier(Detective det, Station station, int step)
    {
        if (det.PlayerInfo.Controller == PlayerController.Ai && det.PlayerInfo.Difficulty == AiDifficulty.Easy)
            return UnityEngine.Random.Range(WeightMultiplierRangeEasy.A, WeightMultiplierRangeEasy.B);

        float baseMul = (det.PlayerInfo.Controller != PlayerController.Ai || det.PlayerInfo.Difficulty == AiDifficulty.Hard)
            ? 1
            : UnityEngine.Random.Range(WeightMultiplierRangeMedium.A, WeightMultiplierRangeMedium.B);

        foreach (var d in GameState.Instance.DetectivesIterator())
        {
            if (d == det)
                continue;

            if (d.CurrentWayPlan != null
                && ((d.CurrentWayPlan.Count > step && d.CurrentWayPlan[step] == station)
                || (d.CurrentWayPlan.Count > step + 1 && d.CurrentWayPlan[step + 1] == station)))
            {
                if (d.PlayerInfo.Controller != PlayerController.Ai)
                {
                    return 2 * baseMul;
                }
                else
                {
                    switch(d.PlayerInfo.Difficulty)
                    {
                        case AiDifficulty.Easy:
                            return 1.333f * baseMul;
                        case AiDifficulty.Medium:
                            return 2 * baseMul;
                        case AiDifficulty.Hard:
                            return 2.5f * baseMul;

                    }
                }
                
            }
        }

        return baseMul;
    }


#if UNITY_EDITOR

	// disabled "Unreachable code detected" warning 
	#pragma warning disable 162
    private void DisplayWay(Detective det)
    {
        return;
        List<UnityEngine.GameObject> select = new List<UnityEngine.GameObject>();
        var connections = FindObjectsOfType<StationConnection>();
        for (int i = -1; i < det.CurrentWayPlan.Count - 1; i++)
        {
            Station a = (i == -1) ? det.Location : det.CurrentWayPlan[i];
            Station b = det.CurrentWayPlan[i + 1];

            var con = connections.FirstOrDefault((o)
                => o.Transportation == det.ConnectionWeight[b.Id].BestTransport
                && ((o.StationA == a && o.StationB == b) || (o.StationA == b && o.StationB == a)));

            if (con != null)
                select.Add(con.gameObject);
        }

        UnityEditor.Selection.objects = select.ToArray();
    }
	#pragma warning restore 162

#endif
}