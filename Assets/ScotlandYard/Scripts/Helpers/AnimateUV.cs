using UnityEngine;
using System.Collections;

public class AnimateUV : MonoBehaviour
{
    public Vector2 animRate = new Vector2(1f, 0f);

    void Update()
    {
        UITexture tex = GetComponent<UITexture>();

        if (tex != null)
        {
            Rect rect = tex.uvRect;
            rect.x += (animRate.x/1000) * Time.deltaTime;
            rect.y += (animRate.y/1000) * Time.deltaTime;
            // modulo to avoid floating point imprecision in shader
            rect.x %= 1.0f;
            rect.y %= 1.0f;
            tex.uvRect = rect;
        }
    }
}
