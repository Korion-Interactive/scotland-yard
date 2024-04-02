
using System;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;
using System.Collections.Generic;

namespace Unity.PSN.PS5.Leaderboard
{
    /// <summary>
    /// The Leaderboards service provided by PSN provides multiple leaderboards onto which scores can be recorded for an application per user (player) and on which rankings are calculated.
    /// </summary>
    public class Leaderboards
    {
        enum NativeMethods : UInt32
        {
            GetBoardDefinition = 0x1100001u,
            RecordScore = 0x1100002u,
            GetRanking = 0x1100003u,
            GetLargeDateByObjectId = 0x1100004u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("Leaderboards");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal Entitlements queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Score sorting order
        /// </summary>
        public enum SortModes
        {
            /// <summary> Not set </summary>
            NotSet,
            /// <summary> Scores are displayed higher-up the smaller they are </summary>
            Ascending,
            /// <summary> Scores are displayed higher-up the larger they are </summary>
            Descending,
        }

        /// <summary>
        /// Score update rules
        /// </summary>
        public enum UpdateModes
        {
            /// <summary> Not set </summary>
            NotSet,
            /// <summary> Scores are displayed higher-up the smaller they are </summary>
            UpdateWithBestScore,
            /// <summary> Scores are displayed higher-up the larger they are </summary>
            UpdateAlways,
        }

        /// <summary>
        /// Which fields are valid when retrieving a board definition
        /// </summary>

        public static class BoardFields
        {
            /// <summary>
            /// Flags used for Board properties
            /// </summary>
            [Flags]
            public enum Flags
            {
                /// <summary> Not set </summary>
                NotSet = 0,
                /// <summary> The session id is used </summary>
                EntryLimit = 1,                          // bit 1
                /// <summary> Date and time of creation of the Player Session </summary>
                LargeDataNumLimit = 2,                   // bit 2
                /// <summary> Maximum number of members who can join a Player Session as players </summary>
                LargeDataSizeLimit = 4,                         // bit 3
                /// <summary> Maximum number of members who can join a Player Session as spectators </summary>
                MaxScoreLimit = 8,                      // bit 4
                /// <summary> Participating members (including players and spectators) </summary>
                MinScoreLimit = 16,                            // bit 5
                /// <summary> Members participating as players </summary>
                SortMode = 32,                     // bit 6
                /// <summary> Members participating as spectators </summary>
                UpdateMode = 64,                  // bit 7

                /// <summary> A typical set of flags when retrieving player sesssion data </summary>
                Default = EntryLimit | LargeDataNumLimit | LargeDataSizeLimit | MaxScoreLimit | MinScoreLimit | SortMode | UpdateMode,
            }

            /// <summary>
            /// Test is a flag is set
            /// </summary>
            /// <param name="flags">The board flags to check</param>
            /// <param name="flagToCheck">The board flag to test</param>
            /// <returns>Returns true if the flag to check is set</returns>

            public static bool IsParamFlagSet(Flags flags, Flags flagToCheck)
            {
                if ((flags & flagToCheck) != 0) return true;

                return false;
            }

            static internal string[] FlagText = new string[]
            {
                "entryLimit",                       // EntryLimit             // bit 1
                "largeDataNumLimit",                // LargeDataNumLimit      // bit 2
                "largeDataSizeLimit",               // LargeDataSizeLimit     // bit 3
                "maxScoreLimit",                    // MaxScoreLimit          // bit 4
                "minScoreLimit",                    // MinScoreLimit          // bit 5
                "sortMode",                         // SortMode               // bit 6
                "updateMode",                       // UpdateMode             // bit 7
            };

            static internal string SerialiseFieldFlags(Flags flags)
            {
                string output = "";

                // Now go through each flag and write out the text for it, if it is set
                uint bits = (uint)flags;
                int index = 0;
                while (bits != 0)
                {
                    // Test lowest bit
                    if ((bits & 1) != 0)
                    {
                        if (index < FlagText.Length)
                        {
                            if (output.Length > 0) output += ",";
                            output += FlagText[index];
                        }
                    }

                    bits = bits >> 1;
                    index++;
                }

                return output;
            }
        }

