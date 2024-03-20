


using System;
using System.Collections.Generic;
using Unity.PSN.PS5.WebApi;
using UnityEngine;

namespace Unity.PSN.PS5.Sessions
{
    /// <summary>
    /// Default set of game session filters
    /// </summary>
    public class GameSessionFilters : WebApiFilters
    {
        public static readonly GameSessionFilters DefaultFilters = new GameSessionFilters();

        /// <summary>
        /// Create a standard set of game session filters
        /// </summary>
        public GameSessionFilters() : base(new string[]{ "psn:sessionManager:gameSession:created",
                                                         "psn:sessionManager:gameSession:deleted",
                                                         "psn:sessionManager:gs:m:players:created",
                                                         "psn:sessionManager:gs:m:players:deleted",
                                                         "psn:sessionManager:gs:m:spectators:created",
                                                         "psn:sessionManager:gs:m:spectators:deleted",
                                                         "psn:sessionManager:gs:maxPlayers:updated",
                                                         "psn:sessionManager:gs:maxSpectators:updated",
                                                         "psn:sessionManager:gs:joinDisabled:updated",
                                                         "psn:sessionManager:gs:representative:updated",
                                                         "psn:sessionManager:gs:m:players:swapped",
                                                         "psn:sessionManager:gs:m:spectators:swapped" },
                                                         "sessionManager", 0)
        {

        }

        /*
          Player Session
                "psn:sessionManager:playerSession:created"                  <- PlayerSession
                "psn:sessionManager:ps:maxPlayers:updated"                  <- PlayerSession
                "psn:sessionManager:ps:maxSpectators:updated"               <- PlayerSession
                "psn:sessionManager:ps:joinDisabled:updated"                <- PlayerSession
                "psn:sessionManager:ps:localizedSessionNames:updated"       <- PlayerSession
                "psn:sessionManager:ps:joinableUserType:updated"            <- PlayerSession
                "psn:sessionManager:ps:invitableUserType:updated"           <- PlayerSession
                "psn:sessionManager:ps:leaderPrivileges:updated"            <- PlayerSession
                "psn:sessionManager:ps:exclusiveLeaderPrivileges:updated"   <- PlayerSession
                "psn:sessionManager:ps:leader:updated"                      <- PlayerSession
                "psn:sessionManager:ps:swapSupported:updated"               <- PlayerSession
                "psn:sessionManager:playerSession:deleted"                  <- PlayerSession
                "psn:sessionManager:ps:joinableSpecifiedUsers:created"      <- PlayerSession
                "psn:sessionManager:ps:joinableSpecifiedUsers:deleted"      <- PlayerSession
                "psn:sessionManager:ps:m:players:deleted"                   <- PlayerSession
                "psn:sessionManager:ps:m:spectators:deleted"                <- PlayerSession
                "psn:sessionManager:ps:m:players:created"                   <- PlayerSession
                "psn:sessionManager:ps:m:spectators:created"                <- PlayerSession
                "psn:sessionManager:ps:m:players:swapped"                   <- PlayerSession
                "psn:sessionManager:ps:m:spectators:swapped"                <- PlayerSession
                "psn:sessionManager:ps:customData1:updated"                                             <- Not Guaranteed
                "psn:sessionManager:ps:customData2:updated"                                             <- Not Guaranteed
                "psn:sessionManager:ps:m:p:customData1:updated"                                         <- Not Guaranteed
                "psn:sessionManager:ps:m:s:customData1:updated"                                         <- Not Guaranteed
                "psn:sessionManager:ps:sessionMessage:created"                                          <- Not Guaranteed
                "psn:sessionManager:ps:invitations:created"                                             <- Not Guaranteed


          Game Session
                "psn:sessionManager:gs:customData1:updated"                                             <- Not Guaranteed
                "psn:sessionManager:gs:customData2:updated"                                             <- Not Guaranteed
                "psn:sessionManager:gs:m:p:customData1:updated"                                         <- Not Guaranteed
                "psn:sessionManager:gs:m:s:customData1:updated"                                         <- Not Guaranteed
                "psn:sessionManager:gs:sessionMessage:created"                                          <- Not Guaranteed
                "psn:sessionManager:gs:invitations:created"                                             <- Not Guaranteed
        */
    }

