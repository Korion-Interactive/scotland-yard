using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Unity.PSN.PS5.Internal;
using Unity.PSN.PS5.Aysnc;
using UnityEngine;

namespace Unity.PSN.PS5.WebApi
{
    // A filter set consits of a set of filters
    // A user registers to one or more filter sets

    // Filter data type and extended keys

    /// <summary>
    /// Push event filter parameters
    /// </summary>
    public class WebApiFilter
    {
        /// <summary>
        /// Data type of Push event to receive
        /// </summary>
        /// <remarks>
        /// Typical values are:
        ///      "np:service:friendlist:friend"
        ///      "np:service:presence2:onlineStatus"
        ///      "np:service:blocklist"
        /// </remarks>
        public string DataType;

        /// <summary>
        /// List of extended data keys for Push event to receive
        /// </summary>
        /// <remarks>
        /// Typical values are:
        ///      "additionalTrigger" when paired with "np:service:friendlist:friend" in <see cref="DataType"/>
        /// </remarks>
        public List<string> ExtendedKeys = new List<string>();

        internal void Serialise(BinaryWriter writer)
        {
            writer.WritePrxString(DataType);

            writer.Write((UInt32)ExtendedKeys.Count);

            for (int i = 0; i < ExtendedKeys.Count; i++)
            {
                writer.WritePrxString(ExtendedKeys[i]);
            }
        }
    }

    /// <summary>
    /// A collection of <see cref="WebApiFilter"/>
    /// </summary>
    public class WebApiFilters
    {
        /// <summary>
        /// Invalid filter id
        /// </summary>
        public const Int32 InvalidFilterId = -1;

        /// <summary>
        /// The collection of filters to use
        /// </summary>
        public List<WebApiFilter> Filters { get; set; } = new List<WebApiFilter>();

        /// <summary>
        /// PSN Filter ID
        /// </summary>
        public Int32 PushFilterId { get; internal set; } = InvalidFilterId;

        /// <summary>
        /// Service label used when registering the filters
        /// </summary>
        public string ServiceName { get; set; } = null;

        /// <summary>
        /// Service Label used when register the filters
        /// </summary>
        public UInt32 ServiceLabel { get; set; } = UInt32.MaxValue;

        /// <summary>
        /// Number of PushEvents referencing these filters
        /// </summary>
        public UInt32 RefCount { get; internal set; } = 0;

        internal void IncRefCount() { RefCount = RefCount + 1; }
        internal void DecRefCount() { RefCount = RefCount - 1; }

        /// <summary>
        /// Have the filters been registered with the system
        /// </summary>
        public bool IsRegistered { get { return PushFilterId != InvalidFilterId; } }


        /// <summary>
        /// Add data type to the filter
        /// </summary>
        /// <param name="dataType">The data type string. See <see cref="WebApiFilter.DataType"/></param>
        /// <returns>The filter parameter created from the dataType</returns>
        public WebApiFilter AddFilterParam(string dataType)
        {
            WebApiFilter filter = FindFilter(dataType);

            if (filter != null) return filter;

            filter = new WebApiFilter() { DataType = dataType };
            Filters.Add(filter);

            return filter;
        }

        /// <summary>
        /// Add an array of data type strings
        /// </summary>
        /// <param name="dataTypes">The data type strings. See <see cref="WebApiFilter.DataType"/></param>
        public void AddFilterParams(string[] dataTypes)
        {
            for(int i = 0; i < dataTypes.Length; i++)
            {
                AddFilterParam(dataTypes[i]);
            }
        }

