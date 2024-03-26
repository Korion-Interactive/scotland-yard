#ifndef PSN_UTILS
#define PSN_UTILS

#include "SharedCoreIncludes.h"
#include "WebApi.h"

namespace psn
{
#define PLATFORM_PS5_FLAG 1
#define PLATFORM_PS4_FLAG 2

    int32_t addStringToVector(Common::LibContext* snccLibContext, const char* str, Common::Vector<Common::String>& vec);

    void GetAccountIdStr(SceNpAccountId accountId, char* accountIdBuf, size_t bufferSize);

    template<size_t ArraySize>
    inline void copyString(char(&dst)[ArraySize], const char* src)
    {
        strncpy(&dst[0], src, ArraySize - 1);
        dst[ArraySize - 1] = '\0';
    }

    template<size_t ArraySize>
    inline void toRtcDateTimeStr(char(&dst)[ArraySize], const char* timeMsStr)
    {
        static_assert(ArraySize >= 27, "too short"); // YYYY-MM-DD HH:MM:SS.mmmmmm\0

        const unsigned long timeMs = strtoul(timeMsStr, nullptr, 10);
        const time_t timeSec = static_cast<time_t>(timeMs / 1000);

        SceRtcDateTime rtcDateTime;
        int ret = sceRtcSetTime_t(&rtcDateTime, timeSec);
        if (ret < 0)
        {
            //PRINT_ERROR(ret);
            return;
        }
        rtcDateTime.microsecond = (timeMs - (timeSec * 1000)) * 1000;

        snprintf(dst, ArraySize, "%04d-%02d-%02d %02d:%02d:%02d.%06d",
            sceRtcGetYear(&rtcDateTime), sceRtcGetMonth(&rtcDateTime), sceRtcGetDay(&rtcDateTime),
            sceRtcGetHour(&rtcDateTime), sceRtcGetMinute(&rtcDateTime), sceRtcGetSecond(&rtcDateTime),
            sceRtcGetMicrosecond(&rtcDateTime));
    }

    template<class ObjectType>
    class IDMap
    {
        std::map<int, ObjectType*> s_ObjectList;

    public:
        void Add(int id, ObjectType* instance)
        {
            s_ObjectList.insert(std::pair<int, ObjectType*>(id, instance));
        }

        ObjectType* Find(int internalId)
        {
            auto it = s_ObjectList.find(internalId);

            if (it == s_ObjectList.end())
            {
                return NULL;
            }

            return it->second;
        }

        void Remove(int internalId)
        {
            auto it = s_ObjectList.find(internalId);

            if (it == s_ObjectList.end())
            {
                return;
            }

            s_ObjectList.erase(internalId);
        }

        bool DoesExist(int internalId)
        {
            auto it = s_ObjectList.find(internalId);

            if (it == s_ObjectList.end())
            {
                return false;
            }

            return true;
        }

        typedef void(*CleanUpCallback)(ObjectType* instance);

        void Clean(CleanUpCallback callback)
        {
            auto it = s_ObjectList.begin();
            while (it != s_ObjectList.end())
            {
                callback(it->second);

                delete it->second;

                s_ObjectList.erase(it->first);

                it = s_ObjectList.begin();
            }
        }
    };

    class Utils
    {
    public:
        static uint32_t GetThisPlatformFlag();
        static const char* GetThisPlatformString();
        static const char* ToPlatformString(uint32_t platformFlag);
        static uint32_t GetPlatformFlag(const char* platform);
        static int AddPlatformStrings(uint32_t platformFlags, Vector<String>& supportedPlatforms);

        static void DeletePtr(Vector<String>* ptr);

        static int InitializeCommonDialog();
    };
}
#endif
