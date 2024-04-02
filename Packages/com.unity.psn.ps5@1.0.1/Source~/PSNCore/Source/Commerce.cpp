#include "Commerce.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include "WebApiNotifications.h"

#include <vector>
#include <np_commerce_dialog.h>

#pragma comment(lib,"libSceNpCommerce_stub_weak.a")

#if !__ORBIS__
namespace psn
{
    void CommerceCommands::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::OpenDialog, CommerceCommands::OpenDialogImpl);
        MsgHandler::AddMethod(Methods::UpdateDialog, CommerceCommands::UpdateDialogImpl);
        MsgHandler::AddMethod(Methods::CloseDialog, CommerceCommands::CloseDialogImpl);
        MsgHandler::AddMethod(Methods::PSStoreIcon, CommerceCommands::PSStoreIconImpl);
    }

    void CommerceCommands::InitializeLib()
    {
    }

    void CommerceCommands::TerminateLib()
    {
    }

    bool CommerceCommands::s_DialogInitialized = false;

    void CommerceCommands::OpenDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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
        Int32 mode = reader.ReadInt32();
        Int32 userId = reader.ReadInt32();
        UInt32 serviceLabel = reader.ReadUInt32();
        Int32 numTargets = reader.ReadInt32();

        const char** targets = NULL;

        if (numTargets > 0)
        {
            targets = (const char**)alloca(sizeof(void*) * numTargets);
            for (int i = 0; i < numTargets; i++)
                targets[i] = reader.ReadStringPtr();
        }

        SceNpCommerceDialogParam param;
        sceNpCommerceDialogParamInitialize(&param);

        param.userId = userId;
        param.serviceLabel = serviceLabel;
        param.mode = mode;
        param.numTargets = numTargets;
        param.targets = targets;
        param.features = SCE_NP_PREMIUM_FEATURE_REALTIME_MULTIPLAY;

        ret = sceNpCommerceDialogOpen(&param);

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

    void CommerceCommands::UpdateDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        SceCommonDialogStatus status = sceNpCommerceDialogUpdateStatus();
        SceNpCommerceDialogResult dialogResult;
        bool hasFinished = false;

        if (status == SCE_COMMON_DIALOG_STATUS_FINISHED)
        {
            ret = sceNpCommerceDialogGetResult(&dialogResult);

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

    void CommerceCommands::CloseDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
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

    int CommerceCommands::InitialzeDialog()
    {
        if (s_DialogInitialized == true) return 0;

        int ret = Utils::InitializeCommonDialog();

        if (ret < 0)
        {
            return ret;
        }

        ret = sceNpCommerceDialogInitialize();

        if (ret < 0)
        {
            return ret;
        }

        s_DialogInitialized = true;

        return ret;
    }

    int CommerceCommands::TerminateDialog()
    {
        if (s_DialogInitialized == false) return 0;

        int ret = sceNpCommerceDialogTerminate();

        if (ret < 0)
        {
            return ret;
        }

        s_DialogInitialized = false;

        return ret;
    }

    void CommerceCommands::PSStoreIconImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = 0;

        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 mode = reader.ReadInt32();
        SceNpCommercePsStoreIconPos pos = (SceNpCommercePsStoreIconPos)reader.ReadInt32();
        SceNpCommercePsStoreIconLayout layout = (SceNpCommercePsStoreIconLayout)reader.ReadInt32();

        ret = sceNpCommerceSetPsStoreIconLayout(layout);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (mode == 0) // Show
        {
            ret = sceNpCommerceShowPsStoreIcon(pos);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }
        else if (mode == 1)
        {
            ret = sceNpCommerceHidePsStoreIcon();
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }
}
#endif
