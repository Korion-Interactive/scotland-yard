
using System;
using System.Collections.Generic;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;
using Unity.PSN.PS5.Sessions;
using Unity.PSN.PS5.WebApi;
using UnityEngine;

namespace Unity.PSN.PS5.Matchmaking
{

    /// <summary>
    /// Matchmaking WebApi notification types
    /// </summary>
    public static class MatchMakingNotifications
    {
        /// <summary>
        /// Types of matchmaking notifications
        /// </summary>
        public enum NotificationTypes
        {
            /// <summary> A session has been created </summary>
            TicketSubmitted,
            /// <summary> A session has been deleted </summary>
            TicketCanceled,
            /// <summary> Session parameters have changed </summary>
            TicketTimedOut,
            /// <summary> A player has joined the session</summary>
            TicketFailed,
            /// <summary> A player has left the session</summary>
            OfferFailed,
        }

        internal class EventHandlingConfig
        {
            public EventHandlingConfig(string dataType, NotificationTypes notificationType, bool hasAdditionalJsonData, bool orderGuaranteed)
            {
                DataType = dataType;
                NotificationType = notificationType;
                HasAdditionalJsonData = hasAdditionalJsonData;
                IsOrderGuaranteed = orderGuaranteed;
            }

            public string DataType { get; set; }

            public NotificationTypes NotificationType { get; set; }

            public bool HasAdditionalJsonData { get; set; }

            public bool IsOrderGuaranteed { get; set; }
        }

