using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DepthPeelingManager : MonoBehaviour {

    public enum TransparentMode { ODT = 0, DepthPeeling }

    #region Public params
    public Shader initializationShader = null;
    public Shader depthPeelingShader = null;
    public Shader blendShader = null;
    public TransparentMode transparentMode = TransparentMode.ODT;
    [Range(2, 8)]
    public int layers = 4;
    #endregion

    #region Private params
    private Camera m_camera = null;
    private Camera m_transparentCamera = null;
    private GameObject m_transparentCameraObj = null;
    private RenderTexture m_opaqueTex = null;
    private RenderTexture[] m_depthTexs = null;
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
        m_transparentCamera.clearFlags = CameraClearFlags.SolidColor;
        m_transparentCamera.enabled = false;

        m_depthTexs = new RenderTexture[2];
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
            m_opaqueTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            m_depthTexs[0] = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            m_depthTexs[1] = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture[] colorTexs = new RenderTexture[layers];
            colorTexs[0] = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            // First render all opaque objects
            m_transparentCamera.targetTexture = m_opaqueTex;
            m_transparentCamera.backgroundColor = m_camera.backgroundColor;
            m_transparentCamera.clearFlags = m_camera.clearFlags;
            m_transparentCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Transparent"));
            m_transparentCamera.Render();

            // First iteration to render the scene as normal
            RenderBuffer[] mrtBuffers = new RenderBuffer[2];
            mrtBuffers[0] = colorTexs[0].colorBuffer;
            mrtBuffers[1] = m_depthTexs[0].colorBuffer;
            m_transparentCamera.SetTargetBuffers(mrtBuffers, m_opaqueTex.depthBuffer);
            m_transparentCamera.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            m_transparentCamera.clearFlags = CameraClearFlags.Color;
            m_transparentCamera.cullingMask = 1 << LayerMask.NameToLayer("Transparent");
            m_transparentCamera.RenderWithShader(initializationShader, null);

            // Peel away the depth
            for (int i = 1; i < layers; i++) {
                colorTexs[i] = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                mrtBuffers[0] = colorTexs[i].colorBuffer;
                mrtBuffers[1] = m_depthTexs[i%2].colorBuffer;
                m_transparentCamera.SetTargetBuffers(mrtBuffers, m_opaqueTex.depthBuffer);
                m_transparentCamera.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                m_transparentCamera.cullingMask = 1 << LayerMask.NameToLayer("Transparent");
                Shader.SetGlobalTexture("_PrevDepthTex", m_depthTexs[1 - i%2]);
                m_transparentCamera.RenderWithShader(depthPeelingShader, null);
            }

            // Blend all the layers
            RenderTexture colorAccumTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(m_opaqueTex, colorAccumTex);
            for (int i = layers - 1; i >= 0; i--) {
                RenderTexture tmpAccumTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                m_blendMat.SetTexture("_LayerTex", colorTexs[i]);
                Graphics.Blit(colorAccumTex, tmpAccumTex, m_blendMat, 1);
                RenderTexture.ReleaseTemporary(colorAccumTex);
                colorAccumTex = tmpAccumTex;
            }

            Graphics.Blit(colorAccumTex, dst);

            RenderTexture.ReleaseTemporary(colorAccumTex);
            RenderTexture.ReleaseTemporary(m_opaqueTex);
            RenderTexture.ReleaseTemporary(m_depthTexs[0]);
            RenderTexture.ReleaseTemporary(m_depthTexs[1]);
            for (int i = 0; i < layers; i++) {
                RenderTexture.ReleaseTemporary(colorTexs[i]);
            }
        }
    }
}
