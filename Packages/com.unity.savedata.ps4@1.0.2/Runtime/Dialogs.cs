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
            /// Savedata dialog system
            /// </summary>
            public class Dialogs
            {
                #region DLL Imports

                [DllImport("__Internal")]
                private static extern void PrxSaveDataOpenDialog(Int32 userId, OpenDialogSettings basicSettings, Items itemsSettings, UserMessageParam userMessage, SystemMessageParam systemMessage, 
                                                                 ErrorCodeParam errorCode, ProgressBarParam progressBar, NewItem newItem, OptionParam optionSettings,
                                                                 AnimationParam animations, out APIResult result);

                [DllImport("__Internal")]
                private static extern int PrxSaveDataDialogUpdateStatus();

                [DllImport("__Internal")]
                private static extern int PrxSaveDataDialogGetStatus();
                
                [DllImport("__Internal")]
                private static extern int PrxSaveDataDialogIsReadyToDisplay(out APIResult result);

                [DllImport("__Internal")]
                private static extern void PrxSaveDataDialogGetResult(out MemoryBufferNative data, out APIResult result);

                [DllImport("__Internal")]
                private static extern void PrxSaveDataDialogProgressBarInc(UInt32 delta, out APIResult result);

                [DllImport("__Internal")]
                private static extern void PrxSaveDataDialogProgressBarSetValue(UInt32 rate, out APIResult result);

                [DllImport("__Internal")]
                private static extern void PrxSaveDataDialogClose(CloseParam closeParam, out APIResult result);

                [DllImport("__Internal")]
                private static extern void PrxSaveDataInitializeDialog(out APIResult result);

                [DllImport("__Internal")]
                private static extern void PrxSaveDataTerminateDialog(out APIResult result);

                #endregion

                #region Native Marshaled classes

                /// <summary>
                /// Basic settings for the dialog
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                internal class OpenDialogSettings
                {
                    internal DialogMode mode;
                    internal DialogType dispType;
                }

                /// <summary>
                /// Parameters for application specified message display mode
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class UserMessageParam
                {
                    /// <summary>
                    /// The maximum length of the message
                    /// </summary>
                    public const Int32 MESSAGE_MAXSIZE = 255; // SCE_SAVE_DATA_DIALOG_USER_MSG_MAXSIZE


                    internal DialogButtonTypes buttonType;
                    internal UserMessageType msgType;

                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MESSAGE_MAXSIZE + 1)]
                    internal string msg;

                    /// <summary>
                    /// Type of button displayed on the dialog
                    /// </summary>
                    public DialogButtonTypes ButtonType
                    {
                        get { return buttonType; }
                        set { buttonType = value; }
                    }

                    /// <summary>
                    /// Type of the dialog
                    /// </summary>
                    public UserMessageType MsgType
                    {
                        get { return msgType; }
                        set { msgType = value; }
                    }

                    /// <summary>
                    /// Message to display in the dialog
                    /// </summary>
                    public string Message
                    {
                        get { return msg; }
                        set
                        {
                            if (value != null)
                            {
                                if (value.Length > MESSAGE_MAXSIZE)
                                {
                                    throw new SaveDataException("The length of the message string is more than " + MESSAGE_MAXSIZE + " characters (MESSAGE_MAXSIZE)");
                                }
                            }

                            msg = value;
                        }
                    }
                }

                /// <summary>
                /// Parameters for system defined message display mode
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class SystemMessageParam
                {
                    internal SystemMessageType sysMsgType;
                    internal UInt64 value;

                    /// <summary>
                    /// Type of system defined message to be displayed
                    /// </summary>
                    public SystemMessageType SysMsgType
                    {
                        get { return sysMsgType; }
                        set { sysMsgType = value; }
                    }

                    /// <summary>
                    /// Number to add to defined message
                    /// </summary>
                    public UInt64 Value
                    {
                        get { return value; }
                        set { this.value = value; }
                    }
                }

                /// <summary>
                /// Animation setting
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class AnimationParam
                {
                    internal Animation userOK;
                    internal Animation userCancel;

                    /// <summary>
                    /// Animation setting when the user performs an operation equivalent to OK
                    /// </summary>
                    public Animation UserOK
                    {
                        get { return userOK; }
                        set { userOK = value; }
                    }

                    /// <summary>
                    /// Set animation for when the user cancels
                    /// </summary>
                    public Animation UserCancel
                    {
                        get { return userCancel; }
                        set { userCancel = value; }
                    }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="Mounting.MountRequest"/> class.
                    /// </summary>
                    public AnimationParam(Animation ok, Animation cancel)
                    {
                        userOK = ok;
                        userCancel = cancel;
                    }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="Mounting.MountRequest"/> class.
                    /// </summary>
                    public AnimationParam()
                    {
                    }
                }

                /// <summary>
                /// Parameters for progress bar display mode
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class ProgressBarParam
                {
                    /// <summary>
                    /// The maximum length of the message
                    /// </summary>
                    public const Int32 MESSAGE_MAXSIZE = 255; // SCE_SAVE_DATA_DIALOG_USER_MSG_MAXSIZE

                    internal ProgressBarType barType;
                    internal ProgressSystemMessageType sysMsgType;

                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MESSAGE_MAXSIZE + 1)]
                    internal string msg;

                    /// <summary>
                    /// Type of progress bar
                    /// </summary>
                    public ProgressBarType BarType
                    {
                        get { return barType; }
                        set { barType = value; }
                    }

                    /// <summary>
                    /// Type of system message to display
                    /// </summary>
                    public ProgressSystemMessageType SysMsgType
                    {
                        get { return sysMsgType; }
                        set { sysMsgType = value; }
                    }

                    /// <summary>
                    /// Message string, can be empty
                    /// </summary>
                    public string Message
                    {
                        get { return msg; }
                        set
                        {
                            if (value != null)
                            {
                                if (value.Length > MESSAGE_MAXSIZE)
                                {
                                    throw new SaveDataException("The length of the message string is more than " + MESSAGE_MAXSIZE + " characters (MESSAGE_MAXSIZE)");
                                }
                            }

                            msg = value;
                        }
                    }
                }

                /// <summary>
                /// Parameters for error code display mode
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class ErrorCodeParam
                {
                    internal Int32 errorCode;

                    /// <summary>
                    /// Error code to be displayed
                    /// </summary>
                    public Int32 ErrorCode
                    {
                        get { return errorCode; }
                        set { errorCode = value; }
                    }
                }

                /// <summary>
                /// Parameters for displaying save data
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class Items
                {
                    /// <summary>
                    /// The maximum amount of targets that can be set.
                    /// </summary>
                    public const int DIR_NAME_MAXSIZE = Searching.DirNameSearchRequest.DIR_NAME_MAXSIZE;

                    [MarshalAs(UnmanagedType.ByValArray, SizeConst = DIR_NAME_MAXSIZE)]
                    internal DirName[] dirNames = new DirName[DIR_NAME_MAXSIZE];

                    internal UInt32 dirNameNum;

                    internal FocusPos focusPos;
                    internal DirName focusPosDirName;
                    internal ItemStyle itemStyle;

                    /// <summary>
                    /// Array of target save data directory names
                    /// </summary>
                    public DirName[] DirNames
                    {
                        get
                        {
                            if (dirNameNum == 0) return null;

                            DirName[] copy = new DirName[dirNameNum];

                            Array.Copy(dirNames, copy, (int)dirNameNum);

                            return copy;
                        }
                        set
                        {
                            if (value != null)
                            {
                                if (value.Length > DIR_NAME_MAXSIZE)
                                {
                                    throw new SaveDataException("The size of the array is more than " + DIR_NAME_MAXSIZE);
                                }

                                value.CopyTo(dirNames, 0);
                                dirNameNum = (UInt32)value.Length;
                            }
                            else
                            {
                                dirNameNum = 0;
                            }
                        }
                    }

                    /// <summary>
                    /// Number of target save data directories. Set this using <see cref="DirNames"/> 
                    /// </summary>
                    public UInt32 DirNameCount
                    {
                        get { return dirNameNum; }
                    }

                    /// <summary>
                    /// Initial focus position on the list
                    /// </summary>
                    public FocusPos FocusPos
                    {
                        get { return focusPos; }
                        set { focusPos = value;  }
                    }

                    /// <summary>
                    /// Initial focus directory name on the list. Only valid is <see cref="DialogMode.List"/> is set in the <see cref="OpenDialogRequest"/> object.
                    /// </summary>
                    /// <exception cref="SaveDataException"> Will throw an exception is <see cref="FocusPos"/> isn't set to <see cref="FocusPos.DirName"/>.</exception>
                    public DirName FocusPosDirName
                    {
                        get { return focusPosDirName; }
                        set
                        {
                            if (focusPos != FocusPos.DirName)
                            {
                                throw new SaveDataException("Can't set a focus DirName unless FocusPos is set to FocusPos.DirName value.");
                            }
                            focusPosDirName = value;
                        }
                    }

                    /// <summary>
                    /// Display style of save data information
                    /// </summary>
                    public ItemStyle ItemStyle
                    {
                        get { return itemStyle; }
                        set { itemStyle = value; }
                    }

                }

                /// <summary>
                /// Parameters for new save data
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class NewItem
                {
                    /// <summary>
                    /// The maximum length of the message
                    /// </summary>
                    public const Int32 TITLE_MAXSIZE = 127; // SCE_SAVE_DATA_TITLE_MAXSIZE

                    /// <summary>
                    /// The maximum length of the pathname to an icon file
                    /// </summary>
                    public const Int32 FILEPATH_LENGTH = Mounting.SaveIconRequest.FILEPATH_LENGTH;

                    /// <summary>
                    /// The maximum number of bytes in the icon PNG
                    /// </summary>
                    public const Int32 ICON_FILE_MAXSIZE = Mounting.SaveIconRequest.ICON_FILE_MAXSIZE;

                    /// <summary>
                    /// The required width of the icon
                    /// </summary>
                    public const Int32 DATA_ICON_WIDTH = Mounting.SaveIconRequest.DATA_ICON_WIDTH;

                    /// <summary>
                    /// The required height of the icon
                    /// </summary>
                    public const Int32 DATA_ICON_HEIGHT = Mounting.SaveIconRequest.DATA_ICON_HEIGHT;

                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = FILEPATH_LENGTH + 1)]
                    internal string iconPath;

                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TITLE_MAXSIZE + 1)]
                    internal string title;

                    [MarshalAs(UnmanagedType.LPArray)]
                    internal byte[] rawPNG;

                    internal UInt64 pngDataSize;

                    /// <summary>
                    /// Title of new save data. Line breaks not possible.
                    /// </summary>
                    public string Title
                    {
                        get { return title; }
                        set
                        {
                            if (value != null)
                            {
                                if (value.Length > TITLE_MAXSIZE)
                                {
                                    throw new SaveDataException("The length of the title string is more than " + TITLE_MAXSIZE + " characters (TITLE_MAXSIZE)");
                                }
                            }

                            title = value;
                        }
                    }

                    /// <summary>
                    /// Path to a PNG file.
                    /// </summary>
                    public string IconPath
                    {
                        get { return iconPath; }
                        set { iconPath = value; }
                    }

                    /// <summary>
                    /// An array of bytes containing the PNG data
                    /// </summary>
                    public byte[] RawPNG
                    {
                        get { return rawPNG; }
                        set
                        {
                            rawPNG = value;
                            pngDataSize = (value != null) ? (UInt64)value.Length : 0;
                        }
                    }
                }

                /// <summary>
                /// Save data dialog option setting parameters
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class OptionParam
                {
                    internal OptionBack back;

                    /// <summary>
                    /// Enable/disable "Back" feature
                    /// </summary>
                    public OptionBack Back
                    {
                        get { return back; }
                        set { back = value; }
                    }
                }

                /// <summary>
                /// Parameters for closing the save data dialog
                /// </summary>
                [StructLayout(LayoutKind.Sequential)]
                public class CloseParam
                {
                    internal Animation anim;

                    /// <summary>
                    /// Animation setting when closing dialog
                    /// </summary>
                    public Animation Anim
                    {
                        get { return anim; }
                        set { anim = value; }
                    }
                }

                #endregion

                #region Requests

                /// <summary>
                /// Save data dialog parameters
                /// </summary>
                public class OpenDialogRequest : RequestBase
                {
                    internal OpenDialogSettings basicSettings = new OpenDialogSettings();
                    internal UserMessageParam userMessage;
                    internal SystemMessageParam systemMessage;
                    internal ErrorCodeParam errorCode;
                    internal ProgressBarParam progressBar;
                    internal OptionParam option;
                    internal Items items = new Items();
                    internal NewItem newItem;
                    internal AnimationParam animations;

                    internal static bool dialogIsOpen = false;
                    internal static CloseParam closeParam;

                    /// <summary>
                    /// Operation mode of the save data dialog
                    /// </summary>
                    public DialogMode Mode
                    {
                        get { return basicSettings.mode; }
                        set { ThrowExceptionIfLocked(); basicSettings.mode = value; }
                    }

                    /// <summary>
                    /// Type of message to display (save/load/delete)
                    /// </summary>
                    public DialogType DispType
                    {
                        get { return basicSettings.dispType; }
                        set { ThrowExceptionIfLocked(); basicSettings.dispType = value; }
                    }

                    /// <summary>
                    /// Application specified message display parameters
                    /// </summary>
                    public UserMessageParam UserMessage
                    {
                        get { return userMessage; }
                        set { ThrowExceptionIfLocked(); userMessage = value; }
                    }

                    /// <summary>
                    /// System defined message display parameters
                    /// </summary>
                    public SystemMessageParam SystemMessage
                    {
                        get { return systemMessage; }
                        set { ThrowExceptionIfLocked(); systemMessage = value; }
                    }

                    /// <summary>
                    /// Error code display parameters
                    /// </summary>
                    public ErrorCodeParam ErrorCode
                    {
                        get { return errorCode; }
                        set { ThrowExceptionIfLocked(); errorCode = value; }
                    }

                    /// <summary>
                    /// Progress bar display parameters
                    /// </summary>
                    public ProgressBarParam ProgressBar
                    {
                        get { return progressBar; }
                        set { ThrowExceptionIfLocked(); progressBar = value; }
                    }

                    /// <summary>
                    /// Option setting parameters
                    /// </summary>
                    public OptionParam Option
                    {
                        get { return option; }
                        set { ThrowExceptionIfLocked(); option = value; }
                    }

                    /// <summary>
                    /// Save data display parameters
                    /// </summary>
                    public Items Items
                    {
                        get { return items; }
                        set { ThrowExceptionIfLocked(); items = value; }
                    }

                    /// <summary>
                    /// Parameters of new save data
                    /// </summary>
                    public NewItem NewItem
                    {
                        get { return newItem; }
                        set { ThrowExceptionIfLocked(); newItem = value; }
                    }

                    /// <summary>
                    /// Animation setting parameters
                    /// </summary>
                    public AnimationParam Animations
                    {
                        get { return animations; }
                        set { ThrowExceptionIfLocked(); animations = value; }
                    }

                    internal override bool IsDeferred { get { return true; } }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="OpenDialogRequest"/> class.
                    /// </summary>
                    public OpenDialogRequest()
                        : base(FunctionTypes.OpenDialog)
                    {
                    }

                    internal override void Execute(PendingRequest pendingRequest)
                    {
                        APIResult result;

                        closeParam = null;

                        if(newItem != null)
                        {
                            if (newItem.pngDataSize == 0 && !string.IsNullOrEmpty(newItem.iconPath))
                            {
                                newItem.rawPNG = System.IO.File.ReadAllBytes(newItem.iconPath);
                                newItem.pngDataSize = (UInt64)newItem.rawPNG.Length;
                            }
                        }

                        OpenDialogResponse response = pendingRequest.response as OpenDialogResponse;

                        bool wasInitialised = false;

                        if (dialogIsOpen == false)
                        {
                            APIResult initResult;
                            PrxSaveDataInitializeDialog(out initResult);

                            if (initResult.sceErrorCode < 0)
                            {
                                response.Populate(initResult); // Set error code
                                return;
                            }
                            wasInitialised = true;

                        }

                        PrxSaveDataOpenDialog(userId, basicSettings, items, userMessage, systemMessage, errorCode, progressBar, newItem, option, animations, out result);

                        if (response != null)
                        {
                            response.Populate(result); // will set error code if there is one

                            if (response.IsErrorCodeWithoutLockCheck == false)
                            {
                                DialogUpdateStatus(); // call this at least once to start the update process. Future calls to poll this method are done in ExecutePolling

                                if (dialogIsOpen == false)
                                {
                                    SendDialogOpenedNotification(pendingRequest);
                                    dialogIsOpen = true;
                                }
                            }
                            else
                            {
                                // must shutdown dialog system if the dialog failed to open.
                                if (wasInitialised == true)
                                {
                                    APIResult initResult;
                                    PrxSaveDataTerminateDialog(out initResult);
                                }
                            }
                        }
                    }

                    internal override bool ExecutePolling(PendingRequest pendingRequest)
                    {
                        DialogStatus status = DialogUpdateStatus();

                        if(status == DialogStatus.Finished)
                        {
                            DialogResult result = GetDialogResult();

                            bool fullyClosingDialog = true;

                            // Test the callresult and the animation paramters
                            // This need to calculate if the userOK or userCancel animation needs to be checked.
                            // Its not as simple as testing the callResult. For userCancel callresult its easy to determine.
                            // When the user selects the 'No' button this results in a DialogCallResults.OK callresult but the buttonid will show which buttons was pressed.

                            if(HasDialogBeenCanceled(result) == true)
                            {
                                if (animations != null && animations.userCancel == Animation.Off)
                                {
                                    fullyClosingDialog = false;
                                }
                            }
                            else
                            {
                                if(closeParam != null) // Was dialog closed via a call to Close()
                                {
                                    if (closeParam.anim == Animation.Off)
                                    {
                                        fullyClosingDialog = false;
                                    }
                                }
                                else if (animations != null && animations.userOK == Animation.Off)
                                {
                                    fullyClosingDialog = false;
                                }
                            }

                            if (fullyClosingDialog == true)
                            {
                                APIResult initResult;
                                PrxSaveDataTerminateDialog(out initResult);

                                SendDialogClosedNotification(pendingRequest);
                                dialogIsOpen = false;
                                closeParam = null;
                            }

                            OpenDialogResponse response = pendingRequest.response as OpenDialogResponse;

                            if (response != null)
                            {
                                response.Populate(result);
                            }

                            return false; // polling should stop
                        }

                        return true; // continue processing
                    }

                    // DevNet Documention:
                    // For userOK, specify the animation settings for when an operation equivalent to OK is performed. Specifically, the following operations are equivalent to OK.
                    //   When an "OK" / "Yes" button is selected
                    //   When save data is selected in save data list display mode
                    //   When a "Back" operation is performed in dialog that has an OK button
                    // For userCancel, specify the animation settings for when an operation equivalent to cancelling is performed.Specifically, the following operations are equivalent to cancelling.
                    //   When a "Cancel" / "No" button is selected
                    //   When a "Back" operation is performed in save data list display mode
                    //   When a "Back" operation is performed in dialog that does not have any buttons
                    // Note
                    //   A "Back" operation may be equivalent to OK or equivalent to cancelling depending on the dialog, and the same animation settings will be applied as those for the button assigned to "Back" operations.Common examples are shown above.

                    // CallResult       ButtonId     Description
                    // OK               Invalid      When save data is selected in dialog in save data list display mode
                    // OK               OK           When a OK button is selected or when a "Back" operation is performed in dialog that has an OK button
                    // OK               Yes          When a Yes button is selected
                    // OK               No           When a No button is selected or when a "Back" operation is performed in dialog that has Yes/ No buttons
                    // UserCanceled     Invalid      When a Cancel button is selected or when a "Back" operation is performed in dialog that does not have any buttons
                    // OK               Invalid      When an application has called sceSaveDataDialogClose()

                    internal bool HasDialogBeenCanceled(DialogResult result)
                    {
                        if (result.callResult == DialogCallResults.UserCanceled)
                        {
                            return true;
                        }

                        // callresult will be DialogCallResults.OK
                        if(result.buttonId == DialogButtonIds.No )
                        {
                            return true;
                        }

                        return false;
                    }



                    internal void SendDialogOpenedNotification(PendingRequest pendingRequest)
                    {
                        // Create notification for dialog opening
                        EmptyResponse notification = new EmptyResponse();
                        notification.returnCode = 0;

                        int userId = -1;

                        if (pendingRequest.request != null)
                        {
                            userId = pendingRequest.request.userId;
                        }

                        DispatchQueueThread.AddNotificationRequest(notification, FunctionTypes.NotificationDialogOpened, userId, pendingRequest.requestId, pendingRequest.request);
                    }

                    internal void SendDialogClosedNotification(PendingRequest pendingRequest)
                    {
                        // Create notification for dialog opening
                        EmptyResponse notification = new EmptyResponse();
                        notification.returnCode = 0;

                        int userId = -1;

                        if (pendingRequest.request != null)
                        {
                            userId = pendingRequest.request.userId;
                        }

                        DispatchQueueThread.AddNotificationRequest(notification, FunctionTypes.NotificationDialogClosed, userId, pendingRequest.requestId, pendingRequest.request);
                    }
                }

                #endregion

                /// <summary>
                /// Save data dialog results
                /// </summary>
                public class OpenDialogResponse : ResponseBase
                {
                    internal DialogResult result;

                    /// <summary>
                    /// The dialog result
                    /// </summary>
                    public DialogResult Result
                    {
                        get { ThrowExceptionIfLocked(); return result; }
                    }

                    /// <summary>
                    /// Initializes a new instance of the <see cref="OpenDialogResponse"/> class.
                    /// </summary>
                    public OpenDialogResponse()
                    {

                    }

                    internal void Populate(DialogResult result)
                    {
                        returnCode = result.sceErrorCode;
                        this.result = result;
                    }
                }

                /// <summary>
                /// This method is used to open a savedata dialog. See sceSaveDataDialogOpen PS4 documention for further details. 
                /// </summary>
                /// <param name="request">The savedata dialog parameters.</param>
                /// <param name="response">The results of the dialog once closed.</param>
                /// <returns>If the operation is asynchronous, the function provides the request Id.</returns>
                /// <exception cref="SaveDataException">Will throw an exception either when the request data is invalid, or an internal error has occured inside the plug-in.</exception>
                public static int OpenDialog(OpenDialogRequest request, OpenDialogResponse response)
                {
                    DispatchQueueThread.ThrowExceptionIfSameThread(request.async);

                    PendingRequest pe = ProcessQueueThread.AddEvent(request, response);

                    return ProcessQueueThread.WaitIfSyncRequest(pe);
                }

                /// <summary>
                /// Status of the save data dialog
                /// </summary>
                public enum DialogStatus
                {
                    /// <summary>Save data dialog is not running. See SCE_COMMON_DIALOG_STATUS_NONE for details.</summary>
                    None,
                    /// <summary>Save data dialog is initialized See SCE_COMMON_DIALOG_STATUS_INITIALIZED for details.</summary>
                    Initialized,
                    /// <summary>Save data dialog is being displayed See SCE_COMMON_DIALOG_STATUS_RUNNING for details.</summary>
                    Running,
                    /// <summary>Save data dialog is closed See SCE_COMMON_DIALOG_STATUS_FINISHED for details.</summary>
                    Finished
                }

                /// <summary>
                /// Update and get operation status of the save data dialog
                /// </summary>
                /// <returns></returns>
                internal static DialogStatus DialogUpdateStatus()
                {
                    return (DialogStatus)PrxSaveDataDialogUpdateStatus();
                }

                /// <summary>
                /// Get operation status of the save data dialog
                /// </summary>
                /// <returns>The status of the save data dialog.</returns>
                public static DialogStatus DialogGetStatus()
                {
                    return (DialogStatus)PrxSaveDataDialogGetStatus();
                }

                /// <summary>
                /// Get if screen preparations are ready or not
                /// </summary>
                /// <returns>Return true when the dialog is ready.</returns>
                public static bool DialogIsReadyToDisplay()
                {
                    APIResult result;

                    int ret = PrxSaveDataDialogIsReadyToDisplay(out result);

                    if (result.RaiseException == true) throw new SaveDataException(result);

                    if (ret == 0)  return false;

                    return true;
                }

                /// <summary>
                /// Operation mode of the save data dialog
                /// </summary>
                public enum DialogMode  
                {
                    /// <summary>Invalid mode. See SCE_SAVE_DATA_DIALOG_MODE_INVALID for details.</summary>
                    Invalid = 0,
                    /// <summary>Save data list display mode. See SCE_SAVE_DATA_DIALOG_MODE_LIST for details.</summary>
                    List = 1, 
                    /// <summary>Application specified message display mode. See SCE_SAVE_DATA_DIALOG_MODE_USER_MSG for details.</summary>
                    UserMsg = 2,
                    /// <summary>System defined message display mode. See SCE_SAVE_DATA_DIALOG_MODE_SYSTEM_MSG for details.</summary>
                    SystemMsg = 3, 
                    /// <summary>Error code display mode. See SCE_SAVE_DATA_DIALOG_MODE_ERROR_CODE for details.</summary>
                    ErrorCode = 4,
                    /// <summary>Progress bar display mode. See SCE_SAVE_DATA_DIALOG_MODE_PROGRESS_BAR for details.</summary>
                    ProgressBar = 5,
                }

                /// <summary>
                /// Type of message to display (save/load/delete)
                /// </summary>
                public enum DialogType
                {
                    /// <summary>Inalvid Type. See SCE_SAVE_DATA_DIALOG_TYPE_INVALID for details.</summary>
                    Invalid = 0,
                    /// <summary>Display message for save processing. See SCE_SAVE_DATA_DIALOG_TYPE_SAVE for details.</summary>
                    Save = 1,
                    /// <summary>Display message for load processing. See SCE_SAVE_DATA_DIALOG_TYPE_LOAD for details.</summary>
                    Load = 2,
                    /// <summary>Display message for delete processing. See SCE_SAVE_DATA_DIALOG_TYPE_DELETE for details.</summary>
                    Delete = 3,
                }

                /// <summary>
                /// User message dialog type
                /// </summary>
                public enum UserMessageType
                {
                    /// <summary>Display as normal dialog. See SCE_SAVE_DATA_DIALOG_USERMSG_TYPE_NORMAL for details.</summary>
                    Normal = 0,
                    /// <summary>Display as error display dialog. See SCE_SAVE_DATA_DIALOG_USERMSG_TYPE_ERROR for details.</summary>
                    Error = 1,
                }

                /// <summary>
                /// Dialog call result
                /// </summary>
                public enum DialogCallResults
                {
                    /// <summary>Closed by user operation or <see cref="Dialogs.Close(CloseParam)"/> . See SCE_COMMON_DIALOG_RESULT_OK for details.</summary>
                    OK = 0,
                    /// <summary>Canceled by the user. See SCE_COMMON_DIALOG_RESULT_USER_CANCELED for details.</summary>
                    UserCanceled = 1,
                }

                /// <summary>
                /// Type of button displayed on the user message dialog
                /// </summary>
                public enum DialogButtonTypes
                {
                    /// <summary>1 button, the "OK" button. See SCE_SAVE_DATA_DIALOG_BUTTON_TYPE_OK for details.</summary>
                    OK = 0,
                    /// <summary>2 buttons, the "Yes" button and the "No" button. See SCE_SAVE_DATA_DIALOG_BUTTON_TYPE_YESNO for details.</summary>
                    YesNo = 1,
                    /// <summary>Do not display buttons. See SCE_SAVE_DATA_DIALOG_BUTTON_TYPE_NONE for details.</summary>
                    None = 2,
                    /// <summary>2 buttons, the "OK" button and the "Cancel" button. See SCE_SAVE_DATA_DIALOG_BUTTON_TYPE_OKCANCEL for details.</summary>
                    OKCancel = 3,
                }

                /// <summary>
                /// Selected button ID
                /// </summary>
                public enum DialogButtonIds
                {
                    /// <summary>No button was selected. See SCE_SAVE_DATA_DIALOG_BUTTON_ID_INVALID for details.</summary>
                    Invalid = 0,
                    /// <summary>The "OK" button was selected. See SCE_SAVE_DATA_DIALOG_BUTTON_ID_OK for details.</summary>
                    OK = 1,
                    /// <summary>The "Yes" button was selected. See SCE_SAVE_DATA_DIALOG_BUTTON_ID_YES for details.</summary>
                    Yes = 1,
                    /// <summary>The "No" button was selected. See SCE_SAVE_DATA_DIALOG_BUTTON_ID_NO for details.</summary>
                    No = 2,
                }

                /// <summary>
                /// Animation setting
                /// </summary>
                public enum Animation
                {
                    /// <summary>Enable animation. See SCE_SAVE_DATA_DIALOG_ANIMATION_ON for details.</summary>
                    On = 0,
                    /// <summary>Disable animation. See SCE_SAVE_DATA_DIALOG_ANIMATION_OFF for details.</summary>
                    Off = 1,
                }

                /// <summary>
                /// Types of system prepared message
                /// </summary>
                /// <remarks>
                /// This will display various message depending on the dialog type <see cref="DialogType"/> and the system message.
                /// See the SaveData SDK documention for details on what message is displayed based on dialog type and system message.
                /// </remarks>
                public enum SystemMessageType
                {
                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_INVALID for details.</summary>
                    Invalid = 0,
                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_NODATA for details.</summary>
                    NoData = 1,
                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_CONFIRM for details.</summary>
                    Confirm = 2,
                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_OVERWRITE for details.</summary>
                    Overwrite = 3,

                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_NOSPACE for details.</summary>
                    NoSpace = 4,
                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_PROGRESS for details.</summary>
                    Progress = 5,
                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_FILE_CORRUPTED for details.</summary>
                    Corrupted = 6,
                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_FINISHED for details.</summary>
                    Finished = 7,

                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_NOSPACE_CONTINUABLE for details.</summary>
                    NoSpaceContinuable = 8,
                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_CORRUPTED_AND_DELETE for details.</summary>
                    CorruptedAndDelete = 10,
                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_CORRUPTED_AND_CREATE for details.</summary>
                    CorruptedAndCreate = 11,
                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_CORRUPTED_AND_RESTORE for details.</summary>
                    CurruptedAndRestore = 13,

                    /// <summary>See SCE_SAVE_DATA_DIALOG_SYSMSG_TYPE_TOTAL_SIZE_EXCEEDED for details.</summary>
                    TotalSizeExceeded = 14,
                }

                /// <summary>
                /// Progress bar type
                /// </summary>
                public enum ProgressBarType
                {
                    /// <summary>Progress bar expressed as a percentage. See SCE_SAVE_DATA_DIALOG_PROGRESSBAR_TYPE_PERCENTAGE for details.</summary>
                    Percentage = 0,
                }

                /// <summary>
                /// Type of progress bar system message to display
                /// </summary>
                public enum ProgressSystemMessageType
                {
                    /// <summary>Arbitrary string specified to <see cref="ProgressBarParam.Message"/>. See SCE_SAVE_DATA_DIALOG_PRGRESS_SYSMSG_TYPE_INVALID for details.</summary>
                    Invalid = 0,
                    /// <summary>System defined message such as "Saving...", "Loading..", etc., according to the message display type. See SCE_SAVE_DATA_DIALOG_PRGRESS_SYSMSG_TYPE_PROGRESS for details.</summary>
                    Progress = 1,
                    /// <summary>"Restoring saved data...". See SCE_SAVE_DATA_DIALOG_PRGRESS_SYSMSG_TYPE_RESTORE for details.</summary>
                    Restore = 2,
                }

                /// <summary>
                /// Indicates if "Back" operation is enabled/disabled
                /// </summary>
                public enum OptionBack
                {
                    /// <summary>Enable "Back" operation. See SCE_SAVE_DATA_DIALOG_OPTION_BACK_ENABLE for details.</summary>
                    Enable = 0,
                    /// <summary>Disable "Back" operation. See SCE_SAVE_DATA_DIALOG_OPTION_BACK_DISABLE for details.</summary>
                    Disable = 1,
                }

                /// <summary>
                /// Initial focus position
                /// </summary>
                public enum FocusPos
                {
                    /// <summary>List head. See SCE_SAVE_DATA_DIALOG_FOCUS_POS_LISTHEAD for details.</summary>
                    ListHead = 0,
                    /// <summary>List end. See SCE_SAVE_DATA_DIALOG_FOCUS_POS_LISTTAIL for details.</summary>
                    ListTail = 1,
                    /// <summary>Data head (same as list head). See SCE_SAVE_DATA_DIALOG_FOCUS_POS_DATAHEAD for details.</summary>
                    DataHead = 2,
                    /// <summary>Data end (same as list end). See SCE_SAVE_DATA_DIALOG_FOCUS_POS_DATATAIL for details.</summary>
                    DataTail = 3,
                    /// <summary>Latest data. See SCE_SAVE_DATA_DIALOG_FOCUS_POS_DATALATEST for details.</summary>
                    DataLatest = 4,
                    /// <summary>Oldest data. See SCE_SAVE_DATA_DIALOG_FOCUS_POS_DATAOLDEST for details.</summary>
                    DataOldest = 5,
                    /// <summary><see cref="Items.FocusPosDirName"/> . See SCE_SAVE_DATA_DIALOG_FOCUS_POS_DIRNAME for details.</summary>
                    DirName = 6,
                }

                /// <summary>
                /// Display style of save data
                /// </summary>
                public enum ItemStyle
                {
                    /// <summary>3-line display. 1st line: Title, 2nd line: Date and Size, 3rd line: Subtitle. See SCE_SAVE_DATA_DIALOG_ITEM_STYLE_TITLE_DATESIZE_SUBTITLE for details.</summary>
                    DateSizeSubtitle = 0,
                    /// <summary>3-line display. 1st line: Title, 2nd line: Subtitle, 3rd line: Date and Size. See SCE_SAVE_DATA_DIALOG_ITEM_STYLE_TITLE_SUBTITLE_DATESIZE for details.</summary>
                    SubtitleDataSize = 1,
                    /// <summary>2-line display. 1st line: Title, 2nd line: Date and Size. See SCE_SAVE_DATA_DIALOG_ITEM_STYLE_TITLE_DATESIZE for details.</summary>
                    DataSize = 2,
                }

                /// <summary>
                /// Save data dialog call result
                /// </summary>
                public class DialogResult
                {
                    internal int sceErrorCode;
                    internal DialogMode mode;
                    internal DialogCallResults callResult;
                    internal DialogButtonIds buttonId;

                    internal DirName dirName;
                    internal SaveDataParams param;

                    /// <summary>
                    /// Mode at the time of call
                    /// </summary>
                    public DialogMode Mode
                    {
                        get { return mode; }
                    }

                    /// <summary>
                    /// The dialog call result - either OK or UserCanceled
                    /// </summary>
                    public DialogCallResults CallResult
                    {
                        get { return callResult; }
                    }

                    /// <summary>
                    /// The button id pressed.
                    /// </summary>
                    public DialogButtonIds ButtonId
                    {
                        get { return buttonId; }
                    }

                    /// <summary>
                    /// Directory name of the displayed or selected save data
                    /// </summary>
                    public DirName DirName
                    {
                        get { return dirName; }
                    }

                    /// <summary>
                    /// Detailed information of the save data
                    /// </summary>
                    public SaveDataParams Param
                    {
                        get { return param; }
                    }

                    /// <summary>
                    /// A SCE error code if the GetDialogResult() failed.
                    /// </summary>
                    public int ErrorCode
                    {
                        get { return sceErrorCode; }
                    }
                } 

                internal static DialogResult GetDialogResult()
                {
                    DialogResult dialogResult = new Dialogs.DialogResult();

                    APIResult result;

                    MemoryBufferNative data = new MemoryBufferNative();

                    PrxSaveDataDialogGetResult(out data, out result);

                    if (result.sceErrorCode < 0)
                    {
                        // an error has occured.
                        dialogResult.sceErrorCode = result.sceErrorCode;
                        dialogResult.mode = DialogMode.Invalid;
                    }
                    else
                    {
                        dialogResult.sceErrorCode = 0; // No error

                        MemoryBuffer readBuffer = new MemoryBuffer(data);
                        readBuffer.CheckStartMarker();  // Will throw exception if start marker isn't present in the buffer.

                        dialogResult.mode = (DialogMode)readBuffer.ReadInt32();
                        dialogResult.callResult = (DialogCallResults)readBuffer.ReadInt32();
                        dialogResult.buttonId = (DialogButtonIds)readBuffer.ReadInt32();

                        dialogResult.dirName.Read(readBuffer);
                        dialogResult.param.Read(readBuffer);

                        readBuffer.CheckEndMarker();
                    }

                    return dialogResult;
                }

                /// <summary>
                /// Add progress bar progress rate to save data dialog
                /// </summary>
                /// <param name="delta">Progress additional value</param>
                public static void ProgressBarInc(UInt32 delta)
                {
                    APIResult result;

                    PrxSaveDataDialogProgressBarInc(delta, out result);

                    if (result.RaiseException == true) throw new SaveDataException(result);
                }

                /// <summary>
                /// Set progress rate of progress bar in save data dialog
                /// </summary>
                /// <param name="rate">Progress value</param>
                public static void ProgressBarSetValue(UInt32 rate)
                {
                    APIResult result;

                    PrxSaveDataDialogProgressBarSetValue(rate, out result);

                    if(result.RaiseException == true) throw new SaveDataException(result);
                }

                /// <summary>
                /// Close the save data dialog
                /// </summary>
                /// <param name="closeParam">Parameters for closing the save data dialog</param>
                public static void Close(CloseParam closeParam)
                {
                    APIResult result;

                    OpenDialogRequest.closeParam = closeParam;

                    PrxSaveDataDialogClose(closeParam, out result);

                    if (result.RaiseException == true)
                    {
                        OpenDialogRequest.closeParam = null;
                        throw new SaveDataException(result);
                    }
                }
            }
        } // SavedGames
    } // PS4
} // Sony