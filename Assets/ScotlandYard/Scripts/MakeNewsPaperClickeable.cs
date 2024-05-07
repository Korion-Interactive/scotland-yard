using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MakeNewsPaperClickeable : MonoBehaviour {

    private void Awake()
    {
        if (GameSetupBehaviour.Instance.Setup.Mode.IsTutorial())
        {
            Destroy(GetComponent<ChangeActionMap>());
        }
    }
    // Use this for initialization
    void Start () {
	    if(GameSetupBehaviour.Instance.Setup.Mode.IsTutorial())
        {
            GetComponent<ActionReceiver>().enabled = false;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void MakeClickable()
    {
        if (GameSetupBehaviour.Instance.Setup.Mode != GameMode.TutorialDetective)
        {
            this.gameObject.GetComponent<UIEventListener>().onClick += LoadMenu;
        }
    }

    public void LoadMenu(GameObject obj)
    {
        if(!GameSetupBehaviour.Instance.Setup.Mode.IsTutorial())
        {
            AppSetup.Instance.DeleteSavegame();
        }

        if (GSP.IsMultiplayerRTAvailable && GSP.MultiplayerRT.IsConnected)
            GSP.MultiplayerRT.Disconnect();

        this.Broadcast<GameGuiEvents>(GameGuiEvents.LoadingScene);
		SceneManager.LoadSceneAsync("MainMenu");
    }
}
