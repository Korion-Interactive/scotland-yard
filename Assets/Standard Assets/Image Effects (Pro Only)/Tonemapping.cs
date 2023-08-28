using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Color Adjustments/Tonemapping")]
public partial class Tonemapping : PostEffectsBase
{
    public enum TonemapperType
    {
        SimpleReinhard = 0,
        UserCurve = 1,
        Hable = 2,
        Photographic = 3,
        OptimizedHejiDawson = 4,
        AdaptiveReinhard = 5,
        AdaptiveReinhardAutoWhite = 6
    }


    public enum AdaptiveTexSize
    {
        Square16 = 16,
        Square32 = 32,
        Square64 = 64,
        Square128 = 128,
        Square256 = 256,
        Square512 = 512,
        Square1024 = 1024
    }


    public Tonemapping.TonemapperType type;
    public Tonemapping.AdaptiveTexSize adaptiveTextureSize;
    // CURVE parameter
    public AnimationCurve remapCurve;
    private Texture2D curveTex;
    // UNCHARTED parameter
    public float exposureAdjustment;
    // REINHARD parameter
    public float middleGrey;
    public float white;
    public float adaptionSpeed;
    // usual & internal stuff
    public Shader tonemapper;
    public bool validRenderTextureFormat;
    private Material tonemapMaterial;
    private RenderTexture rt;
    private RenderTextureFormat rtFormat;
    public override bool CheckResources()
    {
        this.CheckSupport(false, true);
        this.tonemapMaterial = this.CheckShaderAndCreateMaterial(this.tonemapper, this.tonemapMaterial);
        if (!this.curveTex && (this.type == TonemapperType.UserCurve))
        {
            this.curveTex = new Texture2D(256, 1, TextureFormat.ARGB32, false, true);
            this.curveTex.filterMode = FilterMode.Bilinear;
            this.curveTex.wrapMode = TextureWrapMode.Clamp;
            this.curveTex.hideFlags = HideFlags.DontSave;
        }
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual float UpdateCurve()
    {
        float range = 1f;
        if (this.remapCurve.keys.Length < 1)
        {
            this.remapCurve = new AnimationCurve(new Keyframe[] {new Keyframe(0, 0), new Keyframe(2, 1)});
        }
        if (this.remapCurve != null)
        {
            if (this.remapCurve.length != 0)
            {
                range = this.remapCurve[this.remapCurve.length - 1].time;
            }
            float i = 0f;
            while (i <= 1f)
            {
                float c = this.remapCurve.Evaluate((i * 1f) * range);
                this.curveTex.SetPixel((int) Mathf.Floor(i * 255f), 0, new Color(c, c, c));
                i = i + (1f / 255f);
            }
            this.curveTex.Apply();
        }
        return 1f / range;
    }

    public virtual void OnDisable()
    {
        if (this.rt)
        {
            UnityEngine.Object.DestroyImmediate(this.rt);
            this.rt = null;
        }
        if (this.tonemapMaterial)
        {
            UnityEngine.Object.DestroyImmediate(this.tonemapMaterial);
            this.tonemapMaterial = null;
        }
        if (this.curveTex)
        {
            UnityEngine.Object.DestroyImmediate(this.curveTex);
            this.curveTex = null;
        }
    }

    public virtual bool CreateInternalRenderTexture()
    {
        if (this.rt)
        {
            return false;
        }
        this.rtFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) ? RenderTextureFormat.RGHalf : RenderTextureFormat.ARGBHalf;
        this.rt = new RenderTexture(1, 1, 0, this.rtFormat);
        this.rt.hideFlags = HideFlags.DontSave;
        return true;
    }

