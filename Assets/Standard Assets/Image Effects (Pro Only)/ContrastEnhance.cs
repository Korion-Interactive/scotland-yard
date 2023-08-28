using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Color Adjustments/Contrast Enhance (Unsharp Mask)")]
public partial class ContrastEnhance : PostEffectsBase
{
    public float intensity;
    public float threshhold;
    private Material separableBlurMaterial;
    private Material contrastCompositeMaterial;
    public float blurSpread;
    public Shader separableBlurShader;
    public Shader contrastCompositeShader;
    public override bool CheckResources()
    {
        this.CheckSupport(false);
        this.contrastCompositeMaterial = this.CheckShaderAndCreateMaterial(this.contrastCompositeShader, this.contrastCompositeMaterial);
        this.separableBlurMaterial = this.CheckShaderAndCreateMaterial(this.separableBlurShader, this.separableBlurMaterial);
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
        RenderTexture color2 = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0);
        // downsample
        Graphics.Blit(source, color2);
        RenderTexture color4a = RenderTexture.GetTemporary(rtW / 4, rtH / 4, 0);
        Graphics.Blit(color2, color4a);
        RenderTexture.ReleaseTemporary(color2);
        // blur
        this.separableBlurMaterial.SetVector("offsets", new Vector4(0f, (this.blurSpread * 1f) / color4a.height, 0f, 0f));
        RenderTexture color4b = RenderTexture.GetTemporary(rtW / 4, rtH / 4, 0);
        Graphics.Blit(color4a, color4b, this.separableBlurMaterial);
        RenderTexture.ReleaseTemporary(color4a);
        this.separableBlurMaterial.SetVector("offsets", new Vector4((this.blurSpread * 1f) / color4a.width, 0f, 0f, 0f));
        color4a = RenderTexture.GetTemporary(rtW / 4, rtH / 4, 0);
        Graphics.Blit(color4b, color4a, this.separableBlurMaterial);
        RenderTexture.ReleaseTemporary(color4b);
        // composite
        this.contrastCompositeMaterial.SetTexture("_MainTexBlurred", color4a);
        this.contrastCompositeMaterial.SetFloat("intensity", this.intensity);
        this.contrastCompositeMaterial.SetFloat("threshhold", this.threshhold);
        Graphics.Blit(source, destination, this.contrastCompositeMaterial);
        RenderTexture.ReleaseTemporary(color4a);
    }

    public ContrastEnhance()
    {
        this.intensity = 0.5f;
        this.blurSpread = 1f;
    }

}