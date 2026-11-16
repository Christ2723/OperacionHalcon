using System;
using System.IO;
using UnityEditor;
using UnityEngine;

//Texturas generadas por código (pasto, tierra, concreto, madera, metal, camuflaje...) y todos los materiales
public static partial class ConstructorProyecto {

	static Texture2D tPasto, tTierra, tConcreto, tMadera, tCaja, tMetal, tCamuflaje, tSaco, tLona, tRayas, tRoca, tPlacas, tSuave, tEstrella;

	//Materiales del escenario
	static Material mPasto, mTierra, mConcreto, mConcretoOscuro, mMadera, mMaderaOscura, mCaja, mMetal, mMetalOscuro, mCamuflaje,
		mSaco, mLona, mRayas, mRoca, mRocaOscura, mPlacas, mHoja, mHojaOscura, mTronco, mMontana, mBandera, mBanderaRoja, mContenedor,
		mContenedorAzul, mBarril, mBarrilRojo, mInvisible, mAgua, mLava;
	//Dianas y objetivos
	static Material mDianaBlanca, mDianaRoja, mDianaAmarilla, mDianaAzul, mDianaNegra, mVidrio, mGlobo, mGloboAzul, mCuerda, mAcero, mCarton, mCivil;
	//Personajes
	static Material mPiel, mUniforme, mChaleco, mCasco, mBotas, mGuantes, mGafas, mArma, mArmaClaro;
	static Material mRobot, mRobotOscuro, mRobotClaro, mOjoRojo, mNaranja, mRotor, mNucleo, mNucleoFurioso, mColoso, mColosoOscuro, mVisor;
	//Recogibles y efectos
	static Material mMunicion, mLaton, mBotiquin, mCruz, mBrilloAmarillo, mBrilloVerde, mTrazadora, mDestelloBoca, mLuzRoja;
	static Material mFuego, mHumo, mGolpe, mRestos, mEstela;
	static Material cieloDia, cieloTarde, cieloNoche, cieloMenu;

	// ------------------------------------------------ Texturas

	static float Ruido(float x, float y, int semilla) => Mathf.PerlinNoise(x + semilla * 17.31f, y + semilla * 9.73f);

	static float Fractal(float x, float y, int semilla, int octavas = 4) {
		float v = 0f, a = 0.5f, f = 1f;
		for (int i = 0; i < octavas; i++) {
			v += Ruido(x * f, y * f, semilla + i) * a;
			a *= 0.5f;
			f *= 2f;
		}
		return v;
	}

	static Texture2D Textura(string nombre, int tam, Func<int, int, Color> pixel, bool repetir = true) {
		var tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
		var px = new Color[tam * tam];
		for (int y = 0; y < tam; y++)
			for (int x = 0; x < tam; x++)
				px[y * tam + x] = pixel(x, y);
		tex.SetPixels(px);
		tex.Apply();
		string ruta = "Assets/Texturas/" + nombre + ".png";
		File.WriteAllBytes(ruta, tex.EncodeToPNG());
		UnityEngine.Object.DestroyImmediate(tex);
		AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
		var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
		imp.wrapMode = repetir ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
		imp.mipmapEnabled = true;
		imp.anisoLevel = 4;
		imp.alphaIsTransparency = true;
		imp.maxTextureSize = 512;
		imp.SaveAndReimport();
		return AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
	}

	static Color Mezcla(Color a, Color b, float t) => Color.Lerp(a, b, Mathf.Clamp01(t));

