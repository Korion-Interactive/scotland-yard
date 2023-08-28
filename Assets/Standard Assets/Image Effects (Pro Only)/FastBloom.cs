using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Bloom and Glow/Bloom (Optimized)")]
public partial class FastBloom : PostEffectsBase
{
    public enum Resolution
    {
        Low = 0,
        High = 1
    }


    public enum BlurType
    {
        Standard = 0,
        Sgx = 1
    }


    [UnityEngine.Range(0f, 1.5f)]
    public float threshhold;
    [UnityEngine.Range(0f, 2.5f)]
    public float intensity;
    [UnityEngine.Range(0.25f, 5.5f)]
    public float blurSize;
    public FastBloom.Resolution resolution;
    [UnityEngine.Range(1, 4)]
    public int blurIterations;
    public FastBloom.BlurType blurType;
    public Shader fastBloomShader;
    private Material fastBloomMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(false);
        this.fastBloomMaterial = this.CheckShaderAndCreateMaterial(this.fastBloomShader, this.fastBloomMaterial);
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual void OnDisable()
    {
        if (this.fastBloomMaterial)
        {
            UnityEngine.Object.DestroyImmediate(this.fastBloomMaterial);
        }
    }

    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        int divider = this.resolution == Resolution.Low ? 4 : 2;
        float widthMod = this.resolution == Resolution.Low ? 0.5f : 1f;
        this.fastBloomMaterial.SetVector("_Parameter", new Vector4(this.blurSize * widthMod, 0f, this.threshhold, this.intensity));
        source.filterMode = FilterMode.Bilinear;
        int rtW = source.width / divider;
        int rtH = source.height / divider;
        // downsample
        RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit(source, rt, this.fastBloomMaterial, 1);
        int passOffs = this.blurType == BlurType.Standard ? 0 : 2;
        int i = 0;
        while (i < this.blurIterations)
        {
            this.fastBloomMaterial.SetVector("_Parameter", new Vector4((this.blurSize * widthMod) + (i * 1f), 0f, this.threshhold, this.intensity));
            // vertical blur
            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, this.fastBloomMaterial, 2 + passOffs);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;
            // horizontal blur
            rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, this.fastBloomMaterial, 3 + passOffs);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;
            i++;
        }
        this.fastBloomMaterial.SetTexture("_Bloom", rt);
        Graphics.Blit(source, destination, this.fastBloomMaterial, 0);
        RenderTexture.ReleaseTemporary(rt);
    }

    public FastBloom()
    {
        this.threshhold = 0.25f;
        this.intensity = 0.75f;
        this.blurSize = 1f;
        this.resolution = Resolution.Low;
        this.blurIterations = 1;
        this.blurType = BlurType.Standard;
    }

}