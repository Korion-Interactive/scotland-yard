using UnityEngine;
using System.Collections;

public class DestroyGameState : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		GameState.ReleaseInstance ();
	}
	

}
