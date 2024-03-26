#include "Bandwidth.h"
#include "HandleMsg.h"

#pragma comment(lib,"libSceNpUtility_stub_weak.a")

namespace psn
{
    void Bandwidth::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::StartMeasurement, Bandwidth::StartMeasurementImpl);
        MsgHandler::AddMethod(Methods::PollMeasurement, Bandwidth::PollMeasurementImpl);
        MsgHandler::AddMethod(Methods::AbortMeasurement, Bandwidth::AbortMeasurementImpl);
    }

    //

    void Bandwidth::StartMeasurementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 mode = reader.ReadInt32();
        Int32 timeoutMs = reader.ReadInt32();
        UInt64 cpuAffinityMask = reader.ReadUInt64();

        SceNpBandwidthTestInitParam param;
        memset(&param, 0x00, sizeof(param));
        param.size = sizeof(param);

        param.threadPriority = SCE_KERNEL_PRIO_FIFO_DEFAULT;

        param.cpuAffinityMask = cpuAffinityMask;

        UNITY_TRACE("Bandwidth mask = %lx\n", cpuAffinityMask);
//#if __ORBIS__
//        param.cpuAffinityMask = 0x3C; // 111100 - Don't use cores 0 and 1
//#else
//        //  param.cpuAffinityMask = SCE_KERNEL_CPUMASK_13CPU;
//        param.cpuAffinityMask = 0x1FC; // 1111111111100 - Don't use cores 0 and 1
//#endif

        int ret = -1;

        if (mode == 1) // CalcUploadBps
        {
            ret = sceNpBandwidthTestInitStartUpload(&param, timeoutMs * 1000);
        }
        else if (mode == 2) // CalcDownloadBps
        {
            ret = sceNpBandwidthTestInitStartDownload(&param, timeoutMs * 1000);
        }
        else
        {
            ERROR_RESULT(result, "Unknown bandwidth mode");
            return;
        }

        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        int status = GetStatus(ret);

        if (status < 0)
        {
            SCE_ERROR_RESULT(result, status);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(ret); // Write the ctx id
        writer.WriteInt32(status); // Write the status

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    int Bandwidth::GetStatus(int ctxId)
    {
        int status;
        int ret = sceNpBandwidthTestGetStatus(ctxId, &status);
        if (ret < 0)
        {
            return ret;
        }
        return status;
    }

    void Bandwidth::PollMeasurementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        BinaryReader reader(sourceData, sourceSize);

        Int32 ctxId = reader.ReadInt32();

        int status = GetStatus(ctxId);

        if (status < 0)
        {
            SCE_ERROR_RESULT(result, status);
            return;
        }

        SceNpBandwidthTestResult testResult;

        if (status == SCE_NP_BANDWIDTH_TEST_STATUS_FINISHED)
        {
            testResult.uploadBps = 0.0;
            testResult.downloadBps = 0.0;
            testResult.result = -1;

            int ret = sceNpBandwidthTestShutdown(ctxId, &testResult);

            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(status); // Write the status

        if (status == SCE_NP_BANDWIDTH_TEST_STATUS_FINISHED)
        {
            writer.WriteDouble(testResult.uploadBps); // Write the upload bps (bit per second)
            writer.WriteDouble(testResult.downloadBps); // Write the download bps (bit per second)
            writer.WriteInt32(testResult.result); // Write the result
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void Bandwidth::AbortMeasurementImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        SUCCESS_RESULT(result);
    }
}
