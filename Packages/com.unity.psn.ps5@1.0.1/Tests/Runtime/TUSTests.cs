#if UNITY_PS5 || UNITY_PS4
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.TCS;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Check all Title User Storage tests")]
    public class TUSTests : BaseTests
    {
        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");

            return op.IsCompleted;
        }

        [UnityTest, Order(3), Description("Add And Get Variable Test")]
        public IEnumerator AddAndGetVariable()
        {
            var userId = GetMainUserId();
            var accountId = GetMainAccountId();

            Request request;
            AsyncOp op = AddAndGetVariableTest(userId, accountId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(6), Description("Set Variable With Condition Test")]
        public IEnumerator SetVariableWithCondition()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = SetVariableWithConditionTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(10), Description("Get Multiple Variables By Slot test")]
        public IEnumerator GetMultiVariablesBySlot()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetMultiVariablesBySlotTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(15), Description("Set Multiple Variables By User test")]
        public IEnumerator SetMultiVariablesByUser()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = SetMultiVariablesByUserTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(20), Description("Get Multiple Variables By User test")]
        public IEnumerator GetMultiVariablesByUser()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetMultiVariablesByUserTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(25), Description("Delete Multiple Variables By User test")]
        public IEnumerator DeleteMultiVariablesByUser()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = DeleteMultiVariablesByUserTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(30), Description("Upload Data test for current user")]
        public IEnumerator UploadData()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = UploadDataTest(userId, 0, true, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(35), Description("Upload Data test for anyone")]
        public IEnumerator UploadDataAnyone()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = UploadDataTest(userId, 0, false, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(40), Description("Get Multiple Data Statuses By Slot test")]
        public IEnumerator GetMultiDataStatusesBySlot()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetMultiDataStatusesBySlotTest(userId, 0, true, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(45), Description("Get Multiple Data Statuses By User test")]
        public IEnumerator GetMultiDataStatusesByUser()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetMultiDataStatusesByUserTest(userId, true, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(50), Description("Download data")]
        public IEnumerator DownloadData()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = DownloadDataTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(55), Description("Delete data")]
        public IEnumerator DeleteData()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = DeleteDataTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        

        public AsyncOp AddAndGetVariableTest(int userId, UInt64 accountId, out Request testRequest)
        {
            TitleCloudStorage.AddAndGetVariableRequest request = new TitleCloudStorage.AddAndGetVariableRequest()
            {
                UserId = userId,
                AccountId = accountId, //GamePad.activeGamePad.loggedInUser.accountId,
                SlotId = 1,
                Value = 10,
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.AddAndGetVariableRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp SetVariableWithConditionTest(int userId, out Request testRequest)
        {
            TitleCloudStorage.SetVariableWithConditionRequest request = new TitleCloudStorage.SetVariableWithConditionRequest()
            {
                UserId = userId,
                Me = true,
                SlotId = 1,
                Value = 100,
                Condition = TitleCloudStorage.SetVariableWithConditionRequest.Conditions.Greater
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.SetVariableWithConditionRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetMultiVariablesBySlotTest(int userId, out Request testRequest)
        {
            TitleCloudStorage.GetMultiVariablesBySlotRequest request = new TitleCloudStorage.GetMultiVariablesBySlotRequest()
            {
                UserId = userId,
                Me = true,
                Anyone = true,
                SlotId = 1,
                SortCondition = TitleCloudStorage.SortVariableConditions.SlotId,
                SortMode = TitleCloudStorage.SortModes.Ascending
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.GetMultiVariablesBySlotRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp SetMultiVariablesByUserTest(int userId, out Request testRequest)
        {
            List<TitleCloudStorage.VariableToWrite> variables = new List<TitleCloudStorage.VariableToWrite>();

            variables.Add(new TitleCloudStorage.VariableToWrite() { SlotId = 1, Value = 10 });
            variables.Add(new TitleCloudStorage.VariableToWrite() { SlotId = 2, Value = 20 });
            variables.Add(new TitleCloudStorage.VariableToWrite() { SlotId = 3, Value = 30 });

            TitleCloudStorage.SetMultiVariablesByUserRequest request = new TitleCloudStorage.SetMultiVariablesByUserRequest()
            {
                UserId = userId,
                Me = true,
                Variables = variables
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.SetMultiVariablesByUserRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetMultiVariablesByUserTest(int userId, out Request testRequest)
        {
            List<int> slotids = new List<int>();

            slotids.Add(1);
            slotids.Add(2);
            slotids.Add(3);

            TitleCloudStorage.GetMultiVariablesByUserRequest request = new TitleCloudStorage.GetMultiVariablesByUserRequest()
            {
                UserId = userId,
                Me = true,
                SlotIds = slotids,
                SortCondition = TitleCloudStorage.SortVariableConditions.Date,
                SortMode = TitleCloudStorage.SortModes.Descending
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.GetMultiVariablesByUserRequest>(request).ContinueWith((antecedent) =>
            {
            });


            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp DeleteMultiVariablesByUserTest(int userId, out Request testRequest)
        {
            List<int> slotids = new List<int>();

            slotids.Add(1);
            slotids.Add(2);
            slotids.Add(3);

            TitleCloudStorage.DeleteMultiVariablesByUserRequest request = new TitleCloudStorage.DeleteMultiVariablesByUserRequest()
            {
                UserId = userId,
                Me = true,
                SlotIds = slotids
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.DeleteMultiVariablesByUserRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp UploadDataTest(int userId, int slotId, bool me, out Request testRequest)
        {
            byte[] data = MakeData(1024 * 1024 * 4, slotId % 256); // 4mb data
            byte[] info = MakeData(256, (slotId + 1) % 256);

            bool anyone = !me;

            TitleCloudStorage.UploadDataRequest request = new TitleCloudStorage.UploadDataRequest()
            {
                UserId = userId,
                Me = me,
                Anyone = anyone,
                SlotId = slotId,
                Data = data,
                Info = info
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.UploadDataRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        string dataStatusObjectIdSlotMe = "";
        string dataStatusObjectIdSlotAnyone = "";

        public AsyncOp GetMultiDataStatusesBySlotTest(int userId, int slotId, bool me, out Request testRequest)
        {
            bool anyone = !me;

            TitleCloudStorage.GetMultiDataStatusesBySlotRequest request = new TitleCloudStorage.GetMultiDataStatusesBySlotRequest()
            {
                UserId = userId,
                Me = me,
                Anyone = anyone,
                SlotId = slotId,
                SortCondition = TitleCloudStorage.SortDataConditions.DataSize,
                SortMode = TitleCloudStorage.SortModes.Ascending
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.GetMultiDataStatusesBySlotRequest>(request).ContinueWith((antecedent) =>
            {
                if (antecedent.Request.Result.apiResult == Unity.PSN.PS5.APIResultTypes.Success)
                {
                    if (me == true)
                    {
                        dataStatusObjectIdSlotMe = GetFirstObjectID(antecedent.Request.Statuses, 0);
                    }
                    else
                    {
                        dataStatusObjectIdSlotAnyone = GetFirstObjectID(antecedent.Request.Statuses, 0);
                    }
                }
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        string dataStatusObjectIdUserMe = "";
        string dataStatusObjectIdUserAnyone = "";

        public AsyncOp GetMultiDataStatusesByUserTest(int userId, bool me, out Request testRequest)
        {
            List<int> slotids = new List<int>();

            slotids.Add(0);
            slotids.Add(1);

            bool anyone = !me;

            TitleCloudStorage.GetMultiDataStatusesByUserRequest request = new TitleCloudStorage.GetMultiDataStatusesByUserRequest()
            {
                UserId = userId,
                Me = me,
                Anyone = anyone,
                SlotIds = slotids,
                SortCondition = TitleCloudStorage.SortDataConditions.DataSize,
                SortMode = TitleCloudStorage.SortModes.Ascending
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.GetMultiDataStatusesByUserRequest>(request).ContinueWith((antecedent) =>
            {
                if (antecedent.Request.Result.apiResult == Unity.PSN.PS5.APIResultTypes.Success)
                {
                    if (me == true)
                    {
                        dataStatusObjectIdUserMe = GetFirstObjectID(antecedent.Request.Statuses, 0);
                    }
                    else
                    {
                        dataStatusObjectIdUserAnyone = GetFirstObjectID(antecedent.Request.Statuses, 0);
                    }
                }
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp DownloadDataTest(int userId, out Request testRequest)
        {
            Debug.Log("dataStatusObjectIdSlotMe = " + dataStatusObjectIdSlotMe);

            TitleCloudStorage.DownloadDataRequest request = new TitleCloudStorage.DownloadDataRequest()
            {
                UserId = userId,
                Me = true,
                SlotId = 0,
                ObjectId = dataStatusObjectIdSlotMe
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.DownloadDataRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp DeleteDataTest(int userId, out Request testRequest)
        {
            TitleCloudStorage.DeleteMultiDataBySlotRequest request = new TitleCloudStorage.DeleteMultiDataBySlotRequest()
            {
                UserId = userId,
                Me = true,
                SlotId = 0,
            };

            var requestOp = new AsyncRequest<TitleCloudStorage.DeleteMultiDataBySlotRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public byte[] MakeData(int size, int startNumber)
        {
            byte[] someData = new byte[size];

            for (int i = 0; i < someData.Length; i++)
            {
                someData[i] = (byte)(startNumber + i);
            }

            return someData;
        }

        string GetFirstObjectID(TitleCloudStorage.DataStatusCollection statuses, int slotId)
        {
            if (statuses == null) return null;

            int count = statuses.Statuses != null ? statuses.Statuses.Count : 0;

            if (count == 0) return null;

            for (int i = 0; i < count; i++)
            {
                if ((statuses.Statuses[i].ValidFields & TitleCloudStorage.DataStatus.Filters.ObjectId) != 0 &&
                     (statuses.Statuses[i].ValidFields & TitleCloudStorage.DataStatus.Filters.SlotId) != 0 &&
                     statuses.Statuses[i].SlotId == slotId)
                {
                    return statuses.Statuses[i].ObjectId;
                }
            }

            return null;
        }

    }
}
#endif
