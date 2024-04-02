using System;
using System.Collections.Generic;
using System.IO;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Internal;

namespace Unity.PSN.PS5.Dialogs
{
    /// <summary>
    /// Display message dialog
    /// </summary>
    public class MessageDialogSystem
    {
        internal enum NativeMethods : UInt32
        {
            OpenDialog = 0x1000001u,
            UpdateDialog = 0x1000002u,
            CloseDialog = 0x1000003u,
        }

        internal static void Start()
        {
        }

        internal static void Stop()
        {
        }

        /// <summary>
        /// Application specified message display mode parameters
        /// </summary>
        public struct UserMsgParams
        {
            /// <summary>
            /// Types of button on dialog
            /// </summary>
            public enum ButtonTypes
            {
                /// <summary> Displays 1 button, the OK button </summary>
                Ok = 0,
                /// <summary> Displays 2 buttons, the Yes button and the No button. Initial focus is on the Yes button </summary>
                YesNo = 1,
                /// <summary> Does not display buttons </summary>
                None = 2,
                /// <summary> Displays 2 buttons, the OK button and the Cancel button. Initial focus is on the OK button </summary>
                OkCancel = 3,
                /// <summary> Message dialog with an indicator showing that processing is being performed </summary>
                Wait = 5,
                /// <summary> Displays both the Cancel button and an indicator showing that processing is being performed </summary>
                WaitCancel = 6,
                /// <summary> Displays 2 buttons, the Yes button and the No button. Initial focus is on the No button</summary>
                YesNoFocusNo = 7,
                /// <summary> Displays 2 buttons, the OK button and the Cancel button. Initial focus is on the Cancel button </summary>
                OkCancelFocusCancel = 8,
                /// <summary> Displays 2 buttons with application specified strings </summary>
                CustomButtons = 9
            }

            /// <summary>
            /// Type of button set
            /// </summary>
            public ButtonTypes BtnType { get; set; }

            /// <summary>
            /// Displayed message
            /// </summary>
            public string Msg { get; set; }

            /// <summary>
            /// Message for button 1. Only used when <see cref="BtnType"/> is <see cref="ButtonTypes.CustomButtons"/>
            /// </summary>
            /// <remarks>
            /// Initial focus is on CustomBtn1, which is positioned on the right side of the screen. When allocating an assenting choice and a dissenting choice to the two buttons, allocate the assenting choice to Button1 and the dissenting choice to Button2.
            /// </remarks>
            public string CustomBtn1 { get; set; }

            /// <summary>
            /// Message for button 2.  Only used when <see cref="BtnType"/> is <see cref="ButtonTypes.CustomButtons"/>
            /// </summary>
            /// <remarks>
            /// Initial focus is on CustomBtn2, which is positioned on the right side of the screen. When allocating an assenting choice and a dissenting choice to the two buttons, allocate the assenting choice to Button1 and the dissenting choice to Button2.
            /// </remarks>
            public string CustomBtn2 { get; set; }
        }

        /// <summary>
        /// Parameters for the progress bar display mode
        /// </summary>
        public struct ProgressBarParams
        {
            /// <summary>
            /// Types of progress bar
            /// </summary>
            public enum BarTypes
            {
                /// <summary> Displays a progress bar showing percentage of process completion </summary>
                Percentage = 0,
                /// <summary> Displays a progress bar showing percentage of process completion and a Cancel button </summary>
                PercentageCancel = 1,
            }

            /// <summary>
            /// Progress bar type
            /// </summary>
            public BarTypes BarType { get; set; }

            /// <summary>
            /// Displayed user message
            /// </summary>
            public string Msg { get; set; }
        }

        /// <summary>
        /// System defined message display mode parameters
        /// </summary>
        public struct SystemMsgParams
        {
            /// <summary>
            /// Types of progress bar
            /// </summary>
            public enum SystemMessageTypes
            {
                /// <summary> Message required when there is no product available to the user (refer to TRC[R5055]) </summary>
                EmptyStore = 0,
                /// <summary> Message required when a camera is not connected </summary>
                CameraNotConnected = 4,
                /// <summary> Message explaining that the use of social features has been restricted (refer to TRC[R5061])  </summary>
                PSNCommunicationRestriction = 6,
            }

            /// <summary>
            /// System message type
            /// </summary>
            public SystemMessageTypes MsgType { get; set; }
        }

        /// <summary>
        /// Open message dialog
        /// </summary>
        public class OpenMsgDialogRequest : Request
        {
            /// <summary>
            /// Dialog mode
            /// </summary>
            public enum MsgModes
            {
                /// <summary> Invalid </summary>
                Invalid = 0,
                /// <summary> Displays an application specified message </summary>
                UserMsg = 1,
                /// <summary> Displays the progress bar </summary>
                ProgressBar = 2,
                /// <summary> Displays a system defined message </summary>
                SystemMsg = 3
            }

