using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Edge Detection/Crease Shading")]
public partial class Crease : PostEffectsBase
{
    public float intensity;
    public int softness;
    public float spread;
    public Shader blurShader;
    private Material blurMaterial;
    public Shader depthFetchShader;
    private Material depthFetchMaterial;
    public Shader creaseApplyShader;
    private Material creaseApplyMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(true);
        this.blurMaterial = this.CheckShaderAndCreateMaterial(this.blurShader, this.blurMaterial);
        this.depthFetchMaterial = this.CheckShaderAndCreateMaterial(this.depthFetchShader, this.depthFetchMaterial);
        this.creaseApplyMaterial = this.CheckShaderAndCreateMaterial(this.creaseApplyShader, this.creaseApplyMaterial);
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
        float widthOverHeight = (1f * rtW) / (1f * rtH);
        float oneOverBaseSize = 1f / 512f;
        RenderTexture hrTex = RenderTexture.GetTemporary(rtW, rtH, 0);
        RenderTexture lrTex1 = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0);
        Graphics.Blit(source, hrTex, this.depthFetchMaterial);
        Graphics.Blit(hrTex, lrTex1);
        int i = 0;
        while (i < this.softness)
        {
            RenderTexture lrTex2 = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0);
            this.blurMaterial.SetVector("offsets", new Vector4(0f, this.spread * oneOverBaseSize, 0f, 0f));
            Graphics.Blit(lrTex1, lrTex2, this.blurMaterial);
            RenderTexture.ReleaseTemporary(lrTex1);
            lrTex1 = lrTex2;
            lrTex2 = RenderTexture.GetTemporary(rtW / 2, rtH / 2, 0);
            this.blurMaterial.SetVector("offsets", new Vector4((this.spread * oneOverBaseSize) / widthOverHeight, 0f, 0f, 0f));
            Graphics.Blit(lrTex1, lrTex2, this.blurMaterial);
            RenderTexture.ReleaseTemporary(lrTex1);
            lrTex1 = lrTex2;
            i++;
        }
        this.creaseApplyMaterial.SetTexture("_HrDepthTex", hrTex);
        this.creaseApplyMaterial.SetTexture("_LrDepthTex", lrTex1);
        this.creaseApplyMaterial.SetFloat("intensity", this.intensity);
        Graphics.Blit(source, destination, this.creaseApplyMaterial);
        RenderTexture.ReleaseTemporary(hrTex);
        RenderTexture.ReleaseTemporary(lrTex1);
    }

    public Crease()
    {
        this.intensity = 0.5f;
        this.softness = 1;
        this.spread = 1f;
    }

}