    /// <summary>
    /// Callbacks for game session updates
    /// </summary>
    public class GameSessionCallbacks
    {
        /// <summary>
        /// Update method called when an order-guaranteed push event is handled for a game session
        /// </summary>
        /// <remarks>
        /// Some of the notification will include Json data which is parsed when the notification is recieved.
        /// </remarks>
        public GameSession.SessionEventHandler OnSessionUpdated { get; set; } = null;

        /// <summary>
        /// Additional callback to process raw order-guaranteed push event WebApi Notifications.
        /// </summary>
        /// <remarks>
        /// The raw order-guaranteed event data, including the json string, is parsed to this callback
        /// Use this if additional processing is required for the raw event data.
        /// </remarks>
        public Session.RawSessionEventHandler WebApiNotificationCallback = null;
    }

    /// <summary>
    /// Notifications for game sessions
    /// </summary>
    public static class GameSessionNotifications
    {
        /// <summary>
        /// Types of game session notifications
        /// </summary>
        public enum NotificationTypes
        {
            /// <summary> A session has been created </summary>
            Created,
            /// <summary> A session has been deleted </summary>
            Deleted,
            /// <summary> Session parameters have changed </summary>
            ParamsChanged,
            /// <summary> A player has joined the session</summary>
            PlayerJoined,
            /// <summary> A player has left the session</summary>
            PlayerLeft,
            /// <summary> A spectator has joined the session</summary>
            SpectatorJoined,
            /// <summary> A spectator has left the session</summary>
            SpectatorLeft,
            /// <summary> A player has swapped to a spectator </summary>
            PlayerSwappedToSpectator,
            /// <summary> A spectator has swapped to a player </summary>
            SpectatorSwappedToPlayer,
            /// <summary> Custom data for a player has been set </summary>
            PlayerCustomDataUpdated,
            /// <summary> Custom data for a spectator has been set </summary>
            SpectatorCustomDataUpdated,
            /// <summary> An invitation for a user has been received </summary>
            InvitationsCreated,
            /// <summary> An session message for a user has been received </summary>
            SessionMessage
        }

        internal class EventHandlingConfig
        {
            public EventHandlingConfig(string dataType, NotificationTypes notificationType, GameSession.ParamTypes paramFlags, bool hasAdditionalJsonData, bool orderGuaranteed)
            {
                DataType = dataType;
                NotificationType = notificationType;
                ParamFlags = paramFlags;
                HasAdditionalJsonData = hasAdditionalJsonData;
                IsOrderGuaranteed = orderGuaranteed;
            }

            public string DataType { get; set; }

            public NotificationTypes NotificationType { get; set; }

            public GameSession.ParamTypes ParamFlags { get; set; }

            public bool HasAdditionalJsonData { get; set; }

            public bool IsOrderGuaranteed { get; set; }
        }

