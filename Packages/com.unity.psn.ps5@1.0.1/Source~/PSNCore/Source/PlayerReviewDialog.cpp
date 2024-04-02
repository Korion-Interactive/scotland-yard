#include "PlayerReviewDialog.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include "WebApiNotifications.h"

#include <vector>
#if (SCE_PROSPERO_SDK_VERSION<0x06000000u)
#if !__ORBIS__
#include <player_review_dialog.h>
#endif
#endif

#if !__ORBIS__
namespace psn
{
    void PlayerReviewDialog::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::OpenDialog, PlayerReviewDialog::OpenDialogImpl);
        MsgHandler::AddMethod(Methods::UpdateDialog, PlayerReviewDialog::UpdateDialogImpl);
        MsgHandler::AddMethod(Methods::CloseDialog, PlayerReviewDialog::CloseDialogImpl);
    }

    void PlayerReviewDialog::InitializeLib()
    {
    }

    void PlayerReviewDialog::TerminateLib()
    {
    }

    bool PlayerReviewDialog::s_DialogInitialized = false;

    void PlayerReviewDialog::OpenDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
#if (SCE_PROSPERO_SDK_VERSION<0x06000000u)
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
        char* matchId = reader.ReadStringPtr();
        ScePlayerReviewMode mode = (ScePlayerReviewMode)reader.ReadInt32();

        ScePlayerReviewDialogParam param;
        scePlayerReviewDialogParamInitialize(&param);

        param.userId = userId;
        strncpy(param.matchId, matchId, SCE_PLAYER_REVIEW_DIALOG_MATCHID_LENGTH);
        param.mode = mode;

        ret = scePlayerReviewDialogOpen(&param);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            TerminateDialog();
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
#else
		*resultsSize = 0;
		SCE_ERROR_RESULT(result, SCE_KERNEL_ERROR_ENODEV);  // not supported
		return;
#endif
    }

    void PlayerReviewDialog::UpdateDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
#if (SCE_PROSPERO_SDK_VERSION<0x06000000u)
        int ret = 0;

        *resultsSize = 0;

        SceCommonDialogStatus status = scePlayerReviewDialogUpdateStatus();
        ScePlayerReviewDialogResult dialogResult;
        memset(&dialogResult, 0, sizeof(dialogResult));

        bool hasFinished = false;

        if (status == SCE_COMMON_DIALOG_STATUS_FINISHED)
        {
            ret = scePlayerReviewDialogGetResult(&dialogResult);

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
#else
		*resultsSize = 0;
		SCE_ERROR_RESULT(result, SCE_KERNEL_ERROR_ENODEV);  // not supported
		return;
#endif
    }

    void PlayerReviewDialog::CloseDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
#if (SCE_PROSPERO_SDK_VERSION<0x06000000u)
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
#else
		*resultsSize = 0;
		SCE_ERROR_RESULT(result, SCE_KERNEL_ERROR_ENODEV);  // not supported
		return;
#endif
    }

    int PlayerReviewDialog::InitialzeDialog()
    {
#if (SCE_PROSPERO_SDK_VERSION<0x06000000u)
        if (s_DialogInitialized == true) return 0;

        int ret = Utils::InitializeCommonDialog();

        if (ret < 0)
        {
            return ret;
        }

        ret = scePlayerReviewDialogInitialize();

        if (ret < 0)
        {
            return ret;
        }

        s_DialogInitialized = true;

        return ret;
#else
		return SCE_KERNEL_ERROR_ENODEV;
#endif
    }

    int PlayerReviewDialog::TerminateDialog()
    {
#if (SCE_PROSPERO_SDK_VERSION<0x06000000u)
        if (s_DialogInitialized == false) return 0;

        int ret = scePlayerReviewDialogTerminate();

        if (ret < 0)
        {
            return ret;
        }

        s_DialogInitialized = false;

        return ret;
#else
		return SCE_KERNEL_ERROR_ENODEV;
#endif
    }
}
#endif

