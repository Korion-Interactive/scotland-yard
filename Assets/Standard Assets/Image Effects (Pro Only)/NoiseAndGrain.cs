using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Noise/Noise And Grain (Filmic)")]
public partial class NoiseAndGrain : PostEffectsBase
{
    public float intensityMultiplier;
    public float generalIntensity;
    public float blackIntensity;
    public float whiteIntensity;
    public float midGrey;
    public bool dx11Grain;
    public float softness;
    public bool monochrome;
    public Vector3 intensities;
    public Vector3 tiling;
    public float monochromeTiling;
    public FilterMode filterMode;
    public Texture2D noiseTexture;
    public Shader noiseShader;
    private Material noiseMaterial;
    public Shader dx11NoiseShader;
    private Material dx11NoiseMaterial;
    private static float TILE_AMOUNT;
    public override bool CheckResources()
    {
        this.CheckSupport(false);
        this.noiseMaterial = this.CheckShaderAndCreateMaterial(this.noiseShader, this.noiseMaterial);
        if (this.dx11Grain && this.supportDX11)
        {
            this.dx11NoiseShader = Shader.Find("Hidden/NoiseAndGrainDX11");
            this.dx11NoiseMaterial = this.CheckShaderAndCreateMaterial(this.dx11NoiseShader, this.dx11NoiseMaterial);
        }
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if ((this.CheckResources() == false) || (null == this.noiseTexture))
        {
            Graphics.Blit(source, destination);
            if (null == this.noiseTexture)
            {
                Debug.LogWarning("Noise & Grain effect failing as noise texture is not assigned. please assign.", this.transform);
            }
            return;
        }
        this.softness = Mathf.Clamp(this.softness, 0f, 0.99f);
        if (this.dx11Grain && this.supportDX11)
        {
            // We have a fancy, procedural noise pattern in this version, so no texture needed
            this.dx11NoiseMaterial.SetFloat("_DX11NoiseTime", Time.frameCount);
            this.dx11NoiseMaterial.SetTexture("_NoiseTex", this.noiseTexture);
            this.dx11NoiseMaterial.SetVector("_NoisePerChannel", this.monochrome ? Vector3.one : this.intensities);
            this.dx11NoiseMaterial.SetVector("_MidGrey", new Vector3(this.midGrey, 1f / (1f - this.midGrey), -1f / this.midGrey));
            this.dx11NoiseMaterial.SetVector("_NoiseAmount", new Vector3(this.generalIntensity, this.blackIntensity, this.whiteIntensity) * this.intensityMultiplier);
            if (this.softness > Mathf.Epsilon)
            {
                RenderTexture rt = RenderTexture.GetTemporary((int) (source.width * (1f - this.softness)), (int) (source.height * (1f - this.softness)));
                NoiseAndGrain.DrawNoiseQuadGrid(source, rt, this.dx11NoiseMaterial, this.noiseTexture, this.monochrome ? 3 : 2);
                this.dx11NoiseMaterial.SetTexture("_NoiseTex", rt);
                Graphics.Blit(source, destination, this.dx11NoiseMaterial, 4);
                RenderTexture.ReleaseTemporary(rt);
            }
            else
            {
                NoiseAndGrain.DrawNoiseQuadGrid(source, destination, this.dx11NoiseMaterial, this.noiseTexture, this.monochrome ? 1 : 0);
            }
        }
        else
        {
            // normal noise (DX9 style)
            if (this.noiseTexture)
            {
                this.noiseTexture.wrapMode = TextureWrapMode.Repeat;
                this.noiseTexture.filterMode = this.filterMode;
            }
            this.noiseMaterial.SetTexture("_NoiseTex", this.noiseTexture);
            this.noiseMaterial.SetVector("_NoisePerChannel", this.monochrome ? Vector3.one : this.intensities);
            this.noiseMaterial.SetVector("_NoiseTilingPerChannel", this.monochrome ? Vector3.one * this.monochromeTiling : this.tiling);
            this.noiseMaterial.SetVector("_MidGrey", new Vector3(this.midGrey, 1f / (1f - this.midGrey), -1f / this.midGrey));
            this.noiseMaterial.SetVector("_NoiseAmount", new Vector3(this.generalIntensity, this.blackIntensity, this.whiteIntensity) * this.intensityMultiplier);
            if (this.softness > Mathf.Epsilon)
            {
                RenderTexture rt2 = RenderTexture.GetTemporary((int) (source.width * (1f - this.softness)), (int) (source.height * (1f - this.softness)));
                NoiseAndGrain.DrawNoiseQuadGrid(source, rt2, this.noiseMaterial, this.noiseTexture, 2);
                this.noiseMaterial.SetTexture("_NoiseTex", rt2);
                Graphics.Blit(source, destination, this.noiseMaterial, 1);
                RenderTexture.ReleaseTemporary(rt2);
            }
            else
            {
                NoiseAndGrain.DrawNoiseQuadGrid(source, destination, this.noiseMaterial, this.noiseTexture, 0);
            }
        }
    }