        // 26
        static internal EventHandlingConfig[] dataTypeToSessionEvents = new EventHandlingConfig[]
        {
            // Order-Guaranteed
            // These notification are recieved by the Push events assigned to the player session when it is created or joined.
            new EventHandlingConfig("psn:sessionManager:gameSession:created", NotificationTypes.Created, GameSession.ParamTypes.NotSet, false, true),
            new EventHandlingConfig("psn:sessionManager:gameSession:deleted", NotificationTypes.Deleted, GameSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:gs:maxPlayers:updated", NotificationTypes.ParamsChanged, GameSession.ParamTypes.MaxPlayers, false, true),
            new EventHandlingConfig("psn:sessionManager:gs:maxSpectators:updated", NotificationTypes.ParamsChanged, GameSession.ParamTypes.MaxSpectators, false, true),
            new EventHandlingConfig("psn:sessionManager:gs:joinDisabled:updated", NotificationTypes.ParamsChanged, GameSession.ParamTypes.JoinDisabled, false, true),
            new EventHandlingConfig("psn:sessionManager:gs:m:players:deleted", NotificationTypes.PlayerLeft, GameSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:gs:m:spectators:deleted", NotificationTypes.SpectatorLeft, GameSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:gs:m:players:created", NotificationTypes.PlayerJoined, GameSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:gs:m:spectators:created", NotificationTypes.SpectatorJoined, GameSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:gs:m:players:swapped", NotificationTypes.SpectatorSwappedToPlayer, GameSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:gs:m:spectators:swapped", NotificationTypes.PlayerSwappedToSpectator, GameSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:gs:representative:updated", NotificationTypes.ParamsChanged, GameSession.ParamTypes.Representative, true, true),

            // Order Not Guaranteed
            // There notification are recieved by individual users, who may or may not have an associated player session.
            // Some of these notifications are related to sessions, like  "psn:sessionManager:ps:customData1:updated"
            new EventHandlingConfig("psn:sessionManager:gs:sessionMessage:created", NotificationTypes.SessionMessage, GameSession.ParamTypes.NotSet, true, false),
            new EventHandlingConfig("psn:sessionManager:gs:customData1:updated", NotificationTypes.ParamsChanged, GameSession.ParamTypes.CustomData1, false, false),
            new EventHandlingConfig("psn:sessionManager:gs:customData2:updated", NotificationTypes.ParamsChanged, GameSession.ParamTypes.CustomData2, false, false),
            new EventHandlingConfig("psn:sessionManager:gs:m:p:customData1:updated", NotificationTypes.PlayerCustomDataUpdated, GameSession.ParamTypes.NotSet, true, false),
            new EventHandlingConfig("psn:sessionManager:gs:m:s:customData1:updated", NotificationTypes.SpectatorCustomDataUpdated, GameSession.ParamTypes.NotSet, true, false),
            new EventHandlingConfig("psn:sessionManager:gs:invitations:created", NotificationTypes.InvitationsCreated, GameSession.ParamTypes.NotSet, false, false),
        };

        static internal EventHandlingConfig GetEventConfig(string dataType)
        {
            for (int i = 0; i < dataTypeToSessionEvents.Length; i++)
            {
                if (dataTypeToSessionEvents[i].DataType == dataType)
                {
                    return dataTypeToSessionEvents[i];
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Game session instance
    /// </summary>
    public partial class GameSession
    {
        /// <summary>
        /// Notification from order-guaranteed push events
        /// </summary>
        public struct Notification
        {
            /// <summary>
            /// The type of notification for the session change
            /// </summary>
            public GameSessionNotifications.NotificationTypes NotificationType { get; internal set; }

            /// <summary>
            /// The session changed by the notification
            /// </summary>
            public GameSession Session { get; internal set; }

            /// <summary>
            /// The flag incidating the session parameter that has been changed
            /// </summary>
            public GameSession.ParamTypes SessionParamUpdates { get; internal set; }

            /// <summary>
            /// The session member being updated. Can be null if the notification isn't related to a member.
            /// </summary>
            public SessionMember Member { get; internal set; }

            internal void SetSessionUpdated(GameSessionNotifications.NotificationTypes notification, GameSession session , GameSession.ParamTypes paramTypes)
            {
                NotificationType = notification;
                Session = session;
                SessionParamUpdates = paramTypes;
                Member = null;
            }

            internal void SetMemberUpdated(GameSessionNotifications.NotificationTypes notification, GameSession session, SessionMember member)
            {
                NotificationType = notification;
                Session = session;
              //  SessionParamUpdates = PlayerSession.ParamTypes.NotSet;
                Member = member;
            }
        }

        /// <summary>
        /// Delegate for notifications for session events.
        /// </summary>
        public delegate void SessionEventHandler(Notification notificationData);

        /// <summary>
        /// The session notification callback. Note that this type of update doesn't contain any data
        /// to the locally cached data doesn't reflect the change. This only notifies what type of data changed.
        /// Use <see cref="PlayerSessionRequests.GetPlayerSessionsRequest"/> to retieve the latest data about the session.
        /// </summary>
        public SessionEventHandler OnSessionUpdated { get; set; }

        static internal void SessionWebApiEventHandler(WebApiNotifications.CallbackParams eventData)
        {
            GameSession session = null;

            NotificationSessionData eventJsonData = null;

            // All eventData callback params should have a Data sestion which contains the session Id
            if (eventData.Data != null)
            {
                eventJsonData = JsonUtility.FromJson<NotificationSessionData>(eventData.Data);

                if (eventJsonData != null && eventJsonData.sessionId != null)
                {
                    // If this is a creation event use the creation method to either create or find the session
                    // Timing issue can occur if the notification is recieved before the creation request has completed.
                    if (eventData.DataType == "psn:sessionManager:gameSession:created")
                    {
                        bool isNew = false;
                        session = SessionsManager.CreateGameSession(eventJsonData.sessionId, out isNew);
                    }
                    else
                    {
                        session = SessionsManager.FindGameSessionFromSessionId(eventJsonData.sessionId);
                    }

                    if (session == null)
                    {
                        // Can't find a session. This should not happen as there should never been an event arrive for a session that doesn't exist
                        // The session may have been deleted and events that are arriving are for old session.
                        // This situation needs to be handled.
                        // Check to see if this session id was ever deleted
                        if (SessionsManager.IsGameSessionDeleted(eventJsonData.sessionId) == false)
                        {
#if DEBUG
                            Debug.LogError("Game Session event : " + eventData.DataType + " can't find session id " + eventJsonData.sessionId);
#endif
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }

            if (session != null)
            {
                session.EventHandler(eventData, eventJsonData);
            }
            else
            {
#if DEBUG
                Debug.LogError("Session Id from json data not found for : " + eventData.DataType);
                if (eventData.Data != null)
                {
                    Debug.LogError("Event data : " + eventData.DataType);
                }
#endif
            }
        }

        private SessionMember UpdateFrom(GameSessionNotifications.NotificationTypes notification, NotificationMember memberUpdate, bool isSpectator)
        {
            if (memberUpdate.accountId == 0) return null;

            // memberUpdate might be a delete, create, swapped or customedata update
            // Look for existing session member
            bool inSpectatorList;
            SessionPlatforms memberPlatform = PlayerSession.FromString(memberUpdate.platform);
            SessionMember sessionMember = FindFromAccountId(new SessionMemberIdentifier (memberUpdate.accountId, memberPlatform), out inSpectatorList);

            if (notification == GameSessionNotifications.NotificationTypes.PlayerJoined || notification == GameSessionNotifications.NotificationTypes.SpectatorJoined)
            {
                if (sessionMember == null)
                {
                    // Member not in list so add it
                    sessionMember = new SessionMember();
                    if (isSpectator == true)
                    {
                        Spectators.Add(sessionMember);
                    }
                    else
                    {
                        Players.Add(sessionMember);
                    }
                }

                sessionMember.UpdateFrom(memberUpdate, isSpectator);
            }
            else if (notification == GameSessionNotifications.NotificationTypes.PlayerLeft || notification == GameSessionNotifications.NotificationTypes.SpectatorLeft)
            {
                if (sessionMember != null)
                {
                    sessionMember.UpdateFrom(memberUpdate, isSpectator);

                    RemoveMember(sessionMember.AccountIdAndPlatform);
                }
            }
            else if (notification == GameSessionNotifications.NotificationTypes.PlayerSwappedToSpectator || notification == GameSessionNotifications.NotificationTypes.SpectatorSwappedToPlayer)
            {
                if (sessionMember != null)
                {
                    sessionMember.UpdateFrom(memberUpdate, isSpectator);
                    MoveMember(sessionMember);  // Called after IsSpectator flag updated. This will ensure it goes into the correct list
                }
            }
            else if (notification == GameSessionNotifications.NotificationTypes.PlayerCustomDataUpdated || notification == GameSessionNotifications.NotificationTypes.SpectatorCustomDataUpdated)
            {

            }

            return sessionMember;
        }

        private void UpdateMembers(GameSessionNotifications.NotificationTypes notification, NotificationMembers notificationsMember)
        {
            if (notificationsMember != null && (notificationsMember.players != null || notificationsMember.spectators != null))
            {
                bool isSpectator = notificationsMember.spectators != null;

                NotificationMember[] members = notificationsMember.spectators;
                if (members == null)
                {
                    members = notificationsMember.players;
                }

                if (members != null)
                {
                    for (int i = 0; i < members.Length; i++)
                    {
                        if (members[i] != null)
                        {
                            SessionMember member = UpdateFrom(notification, members[i], isSpectator);

                            // Member could be null if two or more PlayerLeft events occured.
                            // This can happen if more than one user, on the local device, is in the same session.
                            // All the local users will get a PlayerLeft event, but only the first one will find and remove
                            // the player. After that all the other events don't need to do anything.
                            if (member != null)
                            {
                                if (OnSessionUpdated != null)
                                {
                                    Notification notificationData = new Notification();
                                    notificationData.SetMemberUpdated(notification, this, member);
                                    OnSessionUpdated(notificationData);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void SendUpdate(GameSessionNotifications.NotificationTypes notification, ParamTypes paramFlags, NotificationSessionData jsonData, UInt64 fromAccountId = 0, UInt64 toAccountId = 0)
        {
            //  Debug.LogError("Event SendUpdate : " + notification + " : " + paramFlags + " : " + fromAccountId + " : " + toAccountId + "\n" + jsonData.ToString() + "\n");

            try
            {
                switch (notification)
                {
                    case GameSessionNotifications.NotificationTypes.Created:
                    //   case PlaterSessionNotifications.NotificationTypes.InvitationsCreated:
                    case GameSessionNotifications.NotificationTypes.ParamsChanged:
                        {
                            if (OnSessionUpdated != null)
                            {
                                Notification notificationData = new Notification();
                                notificationData.SetSessionUpdated(notification, this, paramFlags);
                                OnSessionUpdated(notificationData);
                            }
                        }
                        break;
                    case GameSessionNotifications.NotificationTypes.Deleted:
                        {
                            SessionDeleted();
                            if (OnSessionUpdated != null)
                            {
                                Notification notificationData = new Notification();
                                notificationData.SetSessionUpdated(notification, this, paramFlags);
                                OnSessionUpdated(notificationData);
                            }
                        }
                        break;
                    case GameSessionNotifications.NotificationTypes.PlayerLeft:
                    case GameSessionNotifications.NotificationTypes.SpectatorLeft:
                    case GameSessionNotifications.NotificationTypes.PlayerJoined:
                    case GameSessionNotifications.NotificationTypes.SpectatorJoined:
                    case GameSessionNotifications.NotificationTypes.SpectatorSwappedToPlayer:
                    case GameSessionNotifications.NotificationTypes.PlayerSwappedToSpectator:
                    case GameSessionNotifications.NotificationTypes.PlayerCustomDataUpdated:
                    case GameSessionNotifications.NotificationTypes.SpectatorCustomDataUpdated:
                        UpdateMembers(notification, jsonData.member);
                        break;
                }
            }
#pragma warning disable CS0168
            catch (Exception e)
#pragma warning restore CS0168
            {
#if DEBUG
                string output = "PlayerSession.SendUpdate : " + notification.ToString(); // + " : " + paramFlags + "\n";
                output += e.Message + "\n";
                output += e.StackTrace;

                Debug.LogError(output);
#endif
            }
        }

        private void EventHandler(WebApiNotifications.CallbackParams eventData, NotificationSessionData eventJsonData)
        {
            // Once here, the sessionId has already read from the eventData.
            // However each type might have more info required.

            if (SessionId != null && SessionId.Length > 0 && eventJsonData.sessionId != SessionId)
            {
#if DEBUG
                // Somehow data for a different session has ended up here
                Debug.LogError("Recieved event for session " + eventJsonData.sessionId + " however this session is " + SessionId);
#endif
            }

            UInt64 fromAccountId = 0;
            if (eventData.From != null)
            {
                fromAccountId = eventData.From.AccountId;
            }

            UInt64 toAccountId = 0;
            if (eventData.To != null)
            {
                toAccountId = eventData.To.AccountId;
            }

            GameSessionNotifications.EventHandlingConfig config = GameSessionNotifications.GetEventConfig(eventData.DataType);

            if (config != null)
            {
                SendUpdate(config.NotificationType, config.ParamFlags, eventJsonData, fromAccountId, toAccountId);
            }

            if (OnRawEvent != null)
            {
                OnRawEvent(this, eventData);
            }
            else
            {

            }
        }
    }

    // The following classes are filled in using JsonUtility.FromJson
    // Nothing directly assigns to the fields in the classes so the warning needs to be disabled.
    // warning CS0649: Field '?' is never assigned to, and will always have its default value 0

//#pragma warning disable 0649
//    // Not all paramters in this class will be filled out by the Json parser.
//    // Depending on the type of notification will reflect which data is set
//    [System.Serializable]
//    internal class NotificationGameSessionData
//    {
//        public string sessionId;

//        public override string ToString()
//        {
//            string output = "SessionId : " + sessionId;

//            return output;
//        }
//    }
//#pragma warning restore 0649

}
