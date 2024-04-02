#pragma once
#include <map>

namespace psn
{
    template<class SessionType>
    class SessionMap
    {
        std::map<int, SessionType*> s_SessionsList;

    public:
        void AddSession(int id, SessionType* instance)
        {
            s_SessionsList.insert(std::pair<int, SessionType*>(id, instance));
        }

        SessionType* FindSession(int internalId)
        {
            auto it = s_SessionsList.find(internalId);

            if (it == s_SessionsList.end())
            {
                return NULL;
            }

            return it->second;
        }

        SessionType* FindSession(const char* sessionId)
        {
            //  std::map<int, SessionType*>::iterator it;

            for (auto  it = s_SessionsList.begin(); it != s_SessionsList.end(); it++)
            {
                const char* itSessionId = it->second->GetSessionId();

                if (strcmp(itSessionId, sessionId) == 0)
                {
                    return it->second;
                }
            }

            return NULL;
        }

        void DeleteSession(int internalId)
        {
            auto it = s_SessionsList.find(internalId);

            if (it == s_SessionsList.end())
            {
                return;
            }

            delete it->second;

            s_SessionsList.erase(internalId);
        }

        bool DoesSessionExist(int internalId)
        {
            auto it = s_SessionsList.find(internalId);

            if (it == s_SessionsList.end())
            {
                return false;
            }

            return true;
        }

        typedef void(*CleanUpCallback)(SessionType* instance);

        void Clean(CleanUpCallback callback)
        {
            auto it = s_SessionsList.begin();
            while (it != s_SessionsList.end())
            {
                callback(it->second);

                delete it->second;

                s_SessionsList.erase(it->first);

                it = s_SessionsList.begin();
            }
        }
    };
}
