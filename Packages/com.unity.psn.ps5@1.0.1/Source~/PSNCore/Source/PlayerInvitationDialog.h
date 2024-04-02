#pragma once
#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>
#include <np_cppwebapi.h>

namespace psn
{
    class PlayerInvitationDialog
    {
    public:

        enum Methods
        {
            OpenDialog = 0x1600001u,
            UpdateDialog = 0x1600002u,
            CloseDialog = 0x1600003u,
        };

        static void RegisterMethods();

        static void OpenDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void UpdateDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void CloseDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void InitializeLib();
        static void TerminateLib();

    private:

        static int InitializeDialog();
        static int TerminateDialog();

        static bool s_DialogInitialized;
    };
}
