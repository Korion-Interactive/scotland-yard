using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class IngameOptionsButton : MonoBehaviour
{

    public void EndCurrentGame()
    {
        this.Broadcast<GameGuiEvents>(GameGuiEvents.LoadingScene);
		SceneManager.LoadSceneAsync("MainMenu");
    }
}
