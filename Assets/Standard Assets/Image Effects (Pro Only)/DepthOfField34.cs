using UnityEngine;
using System.Collections;

public enum Dof34QualitySetting
{
    OnlyBackground = 1,
    BackgroundAndForeground = 2
}

public enum DofResolution
{
    High = 2,
    Medium = 3,
    Low = 4
}

public enum DofBlurriness
{
    Low = 1,
    High = 2,
    VeryHigh = 4
}

public enum BokehDestination
{
    Background = 1,
    Foreground = 2,
    BackgroundAndForeground = 3
}

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Camera/Depth of Field (3.4)")]
public partial class DepthOfField34 : PostEffectsBase
{
    private static int SMOOTH_DOWNSAMPLE_PASS;
    private static float BOKEH_EXTRA_BLUR;
    public Dof34QualitySetting quality;
    public DofResolution resolution;
    public bool simpleTweakMode;
    public float focalPoint;
    public float smoothness;
    public float focalZDistance;
    public float focalZStartCurve;
    public float focalZEndCurve;
    private float focalStartCurve;
    private float focalEndCurve;
    private float focalDistance01;
    public Transform objectFocus;
    public float focalSize;
    public DofBlurriness bluriness;
    public float maxBlurSpread;
    public float foregroundBlurExtrude;
    public Shader dofBlurShader;
    private Material dofBlurMaterial;
    public Shader dofShader;
    private Material dofMaterial;
    public bool visualize;
    public BokehDestination bokehDestination;
    private float widthOverHeight;
    private float oneOverBaseSize;
    public bool bokeh;
    public bool bokehSupport;
    public Shader bokehShader;
    public Texture2D bokehTexture;
    public float bokehScale;
    public float bokehIntensity;
    public float bokehThreshholdContrast;
    public float bokehThreshholdLuminance;
    public int bokehDownsample;
    private Material bokehMaterial;
    public virtual void CreateMaterials()
    {
        this.dofBlurMaterial = this.CheckShaderAndCreateMaterial(this.dofBlurShader, this.dofBlurMaterial);
        this.dofMaterial = this.CheckShaderAndCreateMaterial(this.dofShader, this.dofMaterial);
        this.bokehSupport = this.bokehShader.isSupported;
        if ((this.bokeh && this.bokehSupport) && this.bokehShader)
        {
            this.bokehMaterial = this.CheckShaderAndCreateMaterial(this.bokehShader, this.bokehMaterial);
        }
    }

