using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Displacement/Fisheye")]
public partial class Fisheye : PostEffectsBase
{
    public float strengthX;
    public float strengthY;
    public Shader fishEyeShader;
    private Material fisheyeMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(false);
        this.fisheyeMaterial = this.CheckShaderAndCreateMaterial(this.fishEyeShader, this.fisheyeMaterial);
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
        float oneOverBaseSize = 80f / 512f; // to keep values more like in the old version of fisheye
        float ar = (source.width * 1f) / (source.height * 1f);
        this.fisheyeMaterial.SetVector("intensity", new Vector4((this.strengthX * ar) * oneOverBaseSize, this.strengthY * oneOverBaseSize, (this.strengthX * ar) * oneOverBaseSize, this.strengthY * oneOverBaseSize));
        Graphics.Blit(source, destination, this.fisheyeMaterial);
    }

    public Fisheye()
    {
        this.strengthX = 0.05f;
        this.strengthY = 0.05f;
    }

}