        /// <summary>
        /// Find a matching filter based on its dataType
        /// </summary>
        /// <param name="dataType">The data type string. See <see cref="WebApiFilter.DataType"/></param>
        /// <returns>The filter parameter matching the dataType or null</returns>
        public WebApiFilter FindFilter(string dataType)
        {
            if (Filters == null) return null;

            for(int i = 0; i < Filters.Count; i++)
            {
                if(Filters[i].DataType == dataType)
                {
                    return Filters[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Default filter constructor
        /// </summary>
        public WebApiFilters()
        {

        }

        /// <summary>
        /// Construct and initialise the filters
        /// </summary>
        /// <param name="dataTypes">Push Event Data Type</param>
        /// <param name="serviceName">NP service name</param>
        /// <param name="serviceLabel">NP service label</param>
        public WebApiFilters(string[] dataTypes, string serviceName = null, UInt32 serviceLabel = UInt32.MaxValue)
        {
            AddFilterParams(dataTypes);
            ServiceName = serviceName;
            ServiceLabel = serviceLabel;
        }

        internal void Serialise(BinaryWriter writer)
        {
            writer.Write(PushFilterId);
            writer.WritePrxString(ServiceName);
            writer.Write(ServiceLabel);

            writer.Write((UInt32)Filters.Count);

            for (int i = 0; i < Filters.Count; i++)
            {
                Filters[i].Serialise(writer);
            }
        }


    }

    /// <summary>
    /// A set of filters registered for a user
    /// These can also be Order Guaranteed push events.
    /// </summary>
    public class WebApiPushEvent
    {
        /// <summary>
        /// Invlaid callback id
        /// </summary>
        public const Int32 InvalidCallbackId = -1;

        /// <summary>
        /// User id associated with the events
        /// </summary>
        public Int32 UserId { get; set; }

        /// <summary>
        /// Set of Filters for the push event
        /// </summary>
        public WebApiFilters Filters { get; set; }

        /// <summary>
        /// The events order-guaranteed
        /// </summary>
        public bool OrderGuaranteed { get; set; }

        /// <summary>
        /// PSN Callback ID
        /// </summary>
        public Int32 PushCallbackId { get; internal set; } = InvalidCallbackId;

        /// <summary>
        /// Has the event been registered with the system
        /// </summary>
        public bool IsRegistered { get { return PushCallbackId != InvalidCallbackId; } }

        internal void Serialise(BinaryWriter writer)
        {
            writer.Write(PushCallbackId);
            writer.Write(UserId);
            writer.Write(OrderGuaranteed);
            writer.Write(Filters.PushFilterId);
        }
    }

    /// <summary>
    /// Control webapi filters and push notifications
    /// </summary>
    public class WebApiNotifications
    {
        [DllImport("__Internal")]
        private static extern void RegisterEventHandler(PrxCallbackEventHandler callback);

        enum NativeMethods : UInt32
        {
            RegisterFilter = 0x0600001u,
            UnregisterFilter = 0x0600002u,
            RegisterPushEvent = 0x0600003u,
            UnregisterPushEvent = 0x0600004u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("WebApiNotifications");

            Main.OnSystemUpdate += UpdateNotifications;

            RegisterEventHandler(EventCallback);
        }

        internal static void Stop()
        {
            Main.OnSystemUpdate -= UpdateNotifications;
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal WebApiNotifications queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Delegate for notifications about premium events.
        /// </summary>
        /// <param name="eventData">The notification event data</param>
        public delegate void NotificationEventHandler(CallbackParams eventData);

        private delegate void PrxCallbackEventHandler(IntPtr data, Int32 size);

        static Dictionary<Int32, NotificationEventHandler> callbacks = new Dictionary<int, NotificationEventHandler>();

        static Dictionary<Int32, WebApiFilters> activeFilters = new Dictionary<int, WebApiFilters>();
        static Dictionary<Int32, WebApiPushEvent> activePushEvents = new Dictionary<int, WebApiPushEvent>();

        /// <summary>
        /// Peer address of the user
        /// </summary>
        public class NpPeerAddress
        {
            /// <summary>
            /// The account id of the user
            /// </summary>
            public UInt64 AccountId { get; internal set; }

            /// <summary>
            /// The platform for the user
            /// </summary>
            public NpPlatformType Platform { get; internal set; }
        }

        /// <summary>
        /// The extended data attached to the notified Push event
        /// </summary>
        public struct ExtendedData
        {
            /// <summary>
            /// Data key
            /// </summary>
            public string Key { get; internal set; }

            /// <summary>
            /// Data value
            /// </summary>
            public string Data { get; internal set; }
        }

        /// <summary>
        ///
        /// </summary>
        public enum PushContextCallbackType
        {
            /// <summary> Unknown callback type </summary>
            Unknown = -1,
            /// <summary> Indicates a notification that a Push event was received without issue </summary>
            Received,
            /// <summary> indicates a notification that the lack of a Push event was detected. If this notification is received, Push events are not arriving in the correct order;
            /// therefore, you must follow the specifications of the Web API server to handle the issue appropriately with the application. </summary>
            Dropped,
        }

        /// <summary>
        /// The data for a recieved notification
        /// </summary>
        public class CallbackParams
        {
            /// <summary>
            /// User context ID
            /// </summary>
            public Int32 CtxId { get; internal set; }

            /// <summary>
            /// Callback ID
            /// </summary>
            public Int32 CallbackId { get; internal set; }

            /// <summary>
            /// Is the order of the events guaranteed
            /// </summary>
            public bool OrderGuaranteed { get; internal set; }

            /// <summary>
            /// NP service name
            /// </summary>
            public string NpServiceName { get; internal set; }

            /// <summary>
            /// NP service label
            /// </summary>
            public UInt32 NpServiceLabel { get; internal set; }

            /// <summary>
            /// Peer address of the user who is being notified of a Push event
            /// </summary>
            public NpPeerAddress To { get; internal set; }

            /// <summary>
            /// Online ID of the user who is being notified of a Push event
            /// </summary>
            public string ToOnlineId { get; internal set; }

            /// <summary>
            /// Peer address of the originating user of a Push event
            /// </summary>
            public NpPeerAddress From { get; internal set; }

            /// <summary>
            /// Online ID of the originating user of a Push event
            /// </summary>
            public string FromOnlineId { get; internal set; }

            /// <summary>
            /// Data type of the notified Push event
            /// </summary>
            public string DataType { get; internal set; }

            /// <summary>
            /// Data attached to the notified Push event
            /// </summary>
            public string Data { get; internal set; }

            /// <summary>
            /// Array containing the extended data attached to the notified Push event
            /// </summary>
            public ExtendedData[] ExtData { get; internal set; }

            /// <summary>
            /// The push context id if the push event is order-guaranteed <see cref="OrderGuaranteed"/>
            /// </summary>
            public string PushCtxId { get; internal set; }

            /// <summary>
            /// The push context callback type the push event is order-guaranteed <see cref="OrderGuaranteed"/>
            /// </summary>
            public PushContextCallbackType CCType { get; internal set; }
        };

        static string ReadStringFromIntPtr(Int64 strPtrValue, int length)
        {
            if (strPtrValue == 0 || length == 0)
            {
                return null;
            }

            IntPtr strPtr = new IntPtr(strPtrValue);

            byte[] strBytes = new byte[length];
            Marshal.Copy(strPtr, strBytes, 0, length);

            return Encoding.UTF8.GetString(strBytes, 0, strBytes.Length);
        }

        static NpPeerAddress ReadNpPeerAddress(BinaryReader reader)
        {
            bool hasPeer = reader.ReadBoolean();

            UInt64 accountId = reader.ReadUInt64();
            NpPlatformType platform = (NpPlatformType)reader.ReadInt32();

            if (hasPeer == false)
            {
                return null;
            }

            NpPeerAddress peerAddr = new NpPeerAddress();

            peerAddr.AccountId = accountId;
            peerAddr.Platform = platform;

            return peerAddr;
        }

        static ExtendedData[] ReadEventExtData(BinaryReader reader)
        {
            Int64 extDataPtrValue = reader.ReadInt64();
            int extDataNum = reader.ReadInt32();
            int extStructSize = reader.ReadInt32();

            ExtendedData[] extData = null;

            if (extDataNum > 0)
            {
                IntPtr extDataPtr = new IntPtr(extDataPtrValue);

                byte[] managedArray = new byte[extDataNum * extStructSize];
                Marshal.Copy(extDataPtr, managedArray, 0, managedArray.Length);

                MemoryStream stream = new MemoryStream(managedArray, 0, managedArray.Length, false);
                BinaryReader extReader = new BinaryReader(stream);

                extData = new ExtendedData[extDataNum];

                for (int i = 0; i < extData.Length; i++)
                {
                    // Read the extData for each entry
                    Int64 extKeyPtrValue = extReader.ReadInt64();
                    int extKeySize = extReader.ReadInt32();

                    extData[i].Key = ReadStringFromIntPtr(extKeyPtrValue, extKeySize);

                    Int64 dataPtrValue = extReader.ReadInt64();
                    int dataSize = extReader.ReadInt32();

                    extData[i].Data = ReadStringFromIntPtr(dataPtrValue, dataSize);
                }
            }

            return extData;
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(PrxCallbackEventHandler))]
        static void EventCallback(IntPtr rawPtr, Int32 size)
        {
            try
            {
                CallbackParams callbackData = new CallbackParams();

                byte[] managedArray = new byte[size];
                Marshal.Copy(rawPtr, managedArray, 0, size);

                MemoryStream stream = new MemoryStream(managedArray, 0, size, false);
                BinaryReader reader = new BinaryReader(stream);

                callbackData.CtxId = reader.ReadInt32();
                callbackData.CallbackId = reader.ReadInt32();

                callbackData.OrderGuaranteed = reader.ReadBoolean();

                Int64 serviceNamePtrValue = reader.ReadInt64();
                int serviceNameSize = reader.ReadInt32();

                callbackData.NpServiceName = ReadStringFromIntPtr(serviceNamePtrValue, serviceNameSize);

                callbackData.NpServiceLabel = reader.ReadUInt32();

                callbackData.To = ReadNpPeerAddress(reader);

                Int64 onlinePtrValue = reader.ReadInt64();
                int onlineIdSize = reader.ReadInt32();
                callbackData.ToOnlineId = ReadStringFromIntPtr(onlinePtrValue, onlineIdSize);

                callbackData.From = ReadNpPeerAddress(reader);

                onlinePtrValue = reader.ReadInt64();
                onlineIdSize = reader.ReadInt32();
                callbackData.FromOnlineId = ReadStringFromIntPtr(onlinePtrValue, onlineIdSize);

                Int64 dataTypePtrValue = reader.ReadInt64();
                int dataTypeSize = reader.ReadInt32();

                callbackData.DataType = ReadStringFromIntPtr(dataTypePtrValue, dataTypeSize);

                Int64 dataPtrValue = reader.ReadInt64();
                int dataSize = reader.ReadInt32();

                callbackData.Data = ReadStringFromIntPtr(dataPtrValue, dataSize);

                callbackData.ExtData = ReadEventExtData(reader);

                Int64 pushCtxIdPtrValue = reader.ReadInt64();
                int pushCtxIdLength = reader.ReadInt32();
                callbackData.PushCtxId = ReadStringFromIntPtr(pushCtxIdPtrValue, pushCtxIdLength);

                callbackData.CCType = (PushContextCallbackType)reader.ReadInt32();

                EnqueueCallbackParams(callbackData);

                //NotificationEventHandler handler = null;

                //if (callbacks.TryGetValue(callbackData.CallbackId, out handler))
                //{
                //    handler(callbackData);
                //}
            }
#pragma warning disable CS0168
            catch (Exception e)
#pragma warning restore CS0168
            {
#if DEBUG
                Debug.LogError("Exception in WebApi EventCallback :\n" + e.Message + "\n" + e.StackTrace);
#endif
            }
        }

        static void EnqueueCallbackParams(CallbackParams callbackData)
        {
            lock (pendingSyncObj)
            {
                pendingCallbackEvents.Enqueue(callbackData);
            }
        }

        static object pendingSyncObj = new object();
        static Queue<CallbackParams> pendingCallbackEvents = new Queue<CallbackParams>();

        static private void UpdateNotifications()
        {
            try
            {
                lock (pendingSyncObj)
                {
                    while (pendingCallbackEvents.Count > 0)
                    {
                        CallbackParams callbackData = pendingCallbackEvents.Dequeue();

                        NotificationEventHandler handler = null;

                     //   Debug.LogError("callbackData.CallbackId " + callbackData.CallbackId);
                        if (callbacks.TryGetValue(callbackData.CallbackId, out handler))
                        {
                            try
                            {
                                handler(callbackData);
                            }
#pragma warning disable CS0168
                            catch (Exception e)
#pragma warning restore CS0168
                            {
#if DEBUG
                                Debug.LogError("External UpdateNotification Handler :\n" + e.Message + "\n" + e.StackTrace);
#endif
                            }
                        }
                    }
                }
            }
#pragma warning disable CS0168
            catch (Exception e)
#pragma warning restore CS0168
            {
#if DEBUG
                Debug.LogError("UpdateNotifications :\n" + e.Message + "\n" + e.StackTrace);
#endif
            }
        }

        /// <summary>
        /// Get a list of registered WebApi filters
        /// </summary>
        /// <param name="filters"></param>
        static public void GetActiveFilters(List<WebApiFilters> filters)
        {
            filters.Clear();

            foreach(var filter in activeFilters)
            {
                filters.Add(filter.Value);
            }
        }

        /// <summary>
        /// Get a list of registered push events
        /// </summary>
        /// <param name="pushEvents"></param>
        [Obsolete("Use GetActivePushEvents instead")]
        static public void GetActivePashEvents(List<WebApiPushEvent> pushEvents)
        {
            pushEvents.Clear();

            foreach (var pushEvent in activePushEvents)
            {
                pushEvents.Add(pushEvent.Value);
            }
        }

        /// <summary>
        /// Get a list of registered push events
        /// </summary>
        /// <param name="pushEvents">The list to fill with active push events</param>
        static public void GetActivePushEvents(List<WebApiPushEvent> pushEvents)
        {
            pushEvents.Clear();

            foreach (var pushEvent in activePushEvents)
            {
                pushEvents.Add(pushEvent.Value);
            }
        }

        static internal void ActivateFilter(WebApiFilters Filters, Int32 filterId)
        {
            if (Filters.PushFilterId == WebApiFilters.InvalidFilterId)
            {
                Filters.PushFilterId = filterId;
                activeFilters.Add(Filters.PushFilterId, Filters);
            }
            else
            {
                // Validate the filterId matches the expected value
                if (Filters.PushFilterId != filterId)
                {
#if DEBUG
                    // This has gone wrong.
                    Debug.LogError("Push filter id has unexpected value. Current Value " + Filters.PushFilterId + " Expected " + filterId);
#endif
                }
            }
        }

        static internal void DeactivateFilter(WebApiFilters Filters)
        {
            if (Filters.PushFilterId != WebApiFilters.InvalidFilterId && Filters.RefCount == 0)
            {
                activeFilters.Remove(Filters.PushFilterId);

                // Read back the results from the native method
                Filters.PushFilterId = WebApiFilters.InvalidFilterId;
            }
        }

        static internal APIResult RegisterFilterCall(WebApiFilters filters)
        {
            APIResult result = new APIResult();

            if (filters.PushFilterId != WebApiFilters.InvalidFilterId)
            {
                return result;
            }

            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.RegisterFilter);

            // Write the data to match the expected format in the native code
            filters.Serialise(nativeMethod.Writer);

            nativeMethod.Call();

            result = nativeMethod.callResult;

            if (result.apiResult == APIResultTypes.Success)
            {
                // Read back the results from the native method
                Int32 filterId = nativeMethod.Reader.ReadInt32();

                ActivateFilter(filters, filterId);
            }

            MarshalMethods.ReleaseHandle(nativeMethod);

            return result;
        }

        /// <summary>
        /// Register Push event filter
        /// </summary>
        public class RegisterFilterRequest : Request
        {
            /// <summary>
            /// The filters to register
            /// </summary>
            public WebApiFilters Filters { get; set; }

            protected internal override void Run()
            {
                if (Filters.PushFilterId != WebApiFilters.InvalidFilterId)
                {
                    return;
                }

                Result = RegisterFilterCall(Filters);
            }
        }

        /// <summary>
        /// Unregister the push event filter
        /// </summary>
        public class UnregisterFilterRequest : Request
        {
            /// <summary>
            /// The filters to unregister
            /// </summary>
            public WebApiFilters Filters { get; set; }

            protected internal override void Run()
            {
                if(Filters.RefCount > 0)
                {
                    return;
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.UnregisterFilter);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write(Filters.PushFilterId);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    DeactivateFilter(Filters);
                }

                MarshalMethods.ReleaseHandle(nativeMethod);
            }
        }

        static internal void ActivatePushEvent(WebApiPushEvent pushEvent, Int32 callbackId, NotificationEventHandler callback)
        {
            if (pushEvent.PushCallbackId == WebApiPushEvent.InvalidCallbackId)
            {
                pushEvent.PushCallbackId = callbackId;

                pushEvent.Filters.IncRefCount();

                activePushEvents.Add(callbackId, pushEvent);
                callbacks.Add(pushEvent.PushCallbackId, callback);
            }
            else
            {
                // Validate the filterId matches the expected value
                if (pushEvent.PushCallbackId != callbackId)
                {
#if DEBUG
                    // This has gone wrong.
                    Debug.LogError("Push callback id has unexpected value.Current Value " + pushEvent.PushCallbackId + " Expected " + callbackId);
#endif
                }
            }
        }

        static internal void DeactivatePushEvent(WebApiPushEvent pushEvent)
        {
            if (pushEvent.PushCallbackId != WebApiPushEvent.InvalidCallbackId)
            {
                pushEvent.Filters.DecRefCount();

                activePushEvents.Remove(pushEvent.PushCallbackId);
                callbacks.Remove(pushEvent.PushCallbackId);

                pushEvent.PushCallbackId = WebApiPushEvent.InvalidCallbackId;
            }
        }

        static internal APIResult RegisterPushEventCall(WebApiPushEvent pushEvent, NotificationEventHandler Callback)
        {
            APIResult result = new APIResult();

            if (pushEvent.Filters.PushFilterId == WebApiFilters.InvalidFilterId)
            {
                result = RegisterFilterCall(pushEvent.Filters);

                if (result.apiResult != APIResultTypes.Success)
                {
                    return result;
                }
            }

            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.RegisterPushEvent);

            // Write the data to match the expected format in the native code
            pushEvent.Serialise(nativeMethod.Writer);

            nativeMethod.Call();

            result = nativeMethod.callResult;

            if (result.apiResult == APIResultTypes.Success)
            {
                // Read back the results from the native method
                Int32 callbackId = nativeMethod.Reader.ReadInt32();

                ActivatePushEvent(pushEvent, callbackId, Callback);
            }

            MarshalMethods.ReleaseHandle(nativeMethod);

            return result;
        }

        static internal APIResult UnregisterPushEventCall(WebApiPushEvent pushEvent)
        {
            APIResult result = new APIResult();

            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.UnregisterPushEvent);

            // Write the data to match the expected format in the native code
            nativeMethod.Writer.Write(pushEvent.PushCallbackId);

            nativeMethod.Call();

            result = nativeMethod.callResult;

            if (result.apiResult == APIResultTypes.Success)
            {
                DeactivatePushEvent(pushEvent);
            }

            MarshalMethods.ReleaseHandle(nativeMethod);

            return result;
        }

