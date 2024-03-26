#include "GameUpdate.h"
#include "HandleMsg.h"
#if !__ORBIS__
#include <libgameupdate.h>
#include <libsysmodule.h>

#pragma comment(lib,"libSceGameUpdate_stub_weak.a")

namespace psn
{
    bool GameUpdate::s_ModuleLoaded = false;


    void GameUpdate::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::GameUpdateCheck, GameUpdate::GameUpdateCheckImpl);
        MsgHandler::AddMethod(Methods::GameUpdateGetAddcontLatestVersion, GameUpdate::GameUpdateGetAddcontLatestVersionImpl);
    }

    void GameUpdate::InitializeLib()
    {
        int res = sceSysmoduleIsLoaded(SCE_SYSMODULE_GAME_UPDATE);
        if (res != SCE_SYSMODULE_LOADED)
        {
            res = sceSysmoduleLoadModule(SCE_SYSMODULE_GAME_UPDATE);

            if (res < 0)
            {
                UNITY_TRACE("Error loading SCE_SYSMODULE_GAME_UPDATE, 0x%x\n", res);
            }
            s_ModuleLoaded = true;
        }
        res = sceGameUpdateInitialize();
        if (res < 0)
        {
            UNITY_TRACE("Error calling sceGameUpdateInitialize(), 0x%x\n", res);
        }
    }

    void GameUpdate::TerminateLib()
    {
        int res = sceGameUpdateTerminate();
        if (res < 0)
        {
            UNITY_TRACE("Error calling sceGameUpdateTerminate(), 0x%x\n", res);
        }
        if (s_ModuleLoaded == true)
        {
            res = sceSysmoduleUnloadModule(SCE_SYSMODULE_GAME_UPDATE);

            if (res < 0)
            {
                UNITY_TRACE("Error unloading SCE_SYSMODULE_GAME_UPDATE, 0x%x\n", res);
            }
        }
    }

    void GameUpdate::GameUpdateCheckImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        SceGameUpdateCheckResult checkResult;

        BinaryReader reader(sourceData, sourceSize);

        UInt32 option = reader.ReadInt32();

        int ret = sceGameUpdateCreateRequest();
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        int requestId = ret;

        SceGameUpdateCheckParam checkParam;
        memset(&checkParam, 0, sizeof(checkParam));
        checkParam.size = sizeof(checkParam);
        checkParam.option = option;

        memset(&checkResult, 0, sizeof(checkResult));
        checkResult.size = sizeof(checkResult);

        ret = sceGameUpdateCheck(requestId, &checkParam, &checkResult);
        if (ret < 0)
        {
            if (ret == SCE_GAME_UPDATE_ERROR_INTERNAL)
            {
                printf("sceGameUpdateCheck() returned SCE_GAME_UPDATE_ERROR_INTERNAL. Check Debug Settings \"Update Test\"\n");  // https://p.siedev.net/technotes/view/424
            }
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(checkResult.found);
        writer.WriteBool(checkResult.addcontFound);
        writer.WriteString(checkResult.contentVersion);

        *resultsSize = writer.GetWrittenLength();

        sceGameUpdateDeleteRequest(requestId);

        SUCCESS_RESULT(result);
    }

    void GameUpdate::GameUpdateGetAddcontLatestVersionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        SceGameUpdateAddcontVersionInfo info;

        BinaryReader reader(sourceData, sourceSize);

        SceNpServiceLabel serviceLabel = reader.ReadUInt32();
        SceNpUnifiedEntitlementLabel entitlementLabel;
        char *entitlementLabelData = reader.ReadStringPtr();
        memset(&entitlementLabel, 0, sizeof(entitlementLabel));
        strcpy_s(entitlementLabel.data, SCE_NP_UNIFIED_ENTITLEMENT_LABEL_SIZE , entitlementLabelData);

        memset(&info, 0, sizeof(info));
        info.size = sizeof(info);

        int ret = sceGameUpdateGetAddcontLatestVersion(serviceLabel, &entitlementLabel, &info);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteBool(info.found);
        writer.WriteString(info.contentVersion);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }
}
#endif
