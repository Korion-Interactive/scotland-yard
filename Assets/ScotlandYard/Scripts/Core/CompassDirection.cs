using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum CompassDirection
{
    Undefined = -1,

    East = 0,
    NorthEast = 45,
    North = 90,
    NorthWest = 135,
    West = 180,
    SouthWest = 225,
    South = 270,
    SouthEast = 315,
}

public static class CompassDirectionExtensions
{
    public static Vector3 ToDirectionVector3(this CompassDirection self)
    {
        return ToDirectionVector2(self);
    }
    public static Vector2 ToDirectionVector2(this CompassDirection self)
    {
        return new Vector2(Mathf.Cos(Mathf.Deg2Rad * (float)self), Mathf.Sin(Mathf.Deg2Rad * (float)self));
    }

}