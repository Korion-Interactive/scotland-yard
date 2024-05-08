using UnityEngine;

public static class Globals
{
	public static string VersionNumber
	{
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5
        get { return "1.0.0"; }
#else
        get { return Application.version; }
#endif
    }

	// set _isHumbleBundle to true if it is the Humble Bundle
	private const bool _isHumbleBundle = false; 
	public static bool IsHumbleBundle
	{
		get
		{
#if UNITY_ANDROID && GOOGLE_PLAY
				return _isHumbleBundle;
#else
				return false;
#endif
		}
	}

    public const bool IsDevBuild = false;

    public const string PlayerReady_LeftGame = "Cross";
    public const string PlayerReady_OtherReady = "BlueCheck";
    public const string PlayerReady_SelfReady = "WhiteCheck";
#if UNITY_SWITCH || UNITY_PS4 || UNITY_PS5 || UNITY_STANDALONE
    public static readonly string LastGameStatePath = "gamestate.json";
    public static readonly string LastGameSetupPath = "gamesetup.json";
#else
    public static readonly string LastGameStatePath = Application.persistentDataPath + "/gamestate.json";
    public static readonly string LastGameSetupPath = Application.persistentDataPath + "/gamesetup.json";
#endif

    public const int StationCount = 199;

    public const int Detective_TicketStartAmount_Taxi = 11;
    public const int Detective_TicketStartAmount_Bus = 8;
    public const int Detective_TicketStartAmount_Metro = 4;

    public const int MrX_TicketStartAmount_Taxi = 99;
    public const int MrX_TicketStartAmount_Bus = 99;
    public const int MrX_TicketStartAmount_Metro = 99;
    public const int MrX_StartAmount_DoubleTickets = 2;

    public const int MrX_Appear_1st_Time = 3;
    public const int MrX_Appear_2nd_Time = 8;
    public const int MrX_Appear_3rd_Time = 13;
    public const int MrX_Appear_4th_Time = 18;
    public const int MrX_Appear_Last_Time = 24;

    public static readonly int[] StartStations = new int[] { 13, 26, 29, 34, 50, 53, 91, 94, 103, 112, 117, 132, 138, 141, 155, 174, 197, 198, };


    public const byte Net_Context_Connection = 0;
    public const byte Net_Context_GameSetup = 1;
    public const byte Net_Context_SelectStartStations = 2;
    public const byte Net_Context_Ingame = 3;
    public const byte Net_Context_Chat = 4;

    public const int Listen_Early = -1000;
    public const int Listen_Normal = 0;
    public const int Listen_Late = 1000;


    public const int Max_Player_Count = 6;
}
