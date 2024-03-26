#pragma once
#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>
#include <np_cppwebapi.h>

#include <message_dialog.h>

namespace psn
{
    class MsgDialog
    {
    public:

        enum Methods
        {
            OpenDialog = 0x1000001u,
            UpdateDialog = 0x1000002u,
            CloseDialog = 0x1000003u,
        };

        static void RegisterMethods();

        static void OpenDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void UpdateDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void CloseDialogImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static int UpdateProgressBar(BinaryReader& reader, SceCommonDialogStatus status);

        static void InitializeLib();
        static void TerminateLib();

    private:

        static int InitialzeDialog();
        static int TerminateDialog();

        static bool s_DialogInitialized;
        static bool s_ModuleLoaded;
    };
}
