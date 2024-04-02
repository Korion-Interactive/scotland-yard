#include "Authentication.h"
#include "HandleMsg.h"

#include <np/np_auth.h>

#pragma comment(lib,"libSceNpAuth_stub_weak.a")

namespace psn
{
    void Authentication::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::GetAuthorizationCode, Authentication::GetAuthorizationCodeImpl);
        MsgHandler::AddMethod(Methods::GetIdToken, Authentication::GetIdTokenImpl);
    }

    void Authentication::GetAuthorizationCodeImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        SceNpAuthCreateAsyncRequestParameter asyncParam;
        SceNpAuthGetAuthorizationCodeParameterV3 authParam;

        SceNpClientId clientId;

        SceNpAuthorizationCode authCode;
        int issuerId = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        char* clientIdStr = reader.ReadStringPtr();
        char* scopeStr = reader.ReadStringPtr();

        memset(&asyncParam, 0, sizeof(asyncParam));
        asyncParam.size = sizeof(asyncParam);

        int ret = sceNpAuthCreateAsyncRequest(&asyncParam);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        int requestId = ret;

        memset(&clientId, 0, sizeof(clientId));
        strncpy(clientId.id, clientIdStr, SCE_NP_CLIENT_ID_MAX_LEN);

        memset(&authParam, 0, sizeof(authParam));
        authParam.size = sizeof(authParam);
        authParam.userId = userId;
        authParam.clientId = &clientId;
        authParam.scope = scopeStr;

        ret = sceNpAuthGetAuthorizationCodeV3(requestId, &authParam, &authCode, &issuerId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        int asyncResult = 0;

        ret = sceNpAuthWaitAsync(requestId, &asyncResult);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (asyncResult < 0)
        {
            SCE_ERROR_RESULT(result, asyncResult);
            return;
        }

        if (0 < requestId)
        {
            ret = sceNpAuthDeleteRequest(requestId);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, asyncResult);
                return;
            }
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(issuerId);
        writer.WriteString(authCode.code);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void Authentication::GetIdTokenImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        SceNpAuthCreateAsyncRequestParameter asyncParam;
        SceNpAuthGetIdTokenParameterV3 authParam;
        SceNpClientId clientId;
        SceNpClientSecret clientSecret;
        SceNpIdToken idToken;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        char* clientIdStr = reader.ReadStringPtr();
        char* clientSecretStr = reader.ReadStringPtr();
        char* scopeStr = reader.ReadStringPtr();

        memset(&asyncParam, 0, sizeof(asyncParam));
        asyncParam.size = sizeof(asyncParam);

        int ret = sceNpAuthCreateAsyncRequest(&asyncParam);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        int requestId = ret;

        memset(&clientId, 0, sizeof(clientId));
        strncpy(clientId.id, clientIdStr, SCE_NP_CLIENT_ID_MAX_LEN);

        memset(&clientSecret, 0, sizeof(clientSecret));
        strncpy(clientSecret.secret, clientSecretStr, SCE_NP_CLIENT_SECRET_MAX_LEN);

        memset(&authParam, 0, sizeof(authParam));
        authParam.size = sizeof(authParam);
        authParam.userId = userId;
        authParam.clientId = &clientId;
        authParam.clientSecret = &clientSecret;
        authParam.scope = scopeStr;

        ret = sceNpAuthGetIdTokenV3(requestId, &authParam, &idToken);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        int asyncResult = 0;

        ret = sceNpAuthWaitAsync(requestId, &asyncResult);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        if (asyncResult < 0)
        {
            SCE_ERROR_RESULT(result, asyncResult);
            return;
        }

        if (0 < requestId)
        {
            ret = sceNpAuthDeleteRequest(requestId);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, asyncResult);
                return;
            }
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteString(idToken.token);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }
}
