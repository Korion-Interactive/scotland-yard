#if UNITY_PS5
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PSN.PS5.Aysnc;
using Unity.PSN.PS5.Users;
using Unity.PSN.PS5.WebApi;
using UnityEngine;
using UnityEngine.TestTools;

namespace PSNTests
{
    [TestFixture, Description("Check all user tests")]
    public class UserTests : BaseTests
    {
        private void OnReachabilityNotification(UserSystem.ReachabilityEvent reachabilityEvent)
        {
        }

        private void OnSignedInNotification(UserSystem.SignedInEvent signedInEvent)
        {
        }

        public void NotificationEventHandler(WebApiNotifications.CallbackParams eventData)
        {

        }

        WebApiFilters globalUserFilters;

        public bool IsCompleted(AsyncOp op)
        {
            Assert.IsNotNull(op, "AsyncOp is Null");

            return op.IsCompleted;
        }

        [UnityTest, Order(3), Description("Register WebAPI user pushevents")]
        public IEnumerator RegisterPushEvents()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = RegisterPushEventsTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(4), Description("Unregister WebAPI user pushevents")]
        public IEnumerator UnregisterPushEvents()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = UnregisterPushEventsTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(10), Description("Get Friends")]
        public IEnumerator GetFriends()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetFriendsTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(20), Description("Get Users Profile")]
        public IEnumerator GetUsersProfile()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetUsersProfileTest(userId, null, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(30), Description("Get Friends Profile")]
        public IEnumerator GetFriendsProfile()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetFriendsTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            op = GetUsersProfileTest(userId, RetrievedAccountIds, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(40), Description("Get Friends Profile")]
        public IEnumerator GetFriendsPresence()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetFriendsTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");

            if (RetrievedAccountIds != null && RetrievedAccountIds.Count > 0)
            {
                op = GetFriendsPresenceTest(userId, RetrievedAccountIds, out request);

                yield return new WaitUntil(() => IsCompleted(op) == true);

                Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
            }
            else
            {
                Debug.LogWarning("Unable to fully run Presence test as no friends have been retrieved.");
            }
        }

        [UnityTest, Order(50), Description("Get presenses of the current users friends")]
        public IEnumerator GetBlockedUsers()
        {
            var userId = GetMainUserId();

            Request request;
            AsyncOp op = GetBlockedUsersTest(userId, out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(60), Description("Enable notifications for Signin/out event")]
        public IEnumerator EnableSignInNotification()
        {
            Request request;
            AsyncOp op = EnableSignInNotificationTest(out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(70), Description("Disable notifications for Signin/out event")]
        public IEnumerator DisableSignInNotification()
        {
            Request request;
            AsyncOp op = DisableSignInNotificationTest(out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(90), Description("Enable notifications for PSN reachability events")]
        public IEnumerator EnableReachabilityNotification()
        {
            Request request;
            AsyncOp op = EnableReachabilityNotificationTest(out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        [UnityTest, Order(100), Description("Disable notifications for PSN reachability events")]
        public IEnumerator DisableReachabilityNotification()
        {
            Request request;
            AsyncOp op = DisableReachabilityNotificationTest(out request);

            yield return new WaitUntil(() => IsCompleted(op) == true);

            Assert.IsTrue(TestFramework.CheckRequestOK(request), "Request is OK");
        }

        //public AsyncOp AddUserTest(int userId, out Request testRequest)
        //{
        //    var internalRequest = new UserSystem.AddUserRequest() { UserId = userId };

        //    var requestOp = new AsyncRequest<UserSystem.AddUserRequest>(internalRequest).ContinueWith((antecedent) =>
        //    {
        //        if (antecedent != null && antecedent.Request != null)
        //        {
        //            // Test is successful
        //        }
        //    });

        //    UserSystem.Schedule(requestOp);

        //    testRequest = internalRequest;

        //    return requestOp;
        //}

        Dictionary<Int32, WebApiPushEvent> userPushEvents = new Dictionary<int, WebApiPushEvent>();

        public AsyncOp RegisterPushEventsTest(int userId, out Request testRequest)
        {
            WebApiPushEvent pushEvent = new WebApiPushEvent();

            if (globalUserFilters == null)
            {
                globalUserFilters = new WebApiFilters();

                WebApiFilter friendsFilter = globalUserFilters.AddFilterParam("np:service:friendlist:friend");
                friendsFilter.ExtendedKeys = new List<string>() { "additionalTrigger" };

                globalUserFilters.AddFilterParams(new string[] { "np:service:presence2:onlineStatus", "np:service:blocklist" });
            }

            pushEvent.Filters = globalUserFilters;
            pushEvent.UserId = userId;
            pushEvent.OrderGuaranteed = false;

            var request = new WebApiNotifications.RegisterPushEventRequest()
            {
                PushEvent = pushEvent,
                Callback = NotificationEventHandler
            };

            var requestOp = new AsyncRequest<WebApiNotifications.RegisterPushEventRequest>(request).ContinueWith((antecedent) =>
            {
                userPushEvents.Add(antecedent.Request.PushEvent.UserId, antecedent.Request.PushEvent);
            });

            WebApiNotifications.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp UnregisterPushEventsTest(int userId, out Request testRequest)
        {
            WebApiPushEvent pushEvent;

            if (userPushEvents.TryGetValue(userId, out pushEvent) == true)
            {
                userPushEvents.Remove(userId);

                WebApiNotifications.UnregisterPushEventRequest request = new WebApiNotifications.UnregisterPushEventRequest()
                {
                    PushEvent = pushEvent,
                };

                var requestOp = new AsyncRequest<WebApiNotifications.UnregisterPushEventRequest>(request).ContinueWith((antecedent) =>
                {
                });

                WebApiNotifications.Schedule(requestOp);

                testRequest = request;

                return requestOp;
            }
            else
            {
                Assert.IsTrue(false, "UnregisterPushEventsTest failed because it can't find the users WebApiPushEvent object");
                testRequest = null;
                return null;
            }
        }

        List<UInt64> RetrievedAccountIds;

        public AsyncOp GetFriendsTest(int userId, out Request testRequest)
        {
            RetrievedAccountIds = null;

            UInt32 limit = 95;

            var request = new UserSystem.GetFriendsRequest()
            {
                UserId = userId,
                Offset = 0,
                Limit = limit,
                Filter = UserSystem.GetFriendsRequest.Filters.NotSet,
                SortOrder = UserSystem.GetFriendsRequest.Order.OnlineId,
            };

            var requestOp = new AsyncRequest<UserSystem.GetFriendsRequest>(request).ContinueWith((antecedent) =>
            {
                RetrievedAccountIds = antecedent.Request.RetrievedAccountIds;
            });

            UserSystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetUsersProfileTest(int userId, List<ulong> accountids, out Request testRequest)
        {
            UserSystem.GetProfilesRequest request = new UserSystem.GetProfilesRequest()
            {
                UserId = userId,
                AccountIds = accountids, //globalAccountIds,
                RetrievedProfiles = new List<UserSystem.UserProfile>()
            };

            var requestOp = new AsyncRequest<UserSystem.GetProfilesRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UserSystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp GetFriendsPresenceTest(int userId, List<ulong> accountids, out Request testRequest)
        {
            UserSystem.GetBasicPresencesRequest presenceRequest = new UserSystem.GetBasicPresencesRequest()
            {
                UserId = userId,
                AccountIds = accountids,
                RetrievedPresences = new List<UserSystem.BasicPresence>()
            };

            var requestOp = new AsyncRequest<UserSystem.GetBasicPresencesRequest>(presenceRequest).ContinueWith((antecedent) =>
            {
            });

            UserSystem.Schedule(requestOp);

            testRequest = presenceRequest;

            return requestOp;
        }

        public AsyncOp GetBlockedUsersTest(int userId, out Request testRequest)
        {
            UInt32 limit = 95;

            UserSystem.GetBlockingUsersRequest request = new UserSystem.GetBlockingUsersRequest()
            {
                UserId = userId,
                Offset = 0,
                Limit = limit,
                RetrievedAccountIds = new System.Collections.Generic.List<UInt64>((int)limit)
            };

            var requestOp = new AsyncRequest<UserSystem.GetBlockingUsersRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UserSystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp EnableSignInNotificationTest(out Request testRequest)
        {
            UserSystem.OnSignedInNotification += OnSignedInNotification;

            UserSystem.StartSignedStateCallbackRequest request = new UserSystem.StartSignedStateCallbackRequest();

            var requestOp = new AsyncRequest<UserSystem.StartSignedStateCallbackRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UserSystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp DisableSignInNotificationTest(out Request testRequest)
        {
            UserSystem.StopSignedStateCallbackRequest request = new UserSystem.StopSignedStateCallbackRequest();

            var requestOp = new AsyncRequest<UserSystem.StopSignedStateCallbackRequest>(request).ContinueWith((antecedent) =>
            {
                UserSystem.OnSignedInNotification -= OnSignedInNotification;
            });

            UserSystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp EnableReachabilityNotificationTest(out Request testRequest)
        {
            UserSystem.OnReachabilityNotification += OnReachabilityNotification;

            UserSystem.StartReachabilityStateCallbackRequest request = new UserSystem.StartReachabilityStateCallbackRequest();

            var requestOp = new AsyncRequest<UserSystem.StartReachabilityStateCallbackRequest>(request).ContinueWith((antecedent) =>
            {
            });

            UserSystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }

        public AsyncOp DisableReachabilityNotificationTest(out Request testRequest)
        {
            UserSystem.StopReachabilityStateCallbackRequest request = new UserSystem.StopReachabilityStateCallbackRequest();

            var requestOp = new AsyncRequest<UserSystem.StopReachabilityStateCallbackRequest>(request).ContinueWith((antecedent) =>
            {
                UserSystem.OnReachabilityNotification -= OnReachabilityNotification;
            });

            UserSystem.Schedule(requestOp);

            testRequest = request;

            return requestOp;
        }
    }
}
#endif



