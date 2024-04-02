
#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5.GameIntent;
#endif

namespace PSNSample
{
#if UNITY_PS5 || UNITY_PS4

    public class SonyGameIntentNotifications
    {
        public SonyGameIntentNotifications()
        {
            GameIntentSystem.OnGameIntentNotification += OnGameIntentNotification;
        }

        private void OnGameIntentNotification(GameIntentSystem.GameIntent gameIntent)
        {
            OnScreenLog.Add("Game Intent");
            OnScreenLog.Add(System.String.Format("    UserId : 0x{0:X}", gameIntent.UserId));
            OnScreenLog.Add("    Intent Type : " + gameIntent.IntentType);

            if (gameIntent is GameIntentSystem.LaunchActivity)
            {
                GameIntentSystem.LaunchActivity activity = gameIntent as GameIntentSystem.LaunchActivity;
                OnScreenLog.Add("    Activity Id : " + activity.ActivityId);
            }
            else if (gameIntent is GameIntentSystem.JoinSession)
            {
                GameIntentSystem.JoinSession activity = gameIntent as GameIntentSystem.JoinSession;
                OnScreenLog.Add("    Player Session Id : " + activity.PlayerSessionId);
                OnScreenLog.Add("    Member Type : " + activity.MemberType);
            }
            else if (gameIntent is GameIntentSystem.LaunchMultiplayerActivity)
            {
                GameIntentSystem.LaunchMultiplayerActivity activity = gameIntent as GameIntentSystem.LaunchMultiplayerActivity;
                OnScreenLog.Add("    Activity Id : " + activity.ActivityId);
                OnScreenLog.Add("    Player Session Id : " + activity.PlayerSessionId);
            }
        }
    }
#endif
}
