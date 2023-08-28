using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Blur/Blur (Optimized)")]
public partial class Blur : PostEffectsBase
{
    [UnityEngine.Range(0, 2)]
    public int downsample;
    public enum BlurType
    {
        StandardGauss = 0,
        SgxGauss = 1
    }


    [UnityEngine.Range(0f, 10f)]
    public float blurSize;
    [UnityEngine.Range(1, 4)]
    public int blurIterations;
    public Blur.BlurType blurType;
    public Shader blurShader;
    private Material blurMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(false);
        this.blurMaterial = this.CheckShaderAndCreateMaterial(this.blurShader, this.blurMaterial);
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual void OnDisable()
    {
        if (this.blurMaterial)
        {
            UnityEngine.Object.DestroyImmediate(this.blurMaterial);
        }
    }

    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        float widthMod = 1f / (1f * (1 << this.downsample));
        this.blurMaterial.SetVector("_Parameter", new Vector4(this.blurSize * widthMod, -this.blurSize * widthMod, 0f, 0f));
        source.filterMode = FilterMode.Bilinear;
        int rtW = source.width >> this.downsample;
        int rtH = source.height >> this.downsample;
        // downsample
        RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
        rt.filterMode = FilterMode.Bilinear;
        Graphics.Blit(source, rt, this.blurMaterial, 0);
        int passOffs = this.blurType == BlurType.StandardGauss ? 0 : 2;
        int i = 0;
        while (i < this.blurIterations)
        {
            float iterationOffs = i * 1f;
            this.blurMaterial.SetVector("_Parameter", new Vector4((this.blurSize * widthMod) + iterationOffs, (-this.blurSize * widthMod) - iterationOffs, 0f, 0f));
            // vertical blur
            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, this.blurMaterial, 1 + passOffs);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;
            // horizontal blur
            rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt, rt2, this.blurMaterial, 2 + passOffs);
            RenderTexture.ReleaseTemporary(rt);
            rt = rt2;
            i++;
        }
        Graphics.Blit(rt, destination);
        RenderTexture.ReleaseTemporary(rt);
    }

    public Blur()
    {
        this.downsample = 1;
        this.blurSize = 3f;
        this.blurIterations = 2;
        this.blurType = BlurType.StandardGauss;
    }

}