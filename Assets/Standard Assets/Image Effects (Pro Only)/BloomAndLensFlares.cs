using UnityEngine;
using System.Collections;

public enum LensflareStyle34
{
    Ghosting = 0,
    Anamorphic = 1,
    Combined = 2
}

public enum TweakMode34
{
    Basic = 0,
    Complex = 1
}

public enum HDRBloomMode
{
    Auto = 0,
    On = 1,
    Off = 2
}

public enum BloomScreenBlendMode
{
    Screen = 0,
    Add = 1
}

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Bloom and Glow/BloomAndFlares (3.5, Deprecated)")]
public partial class BloomAndLensFlares : PostEffectsBase
{
    public TweakMode34 tweakMode;
    public BloomScreenBlendMode screenBlendMode;
    public HDRBloomMode hdr;
    private bool doHdr;
    public float sepBlurSpread;
    public float useSrcAlphaAsMask;
    public float bloomIntensity;
    public float bloomThreshhold;
    public int bloomBlurIterations;
    public bool lensflares;
    public int hollywoodFlareBlurIterations;
    public LensflareStyle34 lensflareMode;
    public float hollyStretchWidth;
    public float lensflareIntensity;
    public float lensflareThreshhold;
    public Color flareColorA;
    public Color flareColorB;
    public Color flareColorC;
    public Color flareColorD;
    public float blurWidth;
    public Texture2D lensFlareVignetteMask;
    public Shader lensFlareShader;
    private Material lensFlareMaterial;
    public Shader vignetteShader;
    private Material vignetteMaterial;
    public Shader separableBlurShader;
    private Material separableBlurMaterial;
    public Shader addBrightStuffOneOneShader;
    private Material addBrightStuffBlendOneOneMaterial;
    public Shader screenBlendShader;
    private Material screenBlend;
    public Shader hollywoodFlaresShader;
    private Material hollywoodFlaresMaterial;
    public Shader brightPassFilterShader;
    private Material brightPassFilterMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(false);
        this.screenBlend = this.CheckShaderAndCreateMaterial(this.screenBlendShader, this.screenBlend);
        this.lensFlareMaterial = this.CheckShaderAndCreateMaterial(this.lensFlareShader, this.lensFlareMaterial);
        this.vignetteMaterial = this.CheckShaderAndCreateMaterial(this.vignetteShader, this.vignetteMaterial);
        this.separableBlurMaterial = this.CheckShaderAndCreateMaterial(this.separableBlurShader, this.separableBlurMaterial);
        this.addBrightStuffBlendOneOneMaterial = this.CheckShaderAndCreateMaterial(this.addBrightStuffOneOneShader, this.addBrightStuffBlendOneOneMaterial);
        this.hollywoodFlaresMaterial = this.CheckShaderAndCreateMaterial(this.hollywoodFlaresShader, this.hollywoodFlaresMaterial);
        this.brightPassFilterMaterial = this.CheckShaderAndCreateMaterial(this.brightPassFilterShader, this.brightPassFilterMaterial);
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
        // screen blend is not supported when HDR is enabled (will cap values)
        this.doHdr = false;
        if (this.hdr == HDRBloomMode.Auto)
        {
            this.doHdr = (source.format == RenderTextureFormat.ARGBHalf) && this.GetComponent<Camera>().allowHDR;
        }
        else
        {
            this.doHdr = this.hdr == HDRBloomMode.On;
        }
        this.doHdr = this.doHdr && this.supportHDRTextures;
        BloomScreenBlendMode realBlendMode = this.screenBlendMode;
        if (this.doHdr)
        {
            realBlendMode = BloomScreenBlendMode.Add;
        }
        RenderTextureFormat rtFormat = this.doHdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.Default;
        RenderTexture halfRezColor = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0, rtFormat);
        RenderTexture quarterRezColor = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, rtFormat);
        RenderTexture secondQuarterRezColor = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, rtFormat);
        RenderTexture thirdQuarterRezColor = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0, rtFormat);
        float widthOverHeight = (1f * source.width) / (1f * source.height);
        float oneOverBaseSize = 1f / 512f;
        // downsample
        Graphics.Blit(source, halfRezColor, this.screenBlend, 2); // <- 2 is stable downsample
        Graphics.Blit(halfRezColor, quarterRezColor, this.screenBlend, 2); // <- 2 is stable downsample
        RenderTexture.ReleaseTemporary(halfRezColor);
        // cut colors (threshholding)
        this.BrightFilter(this.bloomThreshhold, this.useSrcAlphaAsMask, quarterRezColor, secondQuarterRezColor);
        quarterRezColor.DiscardContents();
        // blurring
        if (this.bloomBlurIterations < 1)
        {
            this.bloomBlurIterations = 1;
        }
        int iter = 0;
        while (iter < this.bloomBlurIterations)
        {
            float spreadForPass = (1f + (iter * 0.5f)) * this.sepBlurSpread;
            this.separableBlurMaterial.SetVector("offsets", new Vector4(0f, spreadForPass * oneOverBaseSize, 0f, 0f));
            RenderTexture src = iter == 0 ? secondQuarterRezColor : quarterRezColor;
            Graphics.Blit(src, thirdQuarterRezColor, this.separableBlurMaterial);
            src.DiscardContents();
            this.separableBlurMaterial.SetVector("offsets", new Vector4((spreadForPass / widthOverHeight) * oneOverBaseSize, 0f, 0f, 0f));
            Graphics.Blit(thirdQuarterRezColor, quarterRezColor, this.separableBlurMaterial);
            thirdQuarterRezColor.DiscardContents();
            iter++;
        }
        // lens flares: ghosting, anamorphic or a combination
        if (this.lensflares)
        {
            if (this.lensflareMode == (LensflareStyle34) 0)
            {
                this.BrightFilter(this.lensflareThreshhold, 0f, quarterRezColor, thirdQuarterRezColor);
                quarterRezColor.DiscardContents();
                // smooth a little, this needs to be resolution dependent
                /*
				separableBlurMaterial.SetVector ("offsets", Vector4 (0.0f, (2.0f) / (1.0f * quarterRezColor.height), 0.0f, 0.0f));
				Graphics.Blit (thirdQuarterRezColor, secondQuarterRezColor, separableBlurMaterial);
				separableBlurMaterial.SetVector ("offsets", Vector4 ((2.0f) / (1.0f * quarterRezColor.width), 0.0f, 0.0f, 0.0f));
				Graphics.Blit (secondQuarterRezColor, thirdQuarterRezColor, separableBlurMaterial);
				*/
                // no ugly edges!
                this.Vignette(0.975f, thirdQuarterRezColor, secondQuarterRezColor);
                thirdQuarterRezColor.DiscardContents();
                this.BlendFlares(secondQuarterRezColor, quarterRezColor);
                secondQuarterRezColor.DiscardContents();
            }
            else
            {
                // (b) hollywood/anamorphic flares?
                // thirdQuarter has the brightcut unblurred colors
                // quarterRezColor is the blurred, brightcut buffer that will end up as bloom
                this.hollywoodFlaresMaterial.SetVector("_Threshhold", new Vector4(this.lensflareThreshhold, 1f / (1f - this.lensflareThreshhold), 0f, 0f));
                this.hollywoodFlaresMaterial.SetVector("tintColor", (new Vector4(this.flareColorA.r, this.flareColorA.g, this.flareColorA.b, this.flareColorA.a) * this.flareColorA.a) * this.lensflareIntensity);
                Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, this.hollywoodFlaresMaterial, 2);
                thirdQuarterRezColor.DiscardContents();
                Graphics.Blit(secondQuarterRezColor, thirdQuarterRezColor, this.hollywoodFlaresMaterial, 3);
                secondQuarterRezColor.DiscardContents();
                this.hollywoodFlaresMaterial.SetVector("offsets", new Vector4(((this.sepBlurSpread * 1f) / widthOverHeight) * oneOverBaseSize, 0f, 0f, 0f));
                this.hollywoodFlaresMaterial.SetFloat("stretchWidth", this.hollyStretchWidth);
                Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, this.hollywoodFlaresMaterial, 1);
                thirdQuarterRezColor.DiscardContents();
                this.hollywoodFlaresMaterial.SetFloat("stretchWidth", this.hollyStretchWidth * 2f);
                Graphics.Blit(secondQuarterRezColor, thirdQuarterRezColor, this.hollywoodFlaresMaterial, 1);
                secondQuarterRezColor.DiscardContents();
                this.hollywoodFlaresMaterial.SetFloat("stretchWidth", this.hollyStretchWidth * 4f);
                Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, this.hollywoodFlaresMaterial, 1);
                thirdQuarterRezColor.DiscardContents();
                if (this.lensflareMode == (LensflareStyle34) 1)
                {
                    int itera = 0;
                    while (itera < this.hollywoodFlareBlurIterations)
                    {
                        this.separableBlurMaterial.SetVector("offsets", new Vector4(((this.hollyStretchWidth * 2f) / widthOverHeight) * oneOverBaseSize, 0f, 0f, 0f));
                        Graphics.Blit(secondQuarterRezColor, thirdQuarterRezColor, this.separableBlurMaterial);
                        secondQuarterRezColor.DiscardContents();
                        this.separableBlurMaterial.SetVector("offsets", new Vector4(((this.hollyStretchWidth * 2f) / widthOverHeight) * oneOverBaseSize, 0f, 0f, 0f));
                        Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, this.separableBlurMaterial);
                        thirdQuarterRezColor.DiscardContents();
                        itera++;
                    }
                    this.AddTo(1f, secondQuarterRezColor, quarterRezColor);
                    secondQuarterRezColor.DiscardContents();
                }
                else
                {
                    // (c) combined
                    int ix = 0;
                    while (ix < this.hollywoodFlareBlurIterations)
                    {
                        this.separableBlurMaterial.SetVector("offsets", new Vector4(((this.hollyStretchWidth * 2f) / widthOverHeight) * oneOverBaseSize, 0f, 0f, 0f));
                        Graphics.Blit(secondQuarterRezColor, thirdQuarterRezColor, this.separableBlurMaterial);
                        secondQuarterRezColor.DiscardContents();
                        this.separableBlurMaterial.SetVector("offsets", new Vector4(((this.hollyStretchWidth * 2f) / widthOverHeight) * oneOverBaseSize, 0f, 0f, 0f));
                        Graphics.Blit(thirdQuarterRezColor, secondQuarterRezColor, this.separableBlurMaterial);
                        thirdQuarterRezColor.DiscardContents();
                        ix++;
                    }
                    this.Vignette(1f, secondQuarterRezColor, thirdQuarterRezColor);
                    secondQuarterRezColor.DiscardContents();
                    this.BlendFlares(thirdQuarterRezColor, secondQuarterRezColor);
                    thirdQuarterRezColor.DiscardContents();
                    this.AddTo(1f, secondQuarterRezColor, quarterRezColor);
                    secondQuarterRezColor.DiscardContents();
                }
            }
        }
        // screen blend bloom results to color buffer
        this.screenBlend.SetFloat("_Intensity", this.bloomIntensity);
        this.screenBlend.SetTexture("_ColorBuffer", source);
        Graphics.Blit(quarterRezColor, destination, this.screenBlend, (int) realBlendMode);
        RenderTexture.ReleaseTemporary(quarterRezColor);
        RenderTexture.ReleaseTemporary(secondQuarterRezColor);
        RenderTexture.ReleaseTemporary(thirdQuarterRezColor);
    }

    private void AddTo(float intensity_, RenderTexture from, RenderTexture to)
    {
        this.addBrightStuffBlendOneOneMaterial.SetFloat("_Intensity", intensity_);
        Graphics.Blit(from, to, this.addBrightStuffBlendOneOneMaterial);
    }

    private void BlendFlares(RenderTexture from, RenderTexture to)
    {
        this.lensFlareMaterial.SetVector("colorA", new Vector4(this.flareColorA.r, this.flareColorA.g, this.flareColorA.b, this.flareColorA.a) * this.lensflareIntensity);
        this.lensFlareMaterial.SetVector("colorB", new Vector4(this.flareColorB.r, this.flareColorB.g, this.flareColorB.b, this.flareColorB.a) * this.lensflareIntensity);
        this.lensFlareMaterial.SetVector("colorC", new Vector4(this.flareColorC.r, this.flareColorC.g, this.flareColorC.b, this.flareColorC.a) * this.lensflareIntensity);
        this.lensFlareMaterial.SetVector("colorD", new Vector4(this.flareColorD.r, this.flareColorD.g, this.flareColorD.b, this.flareColorD.a) * this.lensflareIntensity);
        Graphics.Blit(from, to, this.lensFlareMaterial);
    }

    private void BrightFilter(float thresh, float useAlphaAsMask, RenderTexture from, RenderTexture to)
    {
        if (this.doHdr)
        {
            this.brightPassFilterMaterial.SetVector("threshhold", new Vector4(thresh, 1f, 0f, 0f));
        }
        else
        {
            this.brightPassFilterMaterial.SetVector("threshhold", new Vector4(thresh, 1f / (1f - thresh), 0f, 0f));
        }
        this.brightPassFilterMaterial.SetFloat("useSrcAlphaAsMask", useAlphaAsMask);
        Graphics.Blit(from, to, this.brightPassFilterMaterial);
    }

    private void Vignette(float amount, RenderTexture from, RenderTexture to)
    {
        if (this.lensFlareVignetteMask)
        {
            this.screenBlend.SetTexture("_ColorBuffer", this.lensFlareVignetteMask);
            Graphics.Blit(from, to, this.screenBlend, 3);
        }
        else
        {
            this.vignetteMaterial.SetFloat("vignetteIntensity", amount);
            Graphics.Blit(from, to, this.vignetteMaterial);
        }
    }

    public BloomAndLensFlares()
    {
        this.screenBlendMode = BloomScreenBlendMode.Add;
        this.hdr = HDRBloomMode.Auto;
        this.sepBlurSpread = 1.5f;
        this.useSrcAlphaAsMask = 0.5f;
        this.bloomIntensity = 1f;
        this.bloomThreshhold = 0.5f;
        this.bloomBlurIterations = 2;
        this.hollywoodFlareBlurIterations = 2;
        this.lensflareMode = (LensflareStyle34) 1;
        this.hollyStretchWidth = 3.5f;
        this.lensflareIntensity = 1f;
        this.lensflareThreshhold = 0.3f;
        this.flareColorA = new Color(0.4f, 0.4f, 0.8f, 0.75f);
        this.flareColorB = new Color(0.4f, 0.8f, 0.8f, 0.75f);
        this.flareColorC = new Color(0.8f, 0.4f, 0.8f, 0.75f);
        this.flareColorD = new Color(0.8f, 0.4f, 0f, 0.75f);
        this.blurWidth = 1f;
    }

}