using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

//Las escenas: menú, los tres niveles y la pantalla final
public static partial class ConstructorProyecto {

	// ------------------------------------------------ Ambiente

	static Light NuevaEscena(Material cielo, Color sol, float intensidad, Vector3 rotacionSol, Color ambienteCielo, Color ambienteHorizonte,
		Color ambienteSuelo, Color niebla, float densidadNiebla) {
		EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		var luz = new GameObject("Sol").AddComponent<Light>();
		luz.type = LightType.Directional;
		luz.color = sol;
		luz.intensity = intensidad;
		luz.shadows = LightShadows.Soft;
		luz.shadowStrength = 0.85f;
		luz.shadowBias = 0.04f;
		luz.shadowNormalBias = 0.3f;
		luz.transform.rotation = Quaternion.Euler(rotacionSol);
		RenderSettings.skybox = cielo;
		RenderSettings.sun = luz;
		RenderSettings.ambientMode = AmbientMode.Trilight;
		RenderSettings.ambientSkyColor = ambienteCielo;
		RenderSettings.ambientEquatorColor = ambienteHorizonte;
		RenderSettings.ambientGroundColor = ambienteSuelo;
		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.ExponentialSquared;
		RenderSettings.fogColor = niebla;
		RenderSettings.fogDensity = densidadNiebla;
		RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
		RenderSettings.reflectionIntensity = 0.4f;
		return luz;
	}

	static GameObject Suelo(Transform padre, Vector3 centro, Vector2 tam, Material m) {
		var s = Cubo("Suelo", padre, centro + Vector3.down * 0.5f, new Vector3(tam.x, 1f, tam.y), m);
		s.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
		return s;
	}

	static void Pared(Transform padre, Vector3 centro, Vector3 tam) {
		var p = Vacio("ParedInvisible", padre, centro);
		p.AddComponent<BoxCollider>().size = tam;
	}

