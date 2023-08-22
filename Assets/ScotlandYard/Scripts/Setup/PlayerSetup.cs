using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

public enum PlayerColor
{
    MrXColor = 0,
    Yellow = 1,
    Red = 2,
    Black = 3,
    Blue = 4,
    Green = 5,
}

public static class PlayerColorExtension
{
    public static Color GetColor(this PlayerColor self)
    {
        switch (self)
        {
            case PlayerColor.MrXColor:
                return Helpers.ColorFromRGB(255, 255, 255);
            case PlayerColor.Yellow:
                return Helpers.ColorFromRGB(255, 211, 6);
            case PlayerColor.Red:
                return Helpers.ColorFromRGB(210, 1, 2);
            case PlayerColor.Black:
                return Helpers.ColorFromRGB(0, 0, 0);
            case PlayerColor.Blue:
                return Helpers.ColorFromRGB(1, 94, 246);
            case PlayerColor.Green:
                return Helpers.ColorFromRGB(38, 229, 6);
            default:
                throw new ArgumentOutOfRangeException("self");
        }
    }

    public static string GetColorName(this PlayerColor self)
    {
        switch (self)
        {
            case PlayerColor.MrXColor:
                return "white";
            case PlayerColor.Yellow:
                return "yellow";
            case PlayerColor.Red:
                return "red";
            case PlayerColor.Black:
                return "black";;
            case PlayerColor.Blue:
                return "blue";
            case PlayerColor.Green:
                return "green";
            default:
                throw new ArgumentOutOfRangeException("self");
        }
    }

    public static string GetActorSpriteName(this PlayerColor self)
    {
        return string.Format("actor_{0}", self.GetColorName());
    }
    public static string GetDirectionIndicatorSpriteName(this PlayerColor self)
    {
        string col = (self == PlayerColor.MrXColor) ? "mrx" : self.GetColorName();
        return string.Format("player_indicator_{0}", col);
    }

    public static string GetPawnSpriteName(this PlayerColor self)
    {
        return string.Format("pawn_symbol_{0}", self.GetColorName());
    }
}

[Serializable]
public class PlayerSetup
{
    public PlayerController Controller;
    public AiDifficulty Difficulty;
    public int StartAtStationId;
    public string DisplayName = "";
    public int PlayerId;
    public PlayerColor Color;
    public string ControllingParticipantID;

    public PlayerSetup()
    { }
    public PlayerSetup(PlayerController controller, int playerId)
        : this(controller, playerId, AiDifficulty.Medium, -1)
    { }
    public PlayerSetup(PlayerController controller, int playerId, int startStationId)
        : this(controller, playerId, AiDifficulty.Medium, startStationId)
    { }
    public PlayerSetup(PlayerController controller, int playerId, AiDifficulty difficulty, int startStationId)
    {
        this.Controller = controller;
        this.Difficulty = difficulty;
        this.PlayerId = playerId;
        this.Color = (PlayerColor)PlayerId;

        if(startStationId <= 0)
        {
            Random rnd = new Random(playerId ^ DateTime.Now.Millisecond);
            startStationId = rnd.Next(1, Globals.StationCount + 1);
        }

        this.StartAtStationId = startStationId;
    }
}