        /// <summary>
        /// Additional content information
        /// </summary>
        public class BoardDefinition
        {
            /// <summary>
            /// These are the components of the leaderboard specification
            /// </summary>
            public BoardFields.Flags Fields { get; set; } = BoardFields.Flags.NotSet;

            /// <summary>
            /// Maximum value for the number of entries that can be recorded on a leaderboard
            /// </summary>
            public Int32 EntryLimit { get; internal set; }

            /// <summary>
            /// The lowest rank (where the player who scored first is ranked higher if there is a tie in terms of points) at which a player can record large-volume attachment data. Only players positioned at this rank or better can record such data.
            /// </summary>
            public Int32 LargeDataNumLimit { get; internal set; }

            /// <summary>
            /// Maximum size (in bytes) for large-volume attached data
            /// </summary>
            public Int64 LargeDataSizeLimit { get; internal set; }

            /// <summary>
            /// Maximum score value that can be set
            /// </summary>
            public Int64 MaxScoreLimit { get; internal set; }

            /// <summary>
            /// Minimum  score value that can be set
            /// </summary>
            public Int64 MinScoreLimit { get; internal set; }

            /// <summary>
            /// Score sorting order. <see cref="SortModes.Ascending"/> (scores are displayed higher-up the smaller they are), and <see cref="SortModes.Descending"/> (scores are displayed higher-up the larger they are).
            /// </summary>
            public SortModes SortMode { get; internal set; }

            /// <summary>
            /// Score update rules. update_with_best_score means that a player's score can only be updated when it is his or her personal best, and update_always means that scores can always be updated.
            /// </summary>
            public UpdateModes UpdateMode { get; internal set; }


            internal void Deserialise(BinaryReader reader)
            {
                Fields = BoardFields.Flags.NotSet;

                bool isEntryLimitSet = reader.ReadBoolean();
                if (isEntryLimitSet)
                {
                    EntryLimit = reader.ReadInt32();
                    Fields |= BoardFields.Flags.EntryLimit;
                }

                bool isLargeDataNumLimitSet = reader.ReadBoolean();
                if (isLargeDataNumLimitSet)
                {
                    LargeDataNumLimit = reader.ReadInt32();
                    Fields |= BoardFields.Flags.LargeDataNumLimit;
                }

                bool isLargeDataSizeLimitSet = reader.ReadBoolean();
                if (isLargeDataSizeLimitSet)
                {
                    LargeDataSizeLimit = reader.ReadInt64();
                    Fields |= BoardFields.Flags.LargeDataSizeLimit;
                }

                bool isMaxScoreLimitSet = reader.ReadBoolean();
                if (isMaxScoreLimitSet)
                {
                    MaxScoreLimit = reader.ReadInt64();
                    Fields |= BoardFields.Flags.MaxScoreLimit;
                }

                bool isMinScoreLimitSet = reader.ReadBoolean();
                if (isMinScoreLimitSet)
                {
                    MinScoreLimit = reader.ReadInt64();
                    Fields |= BoardFields.Flags.MinScoreLimit;
                }

                bool isSortModeSet = reader.ReadBoolean();
                if (isSortModeSet)
                {
                    SortMode = (SortModes)reader.ReadInt32();
                    Fields |= BoardFields.Flags.SortMode;
                }

                bool isUpdateModeSet = reader.ReadBoolean();
                if (isUpdateModeSet)
                {
                    UpdateMode = (UpdateModes)reader.ReadInt32();
                    Fields |= BoardFields.Flags.UpdateMode;
                }
            }
        }

        /// <summary>
        /// Obtain leaderboard definition
        /// </summary>
        public class GetBoardDefinitionRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Leaderboard ID
            /// </summary>
            public Int32 BoardId { get; set; }

