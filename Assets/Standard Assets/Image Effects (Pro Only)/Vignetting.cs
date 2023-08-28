using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Camera/Vignette and Chromatic Aberration")]
public partial class Vignetting : PostEffectsBase
{
    public enum AberrationMode
    {
        Simple = 0,
        Advanced = 1
    }


    public Vignetting.AberrationMode mode;
    public float intensity; // intensity == 0 disables pre pass (optimization)
    public float chromaticAberration;
    public float axialAberration;
    public float blur; // blur == 0 disables blur pass (optimization)
    public float blurSpread;
    public float luminanceDependency;
    public float blurDistance;
    public Shader vignetteShader;
    private Material vignetteMaterial;
    public Shader separableBlurShader;
    private Material separableBlurMaterial;
    public Shader chromAberrationShader;
    private Material chromAberrationMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(false);
        this.vignetteMaterial = this.CheckShaderAndCreateMaterial(this.vignetteShader, this.vignetteMaterial);
        this.separableBlurMaterial = this.CheckShaderAndCreateMaterial(this.separableBlurShader, this.separableBlurMaterial);
        this.chromAberrationMaterial = this.CheckShaderAndCreateMaterial(this.chromAberrationShader, this.chromAberrationMaterial);
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        int rtW = source.width;
        int rtH = source.height;
        bool doPrepass = (Mathf.Abs(this.blur) > 0f) || (Mathf.Abs(this.intensity) > 0f);
        float widthOverHeight = (1f * rtW) / (1f * rtH);
        float oneOverBaseSize = 1f / 512f;
        RenderTexture color = null;
        RenderTexture color2a = null;
        RenderTexture color2b = null;
        if (doPrepass)
        {
            color = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            // Blur corners
            if (Mathf.Abs(this.blur) > 0f)
            {
                color2a = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);
                Graphics.Blit(source, color2a, this.chromAberrationMaterial, 0);
                int i = 0;
                while (i < 2) // maybe make iteration count tweakable
                {
                    this.separableBlurMaterial.SetVector("offsets", new Vector4(0f, this.blurSpread * oneOverBaseSize, 0f, 0f));
                    color2b = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);
                    Graphics.Blit(color2a, color2b, this.separableBlurMaterial);
                    RenderTexture.ReleaseTemporary(color2a);
                    this.separableBlurMaterial.SetVector("offsets", new Vector4((this.blurSpread * oneOverBaseSize) / widthOverHeight, 0f, 0f, 0f));
                    color2a = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0, source.format);
                    Graphics.Blit(color2b, color2a, this.separableBlurMaterial);
                    RenderTexture.ReleaseTemporary(color2b);
                    i++;
                }
            }
            this.vignetteMaterial.SetFloat("_Intensity", this.intensity); // intensity for vignette
            this.vignetteMaterial.SetFloat("_Blur", this.blur); // blur intensity
            this.vignetteMaterial.SetTexture("_VignetteTex", color2a); // blurred texture
            Graphics.Blit(source, color, this.vignetteMaterial, 0); // prepass blit: darken & blur corners
        }
        this.chromAberrationMaterial.SetFloat("_ChromaticAberration", this.chromaticAberration);
        this.chromAberrationMaterial.SetFloat("_AxialAberration", this.axialAberration);
        this.chromAberrationMaterial.SetVector("_BlurDistance", new Vector2(-this.blurDistance, this.blurDistance));
        this.chromAberrationMaterial.SetFloat("_Luminance", 1f / Mathf.Max(Mathf.Epsilon, this.luminanceDependency));
        if (doPrepass)
        {
            color.wrapMode = TextureWrapMode.Clamp;
        }
        else
        {
            source.wrapMode = TextureWrapMode.Clamp;
        }
        Graphics.Blit(doPrepass ? color : source, destination, this.chromAberrationMaterial, this.mode == AberrationMode.Advanced ? 2 : 1);
        RenderTexture.ReleaseTemporary(color);
        RenderTexture.ReleaseTemporary(color2a);
    }

    public Vignetting()
    {
        this.mode = AberrationMode.Simple;
        this.intensity = 0.375f;
        this.chromaticAberration = 0.2f;
        this.axialAberration = 0.5f;
        this.blurSpread = 0.75f;
        this.luminanceDependency = 0.25f;
        this.blurDistance = 2.5f;
    }

}
 /* And Chromatic Aberration */