	//Montañas lejanas alrededor del escenario (solo decoración)
	static void Montanas(Transform padre, float radio, int cantidad, Material m, int semilla) {
		var r = new System.Random(semilla);
		for (int i = 0; i < cantidad; i++) {
			float a = i / (float)cantidad * Mathf.PI * 2f + (float)r.NextDouble() * 0.2f;
			float d = radio * (0.9f + (float)r.NextDouble() * 0.3f);
			float alto = 20f + (float)r.NextDouble() * 35f;
			float ancho = 40f + (float)r.NextDouble() * 40f;
			var mt = Esfera("Montana", padre, new Vector3(Mathf.Sin(a) * d, -alto * 0.15f, Mathf.Cos(a) * d), new Vector3(ancho, alto * 1.4f, ancho * 0.8f), m, 0);
			mt.transform.rotation = Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f);
			mt.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
		}
	}

	static void Bosque(Transform padre, Rect zonaLibre, float radioMax, int cantidad, int semilla) {
		var r = new System.Random(semilla);
		int puestos = 0, intentos = 0;
		while (puestos < cantidad && intentos++ < cantidad * 30) {
			float x = ((float)r.NextDouble() * 2f - 1f) * radioMax;
			float z = ((float)r.NextDouble() * 2f - 1f) * radioMax;
			if (zonaLibre.Contains(new Vector2(x, z))) continue;
			var arbol = Poner(r.NextDouble() < 0.5 ? pPino : pArbol, new Vector3(x, 0f, z), (float)r.NextDouble() * 360f, padre, 0.8f + (float)r.NextDouble() * 0.7f);
			arbol.name = "Arbol";
			puestos++;
		}
	}

	static Nivel NuevoNivel(int numero, string nombre, string descripcion, Mision mision, int municion, string siguiente, bool musicaJefe) {
		var go = new GameObject("Nivel");
		var n = go.AddComponent<Nivel>();
		n.numero = numero;
		n.nombre = nombre;
		n.descripcion = descripcion;
		n.mision = mision;
		n.municionInicial = municion;
		n.siguiente = siguiente;
		n.musicaJefe = musicaJefe;
		n.oleadas = new GameObject[0];
		return n;
	}

	static GameObject PonerObjetivo(GameObject prefab, Vector3 pos, Transform padre, Vector3 mirarA) {
		var o = Poner(prefab, pos, 0f, padre);
		MirarHacia(o, mirarA);
		return o;
	}

	static void Deslizar(GameObject go, float distancia, float velocidad, float desfase) {
		var m = go.AddComponent<Movimiento>();
		m.modo = Movimiento.Modo.Deslizar;
		m.eje = Vector3.right;
		m.distancia = distancia;
		m.velocidad = velocidad;
		m.desfase = desfase;
	}

	//Cartel de madera con texto
	static void Cartel(Transform padre, string texto, Vector3 pos, Vector3 mirarA, float ancho, float alto, float altura, Color colorTexto) {
		var c = Vacio("Cartel", padre, pos);
		MirarHacia(c, mirarA);
		var t = c.transform;
		Cubo("Tabla", t, new Vector3(0f, altura, 0f), new Vector3(ancho, alto, 0.12f), mMaderaOscura);
		Cubo("Poste", t, new Vector3(-ancho * 0.4f, altura * 0.5f, 0.1f), new Vector3(0.14f, altura, 0.14f), mMadera);
		Cubo("Poste", t, new Vector3(ancho * 0.4f, altura * 0.5f, 0.1f), new Vector3(0.14f, altura, 0.14f), mMadera);
		Letrero(t, texto, new Vector3(0f, altura, -0.07f), alto * 0.9f, colorTexto, ancho * 0.94f, false, null, 160);
	}

	// ------------------------------------------------ Nivel 1: Galería de disparos

	static void CrearNivel1() {
		NuevaEscena(cieloDia, new Color(1f, 0.95f, 0.85f), 1.25f, new Vector3(42f, -35f, 0f),
			new Color(0.55f, 0.65f, 0.8f), new Color(0.5f, 0.52f, 0.5f), new Color(0.28f, 0.25f, 0.2f), new Color(0.68f, 0.75f, 0.82f), 0.006f);
		var nivel = NuevoNivel(1, "Galería de disparos",
			"Campo de entrenamiento del Escuadrón Halcón. Destruye las 12 dianas con 20 balas.\n" +
			"Cada objetivo da distintos puntos: el centro de la diana vale más que el borde.\n" +
			"<color=#ff6655>No dispares a las siluetas azules (civiles): restan 50 puntos.</color>",
			Mision.Dianas, 20, Juego.ESCENA_NIVEL2, false);

		var escenario = new GameObject("Escenario").transform;
		Vector3 jugador = new Vector3(0f, 0f, -22f);
		Suelo(escenario, new Vector3(0f, 0f, 5f), new Vector2(200f, 200f), mPasto);
		Cubo("Plataforma", escenario, new Vector3(0f, 0.03f, -21f), new Vector3(20f, 0.06f, 12f), mConcreto, null, 1).GetComponent<Renderer>().sharedMaterial.mainTextureScale = Vector2.one * 2f;
		Cubo("Camino", escenario, new Vector3(0f, 0.01f, 10f), new Vector3(5f, 0.02f, 42f), mTierra, null, 0);
		Montanas(escenario, 170f, 14, mMontana, 1);
		Bosque(escenario, new Rect(-27f, -32f, 54f, 74f), 75f, 70, 2);

		//Mostrador de tiro y cartel
		Cubo("Mostrador", escenario, new Vector3(0f, 0.55f, -15f), new Vector3(16f, 1.1f, 0.7f), mMadera);
		Cubo("Tablero", escenario, new Vector3(0f, 1.13f, -15.05f), new Vector3(16.4f, 0.08f, 0.95f), mMaderaOscura);
		Letrero(escenario, "GALERÍA DE TIRO", new Vector3(0f, 0.62f, -15.37f), 0.7f, AMBAR, 6f, true, new Color(0.06f, 0.06f, 0.06f, 0.9f), 160);
		Cartel(escenario, "DIANA 10 · 25 · 50     MÓVIL 20 · 50 · 100\nBOTELLA 15   GLOBO 30   PLACA 100   CIVIL −50",
			new Vector3(-13.5f, 0f, -16f), jugador, 6.5f, 1.6f, 2.2f, Color.white);
		for (int i = -1; i <= 1; i += 2) {
			Poner(pSacos, new Vector3(i * 11f, 0f, -14.5f), 0f, escenario);
			Poner(pSacos, new Vector3(i * 14f, 0f, -14.5f), 0f, escenario);
		}

		//Fondo de la galería: muro de madera y terraplén que detienen las balas
		Cubo("Muro", escenario, new Vector3(0f, 3f, 38f), new Vector3(46f, 6f, 0.6f), mMaderaOscura);
		Esfera("Terraplen", escenario, new Vector3(0f, -1f, 44f), new Vector3(70f, 14f, 12f), mTierra, 0);
		Esfera("TerraplenI", escenario, new Vector3(-29f, -1f, 6f), new Vector3(9f, 8f, 80f), mTierra, 0);
		Esfera("TerraplenD", escenario, new Vector3(29f, -1f, 6f), new Vector3(9f, 8f, 80f), mTierra, 0);
		for (int i = 0; i < 12; i++) Poner(pPoste, new Vector3(i % 2 == 0 ? -13f : 13f, 0f, -10f + (i / 2) * 8f), 0f, escenario);

		//Vallas del perímetro
		for (float z = -28f; z <= 36f; z += 4f) {
			Poner(pValla, new Vector3(-24f, 0f, z), 90f, escenario);
			Poner(pValla, new Vector3(24f, 0f, z), 90f, escenario);
		}
		for (float x = -22f; x <= 22f; x += 4f) Poner(pValla, new Vector3(x, 0f, -30f), 0f, escenario);

		//Campamento alrededor
		Poner(pTorre, new Vector3(-19f, 0f, -6f), 30f, escenario);
		Poner(pTorre, new Vector3(19f, 0f, 28f), -150f, escenario);
		Poner(pCarpa, new Vector3(17f, 0f, -24f), 90f, escenario);
		Poner(pCarpa, new Vector3(19.5f, 0f, -17f), 90f, escenario);
		Poner(pBandera, new Vector3(-7f, 0f, -27f), 0f, escenario);
		Poner(pBandera, new Vector3(7f, 0f, -27f), 0f, escenario);
		Poner(pCajaGrande, new Vector3(-17f, 0f, -24f), 10f, escenario);
		Poner(pCaja, new Vector3(-15f, 0f, -22f), 25f, escenario);
		Poner(pCaja, new Vector3(-15.3f, 1f, -22.2f), 5f, escenario, 0.8f);
		Poner(pCaja, new Vector3(-19.5f, 0f, -20f), -15f, escenario);
		Poner(pBarril, new Vector3(-12f, 0f, -26f), 0f, escenario);
		Poner(pBarril, new Vector3(-11.2f, 0f, -26.6f), 40f, escenario);
		Poner(pBarrilRojo, new Vector3(12f, 0f, -27f), 0f, escenario);
		Poner(pFarol, new Vector3(-10f, 0f, -18f), 90f, escenario);
		Poner(pFarol, new Vector3(10f, 0f, -18f), -90f, escenario);
		Poner(pRoca, new Vector3(-21f, 0f, 14f), 20f, escenario, 1.4f);
		Poner(pRoca, new Vector3(21f, 0f, 6f), 120f, escenario, 1.2f);
		Poner(pContenedor, new Vector3(-20f, 0f, 30f), 0f, escenario);

		//Objetivos
		var objetivos = new GameObject("Objetivos").transform;
		PonerObjetivo(pDiana, new Vector3(-7f, 0f, -2f), objetivos, jugador);
		PonerObjetivo(pDiana, new Vector3(-2.5f, 0f, 5f), objetivos, jugador);
		PonerObjetivo(pDiana, new Vector3(3.5f, 0f, 1f), objetivos, jugador);
		PonerObjetivo(pDiana, new Vector3(8.5f, 0f, 9f), objetivos, jugador);
		var movil1 = PonerObjetivo(pDianaMovil, new Vector3(0f, 0f, 13f), objetivos, new Vector3(0f, 0f, -100f));
		Deslizar(movil1, 5f, 0.7f, 0f);
		Cubo("Riel", escenario, new Vector3(0f, 0.03f, 13.1f), new Vector3(11.5f, 0.06f, 0.5f), mMetalOscuro, null, 0);
		var movil2 = PonerObjetivo(pDianaMovil, new Vector3(-2f, 0f, 25f), objetivos, new Vector3(0f, 0f, -100f));
		Deslizar(movil2, 7f, 1f, 1.5f);
		Cubo("Riel", escenario, new Vector3(-2f, 0.03f, 25.1f), new Vector3(15.5f, 0.06f, 0.5f), mMetalOscuro, null, 0);
		foreach (var p in new[] { new Vector3(-10.5f, 0f, -6f), new Vector3(10.5f, 0f, -3f), new Vector3(6.5f, 0f, 18f) }) {
			Poner(pBarril, p, 0f, escenario);
			PonerObjetivo(pBotella, p + Vector3.up * 0.91f, objetivos, jugador);
		}
		PonerObjetivo(pGlobo, new Vector3(-9f, 3.2f, 15f), objetivos, jugador);
		PonerObjetivo(pGloboAzul, new Vector3(11f, 4.2f, 22f), objetivos, jugador);
		Cubo("PosteGlobo", escenario, new Vector3(-9f, 1f, 15f), new Vector3(0.1f, 2f, 0.1f), mMetalOscuro, null, 0);
		Cubo("PosteGlobo", escenario, new Vector3(11f, 1.5f, 22f), new Vector3(0.1f, 3f, 0.1f), mMetalOscuro, null, 0);
		PonerObjetivo(pPlaca, new Vector3(2.5f, 0f, 34f), objetivos, jugador);
		PonerObjetivo(pCivil, new Vector3(-5.2f, 0f, 2.5f), objetivos, jugador);
		PonerObjetivo(pCivil, new Vector3(6.2f, 0f, 5f), objetivos, jugador);
		PonerObjetivo(pCivil, new Vector3(-6f, 0f, 21f), objetivos, jugador);

		//Límites
		Pared(escenario, new Vector3(-23.5f, 3f, 4f), new Vector3(1f, 6f, 70f));
		Pared(escenario, new Vector3(23.5f, 3f, 4f), new Vector3(1f, 6f, 70f));
		Pared(escenario, new Vector3(0f, 3f, -29.5f), new Vector3(48f, 6f, 1f));

		Poner(pJugador, jugador, 0f, null).name = "Jugador";
		CrearHUD();
		GuardarEscena(Juego.ESCENA_NIVEL1);
	}

	// ------------------------------------------------ Nivel 2: Base enemiga

	static void CrearNivel2() {
		NuevaEscena(cieloTarde, new Color(1f, 0.88f, 0.75f), 1.05f, new Vector3(32f, 60f, 0f),
			new Color(0.5f, 0.55f, 0.62f), new Color(0.48f, 0.45f, 0.42f), new Color(0.25f, 0.22f, 0.2f), new Color(0.62f, 0.6f, 0.6f), 0.009f);
		var nivel = NuevoNivel(2, "Base enemiga",
			"La Legión Ferro tomó la base. Elimina a todos los robots en 3 oleadas.\n" +
			"Los centinelas caminan hacia ti y los drones se lanzan en picada: si te tocan, pierdes una vida.\n" +
			"Apunta a la cabeza (daño doble). Recoge munición y botiquines. Las antenas dan puntos extra.",
			Mision.Enemigos, 35, Juego.ESCENA_NIVEL3, false);

		var escenario = new GameObject("Escenario").transform;
		Vector3 jugador = new Vector3(0f, 0f, -29f);
		Suelo(escenario, Vector3.zero, new Vector2(220f, 220f), mTierra);
		Cubo("Patio", escenario, new Vector3(0f, 0.02f, 0f), new Vector3(36f, 0.04f, 36f), mConcreto, null, 0);
		Montanas(escenario, 160f, 12, mMontana, 3);
		Bosque(escenario, new Rect(-40f, -40f, 80f, 80f), 85f, 60, 4);

		//Muro perimetral con torres en las esquinas
		for (int i = 0; i < 9; i++) {
			float p = -32f + i * 8f;
			Poner(pMuro, new Vector3(p, 0f, 36f), 0f, escenario);
			Poner(pMuro, new Vector3(p, 0f, -36f), 0f, escenario);
			Poner(pMuro, new Vector3(36f, 0f, p), 90f, escenario);
			Poner(pMuro, new Vector3(-36f, 0f, p), 90f, escenario);
		}
		foreach (var e in new[] { new Vector3(-33f, 0f, -33f), new Vector3(33f, 0f, -33f), new Vector3(-33f, 0f, 33f), new Vector3(33f, 0f, 33f) })
			Poner(pTorre, e, 45f, escenario);

		//Edificios y coberturas
		Poner(pBunker, new Vector3(-20f, 0f, 10f), 90f, escenario);
		Poner(pBunker, new Vector3(21f, 0f, 18f), -90f, escenario);
		Poner(pContenedor, new Vector3(-8f, 0f, -12f), 90f, escenario);
		Poner(pContenedorAzul, new Vector3(11f, 0f, -14f), 0f, escenario);
		Poner(pContenedor, new Vector3(2f, 0f, 19f), 90f, escenario);
		Poner(pContenedorAzul, new Vector3(-26f, 0f, -20f), 0f, escenario);
		Poner(pContenedor, new Vector3(27f, 0f, -16f), 10f, escenario);
		Poner(pContenedorAzul, new Vector3(-12f, 0f, 27f), 90f, escenario);
		Poner(pContenedorAzul, new Vector3(-12f, 2.6f, 27f), 85f, escenario);
		foreach (var (p, g) in new[] {
			(new Vector3(-4f, 0f, -22f), 0f), (new Vector3(5f, 0f, -21f), 10f), (new Vector3(-15f, 0f, -3f), 80f), (new Vector3(14f, 0f, 3f), -70f),
			(new Vector3(0f, 0f, 4f), 0f), (new Vector3(-24f, 0f, 24f), 45f), (new Vector3(25f, 0f, 30f), -30f) })
			Poner(pSacos, p, g, escenario);
		foreach (var p in new[] { new Vector3(-18f, 0f, -26f), new Vector3(18f, 0f, -26f), new Vector3(8f, 0f, 9f), new Vector3(-6f, 0f, 12f), new Vector3(29f, 0f, 6f) }) {
			Poner(pCajaGrande, p, Random.Range(-20f, 20f), escenario);
			Poner(pCaja, p + new Vector3(0.3f, 1.5f, 0f), Random.Range(0f, 40f), escenario);
		}
		foreach (var p in new[] { new Vector3(-9f, 0f, -16f), new Vector3(-8.2f, 0f, -16.6f), new Vector3(15f, 0f, -9f), new Vector3(-30f, 0f, 2f), new Vector3(-29.2f, 0f, 2.6f), new Vector3(9f, 0f, 27f) })
			Poner(Random.value < 0.5f ? pBarril : pBarrilRojo, p, Random.Range(0f, 360f), escenario);
		Poner(pBandera, new Vector3(0f, 0f, 33f), 0f, escenario);
		foreach (var p in new[] { new Vector3(-12f, 0f, -30f), new Vector3(12f, 0f, -30f), new Vector3(0f, 0f, 10f) })
			Poner(pFarol, p, 0f, escenario);
		var bandera = Poner(pBandera, new Vector3(-30f, 0f, -30f), 0f, escenario);
		foreach (var r in bandera.GetComponentsInChildren<Renderer>()) if (r.sharedMaterial == mBandera) r.sharedMaterial = mBanderaRoja;

		//Antenas enemigas (puntos extra)
		var objetivos = new GameObject("Objetivos").transform;
		PonerObjetivo(pAntena, new Vector3(-29f, 0f, 29f), objetivos, jugador);
		PonerObjetivo(pAntena, new Vector3(29f, 0f, 27f), objetivos, jugador);
		PonerObjetivo(pAntena, new Vector3(-29f, 0f, -8f), objetivos, jugador);

		//Oleadas de enemigos
		var enemigos = new GameObject("Enemigos").transform;
		GameObject Oleada(string nombre, Vector3[] centinelas, Vector3[] drones) {
			var o = Vacio(nombre, enemigos, Vector3.zero);
			foreach (var p in centinelas) MirarHacia(Poner(pCentinela, p, 0f, o.transform), p + (p - jugador));
			foreach (var p in drones) Poner(pDron, p, 180f, o.transform);
			return o;
		}
		nivel.oleadas = new[] {
			Oleada("Oleada1",
				new[] { new Vector3(-10f, 0f, 22f), new Vector3(10f, 0f, 24f), new Vector3(-3f, 0f, 29f) },
				new[] { new Vector3(-6f, 3f, 15f), new Vector3(6f, 3f, 15f) }),
			Oleada("Oleada2",
				new[] { new Vector3(-29f, 0f, 6f), new Vector3(29f, 0f, 10f), new Vector3(-20f, 0f, 30f), new Vector3(16f, 0f, 30f) },
				new[] { new Vector3(-25f, 3f, -8f), new Vector3(25f, 3f, -6f), new Vector3(0f, 3.5f, 31f) }),
			Oleada("Oleada3",
				new[] { new Vector3(-29f, 0f, -29f), new Vector3(29f, 0f, -29f), new Vector3(-6f, 0f, 31f), new Vector3(8f, 0f, 31f), new Vector3(30f, 0f, 22f) },
				new[] { new Vector3(-30f, 3f, 16f), new Vector3(30f, 3f, -2f), new Vector3(-16f, 3f, -28f), new Vector3(16f, 3.5f, 30f) }),
		};
		foreach (var o in nivel.oleadas) o.SetActive(false);

		//Recogibles
		var recogibles = new GameObject("Recogibles").transform;
		foreach (var p in new[] { new Vector3(-14f, 0f, -22f), new Vector3(14f, 0f, -23f), new Vector3(-25f, 0f, 15f), new Vector3(27f, 0f, 2f), new Vector3(0f, 0f, 8f) })
			Poner(pMunicion, p, 0f, recogibles);
		Poner(pBotiquin, new Vector3(-22f, 0f, -6f), 0f, recogibles);
		Poner(pBotiquin, new Vector3(22f, 0f, 26f), 0f, recogibles);

		Poner(pJugador, jugador, 0f, null).name = "Jugador";
		CrearHUD();
		GuardarEscena(Juego.ESCENA_NIVEL2);
	}

	// ------------------------------------------------ Nivel 3: Guarida del Coloso

	static void Brasero(Transform padre, Vector3 pos) {
		var b = Vacio("Brasero", padre, pos);
		Cilindro("Pie", b.transform, new Vector3(0f, 0.6f, 0f), new Vector3(0.3f, 0.6f, 0.3f), mMetalOscuro);
		Cilindro("Cuenco", b.transform, new Vector3(0f, 1.3f, 0f), new Vector3(1f, 0.15f, 1f), mMetalOscuro, null, 0);
		Esfera("Brasas", b.transform, new Vector3(0f, 1.42f, 0f), new Vector3(0.85f, 0.3f, 0.85f), mLava, 0);
		var luz = Vacio("Luz", b.transform, new Vector3(0f, 2.2f, 0f)).AddComponent<Light>();
		luz.type = LightType.Point;
		luz.color = new Color(1f, 0.45f, 0.15f);
		luz.range = 14f;
		luz.intensity = 2.2f;
		luz.shadows = LightShadows.None;
	}

	static void CrearNivel3() {
		NuevaEscena(cieloNoche, new Color(1f, 0.6f, 0.45f), 1f, new Vector3(30f, 160f, 0f),
			new Color(0.45f, 0.28f, 0.28f), new Color(0.4f, 0.22f, 0.18f), new Color(0.15f, 0.06f, 0.04f), new Color(0.22f, 0.07f, 0.05f), 0.012f);
		var nivel = NuevoNivel(3, "Guarida del Coloso",
			"El Coloso, el robot gigante que dirige a la Legión Ferro, te espera en su guarida.\n" +
			"Dispara a su <color=#66ddff>núcleo azul</color> (daño triple). Cuando brilla en rojo y ruge, va a embestirte:\n" +
			"esquívalo y haz que choque con un pilar para dejarlo aturdido (daño doble).",
			Mision.Jefe, 40, "", true);

		var escenario = new GameObject("Escenario").transform;
		Vector3 jugador = new Vector3(0f, 0f, -24f);
		Suelo(escenario, Vector3.zero, new Vector2(70f, 70f), mPlacas);
		var lava = Cubo("Lava", escenario, new Vector3(0f, -0.45f, 0f), new Vector3(300f, 0.5f, 300f), mLava, null, 0);
		lava.GetComponent<Renderer>().sharedMaterial.mainTextureScale = Vector2.one * 30f;
		Montanas(escenario, 150f, 12, mRocaOscura, 5);

		//Anillo de muros de roca
		int segmentos = 30;
		for (int i = 0; i < segmentos; i++) {
			float a = i / (float)segmentos * Mathf.PI * 2f;
			var m = Cubo("Muro", escenario, new Vector3(Mathf.Sin(a) * 32f, 3f, Mathf.Cos(a) * 32f), new Vector3(7.2f, 6f + (i % 3) * 1.2f, 1.5f), mRocaOscura);
			m.transform.rotation = Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f);
		}
		//Pilares: sirven de cobertura y para aturdir al Coloso
		for (int i = 0; i < 8; i++) {
			float a = (i + 0.5f) / 8f * Mathf.PI * 2f;
			Poner(pPilar, new Vector3(Mathf.Sin(a) * 15f, 0f, Mathf.Cos(a) * 15f), a * Mathf.Rad2Deg, escenario);
		}
		Poner(pPilarRoto, new Vector3(-22f, 0f, 12f), 40f, escenario);
		Poner(pPilarRoto, new Vector3(23f, 0f, -12f), 200f, escenario);
		for (int i = 0; i < 8; i++) {
			float a = i / 8f * Mathf.PI * 2f;
			Brasero(escenario, new Vector3(Mathf.Sin(a) * 28f, 0f, Mathf.Cos(a) * 28f));
		}
		Cilindro("Sello", escenario, new Vector3(0f, 0.02f, 4f), new Vector3(12f, 0.02f, 12f), mRocaOscura, null, 0);
		Cilindro("SelloBrillo", escenario, new Vector3(0f, 0.03f, 4f), new Vector3(10f, 0.01f, 10f), mLava, null, 0);
		Cilindro("SelloCentro", escenario, new Vector3(0f, 0.04f, 4f), new Vector3(9.3f, 0.01f, 9.3f), mRocaOscura, null, 0);

		var coloso = Poner(pColoso, new Vector3(0f, 0f, 8f), 180f, null);
		coloso.name = "Coloso";
		nivel.jefe = coloso.GetComponent<Jefe>();

		var recogibles = new GameObject("Recogibles").transform;
		foreach (var p in new[] { new Vector3(-24f, 0f, -9f), new Vector3(24f, 0f, -9f), new Vector3(-18f, 0f, 19f), new Vector3(18f, 0f, 19f) })
			Poner(pMunicion, p, 0f, recogibles);
		Poner(pBotiquin, new Vector3(-26f, 0f, 6f), 0f, recogibles);
		Poner(pBotiquin, new Vector3(26f, 0f, 6f), 0f, recogibles);

		Poner(pJugador, jugador, 0f, null).name = "Jugador";
		CrearHUD();
		GuardarEscena(Juego.ESCENA_NIVEL3);
	}

	// ------------------------------------------------ Menú

	static Camera CamaraFija(Vector3 pos, Vector3 mirarA, float fov) {
		var go = new GameObject("Camara");
		go.tag = "MainCamera";
		var cam = go.AddComponent<Camera>();
		cam.fieldOfView = fov;
		cam.nearClipPlane = 0.1f;
		cam.farClipPlane = 500f;
		cam.cullingMask = ~(1 << CAPA_ARMA);
		go.AddComponent<AudioListener>();
		go.transform.position = pos;
		go.transform.LookAt(mirarA);
		return cam;
	}

	//Escenario del menú: los personajes del juego en una plataforma al atardecer
	static void Diorama(Transform padre, bool conEnemigos) {
		Suelo(padre, Vector3.zero, new Vector2(200f, 200f), mTierra);
		Cilindro("Plataforma", padre, new Vector3(0f, 0.15f, 1.5f), new Vector3(13f, 0.15f, 13f), mConcretoOscuro);
		Cilindro("Borde", padre, new Vector3(0f, 0.16f, 1.5f), new Vector3(13.3f, 0.12f, 13.3f), mRayas, null, 0);
		Montanas(padre, 120f, 12, mMontana, 7);
		Bosque(padre, new Rect(-14f, -14f, 28f, 30f), 50f, 40, 8);
		Poner(pSacos, new Vector3(-5.5f, 0.3f, 0.5f), 70f, padre);
		Poner(pSacos, new Vector3(5.8f, 0.3f, 0.5f), -60f, padre);
		Poner(pCajaGrande, new Vector3(5.5f, 0.3f, 5f), 15f, padre);
		Poner(pCaja, new Vector3(4f, 0.3f, 3.6f), 40f, padre);
		Poner(pBarrilRojo, new Vector3(-5.5f, 0.3f, 4f), 0f, padre);
		Poner(pBarril, new Vector3(-6.2f, 0.3f, 4.8f), 0f, padre);
		Poner(pBandera, new Vector3(-3f, 0.3f, 6.5f), 0f, padre);
		Poner(pTorre, new Vector3(-12f, 0f, 14f), 20f, padre);
		Poner(pContenedor, new Vector3(13f, 0f, 14f), 70f, padre);
		Poner(pCarpa, new Vector3(-16f, 0f, 2f), 80f, padre);
		Brasero(padre, new Vector3(6.5f, 0.3f, 2.5f));
		Brasero(padre, new Vector3(-6.5f, 0.3f, 2f));
		if (conEnemigos) {
			var coloso = Poner(pColoso, new Vector3(1.5f, 0.3f, 7.5f), 200f, padre, 1f);
			coloso.GetComponent<Jefe>().enabled = false;
			var c = Poner(pCentinela, new Vector3(3.7f, 0.3f, 1.6f), 200f, padre);
			c.GetComponent<Enemigo>().enabled = false;
			var d = Poner(pDron, new Vector3(-1.3f, 2.9f, 1.5f), 165f, padre);
			d.GetComponent<Enemigo>().enabled = false;
		}
		Poner(pSoldado, new Vector3(0.6f, 0.3f, -1.8f), 165f, padre, 1.05f);
	}

	static void CrearMenu() {
		NuevaEscena(cieloMenu, new Color(1f, 0.7f, 0.5f), 1.1f, new Vector3(12f, 120f, 0f),
			new Color(0.45f, 0.35f, 0.35f), new Color(0.45f, 0.32f, 0.25f), new Color(0.15f, 0.12f, 0.1f), new Color(0.55f, 0.4f, 0.35f), 0.012f);
		var escenario = new GameObject("Escenario").transform;
		Diorama(escenario, true);
		var relleno = new GameObject("Contraluz").AddComponent<Light>();
		relleno.type = LightType.Directional;
		relleno.color = new Color(0.45f, 0.6f, 1f);
		relleno.intensity = 0.45f;
		relleno.shadows = LightShadows.None;
		relleno.transform.rotation = Quaternion.Euler(20f, -60f, 0f);

		var centro = new GameObject("CentroCamara").transform;
		centro.position = new Vector3(-2.3f, 0f, 2f);
		var cam = CamaraFija(new Vector3(-3.2f, 2.4f, -8.5f), centro.position + Vector3.up * 1.6f, 50f);

		var menu = new GameObject("Menu").AddComponent<Menu>();
		menu.camara = cam.transform;
		menu.centro = centro;
		CrearInterfazMenu(menu);
		GuardarEscena(Juego.ESCENA_MENU);
	}

	// ------------------------------------------------ Pantalla final

	static void CrearFin() {
		NuevaEscena(cieloMenu, new Color(1f, 0.75f, 0.55f), 1f, new Vector3(15f, 200f, 0f),
			new Color(0.45f, 0.38f, 0.38f), new Color(0.42f, 0.32f, 0.26f), new Color(0.15f, 0.12f, 0.1f), new Color(0.5f, 0.38f, 0.33f), 0.014f);
		var escenario = new GameObject("Escenario").transform;
		Diorama(escenario, false);
		var centro = new GameObject("Centro").transform;
		centro.position = new Vector3(0f, 0f, 1.5f);
		var cam = CamaraFija(new Vector3(0f, 3f, -9f), centro.position + Vector3.up * 1.5f, 55f);
		var fin = new GameObject("Fin").AddComponent<Fin>();
		fin.camara = cam.transform;
		fin.centro = centro;
		CrearInterfazFin(fin);
		GuardarEscena(Juego.ESCENA_FIN);
	}
}
