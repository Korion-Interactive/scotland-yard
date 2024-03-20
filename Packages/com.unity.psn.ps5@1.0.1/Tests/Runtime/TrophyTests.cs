#if UNITY_PS5
using NUnit.Framework;
using System.Collections;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Trophies;
using Unity.PSN.PS5.UDS;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Check all Trophy tests")]
    public class TrophyTests : BaseTests
    {
        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");

            return op.IsCompleted;
        }

        [UnityTest, Order(3), Description("Start the Trophy system")]
        public IEnumerator StartTrophySystem()
        {
            Request request;
            AsyncOp op = StartTrophySystemTest(out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(5), Description("Get game info")]
        public IEnumerator GetGameInfo()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetGameInfoTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(5), Description("Get group info")]
        public IEnumerator GetGroupInfo()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetGroupInfoTest(userId, -1, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(8), Description("Get Trophy Info")]
        public IEnumerator GetTrophyInfo()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetTrophyInfoTest(userId, 1, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(10), Description("Get Trophy Game Icon")]
        public IEnumerator GetTrophyGameIcon()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetTrophyGameIconTest(userId, 1, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(15), Description("Get Trophy Group Icon")]
        public IEnumerator GetTrophyGroupIcon()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetTrophyGroupIconTest(userId, -1, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(20), Description("Get Trophy Icon")]
        public IEnumerator GetTrophyIcon()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetTrophyIconTest(userId, 1, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(25), Description("Get Trophy Reward Icon")]
        public IEnumerator GetTrophyRewardIcon()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetTrophyRewardIconTest(userId, 9, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        public AsyncOp StartTrophySystemTest(out Request testRequest)
        {
            TrophySystem.StartSystemRequest request = new TrophySystem.StartSystemRequest();

            var requestOp = new AsyncRequest<TrophySystem.StartSystemRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TrophySystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetGameInfoTest(int userId, out Request testRequest)
        {
            TrophySystem.TrophyGameDetails gameDetails = new TrophySystem.TrophyGameDetails();
            TrophySystem.TrophyGameData gameData = new TrophySystem.TrophyGameData();

            TrophySystem.GetGameInfoRequest request = new TrophySystem.GetGameInfoRequest()
            {
                UserId = userId,
                GameDetails = gameDetails,
                GameData = gameData
            };

            var requestOp = new AsyncRequest<TrophySystem.GetGameInfoRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TrophySystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetGroupInfoTest(int userId, int groupId, out Request testRequest)
        {
            TrophySystem.GetGroupInfoRequest request = new TrophySystem.GetGroupInfoRequest();

            request.UserId = userId;
            request.GroupId = groupId;
            request.GroupDetails = new TrophySystem.TrophyGroupDetails();
            request.GroupData = new TrophySystem.TrophyGroupData();

            var requestOp = new AsyncRequest<TrophySystem.GetGroupInfoRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetTrophyInfoTest(int userId, int trophyId, out Request testRequest)
        {
            TrophySystem.GetTrophyInfoRequest request = new TrophySystem.GetTrophyInfoRequest();

            request.UserId = userId;
            request.TrophyId = trophyId;
            request.TrophyDetails = new TrophySystem.TrophyDetails();
            request.TrophyData = new TrophySystem.TrophyData();

            var getTrophyOp = new AsyncRequest<TrophySystem.GetTrophyInfoRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            testRequest = request;

            return getTrophyOp;
        }

        public AsyncOp GetTrophyGameIconTest(int userId, int trophyId, out Request testRequest)
        {
            TrophySystem.GetTrophyGameIconRequest request = new TrophySystem.GetTrophyGameIconRequest();

            request.UserId = userId;

            var getTrophyOp = new AsyncRequest<TrophySystem.GetTrophyGameIconRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            testRequest = request;

            return getTrophyOp;
        }

        public AsyncOp GetTrophyGroupIconTest(int userId, int groupId, out Request testRequest)
        {
            TrophySystem.GetTrophyGroupIconRequest request = new TrophySystem.GetTrophyGroupIconRequest();

            request.UserId = userId;
            request.GroupId = groupId;

            var getTrophyOp = new AsyncRequest<TrophySystem.GetTrophyGroupIconRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            testRequest = request;

            return getTrophyOp;
        }

        public AsyncOp GetTrophyIconTest(int userId, int trophyId, out Request testRequest)
        {
            TrophySystem.GetTrophyIconRequest request = new TrophySystem.GetTrophyIconRequest();

            request.UserId = userId;
            request.TrophyId = trophyId;

            var getTrophyOp = new AsyncRequest<TrophySystem.GetTrophyIconRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            testRequest = request;

            return getTrophyOp;
        }

        public AsyncOp GetTrophyRewardIconTest(int userId, int trophyId, out Request testRequest)
        {
            TrophySystem.GetTrophyRewardIconRequest request = new TrophySystem.GetTrophyRewardIconRequest();

            request.UserId = userId;
            request.TrophyId = trophyId;

            var getTrophyOp = new AsyncRequest<TrophySystem.GetTrophyRewardIconRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            testRequest = request;

            return getTrophyOp;
        }

        public AsyncOp ShowTrophyListTest(int userId, out Request testRequest)
        {
            TrophySystem.ShowTrophyListRequest request = new TrophySystem.ShowTrophyListRequest();

            request.UserId = userId;

            var getTrophyOp = new AsyncRequest<TrophySystem.ShowTrophyListRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UniversalDataSystem.Schedule(getTrophyOp);

            testRequest = request;

            return getTrophyOp;
        }

    }
}
#endif
