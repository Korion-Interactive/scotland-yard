#include "OnlineSafety.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include <np_cppwebapi.h>
#include <np_cppwebapi\profanity_filter.h>

namespace CppWebApi = sce::Np::CppWebApi;
namespace CRS = sce::Np::CppWebApi::CommunicationRestrictionStatus::V3;

namespace Profanity = sce::Np::CppWebApi::ProfanityFilter::V2;

namespace psn
{
    void OnlineSafety::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::GetCRS, OnlineSafety::GetCRSImpl);
        MsgHandler::AddMethod(Methods::FilterProfanity, OnlineSafety::FilterProfanityImpl);
        MsgHandler::AddMethod(Methods::TestProfanity, OnlineSafety::TestProfanityImpl);
    }

    enum CRStatus  // Communication Restriction Status
    {
        NOT_CHECKED = 0,
        CHECK_FAILED = 1,
        NOT_SIGNED_UP = 2,
        USER_NOT_FOUND = 3,
        SIGNED_OUT = 4,
        RESTRICTED = 5,
        UNRESTRICTED = 6,
        SIGNED_IN_NOT_REGISTERED = 7
    };

    void OnlineSafety::GetCRSImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        CRStatus status = CHECK_FAILED;

        bool signedup = false;
        int ret = sceNpHasSignedUp(userId, &signedup);

        if (ret < 0)
        {
            if (ret == SCE_NP_ERROR_USER_NOT_FOUND)
            {
                status = USER_NOT_FOUND;
            }
            else
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }
        else
        {
            if (signedup == false)
            {
                status = NOT_SIGNED_UP;
            }
            else
            {
                SceNpAccountId accountId = SCE_NP_INVALID_ACCOUNT_ID;

                ret = sceNpGetAccountIdA(userId, &accountId);

                if (ret < 0)
                {
                    if (ret == SCE_NP_ERROR_SIGNED_OUT) status = SIGNED_OUT;
                    else if (ret == SCE_NP_ERROR_NOT_SIGNED_UP) status = NOT_SIGNED_UP;
                    else if (ret == SCE_NP_ERROR_USER_NOT_FOUND) status = USER_NOT_FOUND;
                    else
                    {
                        SCE_ERROR_RESULT(result, ret);
                        return;
                    }
                }
                else
                {
                    WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

                    if (userWebCtx == NULL)
                    {
                        status = SIGNED_IN_NOT_REGISTERED;
                    }
                    else
                    {
                        CRS::CommunicationRestrictionStatusApi::ParameterToGetCommunicationRestrictionStatus param;
                        CppWebApi::Common::IntrusivePtr<CRS::CommunicationRestrictionStatusResponse> response;

                        CppWebApi::Common::Transaction<
                            CppWebApi::Common::IntrusivePtr<CRS::CommunicationRestrictionStatusResponse> > trans;

                        Common::LibContext* libContextPtr = &WebApi::Instance()->m_cppWebapiLibCtx;

                        ret = trans.start(libContextPtr);
                        if (ret < 0)
                        {
                            SCE_ERROR_RESULT(result, ret);
                            return;
                        }

                        ret = param.initialize(libContextPtr, accountId);
                        if (ret < 0)
                        {
                            trans.finish();
                            SCE_ERROR_RESULT(result, ret);
                            return;
                        }

                        ret = CRS::CommunicationRestrictionStatusApi::getCommunicationRestrictionStatus(userWebCtx->m_webapiUserCtxId, param, trans);
                        if (ret < 0)
                        {
                            param.terminate();
                            trans.finish();
                            SCE_ERROR_RESULT(result, ret);
                            return;
                        }

                        ret = trans.getResponse(response);
                        if (ret < 0)
                        {
                            param.terminate();
                            trans.finish();
                            SCE_ERROR_RESULT(result, ret);
                            return;
                        }

                        if (response->getRestricted())
                        {
                            status = RESTRICTED;
                        }
                        else
                        {
                            status = UNRESTRICTED;
                        }

                        trans.finish();
                        param.terminate();
                    }
                }
            }
        }

        // Write results
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteUInt32(status);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void OnlineSafety::FilterProfanityImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();
        char* locale = reader.ReadStringPtr();
        char* textToFilter = reader.ReadStringPtr();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        Common::Transaction<Common::IntrusivePtr<Profanity::FilterProfanityResponse> > transaction;

        Common::LibContext* libContextPtr = &WebApi::Instance()->m_cppWebapiLibCtx;

        /*E Start the transaction. */
        int ret = transaction.start(libContextPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        Common::IntrusivePtr<Profanity::WebApiFilterRequest> pFilterRequest;
        ret = Profanity::WebApiFilterRequestFactory::create(libContextPtr, textToFilter, &pFilterRequest);
        if (ret < 0)
        {
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        Profanity::ProfanityApi::ParameterToFilterProfanity param;
        ret = param.initialize(libContextPtr, locale, "0", pFilterRequest);
        if (ret < 0)
        {
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        /*E Submit the text to the Profanity Filter service. */
        ret = Profanity::ProfanityApi::filterProfanity(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        Common::IntrusivePtr<Profanity::FilterProfanityResponse> respPtr;
        ret = transaction.getResponse(respPtr);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        const char* outputStr = respPtr->getMessage().c_str();

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteString(outputStr);

        *resultsSize = writer.GetWrittenLength();

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }

    void OnlineSafety::TestProfanityImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();
        char* locale = reader.ReadStringPtr();
        char* textToFilter = reader.ReadStringPtr();

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        Common::Transaction<Common::IntrusivePtr<Profanity::TestForProfanityResponse> > transaction;

        Common::LibContext* libContextPtr = &WebApi::Instance()->m_cppWebapiLibCtx;

        /*E Start the transaction. */
        int ret = transaction.start(libContextPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        Common::IntrusivePtr<Profanity::WebApiFilterRequest> pFilterRequest;
        ret = Profanity::WebApiFilterRequestFactory::create(libContextPtr, textToFilter, &pFilterRequest);
        if (ret < 0)
        {
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        Profanity::ProfanityApi::ParameterToTestForProfanity param;
        ret = param.initialize(libContextPtr, locale, "0", pFilterRequest);
        if (ret < 0)
        {
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        /*E Submit the text to the Profanity Filter service. */
        ret = Profanity::ProfanityApi::testForProfanity(userWebCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        Common::IntrusivePtr<Profanity::TestForProfanityResponse> respPtr;
        ret = transaction.getResponse(respPtr);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        const char* outputStr = respPtr->getMessage().c_str();

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteString(outputStr);

        *resultsSize = writer.GetWrittenLength();

        transaction.finish();
        param.terminate();

        SUCCESS_RESULT(result);
    }
}