    // attribute indicates that the image filter chain will continue in LDR
    [UnityEngine.ImageEffectTransformsToLDR]
    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        this.validRenderTextureFormat = true;
        if (source.format != RenderTextureFormat.ARGBHalf)
        {
            this.validRenderTextureFormat = false;
        }
        // clamp some values to not go out of a valid range
        this.exposureAdjustment = this.exposureAdjustment < 0.001f ? 0.001f : this.exposureAdjustment;
        // SimpleReinhard tonemappers (local, non adaptive)
        if (this.type == TonemapperType.UserCurve)
        {
            float rangeScale = this.UpdateCurve();
            this.tonemapMaterial.SetFloat("_RangeScale", rangeScale);
            this.tonemapMaterial.SetTexture("_Curve", this.curveTex);
            Graphics.Blit(source, destination, this.tonemapMaterial, 4);
            return;
        }
        if (this.type == TonemapperType.SimpleReinhard)
        {
            this.tonemapMaterial.SetFloat("_ExposureAdjustment", this.exposureAdjustment);
            Graphics.Blit(source, destination, this.tonemapMaterial, 6);
            return;
        }
        if (this.type == TonemapperType.Hable)
        {
            this.tonemapMaterial.SetFloat("_ExposureAdjustment", this.exposureAdjustment);
            Graphics.Blit(source, destination, this.tonemapMaterial, 5);
            return;
        }
        if (this.type == TonemapperType.Photographic)
        {
            this.tonemapMaterial.SetFloat("_ExposureAdjustment", this.exposureAdjustment);
            Graphics.Blit(source, destination, this.tonemapMaterial, 8);
            return;
        }
        if (this.type == TonemapperType.OptimizedHejiDawson)
        {
            this.tonemapMaterial.SetFloat("_ExposureAdjustment", 0.5f * this.exposureAdjustment);
            Graphics.Blit(source, destination, this.tonemapMaterial, 7);
            return;
        }
        // still here? 
        // =>  adaptive tone mapping:
        // builds an average log luminance, tonemaps according to 
        // middle grey and white values (user controlled)
        // AdaptiveReinhardAutoWhite will calculate white value automagically
        bool freshlyBrewedInternalRt = this.CreateInternalRenderTexture(); // this retrieves rtFormat, so should happen before rt allocations
        RenderTexture rtSquared = RenderTexture.GetTemporary((int) this.adaptiveTextureSize, (int) this.adaptiveTextureSize, 0, this.rtFormat);
        Graphics.Blit(source, rtSquared);
        int downsample = (int) Mathf.Log(rtSquared.width * 1f, 2);
        int div = 2;
        RenderTexture[] rts = new RenderTexture[downsample];
        int i = 0;
        while (i < downsample)
        {
            rts[i] = RenderTexture.GetTemporary(rtSquared.width / div, rtSquared.width / div, 0, this.rtFormat);
            div = div * 2;
            i++;
        }
        float ar = (source.width * 1f) / (source.height * 1f);
        // downsample pyramid
        RenderTexture lumRt = rts[downsample - 1];
        Graphics.Blit(rtSquared, rts[0], this.tonemapMaterial, 1);
        if (this.type == TonemapperType.AdaptiveReinhardAutoWhite)
        {
            i = 0;
            while (i < (downsample - 1))
            {
                Graphics.Blit(rts[i], rts[i + 1], this.tonemapMaterial, 9);
                lumRt = rts[i + 1];
                i++;
            }
        }
        else
        {
            if (this.type == TonemapperType.AdaptiveReinhard)
            {
                i = 0;
                while (i < (downsample - 1))
                {
                    Graphics.Blit(rts[i], rts[i + 1]);
                    lumRt = rts[i + 1];
                    i++;
                }
            }
        }
        // we have the needed values, let's apply adaptive tonemapping
        this.adaptionSpeed = this.adaptionSpeed < 0.001f ? 0.001f : this.adaptionSpeed;
        this.tonemapMaterial.SetFloat("_AdaptionSpeed", this.adaptionSpeed);
        this.rt.MarkRestoreExpected(); // keeping luminance values between frames, RT restore expected
        if (Application.isPlaying && !freshlyBrewedInternalRt)
        {
            Graphics.Blit(lumRt, this.rt, this.tonemapMaterial, 2);
        }
        else
        {
            Graphics.Blit(lumRt, this.rt, this.tonemapMaterial, 3);
        }
        this.middleGrey = this.middleGrey < 0.001f ? 0.001f : this.middleGrey;
        this.tonemapMaterial.SetVector("_HdrParams", new Vector4(this.middleGrey, this.middleGrey, this.middleGrey, this.white * this.white));
        this.tonemapMaterial.SetTexture("_SmallTex", this.rt);
        if (this.type == TonemapperType.AdaptiveReinhard)
        {
            Graphics.Blit(source, destination, this.tonemapMaterial, 0);
        }
        else
        {
            if (this.type == TonemapperType.AdaptiveReinhardAutoWhite)
            {
                Graphics.Blit(source, destination, this.tonemapMaterial, 10);
            }
            else
            {
                Debug.LogError("No valid adaptive tonemapper type found!");
                Graphics.Blit(source, destination); // at least we get the TransformToLDR effect
            }
        }
        // cleanup for adaptive
        i = 0;
        while (i < downsample)
        {
            RenderTexture.ReleaseTemporary(rts[i]);
            i++;
        }
        RenderTexture.ReleaseTemporary(rtSquared);
    }

    public Tonemapping()
    {
        this.type = TonemapperType.Photographic;
        this.adaptiveTextureSize = AdaptiveTexSize.Square256;
        this.exposureAdjustment = 1.5f;
        this.middleGrey = 0.4f;
        this.white = 2f;
        this.adaptionSpeed = 1.5f;
        this.validRenderTextureFormat = true;
        this.rtFormat = RenderTextureFormat.ARGBHalf;
    }

}