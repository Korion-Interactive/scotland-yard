#if UNITY_PS5 || UNITY_PS4
using NUnit.Framework;
using System;
using System.Collections;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Leaderboard;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Check all Leaderboard tests")]
    public class LeaderboardTests : BaseTests
    {

        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");

            return op.IsCompleted;
        }

        [UnityTest, Order(3), Description("Get Board Definition")]
        public IEnumerator GetBoardDefinition()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetBoardDefinitionTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(5), Description("Record Score")]
        public IEnumerator RecordScore()
        {
            var userId = GetMainUserId();

            Request request;
            Int64 score = (Int64)UnityEngine.Random.Range(0, 1000);

            AsyncOp op = RecordScoreTest(userId, 1, score, true, MakeData(30, (byte)score), null, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(10), Description("Record Score with large data")]
        public IEnumerator RecordScoreWithLargeData()
        {
            var userId = GetMainUserId();

            Request request;
            Int64 score = (Int64)UnityEngine.Random.Range(0, 1000);

            AsyncOp op = RecordScoreTest(userId, 1, score, true, MakeData(30, (byte)score), MakeData(1024 * 20, (byte)(score + 1)), out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            bool checkForError = true;
            // Check for valid error code
            if (request.Result.apiResult == Unity.PSN.PS5.APIResultTypes.Error)
            {
                if((uint)request.Result.sceErrorCode == 0x8222F408) // SCE_NP_WEBAPI_SERVER_ERROR_LEADERBOARDS_LARGE_DATA_EXCEEDS_NUMBER_LIMIT
                {
                    checkForError = false;
                }
            }

            if (checkForError == true)
            {
                Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
            }
        }

        [UnityTest, Order(15), Description("Get ranking")]
        public IEnumerator GetRanking()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetRankingTest(userId, 1, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(25), Description("Get ranking")]
        public IEnumerator GetLargeData()
        {
            var userId = GetMainUserId();

            Request request;

            // First part is get do the ranking test for the board and then find an objectId from the ranking info that
            // can be used to download the large data.
            AsyncOp op = GetRankingTest(userId, 1, out request);
            yield return new WaitUntil(() => IsCompleted(op) == true);

            string bestObjectId = "";

            Leaderboards.GetRankingRequest rankingRequest = request as Leaderboards.GetRankingRequest;

            Assert.IsNotNull(rankingRequest, "rankingRequest shouldn't be null");

            Leaderboards.Rankings rankings = rankingRequest.Rankings;

            Assert.IsNotNull(rankings, "rankings shouldn't be null");

            int count = rankings.Entries != null ? rankings.Entries.Count : 0;

            for (int i = 0; i < count; i++)
            {
                var entry = rankings.Entries[i];
                if (entry.ObjectId != null)
                {
                    bestObjectId = entry.ObjectId;
                    break;
                }
            }

            Assert.IsNotEmpty(bestObjectId, "Can't find any large data uploaded to the board.");

            op = GetLargeDataTest(userId, 1, bestObjectId, out request);
            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        public AsyncOp GetBoardDefinitionTest(int userId, out Request testRequest)
        {
            Leaderboards.GetBoardDefinitionRequest request = new Leaderboards.GetBoardDefinitionRequest()
            {
                UserId = userId,
                BoardId = 1,
                ServiceLabel = 0,
            };

            var requestOp = new AsyncRequest<Leaderboards.GetBoardDefinitionRequest>(request).ContinueWith((antecedent) =>
            {
            });

            Leaderboards.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp RecordScoreTest(int userId, int boardId, Int64 score, bool needsTempRank, byte[] smallData, byte[] largeData, out Request testRequest)
        {
            Leaderboards.RecordScoreRequest request = new Leaderboards.RecordScoreRequest()
            {
                UserId = userId,
                BoardId = boardId,
                Score = score,
                NeedsTmpRank = needsTempRank,
                Comment = "Sample app score",
                SmallData = smallData,
                LargeData = largeData
            };

            var requestOp = new AsyncRequest<Leaderboards.RecordScoreRequest>(request).ContinueWith((antecedent) =>
            {
            });

            Leaderboards.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetRankingTest(int userId, int boardId, out Request testRequest)
        {
            Leaderboards.GetRankingRequest request = new Leaderboards.GetRankingRequest()
            {
                UserId = userId,
                BoardId = boardId,
            };

            var requestOp = new AsyncRequest<Leaderboards.GetRankingRequest>(request).ContinueWith((antecedent) =>
            {
            });

            Leaderboards.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetLargeDataTest(int userId, int boardId, string bestObjectId, out Request testRequest)
        {
            Leaderboards.GetLargeDataRequest request = new Leaderboards.GetLargeDataRequest()
            {
                UserId = userId,
                ObjectId = bestObjectId
            };

            var requestOp = new AsyncRequest<Leaderboards.GetLargeDataRequest>(request).ContinueWith((antecedent) =>
            {
            });

            Leaderboards.Schedule(requestOp);

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


    }
}
#endif
