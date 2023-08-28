using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Rendering/Screen Space Ambient Obscurance")]
public partial class AmbientObscurance : PostEffectsBase
{
    [UnityEngine.Range(0, 3)]
    public float intensity;
    [UnityEngine.Range(0.1f, 3)]
    public float radius;
    [UnityEngine.Range(0, 3)]
    public int blurIterations;
    [UnityEngine.Range(0, 5)]
    public float blurFilterDistance;
    [UnityEngine.Range(0, 1)]
    public int downsample;
    public Texture2D rand;
    public Shader aoShader;
    private Material aoMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(true);
        this.aoMaterial = this.CheckShaderAndCreateMaterial(this.aoShader, this.aoMaterial);
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual void OnDisable()
    {
        if (this.aoMaterial)
        {
            UnityEngine.Object.DestroyImmediate(this.aoMaterial);
        }
        this.aoMaterial = null;
    }

    [UnityEngine.ImageEffectOpaque]
    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        Matrix4x4 P = this.GetComponent<Camera>().projectionMatrix;
        Matrix4x4 invP = P.inverse;
        Vector4 projInfo = new Vector4(-2f / (Screen.width * P[0]), -2f / (Screen.height * P[5]), (1f - P[2]) / P[0], (1f + P[6]) / P[5]);
        this.aoMaterial.SetVector("_ProjInfo", projInfo); // used for unprojection
        this.aoMaterial.SetMatrix("_ProjectionInv", invP); // only used for reference
        this.aoMaterial.SetTexture("_Rand", this.rand); // not needed for DX11 :)
        this.aoMaterial.SetFloat("_Radius", this.radius);
        this.aoMaterial.SetFloat("_Radius2", this.radius * this.radius);
        this.aoMaterial.SetFloat("_Intensity", this.intensity);
        this.aoMaterial.SetFloat("_BlurFilterDistance", this.blurFilterDistance);
        int rtW = source.width;
        int rtH = source.height;
        RenderTexture tmpRt = RenderTexture.GetTemporary(rtW >> this.downsample, rtH >> this.downsample);
        RenderTexture tmpRt2 = null;
        Graphics.Blit(source, tmpRt, this.aoMaterial, 0);
        if (this.downsample > 0)
        {
            tmpRt2 = RenderTexture.GetTemporary(rtW, rtH);
            Graphics.Blit(tmpRt, tmpRt2, this.aoMaterial, 4);
            RenderTexture.ReleaseTemporary(tmpRt);
            tmpRt = tmpRt2;
        }
        // @NOTE: it's probably worth a shot to blur in low resolution 
        //  instead with a bilat-upsample afterwards ...
        int i = 0;
        while (i < this.blurIterations)
        {
            this.aoMaterial.SetVector("_Axis", new Vector2(1f, 0f));
            tmpRt2 = RenderTexture.GetTemporary(rtW, rtH);
            Graphics.Blit(tmpRt, tmpRt2, this.aoMaterial, 1);
            RenderTexture.ReleaseTemporary(tmpRt);
            this.aoMaterial.SetVector("_Axis", new Vector2(0f, 1f));
            tmpRt = RenderTexture.GetTemporary(rtW, rtH);
            Graphics.Blit(tmpRt2, tmpRt, this.aoMaterial, 1);
            RenderTexture.ReleaseTemporary(tmpRt2);
            i++;
        }
        this.aoMaterial.SetTexture("_AOTex", tmpRt);
        Graphics.Blit(source, destination, this.aoMaterial, 2);
        RenderTexture.ReleaseTemporary(tmpRt);
    }

    public AmbientObscurance()
    {
        this.intensity = 0.5f;
        this.radius = 0.2f;
        this.blurIterations = 1;
        this.blurFilterDistance = 1.25f;
    }

}