using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Other/Screen Overlay")]
public partial class ScreenOverlay : PostEffectsBase
{
    public enum OverlayBlendMode
    {
        Additive = 0,
        ScreenBlend = 1,
        Multiply = 2,
        Overlay = 3,
        AlphaBlend = 4
    }


    public ScreenOverlay.OverlayBlendMode blendMode;
    public float intensity;
    public Texture2D texture;
    public Shader overlayShader;
    private Material overlayMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(false);
        this.overlayMaterial = this.CheckShaderAndCreateMaterial(this.overlayShader, this.overlayMaterial);
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
        Vector4 UV_Transform = new Vector4(1, 0, 0, 1);
        this.overlayMaterial.SetVector("_UV_Transform", UV_Transform);
        this.overlayMaterial.SetFloat("_Intensity", this.intensity);
        this.overlayMaterial.SetTexture("_Overlay", this.texture);
        Graphics.Blit(source, destination, this.overlayMaterial, (int) this.blendMode);
    }

    public ScreenOverlay()
    {
        this.blendMode = OverlayBlendMode.Overlay;
        this.intensity = 1f;
    }

}