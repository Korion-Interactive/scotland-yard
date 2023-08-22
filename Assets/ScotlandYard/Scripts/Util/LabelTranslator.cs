using Ravity;
using UnityEngine;

[AddComponentMenu("NGUI/Bit Barons/Label Translator")]
[RequireComponent(typeof(UILabel))]
public class LabelTranslator : MonoBehaviour
{
    string originalText;
    UILabel label;

    public bool UseIosSuffix, UseAndroidSuffix;

    void Awake()
    {
        label = GetComponent<UILabel>();

        if(string.IsNullOrEmpty(originalText))
            originalText = label.text;

        Translate();
    }

    void OnEnable()
    {
        Translate();
    }

    public void SetTextWithStaticParams(string locId, params string[] parameters)
    {
        originalText = locId;
        Translate(parameters);
    }
    public void SetText(string locId)
    {
        originalText = locId;
        Translate();
    }

    public void ClearText()
    {
        originalText = "";
        label.text = "";
    }

    public void Translate(params string[] parameters)
    {
        if (label != null && !string.IsNullOrEmpty(originalText))
        {
            label.text = string.Format(Loc.Get(Suffix(originalText)), parameters);

            //if (gameObject.activeInHierarchy)
            //    StartCoroutine(UpdateFonts());
        }
    }
    public void Translate()
    {
        if (label != null && !string.IsNullOrEmpty(originalText))
        {
            label.text = Loc.Get(Suffix(originalText));

            //if (gameObject.activeInHierarchy)
            //    StartCoroutine(UpdateFonts());
        }
    }

    private string Suffix(string id)
    {
        if (HardwareUtils.IsAndroid && UseAndroidSuffix)
            return id + "_android";
        if (HardwareUtils.IsiOS && UseIosSuffix)
           return id + "_ios";

        return id;
    }

    //IEnumerator UpdateFonts()
    //{
    //    yield return new WaitForEndOfFrame();
    //    yield return GameInitializer.Instance.UpdateFonts();
    //}
}