        /// <summary>
        /// Register a push event
        /// </summary>
        public class RegisterPushEventRequest : Request
        {
            /// <summary>
            /// The push event to register
            /// </summary>
            public WebApiPushEvent PushEvent { get; set; }

            /// <summary>
            /// The callback event to use
            /// </summary>
            public NotificationEventHandler Callback { get; set; }

            protected internal override void Run()
            {
                Result = RegisterPushEventCall(PushEvent, Callback);
            }
        }

        /// <summary>
        /// Unregister the push event
        /// </summary>
        public class UnregisterPushEventRequest : Request
        {
            /// <summary>
            /// The push event to unregister
            /// </summary>
            public WebApiPushEvent PushEvent { get; set; }

            protected internal override void Run()
            {
                Result = UnregisterPushEventCall(PushEvent);
            }
        }

        static internal APIResult CreatePushEventBlocking(int userId, WebApiFilters filters, NotificationEventHandler callback, bool orderGuaranteed, out WebApiPushEvent psuhEvent)
        {
            APIResult result = new APIResult();

            psuhEvent = new WebApiPushEvent();

            psuhEvent.UserId = userId;
            psuhEvent.Filters = filters;
            psuhEvent.OrderGuaranteed = true;

            //  Debug.LogError("RegisterPushEventCall");
            result = WebApiNotifications.RegisterPushEventCall(psuhEvent, callback);

            return result;
        }

        static internal APIResult DestroyPushEventBlocking(WebApiPushEvent psuhEvent)
        {
            APIResult result = WebApiNotifications.UnregisterPushEventCall(psuhEvent);

            return result;
        }
    }

}
