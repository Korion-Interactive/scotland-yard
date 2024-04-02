using UnityEngine;
using System.Collections;
using Ravity;
using Cysharp.Threading.Tasks;

#if UNITY_ANDROID && GOOGLE_PLAY
    using BluetoothMultiplayer = Bluetooth.Android.MultiplayerRT;
#elif UNITY_IPHONE
    using BluetoothMultiplayer = Bluetooth.iOS.Multipeer.MultiplayerRT;
#endif
public class AppSetupSettings : MonoBehaviour
{
    const int ENABLED = 1;
    const int DISABLED = 0;

    public GameObject ContinueButton;

    bool setFromSettings = false;
    GameObject voip;

    public void Set_Music_Sfx_VoIP_PostFX(GameObject music, GameObject sfx, GameObject voIP, GameObject postFX)
    {
        this.voip = voIP;
        StartCoroutine(SetSettings(music, sfx, voIP, postFX));
    }

    IEnumerator SetSettings(GameObject music, GameObject sfx, GameObject voIP, GameObject postFX)
    {
        setFromSettings = true;

        yield return new WaitForEndOfFrame();

        SetState(music, AppSetup.Instance.IsMusicEnabled);
        SetState(sfx, AppSetup.Instance.IsSfxEnabled);

        SetState(postFX, AppSetup.Instance.IsPostEffectEnabled);
        SetState(voIP, AppSetup.Instance.IsVoiceChatEnabled);

        // disable voice chat for all platforms
        voIP.SetActive(false);

        setFromSettings = false;
    }

    private void SetState(GameObject stateButtonHolder, bool val)
    {
        int state = (val) ? ENABLED : DISABLED;
        stateButtonHolder.GetComponent<UIStateButton>().ApplyState(state);
    }

    public void EnableMusic(int buttonState)
    {
        if (setFromSettings)
            return;

        AppSetup.Instance.IsMusicEnabled = buttonState == ENABLED;
    }

    public void EnableSound(int buttonState)
    {
        if (setFromSettings)
            return;

        AppSetup.Instance.IsSfxEnabled = buttonState == ENABLED;
    }

    public void EnableStatusBar(int buttonState)
    {
        if (setFromSettings)
            return;

        AppSetup.Instance.IsStatusBarVisible = buttonState == ENABLED;

    }

    public void EnableVoiceChat(int buttonState)
    {
        if (setFromSettings)
            return;

        AppSetup.Instance.IsVoiceChatEnabled = buttonState == ENABLED;

#if UNITY_IPHONE
        // It is not possible to enable voice chat on iOS bluetooth multiplayer during game
        if (GameState.HasInstance && voip != null && (!AppSetup.Instance.IsVoiceChatEnabled && GSP.IsMultiplayerRTAvailable && GSP.MultiplayerRT is BluetoothMultiplayer))
        {
            voip.SetActive(false);
        }
#endif
    }

    public void EnablePostEffects(int buttonState)
    {
        if (setFromSettings)
            return;

        AppSetup.Instance.IsPostEffectEnabled = buttonState == ENABLED;

        PostProcessingOnCameras();
    }

    public void PostProcessingOnCameras()
    {
        // Get all cameras and disable all post processing effects
        var camerasArray = new Camera[Camera.allCamerasCount];
        Camera.GetAllCameras(camerasArray);
        foreach (var cam in camerasArray)
        {
            cam.gameObject.SetComponentsEnabled<PostEffectsBase>(AppSetup.Instance.IsPostEffectEnabled);
        }
    }

    public void ShowAchievements()
    {
        if (InteractionSpacer.IsTooNarrow())
        {
            return;
        }

        if (GSP.IsStatusAvailable)
        {
            GSP.Status.ShowAchievements();
        }
        else
        {
            GSP.Status.Reconnect();

            float timeOut = Time.realtimeSinceStartup + 10f;
            this.WaitAndDo(new WaitForSeconds(0.2f),
                () => GSP.IsStatusAvailable || Time.realtimeSinceStartup > timeOut,
                () => { if (GSP.IsStatusAvailable) GSP.Status.ShowAchievements(); });
        }
    }


    public void TryPauseGame()
    {
        if (GameState.HasInstance && (GameSetupBehaviour.Instance.Setup.Mode == GameMode.HotSeat || GameSetupBehaviour.Instance.Setup.Mode.IsTutorial()))
            GameState.Instance.SetPausing(true, false);
    }

    public void TryUnpauseGame()
    {
        if (GameState.HasInstance && (GameSetupBehaviour.Instance.Setup.Mode == GameMode.HotSeat || GameSetupBehaviour.Instance.Setup.Mode.IsTutorial()))
            GameState.Instance.SetPausing(false, false);
    }

    public void LeaveGame()
    {
        if (GameState.HasInstance)
        {
            if (GameSetupBehaviour.Instance.Setup.Mode == GameMode.HotSeat && !GameState.Instance.IsGameOver)
            {
                AppSetup.Instance.SaveGame();
            }

            GameState.ReleaseInstance();
        }

        if (GameSetupBehaviour.Instance.Setup.Mode == GameMode.Network)
        {
            GSP.MultiplayerRT.Disconnect();
        }
    }

    public void ContinueGame()
    {
        ContinueAsync().Forget();
    }

    async UniTaskVoid ContinueAsync()
    {
        bool hasOpenGame = await AppSetup.Instance.HasOpenGame();
        if (!hasOpenGame)
            return;

        AppSetup.Instance.LoadLastGame();
    }


    public void NewsClicked()
    {
        ImprintClicked();
    }

    public void MoreGamesClicked()
    {
        if (InteractionSpacer.IsTooNarrow())
        {
            return;
        }
        
        if (HardwareUtils.IsAndroid)
        {
            // see https://developer.android.com/distribute/marketing-tools/linking-to-google-play?hl=de#OpeningPublisher
            Application.OpenURL("https://play.google.com/store/apps/dev?id=7057959691151910430");
        }
        else
        {
            Application.OpenURL("https://apps.apple.com/developer/id1505283467");
        }
    }

    public void ImprintClicked()
    {
        if (InteractionSpacer.IsTooNarrow())
        {
            return;
        }
        
        // open scotland yard specific privacy url in browser
        SystemLanguage language = Loc.Language;
        if (language == SystemLanguage.German)
        {
            Application.OpenURL("https://assets.ravensburger.com/legal/privacy/scotlandyard_mobile/privacy_german.html");
        }
        else if (language == SystemLanguage.French)
        {
            Application.OpenURL("https://assets.ravensburger.com/legal/privacy/scotlandyard_mobile/privacy_french.html");
        }
        else if (language == SystemLanguage.Spanish)
        {
            Application.OpenURL("https://assets.ravensburger.com/legal/privacy/scotlandyard_mobile/privacy_spanish.html");
        }
        else if (language == SystemLanguage.Italian)
        {
            Application.OpenURL("https://assets.ravensburger.com/legal/privacy/scotlandyard_mobile/privacy_italian.html");
        }
        else
        {
            Application.OpenURL("https://assets.ravensburger.com/legal/privacy/scotlandyard_mobile/privacy_english.html");
        }
    }

    //DOES this still trigger --> KORION IO
    async UniTaskVoid Start()
    {
        PostProcessingOnCameras();

        if (ContinueButton != null)
        {
            bool hasOpenGame = await AppSetup.Instance.HasOpenGame();
            ContinueButton.SetActive(hasOpenGame);
        }    
    }
}
