using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
public partial class PostEffectsHelper : MonoBehaviour
{
    public virtual void Start()
    {
    }

    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Debug.Log("OnRenderImage in Helper called ...");
    }

    public static void DrawLowLevelPlaneAlignedWithCamera(float dist, RenderTexture source, RenderTexture dest, Material material, Camera cameraForProjectionMatrix)
    {
        float y1_ = 0.0f;
        float y2_ = 0.0f;
         // Make the destination texture the target for all rendering
        RenderTexture.active = dest;
        // Assign the source texture to a property from a shader
        material.SetTexture("_MainTex", source);
        bool invertY = true; // source.texelSize.y < 0.0f;
        // Set up the simple Matrix
        GL.PushMatrix();
        GL.LoadIdentity();
        GL.LoadProjectionMatrix(cameraForProjectionMatrix.projectionMatrix);
        float fovYHalfRad = (cameraForProjectionMatrix.fieldOfView * 0.5f) * Mathf.Deg2Rad;
        float cotangent = Mathf.Cos(fovYHalfRad) / Mathf.Sin(fovYHalfRad);
        float asp = cameraForProjectionMatrix.aspect;
        float x1 = asp / -cotangent;
        float x2 = asp / cotangent;
        float y1 = 1f / -cotangent;
        float y2 = 1f / cotangent;
        float sc = 1f; // magic constant (for now)
        x1 = x1 * (dist * sc);
        x2 = x2 * (dist * sc);
        y1 = y1 * (dist * sc);
        y2 = y2 * (dist * sc);
        float z1 = -dist;
        int i = 0;
        while (i < material.passCount)
        {
            material.SetPass(i);
            GL.Begin(GL.QUADS);
            if (invertY)
            {
                y1_ = 1f;
                y2_ = 0f;
            }
            else
            {
                y1_ = 0f;
                y2_ = 1f;
            }
            GL.TexCoord2(0f, y1_);
            GL.Vertex3(x1, y1, z1);
            GL.TexCoord2(1f, y1_);
            GL.Vertex3(x2, y1, z1);
            GL.TexCoord2(1f, y2_);
            GL.Vertex3(x2, y2, z1);
            GL.TexCoord2(0f, y2_);
            GL.Vertex3(x1, y2, z1);
            GL.End();
            i++;
        }
        GL.PopMatrix();
    }

    public static void DrawBorder(RenderTexture dest, Material material)
    {
        float x1 = 0.0f;
        float x2 = 0.0f;
        float y1 = 0.0f;
        float y2 = 0.0f;
        float y1_ = 0.0f;
        float y2_ = 0.0f;
        RenderTexture.active = dest;
        bool invertY = true; // source.texelSize.y < 0.0f;
        // Set up the simple Matrix
        GL.PushMatrix();
        GL.LoadOrtho();
        int i = 0;
        while (i < material.passCount)
        {
            material.SetPass(i);
            if (invertY)
            {
                y1_ = 1f;
                y2_ = 0f;
            }
            else
            {
                y1_ = 0f;
                y2_ = 1f;
            }
            // left	        
            x1 = 0f;
            x2 = 0f + (1f / (dest.width * 1f));
            y1 = 0f;
            y2 = 1f;
            GL.Begin(GL.QUADS);
            GL.TexCoord2(0f, y1_);
            GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1f, y1_);
            GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1f, y2_);
            GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0f, y2_);
            GL.Vertex3(x1, y2, 0.1f);
            // right
            x1 = 1f - (1f / (dest.width * 1f));
            x2 = 1f;
            y1 = 0f;
            y2 = 1f;
            GL.TexCoord2(0f, y1_);
            GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1f, y1_);
            GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1f, y2_);
            GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0f, y2_);
            GL.Vertex3(x1, y2, 0.1f);
            // top
            x1 = 0f;
            x2 = 1f;
            y1 = 0f;
            y2 = 0f + (1f / (dest.height * 1f));
            GL.TexCoord2(0f, y1_);
            GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1f, y1_);
            GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1f, y2_);
            GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0f, y2_);
            GL.Vertex3(x1, y2, 0.1f);
            // bottom
            x1 = 0f;
            x2 = 1f;
            y1 = 1f - (1f / (dest.height * 1f));
            y2 = 1f;
            GL.TexCoord2(0f, y1_);
            GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1f, y1_);
            GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1f, y2_);
            GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0f, y2_);
            GL.Vertex3(x1, y2, 0.1f);
            GL.End();
            i++;
        }
        GL.PopMatrix();
    }

    public static void DrawLowLevelQuad(float x1, float x2, float y1, float y2, RenderTexture source, RenderTexture dest, Material material)
    {
        float y1_ = 0.0f;
        float y2_ = 0.0f;
         // Make the destination texture the target for all rendering
        RenderTexture.active = dest;
        // Assign the source texture to a property from a shader
        material.SetTexture("_MainTex", source);
        bool invertY = true; // source.texelSize.y < 0.0f;
        // Set up the simple Matrix
        GL.PushMatrix();
        GL.LoadOrtho();
        int i = 0;
        while (i < material.passCount)
        {
            material.SetPass(i);
            GL.Begin(GL.QUADS);
            if (invertY)
            {
                y1_ = 1f;
                y2_ = 0f;
            }
            else
            {
                y1_ = 0f;
                y2_ = 1f;
            }
            GL.TexCoord2(0f, y1_);
            GL.Vertex3(x1, y1, 0.1f);
            GL.TexCoord2(1f, y1_);
            GL.Vertex3(x2, y1, 0.1f);
            GL.TexCoord2(1f, y2_);
            GL.Vertex3(x2, y2, 0.1f);
            GL.TexCoord2(0f, y2_);
            GL.Vertex3(x1, y2, 0.1f);
            GL.End();
            i++;
        }
        GL.PopMatrix();
    }

}