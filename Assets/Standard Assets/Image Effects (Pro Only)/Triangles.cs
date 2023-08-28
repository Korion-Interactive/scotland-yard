using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class Triangles : MonoBehaviour
{
    public static Mesh[] meshes;
    public static int currentTris;
    public static bool HasMeshes()
    {
        if (Triangles.meshes == null)
        {
            return false;
        }
        foreach (Mesh m in Triangles.meshes)
        {
            if (null == m)
            {
                return false;
            }
        }
        return true;
    }

    public static void Cleanup()
    {
        if (Triangles.meshes == null)
        {
            return;
        }
        foreach (Mesh m in Triangles.meshes)
        {
            if (null != m)
            {
                UnityEngine.Object.DestroyImmediate(m);
            }
        }
        Triangles.meshes = null;
    }

    public static Mesh[] GetMeshes(int totalWidth, int totalHeight)
    {
        if (Triangles.HasMeshes() && (Triangles.currentTris == (totalWidth * totalHeight)))
        {
            return Triangles.meshes;
        }
        int maxTris = 65000 / 3;
        int totalTris = totalWidth * totalHeight;
        Triangles.currentTris = totalTris;
        int meshCount = Mathf.CeilToInt((1f * totalTris) / (1f * maxTris));
        Triangles.meshes = new Mesh[meshCount];
        int i = 0;
        int index = 0;
        i = 0;
        while (i < totalTris)
        {
            int tris = Mathf.FloorToInt(Mathf.Clamp(totalTris - i, 0, maxTris));
            Triangles.meshes[index] = Triangles.GetMesh(tris, i, totalWidth, totalHeight);
            index++;
            i = i + maxTris;
        }
        return Triangles.meshes;
    }

    public static Mesh GetMesh(int triCount, int triOffset, int totalWidth, int totalHeight)
    {
        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.DontSave;
        Vector3[] verts = new Vector3[triCount * 3];
        Vector2[] uvs = new Vector2[triCount * 3];
        Vector2[] uvs2 = new Vector2[triCount * 3];
        int[] tris = new int[triCount * 3];
        float size = 0.0075f;
        int i = 0;
        while (i < triCount)
        {
            int i3 = i * 3;
            int vertexWithOffset = triOffset + i;
            float x = Mathf.Floor(vertexWithOffset % totalWidth) / totalWidth;
            float y = Mathf.Floor(vertexWithOffset / totalWidth) / totalHeight;
            Vector3 position = new Vector3((x * 2) - 1, (y * 2) - 1, 1f);
            verts[i3 + 0] = position;
            verts[i3 + 1] = position;
            verts[i3 + 2] = position;
            uvs[i3 + 0] = new Vector2(0f, 0f);
            uvs[i3 + 1] = new Vector2(1f, 0f);
            uvs[i3 + 2] = new Vector2(0f, 1f);
            uvs2[i3 + 0] = new Vector2(x, y);
            uvs2[i3 + 1] = new Vector2(x, y);
            uvs2[i3 + 2] = new Vector2(x, y);
            tris[i3 + 0] = i3 + 0;
            tris[i3 + 1] = i3 + 1;
            tris[i3 + 2] = i3 + 2;
            i++;
        }
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.uv2 = uvs2;
        return mesh;
    }

}