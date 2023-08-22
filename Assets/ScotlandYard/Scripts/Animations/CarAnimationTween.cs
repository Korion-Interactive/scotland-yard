using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class CarAnimationTween : MonoBehaviour
{

    public Vector3 FirstPosition;
    public Vector3 SecondPosition;
    public Vector2 SpeedRange;
    public Vector2 ReplayDelay;
    public bool bFadeToEnd;

    private UIWidget _uiWidget;

    void Awake()
    {
        _uiWidget = GetComponent<UIWidget>();
    }

	// Use this for initialization
	void Start ()
	{
	    PlayAnimation();
	}

    void PlayAnimation()
    {
        // set start position at random
        GetRandomStartEndPositions();

        // calculate random speed factor
        var randomRange = Random.Range(SpeedRange.x, SpeedRange.y);
        var speed = 10f/randomRange;

        iTween.MoveTo(gameObject, iTween.Hash(
            "name", "MoveTo",
            "position", SecondPosition,
            "islocal", true,
            "time", speed,
            "easeType", "easeOutQuad",
            "oncomplete", "ReplayAnimation"
            ));

        if (bFadeToEnd)
        {
            iTween.ValueTo(gameObject, iTween.Hash(
                "name", "FadeTo",
                "from", 1.0f,
                "to", 0.0f,
                "time", speed,
                "easeType", "easeOutQuad",
                "onupdate", "UpdateTextureAlpha"
                ));
        }
    }

    void UpdateTextureAlpha(float val)
    {
        if (_uiWidget == null) return;

        _uiWidget.alpha = val;
    }

    void GetRandomStartEndPositions()
    {
        var random = Random.Range(0, 2);

        if (random != 1)
        {
            this.transform.localPosition = FirstPosition;
            return;
        }

        var startPos = SecondPosition;
        SecondPosition = FirstPosition;
        FirstPosition = startPos;

        this.transform.localPosition = FirstPosition;

        //Debug.Log(gameObject.name + " playing from: " + FirstPosition + " to " + SecondPosition);
    }

    void ReplayAnimation()
    {
        StartCoroutine("PlayAnimationWithDelay");
    }

    IEnumerator PlayAnimationWithDelay()
    {
        yield return new WaitForSeconds(Random.Range(ReplayDelay.x, ReplayDelay.y));
        PlayAnimation();
    }
	
}