    public override bool CheckResources()
    {
        this.CheckSupport(true);
        this.dofBlurMaterial = this.CheckShaderAndCreateMaterial(this.dofBlurShader, this.dofBlurMaterial);
        this.dofMaterial = this.CheckShaderAndCreateMaterial(this.dofShader, this.dofMaterial);
        this.bokehSupport = this.bokehShader.isSupported;
        if ((this.bokeh && this.bokehSupport) && this.bokehShader)
        {
            this.bokehMaterial = this.CheckShaderAndCreateMaterial(this.bokehShader, this.bokehMaterial);
        }
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual void OnDisable()
    {
        Quads.Cleanup();
    }

    public override void OnEnable()
    {
        this.GetComponent<Camera>().depthTextureMode = this.GetComponent<Camera>().depthTextureMode | DepthTextureMode.Depth;
    }

    public virtual float FocalDistance01(float worldDist)
    {
        return this.GetComponent<Camera>().WorldToViewportPoint(((worldDist - this.GetComponent<Camera>().nearClipPlane) * this.GetComponent<Camera>().transform.forward) + this.GetComponent<Camera>().transform.position).z / (this.GetComponent<Camera>().farClipPlane - this.GetComponent<Camera>().nearClipPlane);
    }

    public virtual int GetDividerBasedOnQuality()
    {
        int divider = 1;
        if (this.resolution == DofResolution.Medium)
        {
            divider = 2;
        }
        else
        {
            if (this.resolution == DofResolution.Low)
            {
                divider = 2;
            }
        }
        return divider;
    }

    public virtual int GetLowResolutionDividerBasedOnQuality(int baseDivider)
    {
        int lowTexDivider = baseDivider;
        if (this.resolution == DofResolution.High)
        {
            lowTexDivider = lowTexDivider * 2;
        }
        if (this.resolution == DofResolution.Low)
        {
            lowTexDivider = lowTexDivider * 2;
        }
        return lowTexDivider;
    }

    private RenderTexture foregroundTexture;
    private RenderTexture mediumRezWorkTexture;
    private RenderTexture finalDefocus;
    private RenderTexture lowRezWorkTexture;
    private RenderTexture bokehSource;
    private RenderTexture bokehSource2;
    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        if (this.smoothness < 0.1f)
        {
            this.smoothness = 0.1f;
        }
        // update needed focal & rt size parameter
        this.bokeh = this.bokeh && this.bokehSupport;
        float bokehBlurAmplifier = this.bokeh ? DepthOfField34.BOKEH_EXTRA_BLUR : 1f;
        bool blurForeground = this.quality > Dof34QualitySetting.OnlyBackground;
        float focal01Size = this.focalSize / (this.GetComponent<Camera>().farClipPlane - this.GetComponent<Camera>().nearClipPlane);
        if (this.simpleTweakMode)
        {
            this.focalDistance01 = this.objectFocus ? this.GetComponent<Camera>().WorldToViewportPoint(this.objectFocus.position).z / this.GetComponent<Camera>().farClipPlane : this.FocalDistance01(this.focalPoint);
            this.focalStartCurve = this.focalDistance01 * this.smoothness;
            this.focalEndCurve = this.focalStartCurve;
            blurForeground = blurForeground && (this.focalPoint > (this.GetComponent<Camera>().nearClipPlane + Mathf.Epsilon));
        }
        else
        {
            if (this.objectFocus)
            {
                Vector3 vpPoint = this.GetComponent<Camera>().WorldToViewportPoint(this.objectFocus.position);
                vpPoint.z = vpPoint.z / this.GetComponent<Camera>().farClipPlane;
                this.focalDistance01 = vpPoint.z;
            }
            else
            {
                this.focalDistance01 = this.FocalDistance01(this.focalZDistance);
            }
            this.focalStartCurve = this.focalZStartCurve;
            this.focalEndCurve = this.focalZEndCurve;
            blurForeground = blurForeground && (this.focalPoint > (this.GetComponent<Camera>().nearClipPlane + Mathf.Epsilon));
        }
        this.widthOverHeight = (1f * source.width) / (1f * source.height);
        this.oneOverBaseSize = 1f / 512f;
        this.dofMaterial.SetFloat("_ForegroundBlurExtrude", this.foregroundBlurExtrude);
        this.dofMaterial.SetVector("_CurveParams", new Vector4(this.simpleTweakMode ? 1f / this.focalStartCurve : this.focalStartCurve, this.simpleTweakMode ? 1f / this.focalEndCurve : this.focalEndCurve, focal01Size * 0.5f, this.focalDistance01));
        this.dofMaterial.SetVector("_InvRenderTargetSize", new Vector4(1f / (1f * source.width), 1f / (1f * source.height), 0f, 0f));
        int divider = this.GetDividerBasedOnQuality();
        int lowTexDivider = this.GetLowResolutionDividerBasedOnQuality(divider);
        this.AllocateTextures(blurForeground, source, divider, lowTexDivider);
        // WRITE COC to alpha channel		
        // source is only being bound to detect y texcoord flip
        Graphics.Blit(source, source, this.dofMaterial, 3);
        // better DOWNSAMPLE (could actually be weighted for higher quality)
        this.Downsample(source, this.mediumRezWorkTexture);
        // BLUR A LITTLE first, which has two purposes
        // 1.) reduce jitter, noise, aliasing
        // 2.) produce the little-blur buffer used in composition later     	     
        this.Blur(this.mediumRezWorkTexture, this.mediumRezWorkTexture, DofBlurriness.Low, 4, this.maxBlurSpread);
        if (this.bokeh && ((this.bokehDestination & BokehDestination.Background) != (BokehDestination) 0))
        {
            this.dofMaterial.SetVector("_Threshhold", new Vector4(this.bokehThreshholdContrast, this.bokehThreshholdLuminance, 0.95f, 0f));
            // add and mark the parts that should end up as bokeh shapes
            Graphics.Blit(this.mediumRezWorkTexture, this.bokehSource2, this.dofMaterial, 11);
            // remove those parts (maybe even a little tittle bittle more) from the regurlarly blurred buffer		
            //Graphics.Blit (mediumRezWorkTexture, lowRezWorkTexture, dofMaterial, 10);
            Graphics.Blit(this.mediumRezWorkTexture, this.lowRezWorkTexture);//, dofMaterial, 10);
            // maybe you want to reblur the small blur ... but not really needed.
            //Blur (mediumRezWorkTexture, mediumRezWorkTexture, DofBlurriness.Low, 4, maxBlurSpread);						
            // bigger BLUR
            this.Blur(this.lowRezWorkTexture, this.lowRezWorkTexture, this.bluriness, 0, this.maxBlurSpread * bokehBlurAmplifier);
        }
        else
        {
            // bigger BLUR
            this.Downsample(this.mediumRezWorkTexture, this.lowRezWorkTexture);
            this.Blur(this.lowRezWorkTexture, this.lowRezWorkTexture, this.bluriness, 0, this.maxBlurSpread);
        }
        this.dofBlurMaterial.SetTexture("_TapLow", this.lowRezWorkTexture);
        this.dofBlurMaterial.SetTexture("_TapMedium", this.mediumRezWorkTexture);
        Graphics.Blit(null, this.finalDefocus, this.dofBlurMaterial, 3);
        // we are only adding bokeh now if the background is the only part we have to deal with
        if (this.bokeh && ((this.bokehDestination & BokehDestination.Background) != (BokehDestination) 0))
        {
            this.AddBokeh(this.bokehSource2, this.bokehSource, this.finalDefocus);
        }
        this.dofMaterial.SetTexture("_TapLowBackground", this.finalDefocus);
        this.dofMaterial.SetTexture("_TapMedium", this.mediumRezWorkTexture); // needed for debugging/visualization
        // FINAL DEFOCUS (background)
        Graphics.Blit(source, blurForeground ? this.foregroundTexture : destination, this.dofMaterial, this.visualize ? 2 : 0);
        // FINAL DEFOCUS (foreground)
        if (blurForeground)
        {
            // WRITE COC to alpha channel			
            Graphics.Blit(this.foregroundTexture, source, this.dofMaterial, 5);
            // DOWNSAMPLE (unweighted)
            this.Downsample(source, this.mediumRezWorkTexture);
            // BLUR A LITTLE first, which has two purposes
            // 1.) reduce jitter, noise, aliasing
            // 2.) produce the little-blur buffer used in composition later   
            this.BlurFg(this.mediumRezWorkTexture, this.mediumRezWorkTexture, DofBlurriness.Low, 2, this.maxBlurSpread);
            if (this.bokeh && ((this.bokehDestination & BokehDestination.Foreground) != (BokehDestination) 0))
            {
                this.dofMaterial.SetVector("_Threshhold", new Vector4(this.bokehThreshholdContrast * 0.5f, this.bokehThreshholdLuminance, 0f, 0f));
                // add and mark the parts that should end up as bokeh shapes
                Graphics.Blit(this.mediumRezWorkTexture, this.bokehSource2, this.dofMaterial, 11);
                // remove the parts (maybe even a little tittle bittle more) that will end up in bokeh space			
                //Graphics.Blit (mediumRezWorkTexture, lowRezWorkTexture, dofMaterial, 10);
                Graphics.Blit(this.mediumRezWorkTexture, this.lowRezWorkTexture);//, dofMaterial, 10);
                // big BLUR		
                this.BlurFg(this.lowRezWorkTexture, this.lowRezWorkTexture, this.bluriness, 1, this.maxBlurSpread * bokehBlurAmplifier);
            }
            else
            {
                // big BLUR		
                this.BlurFg(this.mediumRezWorkTexture, this.lowRezWorkTexture, this.bluriness, 1, this.maxBlurSpread);
            }
            // simple upsample once						
            Graphics.Blit(this.lowRezWorkTexture, this.finalDefocus);
            this.dofMaterial.SetTexture("_TapLowForeground", this.finalDefocus);
            Graphics.Blit(source, destination, this.dofMaterial, this.visualize ? 1 : 4);
            if (this.bokeh && ((this.bokehDestination & BokehDestination.Foreground) != (BokehDestination) 0))
            {
                this.AddBokeh(this.bokehSource2, this.bokehSource, destination);
            }
        }
        this.ReleaseTextures();
    }

