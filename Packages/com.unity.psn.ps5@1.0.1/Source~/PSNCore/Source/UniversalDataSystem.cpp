#include "UniversalDataSystem.h"
#include "HandleMsg.h"
#if !__ORBIS__

#pragma comment(lib,"libSceNpUniversalDataSystem_stub_weak.a")

namespace psn
{
    UserMap<UniversalDataSystem::UserContext> UniversalDataSystem::s_UsersList;

    UniversalDataSystem::UserContext::UserContext(SceUserServiceUserId userId)
    {
        m_userId = userId;
        m_context = SCE_NP_UNIVERSAL_DATA_SYSTEM_INVALID_HANDLE;
        m_handle = SCE_NP_UNIVERSAL_DATA_SYSTEM_INVALID_HANDLE;
    }

    int UniversalDataSystem::UserContext::Create()
    {
        int ret;

        ret = sceNpUniversalDataSystemCreateContext(&m_context, m_userId, 0, 0);

        if (ret < 0)
        {
            return ret;
        }

        ret = sceNpUniversalDataSystemCreateHandle(&m_handle);

        if (ret < 0)
        {
            return ret;
        }

        ret = sceNpUniversalDataSystemRegisterContext(m_context, m_handle, 0);

        if (ret < 0)
        {
            return ret;
        }

        return ret;
    }

    int UniversalDataSystem::UserContext::Destroy()
    {
        int ret;

        ret = sceNpUniversalDataSystemDestroyHandle(m_handle);

        if (ret < 0)
        {
            return ret;
        }

        ret = sceNpUniversalDataSystemDestroyContext(m_context);

        if (ret < 0)
        {
            return ret;
        }

        return ret;
    }

    void UniversalDataSystem::InitializeLib()
    {
    }

    void UniversalDataSystem::TerminateLib()
    {
    }

