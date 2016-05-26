//DEPTH PEELING DEMO OIT
//BY: BRIAN SU

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class DepthPeelEffect : MonoBehaviour
{
	public LayerMask renderLayer;
	public int peelIterations = 1;
	public GameObject[] transparentObjects;

	private float _alphaValue = 0.5f;
	private GameObject _ppCameraGO = null;

	[SerializeField]
	private Shader _depthPeelShader;
	[SerializeField]
	private Shader _colorBufferShader;
	[SerializeField]
	private Shader _renderDepthShader;
	[SerializeField]
	private Material _compositeMaterial;
	private int _passToRender = 0;
	private bool _showPasses = false;
	private bool _toggleOn = true;

	private Camera GetPPCamera()
	{
		if(_ppCameraGO == null)
		{
			_ppCameraGO = new GameObject("Post Processing Camera", typeof(Camera));
			_ppCameraGO.GetComponent<Camera>().enabled = false;
		}
		return _ppCameraGO.GetComponent<Camera>();
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0, 0, 200, 200), GUI.skin.box);
		var toggleOn = GUILayout.Toggle(_toggleOn, " Enabled");
		if(toggleOn != _toggleOn)
		{
			_toggleOn = toggleOn;
			if(!_toggleOn)
			{
				foreach (GameObject go in transparentObjects)
				{
					Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
					foreach (Renderer renderer in renderers)
					{
						Color color = renderer.material.color;
						color.a = _alphaValue;
						renderer.material.shader = Shader.Find("Transparent/Bumped Diffuse");
					}
				}
			} else
			{
				foreach (GameObject go in transparentObjects)
				{
					Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
					foreach (Renderer renderer in renderers)
					{
						Color color = renderer.material.color;
						color.a = _alphaValue;
						renderer.material.shader = Shader.Find("Bumped Diffuse");
					}
				}
			}
		}

		GUILayout.Space(10);

		GUILayout.Label("Opacity");
		float alphaValue = GUILayout.HorizontalSlider(_alphaValue, 0, 1);

		if(alphaValue != _alphaValue)
		{
			_alphaValue = alphaValue;
			foreach (GameObject go in transparentObjects)
			{
				Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in renderers)
				{
					Color color = renderer.material.color;
					color.a = _alphaValue;
					renderer.material.color = color;
				}
			}
		}
		
		GUILayout.Label("Passes " + peelIterations, GUILayout.Width(75));
		peelIterations = (int)GUILayout.HorizontalSlider(peelIterations, 2, 15);

		GUILayout.Space(10);
		_showPasses = GUILayout.Toggle(_showPasses, " Show Passes");
		if(_showPasses)
		{
			_passToRender = Mathf.Min(_passToRender, peelIterations - 1);
			GUILayout.Label("Pass " + (_passToRender + 1), GUILayout.Width(50));
			_passToRender = (int)GUILayout.HorizontalSlider(_passToRender, 0, peelIterations);
		}
		GUILayout.EndArea();
	}


	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if(!_toggleOn)
		{
			Graphics.Blit(source, destination);
			return;
		}
		
		peelIterations = Mathf.Max(2, peelIterations);
		
		var depthBufferA = RenderTexture.GetTemporary(source.width, source.height, 1);
		var depthBufferB = RenderTexture.GetTemporary(source.width, source.height, 1);
		
		var ppCamera = GetPPCamera();
		ppCamera.CopyFrom(GetComponent<Camera>());
		ppCamera.cullingMask = renderLayer;
		ppCamera.renderingPath = RenderingPath.Forward;
		
		RenderTexture[] colorBuffers = new RenderTexture[peelIterations];
		colorBuffers[0] = RenderTexture.GetTemporary(source.width, source.height, 1);
		
		// Render the first pass normally
		ppCamera.backgroundColor = Color.white;
		ppCamera.targetTexture = depthBufferA;
		ppCamera.RenderWithShader(_renderDepthShader, "");
		RenderColor(ppCamera, depthBufferA, colorBuffers[0]);
		
		// Peel away the depth
		bool evenOdd = true;
		for(int i = 1; i < peelIterations; i++)
		{
			colorBuffers[i] = RenderTexture.GetTemporary(source.width, source.height, 1);
			ppCamera.backgroundColor = Color.white;
			Shader.SetGlobalTexture("_DepthTex", (evenOdd) ? depthBufferA : depthBufferB);
			ppCamera.targetTexture = (evenOdd) ? depthBufferB : depthBufferA;
			ppCamera.RenderWithShader(_depthPeelShader, "");
			RenderColor(ppCamera, (evenOdd) ? depthBufferB : depthBufferA, colorBuffers[i]);
			evenOdd = !evenOdd;
		}
		
		RenderTexture colorBufferAccum = RenderTexture.GetTemporary(source.width, source.height, 0);
		RenderTexture.active = colorBufferAccum;
		GL.Clear(true, true, Color.black);

		Graphics.Blit(colorBuffers[peelIterations - 1], colorBufferAccum);
		for(int i = peelIterations - 2; i >= 0; i--)
		{
			_compositeMaterial.SetTexture("_ColorBuffer", colorBuffers[i]);
			Graphics.Blit(colorBufferAccum, colorBufferAccum, _compositeMaterial);
		}
		
		if(_showPasses)
		{
			Graphics.Blit(colorBuffers[Mathf.Min(_passToRender, peelIterations - 1)], destination);
		} 
		else
		{
			Graphics.Blit(colorBufferAccum, destination);
		}

		for(int i = 0; i < peelIterations; i++)
		{
			RenderTexture.ReleaseTemporary(colorBuffers[i]);
		}

		RenderTexture.ReleaseTemporary(depthBufferA);
		RenderTexture.ReleaseTemporary(depthBufferB);
		RenderTexture.ReleaseTemporary(colorBufferAccum);
	}

	private void RenderColor(Camera ppCamera, RenderTexture depthBuffer, RenderTexture colorBuffer)
	{
		ppCamera.backgroundColor = Color.black;
		Shader.SetGlobalTexture("_DepthTex", depthBuffer);
		ppCamera.targetTexture = colorBuffer;
		ppCamera.RenderWithShader(_colorBufferShader, "");
	}
}