            /// <summary>
            /// Button result
            /// </summary>
            public enum ButtonResultTypes
            {
                /// <summary> No button was selected </summary>
                Invalid = 0,
                /// <summary> The OK button was selected </summary>
                OK = 1,
                /// <summary> The Yes button was selected </summary>
                Yes = 1,
                /// <summary> The No button was selected </summary>
                No = 2,
                /// <summary> Button1 was selected </summary>
                Button1 = 1,
                /// <summary> Button2 was selected </summary>
                Button2 = 2
            }

            /// <summary>
            /// User ID
            /// </summary>
            public Int32 UserId { get; set; }

            /// <summary>
            /// The type of message mode
            /// </summary>
            public MsgModes Mode { get; set; }

            /// <summary>
            /// Settings for User messages. Only used when <see cref="Mode"/> is <see cref="MsgModes.UserMsg"/>
            /// </summary>
            public UserMsgParams UserMsg { get; set; }

            /// <summary>
            /// Settings for progress bar. Only used when <see cref="Mode"/> is <see cref="MsgModes.ProgressBar"/>
            /// </summary>
            public ProgressBarParams ProgressBar { get; set; }

            /// <summary>
            /// Settings for system messages. Only used when <see cref="Mode"/> is <see cref="MsgModes.SystemMsg"/>
            /// </summary>
            public SystemMsgParams SystemMsg { get; set; }

            /// <summary>
            /// The Status of the dialog
            /// </summary>
            public DialogSystem.DialogStatus Status { get; internal set; } = DialogSystem.DialogStatus.None;

            /// <summary>
            /// ID of the button selected by the user
            /// </summary>
            public ButtonResultTypes SelectedButton { get; internal set; }

            /// <summary>
            /// Current progress of the bar.
            /// </summary>
            public UInt32 ProgressValue { get; internal set; }

            /// <summary>
            /// Close the dialog when it is open or just before being opened.
            /// Call this if the request has already been scheduled.
            /// </summary>
            public void CloseDialog()
            {
                m_forceCloseDialog = true;
            }

            internal bool m_forceCloseDialog = false;

            [Flags]
            enum UpdateProgressType
            {
                None = 0,
                Increment = 1,
                SetValue = 2,
                Message = 4
            }

            UpdateProgressType NextProgressUpdate = UpdateProgressType.None;
            UInt32 NextProgressValue = 0;
            string NextProgressMsg;

            /// <summary>
            /// Increment the progress bar by a percentage delta
            /// </summary>
            /// <param name="delta">The number of percentage points to increment</param>
            public void IncProgressBar(UInt32 delta)
            {
                NextProgressValue = delta;
                NextProgressUpdate |= UpdateProgressType.Increment;
            }

            /// <summary>
            /// Set the progress bar value
            /// </summary>
            /// <param name="rate">The value of the progress bar</param>
            public void SetProgressBarValue(UInt32 rate)
            {
                NextProgressValue = rate;
                NextProgressUpdate |= UpdateProgressType.SetValue;
            }

            /// <summary>
            /// Set the progress bar message
            /// </summary>
            /// <param name="message"></param>
            public void SetProgressBarMessage(string message)
            {
                NextProgressMsg = message;
                NextProgressUpdate |= UpdateProgressType.Message;
            }

            /// <summary>
            /// Reset the request so it can be used again. Don't call this when the request has been scheduled.
            /// </summary>
            public void Reset()
            {
                m_forceCloseDialog = false;
                Status = DialogSystem.DialogStatus.None;
                ProgressValue = 0;
                NextProgressUpdate = UpdateProgressType.None;
            }

            protected internal override void Run()
            {
                ProgressValue = 0;

                Status = DialogSystem.DialogStatus.None;

                if (m_forceCloseDialog == true)
                {
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return;
                }

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.OpenDialog);

                nativeMethod.Writer.Write(UserId);
                nativeMethod.Writer.Write((Int32)Mode);

