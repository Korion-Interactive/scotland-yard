

using System;
using System.Collections.Generic;
using Unity.PSN.PS5.WebApi;
using UnityEngine;

namespace Unity.PSN.PS5.Sessions
{
    /// <summary>
    /// Default set of player session filters
    /// </summary>
    public class PlayerSessionFilters : WebApiFilters
    {
        public static readonly PlayerSessionFilters DefaultFilters = new PlayerSessionFilters();

        /// <summary>
        /// Create a standard set of player session filters
        /// </summary>
        public PlayerSessionFilters() : base(new string[]{ "psn:sessionManager:playerSession:created",
                                                         "psn:sessionManager:playerSession:deleted",
                                                         "psn:sessionManager:ps:m:players:created",
                                                         "psn:sessionManager:ps:m:players:deleted",
                                                         "psn:sessionManager:ps:m:spectators:created",
                                                         "psn:sessionManager:ps:m:spectators:deleted",
                                                         "psn:sessionManager:ps:leader:updated",
                                                         "psn:sessionManager:ps:m:players:swapped",
                                                         "psn:sessionManager:ps:m:spectators:swapped",
                                                         "psn:sessionManager:ps:maxPlayers:updated",
                                                         "psn:sessionManager:ps:maxSpectators:updated",
                                                         "psn:sessionManager:ps:swapSupported:updated",
                                                         "psn:sessionManager:ps:joinableSpecifiedUsers:created",
                                                         "psn:sessionManager:ps:joinableSpecifiedUsers:deleted",
                                                         "psn:sessionManager:ps:joinDisabled:updated",
                                                         "psn:sessionManager:ps:localizedSessionNames:updated",
                                                         "psn:sessionManager:ps:joinableUserType:updated",
                                                         "psn:sessionManager:ps:invitableUserType:updated",
                                                         "psn:sessionManager:ps:leaderPrivileges:updated",
                                                         "psn:sessionManager:ps:exclusiveLeaderPrivileges:updated"},
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
    /// Callbacks for player session updates
    /// </summary>
    public class PlayerSessionCallbacks
    {
        /// <summary>
        /// Update method called when an order-guaranteed push event is handled for a player session
        /// </summary>
        /// <remarks>
        /// Some of the notification will include Json data which is parsed when the notification is recieved.
        /// </remarks>
        public PlayerSession.SessionEventHandler OnSessionUpdated { get; set; } = null;

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
    /// Player session WebApi notification types
    /// </summary>
    public static class PlayerSessionNotifications
    {
        /// <summary>
        /// Types of player session notifications
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
            /// <summary> The list of specified user has been created </summary>
            JoinableSpecifiedUsersCreated,
            /// <summary> The list of specified user has been deleted </summary>
            JoinableSpecifiedUsersDeleted,
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
            public EventHandlingConfig(string dataType, NotificationTypes notificationType, PlayerSession.ParamTypes paramFlags, bool hasAdditionalJsonData, bool orderGuaranteed)
            {
                DataType = dataType;
                NotificationType = notificationType;
                ParamFlags = paramFlags;
                HasAdditionalJsonData = hasAdditionalJsonData;
                IsOrderGuaranteed = orderGuaranteed;
            }

            public string DataType { get; set; }

            public NotificationTypes NotificationType { get; set; }

            public PlayerSession.ParamTypes ParamFlags { get; set; }

            public bool HasAdditionalJsonData { get; set; }

            public bool IsOrderGuaranteed { get; set; }
        }

        // 26
        static internal EventHandlingConfig[] dataTypeToSessionEvents = new EventHandlingConfig[]
        {
            // Order-Guaranteed
            // These notification are recieved by the Push events assigned to the player session when it is created or joined.
            new EventHandlingConfig("psn:sessionManager:playerSession:created", NotificationTypes.Created, PlayerSession.ParamTypes.NotSet, false, true),
            new EventHandlingConfig("psn:sessionManager:playerSession:deleted", NotificationTypes.Deleted, PlayerSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:ps:maxPlayers:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.MaxPlayers, false, true),
            new EventHandlingConfig("psn:sessionManager:ps:maxSpectators:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.MaxSpectators, false, true),
            new EventHandlingConfig("psn:sessionManager:ps:joinDisabled:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.JoinDisabled, false, true),
            new EventHandlingConfig("psn:sessionManager:ps:localizedSessionNames:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.LocalizedSessionName, false, true),
            new EventHandlingConfig("psn:sessionManager:ps:joinableUserType:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.JoinableUserType, false, true),
            new EventHandlingConfig("psn:sessionManager:ps:invitableUserType:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.InvitableUserType, false, true),
            new EventHandlingConfig("psn:sessionManager:ps:leaderPrivileges:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.LeaderPrivileges, false, true),
            new EventHandlingConfig("psn:sessionManager:ps:exclusiveLeaderPrivileges:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.ExclusiveLeaderPrivileges, false, true),
            new EventHandlingConfig("psn:sessionManager:ps:leader:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.Leader, false, true),
            new EventHandlingConfig("psn:sessionManager:ps:joinableSpecifiedUsers:created", NotificationTypes.JoinableSpecifiedUsersCreated, PlayerSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:ps:joinableSpecifiedUsers:deleted", NotificationTypes.JoinableSpecifiedUsersDeleted, PlayerSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:ps:swapSupported:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.SwapSupported, false, true),
            new EventHandlingConfig("psn:sessionManager:ps:m:players:deleted", NotificationTypes.PlayerLeft, PlayerSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:ps:m:spectators:deleted", NotificationTypes.SpectatorLeft, PlayerSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:ps:m:players:created", NotificationTypes.PlayerJoined, PlayerSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:ps:m:spectators:created", NotificationTypes.SpectatorJoined, PlayerSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:ps:m:players:swapped", NotificationTypes.SpectatorSwappedToPlayer, PlayerSession.ParamTypes.NotSet, true, true),
            new EventHandlingConfig("psn:sessionManager:ps:m:spectators:swapped", NotificationTypes.PlayerSwappedToSpectator, PlayerSession.ParamTypes.NotSet, true, true),

            // Order Not Guaranteed
            // There notification are recieved by individual users, who may or may not have an associated player session.
            // Some of these notifications are related to sessions, like  "psn:sessionManager:ps:customData1:updated"
            new EventHandlingConfig("psn:sessionManager:ps:sessionMessage:created", NotificationTypes.SessionMessage, PlayerSession.ParamTypes.NotSet, true, false),
            new EventHandlingConfig("psn:sessionManager:ps:customData1:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.CustomData1, false, false),
            new EventHandlingConfig("psn:sessionManager:ps:customData2:updated", NotificationTypes.ParamsChanged, PlayerSession.ParamTypes.CustomData2, false, false),
            new EventHandlingConfig("psn:sessionManager:ps:m:p:customData1:updated", NotificationTypes.PlayerCustomDataUpdated, PlayerSession.ParamTypes.NotSet, true, false),
            new EventHandlingConfig("psn:sessionManager:ps:m:s:customData1:updated", NotificationTypes.SpectatorCustomDataUpdated, PlayerSession.ParamTypes.NotSet, true, false),
            new EventHandlingConfig("psn:sessionManager:ps:invitations:created", NotificationTypes.InvitationsCreated, PlayerSession.ParamTypes.NotSet, false, false),
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
    /// Player session instance
    /// </summary>
    public partial class PlayerSession
    {
        /// <summary>
        /// Notification from order-guaranteed push events
        /// </summary>
        public struct Notification
        {
            /// <summary>
            /// The type of notification for the session change
            /// </summary>
            public PlayerSessionNotifications.NotificationTypes NotificationType { get; internal set; }

            /// <summary>
            /// The session changed by the notification
            /// </summary>
            public PlayerSession Session { get; internal set; }

            /// <summary>
            /// The flag incidating the session parameter that has been changed
            /// </summary>
            public PlayerSession.ParamTypes SessionParamUpdates { get; internal set; }

            /// <summary>
            /// The session member being updated. Can be null if the notification isn't related to a member.
            /// </summary>
            public SessionMember Member { get; internal set; }

            /// <summary>
            /// The list of joinable users. Can be null if the notification isn't of type <see cref="PlayerSessionNotifications.NotificationTypes.JoinableSpecifiedUsersCreated"/> or <see cref="PlayerSessionNotifications.NotificationTypes.JoinableSpecifiedUsersDeleted"/>
            /// </summary>
            public NotificationJoinableUser[] JoinableUsers { get; internal set; }


            internal void SetJoinableList(PlayerSessionNotifications.NotificationTypes notification, PlayerSession session, NotificationJoinableUser[] joinableUsers)
            {
                NotificationType = notification;
                Session = session;
                SessionParamUpdates = PlayerSession.ParamTypes.JoinableSpecifiedUsers;
                Member = null;
                JoinableUsers = joinableUsers;
            }

            internal void SetSessionUpdated(PlayerSessionNotifications.NotificationTypes notification, PlayerSession session, PlayerSession.ParamTypes paramTypes)
            {
                NotificationType = notification;
                Session = session;
                SessionParamUpdates = paramTypes;
                Member = null;
                JoinableUsers = null;
            }

            internal void SetMemberUpdated(PlayerSessionNotifications.NotificationTypes notification, PlayerSession session, SessionMember member)
            {
                NotificationType = notification;
                Session = session;
                SessionParamUpdates = PlayerSession.ParamTypes.NotSet;
                Member = member;
                JoinableUsers = null;
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
            PlayerSession session = null;

            NotificationSessionData eventJsonData = null;

            // All eventData callback params should have a Data sestion which contains the session Id
            if (eventData.Data != null)
            {
                eventJsonData = JsonUtility.FromJson<NotificationSessionData>(eventData.Data);

                if (eventJsonData != null && eventJsonData.sessionId != null)
                {
                    // If this is a creation event use the creation method to either create or find the session
                    // Timing issue can occur if the notification is recieved before the creation request has completed.
                    if (eventData.DataType == "psn:sessionManager:playerSession:created")
                    {
                        bool isNew = false;
                        session = SessionsManager.CreatePlayerSession(eventJsonData.sessionId, out isNew);
                    }
                    else
                    {
                        session = SessionsManager.FindPlayerSessionFromSessionId(eventJsonData.sessionId);
                    }

                    if (session == null)
                    {
                        // Can't find a session. This should not happen as there should never been an event arrive for a session that doesn't exist
                        // The session may have been deleted and events that are arriving are for old session.
                        // This situation needs to be handled.
                        // Check to see if this session id was ever deleted
                        if (SessionsManager.IsPlayerSessionDeleted(eventJsonData.sessionId) == false)
                        {
#if DEBUG
                            Debug.LogError("Player Session event : " + eventData.DataType + " can't find session id " + eventJsonData.sessionId);
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

        private SessionMember UpdateFrom(PlayerSessionNotifications.NotificationTypes notification, NotificationMember memberUpdate, bool isSpectator)
        {
            if (memberUpdate.accountId == 0) return null;

            // memberUpdate might be a delete, create, swapped or customedata update
            // Look for existing session member
            bool inSpectatorList;
            SessionPlatforms memberPlatform = PlayerSession.FromString(memberUpdate.platform);
            SessionMember sessionMember = FindFromAccountId(new SessionMemberIdentifier (memberUpdate.accountId, memberPlatform), out inSpectatorList);

            if (notification == PlayerSessionNotifications.NotificationTypes.PlayerJoined || notification == PlayerSessionNotifications.NotificationTypes.SpectatorJoined)
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
            else if (notification == PlayerSessionNotifications.NotificationTypes.PlayerLeft || notification == PlayerSessionNotifications.NotificationTypes.SpectatorLeft)
            {
                if (sessionMember != null)
                {
                    sessionMember.UpdateFrom(memberUpdate, isSpectator);

                    RemoveMember(new SessionMemberIdentifier(memberUpdate.accountId, memberPlatform));
                }
            }
            else if (notification == PlayerSessionNotifications.NotificationTypes.PlayerSwappedToSpectator || notification == PlayerSessionNotifications.NotificationTypes.SpectatorSwappedToPlayer)
            {
                if (sessionMember != null)
                {
                    sessionMember.UpdateFrom(memberUpdate, isSpectator);
                    MoveMember(sessionMember);  // Called after IsSpectator flag updated. This will ensure it goes into the correct list
                }
            }
            else if (notification == PlayerSessionNotifications.NotificationTypes.PlayerCustomDataUpdated || notification == PlayerSessionNotifications.NotificationTypes.SpectatorCustomDataUpdated)
            {

            }

            return sessionMember;
        }

        private void UpdateMembers(PlayerSessionNotifications.NotificationTypes notification, NotificationMembers notificationsMember)
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

        internal void SendUpdate(PlayerSessionNotifications.NotificationTypes notification, ParamTypes paramFlags, NotificationSessionData jsonData, UInt64 fromAccountId = 0, UInt64 toAccountId = 0)
        {
            //  Debug.LogError("Event SendUpdate : " + notification + " : " + paramFlags + " : " + fromAccountId + " : " + toAccountId + "\n" + jsonData.ToString() + "\n");

            try
            {
                switch (notification)
                {
                    case PlayerSessionNotifications.NotificationTypes.Created:
                 //   case PlaterSessionNotifications.NotificationTypes.InvitationsCreated:
                    case PlayerSessionNotifications.NotificationTypes.ParamsChanged:
                        {
                            if (OnSessionUpdated != null)
                            {
                                Notification notificationData = new Notification();
                                notificationData.SetSessionUpdated(notification, this, paramFlags);
                                OnSessionUpdated(notificationData);
                            }
                        }
                        break;
                    case PlayerSessionNotifications.NotificationTypes.Deleted:
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
                    case PlayerSessionNotifications.NotificationTypes.PlayerLeft:
                    case PlayerSessionNotifications.NotificationTypes.SpectatorLeft:
                    case PlayerSessionNotifications.NotificationTypes.PlayerJoined:
                    case PlayerSessionNotifications.NotificationTypes.SpectatorJoined:
                    case PlayerSessionNotifications.NotificationTypes.SpectatorSwappedToPlayer:
                    case PlayerSessionNotifications.NotificationTypes.PlayerSwappedToSpectator:
                    case PlayerSessionNotifications.NotificationTypes.PlayerCustomDataUpdated:
                    case PlayerSessionNotifications.NotificationTypes.SpectatorCustomDataUpdated:
                        UpdateMembers(notification, jsonData.member);
                        break;
                    case PlayerSessionNotifications.NotificationTypes.JoinableSpecifiedUsersCreated:
                    case PlayerSessionNotifications.NotificationTypes.JoinableSpecifiedUsersDeleted:
                        {
                            if (OnSessionUpdated != null)
                            {
                                Notification notificationData = new Notification();
                                notificationData.SetJoinableList(notification, this, jsonData.joinableSpecifiedUsers);
                                OnSessionUpdated(notificationData);
                            }
                        }
                        break;
                   // case PlaterSessionNotifications.NotificationTypes.SessionMessage:
                   ////     UpdateMessage(notification, fromAccountId, toAccountId, jsonData.sessionMessage.payload);
                   //     break;
                }
            }
#pragma warning disable CS0168
            catch (Exception e)
#pragma warning restore CS0168
            {
#if DEBUG
                string output = "PlayerSession.SendUpdate : " + notification.ToString() + " : " + paramFlags + "\n";
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

            PlayerSessionNotifications.EventHandlingConfig config = PlayerSessionNotifications.GetEventConfig(eventData.DataType);

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

#pragma warning disable 0649
    // Not all paramters in this class will be filled out by the Json parser.
    // Depending on the type of notification will reflect which data is set
    [System.Serializable]
    internal class NotificationSessionData
    {
        public string sessionId;
        public NotificationCustomProperties customProperties;
        public NotificationJoinableUser[] joinableSpecifiedUsers;
        public NotificationMembers member;
        public NotificationSessionMessagePayload sessionMessage;

        public override string ToString()
        {
            string output = "SessionId : " + sessionId;

            if (customProperties != null)
            {
                string cpStr = customProperties.ToString();

                if (cpStr != null && cpStr.Length > 0)
                {
                    output += "\n customProperties : " + cpStr;
                }
            }

            if (joinableSpecifiedUsers != null)
            {
                if (joinableSpecifiedUsers != null && joinableSpecifiedUsers.Length > 0)
                {
                    output += "\n joinableSpecifiedUsers";
                    for (int i = 0; i < joinableSpecifiedUsers.Length; i++)
                    {
                        output += "\n   " + joinableSpecifiedUsers[i].ToString();
                    }
                }
            }

            if (member != null && (member.players != null || member.spectators != null))
            {
                output += "\n " + member.ToString();
            }

            if (sessionMessage != null && sessionMessage.payload != null && sessionMessage.payload.Length > 0)
            {
                output += "\n " + sessionMessage.ToString();
            }

            return output;
        }
    }

    [System.Serializable]
    class NotificationSessionMessagePayload
    {
        public string payload;

        public override string ToString()
        {
            string output = "payload : " + payload;
            return output;
        }
    }

    [System.Serializable]
    class NotificationCustomProperties
    {
        public string deletedEventCause;
        public string leftEventCause;
        public string joinTimestamp;

        public override string ToString()
        {
            string output = "";
            if (deletedEventCause != null)
            {
                output += "deletedEventCause : " + deletedEventCause;
            }

            if (leftEventCause != null)
            {
                output += "leftEventCause : " + leftEventCause;
            }

            if (joinTimestamp != null)
            {
                output += "joinTimestamp : " + joinTimestamp;
            }

            return output;
        }
    }

    /// <summary>
    /// Joinable user notification data, containing account id and online id
    /// </summary>
    [System.Serializable]
    public class NotificationJoinableUser
    {
        /// <summary>
        /// Account id of the joinable user
        /// </summary>
        public UInt64 accountId;

        /// <summary>
        /// The online id of the joinable user
        /// </summary>
        public string onlineId;

        /// <summary>
        /// Generate string with account id and online id
        /// </summary>
        /// <returns>Generated string</returns>
        public override string ToString()
        {
            return "accountId : " + accountId + " onlineId : " + onlineId;
        }
    }

    [System.Serializable]
    class NotificationMembers
    {
        public NotificationMember[] players;
        public NotificationMember[] spectators;

        public override string ToString()
        {
            string output = "member";

            if (players == null || players.Length == 0)
            {
                output += "\nplayers : None";
            }
            else
            {
                output += "\nplayers";
                for (int i = 0; i < players.Length; i++)
                {
                    output += "\n" + players[i].ToString();
                }
            }

            if (spectators == null || spectators.Length == 0)
            {
                output += "\nspectators : None";
            }
            else
            {
                output += "\nspectators";
                for (int i = 0; i < spectators.Length; i++)
                {
                    output += "\n" + spectators[i].ToString();
                }
            }

            return output;
        }
    }

    [System.Serializable]
    class NotificationMember
    {
        public UInt64 accountId;
        public string onlineId;
        public string platform;
        public NotificationCustomProperties customProperties;

        public override string ToString()
        {
            string output = "accountId : " + accountId + " onlineId : " + onlineId + " platform : " + platform;

            output += "\n" + customProperties.ToString();

            return output;
        }
    }
#pragma warning restore 0649

    /// <summary>
    /// Test parsing json into NotificationSessionData type
    /// </summary>
    public class TestJSONParsing
    {
        /// <summary>
        /// Log a set of different json strings to NotificationSessionData type
        /// </summary>
        public static void TestJSONNotifciations()
        {
#if DEBUG
            try
            {
                List<string> testJson = new List<string>();

                testJson.Add("{ \"sessionId\": \"1234567890AABCDEFG\" }");
                testJson.Add("{ \"sessionId\": \"1234567890AABCDEFG\",  \"customProperties\":  { \"deletedEventCause\": \"LAST_PLAYER_LEFT\" } }");
                testJson.Add("{ \"sessionId\": \"1234567890AABCDEFG\", \"joinableSpecifiedUsers\": [ { \"accountId\": 987654321, \"onlineId\": \"myname\" } ] }");
                testJson.Add("{ \"sessionId\": \"1234567890AABCDEFG\", \"member\": { \"players\": [ { \"accountId\": 987654321, \"onlineId\": \"myname\", \"platform\": \"PROSPERO\", \"customProperties\": { \"leftEventCause\": \"MEMBER_LEFT\" } } ] } }");
                testJson.Add("{ \"sessionId\": \"1234567890AABCDEFG\", \"member\": { \"spectators\": [ { \"accountId\": 987654321, \"onlineId\": \"myname\", \"platform\": \"PROSPERO\", \"customProperties\": { \"leftEventCause\": \"MEMBER_LEFT\" } } ] } }");
                testJson.Add("{ \"sessionId\": \"1234567890AABCDEFG\", \"member\": { \"players\": [ { \"accountId\": 987654321, \"onlineId\": \"myname\", \"platform\": \"PROSPERO\", \"customProperties\": { \"joinTimestamp\": \"123\" } } ] } }");
                testJson.Add("{ \"sessionId\": \"1234567890AABCDEFG\", \"member\": { \"spectators\": [ { \"accountId\": 987654321, \"onlineId\": \"myname\", \"platform\": \"PROSPERO\", \"customProperties\": { \"joinTimestamp\": \"123\" } } ] } }");
                testJson.Add("{ \"sessionId\": \"1234567890AABCDEFG\", \"member\": { \"spectators\": [ { \"accountId\": 987654321, \"onlineId\": \"myname\", \"platform\": \"PROSPERO\" } ] } }");
                testJson.Add("{ \"sessionId\": \"1234567890AABCDEFG\", \"sessionMessage\": { \"payload\": \"This is a session message\" } }");
                testJson.Add("{\"sessionId\":\"a64d96f3 - 39fb - 4b21 - 9d75 - 9b3722995979\",\"sessionMessage\":{\"payload\":\"This is a test message 103307\"}}");
                testJson.Add("{ \"sessionId\": \"1234567890AABCDEFG\", \"member\": { \"spectators\": [ { \"accountId\": 987654321, \"onlineId\": \"myname\", \"platform\": \"PROSPERO\", \"customProperties\": { \"joinTimestamp\": \"123\" } } ] } }");

                for (int i = 0; i < testJson.Count; i++)
                {
                    var dataA = JsonUtility.FromJson<NotificationSessionData>(testJson[i]);
                    Debug.Log("   Json = " + testJson[i] + "\n     " + dataA.ToString());
                }
            }
            catch (Exception)
            {

            }
#endif
        }
    }
}

// WebAPI Notifications   https://p.siedev.net/resources/documents/WebAPI/1/Session_Manager_WebAPI-Reference/0026.html

// Summary List of all notification as they mostly share the same data structures

// Player and Spectator notifications are usaully the same json format apart from the list being called "players" or "spectators"

// Notifcation have the same header. All of them have a 'data' json section which most notifications share the same structure but
// some of them are different.

// *******  Notifciations header info  ******
//
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient


// ******* Notifcation grouped by data structure

// psn:sessionManager:playerSession:created
// psn:sessionManager:ps:maxPlayers:updated
// psn:sessionManager:ps:maxSpectators:updated
// psn:sessionManager:ps:joinDisabled:updated
// psn:sessionManager:ps:localizedSessionNames:updated
// psn:sessionManager:ps:joinableUserType:updated
// psn:sessionManager:ps:invitableUserType:updated
// psn:sessionManager:ps:leaderPrivileges:updated
// psn:sessionManager:ps:exclusiveLeaderPrivileges:updated
// psn:sessionManager:ps:customData1:updated
// psn:sessionManager:ps:customData2:updated
// psn:sessionManager:ps:leader:updated
// psn:sessionManager:ps:swapSupported:updated
// psn:sessionManager:ps:invitations:created
//    data:
//         {
//             "sessionId": "string"      // session id
//         }

// psn:sessionManager:playerSession:deleted

//    data:
//          {
//              "sessionId": "string",                            Session ID of the deleted
//              "customProperties":
//              {
//                  "deletedEventCause": "LAST_PLAYER_LEFT"      // Reason for deletion - Only possible value "LAST_PLAYER_LEFT"
//              }
//          }

// psn:sessionManager:ps:joinableSpecifiedUsers:created
// psn:sessionManager:ps:joinableSpecifiedUsers:deleted
//    data:
//          {
//              "sessionId": "string",              // Session ID of the Player Session for which users were added to joinableSpecifiedUsers
//              "joinableSpecifiedUsers":
//              [
//                  {
//                      "accountId": 0,             // Account ID of the user who was registered
//                      "onlineId": "string"        // Online ID of the user who was registered
//                  }
//              ]
//          }
//

// psn:sessionManager:ps:m:players:deleted
// psn:sessionManager:ps:m:spectators:deleted
//    data:
//         {
//              "sessionId": "string",                              // Session ID of the Player Session from which the player left
//              "member":
//              {
//                  "players":                      (or)  "spectators":
//                  [
//                      {
//                          "accountId": 0,                         // Account ID of the player who left
//                          "onlineId": "string",                   // Online ID of the player who left
//                          "platform": "PROSPERO",                 // Platform of the player who left "prospero" or "ps4"
//                          "customProperties":
//                          {
//                              "leftEventCause": "MEMBER_LEFT"     // Reason for leaving (MEMBER_LEFT: The member, him or herself, left, KICKED_OUT: Another member made the member leave)
//                          }
//                      }
//                  ]
//              }
//          }
//

// psn:sessionManager:ps:m:players:created
// psn:sessionManager:ps:m:spectators:created
// psn:sessionManager:ps:m:players:swapped
// psn:sessionManager:ps:m:spectators:swapped
//    data:
//         {
//              "sessionId": "string",                              // Session ID of the Player Session that the player joined
//              "member":
//              {
//                  "players":                     (or)  "spectators":
//                  [
//                      {
//                          "accountId": 0,                         // Account ID of the player who left
//                          "onlineId": "string",                   // Online ID of the player who left
//                          "platform": "PROSPERO",                 // Platform of the player who left "prospero" or "ps4"
//                          "customProperties":
//                          {
//                              "joinTimestamp": "0"     // int64 // Date and time of joining as a member (Unix Timestamp) [ms]
//                          }
//                      }
//                  ]
//              }
//          }
//

// psn:sessionManager:ps:m:p:customData1:updated
// psn:sessionManager:ps:m:s:customData1:updated
//    data:
//         {
//              "sessionId": "string",                  // Session ID of the Player Session for which customData1 was updated
//              "member":
//              {
//                  "players":              (or)  "spectators":
//                  [
//                      {
//                          "accountId": 0,             // Account ID of the player for whom customData1 was updated
//                          "onlineId": "string",       // Online ID of the player for whom customData1 was updated
//                          "platform": "PROSPERO"      // Platform of the player for whom customData1 was updated "prospero" or "ps4"
//                      }
//                  ]
//              }
//         }
//

// psn:sessionManager:ps:sessionMessage:created
//    data:
//         {
//              "sessionId": "string",               // Session ID of the Player Session in which the session message was sent
//              "sessionMessage":
//              {
//                  "payload": "string"              // Payload of the sent session message
//              }
//         }
//

// Detailed list of all notifications.

// Anything in the "data" will be a json block containing some data
// e.g
//
//   {
//      "sessionId": "string"
//   }

// psn:sessionManager:playerSession:created
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // session id
//         }
//

// psn:sessionManager:playerSession:deleted
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//          {
//              "sessionId": "string",                            Session ID of the deleted
//              "customProperties":
//              {
//                  "deletedEventCause": "LAST_PLAYER_LEFT"      // Reason for deletion - Only possible value "LAST_PLAYER_LEFT"
//              }
//          }
//

// psn:sessionManager:ps:maxPlayers:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which maxPlayers was updated
//         }
//

// psn:sessionManager:ps:maxSpectators:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which maxSpectators was updated
//         }
//

// psn:sessionManager:ps:joinDisabled:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which joinDisabled was updated
//         }
//

// psn:sessionManager:ps:localizedSessionNames:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which localizedSessionName was updated
//         }
//

// psn:sessionManager:ps:joinableUserType:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which joinableUserType was updated
//         }
//

// psn:sessionManager:ps:invitableUserType:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which invitableUserType was updated
//         }
//

// psn:sessionManager:ps:leaderPrivileges:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which leaderPrivileges was updated
//         }
//

// psn:sessionManager:ps:exclusiveLeaderPrivileges:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which exclusiveLeaderPrivileges was updated
//         }
//

// psn:sessionManager:ps:customData1:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which customData1 was updated
//         }
//

// psn:sessionManager:ps:customData2:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which customData2 was updated
//         }
//

// psn:sessionManager:ps:leader:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which leader was updated
//         }
//

// psn:sessionManager:ps:joinableSpecifiedUsers:created
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//          {
//              "sessionId": "string",              // Session ID of the Player Session for which users were added to joinableSpecifiedUsers
//              "joinableSpecifiedUsers":
//              [
//                  {
//                      "accountId": 0,             // Account ID of the user who was registered
//                      "onlineId": "string"        // Online ID of the user who was registered
//                  }
//              ]
//          }
//

// psn:sessionManager:ps:joinableSpecifiedUsers:deleted
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//          {
//              "sessionId": "string",              // Session ID of the Player Session for which users were deleted from joinableSpecifiedUsers
//              "joinableSpecifiedUsers":
//              [
//                  {
//                      "accountId": 0,             // Account ID of the user who was deleted
//                      "onlineId": "string"        // Online ID of the user who was deleted
//                  }
//              ]
//          }
//

// psn:sessionManager:ps:swapSupported:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which swapSupported was updated
//         }
//


// psn:sessionManager:ps:m:players:deleted
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//              "sessionId": "string",                              // Session ID of the Player Session from which the player left
//              "member":
//              {
//                  "players":                      <---- Players
//                  [
//                      {
//                          "accountId": 0,                         // Account ID of the player who left
//                          "onlineId": "string",                   // Online ID of the player who left
//                          "platform": "PROSPERO",                 // Platform of the player who left "prospero" or "ps4"
//                          "customProperties":
//                          {
//                              "leftEventCause": "MEMBER_LEFT"     // Reason for leaving (MEMBER_LEFT: The member, him or herself, left, KICKED_OUT: Another member made the member leave)
//                          }
//                      }
//                  ]
//              }
//          }
//

// psn:sessionManager:ps:m:spectators:deleted
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//              "sessionId": "string",                              // Session ID of the Player Session from which the spectator left
//              "member":
//              {
//                  "spectators":                   <---- Spectators
//                  [
//                      {
//                          "accountId": 0,                         // Account ID of the spectator who left
//                          "onlineId": "string",                   // Online ID of the spectator who left
//                          "platform": "PROSPERO",                 // Platform of the spectator who left
//                          "customProperties":
//                          {
//                              "leftEventCause": "MEMBER_LEFT"     // Reason for leaving (MEMBER_LEFT: The member, him or herself, left, KICKED_OUT: Another member made the member leave)
//                          }
//                      }
//                  ]
//              }
//          }
//

// psn:sessionManager:ps:m:players:created
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//              "sessionId": "string",                              // Session ID of the Player Session that the player joined
//              "member":
//              {
//                  "players":                      <---- Players
//                  [
//                      {
//                          "accountId": 0,                         // Account ID of the player who left
//                          "onlineId": "string",                   // Online ID of the player who left
//                          "platform": "PROSPERO",                 // Platform of the player who left "prospero" or "ps4"
//                          "customProperties":
//                          {
//                              "joinTimestamp": "0"     // int64 // Date and time of joining as a member (Unix Timestamp) [ms]
//                          }
//                      }
//                  ]
//              }
//          }
//

// psn:sessionManager:ps:m:spectators:created
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//              "sessionId": "string",                              // Session ID of the Player Session from which the spectator left
//              "member":
//              {
//                  "spectators":                   <---- Spectators
//                  [
//                      {
//                          "accountId": 0,                         // Account ID of the spectator who left
//                          "onlineId": "string",                   // Online ID of the spectator who left
//                          "platform": "PROSPERO",                 // Platform of the spectator who left
//                          "customProperties":
//                          {
//                              "joinTimestamp": 0     // int64 // Date and time of joining as a member (Unix Timestamp) [ms]
//                          }
//                      }
//                  ]
//              }
//          }
//

// psn:sessionManager:ps:m:players:swapped
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//              "sessionId": "string",                              // Session ID of the Player Session in which a member performed a swap
//              "member":
//              {
//                  "players":                      <---- Players
//                  [
//                      {
//                          "accountId": 0,                         // Account ID of the member who swapped to become a player
//                          "onlineId": "string",                   // Online ID of the member who swapped to become a player
//                          "platform": "PROSPERO",                 // Platform of the member who swapped to become a player "prospero" or "ps4"
//                          "customProperties":
//                          {
//                              "joinTimestamp": "0"     // int64 // Date and time when the member performed the swap (Unix Timestamp) [ms]
//                          }
//                      }
//                  ]
//              }
//          }
//

// psn:sessionManager:ps:m:spectators:swapped
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//              "sessionId": "string",                              // Session ID of the Player Session in which a member performed a swap
//              "member":
//              {
//                  "spectators":                   <---- Spectators
//                  [
//                      {
//                          "accountId": 0,                         // Account ID of the member who swapped to become a spectator
//                          "onlineId": "string",                   // Online ID of the member who swapped to become a spectator
//                          "platform": "PROSPERO",                 // Platform of the member who swapped to become a spectator "prospero" or "ps4"
//                          "customProperties":
//                          {
//                              "joinTimestamp": 0     // int64 // Date and time when the member performed the swap (Unix Timestamp) [ms]
//                          }
//                      }
//                  ]
//              }
//          }
//

// psn:sessionManager:ps:m:p:customData1:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//              "sessionId": "string",                  // Session ID of the Player Session for which customData1 was updated
//              "member":
//              {
//                  "players":
//                  [
//                      {
//                          "accountId": 0,             // Account ID of the player for whom customData1 was updated
//                          "onlineId": "string",       // Online ID of the player for whom customData1 was updated
//                          "platform": "PROSPERO"      // Platform of the player for whom customData1 was updated "prospero" or "ps4"
//                      }
//                  ]
//              }
//         }
//

// psn:sessionManager:ps:m:s:customData1:updated
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//              "sessionId": "string",                  // Session ID of the Player Session for which customData1 was updated
//              "member":
//              {
//                  "spectators":
//                  [
//                      {
//                          "accountId": 0,             // Account ID of the spectator for whom customData1 was updated
//                          "onlineId": "string",       // Online ID of the spectator for whom customData1 was updated
//                          "platform": "PROSPERO"      // Platform of the spectator for whom customData1 was updated "prospero" or "ps4"
//                      }
//                  ]
//              }
//         }
//

// psn:sessionManager:ps:sessionMessage:created
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//              "sessionId": "string",               // Session ID of the Player Session in which the session message was sent
//              "sessionMessage":
//              {
//                  "payload": "string"              // Payload of the sent session message
//              }
//         }
//

// psn:sessionManager:ps:invitations:created
//    from:
//          accountId    Account ID of the user who executed the request
//          onlineId     Online ID of the user who executed the request
//          platform     The platform of the user who executed the API "prospero" or "ps4"
//    to:
//          accountId    Account ID of the recipient
//          onlineId     Online ID of the recipient
//    data:
//         {
//             "sessionId": "string"      // Session ID of the Player Session for which the invitation was sent
//         }
//
