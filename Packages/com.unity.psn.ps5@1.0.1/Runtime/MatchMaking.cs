
using System;
using System.Collections.Generic;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;
using Unity.PSN.PS5.WebApi;
using UnityEngine;

namespace Unity.PSN.PS5.Matchmaking
{
    /// <summary>
    /// Matchmaking Attribute
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// Attribute type
        /// </summary>
        public enum AttrTypes
        {
            NotSet = 0,
            String,
            Number,
        }

        /// <summary>
        /// Name of attribute (maximum of 64 characters)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Datatype of attribute
        /// </summary>
        public AttrTypes Datatype { get; set; }

        /// <summary>
        /// Value of attribute
        /// </summary>
        public string Value { get; set; }

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(Name);
            writer.Write((Int32)Datatype);
            writer.WritePrxString(Value);
        }

        internal void Deserialise(BinaryReader reader)
        {
            Name = reader.ReadPrxString();
            Datatype = (AttrTypes)reader.ReadInt32();
            Value = reader.ReadPrxString();
        }
    }

    /// <summary>
    /// Ticket Player
    /// </summary>
    public class TicketPlayer
    {
        /// <summary>
        /// Account ID
        /// </summary>
        public UInt64 AccountId { get; set; }

        /// <summary>
        /// Online ID
        /// </summary>
        /// <remarks>
        /// Maybe set when using GetTicket request
        /// </remarks>
        public string OnlineId { get; internal set; }

        /// <summary>
        /// Platform
        /// </summary>
        public NpPlatformType Platform { get; set; }

        /// <summary>
        /// Affiliated team name. Maximum of 64 characters.
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// Is <see cref="NatType"/> set
        /// </summary>
        public bool UseNateType { get; set; }

        /// <summary>
        /// Nat type, 1 to 3
        /// </summary>
        public Int32 NatType { get; set; }

        /// <summary>
        /// List of player attributes
        /// </summary>
        public List<Attribute> Attributes { get; set; }

        internal void Serialise(BinaryWriter writer)
        {
            writer.Write(AccountId);
            writer.Write((Int32)Platform);

            bool hasTeamName = TeamName != null && TeamName.Length > 0;
            writer.Write(hasTeamName);
            if (hasTeamName == true)
            {
                writer.WritePrxString(TeamName);
            }

            writer.Write(UseNateType);
            if (UseNateType == true)
            {
                writer.Write(NatType);
            }

            int numAttributes = Attributes != null ? Attributes.Count : 0;

            writer.Write(numAttributes);

            if (numAttributes > 0)
            {
                for (int i = 0; i < numAttributes; i++)
                {
                    Attributes[i].Serialise(writer);
                }
            }
        }

        internal void Deserialise(BinaryReader reader)
        {
            bool isAccountIdSet = reader.ReadBoolean();
            if (isAccountIdSet)
            {
                AccountId = reader.ReadUInt64();
            }
            else
            {
                AccountId = default;
            }

            bool isOnlineIdSet = reader.ReadBoolean();
            if (isOnlineIdSet)
            {
                OnlineId = reader.ReadPrxString();
            }
            else
            {
                OnlineId = null;
            }

            bool isPlatformSet = reader.ReadBoolean();
            if (isPlatformSet)
            {
                Platform = (NpPlatformType)reader.ReadInt32();
            }
            else
            {
                Platform = NpPlatformType.None;
            }

            bool isTeamNameSet = reader.ReadBoolean();
            if (isTeamNameSet)
            {
                TeamName = reader.ReadPrxString();
            }
            else
            {
                TeamName = null;
            }

            UseNateType = reader.ReadBoolean();
            if (UseNateType)
            {
                NatType = reader.ReadInt32();
            }
            else
            {
                NatType = 0;
            }

            int numAttributes = reader.ReadInt32();

            if (numAttributes > 0)
            {
                Attributes = new List<Attribute>(numAttributes);

                for (int i = 0; i < numAttributes; i++)
                {
                    Attribute attr = new Attribute();
                    attr.Deserialise(reader);
                    Attributes.Add(attr);
                }
            }
            else
            {
                Attributes = null;
            }
        }
    }

    public class TicketSubmitter
    {
        /// <summary>
        /// Account ID
        /// </summary>
        public UInt64 AccountId;

        /// <summary>
        /// Platform
        /// </summary>
        public NpPlatformType Platform;

        internal void Deserialise(BinaryReader reader)
        {
            bool isAccountIdSet = reader.ReadBoolean();
            if (isAccountIdSet)
            {
                AccountId = reader.ReadUInt64();
            }

            bool isPlatformSet = reader.ReadBoolean();
            if (isPlatformSet)
            {
                Platform = (NpPlatformType)reader.ReadInt32();
            }
        }
    }

    /// <summary>
    /// Place to make players who have undergone matchmaking join in
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Game Session ID. The format is UUID Version 4.
        /// </summary>
        public string GameSessionId;

        internal void Deserialise(BinaryReader reader)
        {
            bool isGameSessionIdSet = reader.ReadBoolean();
            if (isGameSessionIdSet)
            {
                GameSessionId = reader.ReadPrxString();
            }
        }
    }

    public class UserTicket
    {
        /// <summary>
        /// Ticket ID. The format is UUID Version 4.
        /// </summary>
        public string TicketId { get; internal set; }

        /// <summary>
        /// Status of the ticket.
        /// </summary>
        public Ticket.TicketStatus Status { get; internal set; }

        /// <summary>
        /// Ruleset name. Maximum of 64 characters.
        /// </summary>
        public string RulesetName { get; internal set; }

        /// <summary>
        /// Platform
        /// </summary>
        public NpPlatformType Platform { get; set; }

        internal void Deserialise(BinaryReader reader)
        {
            bool isTickedIdSet = reader.ReadBoolean();
            if (isTickedIdSet)
            {
                TicketId = reader.ReadPrxString();
            }
            else
            {
                TicketId = null;
            }

            bool isStatusSet = reader.ReadBoolean();
            if (isStatusSet)
            {
                Status = (Ticket.TicketStatus)reader.ReadInt32();
            }
            else
            {
                Status = Ticket.TicketStatus.NotSet;
            }

            bool isRulesetNameSet = reader.ReadBoolean();
            if (isRulesetNameSet)
            {
                RulesetName = reader.ReadPrxString();
            }
            else
            {
                RulesetName = null;
            }

            bool isPlatformSet = reader.ReadBoolean();
            if (isPlatformSet)
            {
                Platform = (NpPlatformType)reader.ReadInt32();
            }
            else
            {
                Platform = NpPlatformType.None;
            }
        }
    }

    public partial class Ticket
    {
        public enum TicketStatus
        {
            NotSet = 0,
            Queued, // "QUEUED"
            Searching, // "SEARCHING"
            Completed, // "COMPLETED"
            TimedOut, // "TIMED_OUT"
            Failed, // "FAILED"
        }

        /// <summary>
        /// Ticket ID. The format is UUID Version 4.
        /// </summary>
        public string TicketId { get; internal set; }

        /// <summary>
        /// Ruleset name. Maximum of 64 characters.
        /// </summary>
        public string RulesetName { get; internal set; }

        /// <summary>
        /// List of ticket attributes
        /// </summary>
        public List<Attribute> TicketAttributes { get; internal set; }

        /// <summary>
        /// List of players included in the ticket. The maximum is 256, like a Game Session.
        /// </summary>
        public List<TicketPlayer> Players { get; internal set; }

        /// <summary>
        /// Status of the ticket. The statuses of tickets are updated as processing proceeds
        /// </summary>
        public TicketStatus Status { get; internal set; }

        /// <summary>
        /// ID of the offer corresponding to the ticket. This will only be returned when the <see cref="Status"/> is <see cref="TicketStatus.Completed"/>.
        /// </summary>
        public string OfferId { get; internal set; }

        /// <summary>
        /// Ticket submitter
        /// </summary>
        public TicketSubmitter Submitter { get; internal set; }

        /// <summary>
        /// Place to make players who have undergone matchmaking join in
        /// </summary>
        public Location Location { get; internal set; }

        /// <summary>
        /// Created date and time
        /// </summary>
        public DateTime CreatedDateTime { get; internal set; }

        /// <summary>
        /// Updated date and time
        /// </summary>
        public DateTime UpdatedDateTime { get; internal set; }

        internal void Deserialise(BinaryReader reader)
        {
            bool isTickedIdSet = reader.ReadBoolean();
            if (isTickedIdSet)
            {
                TicketId = reader.ReadPrxString();
            }
            else
            {
                TicketId = null;
            }

            bool isRulesetNameSet = reader.ReadBoolean();
            if (isRulesetNameSet)
            {
                RulesetName = reader.ReadPrxString();
            }
            else
            {
                RulesetName = null;
            }

            int numTicketAttributes = reader.ReadInt32();

            if (numTicketAttributes > 0)
            {
                TicketAttributes = new List<Attribute>(numTicketAttributes);

                for (int i = 0; i < numTicketAttributes; i++)
                {
                    Attribute attr = new Attribute();
                    attr.Deserialise(reader);
                    TicketAttributes.Add(attr);
                }
            }
            else
            {
                TicketAttributes = null;
            }

            int numPlayers = reader.ReadInt32();

            if (numPlayers > 0)
            {
                Players = new List<TicketPlayer>(numPlayers);

                for (int i = 0; i < numPlayers; i++)
                {
                    TicketPlayer player = new TicketPlayer();
                    player.Deserialise(reader);
                    Players.Add(player);
                }
            }
            else
            {
                Players = null;
            }

            bool isStatusSet = reader.ReadBoolean();
            if (isStatusSet)
            {
                Status = (TicketStatus)reader.ReadInt32();
            }
            else
            {
                Status = TicketStatus.NotSet;
            }

            bool isOfferIdSet = reader.ReadBoolean();
            if (isOfferIdSet)
            {
                OfferId = reader.ReadPrxString();
            }
            else
            {
                OfferId = null;
            }

            bool isSubmitterSet = reader.ReadBoolean();
            if (isSubmitterSet)
            {
                Submitter = new TicketSubmitter();
                Submitter.Deserialise(reader);
            }
            else
            {
                Submitter = null;
            }

            bool isCreatedDateTimeSet = reader.ReadBoolean();
            if (isCreatedDateTimeSet)
            {
                CreatedDateTime = reader.ReadRtcTicks();
            }
            else
            {
                CreatedDateTime = default(DateTime);
            }

            bool isUpdatedDateTimeSet = reader.ReadBoolean();
            if (isUpdatedDateTimeSet)
            {
                UpdatedDateTime = reader.ReadRtcTicks();
            }
            else
            {
                CreatedDateTime = default(DateTime);
            }

            bool isLocationSet = reader.ReadBoolean();
            if (isLocationSet)
            {
                Location = new Location();
                Location.Deserialise(reader);
            }
            else
            {
                Location = null;
            }
        }
    }

    /// <summary>
    /// Offer Player
    /// </summary>
    public class OfferPlayer
    {
        /// <summary>
        /// Account ID
        /// </summary>
        public UInt64 AccountId { get; set; }

        /// <summary>
        /// Online ID
        /// </summary>
        public string OnlineId { get; internal set; }

        /// <summary>
        /// Platform
        /// </summary>
        public NpPlatformType Platform { get; set; }

        /// <summary>
        /// Affiliated team name. Maximum of 64 characters.
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// Ticket ID. The format is UUID Version 4.
        /// </summary>
        public string TickedId { get; internal set; }

        internal void Deserialise(BinaryReader reader)
        {
            bool isAccountIdSet = reader.ReadBoolean();
            if (isAccountIdSet)
            {
                AccountId = reader.ReadUInt64();
            }
            else
            {
                AccountId = default;
            }

            bool isOnlineIdSet = reader.ReadBoolean();
            if (isOnlineIdSet)
            {
                OnlineId = reader.ReadPrxString();
            }
            else
            {
                OnlineId = null;
            }

            bool isPlatformSet = reader.ReadBoolean();
            if (isPlatformSet)
            {
                Platform = (NpPlatformType)reader.ReadInt32();
            }
            else
            {
                Platform = NpPlatformType.None;
            }

            bool isTeamNameSet = reader.ReadBoolean();
            if (isTeamNameSet)
            {
                TeamName = reader.ReadPrxString();
            }
            else
            {
                TeamName = null;
            }

            bool isTickedIdSet = reader.ReadBoolean();
            if (isTickedIdSet)
            {
                TickedId = reader.ReadPrxString();
            }
            else
            {
                TickedId = null;
            }
        }
    }

    public partial class Offer
    {
        public enum OfferStatus
        {
            NotSet = 0,
            Placing, // "PLACING"
            Completed, // "COMPLETED"
            Failed, // "FAILED"
        }

        /// <summary>
        /// Offer ID. The format is UUID Version 4.
        /// </summary>
        public string OfferId { get; internal set; }

        /// <summary>
        /// Ruleset name. Maximum of 64 characters.
        /// </summary>
        public string RulesetName { get; internal set; }

        /// <summary>
        /// List of players included in the ticket. The maximum is 256, like a Game Session.
        /// </summary>
        public List<OfferPlayer> Players { get; internal set; }

        /// <summary>
        /// The status of an an offer is updated as processing proceeds
        /// </summary>
        public OfferStatus Status { get; internal set; }

        /// <summary>
        /// Information indicating what was created based on the offer. This can only be returned when the status is COMPLETED.
        /// </summary>
        public Location Location { get; internal set; }

        /// <summary>
        /// Created date and time
        /// </summary>
        public DateTime CreatedDateTime { get; internal set; }

        /// <summary>
        /// Updated date and time
        /// </summary>
        public DateTime UpdatedDateTime { get; internal set; }

        internal void Deserialise(BinaryReader reader)
        {
            bool isOfferIdSet = reader.ReadBoolean();
            if (isOfferIdSet)
            {
                OfferId = reader.ReadPrxString();
            }
            else
            {
                OfferId = null;
            }

            bool isRulesetNameSet = reader.ReadBoolean();
            if (isRulesetNameSet)
            {
                RulesetName = reader.ReadPrxString();
            }
            else
            {
                RulesetName = null;
            }

            int numPlayers = reader.ReadInt32();

            if (numPlayers > 0)
            {
                Players = new List<OfferPlayer>(numPlayers);

                for (int i = 0; i < numPlayers; i++)
                {
                    OfferPlayer player = new OfferPlayer();
                    player.Deserialise(reader);
                    Players.Add(player);
                }
            }
            else
            {
                Players = null;
            }

            bool isStatusSet = reader.ReadBoolean();
            if (isStatusSet)
            {
                Status = (OfferStatus)reader.ReadInt32();
            }
            else
            {
                Status = OfferStatus.NotSet;
            }

            bool isLocationSet = reader.ReadBoolean();
            if (isLocationSet)
            {
                Location = new Location();
                Location.Deserialise(reader);
            }
            else
            {
                Location = null;
            }

            bool isCreatedDateTimeSet = reader.ReadBoolean();
            if (isCreatedDateTimeSet)
            {
                CreatedDateTime = reader.ReadRtcTicks();
            }
            else
            {
                CreatedDateTime = default(DateTime);
            }

            bool isUpdatedDateTimeSet = reader.ReadBoolean();
            if (isUpdatedDateTimeSet)
            {
                UpdatedDateTime = reader.ReadRtcTicks();
            }
            else
            {
                CreatedDateTime = default(DateTime);
            }
        }
    }



}
