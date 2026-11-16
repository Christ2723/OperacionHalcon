using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

//Construye automáticamente todo el juego "Operación Halcón": texturas, materiales, sonidos, música,
//modelos (soldado, robots, jefe, dianas), los 3 niveles, el menú de entrada y la pantalla final.
//Se ejecuta desde el menú "Halcon" de Unity o por línea de comandos (-executeMethod).
public static partial class ConstructorProyecto {

	const string NOMBRE = "Christopher Payero Estrella";
	const string MATRICULA = "1-21-0909";
	const string TITULO = "OPERACIÓN HALCÓN";

	const int CAPA_ARMA = 12;

	static readonly string[] escenas = {
		Juego.ESCENA_MENU, Juego.ESCENA_NIVEL1, Juego.ESCENA_NIVEL2, Juego.ESCENA_NIVEL3, Juego.ESCENA_FIN
	};

	static Font fuente;

	[MenuItem("Halcon/Construir todo")]
	public static void ConstruirTodo() {
		foreach (var carpeta in new[] { "Scenes", "Prefabs", "Texturas", "Materiales", "Audio", "Resources", "Sprites" }) {
			if (!AssetDatabase.IsValidFolder("Assets/" + carpeta)) AssetDatabase.CreateFolder("Assets", carpeta);
		}
		fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		ConfigurarCapas();
		ConfigurarEjes();
		CrearTexturas();
		CrearMateriales();
		CrearSprites();
		CrearJuegoPrefab();
		CrearPrefabs();

		CrearMenu();
		CrearNivel1();
		CrearNivel2();
		CrearNivel3();
		CrearFin();

		EditorBuildSettings.scenes = escenas.Select(e => new EditorBuildSettingsScene("Assets/Scenes/" + e + ".unity", true)).ToArray();

		PlayerSettings.companyName = NOMBRE;
		PlayerSettings.productName = "Operacion Halcon";
		PlayerSettings.defaultScreenWidth = 1280;
		PlayerSettings.defaultScreenHeight = 720;
		PlayerSettings.defaultWebScreenWidth = 1280;
		PlayerSettings.defaultWebScreenHeight = 720;
		PlayerSettings.runInBackground = true;
		PlayerSettings.colorSpace = ColorSpace.Linear;
		PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
		PlayerSettings.WebGL.decompressionFallback = false;
		PlayerSettings.SplashScreen.show = false;
		ConfigurarCalidad();

		AssetDatabase.SaveAssets();
		Debug.Log("ConstructorProyecto: todo construido");
	}

	[MenuItem("Halcon/Compilar WebGL")]
	public static void CompilarWebGL() => Compilar(BuildTarget.WebGL, "Builds/WebGL");

	[MenuItem("Halcon/Compilar Windows")]
	public static void CompilarWindows() => Compilar(BuildTarget.StandaloneWindows64, "Builds/Windows/OperacionHalcon.exe");

	static void Compilar(BuildTarget destino, string ruta) {
		var opciones = new BuildPlayerOptions {
			scenes = escenas.Select(e => "Assets/Scenes/" + e + ".unity").ToArray(),
			locationPathName = ruta,
			target = destino,
			options = BuildOptions.None
		};
		var reporte = BuildPipeline.BuildPlayer(opciones);
		Debug.Log("ConstructorProyecto: build " + destino + " " + reporte.summary.result + " " + reporte.summary.totalSize / (1024 * 1024) + " MB");
		if (reporte.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded && Application.isBatchMode) EditorApplication.Exit(1);
	}

	// ------------------------------------------------ Configuración del proyecto

	static void ConfigurarCapas() {
		var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		var capas = tagManager.FindProperty("layers");
		for (int i = 8; i < 32; i++) capas.GetArrayElementAtIndex(i).stringValue = "";
		capas.GetArrayElementAtIndex(Capas.Jugador).stringValue = "Jugador";
		capas.GetArrayElementAtIndex(Capas.Enemigos).stringValue = "Enemigos";
		capas.GetArrayElementAtIndex(Capas.Proyectil).stringValue = "Proyectil";
		capas.GetArrayElementAtIndex(Capas.Objetivos).stringValue = "Objetivos";
		capas.GetArrayElementAtIndex(CAPA_ARMA).stringValue = "Arma";
		tagManager.ApplyModifiedProperties();
		AssetDatabase.SaveAssets();
	}

	static void ConfigurarEjes() {
		var inputManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
		var ejes = inputManager.FindProperty("m_Axes");
		ejes.ClearArray();
		void Eje(string nombre, string negativo, string positivo, string altNegativo, string altPositivo, int tipo = 0, int eje = 0, float sensibilidad = 1000f) {
			ejes.arraySize++;
			var e = ejes.GetArrayElementAtIndex(ejes.arraySize - 1);
			e.FindPropertyRelative("m_Name").stringValue = nombre;
			e.FindPropertyRelative("descriptiveName").stringValue = "";
			e.FindPropertyRelative("descriptiveNegativeName").stringValue = "";
			e.FindPropertyRelative("negativeButton").stringValue = negativo;
			e.FindPropertyRelative("positiveButton").stringValue = positivo;
			e.FindPropertyRelative("altNegativeButton").stringValue = altNegativo;
			e.FindPropertyRelative("altPositiveButton").stringValue = altPositivo;
			e.FindPropertyRelative("gravity").floatValue = tipo == 0 ? 1000f : 0f;
			e.FindPropertyRelative("dead").floatValue = tipo == 0 ? 0.001f : 0f;
			e.FindPropertyRelative("sensitivity").floatValue = sensibilidad;
			e.FindPropertyRelative("snap").boolValue = tipo == 0;
			e.FindPropertyRelative("invert").boolValue = false;
			e.FindPropertyRelative("type").intValue = tipo;
			e.FindPropertyRelative("axis").intValue = eje;
			e.FindPropertyRelative("joyNum").intValue = 0;
		}
		Eje("Horizontal", "left", "right", "a", "d");
		Eje("Vertical", "down", "up", "s", "w");
		Eje("Mouse X", "", "", "", "", 1, 0, 0.1f);
		Eje("Mouse Y", "", "", "", "", 1, 1, 0.1f);
		Eje("Mouse ScrollWheel", "", "", "", "", 1, 2, 0.1f);
		Eje("Submit", "", "return", "", "enter");
		Eje("Cancel", "", "escape", "", "");
		inputManager.ApplyModifiedProperties();
	}

