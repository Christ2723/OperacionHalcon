using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

//Retratos de los personajes (para la hoja de personajes que se envía a aprobación)
public static class Capturas {

	const string CARPETA = "Documentacion/personajes";

	[MenuItem("Halcon/Retratos de personajes")]
	public static void Personajes() {
		Directory.CreateDirectory(CARPETA);
		Retrato("Soldado", "soldado", 30f);
		Retrato("Dron", "dron", 25f);
		Retrato("Centinela", "centinela", 30f);
		Retrato("Coloso", "coloso", 30f);
		Debug.Log("Capturas: retratos listos");
		if (Application.isBatchMode) EditorApplication.Exit(0);
	}

	static void Retrato(string prefab, string nombre, float giro) {
		EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		RenderSettings.ambientMode = AmbientMode.Trilight;
		RenderSettings.ambientSkyColor = new Color(0.55f, 0.6f, 0.7f);
		RenderSettings.ambientEquatorColor = new Color(0.45f, 0.42f, 0.4f);
		RenderSettings.ambientGroundColor = new Color(0.2f, 0.18f, 0.16f);
		RenderSettings.skybox = null;

		var sol = new GameObject("Sol").AddComponent<Light>();
		sol.type = LightType.Directional;
		sol.intensity = 1.3f;
		sol.color = new Color(1f, 0.95f, 0.88f);
		sol.shadows = LightShadows.Soft;
		sol.transform.rotation = Quaternion.Euler(35f, 150f, 0f);
		var contra = new GameObject("Contraluz").AddComponent<Light>();
		contra.type = LightType.Directional;
		contra.intensity = 0.7f;
		contra.color = new Color(1f, 0.7f, 0.35f);
		contra.transform.rotation = Quaternion.Euler(20f, -30f, 0f);

		var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + prefab + ".prefab"));
		go.transform.rotation = Quaternion.Euler(0f, 180f + giro, 0f);
		var limites = new Bounds(go.transform.position, Vector3.zero);
		foreach (var r in go.GetComponentsInChildren<Renderer>()) limites.Encapsulate(r.bounds);

		var piso = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		piso.transform.position = new Vector3(limites.center.x, limites.min.y - 0.02f, limites.center.z);
		piso.transform.localScale = new Vector3(limites.size.magnitude * 1.1f, 0.01f, limites.size.magnitude * 1.1f);
		piso.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materiales/ConcretoOscuro.mat");

		var cam = new GameObject("Camara").AddComponent<Camera>();
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = new Color(0.11f, 0.13f, 0.16f);
		cam.fieldOfView = 30f;
		float alto = Mathf.Max(limites.size.y, limites.size.x * 1.1f);
		float distancia = alto * 0.62f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
		Vector3 centro = limites.center;
		cam.transform.position = centro + new Vector3(0f, alto * 0.18f, -distancia);
		cam.transform.LookAt(centro);
		cam.farClipPlane = distancia * 4f;

		const int ANCHO = 800, ALTO = 1000;
		var rt = new RenderTexture(ANCHO, ALTO, 24) { antiAliasing = 8 };
		cam.aspect = ANCHO / (float)ALTO;
		cam.targetTexture = rt;
		cam.Render();
		RenderTexture.active = rt;
		var tex = new Texture2D(ANCHO, ALTO, TextureFormat.RGB24, false);
		tex.ReadPixels(new Rect(0, 0, ANCHO, ALTO), 0, 0);
		tex.Apply();
		File.WriteAllBytes(CARPETA + "/" + nombre + ".png", tex.EncodeToPNG());
		Object.DestroyImmediate(tex);
		RenderTexture.active = null;
		cam.targetTexture = null;
		rt.Release();
	}
}
