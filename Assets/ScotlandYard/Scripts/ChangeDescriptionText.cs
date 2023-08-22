using UnityEngine;
using System.Collections;

public class ChangeDescriptionText : MonoBehaviour {

    public UIScrollView Scroll;
    public LabelTranslator Label;
    public string[] LabelText;

	// Use this for initialization
	void Start () {

        StartCoroutine(SetFirstText());

	}
	
	// Update is called once per frame
	void Update () {

        if (Scroll != null)
        {
            Scroll.UpdatePosition();
        }
        
	}

    public void ChangeLabelText(int arg)
    {
        Label.SetText(LabelText[arg]);
        if (Scroll != null)
        {
            Scroll.ResetPosition();
        }
    }

    IEnumerator SetFirstText()
    {
        yield return new WaitForEndOfFrame();
        Label.SetText(LabelText[0]);
        if (Scroll != null)
        {
            Scroll.ResetPosition();
        }
    }

}
