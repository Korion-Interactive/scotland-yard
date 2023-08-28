using UnityEngine;
using System.Collections;

public enum SunShaftsResolution
{
    Low = 0,
    Normal = 1,
    High = 2
}

public enum ShaftsScreenBlendMode
{
    Screen = 0,
    Add = 1
}

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Rendering/Sun Shafts")]
public partial class SunShafts : PostEffectsBase
{
    public SunShaftsResolution resolution;
    public ShaftsScreenBlendMode screenBlendMode;
    public Transform sunTransform;
    public int radialBlurIterations;
    public Color sunColor;
    public float sunShaftBlurRadius;
    public float sunShaftIntensity;
    public float useSkyBoxAlpha;
    public float maxRadius;
    public bool useDepthTexture;
    public Shader sunShaftsShader;
    private Material sunShaftsMaterial;
    public Shader simpleClearShader;
    private Material simpleClearMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(this.useDepthTexture);
        this.sunShaftsMaterial = this.CheckShaderAndCreateMaterial(this.sunShaftsShader, this.sunShaftsMaterial);
        this.simpleClearMaterial = this.CheckShaderAndCreateMaterial(this.simpleClearShader, this.simpleClearMaterial);
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
        // we actually need to check this every frame
        if (this.useDepthTexture)
        {
            this.GetComponent<Camera>().depthTextureMode = this.GetComponent<Camera>().depthTextureMode | DepthTextureMode.Depth;
        }
        int divider = 4;
        if (this.resolution == SunShaftsResolution.Normal)
        {
            divider = 2;
        }
        else
        {
            if (this.resolution == SunShaftsResolution.High)
            {
                divider = 1;
            }
        }
        Vector3 v = Vector3.one * 0.5f;
        if (this.sunTransform)
        {
            v = this.GetComponent<Camera>().WorldToViewportPoint(this.sunTransform.position);
        }
        else
        {
            v = new Vector3(0.5f, 0.5f, 0f);
        }
        int rtW = source.width / divider;
        int rtH = source.height / divider;
        RenderTexture lrColorB = null;
        RenderTexture lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0);
        // mask out everything except the skybox
        // we have 2 methods, one of which requires depth buffer support, the other one is just comparing images
        this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * this.sunShaftBlurRadius);
        this.sunShaftsMaterial.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, this.maxRadius));
        this.sunShaftsMaterial.SetFloat("_NoSkyBoxMask", 1f - this.useSkyBoxAlpha);
        if (!this.useDepthTexture)
        {
            RenderTexture tmpBuffer = RenderTexture.GetTemporary(source.width, source.height, 0);
            RenderTexture.active = tmpBuffer;
            GL.ClearWithSkybox(false, this.GetComponent<Camera>());
            this.sunShaftsMaterial.SetTexture("_Skybox", tmpBuffer);
            Graphics.Blit(source, lrDepthBuffer, this.sunShaftsMaterial, 3);
            RenderTexture.ReleaseTemporary(tmpBuffer);
        }
        else
        {
            Graphics.Blit(source, lrDepthBuffer, this.sunShaftsMaterial, 2);
        }
        // paint a small black small border to get rid of clamping problems
        this.DrawBorder(lrDepthBuffer, this.simpleClearMaterial);
        // radial blur:
        this.radialBlurIterations = Mathf.Clamp(this.radialBlurIterations, 1, 4);
        float ofs = this.sunShaftBlurRadius * (1f / 768f);
        this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0f, 0f));
        this.sunShaftsMaterial.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, this.maxRadius));
        int it2 = 0;
        while (it2 < this.radialBlurIterations)
        {
            // each iteration takes 2 * 6 samples
            // we update _BlurRadius each time to cheaply get a very smooth look
            lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0);
            Graphics.Blit(lrDepthBuffer, lrColorB, this.sunShaftsMaterial, 1);
            RenderTexture.ReleaseTemporary(lrDepthBuffer);
            ofs = (this.sunShaftBlurRadius * (((it2 * 2f) + 1f) * 6f)) / 768f;
            this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0f, 0f));
            lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0);
            Graphics.Blit(lrColorB, lrDepthBuffer, this.sunShaftsMaterial, 1);
            RenderTexture.ReleaseTemporary(lrColorB);
            ofs = (this.sunShaftBlurRadius * (((it2 * 2f) + 2f) * 6f)) / 768f;
            this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0f, 0f));
            it2++;
        }
        // put together:
        if (v.z >= 0f)
        {
            this.sunShaftsMaterial.SetVector("_SunColor", new Vector4(this.sunColor.r, this.sunColor.g, this.sunColor.b, this.sunColor.a) * this.sunShaftIntensity);
        }
        else
        {
            this.sunShaftsMaterial.SetVector("_SunColor", Vector4.zero); // no backprojection !
        }
        this.sunShaftsMaterial.SetTexture("_ColorBuffer", lrDepthBuffer);
        Graphics.Blit(source, destination, this.sunShaftsMaterial, this.screenBlendMode == ShaftsScreenBlendMode.Screen ? 0 : 4);
        RenderTexture.ReleaseTemporary(lrDepthBuffer);
    }

    public SunShafts()
    {
        this.resolution = SunShaftsResolution.Normal;
        this.screenBlendMode = ShaftsScreenBlendMode.Screen;
        this.radialBlurIterations = 2;
        this.sunColor = Color.white;
        this.sunShaftBlurRadius = 2.5f;
        this.sunShaftIntensity = 1.15f;
        this.useSkyBoxAlpha = 0.75f;
        this.maxRadius = 0.75f;
        this.useDepthTexture = true;
    }

}