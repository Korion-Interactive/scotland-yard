using System;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Dialogs;
using Unity.PSN.PS5.Internal;
using UnityEngine;

#if UNITY_PS5
namespace Unity.PSN.PS5.Commerce
{
    /// <summary>
    /// Display commerce dialog
    /// </summary>
    public class CommerceDialogSystem
    {
        internal enum NativeMethods : UInt32
        {
            OpenDialog = 0x0E00001u,
            UpdateDialog = 0x0E00002u,
            CloseDialog = 0x0E00003u,
        }

        internal enum NativeDialogMode : UInt32
        {
            Category = 0,
            Product = 1,
            ProductCode = 2,
            Checkout = 3,
            DownloadList = 4,
            Premium = 5
        }

        internal static void Start()
        {
        }

        internal static void Stop()
        {
        }

        /// <summary>
        /// Open Join premium dialog
        /// </summary>
        public class OpenJoinPremiumDialogRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }
            /// <summary>
            /// Targets
            /// </summary>
            public string[] Targets;
            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; }

            /// <summary>
            /// The Status of the dialog
            /// </summary>
            public DialogSystem.DialogStatus Status { get; internal set; } = DialogSystem.DialogStatus.None;

            /// <summary>
            /// Close the dialog when it is open or just before being opened.
            /// Call this if the request has already been scheduled.
            /// </summary>
            public void CloseDialog()
            {
                forceCloseDialog = true;
            }

            internal bool forceCloseDialog = false;

            /// <summary>
            /// Reset the request so it can be used again. Don't call this when the request has been scheduled.
            /// </summary>
            public void Reset()
            {
                forceCloseDialog = false;
                Status = DialogSystem.DialogStatus.None;
            }

            protected internal override void Run()
            {
                Status = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return;
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.OpenDialog);
                nativeMethod.Writer.Write((UInt32)NativeDialogMode.Premium);
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                nativeMethod.Writer.Write(0);   // num targets
                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);

                    Status = DialogSystem.DialogStatus.Running;

                    // The dialog was opened sucessfully.
                    // Now keep updating the dialog until it is closed or forcefully closed.
                    //MarshalMethods.ReleaseHandle(nativeMethod);

                }
                else
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);
                }
            }

            protected internal override bool HasUpdate()
            {
                return Status == DialogSystem.DialogStatus.Running;
            }

            protected internal override bool Update()
            {
                // Return true to finish update, false to continue
                DialogSystem.DialogStatus currentStatus = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Result = ForceCloseDialog();
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return true;
                }

                bool finished = UpdateOpenedDialog(out currentStatus);

                if (finished == true)
                {
                    Status = currentStatus;
                }

                return finished;
            }
        }

        /// <summary>
        /// Open Browse Category dialog
        /// </summary>
        public class OpenBrowseCategoryDialogRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }
            /// <summary>
            /// Targets
            /// </summary>
            public string[] Targets;
            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; }

            /// <summary>
            /// The Status of the dialog
            /// </summary>
            public DialogSystem.DialogStatus Status { get; internal set; } = DialogSystem.DialogStatus.None;

            /// <summary>
            /// Close the dialog when it is open or just before being opened.
            /// Call this if the request has already been scheduled.
            /// </summary>
            public void CloseDialog()
            {
                forceCloseDialog = true;
            }

            internal bool forceCloseDialog = false;

            /// <summary>
            /// Reset the request so it can be used again. Don't call this when the request has been scheduled.
            /// </summary>
            public void Reset()
            {
                forceCloseDialog = false;
                Status = DialogSystem.DialogStatus.None;
            }

            protected internal override void Run()
            {
                Status = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return;
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.OpenDialog);
                nativeMethod.Writer.Write((UInt32)NativeDialogMode.Category);
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);

                int numTargets = Targets != null ? Targets.Length : 0;
                nativeMethod.Writer.Write(numTargets);   // num targets
                for (int i = 0; i < numTargets; i++)
                {
                    nativeMethod.Writer.WritePrxString(Targets[i]);
                }
                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);

                    Status = DialogSystem.DialogStatus.Running;

                    // The dialog was opened sucessfully.
                    // Now keep updating the dialog until it is closed or forcefully closed.
                    //MarshalMethods.ReleaseHandle(nativeMethod);

                }
                else
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);
                }
            }

            protected internal override bool HasUpdate()
            {
                return Status == DialogSystem.DialogStatus.Running;
            }

            protected internal override bool Update()
            {
                // Return true to finish update, false to continue
                DialogSystem.DialogStatus currentStatus = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Result = ForceCloseDialog();
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return true;
                }

                bool finished = UpdateOpenedDialog(out currentStatus);

                if (finished == true)
                {
                    Status = currentStatus;
                }

                return finished;
            }
        }

        /// <summary>
        /// Open browse product dialog
        /// </summary>
        public class OpenBrowseProductDialogRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }
            /// <summary>
            /// Targets
            /// </summary>
            public string[] Targets;
            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; }

            /// <summary>
            /// The Status of the dialog
            /// </summary>
            public DialogSystem.DialogStatus Status { get; internal set; } = DialogSystem.DialogStatus.None;

            /// <summary>
            /// Close the dialog when it is open or just before being opened.
            /// Call this if the request has already been scheduled.
            /// </summary>
            public void CloseDialog()
            {
                forceCloseDialog = true;
            }

            internal bool forceCloseDialog = false;

            /// <summary>
            /// Reset the request so it can be used again. Don't call this when the request has been scheduled.
            /// </summary>
            public void Reset()
            {
                forceCloseDialog = false;
                Status = DialogSystem.DialogStatus.None;
            }

            protected internal override void Run()
            {
                Status = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return;
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.OpenDialog);
                nativeMethod.Writer.Write((UInt32)NativeDialogMode.Product);
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                int numTargets = Targets != null ? Targets.Length : 0;
                nativeMethod.Writer.Write(numTargets);   // num targets
                for (int i = 0; i < numTargets; i++)
                {
                    nativeMethod.Writer.WritePrxString(Targets[i]);
                }
                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);

                    Status = DialogSystem.DialogStatus.Running;

                    // The dialog was opened sucessfully.
                    // Now keep updating the dialog until it is closed or forcefully closed.
                    //MarshalMethods.ReleaseHandle(nativeMethod);

                }
                else
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);
                }
            }

            protected internal override bool HasUpdate()
            {
                return Status == DialogSystem.DialogStatus.Running;
            }

            protected internal override bool Update()
            {
                // Return true to finish update, false to continue
                DialogSystem.DialogStatus currentStatus = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Result = ForceCloseDialog();
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return true;
                }

                bool finished = UpdateOpenedDialog(out currentStatus);

                if (finished == true)
                {
                    Status = currentStatus;
                }

                return finished;
            }
        }

        /// <summary>
        /// Open redeem promotion code dialog
        /// </summary>
        public class OpenRedeemPromotionCodeDialogRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }
            /// <summary>
            /// Targets
            /// </summary>
            public string[] Targets;
            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; }

            /// <summary>
            /// The Status of the dialog
            /// </summary>
            public DialogSystem.DialogStatus Status { get; internal set; } = DialogSystem.DialogStatus.None;

            /// <summary>
            /// Close the dialog when it is open or just before being opened.
            /// Call this if the request has already been scheduled.
            /// </summary>
            public void CloseDialog()
            {
                forceCloseDialog = true;
            }

            internal bool forceCloseDialog = false;

            /// <summary>
            /// Reset the request so it can be used again. Don't call this when the request has been scheduled.
            /// </summary>
            public void Reset()
            {
                forceCloseDialog = false;
                Status = DialogSystem.DialogStatus.None;
            }

            protected internal override void Run()
            {
                Status = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return;
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.OpenDialog);
                nativeMethod.Writer.Write((UInt32)NativeDialogMode.ProductCode);
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                int numTargets = Targets != null ? Targets.Length : 0;
                nativeMethod.Writer.Write(numTargets);   // num targets
                for (int i = 0; i < numTargets; i++)
                {
                    nativeMethod.Writer.WritePrxString(Targets[i]);
                }
                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);

                    Status = DialogSystem.DialogStatus.Running;

                    // The dialog was opened sucessfully.
                    // Now keep updating the dialog until it is closed or forcefully closed.
                    //MarshalMethods.ReleaseHandle(nativeMethod);

                }
                else
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);
                }
            }

            protected internal override bool HasUpdate()
            {
                return Status == DialogSystem.DialogStatus.Running;
            }

            protected internal override bool Update()
            {
                // Return true to finish update, false to continue
                DialogSystem.DialogStatus currentStatus = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Result = ForceCloseDialog();
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return true;
                }

                bool finished = UpdateOpenedDialog(out currentStatus);

                if (finished == true)
                {
                    Status = currentStatus;
                }

                return finished;
            }
        }

        /// <summary>
        /// Open checkout dialog
        /// </summary>
        public class OpenCheckoutDialogRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }
            /// <summary>
            /// Targets
            /// </summary>
            public string[] Targets;
            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; }

            /// <summary>
            /// The Status of the dialog
            /// </summary>
            public DialogSystem.DialogStatus Status { get; internal set; } = DialogSystem.DialogStatus.None;

            /// <summary>
            /// Close the dialog when it is open or just before being opened.
            /// Call this if the request has already been scheduled.
            /// </summary>
            public void CloseDialog()
            {
                forceCloseDialog = true;
            }

            internal bool forceCloseDialog = false;

            /// <summary>
            /// Reset the request so it can be used again. Don't call this when the request has been scheduled.
            /// </summary>
            public void Reset()
            {
                forceCloseDialog = false;
                Status = DialogSystem.DialogStatus.None;
            }

            protected internal override void Run()
            {
                Status = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return;
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.OpenDialog);
                nativeMethod.Writer.Write((UInt32)NativeDialogMode.Checkout);
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                int numTargets = Targets != null ? Targets.Length : 0;
                nativeMethod.Writer.Write(numTargets);   // num targets
                for (int i = 0; i < numTargets; i++)
                {
                    nativeMethod.Writer.WritePrxString(Targets[i]);
                }
                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);

                    Status = DialogSystem.DialogStatus.Running;

                    // The dialog was opened sucessfully.
                    // Now keep updating the dialog until it is closed or forcefully closed.
                    //MarshalMethods.ReleaseHandle(nativeMethod);

                }
                else
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);
                }
            }

            protected internal override bool HasUpdate()
            {
                return Status == DialogSystem.DialogStatus.Running;
            }

            protected internal override bool Update()
            {
                // Return true to finish update, false to continue
                DialogSystem.DialogStatus currentStatus = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Result = ForceCloseDialog();
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return true;
                }

                bool finished = UpdateOpenedDialog(out currentStatus);

                if (finished == true)
                {
                    Status = currentStatus;
                }

                return finished;
            }
        }

        /// <summary>
        /// Open download dialog
        /// </summary>
        public class OpenDownloadDialogRequest : Request
        {
            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }
            /// <summary>
            /// Targets
            /// </summary>
            public string[] Targets;
            /// <summary>
            /// Service Label
            /// </summary>
            public UInt32 ServiceLabel { get; set; }

            /// <summary>
            /// The Status of the dialog
            /// </summary>
            public DialogSystem.DialogStatus Status { get; internal set; } = DialogSystem.DialogStatus.None;

            /// <summary>
            /// Close the dialog when it is open or just before being opened.
            /// Call this if the request has already been scheduled.
            /// </summary>
            public void CloseDialog()
            {
                forceCloseDialog = true;
            }

            internal bool forceCloseDialog = false;

            /// <summary>
            /// Reset the request so it can be used again. Don't call this when the request has been scheduled.
            /// </summary>
            public void Reset()
            {
                forceCloseDialog = false;
                Status = DialogSystem.DialogStatus.None;
            }

            protected internal override void Run()
            {
                Status = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return;
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.OpenDialog);
                nativeMethod.Writer.Write((UInt32)NativeDialogMode.DownloadList);
                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write(ServiceLabel);
                int numTargets = Targets != null ? Targets.Length : 0;
                nativeMethod.Writer.Write(numTargets);   // num targets
                for (int i = 0; i < numTargets; i++)
                {
                    nativeMethod.Writer.WritePrxString(Targets[i]);
                }
                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);

                    Status = DialogSystem.DialogStatus.Running;

                    // The dialog was opened sucessfully.
                    // Now keep updating the dialog until it is closed or forcefully closed.
                    //MarshalMethods.ReleaseHandle(nativeMethod);

                }
                else
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);
                }
            }

            protected internal override bool HasUpdate()
            {
                return Status == DialogSystem.DialogStatus.Running;
            }

            protected internal override bool Update()
            {
                // Return true to finish update, false to continue
                DialogSystem.DialogStatus currentStatus = DialogSystem.DialogStatus.None;

                if (forceCloseDialog == true)
                {
                    Result = ForceCloseDialog();
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return true;
                }

                bool finished = UpdateOpenedDialog(out currentStatus);

                if (finished == true)
                {
                    Status = currentStatus;
                }

                return finished;
            }
        }

        static APIResult ForceCloseDialog()
        {
            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.CloseDialog);

            nativeMethod.Call();

            APIResult result = nativeMethod.callResult;

            MarshalMethods.ReleaseHandle(nativeMethod);

            return result;
        }

        static bool UpdateOpenedDialog(out DialogSystem.DialogStatus status)
        {
            bool finished = false;
            status = DialogSystem.DialogStatus.None;

            MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.UpdateDialog);

            nativeMethod.Call();

            if (nativeMethod.callResult.apiResult == APIResultTypes.Success)
            {
                int sceStatus = nativeMethod.Reader.ReadInt32();

                if (sceStatus == 2) status = DialogSystem.DialogStatus.Running;

                bool hasFinished = nativeMethod.Reader.ReadBoolean();

                if (hasFinished == true)
                {
                    int sceDialogResult = nativeMethod.Reader.ReadInt32();

                    if (sceDialogResult == 0) status = DialogSystem.DialogStatus.FinishedOK;
                    else if (sceDialogResult == 1) status = DialogSystem.DialogStatus.FinishedCancel;
                    else if (sceDialogResult == 2) status = DialogSystem.DialogStatus.FinishedPurchased;

                    finished = true;
                }
            }

            MarshalMethods.ReleaseHandle(nativeMethod);

            return finished;
        }
    }

    public class CommerceSystem
    {
        internal enum NativeMethods : UInt32
        {
            PSStoreIcon = 0x0E00004u,
        }

        static WorkerThread workerQueue = new WorkerThread();

        internal static void Start()
        {
            workerQueue.Start("Commerce");
        }

        internal static void Stop()
        {
            workerQueue.Stop();
        }

        /// <summary>
        /// Schedule an <see cref="AsyncOp"/> by adding it to the internal FeatureGating queue
        /// </summary>
        /// <param name="op">The operation to schedule</param>
        /// <exception cref="ExceededMaximumOperations">The number of operation added to the queue has exceeded it limit. Too many operations have been added to the work queue.</exception>
        public static void Schedule(AsyncOp op)
        {
            workerQueue.Schedule(op);
        }

        /// <summary>
        /// Contol the PS Store Icon
        /// </summary>
        public class PSStoreIconRequest : Request
        {
            /// <summary>
            /// Display positions of the Store Icon
            /// </summary>
            public enum IconPositions : UInt32
            {
                /// <summary> Screen lower center  </summary>
                Center = 0,
                /// <summary> Screen lower left </summary>
                Left = 1,
                /// <summary> Screen lower right </summary>
                Right = 2,
            }

            /// <summary>
            /// Layout area type of the Store Icon
            /// </summary>
            public enum IconLayouts : UInt32
            {
                /// <summary> Fix the layout area to the entire screen (100%). </summary>
                Default = 0,
                /// <summary> Change the layout area pursuant to the safe area (90% to 100%). </summary>
                SafeArea = 1,
                /// <summary> Fix the layout area to 90% of the entire screen. </summary>
                FixedScale90Percent,
            }

            /// <summary>
            /// The display modes for the Store Icon
            /// </summary>
            public enum DisplayModes : UInt32
            {
                /// <summary> Shows the Store Icon </summary>
                Show = 0,
                /// <summary> Hides the Store Icon </summary>
                Hide = 1,
            }

            /// <summary>
            ///
            /// </summary>
            public DisplayModes Mode { get; set; }

            /// <summary>
            /// Set the position of the Store Icon
            /// </summary>
            public IconPositions IconPosition { get; set; }

            /// <summary>
            /// Sets the layout area of the Store Icon.
            /// </summary>
            public IconLayouts IconLayout { get; set; } = IconLayouts.Default;

            protected internal override void Run()
            {
                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.PSStoreIcon);

                // Write the data to match the expected format in the native code
                nativeMethod.Writer.Write((Int32)Mode);
                nativeMethod.Writer.Write((Int32)IconPosition);
                nativeMethod.Writer.Write((Int32)IconLayout);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                MarshalMethods.ReleaseHandle(nativeMethod);
            }

        }
    }
}
#endif
