using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public struct WayPartAI
{
    public const int UNREACHABLE = 9999;
    public const int INVALID = int.MaxValue;

    public Station Target;
    //public TicketPool TicketsLeftAfter;
    public int Step;
    public int WayWeightFromStart;
    public TransportationType BestTransport;

    public bool Invalid { get { return Target == null; } }
    public void MakeInvalid()
    {
        Target = null;
        Step = 0;
        WayWeightFromStart = INVALID;
    }

    public static WayPartAI operator +(WayPartAI a, WayPartAI b)
    {
        return new WayPartAI()
            {
                Step = a.Step + b.Step,
                WayWeightFromStart = a.WayWeightFromStart + b.WayWeightFromStart,
                Target = (a.Target == b.Target) ? a.Target : null,
                BestTransport = (a.BestTransport == b.BestTransport) ? a.BestTransport : 0,
            };
    }

    public static WayPartAI operator *(float scale, WayPartAI way)
    {
        return new WayPartAI()
        {
            Step = Mathf.RoundToInt(scale * way.Step),
            WayWeightFromStart = Mathf.RoundToInt(scale * way.WayWeightFromStart),
            Target = way.Target,
            BestTransport = way.BestTransport,
        };
    }

}