            /// <summary>
            /// NP service label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// These are the components of the leaderboard specification
            /// </summary>
            public BoardFields.Flags Fields { get; set; } = BoardFields.Flags.Default;

            /// <summary>
            /// List of additional content entitlements.
            /// </summary>
            public BoardDefinition Board { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetBoardDefinition);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(BoardId);

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (Fields != 0)
                {
                    nativeMethod.Writer.Write(true);

                    string fieldStr = BoardFields.SerialiseFieldFlags(Fields);
                    nativeMethod.Writer.WritePrxString(fieldStr);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Board = new BoardDefinition();

                    Board.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }


        /// <summary>
        /// Records scores
        /// </summary>
        public class RecordScoreRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Leaderboard ID
            /// </summary>
            public Int32 BoardId { get; set; }

            /// <summary>
            /// NP service label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Score value
            /// </summary>
            public Int64 Score { get; set; }

            /// <summary>
            /// Flag that specifies whether to wait for large-volume attached data to be recorded before calculating rankings
            /// Not yet supported. Large data can be written after score has been uploaded.
            /// All handled by the native code as part of this request.
            /// </summary>
            private bool WaitsForData { get; set; } = false;

            /// <summary>
            /// Specifies true when a temporary ranking is required
            /// </summary>
            public bool NeedsTmpRank { get; set; } = false;

            /// <summary>
            /// Object ID. Identifier for large-volume attachment data.
            /// Not yet supported. Large data can be written after score score has been uploaded.
            /// All handled by the native code as part of this request.
            /// </summary>
            private string ObjectId { get; set; }

            /// <summary>
            /// Player character ID
            /// </summary>
            public Int32 PlayerCharacterID { get; set; } = -1;

            /// <summary>
            /// Small-volume attachment data
            /// </summary>
            public byte[] SmallData { get; set; } = null;

            /// <summary>
            /// Text-format comment information
            /// </summary>
            public string Comment { get; set; }

            /// <summary>
            /// Date and time information used for determining whether scores can be updated
            /// </summary>
            public DateTime ComparedDateTime { get; set; }

            /// <summary>
            /// Large data, if required
            /// </summary>
            public byte[] LargeData { get; set; } = null;

            /// <summary>
            /// Temporary ranking calculated with players with the same number of points tied
            /// </summary>
            public Int32 TmpRank { get; internal set; }

            /// <summary>
            /// Temporary ranking with higher rankings for players with the same number of points as others but who scored first, calculated in a simplified manner
            /// </summary>
            public Int32 TmpSerialRank { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.RecordScore);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(BoardId);
                nativeMethod.Writer.Write(Score);

                int largeDataSize = LargeData != null ? LargeData.Length : 0;
                nativeMethod.Writer.Write(largeDataSize);
                if (largeDataSize > 0)
                {
                    nativeMethod.Writer.Write(LargeData);
                }

