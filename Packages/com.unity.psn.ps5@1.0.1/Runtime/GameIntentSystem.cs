
using System;
using System.IO;

using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.GameIntent
{
    /// <summary>
    /// The game intent system notifies an application about information that directly leads a user to a specific scene in the application from various system software features or PSN services.
    /// </summary>
    public class GameIntentSystem
    {
        enum NativeMethods : UInt32
        {
            FetchGameIntent = 0x0400001u,
        }

        /// <summary>
        ///  Delegate for notifications about game intent events.
        /// </summary>
        /// <param name="gameIntent">The game intent event data</param>
        public delegate void GameIntentNotification(GameIntent gameIntent);

        /// <summary>
        /// Event called when a game intent notification occurs.
        /// </summary>
        public static event GameIntentNotification OnGameIntentNotification;

        internal static void Start()
        {
            Main.OnSystemUpdate += Update;
        }

        internal static void Stop()
        {
            Main.OnSystemUpdate -= Update;
        }

        /// <summary>
        /// Update function
        /// </summary>
        private static void Update()
        {
            GameIntent gi = FetchNext();

            if (gi != null)
            {
                OnGameIntentNotification(gi);
            }
        }

        internal static GameIntent FetchNext()
        {
            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.FetchGameIntent);

            nativeMethod.Call();

            APIResult result = nativeMethod.callResult;

            MarshalMethods.ReleaseHandle(nativeMethod);

            if (result.RaiseException == true) throw new PSNException(result);

            if (nativeMethod.ResultsSize == 0)
            {
                return null;
            }

            string typeName = nativeMethod.Reader.ReadPrxString();

            GameIntent newIntent = null;

            if (typeName == "joinSession")
            {
                newIntent = new JoinSession();
            }
            else if (typeName == "launchActivity")
            {
                newIntent = new LaunchActivity();
            }
            else if (typeName == "launchMultiplayerActivity")
            {
                newIntent = new LaunchMultiplayerActivity();
            }
            else
            {
                newIntent = new GameIntent(GameIntent.IntentTypes.Unknown);
            }

            // Read back the results from the native method
            newIntent.Deserialise(nativeMethod.Reader);

            return newIntent;
        }

        /// <summary>
        /// The type of game intent notificaiton. This is a base class for all game intent types
        /// </summary>
        public class GameIntent
        {
            /// <summary>
            /// The types of intent
            /// </summary>
            public enum IntentTypes
            {
                /// <summary>Unknown intent type</summary>
                Unknown,
                /// <summary>Joining a session.</summary>
                JoinSession,
                /// <summary>Launching an activity</summary>
                LaunchActivity,
                /// <summary> </summary>
                LaunchMultiplayerActivity,
            };

            /// <summary>
            /// User ID of the intent
            /// </summary>
            public Int32 UserId { get; internal set; }

            /// <summary>
            /// The intent type
            /// </summary>
            public IntentTypes IntentType { get; internal set; }

            internal GameIntent(IntentTypes intentType)
            {
                IntentType = intentType;
            }

            virtual internal void Deserialise(BinaryReader reader)
            {
                UserId = reader.ReadInt32();
            }
        }

        /// <summary>
        /// This game intent event represents launching an activity.
        /// </summary>
        public class LaunchActivity : GameIntent
        {
            /// <summary>
            /// Activity ID string
            /// </summary>
            public string ActivityId { get; internal set; }

            /// <summary>
            /// Construct a launch activity intent instance
            /// </summary>
            public LaunchActivity() : base(IntentTypes.LaunchActivity) { }

            override internal void Deserialise(BinaryReader reader)
            {
                base.Deserialise(reader);

                ActivityId = reader.ReadPrxString();
            }
        }

        /// <summary>
        /// This game intent event represents joining a session.
        /// </summary>
        public class JoinSession : GameIntent
        {
            /// <summary>
            /// Member types
            /// </summary>
            public enum MemberTypes
            {
                /// <summary>Unknown</summary>
                Unknown,
                /// <summary>"player" type</summary>
                Player,
                /// <summary>"spectator" type</summary>
                Spectator,
            };

            /// <summary>
            /// Player Session ID string
            /// </summary>
            public string PlayerSessionId { get; internal set; }

            /// <summary>
            ///  Member type
            /// </summary>
            public MemberTypes MemberType { get; internal set; }

            /// <summary>
            /// Construct a join session intent instance
            /// </summary>
            public JoinSession() : base(IntentTypes.JoinSession) { }

            override internal void Deserialise(BinaryReader reader)
            {
                base.Deserialise(reader);

                PlayerSessionId = reader.ReadPrxString();

                string memberTypeName = reader.ReadPrxString();

                MemberType = MemberTypes.Unknown;

                if (memberTypeName == "player") MemberType = MemberTypes.Player;
                else if (memberTypeName == "spectator") MemberType = MemberTypes.Spectator;
            }
        }

        /// <summary>
        /// This game intent event represents launching an activity that supports online multi-play.
        /// </summary>
        public class LaunchMultiplayerActivity : GameIntent
        {
            /// <summary>
            /// Activity ID string
            /// </summary>
            public string ActivityId { get; internal set; }

            /// <summary>
            ///  Player Session ID string
            /// </summary>
            public string PlayerSessionId { get; internal set; }

            /// <summary>
            /// Construct a launch multiplayer activity intent instance
            /// </summary>
            public LaunchMultiplayerActivity() : base(IntentTypes.LaunchMultiplayerActivity) { }

            override internal void Deserialise(BinaryReader reader)
            {
                base.Deserialise(reader);

                ActivityId = reader.ReadPrxString();

                PlayerSessionId = reader.ReadPrxString();
            }
        }

    }
}
