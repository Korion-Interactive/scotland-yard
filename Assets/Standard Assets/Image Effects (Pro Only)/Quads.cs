using UnityEngine;
using System.Collections;

[System.Serializable]
public partial class Quads : MonoBehaviour
{
    // same as Triangles but creates quads instead which generally
    // saves fillrate at the expense for more triangles to issue
    public static Mesh[] meshes;
    public static int currentQuads;
    public static bool HasMeshes()
    {
        if (Quads.meshes == null)
        {
            return false;
        }
        foreach (Mesh m in Quads.meshes)
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
        if (Quads.meshes == null)
        {
            return;
        }
        foreach (Mesh m in Quads.meshes)
        {
            if (null != m)
            {
                UnityEngine.Object.DestroyImmediate(m);
            }
        }
        Quads.meshes = null;
    }

    public static Mesh[] GetMeshes(int totalWidth, int totalHeight)
    {
        if (Quads.HasMeshes() && (Quads.currentQuads == (totalWidth * totalHeight)))
        {
            return Quads.meshes;
        }
        int maxQuads = 65000 / 6;
        int totalQuads = totalWidth * totalHeight;
        Quads.currentQuads = totalQuads;
        int meshCount = Mathf.CeilToInt((1f * totalQuads) / (1f * maxQuads));
        Quads.meshes = new Mesh[meshCount];
        int i = 0;
        int index = 0;
        i = 0;
        while (i < totalQuads)
        {
            int quads = Mathf.FloorToInt(Mathf.Clamp(totalQuads - i, 0, maxQuads));
            Quads.meshes[index] = Quads.GetMesh(quads, i, totalWidth, totalHeight);
            index++;
            i = i + maxQuads;
        }
        return Quads.meshes;
    }

    public static Mesh GetMesh(int triCount, int triOffset, int totalWidth, int totalHeight)
    {
        Mesh mesh = new Mesh();
        mesh.hideFlags = HideFlags.DontSave;
        Vector3[] verts = new Vector3[triCount * 4];
        Vector2[] uvs = new Vector2[triCount * 4];
        Vector2[] uvs2 = new Vector2[triCount * 4];
        int[] tris = new int[triCount * 6];
        float size = 0.0075f;
        int i = 0;
        while (i < triCount)
        {
            int i4 = i * 4;
            int i6 = i * 6;
            int vertexWithOffset = triOffset + i;
            float x = Mathf.Floor(vertexWithOffset % totalWidth) / totalWidth;
            float y = Mathf.Floor(vertexWithOffset / totalWidth) / totalHeight;
            Vector3 position = new Vector3((x * 2) - 1, (y * 2) - 1, 1f);
            verts[i4 + 0] = position;
            verts[i4 + 1] = position;
            verts[i4 + 2] = position;
            verts[i4 + 3] = position;
            uvs[i4 + 0] = new Vector2(0f, 0f);
            uvs[i4 + 1] = new Vector2(1f, 0f);
            uvs[i4 + 2] = new Vector2(0f, 1f);
            uvs[i4 + 3] = new Vector2(1f, 1f);
            uvs2[i4 + 0] = new Vector2(x, y);
            uvs2[i4 + 1] = new Vector2(x, y);
            uvs2[i4 + 2] = new Vector2(x, y);
            uvs2[i4 + 3] = new Vector2(x, y);
            tris[i6 + 0] = i4 + 0;
            tris[i6 + 1] = i4 + 1;
            tris[i6 + 2] = i4 + 2;
            tris[i6 + 3] = i4 + 1;
            tris[i6 + 4] = i4 + 2;
            tris[i6 + 5] = i4 + 3;
            i++;
        }
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.uv2 = uvs2;
        return mesh;
    }

}