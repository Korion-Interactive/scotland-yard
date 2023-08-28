using UnityEngine;
using System.Collections;

[System.Serializable]
[UnityEngine.ExecuteInEditMode]
[UnityEngine.RequireComponent(typeof(Camera))]
[UnityEngine.AddComponentMenu("Image Effects/Camera/Camera Motion Blur")]
public partial class CameraMotionBlur : PostEffectsBase
{
     // make sure to match this to MAX_RADIUS in shader ('k' in paper)
    public static int MAX_RADIUS;
    public enum MotionBlurFilter
    {
        CameraMotion = 0, // global screen blur based on cam motion
        LocalBlur = 1, // cheap blur, no dilation or scattering
        Reconstruction = 2, // advanced filter (simulates scattering) as in plausible motion blur paper
        ReconstructionDX11 = 3, // advanced filter (simulates scattering) as in plausible motion blur paper
        ReconstructionDisc = 4 // advanced filter using scaled poisson disc sampling
    }


    // settings		
    public CameraMotionBlur.MotionBlurFilter filterType;
    public bool preview; // show how blur would look like in action ...
    public Vector3 previewScale; // ... given this movement vector
    // params
    public float movementScale;
    public float rotationScale;
    public float maxVelocity; // maximum velocity in pixels
    public float minVelocity; // minimum velocity in pixels
    public float velocityScale; // global velocity scale
    public float softZDistance; // for z overlap check softness (reconstruction filter only)
    public int velocityDownsample; // low resolution velocity buffer? (optimization)
    public LayerMask excludeLayers;
    //public var dynamicLayers : LayerMask = 0;
    private GameObject tmpCam;
    // resources
    public Shader shader;
    public Shader dx11MotionBlurShader;
    public Shader replacementClear;
    //public var replacementDynamics : Shader;
    private Material motionBlurMaterial;
    private Material dx11MotionBlurMaterial;
    public Texture2D noiseTexture;
    public float jitter;
    // (internal) debug
    public bool showVelocity;
    public float showVelocityScale;
    // camera transforms
    private Matrix4x4 currentViewProjMat;
    private Matrix4x4 prevViewProjMat;
    private int prevFrameCount;
    private bool wasActive;
    // shortcuts to calculate global blur direction when using 'CameraMotion'
    private Vector3 prevFrameForward;
    private Vector3 prevFrameRight;
    private Vector3 prevFrameUp;
    private Vector3 prevFramePos;
    private void CalculateViewProjection()
    {
        Matrix4x4 viewMat = this.GetComponent<Camera>().worldToCameraMatrix;
        Matrix4x4 projMat = GL.GetGPUProjectionMatrix(this.GetComponent<Camera>().projectionMatrix, true);
        this.currentViewProjMat = projMat * viewMat;
    }

    public override void Start()
    {
        this.CheckResources();
        this.wasActive = this.gameObject.activeInHierarchy;
        this.CalculateViewProjection();
        this.Remember();
        this.wasActive = false; // hack to fake position/rotation update and prevent bad blurs
    }

    public override void OnEnable()
    {
        this.GetComponent<Camera>().depthTextureMode = this.GetComponent<Camera>().depthTextureMode | DepthTextureMode.Depth;
    }

    public virtual void OnDisable()
    {
        if (null != this.motionBlurMaterial)
        {
            UnityEngine.Object.DestroyImmediate(this.motionBlurMaterial);
            this.motionBlurMaterial = null;
        }
        if (null != this.dx11MotionBlurMaterial)
        {
            UnityEngine.Object.DestroyImmediate(this.dx11MotionBlurMaterial);
            this.dx11MotionBlurMaterial = null;
        }
        if (null != this.tmpCam)
        {
            UnityEngine.Object.DestroyImmediate(this.tmpCam);
            this.tmpCam = null;
        }
    }

