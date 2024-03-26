#if UNITY_PS5
using NUnit.Framework;
using System.Collections;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.PremiumFeatures;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Check all Feature Gating tests")]
    public class FeatureGatingTests : BaseTests
    {

        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");

            return op.IsCompleted;
        }

        [UnityTest, Order(3), Description("Check Premium Test")]
        public IEnumerator CheckPremium()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = CheckPremiumTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(6), Description("Notify Premium Feature Test")]
        public IEnumerator NotifyPremiumFeature()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = NotifyPremiumFeatureTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }


        [UnityTest, Order(10), Description("Start Premium Event Test")]
        public IEnumerator StartPremiumEvent()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = StartPremiumEventTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }


        [UnityTest, Order(20), Description("Stop Premium Event Test")]
        public IEnumerator StopPremiumEvent()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = StopPremiumEventTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }


        public AsyncOp CheckPremiumTest(int userId, out Request testRequest)
        {
            FeatureGating.CheckPremiumRequest request = new FeatureGating.CheckPremiumRequest()
            {
                UserId = userId
            };

            var requestOp = new AsyncRequest<FeatureGating.CheckPremiumRequest>(request).ContinueWith((antecedent) =>
            {
            });

            FeatureGating.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp NotifyPremiumFeatureTest(int userId, out Request testRequest)
        {
            FeatureGating.NotifyPremiumFeatureRequest request = new FeatureGating.NotifyPremiumFeatureRequest()
            {
                UserId = userId,
                Properties = FeatureGating.MultiplayProperties.InEngineSpectating
            };

            var requestOp = new AsyncRequest<FeatureGating.NotifyPremiumFeatureRequest>(request).ContinueWith((antecedent) =>
            {
            });

            FeatureGating.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp StartPremiumEventTest(int userId, out Request testRequest)
        {
            FeatureGating.StartPremiumEventCallbackRequest request = new FeatureGating.StartPremiumEventCallbackRequest();

            var requestOp = new AsyncRequest<FeatureGating.StartPremiumEventCallbackRequest>(request).ContinueWith((antecedent) =>
            {
            });

            FeatureGating.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp StopPremiumEventTest(int userId, out Request testRequest)
        {
            FeatureGating.StopPremiumEventCallbackRequest request = new FeatureGating.StopPremiumEventCallbackRequest();

            var requestOp = new AsyncRequest<FeatureGating.StopPremiumEventCallbackRequest>(request).ContinueWith((antecedent) =>
            {
            });

            FeatureGating.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }
    }
}
#endif
