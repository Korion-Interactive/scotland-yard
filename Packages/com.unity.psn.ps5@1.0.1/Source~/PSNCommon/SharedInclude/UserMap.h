#include <map>

namespace psn
{
    template<class UserData>
    class UserMap
    {
        std::map<SceUserServiceUserId, UserData*> s_UsersList;

    public:
        UserData* CreateUser(SceUserServiceUserId userId)
        {
            UserData* userData = new UserData(userId);

            s_UsersList.insert(std::pair<SceUserServiceUserId, UserData*>(userId, userData));

            return userData;
        }

        UserData* FindUser(SceUserServiceUserId userId)
        {
            auto it = s_UsersList.find(userId);

            if (it == s_UsersList.end())
            {
                return NULL;
            }

            return it->second;
        }

        void DeleteUser(SceUserServiceUserId userId)
        {
            auto it = s_UsersList.find(userId);

            if (it == s_UsersList.end())
            {
                return;
            }

            delete it->second;

            s_UsersList.erase(userId);
        }

        bool DoesUserExist(SceUserServiceUserId userId)
        {
            auto it = s_UsersList.find(userId);

            if (it == s_UsersList.end())
            {
                return false;
            }

            return true;
        }

        typedef void(*CleanUpCallback)(UserData* userData);

        void Clean(CleanUpCallback callback)
        {
            auto it = s_UsersList.begin();
            while (it != s_UsersList.end())
            {
                callback(it->second);

                delete it->second;

                s_UsersList.erase(it->first);

                it = s_UsersList.begin();
            }
        }
    };

    struct InitialiseData
    {
        SceUserServiceUserId userId;
    };

}
