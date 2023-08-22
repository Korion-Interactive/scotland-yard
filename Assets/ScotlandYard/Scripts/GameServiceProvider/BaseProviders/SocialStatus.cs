using System;
using System.Collections.Generic;
using System.Linq;
using Ravity;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class SocialStatus
{
    public bool IsAvailable => Social.localUser.authenticated;
    public string PlayerName => Social.localUser.userName;

    public event Action<IEnumerable<Achievement>> AchievementsRetrievedEvt;
    public event Action<string> AchievementUnlockedSuccessfully;

    public void Initialize()
    {
        #if GOOGLE_PLAY
            GooglePlayGames.PlayGamesPlatform.Activate();
        #endif

        SignIn();
    }

    public void Reconnect()
    {
        // TODO: sign out?
        SignIn();
    }

    public void ShowAchievements()
    {
        Social.ShowAchievementsUI();
    }

    public void UnlockAchievement(string achievementId)
    {
        Social.ReportProgress(achievementId, 100f, success =>
        {
            if (success)
            {
                AchievementUnlockedSuccessfully?.Invoke(achievementId);
            }
        });
    }

    public void ProgressAchievement(string achievementId, int stepIncrement, int totalSteps)
    {
        if (HardwareUtils.IsAndroid)
        {
            #if GOOGLE_PLAY
            // https://github.com/playgameservices/play-games-plugin-for-unity#incrementing-an-achievement
            GooglePlayGames.PlayGamesPlatform.Instance.IncrementAchievement(achievementId, stepIncrement, success =>
            {
                if (success == false)
                {
                    this.LogError($"Failed to report achievement progress for '{achievementId}'.");
                }
            });
            #endif
        }
        else
        {
            Social.LoadAchievements(achievements =>
            {
                double currentProgress = 0.0;
                IAchievement achievement = achievements.FirstOrDefault(a => a.id == achievementId);
                if (achievement != null)
                {
                    currentProgress = achievement.percentCompleted;
                }

                bool alreadyUnlocked = Math.Abs(100.0 - currentProgress) < Mathf.Epsilon;
                if (alreadyUnlocked)
                {
                    return;
                }

                double percentIncrement = (double)(100 * stepIncrement) / totalSteps;
                double percentTotal = percentIncrement + currentProgress;

                Social.ReportProgress(achievementId, percentTotal, success =>
                {
                    if (success == false)
                    {
                        this.LogError($"Failed to report achievement progress for '{achievementId}'.");
                    }
                });
            });
        }
    }

    public void RetrieveAchievementData()
    {
        Social.LoadAchievements(achievements =>
        {
            AchievementsRetrievedEvt?.Invoke(achievements.Select(a => new Achievement(a)));
        });
    }

    private void SignIn()
    {
        Social.localUser.Authenticate((success, message) =>
        {
            if (success == false)
            {
                this.LogError($"Authentication failed: {message}");
            }
        });
    }
}