    public virtual void Blur(RenderTexture from, RenderTexture to, DofBlurriness iterations, int blurPass, float spread)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(to.width, to.height);
        if (iterations > (DofBlurriness) 1)
        {
            this.BlurHex(from, to, blurPass, spread, tmp);
            if (iterations > (DofBlurriness) 2)
            {
                this.dofBlurMaterial.SetVector("offsets", new Vector4(0f, spread * this.oneOverBaseSize, 0f, 0f));
                Graphics.Blit(to, tmp, this.dofBlurMaterial, blurPass);
                this.dofBlurMaterial.SetVector("offsets", new Vector4((spread / this.widthOverHeight) * this.oneOverBaseSize, 0f, 0f, 0f));
                Graphics.Blit(tmp, to, this.dofBlurMaterial, blurPass);
            }
        }
        else
        {
            this.dofBlurMaterial.SetVector("offsets", new Vector4(0f, spread * this.oneOverBaseSize, 0f, 0f));
            Graphics.Blit(from, tmp, this.dofBlurMaterial, blurPass);
            this.dofBlurMaterial.SetVector("offsets", new Vector4((spread / this.widthOverHeight) * this.oneOverBaseSize, 0f, 0f, 0f));
            Graphics.Blit(tmp, to, this.dofBlurMaterial, blurPass);
        }
        RenderTexture.ReleaseTemporary(tmp);
    }

    public virtual void BlurFg(RenderTexture from, RenderTexture to, DofBlurriness iterations, int blurPass, float spread)
    {
        // we want a nice, big coc, hence we need to tap once from this (higher resolution) texture
        this.dofBlurMaterial.SetTexture("_TapHigh", from);
        RenderTexture tmp = RenderTexture.GetTemporary(to.width, to.height);
        if (iterations > (DofBlurriness) 1)
        {
            this.BlurHex(from, to, blurPass, spread, tmp);
            if (iterations > (DofBlurriness) 2)
            {
                this.dofBlurMaterial.SetVector("offsets", new Vector4(0f, spread * this.oneOverBaseSize, 0f, 0f));
                Graphics.Blit(to, tmp, this.dofBlurMaterial, blurPass);
                this.dofBlurMaterial.SetVector("offsets", new Vector4((spread / this.widthOverHeight) * this.oneOverBaseSize, 0f, 0f, 0f));
                Graphics.Blit(tmp, to, this.dofBlurMaterial, blurPass);
            }
        }
        else
        {
            this.dofBlurMaterial.SetVector("offsets", new Vector4(0f, spread * this.oneOverBaseSize, 0f, 0f));
            Graphics.Blit(from, tmp, this.dofBlurMaterial, blurPass);
            this.dofBlurMaterial.SetVector("offsets", new Vector4((spread / this.widthOverHeight) * this.oneOverBaseSize, 0f, 0f, 0f));
            Graphics.Blit(tmp, to, this.dofBlurMaterial, blurPass);
        }
        RenderTexture.ReleaseTemporary(tmp);
    }

    public virtual void BlurHex(RenderTexture from, RenderTexture to, int blurPass, float spread, RenderTexture tmp)
    {
        this.dofBlurMaterial.SetVector("offsets", new Vector4(0f, spread * this.oneOverBaseSize, 0f, 0f));
        Graphics.Blit(from, tmp, this.dofBlurMaterial, blurPass);
        this.dofBlurMaterial.SetVector("offsets", new Vector4((spread / this.widthOverHeight) * this.oneOverBaseSize, 0f, 0f, 0f));
        Graphics.Blit(tmp, to, this.dofBlurMaterial, blurPass);
        this.dofBlurMaterial.SetVector("offsets", new Vector4((spread / this.widthOverHeight) * this.oneOverBaseSize, spread * this.oneOverBaseSize, 0f, 0f));
        Graphics.Blit(to, tmp, this.dofBlurMaterial, blurPass);
        this.dofBlurMaterial.SetVector("offsets", new Vector4((spread / this.widthOverHeight) * this.oneOverBaseSize, -spread * this.oneOverBaseSize, 0f, 0f));
        Graphics.Blit(tmp, to, this.dofBlurMaterial, blurPass);
    }

    public virtual void Downsample(RenderTexture from, RenderTexture to)
    {
        this.dofMaterial.SetVector("_InvRenderTargetSize", new Vector4(1f / (1f * to.width), 1f / (1f * to.height), 0f, 0f));
        Graphics.Blit(from, to, this.dofMaterial, DepthOfField34.SMOOTH_DOWNSAMPLE_PASS);
    }

    public virtual void AddBokeh(RenderTexture bokehInfo, RenderTexture tempTex, RenderTexture finalTarget)
    {
        if (this.bokehMaterial)
        {
            Mesh[] meshes = Quads.GetMeshes(tempTex.width, tempTex.height); // quads: exchanging more triangles with less overdraw			
            RenderTexture.active = tempTex;
            GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
            GL.PushMatrix();
            GL.LoadIdentity();
            // point filter mode is important, otherwise we get bokeh shape & size artefacts
            bokehInfo.filterMode = FilterMode.Point;
            float arW = (bokehInfo.width * 1f) / (bokehInfo.height * 1f);
            float sc = 2f / (1f * bokehInfo.width);
            sc = sc + (((this.bokehScale * this.maxBlurSpread) * DepthOfField34.BOKEH_EXTRA_BLUR) * this.oneOverBaseSize);
            this.bokehMaterial.SetTexture("_Source", bokehInfo);
            this.bokehMaterial.SetTexture("_MainTex", this.bokehTexture);
            this.bokehMaterial.SetVector("_ArScale", new Vector4(sc, sc * arW, 0.5f, 0.5f * arW));
            this.bokehMaterial.SetFloat("_Intensity", this.bokehIntensity);
            this.bokehMaterial.SetPass(0);
            foreach (Mesh m in meshes)
            {
                if (m)
                {
                    Graphics.DrawMeshNow(m, Matrix4x4.identity);
                }
            }
            GL.PopMatrix();
            Graphics.Blit(tempTex, finalTarget, this.dofMaterial, 8);
            // important to set back as we sample from this later on
            bokehInfo.filterMode = FilterMode.Bilinear;
        }
    }

    public virtual void ReleaseTextures()
    {
        if (this.foregroundTexture)
        {
            RenderTexture.ReleaseTemporary(this.foregroundTexture);
        }
        if (this.finalDefocus)
        {
            RenderTexture.ReleaseTemporary(this.finalDefocus);
        }
        if (this.mediumRezWorkTexture)
        {
            RenderTexture.ReleaseTemporary(this.mediumRezWorkTexture);
        }
        if (this.lowRezWorkTexture)
        {
            RenderTexture.ReleaseTemporary(this.lowRezWorkTexture);
        }
        if (this.bokehSource)
        {
            RenderTexture.ReleaseTemporary(this.bokehSource);
        }
        if (this.bokehSource2)
        {
            RenderTexture.ReleaseTemporary(this.bokehSource2);
        }
    }

    public virtual void AllocateTextures(bool blurForeground, RenderTexture source, int divider, int lowTexDivider)
    {
        this.foregroundTexture = null;
        if (blurForeground)
        {
            this.foregroundTexture = RenderTexture.GetTemporary(source.width, source.height, 0);
        }
        this.mediumRezWorkTexture = RenderTexture.GetTemporary(source.width / divider, source.height / divider, 0);
        this.finalDefocus = RenderTexture.GetTemporary(source.width / divider, source.height / divider, 0);
        this.lowRezWorkTexture = RenderTexture.GetTemporary(source.width / lowTexDivider, source.height / lowTexDivider, 0);
        this.bokehSource = null;
        this.bokehSource2 = null;
        if (this.bokeh)
        {
            this.bokehSource = RenderTexture.GetTemporary(source.width / (lowTexDivider * this.bokehDownsample), source.height / (lowTexDivider * this.bokehDownsample), 0, RenderTextureFormat.ARGBHalf);
            this.bokehSource2 = RenderTexture.GetTemporary(source.width / (lowTexDivider * this.bokehDownsample), source.height / (lowTexDivider * this.bokehDownsample), 0, RenderTextureFormat.ARGBHalf);
            this.bokehSource.filterMode = FilterMode.Bilinear;
            this.bokehSource2.filterMode = FilterMode.Bilinear;
            RenderTexture.active = this.bokehSource2;
            GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
        }
        // to make sure: always use bilinear filter setting
        source.filterMode = FilterMode.Bilinear;
        this.finalDefocus.filterMode = FilterMode.Bilinear;
        this.mediumRezWorkTexture.filterMode = FilterMode.Bilinear;
        this.lowRezWorkTexture.filterMode = FilterMode.Bilinear;
        if (this.foregroundTexture)
        {
            this.foregroundTexture.filterMode = FilterMode.Bilinear;
        }
    }

    public DepthOfField34()
    {
        this.quality = Dof34QualitySetting.OnlyBackground;
        this.resolution = DofResolution.Low;
        this.simpleTweakMode = true;
        this.focalPoint = 1f;
        this.smoothness = 0.5f;
        this.focalZStartCurve = 1f;
        this.focalZEndCurve = 1f;
        this.focalStartCurve = 2f;
        this.focalEndCurve = 2f;
        this.focalDistance01 = 0.1f;
        this.bluriness = DofBlurriness.High;
        this.maxBlurSpread = 1.75f;
        this.foregroundBlurExtrude = 1.15f;
        this.bokehDestination = BokehDestination.Background;
        this.widthOverHeight = 1.25f;
        this.oneOverBaseSize = 1f / 512f;
        this.bokehSupport = true;
        this.bokehScale = 2.4f;
        this.bokehIntensity = 0.15f;
        this.bokehThreshholdContrast = 0.1f;
        this.bokehThreshholdLuminance = 0.55f;
        this.bokehDownsample = 1;
    }

    static DepthOfField34()
    {
        DepthOfField34.SMOOTH_DOWNSAMPLE_PASS = 6;
        DepthOfField34.BOKEH_EXTRA_BLUR = 2f;
    }

}