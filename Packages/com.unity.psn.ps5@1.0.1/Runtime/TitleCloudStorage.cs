
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.TCS
{
    /// <summary>
    ///
    /// </summary>
    public class TitleCloudStorage
    {
        enum NativeMethods : UInt32
        {
            AddAndGetVariable = 0x1400001u,  // addAndGetVariable
            SetVariableWithConditions = 0x1400002u,    // setVariableWithConditions
            GetMultiVariablesBySlot = 0x1400003u, // getMultiVariablesBySlot
            SetMultiVariablesByUser = 0x1400004u, // getMultiVariablesBySlot
            GetMultiVariablesByUser = 0x1400005u, // getMultiVariablesBySlot
            DeleteMultiVariablesByUser = 0x1400006u, // getMultiVariablesBySlot

            UploadData = 0x1400007u,
            DownloadData = 0x1400008u,
            DeleteMultiDataBySlot = 0x140009u,
            DeleteMultiDataByUser = 0x140010u,
            GetMultiDataStatusesBySlot = 0x140011u,
            GetMultiDataStatusesByUser = 0x140012u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("TitleCloudStorage");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal OnlineSafety queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// TCS Online id and account id
        /// </summary>
        public class OnlineUserId
        {
            /// <summary>
            /// Account ID of the player
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// Online ID of the player
            /// </summary>
            public string OnlineId { get; set; }

            internal void Reset()
            {
                AccountId = 0;
                OnlineId = "";
            }

            internal void Deserialise(BinaryReader reader)
            {
                bool isAccountIdSet = reader.ReadBoolean();
                if (isAccountIdSet)
                {
                    string accIdStr = reader.ReadPrxString();
                    UInt64 tempId;
                    UInt64.TryParse(accIdStr, out tempId);
                    AccountId = tempId;
                }
                else
                {
                    AccountId = 0;
                }

                bool isOnlineIdSet = reader.ReadBoolean();
                if (isOnlineIdSet)
                {
                    OnlineId = reader.ReadPrxString();
                }
                else
                {
                    OnlineId = "";
                }
            }
        }

        /// <summary>
        /// TCS Variable
        /// </summary>
        public class Variable
        {
            /// <summary>
            /// The type of properties set in the variable
            /// </summary>
            [Flags]
            public enum SetProperties
            {
                /// <summary> Not set </summary>
                NotSet = 0,
                /// <summary> Owners of the slots set </summary>
                OwnerSet = 1,
                /// <summary> Slot ID set </summary>
                SlotIdSet = 2,
                /// <summary>  </summary>
                ValueSet = 4,
                /// <summary>  </summary>
                PrevValueSet = 8,
                /// <summary>  </summary>
                LastUpdatedDateTimeSet = 16,
                /// <summary>  </summary>
                LastUpdatedUserSet = 32,
            }

            /// <summary>
            /// Flags indicating which properties are set.
            /// </summary>
            public SetProperties ValidProperties { get; internal set; }

            /// <summary>
            /// Online IDs indicating the owner of the slot
            /// </summary>
            public OnlineUserId Owner { get; internal set; } = new OnlineUserId();

            /// <summary>
            /// Slot ID
            /// </summary>
            public Int32 SlotId { get; internal set; }

            /// <summary>
            /// 64-bit variable value
            /// </summary>
            public Int64 Value { get; internal set; }

            /// <summary>
            /// Previous 64-bit variable value before update
            /// </summary>
            public Int64 PreviousValue { get; internal set; }

            /// <summary>
            /// Time of the last update
            /// </summary>
            public DateTime LastUpdatedDateTime { get; internal set; }

            /// <summary>
            ///  Online ID that indicates the latest updater of the slot
            /// </summary>
            public OnlineUserId LastUpdatedUser { get; internal set; } = new OnlineUserId();

            internal void Reset()
            {
                ValidProperties = SetProperties.NotSet;
            }

            internal void Deserialise(BinaryReader reader)
            {
                ValidProperties = SetProperties.NotSet;

                bool isOwnerSet = reader.ReadBoolean();
                if (isOwnerSet)
                {
                    ValidProperties |= SetProperties.OwnerSet;
                    Owner.Deserialise(reader);
                }
                else
                {
                    Owner.Reset();
                }

                bool isSlotIdSet = reader.ReadBoolean();
                if (isSlotIdSet)
                {
                    ValidProperties |= SetProperties.SlotIdSet;
                    SlotId = reader.ReadInt32();
                }
                else
                {
                    SlotId = default;
                }

                bool isValueSet = reader.ReadBoolean();
                if (isValueSet)
                {
                    ValidProperties |= SetProperties.ValueSet;
                    Value = reader.ReadInt64();
                }
                else
                {
                    Value = 0;
                }

                bool isPrevValueSet = reader.ReadBoolean();
                if (isPrevValueSet)
                {
                    ValidProperties |= SetProperties.PrevValueSet;
                    PreviousValue = reader.ReadInt64();
                }
                else
                {
                    PreviousValue = 0;
                }

                bool isLastUpdatedDateTimeSet = reader.ReadBoolean();
                if (isLastUpdatedDateTimeSet)
                {
                    ValidProperties |= SetProperties.LastUpdatedDateTimeSet;
                    LastUpdatedDateTime = reader.ReadRtcTicks();
                }
                else
                {
                    LastUpdatedDateTime = default;
                }

                bool isLastUpdatedUserSet = reader.ReadBoolean();
                if (isLastUpdatedUserSet)
                {
                    ValidProperties |= SetProperties.LastUpdatedUserSet;
                    LastUpdatedUser.Deserialise(reader);
                }
                else
                {
                    LastUpdatedUser.Reset();
                }
            }
        }

        /// <summary>
        /// List of variables include limit and offset values
        /// </summary>
        public class VariableCollection
        {
            /// <summary>
            /// Number of records to obtain. Ignore if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 Limit { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Value of the the starting index for the array of records to obtain. Ignore if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 Offset { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Total number of TCS variables. Ignore if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 TotalVariableCount { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Retrieved variables
            /// </summary>
            public List<Variable> Variables { get; set; } = new List<Variable>(128);

            internal void Deserialise(BinaryReader reader)
            {
                Limit = Int32.MaxValue;
                Offset = Int32.MaxValue;
                if (Variables == null)
                {
                    Variables = new List<Variable>(128);
                }
                Variables.Clear();

                bool limitIsSet = reader.ReadBoolean();
                if (limitIsSet)
                {
                    Limit = reader.ReadInt32();
                }

                bool offsetIsSet = reader.ReadBoolean();
                if (offsetIsSet)
                {
                    Offset = reader.ReadInt32();
                }

                bool totalVariableCountIsSet = reader.ReadBoolean();
                if (totalVariableCountIsSet)
                {
                    TotalVariableCount = reader.ReadInt32();
                }

                Int32 numVars = reader.ReadInt32();

                for (int i = 0; i < numVars; i++)
                {
                    Variable newVar = new Variable();
                    newVar.Deserialise(reader);
                    Variables.Add(newVar);
                }
            }
        }

        /// <summary>
        /// Add a value to a TCS variable and read the result
        /// </summary>
        public class AddAndGetVariableRequest : Request
        {
            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountId"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// Slot Id
            /// </summary>
            public Int32 SlotId { get; set; }

            /// <summary>
            /// 64-bit variable value
            /// </summary>
            public Int64 Value { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Date and time for avoiding conflicts.
            /// </summary>
            public DateTime ComparedLastUpdatedDateTime { get; set; } = default;

            /// <summary>
            /// Updater account ID for preventing conflicts
            /// </summary>
            public UInt64 ComparedLastUpdatedUserAccountId { get; set; } = UInt64.MaxValue;

            /// <summary>
            /// The updated variable returned when the request succeeds.
            /// </summary>
            public Variable Variable { get; internal set; } = new Variable();

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.AddAndGetVariable);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                nativeMethod.Writer.Write(AccountId);

                nativeMethod.Writer.Write(SlotId);

                nativeMethod.Writer.Write(Value);

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ComparedLastUpdatedDateTime != default)
                {
                    nativeMethod.Writer.Write(true);
                    const Int64 sceToDotNetTicks = 10;   // sce ticks are microsecond, .net are 100 nanosecond

                    nativeMethod.Writer.Write(ComparedLastUpdatedDateTime.Ticks / sceToDotNetTicks);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ComparedLastUpdatedUserAccountId != UInt64.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ComparedLastUpdatedUserAccountId);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                Variable.Reset();

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    Variable.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Write to a TCS variable conditionally
        /// </summary>
        public class SetVariableWithConditionRequest : Request
        {
            /// <summary>
            /// Write condition
            /// </summary>
            public enum Conditions
            {
                /// <summary> Not set. </summary>
                NotSet = 0,
                /// <summary> Writes if <see cref="Value"/> is the same value as the existing value. </summary>
                Equal,
                /// <summary> Writes if <see cref="Value"/> is a different value than the existing value. </summary>
                NotEqual,
                /// <summary> Writes if <see cref="Value"/> is greater than the existing value. </summary>
                Greater,
                /// <summary> Writes if <see cref="Value"/> is greater than or equal to the existing value. </summary>
                GreaterOrEqual,
                /// <summary> Writes if <see cref="Value"/> is less than the existing value. </summary>
                Less,
                /// <summary> Writes if <see cref="Value"/> is less than or equal to the existing value. </summary>
                LessOrEqual,
            };

            /// <summary>
            /// The local user id to check
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountId"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// Slot id to update
            /// </summary>
            public Int32 SlotId { get; set; }

            /// <summary>
            /// Value to write
            /// </summary>
            public Int64 Value { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Date and time for avoiding conflicts.
            /// </summary>
            public DateTime ComparedLastUpdatedDateTime { get; set; } = default;

            /// <summary>
            /// Updater account ID for preventing conflicts
            /// </summary>
            public UInt64 ComparedLastUpdatedUserAccountId { get; set; } = UInt64.MaxValue;

            /// <summary>
            /// Write condition
            /// </summary>
            public Conditions Condition { get; set; } = Conditions.NotSet;

            /// <summary>
            /// The updated variable returned when the request succeeds.
            /// </summary>
            public Variable Variable { get; internal set; } = new Variable();

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.SetVariableWithConditions);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                nativeMethod.Writer.Write(AccountId);

                nativeMethod.Writer.Write(SlotId);

                nativeMethod.Writer.Write(Value);

                nativeMethod.Writer.Write((Int32)Condition);

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ComparedLastUpdatedDateTime != default)
                {
                    nativeMethod.Writer.Write(true);
                    const Int64 sceToDotNetTicks = 10;   // sce ticks are microsecond, .net are 100 nanosecond

                    nativeMethod.Writer.Write(ComparedLastUpdatedDateTime.Ticks / sceToDotNetTicks);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ComparedLastUpdatedUserAccountId != UInt64.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ComparedLastUpdatedUserAccountId);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                Variable.Reset();

                if (Result.apiResult == APIResultTypes.Success)
                {
                    // Read back the results from the native method
                    Variable.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }


        /// <summary>
        /// Collection of target users, such as friends.
        /// </summary>
        public enum Groups
        {
            /// <summary> No group target. </summary>
            NotSet = 0,
            /// <summary> Target friends. </summary>
            Friends = 1,
        }

        /// <summary>
        /// Sorting conditions for returned variable lists
        /// </summary>
        public enum SortVariableConditions
        {
            /// <summary> No sort condition. </summary>
            NotSet = 0,
            /// <summary> Sort by slot id. </summary>
            SlotId = 1,
            /// <summary> Sort by account id. </summary>
            AccountId = 2,
            /// <summary> Sort by date. </summary>
            Date = 3,
            /// <summary> Sort by value. </summary>
            Value = 4,
        }

        /// <summary>
        /// Sorting conditions for returned data lists
        /// </summary>
        public enum SortDataConditions
        {
            /// <summary> No sort condition. </summary>
            NotSet = 0,
            /// <summary> Sort by account id. </summary>
            AccountId = 1,
            /// <summary> Sort by data size. </summary>
            DataSize = 3,
            /// <summary> Sort by date. </summary>
            Date = 4,
        }

        /// <summary>
        /// Specification of descending or ascending order as the sorting condition for returned lists
        /// </summary>
        public enum SortModes
        {
            /// <summary> No order set. </summary>
            NotSet = 0,
            /// <summary> Ascending order. </summary>
            Ascending = 1,
            /// <summary> Descending order. </summary>
            Descending = 2,
        }



        /// <summary>
        /// Read TCS variables from multiple users.
        /// </summary>
        public class GetMultiVariablesBySlotRequest : Request
        {
            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountIds"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountIds"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public List<UInt64> AccountIds { get; set; }

            public Int32 SlotId { get; set; }

            /// <summary>
            /// Upper limit on the number of returned lists. Ignored if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 Limit { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Offset value for the pagination of returned lists. Ignored if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 Offset { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Collection of target users, such as friends.
            /// </summary>
            public Groups Group { get; set; } = Groups.NotSet;

            /// <summary>
            /// Sorting conditions for returned lists
            /// </summary>
            public SortVariableConditions SortCondition { get; set; } = SortVariableConditions.NotSet;

            /// <summary>
            /// Specification of descending or ascending order as the sorting condition for returned lists
            /// </summary>
            public SortModes SortMode { get; set; } = SortModes.NotSet;

            /// <summary>
            /// Service Label.
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Retrieved variables.
            /// </summary>
            public VariableCollection Variables { get; internal set; } = new VariableCollection();

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetMultiVariablesBySlot);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);

                int numIds = AccountIds != null ? AccountIds.Count : 0;

                nativeMethod.Writer.Write(numIds);

                for (int i = 0; i < numIds; i++)
                {
                    nativeMethod.Writer.Write(AccountIds[i]);
                }

                nativeMethod.Writer.Write(SlotId);

                if (Limit != Int32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(Limit);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (Offset != Int32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(Offset);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (Group != Groups.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((Int32)Group);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (SortCondition != SortVariableConditions.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((Int32)SortCondition);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (SortMode != SortModes.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((Int32)SortMode);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }


                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    if (Variables == null)
                    {
                        Variables = new VariableCollection();
                    }

                    Variables.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Variable data to write
        /// </summary>
        public class VariableToWrite
        {
            /// <summary>
            ///  Slot Id to update.
            /// </summary>
            public Int32 SlotId;

            /// <summary>
            /// Value to write.
            /// </summary>
            public Int64 Value;
        }

        /// <summary>
        /// Write TCS variables.
        /// </summary>
        public class SetMultiVariablesByUserRequest : Request
        {
            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title. If true used in preference to <see cref="AccountId"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// List of variables to write
            /// </summary>
            public List<VariableToWrite> Variables { get; set; }

            /// <summary>
            /// Service Label.
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;


            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.SetMultiVariablesByUser);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                nativeMethod.Writer.Write(AccountId);

                int numVars = 0;

                if (Variables != null)
                {
                    numVars = Variables.Count;
                }

                nativeMethod.Writer.Write(numVars);

                for (int i = 0; i < numVars; i++)
                {
                    nativeMethod.Writer.Write(Variables[i].SlotId);
                    nativeMethod.Writer.Write(Variables[i].Value);
                }

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Read TCS variables.
        /// </summary>
        public class GetMultiVariablesByUserRequest : Request
        {
            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>.
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountId"/>.
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// Multiple slot ids can be specified
            /// </summary>
            public List<Int32> SlotIds { get; set; }

            /// <summary>
            /// Upper limit on the number of returned lists. Ignored if value set to <see cref="Int32.MaxValue"/>.
            /// </summary>
            public Int32 Limit { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Offset value for the pagination of returned lists. Ignored if value set to <see cref="Int32.MaxValue"/>.
            /// </summary>
            public Int32 Offset { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Sorting conditions for returned lists.
            /// </summary>
            public SortVariableConditions SortCondition { get; set; } = SortVariableConditions.NotSet;

            /// <summary>
            /// Specification of descending or ascending order as the sorting condition for returned lists.
            /// </summary>
            public SortModes SortMode { get; set; } = SortModes.NotSet;

            /// <summary>
            /// Service Label.
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Retrieved variables.
            /// </summary>
            public VariableCollection Variables { get; internal set; } = new VariableCollection();

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetMultiVariablesByUser);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                nativeMethod.Writer.Write(AccountId);

                int numSlots = 0;

                if (SlotIds != null)
                {
                    numSlots = SlotIds.Count;
                }

                nativeMethod.Writer.Write(numSlots);

                for (int i = 0; i < numSlots; i++)
                {
                    nativeMethod.Writer.Write(SlotIds[i]);
                }

                if (Limit != Int32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(Limit);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (Offset != Int32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(Offset);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (SortCondition != SortVariableConditions.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((Int32)SortCondition);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (SortMode != SortModes.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((Int32)SortMode);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }


                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    Variables.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Delete TCS variables
        /// </summary>
        public class DeleteMultiVariablesByUserRequest : Request
        {
            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountId"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// Multiple slot ids can be specified
            /// </summary>
            public List<Int32> SlotIds { get; set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeleteMultiVariablesByUser);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                nativeMethod.Writer.Write(AccountId);

                int numSlots = 0;

                if (SlotIds != null)
                {
                    numSlots = SlotIds.Count;
                }

                nativeMethod.Writer.Write(numSlots);

                for (int i = 0; i < numSlots; i++)
                {
                    nativeMethod.Writer.Write(SlotIds[i]);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Write TCS data
        /// </summary>
        public class UploadDataRequest : Request
        {
            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountId"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// Slot id
            /// </summary>
            public Int32 SlotId { get; set; }

            /// <summary>
            /// Data to upload
            /// </summary>
            public byte[] Data { get; set; }

            /// <summary>
            /// Write supplementary data for TCS data
            /// </summary>
            public byte[] Info { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Date and time for avoiding conflicts.
            /// </summary>
            public DateTime ComparedLastUpdatedDateTime { get; set; } = default;

            /// <summary>
            /// Updater account ID for preventing conflicts.
            /// </summary>
            public UInt64 ComparedLastUpdatedUserAccountId { get; set; } = UInt64.MaxValue;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.UploadData);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                nativeMethod.Writer.Write(AccountId);

                nativeMethod.Writer.Write(SlotId);

                int dataSize = Data != null ? Data.Length : 0;
                nativeMethod.Writer.Write(dataSize);

                GCHandle pinnedDataArray = default;

                if (dataSize > 0)
                {
                    pinnedDataArray = GCHandle.Alloc(Data, GCHandleType.Pinned);
                    IntPtr dataPointer = pinnedDataArray.AddrOfPinnedObject();

                    nativeMethod.Writer.Write(dataPointer.ToInt64());
                }

                int infoSize = Info != null ? Info.Length : 0;
                nativeMethod.Writer.Write(infoSize);
                if (infoSize > 0)
                {
                    nativeMethod.Writer.Write(Info);
                }

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ComparedLastUpdatedDateTime != default)
                {
                    nativeMethod.Writer.Write(true);
                    const Int64 sceToDotNetTicks = 10;   // sce ticks are microsecond, .net are 100 nanosecond

                    nativeMethod.Writer.Write(ComparedLastUpdatedDateTime.Ticks / sceToDotNetTicks);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ComparedLastUpdatedUserAccountId != UInt64.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ComparedLastUpdatedUserAccountId);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {

                }

                if (dataSize > 0)
                {
                    pinnedDataArray.Free();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        /// <summary>
        /// Scope of range download
        /// </summary>
        public class Range
        {
            /// <summary>
            /// Min bytes range
            /// </summary>
            public Int32 MinBytes { get; set; }

            /// <summary>
            /// Max bytes range
            /// </summary>
            public Int32 MaxBytes { get; set; }
        }

        /// <summary>
        /// Read TCS data
        /// </summary>
        public class DownloadDataRequest : Request
        {
            static readonly UInt32 MaxDataBufferSize = (1024 * 1024 * 4);

            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountId"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// The slot id to download
            /// </summary>
            public Int32 SlotId { get; set; }

            /// <summary>
            /// Data object ID
            /// </summary>
            public string ObjectId { get; set; }

            /// <summary>
            /// Scope of range download
            /// </summary>
            public Range Range { get; set; } = null;

            /// <summary>
            /// Use during conditional data downloads. The specified value is an ETag. RegEx Pattern: "(\*|\"[ -~]+\")"
            /// </summary>
            public string IfMatch { get; set; } = null;

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// The array to recieve the read TCS data. This buffer should be made large enough to recieve the data block <see cref="DataStatus.DataSize"/>.
            /// </summary>
            public byte[] DownloadData { get; set; } = new byte[MaxDataBufferSize];

            /// <summary>
            /// The number of bytes written into the <see cref="DownloadData"/> buffer.
            /// </summary>
            public UInt64 DownloadDataSize { get; internal set; }

            /// <summary>
            /// The total number of bytes available from cloud storage. This value might be greater than the <see cref="DownloadDataSize"/> if the
            /// <see cref="DownloadData"/> buffer isn't large enough to contain all the data.
            /// </summary>
            public UInt64 TotalBytesRead { get; internal set; }

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DownloadData);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                nativeMethod.Writer.Write(AccountId);

                nativeMethod.Writer.Write(SlotId);

                nativeMethod.Writer.WritePrxString(ObjectId);

                if (Range != null)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.WritePrxString("bytes=");
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (IfMatch != null && IfMatch.Length > 0)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.WritePrxString(IfMatch);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                DownloadDataSize = 0;
                TotalBytesRead = 0;

                UInt32 bufferSize = DownloadData != null ? (UInt32)DownloadData.Length : 0;

                nativeMethod.Writer.Write(bufferSize);

                GCHandle pinnedDownloadArray = default;

                if (bufferSize > 0)
                {
                    pinnedDownloadArray = GCHandle.Alloc(DownloadData, GCHandleType.Pinned);
                    IntPtr dataPointer = pinnedDownloadArray.AddrOfPinnedObject();

                    nativeMethod.Writer.Write(dataPointer.ToInt64());
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    DownloadDataSize = nativeMethod.Reader.ReadUInt64();
                    TotalBytesRead = nativeMethod.Reader.ReadUInt64();
                }

                if (bufferSize > 0)
                {
                    pinnedDownloadArray.Free();
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

        }

        /// <summary>
        /// Delete TCS data from multiple users
        /// </summary>
        public class DeleteMultiDataBySlotRequest : Request
        {
            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountId"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public List<UInt64> AccountIds { get; set; }

            /// <summary>
            /// The slot id to download
            /// </summary>
            public Int32 SlotId { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeleteMultiDataBySlot);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                int numIds = AccountIds != null ? AccountIds.Count : 0;

                nativeMethod.Writer.Write(numIds);

                for (int i = 0; i < numIds; i++)
                {
                    nativeMethod.Writer.Write(AccountIds[i]);
                }

                nativeMethod.Writer.Write(SlotId);

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

        }


        /// <summary>
        /// Delete TCS data from multiple slots belonging to the specified user
        /// </summary>
        public class DeleteMultiDataByUserRequest : Request
        {
            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountId"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// Multiple slot ids can be specified
            /// </summary>
            public List<Int32> SlotIds { get; set; }

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.DeleteMultiDataByUser);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                nativeMethod.Writer.Write(AccountId);

                int numSlots = 0;

                if (SlotIds != null)
                {
                    numSlots = SlotIds.Count;
                }

                nativeMethod.Writer.Write(numSlots);

                for (int i = 0; i < numSlots; i++)
                {
                    nativeMethod.Writer.Write(SlotIds[i]);
                }

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

        }



        /// <summary>
        /// TCS data status
        /// </summary>
        public class DataStatus
        {
            /// <summary>
            /// Data status output filter conditions
            /// </summary>
            [Flags]
            public enum Filters
            {
                /// <summary> Not set </summary>
                NotSet = 0,
                /// <summary> Data size. </summary>
                DataSize = 1,
                /// <summary> Supplementary data. </summary>
                Info = 2,
                /// <summary> Date and time of last update. </summary>
                LastUpdatedDateTime = 4,
                /// <summary> User information about the last updater. </summary>
                LastUpdatedUser = 8,
                /// <summary> Object ID. </summary>
                ObjectId = 16,
                /// <summary> Owner.  </summary>
                Owner = 32,
                /// <summary> SlotId.  </summary>
                SlotId = 64,
            }

            private static string[] s_DataStatusFieldNames = { "dataSize", "info", "lastUpdatedDateTime", "lastUpdatedUser", "objectId", "owner", "slotId" };

            internal static String GetFilterStr(Filters filters)
            {
                if (filters == Filters.NotSet) return null;

                string fields = "";

                // Write a string of filters
                int flag = 1;
                for (int i = 0; i < s_DataStatusFieldNames.Length; i++)
                {
                    if (filters.HasFlag((Filters)flag))
                    {
                        if (fields.Length > 0) fields += ",";
                        fields += s_DataStatusFieldNames[i];
                    }

                    flag = flag << 1;
                }

                return fields;
            }

            /// <summary>
            /// Flags indicating which properties are set.
            /// </summary>
            public Filters ValidFields { get; internal set; }

            /// <summary>
            /// Online IDs indicating the owner of the slot
            /// </summary>
            public OnlineUserId Owner { get; internal set; } = new OnlineUserId();

            /// <summary>
            /// Slot ID
            /// </summary>
            public Int32 SlotId { get; internal set; }

            /// <summary>
            /// Data size
            /// </summary>
            public Int64 DataSize { get; internal set; }

            /// <summary>
            /// Supplementary data. A maximum of 512 bytes of arbitrary.
            /// </summary>
            public byte[] Info { get; internal set; }

            /// <summary>
            /// Data object ID.
            /// </summary>
            public string ObjectId { get; internal set; }

            /// <summary>
            /// Time of the last update.
            /// </summary>
            public DateTime LastUpdatedDateTime { get; internal set; }

            /// <summary>
            ///  Online ID that indicates the latest updater of the slot.
            /// </summary>
            public OnlineUserId LastUpdatedUser { get; internal set; } = new OnlineUserId();

            internal void Reset()
            {
                ValidFields = Filters.NotSet;
            }

            internal void Deserialise(BinaryReader reader)
            {
               // ValidProperties = SetProperties.NotSet;

                bool isOwnerSet = reader.ReadBoolean();
                if (isOwnerSet)
                {
                    ValidFields |= Filters.Owner;
                    Owner.Deserialise(reader);
                }
                else
                {
                    Owner.Reset();
                }

                bool isSlotIdSet = reader.ReadBoolean();
                if (isSlotIdSet)
                {
                    ValidFields |= Filters.SlotId;
                    SlotId = reader.ReadInt32();
                }
                else
                {
                    SlotId = default;
                }

                bool isDataSizeSet = reader.ReadBoolean();
                if (isDataSizeSet)
                {
                    ValidFields |= Filters.DataSize;
                    DataSize = reader.ReadInt64();
                }
                else
                {
                    DataSize = 0;
                }

                bool isInfoSetSet = reader.ReadBoolean();
                if (isInfoSetSet)
                {
                    ValidFields |= Filters.Info;
                    Info = reader.ReadData();
                }
                else
                {
                    Info = null;
                }

                bool isObjectIdSet = reader.ReadBoolean();
                if (isObjectIdSet)
                {
                    ValidFields |= Filters.ObjectId;
                    ObjectId = reader.ReadPrxString();
                }
                else
                {
                    ObjectId = null;
                }

                bool isLastUpdatedDateTimeSet = reader.ReadBoolean();
                if (isLastUpdatedDateTimeSet)
                {
                    ValidFields |= Filters.LastUpdatedDateTime;
                    LastUpdatedDateTime = reader.ReadRtcTicks();
                }
                else
                {
                    LastUpdatedDateTime = default;
                }

                bool isLastUpdatedUserSet = reader.ReadBoolean();
                if (isLastUpdatedUserSet)
                {
                    ValidFields |= Filters.LastUpdatedUser;
                    LastUpdatedUser.Deserialise(reader);
                }
                else
                {
                    LastUpdatedUser.Reset();
                }
            }
        }

        /// <summary>
        /// List of Data status include limit and offset values
        /// </summary>
        public class DataStatusCollection
        {
            /// <summary>
            /// Number of records to obtain. Ignore if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 Limit { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Value of the the starting index for the array of records to obtain. Ignore if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 Offset { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Total number of record arrays that match the conditions. Ignore if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 TotalDataStatusCount { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Retrieved variables
            /// </summary>
            public List<DataStatus> Statuses { get; set; } = new List<DataStatus>(128);

            internal void Deserialise(BinaryReader reader)
            {
                Limit = Int32.MaxValue;
                Offset = Int32.MaxValue;
                TotalDataStatusCount = Int32.MaxValue;
                if (Statuses == null)
                {
                    Statuses = new List<DataStatus>(128);
                }
                Statuses.Clear();

                bool limitIsSet = reader.ReadBoolean();
                if (limitIsSet)
                {
                    Limit = reader.ReadInt32();
                }

                bool offsetIsSet = reader.ReadBoolean();
                if (offsetIsSet)
                {
                    Offset = reader.ReadInt32();
                }

                bool totalDataStatusCountIsSet = reader.ReadBoolean();
                if (totalDataStatusCountIsSet)
                {
                    TotalDataStatusCount = reader.ReadInt32();
                }

                Int32 numStatuses = reader.ReadInt32();

                for (int i = 0; i < numStatuses; i++)
                {
                    DataStatus newStatus = new DataStatus();
                    newStatus.Deserialise(reader);
                    Statuses.Add(newStatus);
                }
            }
        }

        /// <summary>
        /// Read TCS data statuses from multiple users
        /// </summary>
        public class GetMultiDataStatusesBySlotRequest : Request
        {
            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountId"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public List<UInt64> AccountIds { get; set; }

            /// <summary>
            /// The slot id to download
            /// </summary>
            public Int32 SlotId { get; set; }

            /// <summary>
            /// Option filters to control which attributes to return
            /// </summary>
            public DataStatus.Filters RequireFields { get; set; } = DataStatus.Filters.NotSet;

            /// <summary>
            /// Upper limit on the number of returned lists. Ignored if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 Limit { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Offset value for the pagination of returned lists. Ignored if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 Offset { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Collection of target users, such as friends.
            /// </summary>
            public Groups Group { get; set; } = Groups.NotSet;

            /// <summary>
            /// Sorting conditions for returned lists
            /// </summary>
            public SortDataConditions SortCondition { get; set; } = SortDataConditions.NotSet;

            /// <summary>
            /// Specification of descending or ascending order as the sorting condition for returned lists
            /// </summary>
            public SortModes SortMode { get; set; } = SortModes.NotSet;

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Retrieved data status.
            /// </summary>
            public DataStatusCollection Statuses { get; internal set; } = new DataStatusCollection();

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetMultiDataStatusesBySlot);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                int numIds = AccountIds != null ? AccountIds.Count : 0;

                nativeMethod.Writer.Write(numIds);

                for (int i = 0; i < numIds; i++)
                {
                    nativeMethod.Writer.Write(AccountIds[i]);
                }

                nativeMethod.Writer.Write(SlotId);

                if (RequireFields != DataStatus.Filters.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    string fields = DataStatus.GetFilterStr(RequireFields);
                    nativeMethod.Writer.WritePrxString(fields);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (Limit != Int32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(Limit);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (Offset != Int32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(Offset);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (Group != Groups.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((Int32)Group);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (SortCondition != SortDataConditions.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((Int32)SortCondition);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (SortMode != SortModes.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((Int32)SortMode);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    if (Statuses == null)
                    {
                        Statuses = new DataStatusCollection();
                    }

                    Statuses.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

        }

        /// <summary>
        /// Read TCS data statuses from multiple slots for the specified user
        /// </summary>
        public class GetMultiDataStatusesByUserRequest : Request
        {
            /// <summary>
            /// The local user id to check.
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// Indicates the current user. If true used in preference to <see cref="Anyone"/> or <see cref="AccountId"/>
            /// </summary>
            public bool Me { get; set; }

            /// <summary>
            /// Indicates all users of the title.  If true used in preference to <see cref="AccountId"/>
            /// </summary>
            public bool Anyone { get; set; }

            /// <summary>
            /// Account ID of the a player. Only used if <see cref="Me"/> and <see cref="Anyone"/> are false.
            /// </summary>
            public UInt64 AccountId { get; set; }

            /// <summary>
            /// Multiple slot ids can be specified
            /// </summary>
            public List<Int32> SlotIds { get; set; }

            /// <summary>
            /// Option filters to control which attributes to return
            /// </summary>
            public DataStatus.Filters RequireFields { get; set; } = DataStatus.Filters.NotSet;

            /// <summary>
            /// Upper limit on the number of returned lists. Ignored if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 Limit { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Offset value for the pagination of returned lists. Ignored if value set to <see cref="Int32.MaxValue"/>
            /// </summary>
            public Int32 Offset { get; set; } = Int32.MaxValue;

            /// <summary>
            /// Sorting conditions for returned lists
            /// </summary>
            public SortDataConditions SortCondition { get; set; } = SortDataConditions.NotSet;

            /// <summary>
            /// Specification of descending or ascending order as the sorting condition for returned lists
            /// </summary>
            public SortModes SortMode { get; set; } = SortModes.NotSet;

            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

            /// <summary>
            /// Retrieved data status.
            /// </summary>
            public DataStatusCollection Statuses { get; internal set; } = new DataStatusCollection();

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.GetMultiDataStatusesByUser);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(UserId);

                nativeMethod.Writer.Write(Me);
                nativeMethod.Writer.Write(Anyone);
                nativeMethod.Writer.Write(AccountId);

                int numSlots = 0;

                if (SlotIds != null)
                {
                    numSlots = SlotIds.Count;
                }

                nativeMethod.Writer.Write(numSlots);

                for (int i = 0; i < numSlots; i++)
                {
                    nativeMethod.Writer.Write(SlotIds[i]);
                }

                if (RequireFields != DataStatus.Filters.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    string fields = DataStatus.GetFilterStr(RequireFields);
                    nativeMethod.Writer.WritePrxString(fields);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (Limit != Int32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(Limit);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (Offset != Int32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(Offset);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (SortCondition != SortDataConditions.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((Int32)SortCondition);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (SortMode != SortModes.NotSet)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write((Int32)SortMode);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                if (ServiceLabel != UInt32.MaxValue)
                {
                    nativeMethod.Writer.Write(true);
                    nativeMethod.Writer.Write(ServiceLabel);
                }
                else
                {
                    nativeMethod.Writer.Write(false);
                }

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    if (Statuses == null)
                    {
                        Statuses = new DataStatusCollection();
                    }

                    Statuses.Deserialise(nativeMethod.Reader);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }
    }
}
