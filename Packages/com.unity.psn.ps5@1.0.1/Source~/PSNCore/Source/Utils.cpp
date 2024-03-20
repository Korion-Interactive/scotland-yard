#include "Utils.h"

#include <common_dialog.h>

#pragma comment(lib,"libSceCommonDialog_stub_weak.a")

#if __ORBIS__
#pragma comment(lib,"libSceNpManager_stub_weak.a")
#pragma comment(lib,"libSceNpCommon_stub_weak.a")
#endif

namespace psn
{
    int32_t addStringToVector(Common::LibContext* snccLibContext, const char* str, Common::Vector<Common::String>& vec)
    {
        int32_t ret = 0;

        Common::String addString(snccLibContext);
        ret = addString.append(str);
        if (ret < 0) return ret;

        ret = vec.pushBack(addString);
        if (ret < 0) return ret;

        return 0;
    }

    void GetAccountIdStr(SceNpAccountId accountId, char* accountIdBuf, size_t bufferSize)
    {
        sprintf_s(accountIdBuf, bufferSize, "%lu", accountId);
    }

    const char* Utils::GetThisPlatformString()
    {
#ifdef __PROSPERO__
        return "PS5";
#else
        return "PS4";
#endif
    }

    uint32_t Utils::GetThisPlatformFlag()
    {
#ifdef __PROSPERO__
        return PLATFORM_PS5_FLAG;
#else
        return PLATFORM_PS4_FLAG;
#endif
    }

    const char* Utils::ToPlatformString(uint32_t platformFlag)
    {
        if (platformFlag == PLATFORM_PS5_FLAG)
        {
            return "PS5";
        }

        if (platformFlag == PLATFORM_PS4_FLAG)
        {
            return "PS4";
        }

        return "NONE";
    }

    uint32_t Utils::GetPlatformFlag(const char* platform)
    {
        // Read the string to see what flags are set
        if (strncmp(platform, "PS5", 3) == 0)
        {
            return PLATFORM_PS5_FLAG;
        }

        if (strncmp(platform, "PROSPERO", 8) == 0)
        {
            return PLATFORM_PS5_FLAG;
        }

        if (strncmp(platform, "PS4", 3) == 0)
        {
            return PLATFORM_PS4_FLAG;
        }

        return 0;
    }

    int Utils::AddPlatformStrings(uint32_t platformFlags, Vector<String>& supportedPlatforms)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        int ret = 0;

        if ((platformFlags & PLATFORM_PS5_FLAG) != 0)
        {
            ret = addStringToVector(libContextPtr, "PS5", supportedPlatforms);
            if (ret < 0) return ret;
        }

        if ((platformFlags & PLATFORM_PS4_FLAG) != 0)
        {
            ret = addStringToVector(libContextPtr, "PS4", supportedPlatforms);
            if (ret < 0) return ret;
        }

        return ret;
    }

    void Utils::DeletePtr(Vector<String>* ptr)
    {
        delete ptr;
    }

    int Utils::InitializeCommonDialog()
    {
        int ret = sceCommonDialogInitialize();

        if (ret == SCE_COMMON_DIALOG_ERROR_ALREADY_SYSTEM_INITIALIZED)
        {
            return 0;
        }

        return ret;
    }
}
