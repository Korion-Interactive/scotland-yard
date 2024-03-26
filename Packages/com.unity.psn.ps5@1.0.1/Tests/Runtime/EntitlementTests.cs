#if UNITY_PS5
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Entitlement;
using Unity.PSN.PS5.TCS;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Check all Entitlement tests")]
    public class EntitlementTests : BaseTests
    {
        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");

            return op.IsCompleted;
        }

        [UnityTest, Order(1), Description("Get Sku Test")]
        public IEnumerator GetSku()
        {
            Entitlements.GetSkuFlagRequest request;
            AsyncOp op = GetSkuTest(out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(3), Description("Get Additional Content Entitlement List Test")]
        public IEnumerator GetAdditionalContentEntitlementList()
        {
            Entitlements.GetAdditionalContentEntitlementListRequest request;
            AsyncOp op = GetAdditionalContentEntitlementListTest(out request);

            while (IsCompleted(op) != true)
            {
                Unity.PSN.PS5.Main.Update();
            }

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(10), Description("Get Unified Entitlement List Test")]
        public IEnumerator GetUnifiedEntitlementInfoList()
        {
            var userId = GetMainUserId();

            Entitlements.GetUnifiedEntitlementInfoListRequest request;
            AsyncOp op = GetUnifiedEntitlementInfoListTest(userId, out request);

            while (IsCompleted(op) != true)
            {
                Unity.PSN.PS5.Main.Update();
            }

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(20), Description("Get Unified Entitlement Test")]
        public IEnumerator GetUnifiedEntitlementInfo()
        {
            var userId = GetMainUserId();

            Entitlements.GetUnifiedEntitlementInfoListRequest request;
            AsyncOp op = GetUnifiedEntitlementInfoListTest(userId, out request);

            while (IsCompleted(op) != true)
            {
                Unity.PSN.PS5.Main.Update();
            }

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            if (request.Entitlements != null && request.Entitlements.Length > 0)
            {
                Entitlements.GetUnifiedEntitlementInfoRequest infoRequest;
                op = GetUnifiedEntitlementInfoTest(userId, request.Entitlements[0].EntitlementLabel, out infoRequest);

                while (IsCompleted(op) != true)
                {
                    Unity.PSN.PS5.Main.Update();
                }

                yield return new WaitUntil(() => IsCompleted(op) == true);

                Assert.IsTrue(TestFramework.CheckRequestOK(infoRequest), "Request is OK");
            }
        }

        [UnityTest, Order(30), Description("Consume Unified Entitlement Test")]
        public IEnumerator ConsumeUnifiedEntitlement()
        {
            var userId = GetMainUserId();

            Entitlements.GetUnifiedEntitlementInfoListRequest request;
            AsyncOp op = GetUnifiedEntitlementInfoListTest(userId, out request);

            while (IsCompleted(op) != true)
            {
                Unity.PSN.PS5.Main.Update();
            }

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            if (request.Entitlements != null && request.Entitlements.Length > 0 && request.Entitlements[0].UseLimit > 0)
            {
                Entitlements.ConsumeUnifiedEntitlementRequest consumeRequest;
                op = ConsumeUnifiedEntitlementInfoTest(userId, request.Entitlements[0].EntitlementLabel, out consumeRequest);

                while (IsCompleted(op) != true)
                {
                    Unity.PSN.PS5.Main.Update();
                }

                yield return new WaitUntil(() => IsCompleted(op) == true);

                Assert.IsTrue(TestFramework.CheckRequestOK(consumeRequest), "Request is OK");
            }
        }

        public AsyncOp GetSkuTest(out Entitlements.GetSkuFlagRequest testRequest)
        {
            Entitlements.GetSkuFlagRequest request = new Entitlements.GetSkuFlagRequest()
            {
            };

            var requestOp = new AsyncRequest<Entitlements.GetSkuFlagRequest>(request).ContinueWith((antecedent) =>
            {
            });

            Entitlements.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetAdditionalContentEntitlementListTest(out Entitlements.GetAdditionalContentEntitlementListRequest testRequest)
        {
            Entitlements.GetAdditionalContentEntitlementListRequest request = new Entitlements.GetAdditionalContentEntitlementListRequest()
            {
                ServiceLabel = 0
            };

            var requestOp = new AsyncRequest<Entitlements.GetAdditionalContentEntitlementListRequest>(request).ContinueWith((antecedent) =>
            {
            });

            Entitlements.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetUnifiedEntitlementInfoListTest(int userId, out Entitlements.GetUnifiedEntitlementInfoListRequest testRequest)
        {
            Entitlements.GetUnifiedEntitlementInfoListRequest request = new Entitlements.GetUnifiedEntitlementInfoListRequest()
            {
                UserId = userId,
                ServiceLabel = 0,
                Sort = Entitlements.SortTypes.ActiveData,
                SortDirection = Entitlements.SortOrders.Ascending,
                PackageType = Entitlements.EntitlementAccessPackageType.PSCONS
            };

            var requestOp = new AsyncRequest<Entitlements.GetUnifiedEntitlementInfoListRequest>(request).ContinueWith((antecedent) =>
            {
            });

            Entitlements.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetUnifiedEntitlementInfoTest(int userId, string entitlementLabel, out Entitlements.GetUnifiedEntitlementInfoRequest testRequest)
        {
            Entitlements.GetUnifiedEntitlementInfoRequest request = new Entitlements.GetUnifiedEntitlementInfoRequest()
            {
                UserId = userId,
                ServiceLabel = 0,
                EntitlementLabel = entitlementLabel
            };

            var requestOp = new AsyncRequest<Entitlements.GetUnifiedEntitlementInfoRequest>(request).ContinueWith((antecedent) =>
            {
            });

            Entitlements.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp ConsumeUnifiedEntitlementInfoTest(int userId, string entitlementLabel, out Entitlements.ConsumeUnifiedEntitlementRequest testRequest)
        {
            Entitlements.ConsumeUnifiedEntitlementRequest request = new Entitlements.ConsumeUnifiedEntitlementRequest()
            {
                UserId = userId,
                ServiceLabel = 0,
                UseCount = 1,
                EntitlementLabel = entitlementLabel
            };

            var requestOp = new AsyncRequest<Entitlements.ConsumeUnifiedEntitlementRequest>(request).ContinueWith((antecedent) =>
            {
            });

            Entitlements.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }
    }
}
#endif