    void UniversalDataSystem::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::StartSystem, UniversalDataSystem::StartSystemImpl);
        MsgHandler::AddMethod(Methods::StopSystem, UniversalDataSystem::StopSystemImpl);
        MsgHandler::AddMethod(Methods::GetMemoryStats, UniversalDataSystem::GetMemoryStatsImpl);
        MsgHandler::AddMethod(Methods::PostEvent, UniversalDataSystem::PostEventImpl);
        MsgHandler::AddMethod(Methods::EventToString, UniversalDataSystem::EventToStringImpl);
        MsgHandler::AddMethod(Methods::UnlockTrophy, UniversalDataSystem::UnlockTrophyImpl);
        MsgHandler::AddMethod(Methods::UnlockTrophyProgress, UniversalDataSystem::UnlockTrophyProgressImpl);

        MsgHandler::RegisterUserCallback(HandleUserState);
    }

    void UniversalDataSystem::StartSystemImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        BinaryReader reader(sourceData, sourceSize);

        Int64 memSize = reader.ReadUInt64();

        int ret;
        SceNpUniversalDataSystemInitParam param;
        memset(&param, 0, sizeof(param));
        param.size = sizeof(param);
        param.poolSize = memSize;

        ret = sceNpUniversalDataSystemInitialize(&param);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void UniversalDataSystem::StopSystemImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        int ret = sceNpUniversalDataSystemTerminate();

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void UniversalDataSystem::GetMemoryStatsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        SceNpUniversalDataSystemMemoryStat stat;

        memset(&stat, 0, sizeof(SceNpUniversalDataSystemMemoryStat));

        int ret = sceNpUniversalDataSystemGetMemoryStat(&stat);
        if (ret < SCE_OK)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteUInt64(stat.poolSize);
        writer.WriteUInt64(stat.maxInuseSize);
        writer.WriteUInt64(stat.currentInuseSize);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void UniversalDataSystem::HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result)
    {
        if (state == MsgHandler::UserState::Added)
        {
            if (s_UsersList.DoesUserExist(userId) == true)
            {
                // User already registered so don't do this again
                WARNING_RESULT(result, "User already initialised with UDS service");
                return;
            }

            UserContext* user = s_UsersList.CreateUser(userId);

            user->Create();
        }
        else if (state == MsgHandler::UserState::Removed)
        {
            UserContext* user = s_UsersList.FindUser(userId);

            if (user == NULL)
            {
                WARNING_RESULT(result, "User not registered with UDS");
                return;
            }

            if (user->m_context == SCE_NP_UNIVERSAL_DATA_SYSTEM_INVALID_HANDLE)
            {
                ERROR_RESULT(result, "User context is invalid");
                return;
            }

            user->Destroy();

            s_UsersList.DeleteUser(userId);
        }

        SUCCESS_RESULT(result);
    }

    struct TrophyParams
    {
        SceUserServiceUserId userId;
        Int32 trophyId;
    };

    struct TrophyProgressParams
    {
        SceUserServiceUserId userId;
        Int32 trophyId;
        Int64 progress;
    };

    bool UniversalDataSystem::ReadProperty(BinaryReader& reader, SceNpUniversalDataSystemEventPropertyObject* properties, APIResult* result)
    {
        int ret = 0;

        char* key = reader.ReadStringPtr();

        PropertyType valueType = (PropertyType)reader.ReadUInt32();

        switch (valueType)
        {
            case PropertyType::kInt32:
                ret = sceNpUniversalDataSystemEventPropertyObjectSetInt32(properties, key, reader.ReadInt32());
                break;
            case PropertyType::kUInt32:
                ret = sceNpUniversalDataSystemEventPropertyObjectSetUInt32(properties, key, reader.ReadUInt32());
                break;
            case PropertyType::kInt64:
                ret = sceNpUniversalDataSystemEventPropertyObjectSetInt64(properties, key, reader.ReadInt64());
                break;
            case PropertyType::kUInt64:
                ret = sceNpUniversalDataSystemEventPropertyObjectSetUInt64(properties, key, reader.ReadUInt64());
                break;
            case PropertyType::kString:
                ret = sceNpUniversalDataSystemEventPropertyObjectSetString(properties, key, reader.ReadStringPtr());
                break;
            case PropertyType::kFloat:
                ret = sceNpUniversalDataSystemEventPropertyObjectSetFloat32(properties, key, reader.ReadFloat());
                break;
            case PropertyType::kFloat64:
                ret = sceNpUniversalDataSystemEventPropertyObjectSetFloat64(properties, key, reader.ReadDouble());
                break;
            case PropertyType::kBool:
                ret = sceNpUniversalDataSystemEventPropertyObjectSetBool(properties, key, reader.ReadBool());
                break;
            case PropertyType::kBinary:
            {
                int size = reader.ReadInt32();
                void* data = reader.ReadDataPtr(size);
                ret = sceNpUniversalDataSystemEventPropertyObjectSetBinary(properties, key, data, size);
            }
            break;
            case PropertyType::kProperties:
            {
                SceNpUniversalDataSystemEventPropertyObject* subProperties = ReadProperties(reader, result);

                if (subProperties == NULL)
                {
                    return false;
                }

                ret = sceNpUniversalDataSystemEventPropertyObjectSetObject(properties, key, subProperties, NULL);

                // once properties are set its ok to delete the object
                sceNpUniversalDataSystemDestroyEventPropertyObject(subProperties);
            }
            break;
            case PropertyType::kArray:
            {
                SceNpUniversalDataSystemEventPropertyArray* subArray = ReadPropertiesArray(reader, result);

                if (subArray == NULL)
                {
                    return false;
                }

                ret = sceNpUniversalDataSystemEventPropertyObjectSetArray(properties, key, subArray, NULL);

                // once properties are set its ok to delete the object
                sceNpUniversalDataSystemDestroyEventPropertyArray(subArray);
            }
            break;
            case PropertyType::kInvalid:
                break;
        }

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return false;
        }

        return true;
    }

    bool UniversalDataSystem::ReadArrayValue(BinaryReader& reader, PropertyType arrayType, SceNpUniversalDataSystemEventPropertyArray* propertiesArray, APIResult* result)
    {
        int ret = 0;

        switch (arrayType)
        {
            case PropertyType::kInt32:
                ret = sceNpUniversalDataSystemEventPropertyArraySetInt32(propertiesArray, reader.ReadInt32());
                break;
            case PropertyType::kUInt32:
                ret = sceNpUniversalDataSystemEventPropertyArraySetUInt32(propertiesArray, reader.ReadUInt32());
                break;
            case PropertyType::kInt64:
                ret = sceNpUniversalDataSystemEventPropertyArraySetInt64(propertiesArray, reader.ReadInt64());
                break;
            case PropertyType::kUInt64:
                ret = sceNpUniversalDataSystemEventPropertyArraySetUInt64(propertiesArray, reader.ReadUInt64());
                break;
            case PropertyType::kString:
                ret = sceNpUniversalDataSystemEventPropertyArraySetString(propertiesArray, reader.ReadStringPtr());
                break;
            case PropertyType::kFloat:
                ret = sceNpUniversalDataSystemEventPropertyArraySetFloat32(propertiesArray, reader.ReadFloat());
                break;
            case PropertyType::kFloat64:
                ret = sceNpUniversalDataSystemEventPropertyArraySetFloat64(propertiesArray, reader.ReadDouble());
                break;
            case PropertyType::kBool:
                ret = sceNpUniversalDataSystemEventPropertyArraySetBool(propertiesArray, reader.ReadBool());
                break;
            case PropertyType::kBinary:
            {
                int size = reader.ReadInt32();
                void* data = reader.ReadDataPtr(size);
                ret = sceNpUniversalDataSystemEventPropertyArraySetBinary(propertiesArray, data, size);
            }
            break;
            case PropertyType::kProperties:
            {
                SceNpUniversalDataSystemEventPropertyObject* subProperties = ReadProperties(reader, result);

                if (subProperties == NULL)
                {
                    return false;
                }

                ret = sceNpUniversalDataSystemEventPropertyArraySetObject(propertiesArray, subProperties, NULL);

                // once properties are set its ok to delete the object
                sceNpUniversalDataSystemDestroyEventPropertyObject(subProperties);
            }
            break;
            case PropertyType::kArray:
            {
                SceNpUniversalDataSystemEventPropertyArray* subArray = ReadPropertiesArray(reader, result);

                if (subArray == NULL)
                {
                    return false;
                }

                ret = sceNpUniversalDataSystemEventPropertyArraySetArray(propertiesArray, subArray, NULL);

                // once properties are set its ok to delete the object
                sceNpUniversalDataSystemDestroyEventPropertyArray(subArray);
            }
            break;
            default:
                break;
        }

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return false;
        }

        return true;
    }

    SceNpUniversalDataSystemEventPropertyArray* UniversalDataSystem::ReadPropertiesArray(BinaryReader& reader, APIResult* result)
    {
        SceNpUniversalDataSystemEventPropertyArray *propertiesArray;

        int ret = sceNpUniversalDataSystemCreateEventPropertyArray(&propertiesArray);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return NULL;
        }

        PropertyType arrayType = (PropertyType)reader.ReadUInt32();

        int numitems = reader.ReadUInt32();

        for (int i = 0; i < numitems; i++)
        {
            if (ReadArrayValue(reader, arrayType, propertiesArray, result) != true)
            {
                sceNpUniversalDataSystemDestroyEventPropertyArray(propertiesArray);
                return NULL;
            }
        }

        return propertiesArray;
    }

    SceNpUniversalDataSystemEventPropertyObject* UniversalDataSystem::ReadProperties(BinaryReader& reader, APIResult* result)
    {
        SceNpUniversalDataSystemEventPropertyObject *properties;

        int ret = sceNpUniversalDataSystemCreateEventPropertyObject(&properties);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return NULL;
        }

        int numprops = reader.ReadUInt32();

        for (int i = 0; i < numprops; i++)
        {
            if (ReadProperty(reader, properties, result) != true)
            {
                sceNpUniversalDataSystemDestroyEventPropertyObject(properties);
                return NULL;
            }
        }

        return properties;
    }

    SceNpUniversalDataSystemEvent* UniversalDataSystem::ReadEvent(BinaryReader& reader, APIResult* result)
    {
        int ret = 0;

        SceNpUniversalDataSystemEvent *udsEvent = NULL;
        SceNpUniversalDataSystemEventPropertyObject *properties = NULL;

        char* eventName;
        eventName = reader.ReadStringPtr();

        bool hasProperties = reader.ReadBool();

        if (hasProperties == false)
        {
            ret = sceNpUniversalDataSystemCreateEvent(eventName, NULL, &udsEvent, NULL);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return NULL;
            }
        }
        else
        {
            properties = ReadProperties(reader, result);

            if (properties == NULL)
            {
                // Properties should have been created but an error occured. APIResult should be correctly filled with error info from ReadProperties()
                return NULL;
            }

            // Read and create properties
            int ret = sceNpUniversalDataSystemCreateEvent(eventName, properties, &udsEvent, NULL);

            if (ret < 0)
            {
                sceNpUniversalDataSystemDestroyEventPropertyObject(properties);

                SCE_ERROR_RESULT(result, ret);
                return NULL;
            }

            // once properties are set its ok to delete the object
            sceNpUniversalDataSystemDestroyEventPropertyObject(properties);
        }

        return udsEvent;
    }

    void UniversalDataSystem::PostEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();
        bool reportEstimatedSize = reader.ReadBool();

        UserContext* user = s_UsersList.FindUser(userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with UDS");
            return;
        }

        // Read and create the event.
        SceNpUniversalDataSystemEvent* udsEvent = ReadEvent(reader, result);

        if (udsEvent != NULL)
        {
            ret = sceNpUniversalDataSystemPostEvent(user->m_context, user->m_handle, udsEvent, 0);

            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            size_t estimatedSize = 0;

            if (reportEstimatedSize == true)
            {
                ret = sceNpUniversalDataSystemEventEstimateSize(udsEvent, &estimatedSize);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }
            }

            sceNpUniversalDataSystemDestroyEvent(udsEvent);

            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            BinaryWriter writer(resultsData, resultsMaxSize);

            writer.WriteUInt64(estimatedSize);

            *resultsSize = writer.GetWrittenLength();
        }
        else
        {
            return;
        }

        SUCCESS_RESULT(result);
    }

    void UniversalDataSystem::EventToStringImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;
        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        UserContext* user = s_UsersList.FindUser(userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with UDS");
            return;
        }

        // Read and create the event.
        SceNpUniversalDataSystemEvent* udsEvent = ReadEvent(reader, result);

        if (udsEvent != NULL)
        {
            char buf[4096];
            ret = sceNpUniversalDataSystemEventToString(udsEvent, buf, sizeof(buf), NULL);

            sceNpUniversalDataSystemDestroyEvent(udsEvent);

            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            BinaryWriter writer(resultsData, resultsMaxSize);

            writer.WriteString(buf);

            *resultsSize = writer.GetWrittenLength();
        }
        else
        {
            return;
        }

        SUCCESS_RESULT(result);
    }

    //https://p.siedev.net/resources/documents/SDK/0.800/Trophy_System-Overview/0004.html
    void UniversalDataSystem::UnlockTrophyImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        TrophyParams* params = (TrophyParams *)(sourceData);

        UserContext* user = s_UsersList.FindUser(params->userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with UDS");
            return;
        }

        int ret;

        SceNpUniversalDataSystemEvent *event = NULL;
        SceNpUniversalDataSystemEventPropertyObject *prop = NULL;
        ret = sceNpUniversalDataSystemCreateEvent("_UnlockTrophy", NULL, &event, &prop);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }
        ret = sceNpUniversalDataSystemEventPropertyObjectSetInt32(prop, "_trophy_id", params->trophyId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }
        ret = sceNpUniversalDataSystemPostEvent(user->m_context, user->m_handle, event, 0);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        //char buf[256];
        //ret = sceNpUniversalDataSystemEventToString(event, buf, sizeof(buf), NULL);
        //if (ret < 0)
        //{
        //  // Error handling
        //}

        ret = sceNpUniversalDataSystemDestroyEvent(event);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void UniversalDataSystem::UnlockTrophyProgressImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        TrophyProgressParams* params = (TrophyProgressParams*)(sourceData);

        UserContext* user = s_UsersList.FindUser(params->userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with UDS");
            return;
        }

        int ret;

        SceNpUniversalDataSystemEvent* event = NULL;
        SceNpUniversalDataSystemEventPropertyObject* prop = NULL;
        ret = sceNpUniversalDataSystemCreateEvent("_UpdateTrophyProgress", NULL, &event, &prop);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }
        ret = sceNpUniversalDataSystemEventPropertyObjectSetInt32(prop, "_trophy_id", params->trophyId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }
        ret = sceNpUniversalDataSystemEventPropertyObjectSetInt32(prop, "_trophy_progress", (Int32)params->progress);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }
        ret = sceNpUniversalDataSystemPostEvent(user->m_context, user->m_handle, event, 0);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        //char buf[256];
        //ret = sceNpUniversalDataSystemEventToString(event, buf, sizeof(buf), NULL);
        //if (ret < 0)
        //{
        //  // Error handling
        //}

        ret = sceNpUniversalDataSystemDestroyEvent(event);

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }
}
#endif
