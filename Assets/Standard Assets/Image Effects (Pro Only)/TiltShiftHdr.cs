using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Camera/Tilt Shift (Lens Blur)")]
public partial class TiltShiftHdr : PostEffectsBase
{
    public enum TiltShiftMode
    {
        TiltShiftMode = 0,
        IrisMode = 1
    }


    public enum TiltShiftQuality
    {
        Preview = 0,
        Normal = 1,
        High = 2
    }


    public TiltShiftHdr.TiltShiftMode mode;
    public TiltShiftHdr.TiltShiftQuality quality;
    [UnityEngine.Range(0f, 15f)]
    public float blurArea;
    [UnityEngine.Range(0f, 25f)]
    public float maxBlurSize;
    [UnityEngine.Range(0, 1)]
    public int downsample;
    public Shader tiltShiftShader;
    private Material tiltShiftMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(true);
        this.tiltShiftMaterial = this.CheckShaderAndCreateMaterial(this.tiltShiftShader, this.tiltShiftMaterial);
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
        this.tiltShiftMaterial.SetFloat("_BlurSize", this.maxBlurSize < 0f ? 0f : this.maxBlurSize);
        this.tiltShiftMaterial.SetFloat("_BlurArea", this.blurArea);
        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = destination;
        if (this.downsample != 0)
        {
            rt = RenderTexture.GetTemporary(source.width >> this.downsample, source.height >> this.downsample, 0, source.format);
            rt.filterMode = FilterMode.Bilinear;
        }
        int basePassNr = (int) this.quality;
        basePassNr = basePassNr * 2;
        Graphics.Blit(source, rt, this.tiltShiftMaterial, this.mode == TiltShiftMode.TiltShiftMode ? basePassNr : basePassNr + 1);
        if (this.downsample != 0)
        {
            this.tiltShiftMaterial.SetTexture("_Blurred", rt);
            Graphics.Blit(source, destination, this.tiltShiftMaterial, 6);
        }
        if (rt != destination)
        {
            RenderTexture.ReleaseTemporary(rt);
        }
    }

    public TiltShiftHdr()
    {
        this.mode = TiltShiftMode.TiltShiftMode;
        this.quality = TiltShiftQuality.Normal;
        this.blurArea = 1f;
        this.maxBlurSize = 5f;
    }

}