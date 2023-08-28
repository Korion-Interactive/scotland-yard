using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Rendering/Global Fog")]
public partial class GlobalFog : PostEffectsBase
{
    public enum FogMode
    {
        AbsoluteYAndDistance = 0,
        AbsoluteY = 1,
        Distance = 2,
        RelativeYAndDistance = 3
    }


    public GlobalFog.FogMode fogMode;
    private float CAMERA_NEAR;
    private float CAMERA_FAR;
    private float CAMERA_FOV;
    private float CAMERA_ASPECT_RATIO;
    public float startDistance;
    public float globalDensity;
    public float heightScale;
    public float height;
    public Color globalFogColor;
    public Shader fogShader;
    private Material fogMaterial;
    public override bool CheckResources()
    {
        this.CheckSupport(true);
        this.fogMaterial = this.CheckShaderAndCreateMaterial(this.fogShader, this.fogMaterial);
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Vector4 vec = default(Vector4);
        Vector3 corner = default(Vector3);
        if (this.CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        this.CAMERA_NEAR = this.GetComponent<Camera>().nearClipPlane;
        this.CAMERA_FAR = this.GetComponent<Camera>().farClipPlane;
        this.CAMERA_FOV = this.GetComponent<Camera>().fieldOfView;
        this.CAMERA_ASPECT_RATIO = this.GetComponent<Camera>().aspect;
        Matrix4x4 frustumCorners = Matrix4x4.identity;
        float fovWHalf = this.CAMERA_FOV * 0.5f;
        Vector3 toRight = ((this.GetComponent<Camera>().transform.right * this.CAMERA_NEAR) * Mathf.Tan(fovWHalf * Mathf.Deg2Rad)) * this.CAMERA_ASPECT_RATIO;
        Vector3 toTop = (this.GetComponent<Camera>().transform.up * this.CAMERA_NEAR) * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);
        Vector3 topLeft = ((this.GetComponent<Camera>().transform.forward * this.CAMERA_NEAR) - toRight) + toTop;
        float CAMERA_SCALE = (topLeft.magnitude * this.CAMERA_FAR) / this.CAMERA_NEAR;
        topLeft.Normalize();
        topLeft = topLeft * CAMERA_SCALE;
        Vector3 topRight = ((this.GetComponent<Camera>().transform.forward * this.CAMERA_NEAR) + toRight) + toTop;
        topRight.Normalize();
        topRight = topRight * CAMERA_SCALE;
        Vector3 bottomRight = ((this.GetComponent<Camera>().transform.forward * this.CAMERA_NEAR) + toRight) - toTop;
        bottomRight.Normalize();
        bottomRight = bottomRight * CAMERA_SCALE;
        Vector3 bottomLeft = ((this.GetComponent<Camera>().transform.forward * this.CAMERA_NEAR) - toRight) - toTop;
        bottomLeft.Normalize();
        bottomLeft = bottomLeft * CAMERA_SCALE;
        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);
        this.fogMaterial.SetMatrix("_FrustumCornersWS", frustumCorners);
        this.fogMaterial.SetVector("_CameraWS", this.GetComponent<Camera>().transform.position);
        this.fogMaterial.SetVector("_StartDistance", new Vector4(1f / this.startDistance, CAMERA_SCALE - this.startDistance));
        this.fogMaterial.SetVector("_Y", new Vector4(this.height, 1f / this.heightScale));
        this.fogMaterial.SetFloat("_GlobalDensity", this.globalDensity * 0.01f);
        this.fogMaterial.SetColor("_FogColor", this.globalFogColor);
        GlobalFog.CustomGraphicsBlit(source, destination, this.fogMaterial, (int) this.fogMode);
    }

    public static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
    {
        RenderTexture.active = dest;
        fxMaterial.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        fxMaterial.SetPass(passNr);
        GL.Begin(GL.QUADS);
        GL.MultiTexCoord2(0, 0f, 0f);
        GL.Vertex3(0f, 0f, 3f); // BL
        GL.MultiTexCoord2(0, 1f, 0f);
        GL.Vertex3(1f, 0f, 2f); // BR
        GL.MultiTexCoord2(0, 1f, 1f);
        GL.Vertex3(1f, 1f, 1f); // TR
        GL.MultiTexCoord2(0, 0f, 1f);
        GL.Vertex3(0f, 1f, 0f); // TL
        GL.End();
        GL.PopMatrix();
    }

    public GlobalFog()
    {
        this.fogMode = FogMode.AbsoluteYAndDistance;
        this.CAMERA_NEAR = 0.5f;
        this.CAMERA_FAR = 50f;
        this.CAMERA_FOV = 60f;
        this.CAMERA_ASPECT_RATIO = 1.333333f;
        this.startDistance = 200f;
        this.globalDensity = 1f;
        this.heightScale = 100f;
        this.globalFogColor = Color.grey;
    }

}