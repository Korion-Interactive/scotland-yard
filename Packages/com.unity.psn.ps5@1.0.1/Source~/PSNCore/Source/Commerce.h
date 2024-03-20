#pragma once
#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>
#include <np_cppwebapi.h>

#if !__ORBIS__
namespace psn
{
    class CommerceCommands
    {
    public:

        enum Methods
        {
            OpenDialog = 0x0E00001u,
            UpdateDialog = 0x0E00002u,
            CloseDialog = 0x0E00003u,
            PSStoreIcon = 0x0E00004u,
        };

        static void RegisterMethods();

        static void OpenDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void UpdateDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void CloseDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void PSStoreIconImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void InitializeLib();
        static void TerminateLib();

    private:

        static int InitialzeDialog();
        static int TerminateDialog();

        static bool s_DialogInitialized;
    };
}
#endif
