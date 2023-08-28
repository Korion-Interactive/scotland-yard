using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.AddComponentMenu("Image Effects/Color Adjustments/Color Correction (3D Lookup Texture)")]
public partial class ColorCorrectionLut : PostEffectsBase
{
    public Shader shader;
    private Material material;
    // serialize this instead of having another 2d texture ref'ed
    public Texture3D converted3DLut;
    public string basedOnTempTex;
    public override bool CheckResources()
    {
        this.CheckSupport(false);
        this.material = this.CheckShaderAndCreateMaterial(this.shader, this.material);
        if (!this.isSupported || !SystemInfo.supports3DTextures)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual void OnDisable()
    {
        if (this.material)
        {
            UnityEngine.Object.DestroyImmediate(this.material);
            this.material = null;
        }
    }

    public virtual void OnDestroy()
    {
        if (this.converted3DLut)
        {
            UnityEngine.Object.DestroyImmediate(this.converted3DLut);
        }
        this.converted3DLut = null;
    }

    public virtual void SetIdentityLut()
    {
        int dim = 16;
        Color[] newC = new Color[(dim * dim) * dim];
        float oneOverDim = 1f / ((1f * dim) - 1f);
        int i = 0;
        while (i < dim)
        {
            int j = 0;
            while (j < dim)
            {
                int k = 0;
                while (k < dim)
                {
                    newC[(i + (j * dim)) + ((k * dim) * dim)] = new Color((i * 1f) * oneOverDim, (j * 1f) * oneOverDim, (k * 1f) * oneOverDim, 1f);
                    k++;
                }
                j++;
            }
            i++;
        }
        if (this.converted3DLut)
        {
            UnityEngine.Object.DestroyImmediate(this.converted3DLut);
        }
        this.converted3DLut = new Texture3D(dim, dim, dim, TextureFormat.ARGB32, false);
        this.converted3DLut.SetPixels(newC);
        this.converted3DLut.Apply();
        this.basedOnTempTex = "";
    }

    public virtual bool ValidDimensions(Texture2D tex2d)
    {
        if (!tex2d)
        {
            return false;
        }
        int h = tex2d.height;
        if (h != Mathf.FloorToInt(Mathf.Sqrt(tex2d.width)))
        {
            return false;
        }
        return true;
    }

    public virtual void Convert(Texture2D temp2DTex, string path)
    {
        // conversion fun: the given 2D texture needs to be of the format
        //  w * h, wheras h is the 'depth' (or 3d dimension 'dim') and w = dim * dim
        if (temp2DTex)
        {
            int dim = temp2DTex.width * temp2DTex.height;
            dim = temp2DTex.height;
            if (!this.ValidDimensions(temp2DTex))
            {
                Debug.LogWarning(("The given 2D texture " + temp2DTex.name) + " cannot be used as a 3D LUT.");
                this.basedOnTempTex = "";
                return;
            }
            Color[] c = temp2DTex.GetPixels();
            Color[] newC = new Color[c.Length];
            int i = 0;
            while (i < dim)
            {
                int j = 0;
                while (j < dim)
                {
                    int k = 0;
                    while (k < dim)
                    {
                        int j_ = (dim - j) - 1;
                        newC[(i + (j * dim)) + ((k * dim) * dim)] = c[((k * dim) + i) + ((j_ * dim) * dim)];
                        k++;
                    }
                    j++;
                }
                i++;
            }
            if (this.converted3DLut)
            {
                UnityEngine.Object.DestroyImmediate(this.converted3DLut);
            }
            this.converted3DLut = new Texture3D(dim, dim, dim, TextureFormat.ARGB32, false);
            this.converted3DLut.SetPixels(newC);
            this.converted3DLut.Apply();
            this.basedOnTempTex = path;
        }
        else
        {
            // error, something went terribly wrong
            Debug.LogError("Couldn't color correct with 3D LUT texture. Image Effect will be disabled.");
        }
    }

    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if ((this.CheckResources() == false) || !SystemInfo.supports3DTextures)
        {
            Graphics.Blit(source, destination);
            return;
        }
        if (this.converted3DLut == null)
        {
            this.SetIdentityLut();
        }
        int lutSize = this.converted3DLut.width;
        this.converted3DLut.wrapMode = TextureWrapMode.Clamp;
        this.material.SetFloat("_Scale", (lutSize - 1) / (1f * lutSize));
        this.material.SetFloat("_Offset", 1f / (2f * lutSize));
        this.material.SetTexture("_ClutTex", this.converted3DLut);
        Graphics.Blit(source, destination, this.material, QualitySettings.activeColorSpace == ColorSpace.Linear ? 1 : 0);
    }

    public ColorCorrectionLut()
    {
        this.basedOnTempTex = "";
    }

}