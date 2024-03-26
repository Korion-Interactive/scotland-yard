using System;
using System.IO;
using System.Collections.Generic;

using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;
using UnityEngine;
using Unity.PSN.PS5.WebApi;
using Unity.PSN.PS5.Matchmaking;

namespace Unity.PSN.PS5.Sessions
{
    /// <summary>
    /// Default set of user session filters
    /// </summary>
    public class UserSessionFilters : WebApiFilters
    {
        /// <summary>
        /// Crreate and initialise a set of user based player session and game session filters.
        /// </summary>
        public UserSessionFilters() : base(new string[]{ "psn:sessionManager:ps:invitations:created",
                                                         "psn:sessionManager:ps:m:p:customData1:updated",
                                                         "psn:sessionManager:ps:m:s:customData1:updated",
                                                         "psn:sessionManager:ps:customData1:updated",
                                                         "psn:sessionManager:ps:customData2:updated",
                                                         "psn:sessionManager:ps:sessionMessage:created",

                                                         "psn:sessionManager:gs:invitations:created",
                                                         "psn:sessionManager:gs:m:p:customData1:updated",
                                                         "psn:sessionManager:gs:m:s:customData1:updated",
                                                         "psn:sessionManager:gs:customData1:updated",
                                                         "psn:sessionManager:gs:customData2:updated",
                                                         "psn:sessionManager:gs:sessionMessage:created",

                                                         "psn:matchmaking:ticket:submitted",
                                                         "psn:matchmaking:ticket:canceled",
                                                         "psn:matchmaking:ticket:timedOut",
                                                         "psn:matchmaking:ticket:failed",
                                                         "psn:matchmaking:offer:failed"},
                                                         "sessionManager", 0)
        {
            /* Player session
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
    }

    /// <summary>
    /// Provides methods to handle session management.
    /// </summary>
    public class SessionsManager
    {
        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("SessionManager");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal Session queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// The user session filters set for each local user when a user push event is created. Defualts to <see cref="UserSessionFilters"/>
        /// </summary>
        /// <remarks>
        /// Default filters are:
        ///    "psn:sessionManager:ps:invitations:created",
        ///    "psn:sessionManager:gs:invitations:created",
        ///    "psn:sessionManager:gs:sessionMessage:created"
        /// </remarks>
        static public WebApiFilters UserSessionFilters { get; set; } = new UserSessionFilters();

        /// <summary>
        /// Get the <see cref="WebApiPushEvent"/> for a user if one has been created
        /// </summary>
        /// <param name="userId">The local userId</param>
        /// <returns>The push event object</returns>
        static public WebApiPushEvent GetUserSessionPushEvent(int userId)
        {
            WebApiPushEvent userEvent;

            lock (UserEventSyncObj)
            {
                if (userSessionPushEvents.TryGetValue(userId, out userEvent) == false)
                {
                    return null;
                }
            }

            return userEvent;
        }

        static object UserEventSyncObj = new object();

        /// <summary>
        /// Has a push event been registered for the given local user
        /// </summary>
        /// <param name="userId">The local userId</param>
        /// <returns>Returns true if the push event is registered</returns>
        static public bool IsUserRegistered(int userId)
        {
            WebApiPushEvent userEvent = GetUserSessionPushEvent(userId);

            if (userEvent == null) return false;

            return userEvent.IsRegistered;
        }

        /// <summary>
        /// Register a userId with the session system so it creates a <see cref="WebApiPushEvent"/> using the <see cref="UserSessionFilters"/> filters.
        /// </summary>
        /// <param name="userId">The local user id to register</param>
        /// <remarks>
        /// This creates a <see cref="WebApiNotifications.RegisterPushEventRequest"/> request which is added to the <see cref="WebApiNotifications"/> queue.
        /// The user won't be fully registered until the request has been processed. Use <see cref="IsUserRegistered"/> to check if the user registration is complete
        /// </remarks>
        static public AsyncAction<AsyncRequest<WebApiNotifications.RegisterPushEventRequest>> RegisterUserSessionEvent(int userId)
        {
            WebApiPushEvent userEvent = GetUserSessionPushEvent(userId);

            if (userEvent != null)
            {
                // userEvent exists and is part of the internal dictionary.
                // If should be registered at this point, but registration or unregistration might be
                // pending in the queue.
                // Don't allow the user to be registered when it might be in either of these states
#if DEBUG
                //Debug.LogError("User already registered");
#endif

                return null;
            }

            WebApiPushEvent pushEvent = new WebApiPushEvent();

            pushEvent.UserId = userId;
            pushEvent.Filters = UserSessionFilters;
            pushEvent.OrderGuaranteed = false;

            WebApiNotifications.RegisterPushEventRequest request = new WebApiNotifications.RegisterPushEventRequest()
            {
                PushEvent = pushEvent,
                Callback = UserWebApiEventHandler,
            };

            var requestOp = new AsyncRequest<WebApiNotifications.RegisterPushEventRequest>(request).ContinueWith((antecedent) =>
            {
                if (antecedent == null)
                {
                    return;
                }

                if (antecedent.Request == null)
                {
                    return;
                }

                if (antecedent.Request.Result.apiResult == APIResultTypes.Success)
                {
                    lock (UserEventSyncObj)
                    {
                        if (userSessionPushEvents.ContainsKey(antecedent.Request.PushEvent.UserId) == false)
                        {
                            userSessionPushEvents.Add(antecedent.Request.PushEvent.UserId, antecedent.Request.PushEvent);
                        }
                    }
                }
                else
                {
#if DEBUG
                   // Debug.LogError("RegisterUserSessionEvent failed. : " + antecedent.Request.Result.ErrorMessage() );
#endif
                }
            });

            WebApiNotifications.Schedule(requestOp);

            return requestOp;
        }

        /// <summary>
        /// Unregister the user <see cref="WebApiPushEvent"/> from the system.
        /// </summary>
        /// <param name="userId">The local user id</param>
        static public void UnregisterUserSessionEvent(int userId)
        {
            WebApiPushEvent userEvent = GetUserSessionPushEvent(userId);

            if (userEvent == null) return;

            WebApiPushEvent pushEvent = new WebApiPushEvent();

            pushEvent.UserId = userId;
            pushEvent.Filters = UserSessionFilters;
            pushEvent.OrderGuaranteed = false;

            WebApiNotifications.UnregisterPushEventRequest request = new WebApiNotifications.UnregisterPushEventRequest()
            {
                PushEvent = pushEvent,
            };

            var requestOp = new AsyncRequest<WebApiNotifications.UnregisterPushEventRequest>(request).ContinueWith((antecedent) =>
            {
                if (antecedent == null)
                {
                    return;
                }

                if (antecedent.Request == null)
                {
                    return;
                }

                if (antecedent.Request.Result.apiResult == APIResultTypes.Success)
                {
                    lock (UserEventSyncObj)
                    {
                        userSessionPushEvents.Remove(antecedent.Request.PushEvent.UserId);
                    }
                }
            });

            WebApiNotifications.Schedule(requestOp);
        }

        /// <summary>
        /// Delegate for notifications about user events
        /// </summary>
        public delegate void RawUserSessionEventHandler(WebApiNotifications.CallbackParams eventData);

        /// <summary>
        /// Set an external event callback to provide additional process after player session has processed any WebApi events
        /// </summary>
        public static RawUserSessionEventHandler OnRawUserEvent;

        /// <summary>
        /// Delegate for notifications for session events.
        /// </summary>
        public delegate void UserEventHandler(Notification notificationData);

        /// <summary>
        /// The session notification callback. Note that this type of update doesn't contain any data
        /// to the locally cached data doesn't reflect the change. This only notifies what type of data changed.
        /// Use <see cref="PlayerSessionRequests.GetPlayerSessionsRequest"/> to retieve the latest data about the session.
        /// </summary>
        public static UserEventHandler OnUserEvent { get; set; }


        /// <summary>
        /// Notification from user push events
        /// </summary>
        public abstract class Notification
        {
            /// <summary>
            /// The session id related to the notification
            /// </summary>
            public string SessionId { get; internal set; }

            /// <summary>
            /// The originating user of a Push event
            /// </summary>
            public UInt64 FromAccountId { get; internal set; }

            /// <summary>
            /// The user who is being notified of a Push event
            /// </summary>
            public UInt64 ToAccountId { get; internal set; }

            /// <summary>
            /// Contains message payload if the notification type is <see cref="PlayerSessionNotifications.NotificationTypes.SessionMessage"/>
            /// </summary>
            public string MessagePayload { get; internal set; }
        }

        /// <summary>
        /// Player notification from user push events
        /// </summary>
        public class PlayerNotification : Notification
        {
            /// <summary>
            /// The type of player notification for the session change
            /// </summary>
            public PlayerSessionNotifications.NotificationTypes NotificationType { get; internal set; }

            internal void SetMessage(PlayerSessionNotifications.NotificationTypes notification, string sessionId, UInt64 fromAccountId, UInt64 toAccountId, string payload)
            {
                NotificationType = notification;
                SessionId = sessionId;
                FromAccountId = fromAccountId;
                ToAccountId = toAccountId;
                MessagePayload = payload;
            }

            internal void SetInvitation(PlayerSessionNotifications.NotificationTypes notification, string sessionId, UInt64 fromAccountId, UInt64 toAccountId)
            {
                NotificationType = notification;
                SessionId = sessionId;
                FromAccountId = fromAccountId;
                ToAccountId = toAccountId;
                MessagePayload = null;
            }
        }

        /// <summary>
        /// Game notification from user push events
        /// </summary>
        public class GameNotification : Notification
        {
            /// <summary>
            /// The type of game notification for the session change
            /// </summary>
            public GameSessionNotifications.NotificationTypes NotificationType { get; internal set; }

            internal void SetMessage(GameSessionNotifications.NotificationTypes notification, string sessionId, UInt64 fromAccountId, UInt64 toAccountId, string payload)
            {
                NotificationType = notification;
                SessionId = sessionId;
                FromAccountId = fromAccountId;
                ToAccountId = toAccountId;
                MessagePayload = payload;
            }

            internal void SetInvitation(GameSessionNotifications.NotificationTypes notification, string sessionId, UInt64 fromAccountId, UInt64 toAccountId)
            {
                NotificationType = notification;
                SessionId = sessionId;
                FromAccountId = fromAccountId;
                ToAccountId = toAccountId;
                MessagePayload = null;
            }
        }

        static internal void SendUpdate(PlayerSessionNotifications.NotificationTypes notification, string sessionId, PlayerSession.ParamTypes paramFlags, NotificationSessionData jsonData, UInt64 fromAccountId = 0, UInt64 toAccountId = 0)
        {
            PlayerSession session = SessionsManager.FindSessionFromSessionId(sessionId) as PlayerSession;

            try
            {
                switch (notification)
                {
                    case PlayerSessionNotifications.NotificationTypes.PlayerCustomDataUpdated:
                    case PlayerSessionNotifications.NotificationTypes.SpectatorCustomDataUpdated:
                        {
                            if(session != null)
                            {
                                session.SendUpdate(notification, paramFlags, jsonData, fromAccountId, toAccountId);
                            }
                        }
                        break;
                    case PlayerSessionNotifications.NotificationTypes.ParamsChanged:
                        {
                            // PlayerSession.ParamTypes.CustomData1 or PlayerSession.ParamTypes.CustomData2:
                            if (session != null)
                            {
                                session.SendUpdate(notification, paramFlags, jsonData, fromAccountId, toAccountId);
                            }
                        }
                        break;
                     case PlayerSessionNotifications.NotificationTypes.SessionMessage:
                        {
                            if (OnUserEvent != null && jsonData.sessionMessage != null)
                            {
                                PlayerNotification notificationData = new PlayerNotification();
                                notificationData.SetMessage(notification, jsonData.sessionId, fromAccountId, toAccountId, jsonData.sessionMessage.payload);
                                OnUserEvent(notificationData);
                            }
                        }
                        break;
                    case PlayerSessionNotifications.NotificationTypes.InvitationsCreated:
                        {
                            if (OnUserEvent != null)
                            {
                                PlayerNotification notificationData = new PlayerNotification();
                                notificationData.SetInvitation(notification, jsonData.sessionId, fromAccountId, toAccountId);
                                OnUserEvent(notificationData);
                            }
                        }
                        break;
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

        static internal void SendUpdate(GameSessionNotifications.NotificationTypes notification, string sessionId, GameSession.ParamTypes paramFlags, NotificationSessionData jsonData, UInt64 fromAccountId = 0, UInt64 toAccountId = 0)
        {
            GameSession session = SessionsManager.FindSessionFromSessionId(sessionId) as GameSession;

            try
            {
                switch (notification)
                {
                    case GameSessionNotifications.NotificationTypes.PlayerCustomDataUpdated:
                    case GameSessionNotifications.NotificationTypes.SpectatorCustomDataUpdated:
                        {
                            if (session != null)
                            {
                                session.SendUpdate(notification, paramFlags, jsonData, fromAccountId, toAccountId);
                            }
                        }
                        break;
                    case GameSessionNotifications.NotificationTypes.ParamsChanged:
                        {
                            // PlayerSession.ParamTypes.CustomData1 or PlayerSession.ParamTypes.CustomData2:
                            if (session != null)
                            {
                                session.SendUpdate(notification, paramFlags, jsonData, fromAccountId, toAccountId);
                            }
                        }
                        break;
                    case GameSessionNotifications.NotificationTypes.SessionMessage:
                        {
                            if (OnUserEvent != null && jsonData.sessionMessage != null)
                            {
                                GameNotification notificationData = new GameNotification();
                                notificationData.SetMessage(notification, jsonData.sessionId, fromAccountId, toAccountId, jsonData.sessionMessage.payload);
                                OnUserEvent(notificationData);
                            }
                        }
                        break;
                    case GameSessionNotifications.NotificationTypes.InvitationsCreated:
                        {
                            if (OnUserEvent != null)
                            {
                                GameNotification notificationData = new GameNotification();
                                notificationData.SetInvitation(notification, jsonData.sessionId, fromAccountId, toAccountId);
                                OnUserEvent(notificationData);
                            }
                        }
                        break;
                }
            }
#pragma warning disable CS0168
            catch (Exception e)
#pragma warning restore CS0168
            {
#if DEBUG
                string output = "GameSession.SendUpdate : " + notification.ToString() + " : " + paramFlags + "\n";
                output += e.Message + "\n";
                output += e.StackTrace;

                Debug.LogError(output);
#endif
            }

        }

        static internal void HandleSessionNotification(string notificationName, NotificationSessionData jsonData, UInt64 fromAccountId = 0, UInt64 toAccountId = 0)
        {
            PlayerSessionNotifications.EventHandlingConfig psConfig = PlayerSessionNotifications.GetEventConfig(notificationName);

            if (psConfig != null)
            {
                SendUpdate(psConfig.NotificationType, jsonData.sessionId, psConfig.ParamFlags, jsonData, fromAccountId, toAccountId);
            }
            else
            {
                GameSessionNotifications.EventHandlingConfig gsConfig = GameSessionNotifications.GetEventConfig(notificationName);

                if (gsConfig != null)
                {
                    SendUpdate(gsConfig.NotificationType, jsonData.sessionId, gsConfig.ParamFlags, jsonData, fromAccountId, toAccountId);
                }
            }
        }

        // Called directly when WebApi notifications are called for a user
        // These might be either Player session or Game session notifications or matchmaking

        // "psn:sessionManager:ps:invitations:created"
        // "psn:sessionManager:ps:m:p:customData1:updated"
        // "psn:sessionManager:ps:m:s:customData1:updated"
        // "psn:sessionManager:ps:customData1:updated"
        // "psn:sessionManager:ps:customData2:updated"
        // "psn:sessionManager:ps:sessionMessage:created"

        // "psn:sessionManager:gs:invitations:created"
        // "psn:sessionManager:gs:sessionMessage:created"

        // "psn:matchmaking:ticket:submitted"
        // "psn:matchmaking:ticket:canceled"
        // "psn:matchmaking:ticket:timedOut"
        // "psn:matchmaking:ticket:failed"
        // "psn:matchmaking:offer:failed"

        static internal void UserWebApiEventHandler(WebApiNotifications.CallbackParams eventData)
        {
            UInt64 fromAccountId = 0;
            if (eventData.From != null)
            {
                fromAccountId = eventData.From.AccountId;
            }
            else
            {
                // Can be null for ticket notifications
            }

            UInt64 toAccountId = 0;
            if (eventData.To != null)
            {
                toAccountId = eventData.To.AccountId;
            }

            if (eventData.Data != null)
            {
                NotificationSessionData eventJsonData = JsonUtility.FromJson<NotificationSessionData>(eventData.Data);

                if (eventJsonData != null && eventJsonData.sessionId != null)
                {
                    // Contains a "sessionId" value in the JSon.
                    HandleSessionNotification(eventData.DataType, eventJsonData, fromAccountId, toAccountId);
                }
                else
                {
                    NotificationMatchMakingData eventMatchMakingJsonData = JsonUtility.FromJson<NotificationMatchMakingData>(eventData.Data);

                    // contains a "ticketId" value in the Json
                    if (eventMatchMakingJsonData != null)
                    {
                        if (eventMatchMakingJsonData.ticketId != null)
                        {
                            Ticket.SendNotification(eventData.DataType, eventMatchMakingJsonData, fromAccountId, toAccountId);
                        }
                        else if(eventMatchMakingJsonData.offerId != null)
                        {
                            Offer.SendNotification(eventData.DataType, eventMatchMakingJsonData, fromAccountId, toAccountId);
                        }
                    }
                }
            }

            if (OnRawUserEvent != null)
            {
                OnRawUserEvent(eventData);
            }
        }

        /// <summary>
        /// Find a session instance from a session id
        /// </summary>
        /// <param name="sessionId">The session id to find</param>
        /// <returns></returns>
        static public Session FindSessionFromSessionId(string sessionId)
        {
            // Check for player session first
            Session session = activePlayerSessions.FindSessionFromSessionId(sessionId);

            // Then check for game session
            if(session == null)
            {
                session = activeGameSessions.FindSessionFromSessionId(sessionId);
            }

            return session;
        }

        /// <summary>
        /// Update a player session from data retrieved from <see cref="PlayerSessionRequests.GetPlayerSessionsRequest"/>
        /// </summary>
        /// <param name="data">The data to update</param>
        static public void UpdateSession(PlayerSessionRequests.RetrievedSessionData data)
        {
            if (PlayerSession.IsParamFlagSet(data.SetFlags, PlayerSession.ParamTypes.SessionId) == false)
            {
#if DEBUG
                Debug.LogError("SessionManager.UpdateSession : SessionId not set in RetrievedSessionData");
#endif
                return;
            }

            PlayerSession ps = FindPlayerSessionFromSessionId(data.SessionId);

            if (ps != null)
            {
                ps.UpdateFrom(data);
            }
        }

        static internal PlayerSession CreatePlayerSession(string sessionId, out bool isNew)
        {
            return activePlayerSessions.CreateSession(sessionId, out isNew);
        }

        static internal bool IsPlayerSessionDeleted(string sessionId)
        {
            return activePlayerSessions.IsSessionDeleted(sessionId);
        }

        static internal void DestroyPlayerSession(PlayerSession ps)
        {
            activePlayerSessions.DestroySession(ps);
        }

        /// <summary>
        /// Find a session instance from a session id
        /// </summary>
        /// <param name="sessionId">The session id to find</param>
        /// <returns></returns>
        static public PlayerSession FindPlayerSessionFromSessionId(string sessionId)
        {
            return activePlayerSessions.FindSessionFromSessionId(sessionId);
        }

        /// <summary>
        /// Find session from a local user id. The user can be a player or spectator
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        static public PlayerSession FindPlayerSessionFromUserId(Int32 userId)
        {
            return activePlayerSessions.FindSessionFromUserId(userId);
        }


        /// <summary>
        /// Removes a player session from the active sessions list
        /// </summary>
        /// <param name="playerSession">The session to remove</param>
        static public void RemovePlayerSession(PlayerSession playerSession)
        {
            activePlayerSessions.DestroySession(playerSession);
        }

        /// <summary>
        /// A list of active player sessions. This is a snap shot of the current active sessions. If any sessions are deleted or created this property needs to be used again to get an accurate state of the system.
        /// </summary>
        static public List<PlayerSession> ActivePlayerSessions
        {
            get
            {
                return activePlayerSessions.CopyOfSessionsList;
            }
        }

        /// <summary>
        /// Update a player session from data retrieved from <see cref="GameSessionRequests.GetGameSessionsRequest"/>
        /// </summary>
        /// <param name="data">The data to update</param>
        static public void UpdateSession(GameSessionRequests.RetrievedSessionData data)
        {
            if (GameSession.IsParamFlagSet(data.SetFlags, GameSession.ParamTypes.SessionId) == false)
            {
#if DEBUG
                Debug.LogError("SessionManager.UpdateSession : SessionId not set in RetrievedSessionData");
#endif
                return;
            }

            GameSession ps = FindGameSessionFromSessionId(data.SessionId);

            if (ps != null)
            {
                ps.UpdateFrom(data);
            }
        }

        static internal GameSession CreateGameSession(string sessionId, out bool isNew)
        {
            return activeGameSessions.CreateSession(sessionId, out isNew);
        }

        static internal bool IsGameSessionDeleted(string sessionId)
        {
            return activeGameSessions.IsSessionDeleted(sessionId);
        }

        static internal void DestroyGameSession(GameSession gs)
        {
            activeGameSessions.DestroySession(gs);
        }

        /// <summary>
        /// Find a session instance from a session id
        /// </summary>
        /// <param name="sessionId">The session id to find</param>
        /// <returns></returns>
        static public GameSession FindGameSessionFromSessionId(string sessionId)
        {
            return activeGameSessions.FindSessionFromSessionId(sessionId);
        }

        /// <summary>
        /// Find session from a local user id. The user can be a player or spectator
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        static public GameSession FindGameSessionFromUserId(Int32 userId)
        {
            return activeGameSessions.FindSessionFromUserId(userId);
        }

        /// <summary>
        /// Removes a game session from the active sessions list
        /// </summary>
        /// <param name="gameSession">The session to remove</param>
        static public void RemoveGameSession(GameSession gameSession)
        {
            activeGameSessions.DestroySession(gameSession);
        }

        /// <summary>
        /// A list of active game sessions. This is a snap shot of the current active sessions. If any sessions are deleted or created this property needs to be used again to get an accurate state of the system.
        /// </summary>
        static public List<GameSession> ActiveGameSessions
        {
            get
            {
                return activeGameSessions.CopyOfSessionsList;
            }
        }

        static Dictionary<int, WebApiPushEvent> userSessionPushEvents = new Dictionary<int, WebApiPushEvent>();
        static SessionsList<PlayerSession> activePlayerSessions = new SessionsList<PlayerSession>();
        static SessionsList<GameSession> activeGameSessions = new SessionsList<GameSession>();
    }

    internal class SessionsList<T> where T : Session, new()
    {
        object SessionCreationSyncObj = new object();
        List<T> activeSessions = new List<T>();
        List<string> deletedSessionIds = new List<string>();

        internal T CreateSession(string sessionId, out bool isNew)
        {
            isNew = false;

            T ps = null;

            lock (SessionCreationSyncObj)
            {
                // must do this because there may have been a "psn:sessionManager:playerSession:created" event come in that may have already created the session
                ps = FindSessionFromSessionId(sessionId);

                if (ps == null)
                {
                    isNew = true;
                    ps = new T();
                    ps.SessionId = sessionId;

                    TrackSession(ps);
                }
            }

            return ps;
        }

        internal void DestroySession(T session)
        {
            lock (SessionCreationSyncObj)
            {
                if (session != null && session.SessionId != null && session.SessionId.Length > 0)
                {
                    deletedSessionIds.Add(session.SessionId);
                }
                UntrackSession(session);
            }
        }

        // Only called while under the SessionCreationSyncObj lock
        private void TrackSession(T session)
        {
            if (activeSessions.Contains(session)) return;

            activeSessions.Add(session);
        }

        // Only called while under the SessionCreationSyncObj lock
        private void UntrackSession(T session)
        {
            activeSessions.Remove(session);
        }

        internal T FindSessionFromSessionId(string sessionId)
        {
            lock (SessionCreationSyncObj)
            {
                foreach (var session in activeSessions)
                {
                    if (session.SessionId == sessionId)
                    {
                        return session;
                    }
                }
            }

            return null;
        }

        internal T FindSessionFromUserId(Int32 userId)
        {
            lock (SessionCreationSyncObj)
            {
                foreach (var session in activeSessions)
                {
                    SessionMember member = session.FindFromUserId(userId);

                    if (member != null)
                    {
                        return session;
                    }
                }
            }

            return null;
        }

        internal bool IsSessionDeleted(string sessionId)
        {
            lock (SessionCreationSyncObj)
            {
                for (int i = 0; i < deletedSessionIds.Count; i++)
                {
                    if (deletedSessionIds[i] == sessionId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal List<T> CopyOfSessionsList
        {
            get
            {
                List<T> copy = new List<T>(activeSessions);
                return copy;
            }
        }
    }

}
