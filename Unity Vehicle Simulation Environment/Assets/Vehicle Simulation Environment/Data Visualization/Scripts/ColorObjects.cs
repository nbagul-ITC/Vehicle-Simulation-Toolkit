//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class ColorObjects : MonoBehaviour {
	public List<string> tags;
	public List<Color> colors;
	public Color skyboxColor = Color.black;

	void Start () {
		Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();
		cam.clearFlags = CameraClearFlags.SolidColor;
		RenderSettings.skybox.color = skyboxColor;
		foreach (String tag in tags) {
			GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
			foreach (GameObject obj in objects) {
				foreach (MeshRenderer mesher in obj.GetComponentsInChildren<MeshRenderer>()) {
					foreach (Material mat in mesher.materials) {
						mat.shader = Shader.Find("GUI/Text Shader");;
						mat.color = colors[tags.IndexOf(tag)];
						mesher.receiveShadows = false;
					}
				}
				foreach (SkinnedMeshRenderer mesher in obj.GetComponentsInChildren<SkinnedMeshRenderer>()) {
					foreach (Material mat in mesher.materials) {
						mat.shader = Shader.Find("GUI/Text Shader");;
						mat.color = colors[tags.IndexOf(tag)];
						mesher.receiveShadows = false;
					}
				}
			}
		}
	}

}
