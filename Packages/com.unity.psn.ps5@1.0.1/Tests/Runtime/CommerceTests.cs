#if UNITY_PS5
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Commerce;
using Unity.PSN.PS5.TCS;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Check all Commerce tests")]
    public class CommerceTests : BaseTests
    {
        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");

            return op.IsCompleted;
        }

        [UnityTest, Order(3), Description("Open and Close Join Premium Dialog Test")]
        public IEnumerator OpenJoinPremiumDialog()
        {
            var userId = GetMainUserId();

            CommerceDialogSystem.OpenJoinPremiumDialogRequest request;
            AsyncOp op = OpenJoinPremiumDialogTest(userId, out request);

            yield return new WaitForSeconds(5.0f);

            request.CloseDialog();

            while(IsCompleted(op) != true)
            {
                Unity.PSN.PS5.Main.Update();
            }

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(5), Description("Open and Close Browse Category Dialog Test")]
        public IEnumerator OpenBrowseCategoryDialog()
        {
            var userId = GetMainUserId();

            CommerceDialogSystem.OpenBrowseCategoryDialogRequest request;
            AsyncOp op = OpenBrowseCategoryDialogTest(userId, out request);

            yield return new WaitForSeconds(5.0f);

            request.CloseDialog();

            while (IsCompleted(op) != true)
            {
                Unity.PSN.PS5.Main.Update();
            }

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(15), Description("Open and Close Browse Product Dialog Test")]
        public IEnumerator OpenBrowseProductDialog()
        {
            var userId = GetMainUserId();

            CommerceDialogSystem.OpenBrowseProductDialogRequest request;
            AsyncOp op = OpenBrowseProductDialogTest(userId, out request);

            yield return new WaitForSeconds(5.0f);

            request.CloseDialog();

            while (IsCompleted(op) != true)
            {
                Unity.PSN.PS5.Main.Update();
            }

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(20), Description("Open and Close Redeem Promotion Code Dialog Test")]
        public IEnumerator OpenRedeemPromotionCodeDialog()
        {
            var userId = GetMainUserId();

            CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest request;
            AsyncOp op = OpenRedeemPromotionCodeDialogTest(userId, out request);

            yield return new WaitForSeconds(5.0f);

            request.CloseDialog();

            while (IsCompleted(op) != true)
            {
                Unity.PSN.PS5.Main.Update();
            }

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(30), Description("Open and Close Checkout Dialog Test")]
        public IEnumerator OpenCheckoutDialog()
        {
            var userId = GetMainUserId();

            CommerceDialogSystem.OpenCheckoutDialogRequest request;
            AsyncOp op = OpenCheckoutDialogTest(userId, out request);

            yield return new WaitForSeconds(5.0f);

            request.CloseDialog();

            while (IsCompleted(op) != true)
            {
                Unity.PSN.PS5.Main.Update();
            }

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(40), Description("Open and Close Download Dialog Test")]
        public IEnumerator OpenDownloadDialog()
        {
            var userId = GetMainUserId();

            CommerceDialogSystem.OpenDownloadDialogRequest request;
            AsyncOp op = OpenDownloadDialogTest(userId, out request);

            yield return new WaitForSeconds(5.0f);

            request.CloseDialog();

            while (IsCompleted(op) != true)
            {
                Unity.PSN.PS5.Main.Update();
            }

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        public AsyncOp OpenJoinPremiumDialogTest(int userId, out CommerceDialogSystem.OpenJoinPremiumDialogRequest testRequest)
        {
            CommerceDialogSystem.OpenJoinPremiumDialogRequest request = new CommerceDialogSystem.OpenJoinPremiumDialogRequest()
            {
                UserId = userId,
            };

            var requestOp = new AsyncRequest<CommerceDialogSystem.OpenJoinPremiumDialogRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp OpenBrowseCategoryDialogTest(int userId, out CommerceDialogSystem.OpenBrowseCategoryDialogRequest testRequest)
        {
            List<string> categories = new List<string>();
            categories.Add("TESTENTITLEMENT");

            CommerceDialogSystem.OpenBrowseCategoryDialogRequest request = new CommerceDialogSystem.OpenBrowseCategoryDialogRequest()
            {
                UserId = userId,
                Targets = categories.ToArray()
            };

            var requestOp = new AsyncRequest<CommerceDialogSystem.OpenBrowseCategoryDialogRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp OpenBrowseProductDialogTest(int userId, out CommerceDialogSystem.OpenBrowseProductDialogRequest testRequest)
        {
            List<string> targets = new List<string>();
            targets.Add("4068583663090088");

            CommerceDialogSystem.OpenBrowseProductDialogRequest request = new CommerceDialogSystem.OpenBrowseProductDialogRequest()
            {
                UserId = userId,
                Targets = targets.ToArray()
            };

            var requestOp = new AsyncRequest<CommerceDialogSystem.OpenBrowseProductDialogRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp OpenRedeemPromotionCodeDialogTest(int userId, out CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest testRequest)
        {
            CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest request = new CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest()
            {
                UserId = userId,
            };

            var requestOp = new AsyncRequest<CommerceDialogSystem.OpenRedeemPromotionCodeDialogRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp OpenCheckoutDialogTest(int userId, out CommerceDialogSystem.OpenCheckoutDialogRequest testRequest)
        {
            List<string> targets = new List<string>();
            targets.Add("4068583663090088");
            targets.Add("EXTRALIVESENTITL");

            CommerceDialogSystem.OpenCheckoutDialogRequest request = new CommerceDialogSystem.OpenCheckoutDialogRequest()
            {
                UserId = userId,
                Targets = targets.ToArray()
            };

            var requestOp = new AsyncRequest<CommerceDialogSystem.OpenCheckoutDialogRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp OpenDownloadDialogTest(int userId, out CommerceDialogSystem.OpenDownloadDialogRequest testRequest)
        {
            CommerceDialogSystem.OpenDownloadDialogRequest request = new CommerceDialogSystem.OpenDownloadDialogRequest()
            {
                UserId = userId,
            };

            var requestOp = new AsyncRequest<CommerceDialogSystem.OpenDownloadDialogRequest>(request).ContinueWith((antecedent) =>
            {
            });

            TitleCloudStorage.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }
    }
}
#endif
