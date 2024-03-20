#if UNITY_PS5 || UNITY_PS4
using NUnit.Framework;
using System.Collections;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Checks;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;

[assembly: Preserve]

namespace PSNTests
{
    [TestFixture, Description("Check all Online Safety tests")]
    public class OnlineSafetyTests : BaseTests
    {
        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");

            return op.IsCompleted;
        }

        [UnityTest, Order(3), Description("Communication Restriction Status test")]
        public IEnumerator CRStatus()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = CRStatusTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(5), Description("Filter Profanity test")]
        public IEnumerator FilterProfanity()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = FilterProfanityTest(userId, OnlineSafety.ProfanityFilterType.ReplaceProfanity, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        public AsyncOp CRStatusTest(int userId, out Request testRequest)
        {
            OnlineSafety.GetCommunicationRestrictionStatusRequest request = new OnlineSafety.GetCommunicationRestrictionStatusRequest()
            {
                UserId = userId
            };

            var requestOp = new AsyncRequest<OnlineSafety.GetCommunicationRestrictionStatusRequest>(request).ContinueWith((antecedent) =>
            {
            });

            OnlineSafety.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp FilterProfanityTest(int userId, OnlineSafety.ProfanityFilterType filterType, out Request testRequest)
        {
            OnlineSafety.FilterProfanityRequest request = new OnlineSafety.FilterProfanityRequest()
            {
                UserId = userId,
                Locale = "en-US",
                TextToFilter = "This is a test of a shit string",
                FilterType = filterType
            };

            var requestOp = new AsyncRequest<OnlineSafety.FilterProfanityRequest>(request).ContinueWith((antecedent) =>
            {
            });

            OnlineSafety.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

    }
}
#endif
