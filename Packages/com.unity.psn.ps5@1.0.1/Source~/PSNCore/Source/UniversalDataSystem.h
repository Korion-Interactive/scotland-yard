#pragma once
#include "SharedCoreIncludes.h"
//#include <map>

namespace psn
{
    enum PropertyType
    {
        kInvalid = 0,
        kInt32,
        kUInt32,
        kInt64,
        kUInt64,
        kString,
        kFloat,
        kFloat64,
        kBool,
        kBinary,
        kProperties,
        kArray
    };

    class UniversalDataSystem
    {
    public:

        enum Methods
        {
            StartSystem = 0x0200001u,
            StopSystem = 0x0200002u,
            GetMemoryStats = 0x0200003u,
            PostEvent = 0x0200006u,
            EventToString = 0x0200007u,
            UnlockTrophy = 0x0200010u,
            UnlockTrophyProgress = 0x0200011u,
        };

        class UserContext
        {
        public:

            UserContext(SceUserServiceUserId userId);

            int Create();
            int Destroy();

            SceUserServiceUserId m_userId;
#if !__ORBIS__
            SceNpUniversalDataSystemContext m_context;
            SceNpUniversalDataSystemHandle m_handle;
#endif
        };


        static void InitializeLib();
        static void TerminateLib();

        static void RegisterMethods();

        static void HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result);

        static void StartSystemImpl(UInt8* sourceData, int sourceSize, APIResult* result);
        static void StopSystemImpl(UInt8* sourceData, int sourceSize, APIResult* result);
        static void GetMemoryStatsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void PostEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void EventToStringImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void UnlockTrophyImpl(UInt8* sourceData, int sourceSize, APIResult* result);
        static void UnlockTrophyProgressImpl(UInt8* sourceData, int sourceSize, APIResult* result);

#if !__ORBIS__
        static SceNpUniversalDataSystemEvent* ReadEvent(BinaryReader& reader, APIResult* result);
        static SceNpUniversalDataSystemEventPropertyObject* ReadProperties(BinaryReader& reader, APIResult* result);
        static SceNpUniversalDataSystemEventPropertyArray* ReadPropertiesArray(BinaryReader& reader, APIResult* result);

        static bool ReadArrayValue(BinaryReader& reader, PropertyType arrayType, SceNpUniversalDataSystemEventPropertyArray* propertiesArray, APIResult* result);
        static bool ReadProperty(BinaryReader& reader, SceNpUniversalDataSystemEventPropertyObject* properties, APIResult* result);
#endif
        static UserMap<UserContext> s_UsersList;
    };
}