                nativeMethod.Writer.Write(WaitsForData);
                nativeMethod.Writer.Write(NeedsTmpRank);

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ObjectId != null && ObjectId.Length > 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.WritePrxString(ObjectId);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (PlayerCharacterID >= 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(PlayerCharacterID);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (SmallData != null && SmallData.Length > 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((UInt64)SmallData.Length);
                    nativeMethod.Writer.Write(SmallData);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (Comment != null && Comment.Length > 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.WritePrxString(Comment);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ComparedDateTime != default)
                {
                    const Int64 sceToDotNetTicks = 10;   // sce ticks are microsecond, .net are 100 nanosecond

                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ComparedDateTime.Ticks / sceToDotNetTicks);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    bool isTmpRankSet = nativeMethod.Reader.ReadBoolean();
                    if (isTmpRankSet)
                    {
                        TmpRank = nativeMethod.Reader.ReadInt32();
                        TmpSerialRank = nativeMethod.Reader.ReadInt32();
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Leaderboard entry
        /// </summary>

        public class RankingEntry
        {
            /// <summary>
            /// Account ID
            /// </summary>
            public UInt64 AccountId { get; internal set; }

            /// <summary>
            /// Player character ID
            /// </summary>
            public Int32 PlayerCharacterID { get; internal set; } = 0;

            /// <summary>
            /// Official ranking calculated with higher rankings for players with the same number of points as others but who scored first
            /// </summary>
            public Int32 SerialRank { get; internal set; }

            /// <summary>
            /// Highest ranking (with the player who scored first ranked higher if there is a tie in terms of points)
            /// </summary>
            public Int32 HighestSerialRank { get; internal set; }

            /// <summary>
            /// Official ranking calculated with the same rankings for players with the same number of points
            /// </summary>
            public Int32 Rank { get; internal set; }

            /// <summary>
            /// Highest ranking (with players with the same number of points tied)
            /// </summary>
            public Int32 HighestRank { get; internal set; }

            /// <summary>
            /// Score value
            /// </summary>
            public Int64 Score { get; internal set; }

            /// <summary>
            /// Small-volume attached data
            /// </summary>
            public byte[] SmallData { get; internal set; }

            /// <summary>
            /// Object ID. Identifier for large-volume attached data.
            /// </summary>
            public string ObjectId { get; internal set; }

            /// <summary>
            /// Text-formatted comment information
            /// </summary>
            public string Comment { get; internal set; }

            /// <summary>
            /// Online ID
            /// </summary>
            public string OnlineId { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                AccountId = reader.ReadUInt64();

                bool isPcIdSet = reader.ReadBoolean();
                if (isPcIdSet)
                {
                    PlayerCharacterID = reader.ReadInt32();
                }
                else
                {
                    PlayerCharacterID = 0;
                }

                SerialRank = reader.ReadInt32();
                HighestSerialRank = reader.ReadInt32();
                Rank = reader.ReadInt32();
                HighestRank = reader.ReadInt32();
                Score = reader.ReadInt64();

                bool isSmallDataSet = reader.ReadBoolean();
                if (isSmallDataSet)
                {
                    SmallData = reader.ReadData();
                }
                else
                {
                    SmallData = null;
                }

                bool isObjectIdSet = reader.ReadBoolean();
                if (isObjectIdSet)
                {
                    ObjectId = reader.ReadPrxString();
                }
                else
                {
                    ObjectId = null;
                }

                bool isCommentSet = reader.ReadBoolean();
                if (isCommentSet)
                {
                    Comment = reader.ReadPrxString();
                }
                else
                {
                    Comment = null;
                }

                OnlineId = reader.ReadPrxString();
            }
        }

        /// <summary>
        /// Leadboard rankings
        /// </summary>
        public class Rankings
        {
            /// <summary>
            /// Leaderboard entries
            /// </summary>
            public List<RankingEntry> Entries { get; internal set; }

            /// <summary>
            /// Date and time official rankings were most recently calculated
            /// </summary>
            public DateTime LastUpdateDateTime { get; internal set; }

            /// <summary>
            /// Total number of entries
            /// </summary>
            public Int32 TotalEntryCount { get; internal set; }

            internal void Deserialise(BinaryReader reader)
            {
                Int32 numUsers = reader.ReadInt32();

                if (numUsers > 0)
                {
                    Entries = new List<RankingEntry>(numUsers);
                    for (int i = 0; i < numUsers; i++)
                    {
                        RankingEntry entry = new RankingEntry();
                        entry.Deserialise(reader);
                        Entries.Add(entry);
                    }
                }

                LastUpdateDateTime = reader.ReadRtcTicks();
                TotalEntryCount = reader.ReadInt32();
            }
        }

        /// <summary>
        /// Obtains ranking data
        /// </summary>
        public class GetRankingRequest : Request
        {
            /// <summary>
            /// Collection of target users
            /// </summary>
            public enum Groups
            {
                /// <summary> Not set </summary>
                NotSet,
                /// <summary> Friends </summary>
                Friends,
            }

            /// <summary>
            /// Settings for a user when specifying a list of users
            /// </summary>
            public class UserInfo
            {
                /// <summary>
                /// Account ID
                /// </summary>
                public UInt64 AccountId { get; set; }

                /// <summary>
                /// Player character ID
                /// </summary>
                public Int32 PlayerCharacterID { get; set; }

                internal void Serialise(BinaryWriter writer)
                {
                    writer.Write(AccountId);
                    writer.Write(PlayerCharacterID);
                }
            }

            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Leaderboard ID
            /// </summary>
            public Int32 BoardId { get; set; }

            /// <summary>
            /// NP service label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// From what position within the target range for obtaining rankings to do so
            /// </summary>
            public Int32 Offset { get; set; } = 0;

            /// <summary>
            /// Number of entries
            /// </summary>
            public Int32 Limit { get; set; } = 10;

            /// <summary>
            /// From which rank (where the player who scored first is ranked higher if there is a tie in terms of points) to start targeting for obtaining rankings
            /// </summary>
            public Int32 StartSerialRank { get; set; } = 1;

            /// <summary>
            /// Collection of target users, such as friends
            /// </summary>
            public Groups Group { get; set; } = Groups.NotSet;

            /// <summary>
            /// Specified set of users
            /// </summary>
            public List<UserInfo> Users { get; set; } = null;

            /// <summary>
            /// Ranking results will be centered around a given user. Use <see cref="UserCenteredAround"/> to set the number of users around the center ranking.
            /// </summary>
            public UserInfo UserCenteredAround { get; set; } = null;

            /// <summary>
            /// Maximum number of entries to obtain above and below the ranking of the specified user <see cref="UserCenteredAround"/> (where the player who scored first is ranked higher if players are tied in terms of points).
            /// This must be specified if <see cref="UserCenteredAround"/> is specified.
            /// </summary>
            public Int32 CenterToEdgeLimit { get; set; } = 0;

            /// <summary>
            /// Leaderboard ranking results
            /// </summary>
            public Rankings Rankings { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetRanking);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(BoardId);

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Writer.Write(Offset);
                nativeMethod.Writer.Write(Limit);
                nativeMethod.Writer.Write(StartSerialRank);
                nativeMethod.Writer.Write((Int32)Group);

                if(UserCenteredAround != null)
                {
                    nativeMethod.Writer.Write(true);
                    UserCenteredAround.Serialise(nativeMethod.Writer);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if(CenterToEdgeLimit > 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(CenterToEdgeLimit);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                int numUsers = Users != null ? Users.Count : 0;
                nativeMethod.Writer.Write(numUsers);

                if (numUsers > 0)
                {
                    for (int i = 0; i < numUsers; i++)
                    {
                        Users[i].Serialise(nativeMethod.Writer);
                    }
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Rankings = new Rankings();
                    Rankings.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Records scores
        /// </summary>
        public class GetLargeDataRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Object ID. Identifier for large-volume attached data.
            /// </summary>
            public string ObjectId { get; set; }

            /// <summary>
            /// NP service label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Scope of range download. RegEx Pattern: "bytes=\d+-\d+"
            /// </summary>
            public string Range { get; set; } = null;

            /// <summary>
            /// Used when downloading conditional data. The specified value is an ETag. RegEx Pattern: "(\*|\"[ -~]+\")"
            /// </summary>
            public string IfMatch { get; set; } = null;

            /// <summary>
            /// Downloaded large data
            /// </summary>
            public byte[] LargeData { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetLargeDateByObjectId);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.WritePrxString(ObjectId);

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (String.IsNullOrEmpty(Range) == false)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.WritePrxString(Range);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (String.IsNullOrEmpty(IfMatch) == false)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.WritePrxString(IfMatch);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    LargeData = nativeMethod.Reader.ReadData();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }
}
