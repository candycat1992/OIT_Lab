using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class WeightedBlendedManager : MonoBehaviour {

    public enum TransparentMode { ODT = 0, Blended, BlendedAndWeighted }
    public enum WeightFunction { Weight0 = 0,  Weight1, Weight2 }

    #region Public params
    public Shader accumulateShader = null;
    public Shader revealageShader = null;
    public Shader blendShader = null;
    public TransparentMode transparentMode = TransparentMode.ODT;
    public WeightFunction weightFunction = WeightFunction.Weight0;
    #endregion

    #region Private params
    private Camera m_camera = null;
    private Camera m_transparentCamera = null;
    private GameObject m_transparentCameraObj = null;
    private RenderTexture m_opaqueTex = null;
    private RenderTexture m_accumTex = null;
    private RenderTexture m_revealageTex = null;
    private Material m_blendMat = null;
    #endregion

	// Use this for initialization
	void Awake () {
        m_camera = GetComponent<Camera>();
        if (m_transparentCameraObj != null) {
            DestroyImmediate(m_transparentCameraObj);
        }
        m_transparentCameraObj = new GameObject("OITCamera");
        m_transparentCameraObj.hideFlags = HideFlags.DontSave;
        m_transparentCameraObj.transform.parent = transform;
        m_transparentCameraObj.transform.localPosition = Vector3.zero;
        m_transparentCamera = m_transparentCameraObj.AddComponent<Camera>();
        m_transparentCamera.CopyFrom(m_camera);
        m_transparentCamera.clearFlags = CameraClearFlags.Nothing;
        m_transparentCamera.enabled = false;

        m_blendMat = new Material(blendShader);
        m_blendMat.hideFlags = HideFlags.DontSave;
	}

    void OnDestroy() {
        DestroyImmediate(m_transparentCameraObj);
    }

    void OnPreRender() {
        if (transparentMode == TransparentMode.ODT) {
            // Just render everything as normal
            m_camera.cullingMask = -1;
        } else {
            // The main camera shouldn't render anything
            // Everything is rendered in procedural
            m_camera.cullingMask = 0;
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        if (transparentMode == TransparentMode.ODT) {
            Graphics.Blit(src, dst);
        } else {
            switch (transparentMode) {
                case TransparentMode.Blended:
                    Shader.DisableKeyword("_WEIGHTED_ON");
                break;
                case TransparentMode.BlendedAndWeighted:
                    Shader.EnableKeyword("_WEIGHTED_ON");
                break;
            }
            switch (weightFunction) {
                case WeightFunction.Weight0:
                    Shader.EnableKeyword("_WEIGHTED0");
                    Shader.DisableKeyword("_WEIGHTED1");
                    Shader.DisableKeyword("_WEIGHTED2");
                break;
                case WeightFunction.Weight1:
                    Shader.EnableKeyword("_WEIGHTED1");
                    Shader.DisableKeyword("_WEIGHTED0");
                    Shader.DisableKeyword("_WEIGHTED2");
                break;
                case WeightFunction.Weight2:
                    Shader.EnableKeyword("_WEIGHTED2");
                    Shader.DisableKeyword("_WEIGHTED0");
                    Shader.DisableKeyword("_WEIGHTED1");
                break;
            }

            m_opaqueTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            m_accumTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
            m_revealageTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);

            // First render all opaque objects
            m_transparentCamera.targetTexture = m_opaqueTex;
            m_transparentCamera.backgroundColor = m_camera.backgroundColor;
            m_transparentCamera.clearFlags = m_camera.clearFlags;
            m_transparentCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Transparent"));
            m_transparentCamera.Render();

            // Clear accumTexture to float4(0)
            m_transparentCamera.targetTexture = m_accumTex;
            m_transparentCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            m_transparentCamera.clearFlags = CameraClearFlags.SolidColor;
            m_transparentCamera.cullingMask = 0;
            m_transparentCamera.Render();
            // Render accumTexture
            m_transparentCamera.SetTargetBuffers(m_accumTex.colorBuffer, m_opaqueTex.depthBuffer);
            m_transparentCamera.clearFlags = CameraClearFlags.Nothing;
            m_transparentCamera.cullingMask = 1 << LayerMask.NameToLayer("Transparent");
            m_transparentCamera.RenderWithShader(accumulateShader, null);

            // Clear revealageTex to float4(1)
            m_transparentCamera.targetTexture = m_revealageTex;
            m_transparentCamera.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            m_transparentCamera.clearFlags = CameraClearFlags.SolidColor;
            m_transparentCamera.cullingMask = 0;
            m_transparentCamera.Render();
            // Render revealageTex
            m_transparentCamera.SetTargetBuffers(m_revealageTex.colorBuffer, m_opaqueTex.depthBuffer);
            m_transparentCamera.clearFlags = CameraClearFlags.Nothing;
            m_transparentCamera.cullingMask = 1 << LayerMask.NameToLayer("Transparent");
            m_transparentCamera.RenderWithShader(revealageShader, null);

            m_blendMat.SetTexture("_AccumTex", m_accumTex);
            m_blendMat.SetTexture("_RevealageTex", m_revealageTex);

            Graphics.Blit(m_opaqueTex, dst, m_blendMat);

            RenderTexture.ReleaseTemporary(m_opaqueTex);
            RenderTexture.ReleaseTemporary(m_accumTex);
            RenderTexture.ReleaseTemporary(m_revealageTex);
        }
    }
}
