using System;
using System.Collections.Generic;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Dialogs;
using Unity.PSN.PS5.Internal;

#if UNITY_PS5
namespace Unity.PSN.PS5.Matches
{
    /// <summary>
    /// Display matches dialog
    /// </summary>
    public class MatchesDialogSystem
    {
        internal enum NativeMethods : UInt32
        {
            OpenDialog = 0x0F00001u,
            UpdateDialog = 0x0F00002u,
            CloseDialog = 0x0F00003u,
        }

        internal static void Start()
        {
        }

        internal static void Stop()
        {
        }

        /// <summary>
        /// Open player review dialog
        /// </summary>
        public class OpenPlayerReviewDialogRequest : Request
        {
            /// <summary>
            /// Review mode
            /// </summary>
            public enum ReviewModes
            {
                /// <summary> Review all players </summary>
                ReviewAllPlayers = 0,
                /// <summary> Review only players on the same team </summary>
                ReviewTeamOnly = 1,
            }

            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// The match id
            /// </summary>
            public string MatchId { get; set; }

            /// <summary>
            /// The type of review mode
            /// </summary>
            public ReviewModes Mode { get; set; }


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
                nativeMethod.Writer.WritePrxString(MatchId);
                nativeMethod.Writer.Write((Int32)Mode);

                nativeMethod.Call();

                Result = nativeMethod.callResult;

                if (Result.apiResult == APIResultTypes.Success)
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);

                    Status = DialogSystem.DialogStatus.Running;

                    //// The dialog was opened sucessfully.
                    //// Now keep updating the dialog until it is closed or forcefully closed.
                    //bool continueUpdate = true;

                    //DialogSystem.DialogStatus currentStatus = DialogSystem.DialogStatus.None;

                    //while (continueUpdate)
                    //{
                    //    continueUpdate = !UpdateOpenedDialog(out currentStatus);

                    //    if (forceCloseDialog == true)
                    //    {
                    //        Result = ForceCloseDialog();
                    //        Status = DialogSystem.DialogStatus.ForceClosed;
                    //        return;
                    //    }
                    //}

                    //Status = currentStatus;
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
