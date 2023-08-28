using UnityEngine;
using System.Collections;

public enum EdgeDetectMode
{
    TriangleDepthNormals = 0,
    RobertsCrossDepthNormals = 1,
    SobelDepth = 2,
    SobelDepthThin = 3,
    TriangleLuminance = 4
}

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Edge Detection/Edge Detection")]
public partial class EdgeDetectEffectNormals : PostEffectsBase
{
    public EdgeDetectMode mode;
    public float sensitivityDepth;
    public float sensitivityNormals;
    public float lumThreshhold;
    public float edgeExp;
    public float sampleDist;
    public float edgesOnly;
    public Color edgesOnlyBgColor;
    public Shader edgeDetectShader;
    private Material edgeDetectMaterial;
    private EdgeDetectMode oldMode;
    public override bool CheckResources()
    {
        this.CheckSupport(true);
        this.edgeDetectMaterial = this.CheckShaderAndCreateMaterial(this.edgeDetectShader, this.edgeDetectMaterial);
        if (this.mode != this.oldMode)
        {
            this.SetCameraFlag();
        }
        this.oldMode = this.mode;
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public override void Start()
    {
        this.oldMode = this.mode;
    }

    public virtual void SetCameraFlag()
    {
        if (this.mode > (EdgeDetectMode) 1)
        {
            this.GetComponent<Camera>().depthTextureMode = this.GetComponent<Camera>().depthTextureMode | DepthTextureMode.Depth;
        }
        else
        {
            this.GetComponent<Camera>().depthTextureMode = this.GetComponent<Camera>().depthTextureMode | DepthTextureMode.DepthNormals;
        }
    }

    public override void OnEnable()
    {
        this.SetCameraFlag();
    }

    [UnityEngine.ImageEffectOpaque]
    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (this.CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        Vector2 sensitivity = new Vector2(this.sensitivityDepth, this.sensitivityNormals);
        this.edgeDetectMaterial.SetVector("_Sensitivity", new Vector4(sensitivity.x, sensitivity.y, 1f, sensitivity.y));
        this.edgeDetectMaterial.SetFloat("_BgFade", this.edgesOnly);
        this.edgeDetectMaterial.SetFloat("_SampleDistance", this.sampleDist);
        this.edgeDetectMaterial.SetVector("_BgColor", this.edgesOnlyBgColor);
        this.edgeDetectMaterial.SetFloat("_Exponent", this.edgeExp);
        this.edgeDetectMaterial.SetFloat("_Threshold", this.lumThreshhold);
        Graphics.Blit(source, destination, this.edgeDetectMaterial, (int) this.mode);
    }

    public EdgeDetectEffectNormals()
    {
        this.mode = EdgeDetectMode.SobelDepthThin;
        this.sensitivityDepth = 1f;
        this.sensitivityNormals = 1f;
        this.lumThreshhold = 0.2f;
        this.edgeExp = 1f;
        this.sampleDist = 1f;
        this.edgesOnlyBgColor = Color.white;
        this.oldMode = EdgeDetectMode.SobelDepthThin;
    }

}