    public static void DrawNoiseQuadGrid(RenderTexture source, RenderTexture dest, Material fxMaterial, Texture2D noise, int passNr)
    {
        RenderTexture.active = dest;
        float noiseSize = noise.width * 1f;
        float subDs = (1f * source.width) / NoiseAndGrain.TILE_AMOUNT;
        fxMaterial.SetTexture("_MainTex", source);
        GL.PushMatrix();
        GL.LoadOrtho();
        float aspectCorrection = (1f * source.width) / (1f * source.height);
        float stepSizeX = 1f / subDs;
        float stepSizeY = stepSizeX * aspectCorrection;
        float texTile = noiseSize / (noise.width * 1f);
        fxMaterial.SetPass(passNr);
        GL.Begin(GL.QUADS);
        float x1 = 0f;
        while (x1 < 1f)
        {
            float y1 = 0f;
            while (y1 < 1f)
            {
                float tcXStart = Random.Range(0f, 1f);
                float tcYStart = Random.Range(0f, 1f);
                //var v3 : Vector3 = Random.insideUnitSphere; 
                //var c : Color = new Color(v3.x, v3.y, v3.z);
                tcXStart = Mathf.Floor(tcXStart * noiseSize) / noiseSize;
                tcYStart = Mathf.Floor(tcYStart * noiseSize) / noiseSize;
                float texTileMod = 1f / noiseSize;
                GL.MultiTexCoord2(0, tcXStart, tcYStart);
                GL.MultiTexCoord2(1, 0f, 0f);
                //GL.Color( c );
                GL.Vertex3(x1, y1, 0.1f);
                GL.MultiTexCoord2(0, tcXStart + (texTile * texTileMod), tcYStart);
                GL.MultiTexCoord2(1, 1f, 0f);
                //GL.Color( c );
                GL.Vertex3(x1 + stepSizeX, y1, 0.1f);
                GL.MultiTexCoord2(0, tcXStart + (texTile * texTileMod), tcYStart + (texTile * texTileMod));
                GL.MultiTexCoord2(1, 1f, 1f);
                //GL.Color( c );
                GL.Vertex3(x1 + stepSizeX, y1 + stepSizeY, 0.1f);
                GL.MultiTexCoord2(0, tcXStart, tcYStart + (texTile * texTileMod));
                GL.MultiTexCoord2(1, 0f, 1f);
                //GL.Color( c );
                GL.Vertex3(x1, y1 + stepSizeY, 0.1f);
                y1 = y1 + stepSizeY;
            }
            x1 = x1 + stepSizeX;
        }
        GL.End();
        GL.PopMatrix();
    }

    public NoiseAndGrain()
    {
        this.intensityMultiplier = 0.25f;
        this.generalIntensity = 0.5f;
        this.blackIntensity = 1f;
        this.whiteIntensity = 1f;
        this.midGrey = 0.2f;
        this.intensities = new Vector3(1f, 1f, 1f);
        this.tiling = new Vector3(64f, 64f, 64f);
        this.monochromeTiling = 64f;
        this.filterMode = FilterMode.Bilinear;
    }

    static NoiseAndGrain()
    {
        NoiseAndGrain.TILE_AMOUNT = 64f;
    }

}