	//Sombras visibles en todas las calidades (también en el navegador)
	static void ConfigurarCalidad() {
		var nombres = QualitySettings.names;
		int actual = QualitySettings.GetQualityLevel();
		for (int i = 0; i < nombres.Length; i++) {
			QualitySettings.SetQualityLevel(i, false);
			QualitySettings.shadows = ShadowQuality.All;
			QualitySettings.shadowResolution = i >= 3 ? ShadowResolution.High : ShadowResolution.Medium;
			QualitySettings.shadowDistance = 70f;
			QualitySettings.shadowCascades = i >= 3 ? 2 : 1;
			QualitySettings.pixelLightCount = Mathf.Max(QualitySettings.pixelLightCount, 3);
			QualitySettings.antiAliasing = i >= 2 ? 4 : 0;
			QualitySettings.vSyncCount = 1;
		}
		QualitySettings.SetQualityLevel(actual, false);
	}

	// ------------------------------------------------ Utilidades de objetos

	static GameObject Vacio(string nombre, Transform padre, Vector3 pos, int capa = 0) {
		var go = new GameObject(nombre);
		go.layer = capa;
		if (padre != null) go.transform.SetParent(padre, false);
		go.transform.localPosition = pos;
		return go;
	}

	//Primitiva con material. colision: 0 = sin colisionador, 1 = el de la primitiva, 2 = de malla (exacto)
	static GameObject Pieza(PrimitiveType tipo, string nombre, Transform padre, Vector3 pos, Vector3 escala, Material mat,
		Vector3? rot = null, int colision = 1, int capa = -1) {
		var go = GameObject.CreatePrimitive(tipo);
		go.name = nombre;
		if (padre != null) go.transform.SetParent(padre, false);
		go.transform.localPosition = pos;
		go.transform.localScale = escala;
		go.transform.localRotation = Quaternion.Euler(rot ?? Vector3.zero);
		go.layer = capa >= 0 ? capa : (padre != null ? padre.gameObject.layer : 0);
		go.GetComponent<Renderer>().sharedMaterial = mat;
		var col = go.GetComponent<Collider>();
		if (colision == 0) {
			Object.DestroyImmediate(col);
		} else if (colision == 2) {
			var malla = go.GetComponent<MeshFilter>().sharedMesh;
			Object.DestroyImmediate(col);
			go.AddComponent<MeshCollider>().sharedMesh = malla;
		}
		return go;
	}

	static GameObject Cubo(string n, Transform p, Vector3 pos, Vector3 esc, Material m, Vector3? rot = null, int col = 1) =>
		Pieza(PrimitiveType.Cube, n, p, pos, esc, m, rot, col);

	static GameObject Cilindro(string n, Transform p, Vector3 pos, Vector3 esc, Material m, Vector3? rot = null, int col = 1) =>
		Pieza(PrimitiveType.Cylinder, n, p, pos, esc, m, rot, col);

	static GameObject Esfera(string n, Transform p, Vector3 pos, Vector3 esc, Material m, int col = 1) =>
		Pieza(PrimitiveType.Sphere, n, p, pos, esc, m, null, col);

	static GameObject Capsula(string n, Transform p, Vector3 pos, Vector3 esc, Material m, Vector3? rot = null, int col = 1) =>
		Pieza(PrimitiveType.Capsule, n, p, pos, esc, m, rot, col);

	static void Capa(GameObject go, int capa) {
		foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = capa;
	}

	static void SinSombras(GameObject go) {
		foreach (var r in go.GetComponentsInChildren<Renderer>(true)) {
			r.shadowCastingMode = ShadowCastingMode.Off;
			r.receiveShadows = false;
		}
	}

	static GameObject Guardar(GameObject go, string nombre) {
		var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/" + nombre + ".prefab");
		Object.DestroyImmediate(go);
		return prefab;
	}

	static GameObject Poner(GameObject prefab, Vector3 pos, float giro, Transform padre, float escala = 1f) {
		var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, padre);
		go.transform.position = pos;
		go.transform.rotation = Quaternion.Euler(0f, giro, 0f);
		if (escala != 1f) go.transform.localScale *= escala;
		return go;
	}

	//Gira un objeto para que su cara frontal (-Z) mire hacia un punto
	static void MirarHacia(GameObject go, Vector3 punto) {
		Vector3 d = go.transform.position - punto;
		d.y = 0f;
		if (d.sqrMagnitude > 0.001f) go.transform.rotation = Quaternion.LookRotation(d);
	}

	//Guarda la escena activa registrando los cambios hechos a las instancias de prefabs
	static void GuardarEscena(string nombre) {
		var escena = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		foreach (var raiz in escena.GetRootGameObjects()) {
			foreach (var t in raiz.GetComponentsInChildren<Transform>(true)) {
				foreach (var comp in t.GetComponents<Component>()) {
					if (comp != null && PrefabUtility.IsPartOfPrefabInstance(comp)) PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
				}
			}
		}
		EditorSceneManager.SaveScene(escena, "Assets/Scenes/" + nombre + ".unity");
	}
}
