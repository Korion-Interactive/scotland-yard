using UnityEngine;
using System.Collections;

public enum AAMode
{
    FXAA2 = 0,
    FXAA3Console = 1,
    FXAA1PresetA = 2,
    FXAA1PresetB = 3,
    NFAA = 4,
    SSAA = 5,
    DLAA = 6
}

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Other/Antialiasing")]
public partial class AntialiasingAsPostEffect : PostEffectsBase
{
    public AAMode mode;
    public bool showGeneratedNormals;
    public float offsetScale;
    public float blurRadius;
    public float edgeThresholdMin;
    public float edgeThreshold;
    public float edgeSharpness;
    public bool dlaaSharp;
    public Shader ssaaShader;
    private Material ssaa;
    public Shader dlaaShader;
    private Material dlaa;
    public Shader nfaaShader;
    private Material nfaa;
    public Shader shaderFXAAPreset2;
    private Material materialFXAAPreset2;
    public Shader shaderFXAAPreset3;
    private Material materialFXAAPreset3;
    public Shader shaderFXAAII;
    private Material materialFXAAII;
    public Shader shaderFXAAIII;
    private Material materialFXAAIII;
    public virtual Material CurrentAAMaterial()
    {
        Material returnValue = null;
        switch (this.mode)
        {
            case AAMode.FXAA3Console:
                returnValue = this.materialFXAAIII;
                break;
            case AAMode.FXAA2:
                returnValue = this.materialFXAAII;
                break;
            case AAMode.FXAA1PresetA:
                returnValue = this.materialFXAAPreset2;
                break;
            case AAMode.FXAA1PresetB:
                returnValue = this.materialFXAAPreset3;
                break;
            case AAMode.NFAA:
                returnValue = this.nfaa;
                break;
            case AAMode.SSAA:
                returnValue = this.ssaa;
                break;
            case AAMode.DLAA:
                returnValue = this.dlaa;
                break;
            default:
                returnValue = null;
                break;
        }
        return returnValue;
    }

    public override bool CheckResources()
    {
        this.CheckSupport(false);
        this.materialFXAAPreset2 = this.CreateMaterial(this.shaderFXAAPreset2, this.materialFXAAPreset2);
        this.materialFXAAPreset3 = this.CreateMaterial(this.shaderFXAAPreset3, this.materialFXAAPreset3);
        this.materialFXAAII = this.CreateMaterial(this.shaderFXAAII, this.materialFXAAII);
        this.materialFXAAIII = this.CreateMaterial(this.shaderFXAAIII, this.materialFXAAIII);
        this.nfaa = this.CreateMaterial(this.nfaaShader, this.nfaa);
        this.ssaa = this.CreateMaterial(this.ssaaShader, this.ssaa);
        this.dlaa = this.CreateMaterial(this.dlaaShader, this.dlaa);
        if (!this.ssaaShader.isSupported)
        {
            this.NotSupported();
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
        // .............................................................................
        // FXAA antialiasing modes .....................................................
        if ((this.mode == AAMode.FXAA3Console) && (this.materialFXAAIII != null))
        {
            this.materialFXAAIII.SetFloat("_EdgeThresholdMin", this.edgeThresholdMin);
            this.materialFXAAIII.SetFloat("_EdgeThreshold", this.edgeThreshold);
            this.materialFXAAIII.SetFloat("_EdgeSharpness", this.edgeSharpness);
            Graphics.Blit(source, destination, this.materialFXAAIII);
        }
        else
        {
            if ((this.mode == AAMode.FXAA1PresetB) && (this.materialFXAAPreset3 != null))
            {
                Graphics.Blit(source, destination, this.materialFXAAPreset3);
            }
            else
            {
                if ((this.mode == AAMode.FXAA1PresetA) && (this.materialFXAAPreset2 != null))
                {
                    source.anisoLevel = 4;
                    Graphics.Blit(source, destination, this.materialFXAAPreset2);
                    source.anisoLevel = 0;
                }
                else
                {
                    if ((this.mode == AAMode.FXAA2) && (this.materialFXAAII != null))
                    {
                        Graphics.Blit(source, destination, this.materialFXAAII);
                    }
                    else
                    {
                        if ((this.mode == AAMode.SSAA) && (this.ssaa != null))
                        {
                            // .............................................................................
                            // SSAA antialiasing ...........................................................
                            Graphics.Blit(source, destination, this.ssaa);
                        }
                        else
                        {
                            if ((this.mode == AAMode.DLAA) && (this.dlaa != null))
                            {
                                // .............................................................................
                                // DLAA antialiasing ...........................................................
                                source.anisoLevel = 0;
                                RenderTexture interim = RenderTexture.GetTemporary(source.width, source.height);
                                Graphics.Blit(source, interim, this.dlaa, 0);
                                Graphics.Blit(interim, destination, this.dlaa, this.dlaaSharp ? 2 : 1);
                                RenderTexture.ReleaseTemporary(interim);
                            }
                            else
                            {
                                if ((this.mode == AAMode.NFAA) && (this.nfaa != null))
                                {
                                    // .............................................................................
                                    // nfaa antialiasing ..............................................
                                    source.anisoLevel = 0;
                                    this.nfaa.SetFloat("_OffsetScale", this.offsetScale);
                                    this.nfaa.SetFloat("_BlurRadius", this.blurRadius);
                                    Graphics.Blit(source, destination, this.nfaa, this.showGeneratedNormals ? 1 : 0);
                                }
                                else
                                {
                                    // none of the AA is supported, fallback to a simple blit
                                    Graphics.Blit(source, destination);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public AntialiasingAsPostEffect()
    {
        this.mode = AAMode.FXAA3Console;
        this.offsetScale = 0.2f;
        this.blurRadius = 18f;
        this.edgeThresholdMin = 0.05f;
        this.edgeThreshold = 0.2f;
        this.edgeSharpness = 4f;
    }

}