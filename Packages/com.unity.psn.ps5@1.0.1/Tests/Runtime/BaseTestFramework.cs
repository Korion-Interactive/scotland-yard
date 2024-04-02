#if UNITY_PS5 || UNITY_PS4
using NUnit.Framework;
using UnityEngine;
using Unity.PSN.PS5;
using Unity.PSN.PS5.Initialization;
using Unity.PSN.PS5.Users;
using Unity.PSN.PS5.Aysnc;
using System.Collections.Generic;

#if UNITY_PS4
using PlatformInput = UnityEngine.PS4.PS4Input;
#elif UNITY_PS5
using PlatformInput = UnityEngine.PS5.PS5Input;
#endif

namespace PSNTests
{

    public class BaseTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            float startTime = Time.realtimeSinceStartup;

            Debug.Log("OneTimeSetUp: Initializing PSN package.");
            while (TestFramework.RunInitialize() == false)
            {
                // Give up if initialization is taking too long.
                if(Time.realtimeSinceStartup - startTime > 20.0f) // 20 seconds
                {
                    break;
                }
            }

            Assert.AreEqual(TestFramework.IsInitialized(), true);

            TestFramework.TestState(TestFramework.InitState.Ready);
        }

        public int GetMainUserId()
        {
            var user = TestFramework.GetUser();

            Assert.AreEqual(TestFramework.IsUserLoggedIn(user), true, "User not logged in");

            return user.userId;
        }

        public ulong GetMainAccountId()
        {
            var user = TestFramework.GetUser();

            Assert.AreEqual(TestFramework.IsUserLoggedIn(user), true, "User not logged in");

            return user.accountId;
        }

        public int GetUserId(int index)
        {
            var user = TestFramework.GetUser(index);

            Assert.AreEqual(TestFramework.IsUserLoggedIn(user), true, "User not logged in");

            return user.userId;
        }

        public ulong GetAccountId(int index)
        {
            var user = TestFramework.GetUser(index);

            Assert.AreEqual(TestFramework.IsUserLoggedIn(user), true, "User not logged in");

            return user.accountId;
        }

        public void LogException(System.Exception e)
        {
            TestFramework.LogException(e);
        }

        public bool CheckRequestOK<R>(R request) where R : Request
        {
            return TestFramework.CheckRequestOK<R>(request);
        }

        public void OutputApiResult(APIResult result)
        {
            TestFramework.OutputApiResult(result);
        }
    }

    static class TestFramework
    {
        public enum InitState
        {
            Started,
            Initalising,
            SetupUsers,
            UsersBeingSetup,
            Ready,
            Error,
        }

        static InitState s_CurrentInitState = InitState.Started;

        static public string s_InitOutput = "";

        static bool initOK = false;

        // Called by each test, which must make sure the PSN system is correctly initialized

        // Each test will have its own instance of BaseTestFramework, but the initialisation must be shared.
        // Only one of the tests should actually run it. Only one tests runs anyway at a time

        static public bool RunInitialize()
        {
            if (s_CurrentInitState == InitState.Ready)
            {
                return true;
            }

            // Give the app time to start up
            if ( Time.realtimeSinceStartup < 5.0f )
            {
                return false;
            }

            switch(s_CurrentInitState)
            {
                case InitState.Started:
                    initOK = InitializePackage();
                    s_CurrentInitState = InitState.Initalising;
                    break;
                case InitState.Initalising:
                    if (initOK == true) s_CurrentInitState = InitState.SetupUsers;
                    else s_CurrentInitState = InitState.Error;               
                    break;
                case InitState.SetupUsers:
                    if (SetupUsers() == true)
                    {
                        s_CurrentInitState = InitState.UsersBeingSetup;
                    }
                    else s_CurrentInitState = InitState.Error;
                    break;
                case InitState.UsersBeingSetup:
                    // Wait for users to be initialised correctly.
                    WaitForUsers();
                    break;
                case InitState.Error:
                    return true; // 
            }

            return false;
        }

        public class TestUser
        {
            public PlatformInput.LoggedInUser LoggedInUser;
            public bool IsReady;
            public bool SetupError;
        }

        static List<TestUser> testUsers = new List<TestUser>();

        static private bool SetupUsers()
        {
            // Find every user currently logged on and signed into PSN.
            // Then add them to the list of available users.
            for(int i = 0; i < 4; i++)
            {
                var loggedInUser = PlatformInput.RefreshUsersDetails(i);

                if(loggedInUser.status == 1 && loggedInUser.onlineStatus == PlatformInput.OnlineStatus.SignedIn)
                {
                    TestUser newUser = new TestUser();
                    newUser.LoggedInUser = loggedInUser;

                    AddUserTest(newUser);
                }
            }

            if(testUsers.Count == 0)
            {
                s_InitOutput = "Unable to find any logged-in and signed-in users on the console.";
                return false;
            }    

            return true;
        }

        private static void AddUserTest(TestUser user)
        {
            testUsers.Add(user);

            var internalRequest = new UserSystem.AddUserRequest() { UserId = user.LoggedInUser.userId };

            var requestOp = new AsyncRequest<UserSystem.AddUserRequest>(internalRequest).ContinueWith((antecedent) =>
            {
                if (antecedent != null && antecedent.Request != null)
                {
                    // Test is successful
                    if (TestFramework.CheckRequestOK(antecedent.Request))
                    {
                        user.IsReady = true;
                    }
                    else
                    {
                        user.SetupError = true;
                    }
                }
            });

            UserSystem.Schedule(requestOp);
        }

        static private void WaitForUsers()
        {
            bool allUsersReady = true;

            // Test if all users are ready and none of them have an error
            for(int i = 0; i < testUsers.Count; i++)
            {
                if(testUsers[i].IsReady == false)
                {
                    allUsersReady = false;
                }
                if (testUsers[i].SetupError == true)
                {
                    s_InitOutput = "Error has occured when setting up a user";
                    s_CurrentInitState = InitState.Error;
                    return;
                }
            }

            if (allUsersReady == true)
            {
                s_CurrentInitState = InitState.Ready;
            }
        }

        static public bool IsInitialized()
        {
            return s_CurrentInitState == InitState.Ready;
        }

        static public void TestState(InitState expectedState)
        {
            Assert.AreEqual(s_CurrentInitState, expectedState);
        }

        static public InitResult initResult;

        static private bool InitializePackage()
        {
            try
            {
                initResult = Main.Initialize();

                if (initResult.Initialized == true)
                {
                    Debug.Log("PSN Initialized ");
                }
                else
                {
                    Debug.Log("PSN not initialized ");
                }

                return initResult.Initialized;
            }
            catch (PSNException e)
            {
                Debug.LogError("Exception During Initialization : " + e.ExtendedMessage);
            }
#if UNITY_EDITOR
            catch (System.DllNotFoundException e)
            {
                Debug.LogError("Missing DLL Expection : " + e.Message);
                Debug.LogError("The sample APP will not run in the editor.");
            }
#endif

            return false;
        }

        /// <summary>
        ///  Return structure of first LoggedInUser for either PS5 or PS4 platform
        /// </summary>
        public static PlatformInput.LoggedInUser GetUser(int index = 0)
        {
            //PlatformInput.LoggedInUser loggedInUser = PlatformInput.RefreshUsersDetails(0);
            return testUsers[index].LoggedInUser;
        }

        public static bool IsUserLoggedIn(PlatformInput.LoggedInUser loggedInUser)
        {
            return loggedInUser.status == 1;
        }

        public static void LogException(System.Exception e)
        {
            if (e is PSNException)
            {
                PSNException psne = e as PSNException;

                Debug.LogError("PSN Exception : " + psne.ExtendedMessage);
            }
            else
            {
                Debug.LogError("Exception : " + e.Message);
            }
        }

        public static bool CheckRequestOK<R>(R request) where R : Request
        {
            Assert.IsNotNull(request, "Request object is null");

            if (request == null)
            {
                UnityEngine.Debug.LogError("Request is null");
                return false;
            }

            if (request.Result.apiResult == APIResultTypes.Success)
            {
                return true;
            }

            OutputApiResult(request.Result);

            return false;
        }

        public static void OutputApiResult(APIResult result)
        {
            if (result.apiResult == APIResultTypes.Success)
            {
                return;
            }

            string output = result.ErrorMessage();

            if (result.apiResult == APIResultTypes.Error)
            {
                UnityEngine.Debug.LogError(output);
            }
            else
            {
                UnityEngine.Debug.LogWarning(output);
            }
        }
    }
}

#endif

