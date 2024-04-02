#if UNITY_PS5
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.UDS;
using Unity.PSN.PS5.Users;
using Unity.PSN.PS5.WebApi;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Check all UDS tests")]
    public class UDSTests : BaseTests
    {
        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");

            return op.IsCompleted;
        }

        [UnityTest, Order(3), Description("Start the USD system")]
        public IEnumerator StartUDS()
        {
            Request request;
            AsyncOp op = StartUDSTest(out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(5), Description("Unlock Trophy")]
        public IEnumerator UnlockTrophy()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = UnlockTrophyTest(userId, 1, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(8), Description("Post a complex event to the UDS")]
        public IEnumerator PostComplexEvent()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = PostComplexEventTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(12), Description("Post a binary event to the UDS")]
        public IEnumerator PostBinaryEvent()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = PostBinaryEventTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        public AsyncOp StartUDSTest(out Request testRequest)
        {
            var request = new UniversalDataSystem.StartSystemRequest();

            request.PoolSize = 256 * 1024;

            var requestOp = new AsyncRequest<UniversalDataSystem.StartSystemRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp StopUDSTest(out Request testRequest)
        {
            var request = new UniversalDataSystem.StopSystemRequest();

            var requestOp = new AsyncRequest<UniversalDataSystem.StopSystemRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp UnlockTrophyTest(int userId, int trophyId, out Request testRequest)
        {
            UniversalDataSystem.UDSEvent myEvent = new UniversalDataSystem.UDSEvent();

            myEvent.Create("_UnlockTrophy");

            UniversalDataSystem.EventProperty prop = new UniversalDataSystem.EventProperty("_trophy_id", (Int32)trophyId);

            myEvent.Properties.Set(prop);

            UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();

            request.UserId = userId;
            request.EventData = myEvent;

            var getTrophyOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            testRequest = request;

            return getTrophyOp;
        }

        public AsyncOp PostComplexEventTest(int userId, out Request testRequest)
        {
            UniversalDataSystem.UDSEvent myEvent = new UniversalDataSystem.UDSEvent();

            myEvent.Create("Test");

            myEvent.Properties.Set("name", "a test name");
            myEvent.Properties.Set("someid", (Int64)10);
            myEvent.Properties.Set("score", 1.234f);
            myEvent.Properties.Set("double", (double)1.23456789012345);
            myEvent.Properties.Set("bool", true);

            byte[] someData = new byte[100];

            for (int i = 0; i < someData.Length; i++)
            {
                someData[i] = (byte)i;
            }

            myEvent.Properties.Set("binary", someData);

            UniversalDataSystem.EventProperties subProperties = new UniversalDataSystem.EventProperties();
            myEvent.Properties.Set("nested", subProperties);

            subProperties.Set("moredata", 5.678f);
            subProperties.Set("anotherstring", "subprops");

            UniversalDataSystem.EventPropertyArray nestedArray = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Int32);

            subProperties.Set("nestedArray", nestedArray);
            nestedArray.CopyValues(new Int32[] { 10, 11, 12, 13, 14 });

            UniversalDataSystem.EventPropertyArray arrayProps = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.String);
            myEvent.Properties.Set("ArrayOfStuff", arrayProps);

            string[] testArray = new string[] { "One", "Two", "Three", "Four", "Five" };
            arrayProps.CopyValues(testArray);

            // Create an array of arrays
            UniversalDataSystem.EventPropertyArray arrayOfArraysProps = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Array);
            myEvent.Properties.Set("ArrayOfArrays", arrayOfArraysProps);

            // Create two array of different types to add
            UniversalDataSystem.EventPropertyArray array1 = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Int32);
            UniversalDataSystem.EventPropertyArray array2 = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Float);

            arrayOfArraysProps.Set(array1);
            arrayOfArraysProps.Set(array2);

            Int32[] testArray1 = new Int32[] { 1, 2, 3, 4, 5 };
            array1.CopyValues(testArray1);

            float[] testArray2 = new float[] { 1.12f, 2.23f, 3.45f, 4.56f, 5.67f };
            array2.CopyValues(testArray2);

            UniversalDataSystem.EventPropertyArray array3 = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Binary);

            arrayOfArraysProps.Set(array3);

            byte[] moreBinaryDataA = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
            byte[] moreBinaryDataB = new byte[] { 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119 };
            byte[] moreBinaryDataC = new byte[] { 200, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219 };

            List<byte[]> testBinaryArray3 = new List<byte[]>();
            testBinaryArray3.Add(moreBinaryDataA);
            testBinaryArray3.Add(moreBinaryDataB);
            testBinaryArray3.Add(moreBinaryDataC);

            array3.CopyValues(testBinaryArray3.ToArray());

            // Create an array of properties
            UniversalDataSystem.EventPropertyArray arrayOfProperties = new UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.Properties);
            myEvent.Properties.Set("ArrayOfProperties", arrayOfProperties);

            UniversalDataSystem.EventProperties subProperties1 = new UniversalDataSystem.EventProperties();
            UniversalDataSystem.EventProperties subProperties2 = new UniversalDataSystem.EventProperties();

            arrayOfProperties.Set(subProperties1);
            arrayOfProperties.Set(subProperties2);

            subProperties1.Set("moredata", 100.1234f);
            subProperties1.Set("anotherstring", "inside array 1");

            subProperties2.Set("moredata", 200.1234f);
            subProperties2.Set("anotherstring", "inside array 2");

            return PostEvent(userId, myEvent, out testRequest);
        }

        public AsyncOp PostBinaryEventTest(int userId, out Request testRequest)
        {
            UniversalDataSystem.UDSEvent myEvent = new UniversalDataSystem.UDSEvent();

            myEvent.Create("Test");

            myEvent.Properties.Set("name", "a test name");
            myEvent.Properties.Set("someid", (Int64)10);
            myEvent.Properties.Set("score", 1.234f);
            myEvent.Properties.Set("double", (double)1.23456789012345);
            myEvent.Properties.Set("bool", true);

            byte[] someData = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
            myEvent.Properties.Set("binary", someData);

            UniversalDataSystem.EventProperties subProperties = new UniversalDataSystem.EventProperties();
            myEvent.Properties.Set("nested", subProperties);

            subProperties.Set("morebinary", someData);

            return PostEvent(userId, myEvent, out testRequest);
        }

        private AsyncOp PostEvent(int userId, UniversalDataSystem.UDSEvent udsEvent, out Request testRequest)
        {
            UniversalDataSystem.PostEventRequest request = new UniversalDataSystem.PostEventRequest();

            request.UserId = userId;
            request.CalculateEstimatedSize = true;
            request.EventData = udsEvent;

            var requestOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(requestOp);

            UniversalDataSystem.EventDebugStringRequest stringRequest = new UniversalDataSystem.EventDebugStringRequest();

            stringRequest.UserId = userId;
            stringRequest.EventData = udsEvent;

            var secondRequestOp = new AsyncRequest<UniversalDataSystem.EventDebugStringRequest>(stringRequest).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(secondRequestOp);

            testRequest = request;

            return requestOp;
        }
    }
}
#endif