	static void CrearTexturas() {
		const int T = 256;
		//Coordenadas que se repiten sin costuras: el ruido se toma sobre un toro
		float Periodico(int x, int y, float escala, int semilla, int oct = 4) {
			float u = x / (float)T * Mathf.PI * 2f, v = y / (float)T * Mathf.PI * 2f;
			float r = escala / (Mathf.PI * 2f);
			float a = Fractal(Mathf.Cos(u) * r + 50f, Mathf.Sin(u) * r + Mathf.Cos(v) * r * 0.5f + 50f, semilla, oct);
			float b = Fractal(Mathf.Sin(v) * r + 80f, Mathf.Cos(v) * r + Mathf.Sin(u) * r * 0.5f + 80f, semilla + 5, oct);
			return (a + b) * 0.5f;
		}

		tPasto = Textura("Pasto", T, (x, y) => {
			float n = Periodico(x, y, 6f, 1), d = Periodico(x, y, 40f, 2, 2);
			var c = Mezcla(new Color(0.21f, 0.33f, 0.12f), new Color(0.42f, 0.52f, 0.22f), n * 1.4f - 0.2f);
			return Mezcla(c, new Color(0.5f, 0.45f, 0.25f), (d - 0.62f) * 3f);
		});
		tTierra = Textura("Tierra", T, (x, y) => {
			float n = Periodico(x, y, 8f, 3), p = Periodico(x, y, 50f, 4, 2);
			var c = Mezcla(new Color(0.36f, 0.27f, 0.18f), new Color(0.56f, 0.45f, 0.31f), n * 1.3f - 0.15f);
			return Mezcla(c, new Color(0.25f, 0.2f, 0.15f), (p - 0.64f) * 4f);
		});
		tConcreto = Textura("Concreto", T, (x, y) => {
			float n = Periodico(x, y, 10f, 5), m = Periodico(x, y, 60f, 6, 2);
			float g = 0.55f + (n - 0.5f) * 0.25f + (m - 0.5f) * 0.12f;
			bool junta = x % 128 < 2 || y % 128 < 2;
			if (junta) g *= 0.6f;
			return new Color(g, g * 0.98f, g * 0.95f);
		});
		tMadera = Textura("Madera", T, (x, y) => {
			int tabla = y / 32;
			float veta = Mathf.Sin((x * 0.06f + Periodico(x, y, 4f, 7) * 6f + tabla * 3f)) * 0.5f + 0.5f;
			var c = Mezcla(new Color(0.45f, 0.3f, 0.16f), new Color(0.62f, 0.45f, 0.26f), veta * 0.7f + tabla % 3 * 0.12f);
			if (y % 32 < 2) c *= 0.55f;
			return c;
		});
		tCaja = Textura("Caja", T, (x, y) => {
			float veta = Mathf.Sin(x * 0.08f + Periodico(x, y, 3f, 8) * 5f) * 0.5f + 0.5f;
			var c = Mezcla(new Color(0.5f, 0.36f, 0.2f), new Color(0.68f, 0.5f, 0.3f), veta * 0.6f + 0.2f);
			bool marco = x < 22 || x > T - 23 || y < 22 || y > T - 23;
			bool diagonal = Mathf.Abs(x - y) < 14 || Mathf.Abs(x - (T - y)) < 14;
			if (marco || diagonal) c = Mezcla(new Color(0.38f, 0.26f, 0.14f), new Color(0.5f, 0.36f, 0.2f), veta);
			if (y % 64 < 2 && !marco) c *= 0.7f;
			if (marco && (x == 22 || x == T - 23 || y == 22 || y == T - 23)) c *= 0.5f;
			return c;
		}, false);
		tMetal = Textura("Metal", T, (x, y) => {
			float n = Periodico(x, y, 12f, 9), r = Periodico(x, y, 70f, 10, 2);
			float g = 0.5f + (n - 0.5f) * 0.18f;
			var c = new Color(g, g * 1.02f, g * 1.06f);
			if (x % 128 < 2 || y % 128 < 2) c *= 0.55f;
			int rx = x % 128, ry = y % 128;
			if ((rx - 10) * (rx - 10) + (ry - 10) * (ry - 10) < 12) c *= 1.3f;
			return Mezcla(c, new Color(0.4f, 0.27f, 0.17f), (r - 0.66f) * 3f);
		});
		tPlacas = Textura("Placas", T, (x, y) => {
			float n = Periodico(x, y, 9f, 11);
			float g = 0.32f + (n - 0.5f) * 0.12f;
			var c = new Color(g, g, g * 1.05f);
			bool linea = x % 64 < 2 || y % 64 < 2;
			if (linea) c *= 0.5f;
			bool raya = (x + y) % 16 < 2 && (x / 64 + y / 64) % 2 == 0;
			if (raya) c *= 1.15f;
			return c;
		});
		tCamuflaje = Textura("Camuflaje", T, (x, y) => {
			float a = Periodico(x, y, 5f, 12, 3), b = Periodico(x, y, 7f, 13, 3);
			if (a > 0.58f) return new Color(0.2f, 0.22f, 0.13f);
			if (b > 0.56f) return new Color(0.42f, 0.36f, 0.24f);
			if (a < 0.4f) return new Color(0.28f, 0.33f, 0.18f);
			return new Color(0.36f, 0.4f, 0.25f);
		});
		tSaco = Textura("Saco", T, (x, y) => {
			float tejido = ((x / 3 + y / 3) % 2 == 0 ? 0.04f : -0.04f);
			float n = Periodico(x, y, 10f, 14);
			float g = 0.55f + (n - 0.5f) * 0.2f + tejido;
			return new Color(g * 1.05f, g * 0.95f, g * 0.72f);
		});
		tLona = Textura("Lona", T, (x, y) => {
			float n = Periodico(x, y, 6f, 15);
			float g = 0.4f + (n - 0.5f) * 0.15f + ((x % 4 < 2) ? 0.02f : -0.02f);
			return new Color(g * 0.9f, g, g * 0.65f);
		});
		tRayas = Textura("Rayas", 128, (x, y) => ((x + y) / 32) % 2 == 0 ? new Color(0.95f, 0.75f, 0.1f) : new Color(0.1f, 0.1f, 0.1f));
		tRoca = Textura("Roca", T, (x, y) => {
			float n = Periodico(x, y, 7f, 16, 5), g2 = Periodico(x, y, 30f, 17, 3);
			float g = 0.38f + (n - 0.5f) * 0.5f;
			var c = new Color(g, g * 0.97f, g * 0.93f);
			if (g2 > 0.62f) c *= 0.75f;
			return c;
		});
		//Partícula suave y destello en estrella
		tSuave = Textura("Suave", 64, (x, y) => {
			float d = new Vector2(x - 31.5f, y - 31.5f).magnitude / 32f;
			float a = Mathf.Clamp01(1f - d);
			return new Color(1f, 1f, 1f, a * a);
		}, false);
		tEstrella = Textura("Estrella", 128, (x, y) => {
			Vector2 p = new Vector2(x - 63.5f, y - 63.5f) / 64f;
			float d = p.magnitude;
			float rayos = Mathf.Pow(Mathf.Abs(Mathf.Cos(Mathf.Atan2(p.y, p.x) * 3f)), 8f);
			float a = Mathf.Clamp01((1f - d) * (0.35f + rayos)) + Mathf.Clamp01(1f - d * 3f);
			return new Color(1f, 1f, 1f, Mathf.Clamp01(a));
		}, false);
	}

