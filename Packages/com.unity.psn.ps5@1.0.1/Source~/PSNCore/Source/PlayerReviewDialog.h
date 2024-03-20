#pragma once
#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>
#include <np_cppwebapi.h>

namespace psn
{
    class PlayerReviewDialog
    {
    public:

        enum Methods
        {
            OpenDialog = 0x0F00001u,
            UpdateDialog = 0x0F00002u,
            CloseDialog = 0x0F00003u,
        };

        static void RegisterMethods();

        static void OpenDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void UpdateDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void CloseDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void InitializeLib();
        static void TerminateLib();

    private:

        static int InitialzeDialog();
        static int TerminateDialog();

        static bool s_DialogInitialized;
    };
}
