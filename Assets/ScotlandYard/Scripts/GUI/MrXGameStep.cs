using UnityEngine;
using System.Collections;

public class MrXGameStep : MonoBehaviour
{
    public int Step;
    public bool isShowUpStep;

    public UISprite ShowUpSprite;
    public UILabel Label;

    void Start()
    {
        //if (isShowUpStep) ShowUpSprite.enabled = true;
        Label.text = Step.ToString();
    }

    public void SetMrXMoveStep(string transportSpriteName)
    {
        UISprite sprite = GetComponent<UISprite>();
        sprite.spriteName = transportSpriteName;
        sprite.SetDirty();

        Label.enabled = false;
    }

}
