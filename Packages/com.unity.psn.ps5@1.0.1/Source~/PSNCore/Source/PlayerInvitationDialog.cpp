#include "PlayerInvitationDialog.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include "WebApiNotifications.h"

#include <vector>
#include <string.h>

#if !__ORBIS__
#pragma comment(lib,"libScePlayerInvitationDialog_stub_weak.a")

#include <player_invitation_dialog.h>
#include <libsysmodule.h>
#endif

#if !__ORBIS__
namespace psn
{
    void PlayerInvitationDialog::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::OpenDialog, PlayerInvitationDialog::OpenDialogImpl);
        MsgHandler::AddMethod(Methods::UpdateDialog, PlayerInvitationDialog::UpdateDialogImpl);
        MsgHandler::AddMethod(Methods::CloseDialog, PlayerInvitationDialog::CloseDialogImpl);
    }

    void PlayerInvitationDialog::InitializeLib()
    {
    }

    void PlayerInvitationDialog::TerminateLib()
    {
    }

    bool PlayerInvitationDialog::s_DialogInitialized = false;

    void PlayerInvitationDialog::OpenDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        ret = InitializeDialog();

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        char* sessionId = reader.ReadStringPtr();
        ScePlayerInvitationDialogMode mode = (ScePlayerInvitationDialogMode)reader.ReadInt32();

        ScePlayerInvitationDialogParam param;
        scePlayerInvitationDialogParamInitialize(&param);

        ScePlayerInvitationDialogSendParam sendParam;
        memset(&sendParam, 0, sizeof(sendParam));
        sendParam.sessionId = sessionId;

        param.userId = userId;
        param.mode = mode;
        param.sendParam = &sendParam;

        ret = scePlayerInvitationDialogOpen(&param);

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

    void PlayerInvitationDialog::UpdateDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        SceCommonDialogStatus status = scePlayerInvitationDialogUpdateStatus();
        ScePlayerInvitationDialogResult dialogResult;
        memset(&dialogResult, 0, sizeof(dialogResult));

        bool hasFinished = false;

        if (status == SCE_COMMON_DIALOG_STATUS_FINISHED)
        {
            ret = scePlayerInvitationDialogGetResult(&dialogResult);

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
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void PlayerInvitationDialog::CloseDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

    int PlayerInvitationDialog::InitializeDialog()
    {
        if (s_DialogInitialized == true) return 0;

        int ret = Utils::InitializeCommonDialog();

        if (ret < 0)
        {
            return ret;
        }

        ret = sceSysmoduleIsLoaded(SCE_SYSMODULE_PLAYER_INVITATION_DIALOG);
        if (ret != SCE_OK)
        {
            if (ret == SCE_SYSMODULE_ERROR_UNLOADED)
            {
                ret = sceSysmoduleLoadModule(SCE_SYSMODULE_PLAYER_INVITATION_DIALOG);
            }

            if (ret != SCE_OK)
            {
                return ret;
            }
        }

        ret = scePlayerInvitationDialogInitialize();

        if (ret < 0)
        {
            return ret;
        }

        s_DialogInitialized = true;

        return ret;
    }

    int PlayerInvitationDialog::TerminateDialog()
    {
        if (s_DialogInitialized == false) return 0;

        int ret = scePlayerInvitationDialogTerminate();

        if (ret < 0)
        {
            return ret;
        }

        s_DialogInitialized = false;

        return ret;
    }
}
#endif