        static internal EventHandlingConfig[] dataTypeToSessionEvents = new EventHandlingConfig[]
        {
            // Order Not Guaranteed
            // There notification are recieved by individual users, who may or may not have an associated player session.
            // Some of these notifications are related to sessions, like  "psn:sessionManager:ps:customData1:updated"
            new EventHandlingConfig("psn:matchmaking:ticket:submitted", NotificationTypes.TicketSubmitted, true, false),
            new EventHandlingConfig("psn:matchmaking:ticket:canceled", NotificationTypes.TicketCanceled, false, false),
            new EventHandlingConfig("psn:matchmaking:ticket:timedOut", NotificationTypes.TicketTimedOut, false, false),
            new EventHandlingConfig("psn:matchmaking:ticket:failed", NotificationTypes.TicketFailed,  true, false),
            new EventHandlingConfig("psn:matchmaking:offer:failed", NotificationTypes.OfferFailed, true, false),
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

    public partial class Ticket
    {
        /// <summary>
        /// Notification from push events
        /// </summary>
        public struct Notification
        {
            public enum CancelType
            {
                NotSet = -1,
                Cancelled = 0,
                SameRulesetCreated = 1,
                UserOffline = 2,
            }

            /// <summary>
            /// The ticket id
            /// </summary>
            public string TicketId { get; internal set; }

            /// <summary>
            /// The type of notification for the ticket
            /// </summary>
            public MatchMakingNotifications.NotificationTypes NotificationType { get; internal set; }

            /// <summary>
            /// Reason that the ticket was canceled
            /// </summary>
            public CancelType CancelReason { get; internal set; }
        }

        /// <summary>
        /// Delegate for notifications for session events.
        /// </summary>
        public delegate void TicketEventHandler(Notification notificationData);

        /// <summary>
        /// The ticket notification callback.
        /// </summary>
        static public TicketEventHandler OnTicketNotification { get; set; }


        static internal void SendNotification(string dataType, NotificationMatchMakingData jsonData, UInt64 fromAccountId = 0, UInt64 toAccountId = 0)
        {
            if (OnTicketNotification == null)
            {
                return;
            }

            MatchMakingNotifications.EventHandlingConfig config = MatchMakingNotifications.GetEventConfig(dataType);

            if (config == null)
            {
                return;
            }

            Notification notificationData = new Notification()
            {
                TicketId = jsonData.ticketId,
                NotificationType = config.NotificationType,
                CancelReason = Notification.CancelType.NotSet
            };

            try
            {
                switch (config.NotificationType)
                {
                    case MatchMakingNotifications.NotificationTypes.TicketSubmitted:
                    case MatchMakingNotifications.NotificationTypes.TicketTimedOut:
                    case MatchMakingNotifications.NotificationTypes.TicketFailed:
                        break;
                    case MatchMakingNotifications.NotificationTypes.TicketCanceled:
                        notificationData.CancelReason = (Notification.CancelType)jsonData.cancelType;
                        break;
                }
            }
#pragma warning disable CS0168
            catch (Exception e)
#pragma warning restore CS0168
            {
#if DEBUG
                string output = "Ticket.SendNotification : " + config.NotificationType.ToString() + "\n";
                output += e.Message + "\n";
                output += e.StackTrace;

                Debug.LogError(output);
#endif
                return;
            }

            OnTicketNotification(notificationData);
        }
    }

    public partial class Offer
    {
        /// <summary>
        /// Notification from push events
        /// </summary>
        public struct Notification
        {
            /// <summary>
            /// The offer id
            /// </summary>
            public string OfferId { get; internal set; }

            /// <summary>
            /// The type of notification for the offer
            /// </summary>
            public MatchMakingNotifications.NotificationTypes NotificationType { get; internal set; }

        }

        /// <summary>
        /// Delegate for notifications for session events.
        /// </summary>
        public delegate void TicketEventHandler(Notification notificationData);

        /// <summary>
        /// The ticket notification callback.
        /// </summary>
        static public TicketEventHandler OnOfferNotification { get; set; }


        static internal void SendNotification(string dataType, NotificationMatchMakingData jsonData, UInt64 fromAccountId = 0, UInt64 toAccountId = 0)
        {
            if (OnOfferNotification == null)
            {
                return;
            }

            MatchMakingNotifications.EventHandlingConfig config = MatchMakingNotifications.GetEventConfig(dataType);

            Notification notificationData = new Notification()
            {
                OfferId = jsonData.offerId,
                NotificationType = config.NotificationType,
            };

            if (config == null)
            {
                return;
            }

            try
            {
                switch (config.NotificationType)
                {
                    case MatchMakingNotifications.NotificationTypes.OfferFailed:
                        break;
                }
            }
#pragma warning disable CS0168
            catch (Exception e)
#pragma warning restore CS0168
            {
#if DEBUG
                string output = "Offer.SendNotification : " + config.NotificationType.ToString() + "\n";
                output += e.Message + "\n";
                output += e.StackTrace;

                Debug.LogError(output);
#endif
            }

            OnOfferNotification(notificationData);
        }
    }


    // The following classes are filled in using JsonUtility.FromJson
    // Nothing directly assigns to the fields in the classes so the warning needs to be disabled.
    // warning CS0649: Field '?' is never assigned to, and will always have its default value 0

#pragma warning disable 0649
    // Not all paramters in this class will be filled out by the Json parser.
    // Depending on the type of notification will reflect which data is set
    [System.Serializable]
    internal class NotificationMatchMakingData
    {
        public string ticketId;
        public string offerId;
        public string rulesetName;
        public NotificationMatchMakingSubmitter submitter;
        public NotificationMatchMakingTicket ticket;
        public Int32 cancelType = -1;

        public override string ToString()
        {
            string output = "TicketId : " + ticketId;

            output += "\nRulesetName : " + rulesetName;

            if (submitter != null)
            {
                string sStr = submitter.ToString();

                if (sStr != null && sStr.Length > 0)
                {
                    output += "\n Submitter : " + sStr;
                }
            }

            if (ticket != null)
            {
                string sStr = ticket.ToString();

                if (sStr != null && sStr.Length > 0)
                {
                    output += "\n Ticket : " + sStr;
                }
            }

            output += "\nCancelType : " + cancelType;

            return output;
        }
    }

    [System.Serializable]
    class NotificationMatchMakingSubmitter
    {
        public string accountId;
        public string onlineId;
        public string platform;

        public override string ToString()
        {
            string output = "AccountId : " + accountId;
            output += "\nOnlineId : " + onlineId;
            output += "\nPlatform : " + platform;
            return output;
        }
    }

    [System.Serializable]
    class NotificationMatchMakingTicket
    {
        public string ticketId;
        public NotificationMatchMakingSubmitter submitter;

        public override string ToString()
        {
            string output = "\nTicketId : " + ticketId;
            if (submitter != null)
            {
                string sStr = submitter.ToString();

                if (sStr != null && sStr.Length > 0)
                {
                    output += "\n Submitter : " + sStr;
                }
            }
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

                testJson.Add("{\"rulesetName\": \"myruleset\", \"ticketId\": \"ABCDEFGHIJKLM\", \"submitter\": { \"accountId\": \"01234567\", \"onlineId\": \"myonlineid\", \"platform\": \"PS4\" } } " );
                testJson.Add("{\"rulesetName\": \"myruleset\", \"ticketId\": \"ABCDEFGHIJKLM\", \"submitter\": { \"accountId\": \"01234567\", \"onlineId\": \"myonlineid\", \"platform\": \"PS4\" }, \"cancelType\": 0 } ");
                testJson.Add("{\"rulesetName\": \"myruleset\", \"offerId\": \"AAAABBBCCCDDD\", \"ticket\": { \"ticketId\": \"ABCDEFGHIJKLM\", \"submitter\": { \"accountId\": \"01234567\", \"onlineId\": \"myonlineid\", \"platform\": \"PS4\" } } }");

                for (int i = 0; i < testJson.Count; i++)
                {
                    var dataA = JsonUtility.FromJson<NotificationMatchMakingData>(testJson[i]);
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
