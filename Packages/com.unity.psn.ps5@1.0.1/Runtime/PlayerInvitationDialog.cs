using System;
using System.Collections.Generic;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Dialogs;
using Unity.PSN.PS5.Internal;

#if UNITY_PS5
namespace Unity.PSN.PS5.Sessions
{
    /// <summary>
    /// Display invitation dialog
    /// </summary>
    public class InvitationDialogSystem
    {
        internal enum NativeMethods : UInt32
        {
            OpenDialog = 0x1600001u,
            UpdateDialog = 0x1600002u,
            CloseDialog = 0x1600003u,
        }

        internal static void Start()
        {
        }

        internal static void Stop()
        {
        }

        /// <summary>
        /// Open player invitation dialog
        /// </summary>
        public class OpenPlayerInvitationDialogRequest : Request
        {
            /// <summary>
            /// Invitation mode
            /// </summary>
            public enum InvitationModes
            {
                /// <summary> Invalid value</summary>
                Invalid = 0,
                /// <summary> Send Mode </summary>
                Send = 1,
            }

            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// The session id
            /// </summary>
            public string SessionId { get; set; }

            /// <summary>
            /// The type of Invitation mode
            /// </summary>
            public InvitationModes Mode { get; set; }


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

                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.WritePrxString(SessionId);
                nativeMethod.Writer.Write((Int32)Mode);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);
                    Status = DialogSystem.DialogStatus.Running;
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
}
#endif
