#include "MsgDialog.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include "WebApiNotifications.h"

#include <vector>
#include <libsysmodule.h>

#include <message_dialog.h>

#pragma comment(lib,"libSceMsgDialog_stub_weak.a")

namespace psn
{
    void MsgDialog::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::OpenDialog, MsgDialog::OpenDialogImpl);
        MsgHandler::AddMethod(Methods::UpdateDialog, MsgDialog::UpdateDialogImpl);
        MsgHandler::AddMethod(Methods::CloseDialog, MsgDialog::CloseDialogImpl);
    }

    void MsgDialog::InitializeLib()
    {
        int res = sceSysmoduleIsLoaded(SCE_SYSMODULE_MESSAGE_DIALOG);
        if (res != SCE_SYSMODULE_LOADED)
        {
            res = sceSysmoduleLoadModule(SCE_SYSMODULE_MESSAGE_DIALOG);

            if (res < 0)
            {
                UNITY_TRACE("Error loading SCE_SYSMODULE_MESSAGE_DIALOG, 0x%x\n", res);
            }
            s_ModuleLoaded = true;
        }
    }

    void MsgDialog::TerminateLib()
    {
        if (s_ModuleLoaded == true)
        {
            int res = sceSysmoduleUnloadModule(SCE_SYSMODULE_MESSAGE_DIALOG);

            if (res < 0)
            {
                UNITY_TRACE("Error unloading SCE_SYSMODULE_MESSAGE_DIALOG, 0x%x\n", res);
            }
        }
    }

    bool MsgDialog::s_DialogInitialized = false;
    bool MsgDialog::s_ModuleLoaded = false;

    void MsgDialog::OpenDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        ret = InitialzeDialog();

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        SceMsgDialogMode mode = (SceMsgDialogMode)reader.ReadInt32();

        SceMsgDialogParam param;
        sceMsgDialogParamInitialize(&param);

        param.userId = userId;
        param.mode = mode;

        SceMsgDialogUserMessageParam userMsgParam;
        memset(&userMsgParam, 0, sizeof(userMsgParam));

        SceMsgDialogButtonsParam buttonParam;
        memset(&buttonParam, 0, sizeof(buttonParam));

        SceMsgDialogProgressBarParam progressParam;
        memset(&progressParam, 0, sizeof(progressParam));

        SceMsgDialogSystemMessageParam sysMsgParam;
        memset(&sysMsgParam, 0, sizeof(sysMsgParam));

        if (mode == SCE_MSG_DIALOG_MODE_USER_MSG)
        {
            userMsgParam.buttonType = (SceMsgDialogButtonType)reader.ReadInt32();
            userMsgParam.msg = reader.ReadStringPtr();

            if (userMsgParam.buttonType == SCE_MSG_DIALOG_BUTTON_TYPE_2BUTTONS)
            {
                buttonParam.msg1 = reader.ReadStringPtr();
                buttonParam.msg2 = reader.ReadStringPtr();

                userMsgParam.buttonsParam = &buttonParam;
            }

            param.userMsgParam = &userMsgParam;
        }
        else if (mode == SCE_MSG_DIALOG_MODE_PROGRESS_BAR)
        {
            progressParam.barType = (SceMsgDialogProgressBarType)reader.ReadInt32();
            progressParam.msg = reader.ReadStringPtr();

            param.progBarParam = &progressParam;
        }
        else if (mode == SCE_MSG_DIALOG_MODE_SYSTEM_MSG)
        {
            sysMsgParam.sysMsgType = (SceMsgDialogSystemMessageType)reader.ReadInt32();

            param.sysMsgParam = &sysMsgParam;
        }

        ret = sceMsgDialogOpen(&param);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            TerminateDialog();
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void MsgDialog::UpdateDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        SceCommonDialogStatus status = sceMsgDialogUpdateStatus();

        BinaryReader reader(sourceData, sourceSize);
        UpdateProgressBar(reader, status);

        SceMsgDialogResult dialogResult;
        memset(&dialogResult, 0, sizeof(dialogResult));

        bool hasFinished = false;

        if (status == SCE_COMMON_DIALOG_STATUS_FINISHED)
        {
            ret = sceMsgDialogGetResult(&dialogResult);

            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                TerminateDialog();
                return;
            }

            hasFinished = true;
            TerminateDialog();
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(status);
        writer.WriteBool(hasFinished);
        if (hasFinished == true)
        {
            writer.WriteInt32(dialogResult.result);
            writer.WriteInt32(dialogResult.buttonId);
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void MsgDialog::CloseDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        ret = TerminateDialog();

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    int MsgDialog::UpdateProgressBar(BinaryReader& reader, SceCommonDialogStatus status)
    {
        int ret = 0;

        Int32 updateType = reader.ReadInt32();

        if ((updateType & 1) != 0)
        {
            // Increment the bar
            UInt32 delta = reader.ReadUInt32();

            if (status == SCE_COMMON_DIALOG_STATUS_RUNNING)
            {
                ret = sceMsgDialogProgressBarInc(SCE_MSG_DIALOG_PROGRESSBAR_TARGET_BAR_DEFAULT, delta);
                if (ret < 0)
                {
                    return ret;
                }
            }
        }
        else if ((updateType & 2) != 0)
        {
            // Set value
            UInt32 rate = reader.ReadUInt32();

            if (status == SCE_COMMON_DIALOG_STATUS_RUNNING)
            {
                ret = sceMsgDialogProgressBarSetValue(SCE_MSG_DIALOG_PROGRESSBAR_TARGET_BAR_DEFAULT, rate);
                if (ret < 0)
                {
                    return ret;
                }
            }
        }

        if ((updateType & 4) != 0)
        {
            // Set msg
            char* msg = reader.ReadStringPtr();

            if (status == SCE_COMMON_DIALOG_STATUS_RUNNING)
            {
                ret = sceMsgDialogProgressBarSetMsg(SCE_MSG_DIALOG_PROGRESSBAR_TARGET_BAR_DEFAULT, msg);
                if (ret < 0)
                {
                    return ret;
                }
            }
        }

        return ret;
    }

    int MsgDialog::InitialzeDialog()
    {
        if (s_DialogInitialized == true) return 0;

        int ret = Utils::InitializeCommonDialog();

        if (ret < 0)
        {
            return ret;
        }

        ret = sceMsgDialogInitialize();

        if (ret < 0)
        {
            return ret;
        }

        s_DialogInitialized = true;

        return ret;
    }

    int MsgDialog::TerminateDialog()
    {
        if (s_DialogInitialized == false) return 0;

        int ret = sceMsgDialogTerminate();

        if (ret < 0)
        {
            return ret;
        }

        s_DialogInitialized = false;

        return ret;
    }
}
