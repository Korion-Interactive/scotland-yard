using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sony
{
	namespace PS4
	{
		namespace SaveData
        {
            /// <summary>
            /// Main entry point to the SaveData plug-in and initialization
            /// </summary>
            public class Main
			{
				// Initialisation.
				[DllImport("__Internal")]
				private static extern void PrxSaveDataInitialize(out NativeInitResult nativeResult, out APIResult result);

				[DllImport("__Internal")]
				private static extern void PrxSaveDataTerminate(out APIResult result);


                // A global struct showing if SaveData has been initialised and the SDK version number for the native plugin.
                static internal InitResult initResult;

                /// <summary>
                /// Delegate event handler defining the callback event
                /// </summary>
                /// <param name="npEvent"></param>
                public delegate void EventHandler(SaveDataCallbackEvent npEvent);

                /// <summary>
                /// The event called when an async request has been completed or a notification
                /// </summary>
                public static event EventHandler OnAsyncEvent;

                /// <summary>
                /// Initialise the SaveData system
                /// </summary>
                /// <param name="initSettings">The initialisation paramaters.</param>
                /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the SaveData plug-in.</exception>
                public static InitResult Initialize(InitSettings initSettings)
				{
                    APIResult result;

                    NativeInitResult nativeResult = new NativeInitResult();

                    PrxSaveDataInitialize(out nativeResult, out result);

                    if (result.RaiseException == true) throw new SaveDataException(result);

                    initResult.Initialise(nativeResult);

                    ProcessQueueThread.Start(initSettings.affinity);
                    DispatchQueueThread.Start(initSettings.affinity);
                    Notifications.Start(initSettings.affinity);

                    return initResult;
                }

                /// <summary>
                /// Terminate the SaveData system
                /// </summary>
                public static void Terminate()
				{
                    APIResult result;

                    PrxSaveDataTerminate(out result);

                    if (result.RaiseException == true) throw new SaveDataException(result);

                    ProcessQueueThread.Stop();
                    DispatchQueueThread.Stop();
                    Notifications.Stop();
                }

                internal static void CallOnAsyncEvent(SaveDataCallbackEvent sdEvent)
                {
                    // do any internal management here
                    try
                    {
                        //ProcessInternalResponses(sdEvent);
                        OnAsyncEvent(sdEvent);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception Occured in OnAsyncEvent handler : " + e.Message);
                        Console.WriteLine(e.StackTrace);
                        throw;  // Throw the expection again as this shouldn't really hide the exception has occured.
                    }

                }

                internal static void ProcessInternalResponses(RequestBase request, ResponseBase response)
                {
                    if (request == null) return;

                    if (request.functionType == FunctionTypes.Mount)
                    {
                        if (response.ReturnCode == ReturnCodes.SUCCESS)
                        {
                            Mounting.MountResponse mr = response as Mounting.MountResponse;

                            Mounting.AddMountPoint(mr.MountPoint);
                        }
                    }
                    else if (request.functionType == FunctionTypes.Unmount)
                    {
                        if (response.ReturnCode == ReturnCodes.SUCCESS)
                        {
                            Mounting.UnmountRequest mr = request as Mounting.UnmountRequest;

                            Mounting.RemoveMountPoint(mr.MountPointName);
                        }
                    }
                }

                /// <summary>
                /// Abort a pending request. A pending request at the top of the list may not abort as processing the request may have already started.
                /// </summary>
                /// <param name="requestId">The request to abort.</param>
                /// <returns>Returns true is the request is in the pending list, otherwise returns false.</returns>
                /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the SaveData plug-in.</exception>
                public static bool AbortRequest(Int32 requestId)
                {
                    return ProcessQueueThread.AbortRequest(requestId);
                }

                /// <summary>
                /// Get the pending async requests list. This takes a copy of the list so it is safe to enumerate the list.
                /// </summary>
                /// <returns>A list of pending async requests.</returns>
                public static List<PendingRequest> GetPendingRequests()
                {
                    return ProcessQueueThread.PendingRequests;
                }
            }
        } // SaveData
    } // PS4
} // Sony
