
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
    /// Requests used for matchmaking
    /// </summary>
    public class MatchMakingRequests
    {
        internal enum NativeMethods : UInt32
        {
            SubmitTicket = 0x0D00001u,
            GetTicket = 0x0D00002u,
            CancelTicket = 0x0D00003u,
            GetOffer = 0x0D00004u,
            ListUserTickets = 0x0D00005u,
        }

        /// <summary>
        /// Create a matchmaking ticket
        /// </summary>
        public class SubmitTicketRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Ruleset name. Maximum of 64 characters.
            /// </summary>
            public String RulesetName { get; set; }

            /// <summary>
            /// List of players included in the ticket
            /// </summary>
            public List<TicketPlayer> Players { get; set; }

            /// <summary>
            /// List of ticket attributes
            /// </summary>
            public List<Attribute> TicketAttributes { get; set; }

            /// <summary>
            /// Game Session ID for backfilling an existing session
            /// </summary>
            public string GameSessionId { get; set; }

            /// <summary>
            /// The created ticket
            /// </summary>
            public Ticket Ticket { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.SubmitTicket);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(RulesetName);

                int numPlayers = Players != null ? Players.Count : 0;

                nativeMethod.Writer.Write(numPlayers);

                if(numPlayers > 0)
                {
                    for(int i = 0; i < numPlayers; i++)
                    {
                        Players[i].Serialise(nativeMethod.Writer);
                    }
                }

                int numAttributes = TicketAttributes != null ? TicketAttributes.Count : 0;

                nativeMethod.Writer.Write(numAttributes);

                if (numAttributes > 0)
                {
                    for (int i = 0; i < numAttributes; i++)
                    {
                        TicketAttributes[i].Serialise(nativeMethod.Writer);
                    }
                }

                bool isGameSessionIdSet = GameSessionId != null && GameSessionId.Length > 0;

                nativeMethod.Writer.Write(isGameSessionIdSet);

                if (isGameSessionIdSet)
                {
                    nativeMethod.Writer.WritePrxString(GameSessionId);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Ticket = new Ticket();
                    Ticket.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Obtain a matchmaking ticket
        /// </summary>
        public class GetTicketRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Ticket ID. The format is UUID Version 4.
            /// </summary>
            public String TicketId { get; set; }

            /// <summary>
            /// Specify the collection of fields that can be returned as the response by name.
            /// </summary>
            /// <remarks>
            /// If "default" : ticketId, rulesetName, status, offerId, createdDateTime
            /// If "v1.0" : ticketId, rulesetName, ticketAttributes, players, status, offerId, submitter, createdDateTime, updatedDateTime
            /// </remarks>
            public String View { get; set; } = "default";

            /// <summary>
            /// The created ticket
            /// </summary>
            public Ticket Ticket { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetTicket);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(TicketId);

                nativeMethod.Writer.WritePrxString(View);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Ticket = new Ticket();

                    Ticket.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Delete a matchmaking ticket
        /// </summary>
        public class CancelTicketRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Ticket ID. The format is UUID Version 4.
            /// </summary>
            public String TicketId { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.CancelTicket);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(TicketId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                  //  SessionsManager.DestroyTicket(TicketId);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Obtain a matchmaking offer
        /// </summary>
        public class GetOfferRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Offer ID. The format is UUID Version 4.
            /// </summary>
            public String OfferId { get; set; }

            /// <summary>
            /// Offer
            /// </summary>
            public Offer Offer { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetOffer);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.WritePrxString(OfferId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Offer = new Offer();

                    Offer.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Obtain a list of matchmaking tickets that a user has joined
        /// </summary>
        public class ListUserTicketsRequest : Request
        {
            /// <summary>
            /// Filters to narrow down the elements returned by the request
            /// </summary>
            [Flags]
            public enum PlatformFilters
            {
                /// <summary> PS5 </summary>
                PS5 = 1,
                /// <summary> PS4 </summary>
                PS4 = 2,
            }

            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Ruleset name. Maximum of 64 characters. Optional
            /// </summary>
            public String RulesetFilterName { get; set; }

            /// <summary>
            /// Specifies a platform to narrow down the elements in the list. Optional
            /// </summary>
            public PlatformFilters PlatformFilter { get; set; } = 0;

            /// <summary>
            /// List of tickets obtained
            /// </summary>
            public List<UserTicket> UserTickets { get; internal set; } = new List<UserTicket>(20);

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.ListUserTickets);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                if(PlatformFilter != 0)
                {
                    string filterText = "";

                    if( (PlatformFilter & PlatformFilters.PS4) != 0)
                    {
                        filterText += "PS4,";
                    }
                    if ((PlatformFilter & PlatformFilters.PS5) != 0)
                    {
                        filterText += "PS5";
                    }

                    if (filterText != null && filterText.Length > 0)
                    {
                        nativeMethod.Writer.Write(true);
                        nativeMethod.Writer.WritePrxString(filterText);
                    }
                    else
                    {
                        nativeMethod.Writer.Write(false);
                    }
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (RulesetFilterName != null && RulesetFilterName.Length > 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.WritePrxString(RulesetFilterName);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    UserTickets.Clear();

                    Int32 numTickets = nativeMethod.Reader.ReadInt32();

                    for(int i = 0; i < numTickets; i++)
                    {
                        UserTicket userTicket = new UserTicket();
                        userTicket.Deserialise(nativeMethod.Reader);

                        UserTickets.Add(userTicket);
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }
}
