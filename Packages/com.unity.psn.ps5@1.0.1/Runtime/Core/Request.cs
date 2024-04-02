using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Unity.PSN.PS5.Internal
{
    internal class MarshalMethods
    {
        [DllImport("__Internal")]
        private static extern void PSN_PrxProcessMsg(IntPtr sourceData, int sourceSize, IntPtr resultsData, int resultsMaxSize, out int resultsSize, out APIResult result);

        static void Send(byte[] sourceData, UInt32 sourceSize, byte[] resultsData, out int resultsSize, out APIResult callResult)
        {
            GCHandle pinnedSourceArray = GCHandle.Alloc(sourceData, GCHandleType.Pinned);
            IntPtr sourcePointer = pinnedSourceArray.AddrOfPinnedObject();

            GCHandle pinnedResultsArray = GCHandle.Alloc(resultsData, GCHandleType.Pinned);
            IntPtr resultsPointer = pinnedResultsArray.AddrOfPinnedObject();

            //  Debug.Log("sourcePointer = " + sourcePointer.ToString("X"));
            //  Debug.Log("resultsPointer = " + resultsPointer.ToString("X"));

            PSN_PrxProcessMsg(sourcePointer, (int)sourceSize, resultsPointer, resultsData.Length, out resultsSize, out callResult);

            pinnedSourceArray.Free();
            pinnedResultsArray.Free();
        }

        static void WriteMethodHeader(UInt32 methodId, BinaryWriter buffer)
        {
            buffer.Write(headerVersion); // Version
            buffer.Write(methodId);
        }

        internal struct MethodHandle : System.IDisposable
        {
            public APIResult callResult;
            internal DataBuffers buffers;

            private int resultsSize;

            public BinaryWriter Writer { get { return buffers.writer; } }
            public BinaryReader Reader { get { return buffers.reader; } }

            public int ResultsSize { get { return resultsSize; } }

            public void Call()
            {
                resultsSize = 0;

                Send(buffers.data, (UInt32)buffers.writeStream.Position, buffers.results, out resultsSize, out callResult);
            }

            public void Dispose ()
            {
                ReleaseHandle(this);
            }
        }

        static System.Object queueSyncObj = new System.Object();

        internal static MethodHandle PrepareMethod(UInt32 methodId)
        {
            MethodHandle newHandle = new MethodHandle();

            lock (queueSyncObj)
            {
#if DEBUG
                if (availableBuffers.Count == 0)
                {
                    Debug.LogError("PrepareMethod has ran out of methods.");
                }
#endif

                newHandle.buffers = availableBuffers.Dequeue();
            }

            newHandle.buffers.writeStream.Position = 0;
            newHandle.buffers.readStream.Position = 0;

            WriteMethodHeader(methodId, newHandle.buffers.writer);

            return newHandle;
        }

        internal static void ReleaseHandle(MethodHandle handle)
        {
            bool alreadyReleased = handle.buffers == null;
            if (alreadyReleased)
                return;
            lock (queueSyncObj)
            {
                availableBuffers.Enqueue(handle.buffers);
                handle.buffers = null;
            }
        }

        const UInt32 headerVersion = 1;

        internal class DataBuffers
        {
            public byte[] data;
            public byte[] results;

            public MemoryStream writeStream;
            public MemoryStream readStream;

            public BinaryWriter writer;
            public BinaryReader reader;

            public DataBuffers(UInt32 dataBufferSize, UInt32 resultsBufferSize)
            {
                data = new byte[dataBufferSize];
                results = new byte[resultsBufferSize];

                writeStream = new MemoryStream(data);
                readStream = new MemoryStream(results);

                writer = new BinaryWriter(writeStream);
                reader = new BinaryReader(readStream);
            }
        }

        static Queue<DataBuffers> availableBuffers = new Queue<DataBuffers>();

        internal static void Initialize(uint nativeWriteBufferK = 2048, uint nativeReadBufferK = 2048)
        {
            lock (queueSyncObj)
            {
                for (int i = 0; i < 10; i++)
                {
                    DataBuffers newBuffer = new DataBuffers(nativeWriteBufferK * 1024, nativeReadBufferK * 1024);

                    availableBuffers.Enqueue(newBuffer);
                }
            }
        }
    }
}
