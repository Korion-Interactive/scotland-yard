using UnityEngine;
using System.Collections;

public enum ColorCorrectionMode
{
    Simple = 0,
    Advanced = 1
}

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.AddComponentMenu("Image Effects/Color Adjustments/Color Correction (Curves, Saturation)")]
public partial class ColorCorrectionCurves : PostEffectsBase
{
    public AnimationCurve redChannel;
    public AnimationCurve greenChannel;
    public AnimationCurve blueChannel;
    public bool useDepthCorrection;
    public AnimationCurve zCurve;
    public AnimationCurve depthRedChannel;
    public AnimationCurve depthGreenChannel;
    public AnimationCurve depthBlueChannel;
    private Material ccMaterial;
    private Material ccDepthMaterial;
    private Material selectiveCcMaterial;
    private Texture2D rgbChannelTex;
    private Texture2D rgbDepthChannelTex;
    private Texture2D zCurveTex;
    public float saturation;
    public bool selectiveCc;
    public Color selectiveFromColor;
    public Color selectiveToColor;
    public ColorCorrectionMode mode;
    public bool updateTextures;
    public Shader colorCorrectionCurvesShader;
    public Shader simpleColorCorrectionCurvesShader;
    public Shader colorCorrectionSelectiveShader;
    private bool updateTexturesOnStartup;
    public override void Start()
    {
        base.Start();
        this.updateTexturesOnStartup = true;
    }

    public virtual void Awake()
    {
    }

    public override bool CheckResources()
    {
        this.CheckSupport(this.mode == ColorCorrectionMode.Advanced);
        this.ccMaterial = this.CheckShaderAndCreateMaterial(this.simpleColorCorrectionCurvesShader, this.ccMaterial);
        this.ccDepthMaterial = this.CheckShaderAndCreateMaterial(this.colorCorrectionCurvesShader, this.ccDepthMaterial);
        this.selectiveCcMaterial = this.CheckShaderAndCreateMaterial(this.colorCorrectionSelectiveShader, this.selectiveCcMaterial);
        if (!this.rgbChannelTex)
        {
            this.rgbChannelTex = new Texture2D(256, 4, TextureFormat.ARGB32, false, true);
        }
        if (!this.rgbDepthChannelTex)
        {
            this.rgbDepthChannelTex = new Texture2D(256, 4, TextureFormat.ARGB32, false, true);
        }
        if (!this.zCurveTex)
        {
            this.zCurveTex = new Texture2D(256, 1, TextureFormat.ARGB32, false, true);
        }
        this.rgbChannelTex.hideFlags = HideFlags.DontSave;
        this.rgbDepthChannelTex.hideFlags = HideFlags.DontSave;
        this.zCurveTex.hideFlags = HideFlags.DontSave;
        this.rgbChannelTex.wrapMode = TextureWrapMode.Clamp;
        this.rgbDepthChannelTex.wrapMode = TextureWrapMode.Clamp;
        this.zCurveTex.wrapMode = TextureWrapMode.Clamp;
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual void UpdateParameters()
    {
        this.CheckResources(); // textures might not be created if we're tweaking UI while disabled
        if (((this.redChannel != null) && (this.greenChannel != null)) && (this.blueChannel != null))
        {
            float i = 0f;
            while (i <= 1f)
            {
                float rCh = Mathf.Clamp(this.redChannel.Evaluate(i), 0f, 1f);
                float gCh = Mathf.Clamp(this.greenChannel.Evaluate(i), 0f, 1f);
                float bCh = Mathf.Clamp(this.blueChannel.Evaluate(i), 0f, 1f);
                this.rgbChannelTex.SetPixel((int) Mathf.Floor(i * 255f), 0, new Color(rCh, rCh, rCh));
                this.rgbChannelTex.SetPixel((int) Mathf.Floor(i * 255f), 1, new Color(gCh, gCh, gCh));
                this.rgbChannelTex.SetPixel((int) Mathf.Floor(i * 255f), 2, new Color(bCh, bCh, bCh));
                float zC = Mathf.Clamp(this.zCurve.Evaluate(i), 0f, 1f);
                this.zCurveTex.SetPixel((int) Mathf.Floor(i * 255f), 0, new Color(zC, zC, zC));
                rCh = Mathf.Clamp(this.depthRedChannel.Evaluate(i), 0f, 1f);
                gCh = Mathf.Clamp(this.depthGreenChannel.Evaluate(i), 0f, 1f);
                bCh = Mathf.Clamp(this.depthBlueChannel.Evaluate(i), 0f, 1f);
                this.rgbDepthChannelTex.SetPixel((int) Mathf.Floor(i * 255f), 0, new Color(rCh, rCh, rCh));
                this.rgbDepthChannelTex.SetPixel((int) Mathf.Floor(i * 255f), 1, new Color(gCh, gCh, gCh));
                this.rgbDepthChannelTex.SetPixel((int) Mathf.Floor(i * 255f), 2, new Color(bCh, bCh, bCh));
                i = i + (1f / 255f);
            }
            this.rgbChannelTex.Apply();
            this.rgbDepthChannelTex.Apply();
            this.zCurveTex.Apply();
        }
    }

    public virtual void UpdateTextures()
    {
        this.UpdateParameters();
    }

    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        if (this.updateTexturesOnStartup)
        {
            this.UpdateParameters();
            this.updateTexturesOnStartup = false;
        }
        if (this.useDepthCorrection)
        {
            this.GetComponent<Camera>().depthTextureMode = this.GetComponent<Camera>().depthTextureMode | DepthTextureMode.Depth;
        }
        RenderTexture renderTarget2Use = destination;
        if (this.selectiveCc)
        {
            renderTarget2Use = RenderTexture.GetTemporary(source.width, source.height);
        }
        if (this.useDepthCorrection)
        {
            this.ccDepthMaterial.SetTexture("_RgbTex", this.rgbChannelTex);
            this.ccDepthMaterial.SetTexture("_ZCurve", this.zCurveTex);
            this.ccDepthMaterial.SetTexture("_RgbDepthTex", this.rgbDepthChannelTex);
            this.ccDepthMaterial.SetFloat("_Saturation", this.saturation);
            Graphics.Blit(source, renderTarget2Use, this.ccDepthMaterial);
        }
        else
        {
            this.ccMaterial.SetTexture("_RgbTex", this.rgbChannelTex);
            this.ccMaterial.SetFloat("_Saturation", this.saturation);
            Graphics.Blit(source, renderTarget2Use, this.ccMaterial);
        }
        if (this.selectiveCc)
        {
            this.selectiveCcMaterial.SetColor("selColor", this.selectiveFromColor);
            this.selectiveCcMaterial.SetColor("targetColor", this.selectiveToColor);
            Graphics.Blit(renderTarget2Use, destination, this.selectiveCcMaterial);
            RenderTexture.ReleaseTemporary(renderTarget2Use);
        }
    }

    public ColorCorrectionCurves()
    {
        this.saturation = 1f;
        this.selectiveFromColor = Color.white;
        this.selectiveToColor = Color.white;
        this.updateTextures = true;
        this.updateTexturesOnStartup = true;
    }

}