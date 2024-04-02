#if UNITY_PS5
using NUnit.Framework;
using System.Collections;
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Check all Game Update tests")]
    public class GameUpdateTests : BaseTests
    {
        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");

            return op.IsCompleted;
        }

        [UnityTest, Order(3), Description("Check Game Update")]
        public IEnumerator CheckGameUpdate()
        {
            GameUpdate.GameUpdateRequest request = new GameUpdate.GameUpdateRequest()
            {
            };

            var requestOp = new AsyncRequest<GameUpdate.GameUpdateRequest>(request).ContinueWith((antecedent) =>
            {
            });

            GameUpdate.Schedule(requestOp);

            yield return new WaitUntil(() => IsCompleted(requestOp) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(6), Description("Coeck AddCont Update")]
        public IEnumerator CheckAddContUpdate()
        {
            GameUpdate.AddcontLatestVersionRequest request = new GameUpdate.AddcontLatestVersionRequest()
            {
                ServiceLabel=0,
                EntitlementLabel="test-entitlement"
            };

            var requestOp = new AsyncRequest<GameUpdate.AddcontLatestVersionRequest>(request).ContinueWith((antecedent) =>
            {
            });

            GameUpdate.Schedule(requestOp);

            yield return new WaitUntil(() => IsCompleted(requestOp) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }



    }
}
#endif
