using UnityEngine;
using System.Collections;

public class ModelSwapper : MonoBehaviour
{
	public GameObject[] models;

	private void Start()
	{
		if(models == null)
		{
			gameObject.SetActive(false);
			return;
		}
		
		int i = 0;
		foreach (var model in models)
		{
			if(i++ == 0)
			{
				continue;
			}
				
			Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
			foreach (var renderer in renderers)
			{
				renderer.enabled = false;
			}
		}
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0, Screen.height - 25, Screen.width, 25));
		GUILayout.BeginHorizontal();
		foreach (var model in models)
		{
			if(GUILayout.Button(model.name, GUILayout.Width(150)))
			{
				Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
				foreach (var renderer in renderers)
				{
					renderer.enabled = !renderer.enabled;
				}
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}
