using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
#if UNITY_PS5
using Unity.PSN.PS5.GameIntent;
using static Unity.PSN.PS5.GameIntent.GameIntentSystem;
#endif


[DefaultExecutionOrder(-1)]
public class GameIntentManager : MonoBehaviour
{
    private static GameIntentManager _instance;
    public static GameIntentManager Instance => _instance;

    public bool HasIntent
    {
        get
        {
#if UNITY_PS5
            return GameIntent != null;
#else
                return false;
#endif
        }
    }

    private async UniTaskVoid Awake()
    {
        Debug.Log("Starting GameIntentManager");
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Type Safety", "UNT0006", Justification = "Method signature is not incorrect as UniTask will make it work")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051", Justification = "Member not unused and should not be removed")]
    private async UniTaskVoid Start()
    {
        Debug.Log("In Start of GameIntentManager");
        if (_instance == this)
        {
            StartListeningForGameIntent();

#if UNITY_PS5 && !UNITY_EDITOR
            // This stuff must be done within the first frame, otherwise we will not receive "LaunchActivity" events which originate from "continue activity" on a closed application
            if (!Korion.PS5.PSN.PSNService.IsInitialized)
            {
                await Korion.PS5.PSN.PSNFacade.InitializeAsync(System.Threading.CancellationToken.None);
                await Korion.PS5.PSN.UniversalDataSystemService.AddUserAsync(UnityEngine.PS5.Utility.initialUserId, System.Threading.CancellationToken.None);
            }
#endif
        }
        await UniTask.CompletedTask; // only exists to get rid of warning: missing await operator
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            StopListeningForGameIntent();
            _instance = null;
        }
    }

    [System.Diagnostics.Conditional("UNITY_PS5")]
    private void StartListeningForGameIntent()
    {
#if UNITY_PS5 && !UNITY_EDITOR
            GameIntentSystem.OnGameIntentNotification += OnGameIntentNotification;
#endif
    }

    [System.Diagnostics.Conditional("UNITY_PS5")]
    private void StopListeningForGameIntent()
    {
#if UNITY_PS5 && !UNITY_EDITOR
            GameIntentSystem.OnGameIntentNotification -= OnGameIntentNotification;
#endif
    }

#if UNITY_PS5
    public GameIntent GameIntent { get; private set; }

    private GameIntentNotificationEvent _notificationEvent = new GameIntentNotificationEvent();
    public class GameIntentNotificationEvent : UnityEvent<GameIntent> { }
    public GameIntentNotificationEvent NotificationEvent => _notificationEvent;

    /// <summary>
    /// Sets <see cref="GameIntent"/> to null.
    /// </summary>
    public void MarkAsHandled() => GameIntent = null;
#endif

#if UNITY_PS5 && !UNITY_EDITOR

        private void OnGameIntentNotification(GameIntentSystem.GameIntent gameIntent)
        {
            Debug.Log($"[{nameof(GameIntentManager)}] Frame #{Time.frameCount}: Received gameintent with type: {gameIntent.IntentType}");

            if (gameIntent.IntentType == GameIntentSystem.GameIntent.IntentTypes.LaunchActivity)
            {
                GameIntent = gameIntent;
                if (NotificationEvent != null)
                    NotificationEvent.Invoke(GameIntent);
            }
        }
#endif
}