                if(Mode == MsgModes.UserMsg)
                {
                    nativeMethod.Writer.Write((Int32)UserMsg.BtnType);
                    nativeMethod.Writer.WritePrxString(UserMsg.Msg);

                    if(UserMsg.BtnType == UserMsgParams.ButtonTypes.CustomButtons)
                    {
                        nativeMethod.Writer.WritePrxString(UserMsg.CustomBtn1);
                        nativeMethod.Writer.WritePrxString(UserMsg.CustomBtn2);
                    }
                }
                else if (Mode == MsgModes.ProgressBar)
                {
                    nativeMethod.Writer.Write((Int32)ProgressBar.BarType);
                    nativeMethod.Writer.WritePrxString(ProgressBar.Msg);
                }
                else if (Mode == MsgModes.SystemMsg)
                {
                    nativeMethod.Writer.Write((Int32)SystemMsg.MsgType);
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
                    //bool continueUpdate = true;

                    //DialogSystem.DialogStatus currentStatus = DialogSystem.DialogStatus.None;
                    //ButtonResultTypes selectedBtn = ButtonResultTypes.Invalid;

                    //while (continueUpdate)
                    //{
                    //    continueUpdate = !UpdateOpenedDialog(out currentStatus, out selectedBtn);

                    //    if (forceCloseDialog == true)
                    //    {
                    //        Result = ForceCloseDialog();
                    //        Status = DialogSystem.DialogStatus.ForceClosed;
                    //        return;
                    //    }
                    //}

                    //Status = currentStatus;
                    //SelectedButton = selectedBtn;
                }
                else
                {
                    MarshalMethods.ReleaseHandle(nativeMethod);
                }

            }

            protected internal override bool HasUpdate()
            {
                bool hasUpdate = Status == DialogSystem.DialogStatus.Running;

                Console.WriteLine("MsgDialog.HasUpdate = " + hasUpdate);
                return hasUpdate;
            }

            protected internal override bool Update()
            {
                Console.WriteLine("MsgDialog.Update");
                // Return true to finish update, false to continue

                if (m_forceCloseDialog == true)
                {
                    Result = ForceCloseDialog();
                    Status = DialogSystem.DialogStatus.ForceClosed;
                    return true;
                }

                DialogSystem.DialogStatus currentStatus = DialogSystem.DialogStatus.None;
                ButtonResultTypes selectedBtn = ButtonResultTypes.Invalid;

                bool finished = UpdateOpenedDialog(out currentStatus, out selectedBtn);

                if (finished == true)
                {
                    Console.WriteLine("finished : " + Status + " : " + SelectedButton);
                    Status = currentStatus;
                    SelectedButton = selectedBtn;
                }

                return finished;
            }

            bool UpdateOpenedDialog(out DialogSystem.DialogStatus status, out OpenMsgDialogRequest.ButtonResultTypes selectedBtn)
            {
                bool finished = false;
                status = DialogSystem.DialogStatus.None;
                selectedBtn = OpenMsgDialogRequest.ButtonResultTypes.Invalid;

                MarshalMethods.MethodHandle nativeMethod = MarshalMethods.PrepareMethod((UInt32)NativeMethods.UpdateDialog);

                UpdateProgressType currentProgressUpdate = NextProgressUpdate;
                UInt32 currentProgressValue = NextProgressValue;
                string currentProgressMsg = NextProgressMsg;

                NextProgressUpdate = UpdateProgressType.None;

                nativeMethod.Writer.Write((Int32)currentProgressUpdate);

                if (Mode == MsgModes.ProgressBar)
                {
                    if ((currentProgressUpdate & UpdateProgressType.Increment) != 0)
                    {
                        nativeMethod.Writer.Write(currentProgressValue);
                    }
                    else if ((currentProgressUpdate & UpdateProgressType.SetValue) != 0)
                    {
                        nativeMethod.Writer.Write(currentProgressValue);
                    }

                    if ((currentProgressUpdate & UpdateProgressType.Message) != 0)
                    {
                        nativeMethod.Writer.WritePrxString(NextProgressMsg);
                    }
                }

                nativeMethod.Call();

                if (nativeMethod.callResult.apiResult == APIResultTypes.Success)
                {
                    int sceStatus = nativeMethod.Reader.ReadInt32();

                    if (sceStatus == 2) status = DialogSystem.DialogStatus.Running;

                    if(status == DialogSystem.DialogStatus.Running)
                    {
                        if ((currentProgressUpdate & UpdateProgressType.Increment) != 0)
                        {
                            ProgressValue += currentProgressValue;
                        }
                        else if ((currentProgressUpdate & UpdateProgressType.SetValue) != 0)
                        {
                            ProgressValue = currentProgressValue;
                        }
                    }

                    bool hasFinished = nativeMethod.Reader.ReadBoolean();

                    if (hasFinished == true)
                    {
                        int sceDialogResult = nativeMethod.Reader.ReadInt32();

                        if (sceDialogResult == 0) status = DialogSystem.DialogStatus.FinishedOK;
                        else if (sceDialogResult == 1) status = DialogSystem.DialogStatus.FinishedCancel;
                        else if (sceDialogResult == 2) status = DialogSystem.DialogStatus.FinishedPurchased;

                        selectedBtn = (OpenMsgDialogRequest.ButtonResultTypes)nativeMethod.Reader.ReadInt32();

                        finished = true;
                    }
                }

                MarshalMethods.ReleaseHandle(nativeMethod);

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
    }
}
