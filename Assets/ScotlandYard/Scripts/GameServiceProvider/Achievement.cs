using UnityEngine.SocialPlatforms;

public class Achievement
{
    public string Identifier { get; }
    public bool Completed { get; }

    public Achievement(IAchievement unityAchievement)
    {
        Identifier = unityAchievement.id;
        Completed = unityAchievement.completed;
    }
}