	// ------------------------------------------------ Materiales

	static Material Mat(string nombre, Color color, Texture2D tex = null, float repetir = 1f, float metal = 0f, float brillo = 0.2f, Color? emision = null,
		string carpeta = "Assets/Materiales/") {
		string ruta = carpeta + nombre + ".mat";
		var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
		if (m == null) {
			m = new Material(Shader.Find("Standard"));
			AssetDatabase.CreateAsset(m, ruta);
		}
		m.shader = Shader.Find("Standard");
		m.color = color;
		m.mainTexture = tex;
		m.mainTextureScale = Vector2.one * repetir;
		m.SetFloat("_Metallic", metal);
		m.SetFloat("_Glossiness", brillo);
		if (emision.HasValue) {
			m.EnableKeyword("_EMISSION");
			m.SetColor("_EmissionColor", emision.Value);
			m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
		} else {
			m.DisableKeyword("_EMISSION");
			m.SetColor("_EmissionColor", Color.black);
		}
		m.enableInstancing = true;
		EditorUtility.SetDirty(m);
		return m;
	}

	static Material Particulas(string nombre, bool aditivo, Texture2D tex) {
		string ruta = "Assets/Resources/" + nombre + ".mat";
		var sh = Shader.Find(aditivo ? "Legacy Shaders/Particles/Additive" : "Legacy Shaders/Particles/Alpha Blended");
		if (sh == null) sh = Shader.Find("Particles/Standard Unlit");
		var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
		if (m == null) {
			m = new Material(sh);
			AssetDatabase.CreateAsset(m, ruta);
		}
		m.shader = sh;
		m.mainTexture = tex;
		if (m.HasProperty("_TintColor")) m.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));
		EditorUtility.SetDirty(m);
		return m;
	}

	static Material Cielo(string nombre, Color tinte, Color suelo, float exposicion, float atmosfera, float sol) {
		string ruta = "Assets/Materiales/" + nombre + ".mat";
		var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
		var sh = Shader.Find("Skybox/Procedural");
		if (m == null) {
			m = new Material(sh);
			AssetDatabase.CreateAsset(m, ruta);
		}
		m.shader = sh;
		m.SetColor("_SkyTint", tinte);
		m.SetColor("_GroundColor", suelo);
		m.SetFloat("_Exposure", exposicion);
		m.SetFloat("_AtmosphereThickness", atmosfera);
		m.SetFloat("_SunSize", sol);
		m.SetFloat("_SunSizeConvergence", 6f);
		EditorUtility.SetDirty(m);
		return m;
	}

	static void CrearMateriales() {
		mPasto = Mat("Pasto", Color.white, tPasto, 18f);
		mTierra = Mat("Tierra", Color.white, tTierra, 14f);
		mConcreto = Mat("Concreto", Color.white, tConcreto, 2f, 0f, 0.15f);
		mConcretoOscuro = Mat("ConcretoOscuro", new Color(0.55f, 0.55f, 0.58f), tConcreto, 2f, 0f, 0.15f);
		mMadera = Mat("Madera", Color.white, tMadera, 1f, 0f, 0.15f);
		mMaderaOscura = Mat("MaderaOscura", new Color(0.55f, 0.45f, 0.4f), tMadera, 1f, 0f, 0.1f);
		mCaja = Mat("Caja", Color.white, tCaja, 1f, 0f, 0.12f);
		mMetal = Mat("Metal", new Color(0.8f, 0.82f, 0.85f), tMetal, 1f, 0.35f, 0.4f);
		mMetalOscuro = Mat("MetalOscuro", new Color(0.35f, 0.37f, 0.4f), tMetal, 1f, 0.4f, 0.45f);
		mCamuflaje = Mat("Camuflaje", Color.white, tCamuflaje, 1f, 0f, 0.15f);
		mSaco = Mat("Saco", Color.white, tSaco, 1f, 0f, 0.08f);
		mLona = Mat("Lona", Color.white, tLona, 2f, 0f, 0.1f);
		mRayas = Mat("Rayas", Color.white, tRayas, 1f, 0f, 0.3f);
		mRoca = Mat("Roca", Color.white, tRoca, 2f, 0f, 0.15f);
		mRocaOscura = Mat("RocaOscura", new Color(0.55f, 0.47f, 0.47f), tRoca, 3f, 0f, 0.2f);
		mPlacas = Mat("Placas", Color.white, tPlacas, 12f, 0.3f, 0.35f);
		mHoja = Mat("Hoja", new Color(0.2f, 0.36f, 0.14f), tPasto, 1f, 0f, 0.1f);
		mHojaOscura = Mat("HojaOscura", new Color(0.13f, 0.25f, 0.11f), tPasto, 1f, 0f, 0.1f);
		mTronco = Mat("Tronco", new Color(0.35f, 0.24f, 0.15f), tMadera, 1f, 0f, 0.05f);
		mMontana = Mat("Montana", new Color(0.42f, 0.48f, 0.42f), tRoca, 4f, 0f, 0.05f);
		mBandera = Mat("Bandera", new Color(0.15f, 0.3f, 0.6f), null, 1f, 0f, 0.1f);
		mBanderaRoja = Mat("BanderaRoja", new Color(0.7f, 0.12f, 0.1f), null, 1f, 0f, 0.1f);
		mContenedor = Mat("Contenedor", new Color(0.65f, 0.28f, 0.18f), tMetal, 1f, 0.3f, 0.35f);
		mContenedorAzul = Mat("ContenedorAzul", new Color(0.2f, 0.35f, 0.55f), tMetal, 1f, 0.3f, 0.35f);
		mBarril = Mat("Barril", new Color(0.3f, 0.36f, 0.22f), tMetal, 1f, 0.3f, 0.4f);
		mBarrilRojo = Mat("BarrilRojo", new Color(0.65f, 0.14f, 0.1f), tMetal, 1f, 0.3f, 0.45f);
		mInvisible = Mat("Invisible", new Color(1f, 1f, 1f, 0f));
		mAgua = Mat("Agua", new Color(0.12f, 0.25f, 0.32f), null, 1f, 0.2f, 0.9f);
		mLava = Mat("Lava", new Color(0.3f, 0.05f, 0.02f), tRoca, 3f, 0f, 0.4f, new Color(1.6f, 0.35f, 0.05f));

		mDianaBlanca = Mat("DianaBlanca", new Color(0.95f, 0.94f, 0.9f), null, 1f, 0f, 0.25f);
		mDianaRoja = Mat("DianaRoja", new Color(0.8f, 0.1f, 0.08f), null, 1f, 0f, 0.3f);
		mDianaAmarilla = Mat("DianaAmarilla", new Color(1f, 0.78f, 0.1f), null, 1f, 0.1f, 0.5f, new Color(0.25f, 0.18f, 0f));
		mDianaAzul = Mat("DianaAzul", new Color(0.12f, 0.35f, 0.85f), null, 1f, 0f, 0.3f);
		mDianaNegra = Mat("DianaNegra", new Color(0.08f, 0.08f, 0.08f), null, 1f, 0f, 0.2f);
		mVidrio = Mat("Vidrio", new Color(0.15f, 0.55f, 0.25f), null, 1f, 0.1f, 0.95f, new Color(0.02f, 0.08f, 0.03f));
		mGlobo = Mat("Globo", new Color(0.9f, 0.12f, 0.15f), null, 1f, 0f, 0.85f);
		mGloboAzul = Mat("GloboAzul", new Color(0.15f, 0.5f, 0.95f), null, 1f, 0f, 0.85f);
		mCuerda = Mat("Cuerda", new Color(0.85f, 0.85f, 0.8f));
		mAcero = Mat("Acero", new Color(0.85f, 0.87f, 0.9f), null, 1f, 0.8f, 0.75f);
		mCarton = Mat("Carton", new Color(0.72f, 0.58f, 0.4f), tSaco, 1f, 0f, 0.05f);
		mCivil = Mat("Civil", new Color(0.25f, 0.55f, 0.95f), null, 1f, 0f, 0.2f);

		mPiel = Mat("Piel", new Color(0.72f, 0.52f, 0.38f), null, 1f, 0f, 0.3f);
		mUniforme = Mat("Uniforme", Color.white, tCamuflaje, 1f, 0f, 0.1f);
		mChaleco = Mat("Chaleco", new Color(0.5f, 0.52f, 0.36f), tSaco, 1f, 0f, 0.15f);
		mCasco = Mat("Casco", new Color(0.85f, 0.9f, 0.8f), tCamuflaje, 1f, 0.05f, 0.3f);
		mBotas = Mat("Botas", new Color(0.13f, 0.11f, 0.09f), null, 1f, 0f, 0.35f);
		mGuantes = Mat("Guantes", new Color(0.16f, 0.15f, 0.13f), null, 1f, 0f, 0.25f);
		mGafas = Mat("Gafas", new Color(0.1f, 0.12f, 0.12f), null, 1f, 0.5f, 0.9f, new Color(0.05f, 0.12f, 0.12f));
		mArma = Mat("Arma", new Color(0.22f, 0.23f, 0.24f), tMetal, 1f, 0.15f, 0.35f);
		mArmaClaro = Mat("ArmaClaro", new Color(0.36f, 0.33f, 0.26f), tSaco, 1f, 0.1f, 0.3f);

		mRobot = Mat("Robot", new Color(0.55f, 0.58f, 0.62f), tMetal, 1f, 0.55f, 0.5f);
		mRobotOscuro = Mat("RobotOscuro", new Color(0.18f, 0.19f, 0.22f), tMetal, 1f, 0.5f, 0.45f);
		mRobotClaro = Mat("RobotClaro", new Color(0.75f, 0.32f, 0.12f), tMetal, 1f, 0.4f, 0.5f);
		mOjoRojo = Mat("OjoRojo", new Color(1f, 0.1f, 0.05f), null, 1f, 0f, 0.9f, new Color(3f, 0.25f, 0.1f));
		mNaranja = Mat("Naranja", new Color(1f, 0.5f, 0.1f), null, 1f, 0.2f, 0.6f, new Color(1.2f, 0.45f, 0.05f));
		mRotor = Mat("Rotor", new Color(0.25f, 0.25f, 0.28f), null, 1f, 0.5f, 0.6f);
		mNucleo = Mat("Nucleo", new Color(0.3f, 0.9f, 1f), null, 1f, 0f, 0.95f, new Color(0.6f, 2.4f, 3f));
		mNucleoFurioso = Mat("NucleoFurioso", new Color(1f, 0.25f, 0.1f), null, 1f, 0f, 0.95f, new Color(4f, 0.6f, 0.2f));
		mColoso = Mat("Coloso", new Color(0.42f, 0.4f, 0.38f), tPlacas, 1f, 0.6f, 0.45f);
		mColosoOscuro = Mat("ColosoOscuro", new Color(0.16f, 0.15f, 0.16f), tMetal, 1f, 0.55f, 0.4f);
		mVisor = Mat("Visor", new Color(1f, 0.15f, 0.05f), null, 1f, 0f, 0.9f, new Color(2.5f, 0.3f, 0.05f));

		mMunicion = Mat("Municion", new Color(0.3f, 0.36f, 0.2f), tMetal, 1f, 0.3f, 0.35f);
		mLaton = Mat("Laton", new Color(0.85f, 0.65f, 0.25f), null, 1f, 0.9f, 0.7f, new Color(0.15f, 0.1f, 0f));
		mBotiquin = Mat("Botiquin", new Color(0.92f, 0.92f, 0.9f), null, 1f, 0f, 0.5f);
		mCruz = Mat("Cruz", new Color(0.85f, 0.1f, 0.1f), null, 1f, 0f, 0.5f, new Color(0.6f, 0.05f, 0.05f));
		mBrilloAmarillo = Mat("BrilloAmarillo", new Color(1f, 0.8f, 0.2f), null, 1f, 0f, 0.5f, new Color(1.5f, 1.1f, 0.2f));
		mBrilloVerde = Mat("BrilloVerde", new Color(0.3f, 1f, 0.4f), null, 1f, 0f, 0.5f, new Color(0.4f, 1.6f, 0.5f));
		mLuzRoja = Mat("LuzRoja", new Color(1f, 0.1f, 0.1f), null, 1f, 0f, 0.5f, new Color(4f, 0.3f, 0.2f));
		mTrazadora = Mat("Trazadora", new Color(1f, 0.85f, 0.4f), null, 1f, 0f, 0.5f, new Color(6f, 4f, 1.2f));

		mFuego = Particulas("Fuego", true, tSuave);
		mHumo = Particulas("Humo", false, tSuave);
		mDestelloBoca = Particulas("DestelloBoca", true, tEstrella);
		mEstela = Particulas("Estela", true, tSuave);
		mRestos = Mat("Restos", Color.white, null, 1f, 0.1f, 0.3f, null, "Assets/Resources/");
		mGolpe = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Golpe.mat");
		if (mGolpe == null) {
			mGolpe = new Material(Shader.Find("Unlit/Color"));
			AssetDatabase.CreateAsset(mGolpe, "Assets/Resources/Golpe.mat");
		}
		mGolpe.color = new Color(1f, 0.95f, 0.9f);
		EditorUtility.SetDirty(mGolpe);

		cieloDia = Cielo("CieloDia", new Color(0.45f, 0.55f, 0.7f), new Color(0.4f, 0.38f, 0.33f), 1.25f, 0.9f, 0.04f);
		cieloTarde = Cielo("CieloTarde", new Color(0.55f, 0.5f, 0.5f), new Color(0.3f, 0.28f, 0.26f), 1.05f, 1.4f, 0.035f);
		cieloNoche = Cielo("CieloNoche", new Color(0.55f, 0.35f, 0.35f), new Color(0.12f, 0.04f, 0.03f), 0.38f, 1f, 0.05f);
		cieloMenu = Cielo("CieloMenu", new Color(0.5f, 0.35f, 0.3f), new Color(0.15f, 0.12f, 0.1f), 0.8f, 1.9f, 0.05f);
		AssetDatabase.SaveAssets();
	}
}