    public override bool CheckResources()
    {
        this.CheckSupport(true, true); // depth & hdr needed
        this.motionBlurMaterial = this.CheckShaderAndCreateMaterial(this.shader, this.motionBlurMaterial);
        if (this.supportDX11 && (this.filterType == MotionBlurFilter.ReconstructionDX11))
        {
            this.dx11MotionBlurMaterial = this.CheckShaderAndCreateMaterial(this.dx11MotionBlurShader, this.dx11MotionBlurMaterial);
        }
        if (!this.isSupported)
        {
            this.ReportAutoDisable();
        }
        return this.isSupported;
    }

    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (false == this.CheckResources())
        {
            Graphics.Blit(source, destination);
            return;
        }
        if (this.filterType == MotionBlurFilter.CameraMotion)
        {
            this.StartFrame();
        }
        // use if possible new RG format ... fallback to half otherwise
        RenderTextureFormat rtFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGHalf) ? RenderTextureFormat.RGHalf : RenderTextureFormat.ARGBHalf;
        // get temp textures
        RenderTexture velBuffer = RenderTexture.GetTemporary(this.divRoundUp(source.width, this.velocityDownsample), this.divRoundUp(source.height, this.velocityDownsample), 0, rtFormat);
        int tileWidth = 1;
        int tileHeight = 1;
        this.maxVelocity = Mathf.Max(2f, this.maxVelocity);
        float _maxVelocity = this.maxVelocity;
        // note: 's' is hardcoded in shaders except for DX11 path // calculate 'k'
        // auto DX11 fallback!
        bool fallbackFromDX11 = false;
        if ((this.filterType == MotionBlurFilter.ReconstructionDX11) && (this.dx11MotionBlurMaterial == null))
        {
            fallbackFromDX11 = true;
        }
        if (((this.filterType == MotionBlurFilter.Reconstruction) || fallbackFromDX11) || (this.filterType == MotionBlurFilter.ReconstructionDisc))
        {
            this.maxVelocity = Mathf.Min(this.maxVelocity, CameraMotionBlur.MAX_RADIUS);
            tileWidth = this.divRoundUp(velBuffer.width, (int) this.maxVelocity);
            tileHeight = this.divRoundUp(velBuffer.height, (int) this.maxVelocity);
            _maxVelocity = velBuffer.width / tileWidth;
        }
        else
        {
            tileWidth = this.divRoundUp(velBuffer.width, (int) this.maxVelocity);
            tileHeight = this.divRoundUp(velBuffer.height, (int) this.maxVelocity);
            _maxVelocity = velBuffer.width / tileWidth;
        }
        RenderTexture tileMax = RenderTexture.GetTemporary(tileWidth, tileHeight, 0, rtFormat);
        RenderTexture neighbourMax = RenderTexture.GetTemporary(tileWidth, tileHeight, 0, rtFormat);
        velBuffer.filterMode = FilterMode.Point;
        tileMax.filterMode = FilterMode.Point;
        neighbourMax.filterMode = FilterMode.Point;
        if (this.noiseTexture)
        {
            this.noiseTexture.filterMode = FilterMode.Point;
        }
        source.wrapMode = TextureWrapMode.Clamp;
        velBuffer.wrapMode = TextureWrapMode.Clamp;
        neighbourMax.wrapMode = TextureWrapMode.Clamp;
        tileMax.wrapMode = TextureWrapMode.Clamp;
        // calc correct viewprj matrix
        this.CalculateViewProjection();
        // just started up?		
        if (this.gameObject.activeInHierarchy && !this.wasActive)
        {
            this.Remember();
        }
        this.wasActive = this.gameObject.activeInHierarchy;
        // matrices
        Matrix4x4 invViewPrj = Matrix4x4.Inverse(this.currentViewProjMat);
        this.motionBlurMaterial.SetMatrix("_InvViewProj", invViewPrj);
        this.motionBlurMaterial.SetMatrix("_PrevViewProj", this.prevViewProjMat);
        this.motionBlurMaterial.SetMatrix("_ToPrevViewProjCombined", this.prevViewProjMat * invViewPrj);
        this.motionBlurMaterial.SetFloat("_MaxVelocity", _maxVelocity);
        this.motionBlurMaterial.SetFloat("_MaxRadiusOrKInPaper", _maxVelocity);
        this.motionBlurMaterial.SetFloat("_MinVelocity", this.minVelocity);
        this.motionBlurMaterial.SetFloat("_VelocityScale", this.velocityScale);
        this.motionBlurMaterial.SetFloat("_Jitter", this.jitter);
        // texture samplers
        this.motionBlurMaterial.SetTexture("_NoiseTex", this.noiseTexture);
        this.motionBlurMaterial.SetTexture("_VelTex", velBuffer);
        this.motionBlurMaterial.SetTexture("_NeighbourMaxTex", neighbourMax);
        this.motionBlurMaterial.SetTexture("_TileTexDebug", tileMax);
        if (this.preview)
        {
            // generate an artifical 'previous' matrix to simulate blur look
            Matrix4x4 viewMat = this.GetComponent<Camera>().worldToCameraMatrix;
            Matrix4x4 offset = Matrix4x4.identity;
            offset.SetTRS(this.previewScale * 0.3333f, Quaternion.identity, Vector3.one); // using only translation
            Matrix4x4 projMat = GL.GetGPUProjectionMatrix(this.GetComponent<Camera>().projectionMatrix, true);
            this.prevViewProjMat = (projMat * offset) * viewMat;
            this.motionBlurMaterial.SetMatrix("_PrevViewProj", this.prevViewProjMat);
            this.motionBlurMaterial.SetMatrix("_ToPrevViewProjCombined", this.prevViewProjMat * invViewPrj);
        }
        if (this.filterType == MotionBlurFilter.CameraMotion)
        {
             // build blur vector to be used in shader to create a global blur direction
            Vector4 blurVector = Vector4.zero;
            float lookUpDown = Vector3.Dot(this.transform.up, Vector3.up);
            Vector3 distanceVector = this.prevFramePos - this.transform.position;
            float distMag = distanceVector.magnitude;
            float farHeur = 1f;
            // pitch (vertical)
            farHeur = (Vector3.Angle(this.transform.up, this.prevFrameUp) / this.GetComponent<Camera>().fieldOfView) * (source.width * 0.75f);
            blurVector.x = this.rotationScale * farHeur;//Mathf.Clamp01((1.0f-Vector3.Dot(transform.up, prevFrameUp)));
            // yaw #1 (horizontal, faded by pitch)
            farHeur = (Vector3.Angle(this.transform.forward, this.prevFrameForward) / this.GetComponent<Camera>().fieldOfView) * (source.width * 0.75f);
            blurVector.y = (this.rotationScale * lookUpDown) * farHeur;//Mathf.Clamp01((1.0f-Vector3.Dot(transform.forward, prevFrameForward)));
            // yaw #2 (when looking down, faded by 1-pitch)
            farHeur = (Vector3.Angle(this.transform.forward, this.prevFrameForward) / this.GetComponent<Camera>().fieldOfView) * (source.width * 0.75f);
            blurVector.z = (this.rotationScale * (1f - lookUpDown)) * farHeur;//Mathf.Clamp01((1.0f-Vector3.Dot(transform.forward, prevFrameForward)));
            if ((distMag > Mathf.Epsilon) && (this.movementScale > Mathf.Epsilon))
            {
                // forward (probably most important)
                blurVector.w = (this.movementScale * Vector3.Dot(this.transform.forward, distanceVector)) * (source.width * 0.5f);
                // jump (maybe scale down further)
                blurVector.x = blurVector.x + ((this.movementScale * Vector3.Dot(this.transform.up, distanceVector)) * (source.width * 0.5f));
                // strafe (maybe scale down further)
                blurVector.y = blurVector.y + ((this.movementScale * Vector3.Dot(this.transform.right, distanceVector)) * (source.width * 0.5f));
            }
            if (this.preview) // crude approximation
            {
                this.motionBlurMaterial.SetVector("_BlurDirectionPacked", (new Vector4(this.previewScale.y, this.previewScale.x, 0f, this.previewScale.z) * 0.5f) * this.GetComponent<Camera>().fieldOfView);
            }
            else
            {
                this.motionBlurMaterial.SetVector("_BlurDirectionPacked", blurVector);
            }
        }
        else
        {
            // generate velocity buffer	
            Graphics.Blit(source, velBuffer, this.motionBlurMaterial, 0);
            // patch up velocity buffer:
            // exclude certain layers (e.g. skinned objects as we cant really support that atm)
            Camera cam = null;
            if (this.excludeLayers.value != 0)// || dynamicLayers.value)
            {
                cam = this.GetTmpCam();
            }
            if (((cam && (this.excludeLayers.value != 0)) && this.replacementClear) && this.replacementClear.isSupported)
            {
                cam.targetTexture = velBuffer;
                cam.cullingMask = (int) this.excludeLayers;
                cam.RenderWithShader(this.replacementClear, "");
            }
        }
        // dynamic layers (e.g. rigid bodies)
        // no worky in 4.0, but let's fix for 4.x
        /*
			if (cam && dynamicLayers.value != 0 && replacementDynamics && replacementDynamics.isSupported) {

				Shader.SetGlobalFloat ("_MaxVelocity", maxVelocity);
				Shader.SetGlobalFloat ("_VelocityScale", velocityScale);
				Shader.SetGlobalVector ("_VelBufferSize", Vector4 (velBuffer.width, velBuffer.height, 0, 0));
				Shader.SetGlobalMatrix ("_PrevViewProj", prevViewProjMat);
				Shader.SetGlobalMatrix ("_ViewProj", currentViewProjMat);

				cam.targetTexture = velBuffer;				
				cam.cullingMask = dynamicLayers;
				cam.RenderWithShader (replacementDynamics, "");
			}
			*/
        if (!this.preview && (Time.frameCount != this.prevFrameCount))
        {
            // remember current transformation data for next frame
            this.prevFrameCount = Time.frameCount;
            this.Remember();
        }
        source.filterMode = FilterMode.Bilinear;
        // debug vel buffer:
        if (this.showVelocity)
        {
            // generate tile max and neighbour max		
            //Graphics.Blit (velBuffer, tileMax, motionBlurMaterial, 2);
            //Graphics.Blit (tileMax, neighbourMax, motionBlurMaterial, 3);
            this.motionBlurMaterial.SetFloat("_DisplayVelocityScale", this.showVelocityScale);
            Graphics.Blit(velBuffer, destination, this.motionBlurMaterial, 1);
        }
        else
        {
            if ((this.filterType == MotionBlurFilter.ReconstructionDX11) && !fallbackFromDX11)
            {
                // need to reset some parameters for dx11 shader
                this.dx11MotionBlurMaterial.SetFloat("_MinVelocity", this.minVelocity);
                this.dx11MotionBlurMaterial.SetFloat("_VelocityScale", this.velocityScale);
                this.dx11MotionBlurMaterial.SetFloat("_Jitter", this.jitter);
                // texture samplers
                this.dx11MotionBlurMaterial.SetTexture("_NoiseTex", this.noiseTexture);
                this.dx11MotionBlurMaterial.SetTexture("_VelTex", velBuffer);
                this.dx11MotionBlurMaterial.SetTexture("_NeighbourMaxTex", neighbourMax);
                this.dx11MotionBlurMaterial.SetFloat("_SoftZDistance", Mathf.Max(0.00025f, this.softZDistance));
                this.dx11MotionBlurMaterial.SetFloat("_MaxRadiusOrKInPaper", _maxVelocity);
                // generate tile max and neighbour max		
                Graphics.Blit(velBuffer, tileMax, this.dx11MotionBlurMaterial, 0);
                Graphics.Blit(tileMax, neighbourMax, this.dx11MotionBlurMaterial, 1);
                // final blur
                Graphics.Blit(source, destination, this.dx11MotionBlurMaterial, 2);
            }
            else
            {
                if ((this.filterType == MotionBlurFilter.Reconstruction) || fallbackFromDX11)
                {
                    // 'reconstructing' properly integrated color
                    this.motionBlurMaterial.SetFloat("_SoftZDistance", Mathf.Max(0.00025f, this.softZDistance));
                    // generate tile max and neighbour max		
                    Graphics.Blit(velBuffer, tileMax, this.motionBlurMaterial, 2);
                    Graphics.Blit(tileMax, neighbourMax, this.motionBlurMaterial, 3);
                    // final blur
                    Graphics.Blit(source, destination, this.motionBlurMaterial, 4);
                }
                else
                {
                    if (this.filterType == MotionBlurFilter.CameraMotion)
                    {
                        // orange box style motion blur
                        Graphics.Blit(source, destination, this.motionBlurMaterial, 6);
                    }
                    else
                    {
                        if (this.filterType == MotionBlurFilter.ReconstructionDisc)
                        {
                            // dof style motion blur defocuing and ellipse around the princical blur direction
                            // 'reconstructing' properly integrated color
                            this.motionBlurMaterial.SetFloat("_SoftZDistance", Mathf.Max(0.00025f, this.softZDistance));
                            // generate tile max and neighbour max		
                            Graphics.Blit(velBuffer, tileMax, this.motionBlurMaterial, 2);
                            Graphics.Blit(tileMax, neighbourMax, this.motionBlurMaterial, 3);
                            Graphics.Blit(source, destination, this.motionBlurMaterial, 7);
                        }
                        else
                        {
                            // simple & fast blur (low quality): just blurring along velocity
                            Graphics.Blit(source, destination, this.motionBlurMaterial, 5);
                        }
                    }
                }
            }
        }
        // cleanup
        RenderTexture.ReleaseTemporary(velBuffer);
        RenderTexture.ReleaseTemporary(tileMax);
        RenderTexture.ReleaseTemporary(neighbourMax);
    }

    public virtual void Remember()
    {
        this.prevViewProjMat = this.currentViewProjMat;
        this.prevFrameForward = this.transform.forward;
        this.prevFrameRight = this.transform.right;
        this.prevFrameUp = this.transform.up;
        this.prevFramePos = this.transform.position;
    }

    public virtual Camera GetTmpCam()
    {
        if (this.tmpCam == null)
        {
            string name = ("_" + this.GetComponent<Camera>().name) + "_MotionBlurTmpCam";
            GameObject go = GameObject.Find(name);
            if (null == go) // couldn't find, recreate
            {
                this.tmpCam = new GameObject(name, new System.Type[] {typeof(Camera)});
            }
            else
            {
                this.tmpCam = go;
            }
        }
        this.tmpCam.hideFlags = HideFlags.DontSave;
        this.tmpCam.transform.position = this.GetComponent<Camera>().transform.position;
        this.tmpCam.transform.rotation = this.GetComponent<Camera>().transform.rotation;
        this.tmpCam.transform.localScale = this.GetComponent<Camera>().transform.localScale;
        this.tmpCam.GetComponent<Camera>().CopyFrom(this.GetComponent<Camera>());
        this.tmpCam.GetComponent<Camera>().enabled = false;
        this.tmpCam.GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
        this.tmpCam.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
        return this.tmpCam.GetComponent<Camera>();
    }

    public virtual void StartFrame()
    {
        // take only x% of positional changes into account (camera motion)
        // TODO: possibly do the same for rotational part
        this.prevFramePos = Vector3.Slerp(this.prevFramePos, this.transform.position, 0.75f);
    }

    public virtual int divRoundUp(int x, int d)
    {
        return ((x + d) - 1) / d;
    }

    public CameraMotionBlur()
    {
        this.filterType = MotionBlurFilter.Reconstruction;
        this.previewScale = Vector3.one;
        this.rotationScale = 1f;
        this.maxVelocity = 8f;
        this.minVelocity = 0.1f;
        this.velocityScale = 0.375f;
        this.softZDistance = 0.005f;
        this.velocityDownsample = 1;
        this.jitter = 0.05f;
        this.showVelocityScale = 1f;
        this.prevFrameForward = Vector3.forward;
        this.prevFrameRight = Vector3.right;
        this.prevFrameUp = Vector3.up;
        this.prevFramePos = Vector3.zero;
    }

    static CameraMotionBlur()
    {
        CameraMotionBlur.MAX_RADIUS = (int) 10f;
    }

}