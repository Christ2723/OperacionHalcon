using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

//Modelos hechos con piezas básicas: jugador y su fusil, soldado (para el menú), centinela, dron, Coloso,
//dianas, botellas, globos, placa, civiles, antena, recogibles y la utilería del escenario
public static partial class ConstructorProyecto {

	static GameObject pJugador, pProyectil, pSoldado, pCentinela, pDron, pColoso;
	static GameObject pDiana, pDianaMovil, pBotella, pGlobo, pGloboAzul, pPlaca, pCivil, pAntena;
	static GameObject pMunicion, pBotiquin;
	static GameObject pCaja, pCajaGrande, pBarril, pBarrilRojo, pSacos, pArbol, pPino, pRoca, pTorre, pCarpa, pContenedor, pContenedorAzul,
		pBandera, pFarol, pPilar, pPilarRoto, pValla, pBunker, pMuro, pPoste;

	static void CrearPrefabs() {
		CrearProyectil();
		CrearJugador();
		CrearSoldado();
		CrearDron();
		CrearCentinela();
		CrearColoso();
		CrearObjetivos();
		CrearRecogibles();
		CrearUtileria();
	}

	// ------------------------------------------------ Jugador

	static void CrearProyectil() {
		var go = Vacio("Proyectil", null, Vector3.zero, Capas.Proyectil);
		go.AddComponent<Proyectil>();
		var bala = Capsula("Trazadora", go.transform, Vector3.zero, new Vector3(0.035f, 0.16f, 0.035f), mTrazadora, new Vector3(90f, 0f, 0f), 0);
		bala.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		var estela = go.AddComponent<TrailRenderer>();
		estela.sharedMaterial = mEstela;
		estela.time = 0.07f;
		estela.startWidth = 0.05f;
		estela.endWidth = 0f;
		estela.minVertexDistance = 0.1f;
		estela.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		var g = new Gradient();
		g.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0f), new GradientColorKey(new Color(1f, 0.5f, 0.1f), 1f) },
			new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
		estela.colorGradient = g;
		pProyectil = Guardar(go, "Proyectil");
	}

	//Fusil en primera persona con guantes y mangas (lo dibuja una cámara aparte para que no atraviese paredes)
	static Arma CrearFusil(Transform padre, Camera camaraPrincipal) {
		var arma = Vacio("Fusil", padre, new Vector3(0.2f, -0.22f, 0.44f), CAPA_ARMA);
		var t = arma.transform;
		Cubo("Cajon", t, new Vector3(0f, 0f, 0f), new Vector3(0.065f, 0.1f, 0.36f), mArma, null, 0);
		Cubo("Guardamanos", t, new Vector3(0f, -0.005f, 0.3f), new Vector3(0.07f, 0.08f, 0.26f), mArmaClaro, null, 0);
		Cilindro("Canon", t, new Vector3(0f, 0.012f, 0.5f), new Vector3(0.026f, 0.12f, 0.026f), mArma, new Vector3(90f, 0f, 0f), 0);
		Cilindro("Bocacha", t, new Vector3(0f, 0.012f, 0.62f), new Vector3(0.04f, 0.03f, 0.04f), mArma, new Vector3(90f, 0f, 0f), 0);
		Cubo("Cargador", t, new Vector3(0f, -0.11f, 0.06f), new Vector3(0.045f, 0.16f, 0.07f), mArma, new Vector3(-12f, 0f, 0f), 0);
		Cubo("Empunadura", t, new Vector3(0f, -0.09f, -0.09f), new Vector3(0.045f, 0.12f, 0.05f), mArmaClaro, new Vector3(18f, 0f, 0f), 0);
		Cubo("Culata", t, new Vector3(0f, -0.02f, -0.27f), new Vector3(0.055f, 0.1f, 0.2f), mArmaClaro, null, 0);
		Cubo("Riel", t, new Vector3(0f, 0.057f, 0.08f), new Vector3(0.04f, 0.015f, 0.42f), mArma, null, 0);
		Cilindro("Mira", t, new Vector3(0f, 0.085f, 0.03f), new Vector3(0.036f, 0.06f, 0.036f), mArma, new Vector3(90f, 0f, 0f), 0);
		Cilindro("Lente", t, new Vector3(0f, 0.085f, 0.092f), new Vector3(0.032f, 0.002f, 0.032f), mGafas, new Vector3(90f, 0f, 0f), 0);
		Cubo("PuntoRojo", t, new Vector3(0f, 0.085f, 0.095f), new Vector3(0.006f, 0.006f, 0.002f), mOjoRojo, null, 0);
		Cubo("Linterna", t, new Vector3(-0.045f, 0f, 0.36f), new Vector3(0.025f, 0.025f, 0.08f), mArma, null, 0);
		//Manos y mangas
		Cubo("GuanteDerecho", t, new Vector3(0.012f, -0.1f, -0.08f), new Vector3(0.075f, 0.08f, 0.1f), mGuantes, new Vector3(18f, 0f, 0f), 0);
		Cubo("GuanteIzquierdo", t, new Vector3(-0.015f, -0.06f, 0.3f), new Vector3(0.09f, 0.07f, 0.1f), mGuantes, null, 0);

		var boca = Vacio("Boca", t, new Vector3(0f, 0.012f, 0.66f), CAPA_ARMA);
		var destello = Pieza(PrimitiveType.Quad, "Destello", boca.transform, new Vector3(0f, 0f, 0.05f), Vector3.one * 0.22f, mDestelloBoca, null, 0, CAPA_ARMA);
		var luz = Vacio("Luz", boca.transform, new Vector3(0f, 0f, 0.1f), CAPA_ARMA).AddComponent<Light>();
		luz.type = LightType.Point;
		luz.color = new Color(1f, 0.75f, 0.4f);
		luz.range = 7f;
		luz.intensity = 3f;
		luz.shadows = LightShadows.None;

		Capa(arma, CAPA_ARMA);
		SinSombras(arma);
		var a = arma.AddComponent<Arma>();
		a.boca = boca.transform;
		a.destello = destello;
		a.luz = luz;
		a.camara = camaraPrincipal;
		a.proyectil = pProyectil.GetComponent<Proyectil>();
		return a;
	}

	static void CrearJugador() {
		var go = Vacio("Jugador", null, Vector3.zero, Capas.Jugador);
		var cc = go.AddComponent<CharacterController>();
		cc.height = 1.8f;
		cc.radius = 0.4f;
		cc.center = new Vector3(0f, 0.9f, 0f);
		cc.stepOffset = 0.4f;
		cc.slopeLimit = 50f;
		cc.skinWidth = 0.05f;

		var camGo = Vacio("Camara", go.transform, new Vector3(0f, 1.62f, 0f), Capas.Jugador);
		camGo.tag = "MainCamera";
		var cam = camGo.AddComponent<Camera>();
		cam.fieldOfView = 72f;
		cam.nearClipPlane = 0.05f;
		cam.farClipPlane = 600f;
		cam.cullingMask = ~(1 << CAPA_ARMA);
		cam.depth = 0;
		camGo.AddComponent<AudioListener>();

		var camArmaGo = Vacio("CamaraArma", camGo.transform, Vector3.zero, CAPA_ARMA);
		var camArma = camArmaGo.AddComponent<Camera>();
		camArma.clearFlags = CameraClearFlags.Depth;
		camArma.cullingMask = 1 << CAPA_ARMA;
		camArma.fieldOfView = 62f;
		camArma.nearClipPlane = 0.01f;
		camArma.farClipPlane = 5f;
		camArma.depth = 1;

		var arma = CrearFusil(camGo.transform, cam);
		var j = go.AddComponent<Jugador>();
		j.camara = camGo.transform;
		j.arma = arma;
		pJugador = Guardar(go, "Jugador");
	}

	//Soldado del Escuadrón Halcón: el protagonista visto en tercera persona (menú, final y hoja de personajes)
	static void CrearSoldado() {
		var go = Vacio("Soldado", null, Vector3.zero);
		var t = go.transform;
		foreach (int s in new[] { -1, 1 }) {
			Capsula("Pierna", t, new Vector3(0.13f * s, 0.5f, 0f), new Vector3(0.22f, 0.42f, 0.24f), mUniforme, null, 0);
			Cubo("Bota", t, new Vector3(0.13f * s, 0.08f, 0.04f), new Vector3(0.2f, 0.16f, 0.32f), mBotas, null, 0);
			Cubo("Rodillera", t, new Vector3(0.13f * s, 0.5f, 0.11f), new Vector3(0.16f, 0.14f, 0.06f), mChaleco, null, 0);
		}
		Cubo("Cadera", t, new Vector3(0f, 0.92f, 0f), new Vector3(0.44f, 0.18f, 0.26f), mUniforme, null, 0);
		Cubo("Cinturon", t, new Vector3(0f, 0.98f, 0f), new Vector3(0.46f, 0.06f, 0.28f), mBotas, null, 0);
		Cubo("Torso", t, new Vector3(0f, 1.25f, 0f), new Vector3(0.46f, 0.52f, 0.26f), mUniforme, null, 0);
		Cubo("Chaleco", t, new Vector3(0f, 1.27f, 0f), new Vector3(0.5f, 0.44f, 0.32f), mChaleco, null, 0);
		for (int i = -1; i <= 1; i++) Cubo("Bolsillo", t, new Vector3(0.13f * i, 1.13f, 0.17f), new Vector3(0.11f, 0.13f, 0.05f), mChaleco, null, 0);
		Cubo("Insignia", t, new Vector3(-0.13f, 1.4f, 0.165f), new Vector3(0.08f, 0.05f, 0.01f), mDianaAmarilla, null, 0);
		Capsula("Mochila", t, new Vector3(0f, 1.25f, -0.22f), new Vector3(0.36f, 0.24f, 0.16f), mChaleco, null, 0);
		Cubo("Cuello", t, new Vector3(0f, 1.53f, 0f), new Vector3(0.14f, 0.08f, 0.14f), mPiel, null, 0);
		Esfera("Cabeza", t, new Vector3(0f, 1.66f, 0f), new Vector3(0.22f, 0.25f, 0.23f), mPiel, 0);
		Esfera("Casco", t, new Vector3(0f, 1.74f, -0.01f), new Vector3(0.29f, 0.2f, 0.31f), mCasco, 0);
		Cubo("Visera", t, new Vector3(0f, 1.67f, 0.12f), new Vector3(0.2f, 0.05f, 0.04f), mGafas, null, 0);
		Cubo("Correa", t, new Vector3(0f, 1.6f, 0.1f), new Vector3(0.16f, 0.02f, 0.03f), mBotas, null, 0);
		//Brazos sosteniendo el fusil
		Capsula("BrazoD", t, new Vector3(0.28f, 1.24f, 0.08f), new Vector3(0.13f, 0.2f, 0.13f), mUniforme, new Vector3(-55f, 0f, 10f), 0);
		Capsula("BrazoI", t, new Vector3(-0.24f, 1.24f, 0.16f), new Vector3(0.13f, 0.22f, 0.13f), mUniforme, new Vector3(-70f, -25f, 0f), 0);
		Esfera("Hombrera", t, new Vector3(0.27f, 1.43f, 0f), new Vector3(0.16f, 0.12f, 0.18f), mChaleco, 0);
		Esfera("Hombrera", t, new Vector3(-0.27f, 1.43f, 0f), new Vector3(0.16f, 0.12f, 0.18f), mChaleco, 0);
		var fusil = Vacio("Fusil", t, new Vector3(0.08f, 1.18f, 0.32f));
		fusil.transform.localRotation = Quaternion.Euler(-8f, -6f, 0f);
		var f = fusil.transform;
		Cubo("Cuerpo", f, Vector3.zero, new Vector3(0.06f, 0.1f, 0.55f), mArma, null, 0);
		Cubo("Guardamanos", f, new Vector3(0f, 0f, 0.3f), new Vector3(0.065f, 0.08f, 0.24f), mArmaClaro, null, 0);
		Cilindro("Canon", f, new Vector3(0f, 0.01f, 0.5f), new Vector3(0.025f, 0.12f, 0.025f), mArma, new Vector3(90f, 0f, 0f), 0);
		Cubo("Cargador", f, new Vector3(0f, -0.1f, 0.05f), new Vector3(0.04f, 0.14f, 0.07f), mArma, new Vector3(-12f, 0f, 0f), 0);
		Cilindro("Mira", f, new Vector3(0f, 0.08f, 0.02f), new Vector3(0.045f, 0.08f, 0.045f), mArma, new Vector3(90f, 0f, 0f), 0);
		Cubo("Culata", f, new Vector3(0f, -0.02f, -0.3f), new Vector3(0.05f, 0.1f, 0.18f), mArmaClaro, null, 0);
		Cubo("Guante", f, new Vector3(-0.02f, -0.05f, 0.3f), new Vector3(0.09f, 0.07f, 0.09f), mGuantes, null, 0);
		Cubo("Guante", f, new Vector3(0.02f, -0.08f, -0.08f), new Vector3(0.08f, 0.08f, 0.09f), mGuantes, null, 0);
		pSoldado = Guardar(go, "Soldado");
	}

	// ------------------------------------------------ Enemigos

	static void CrearDron() {
		var go = Vacio("Dron", null, Vector3.zero, Capas.Enemigos);
		var cuerpo = Vacio("Cuerpo", go.transform, Vector3.zero, Capas.Enemigos);
		var c = cuerpo.transform;
		Esfera("Casco", c, Vector3.zero, new Vector3(0.75f, 0.45f, 0.75f), mRobotOscuro, 1);
		Cilindro("Anillo", c, new Vector3(0f, 0f, 0f), new Vector3(0.82f, 0.05f, 0.82f), mNaranja, null, 0);
		Esfera("Cupula", c, new Vector3(0f, 0.15f, 0f), new Vector3(0.4f, 0.25f, 0.4f), mRobot, 0);
		var ojo = Esfera("Ojo", c, new Vector3(0f, -0.02f, 0.32f), new Vector3(0.22f, 0.22f, 0.15f), mOjoRojo, 1);
		var parte = ojo.AddComponent<ParteEnemigo>();
		parte.multiplicador = 2;
		parte.etiqueta = "OJO";
		Cubo("Antena", c, new Vector3(0.1f, 0.3f, -0.1f), new Vector3(0.02f, 0.25f, 0.02f), mRobotOscuro, null, 0);
		Esfera("LuzAntena", c, new Vector3(0.1f, 0.44f, -0.1f), Vector3.one * 0.05f, mOjoRojo, 0);
		var rotores = new Transform[4];
		for (int i = 0; i < 4; i++) {
			var giro = Quaternion.Euler(0f, 45f + 90f * i, 0f);
			Vector3 extremo = giro * new Vector3(0f, 0.05f, 0.62f);
			Cubo("Brazo", c, giro * new Vector3(0f, 0.05f, 0.38f), new Vector3(0.07f, 0.05f, 0.5f), mRobotOscuro, new Vector3(0f, 45f + 90f * i, 0f), 0);
			Cilindro("Motor", c, extremo, new Vector3(0.14f, 0.08f, 0.14f), mRobot, null, 0);
			var rotor = Vacio("Rotor", c, extremo + Vector3.up * 0.1f, Capas.Enemigos);
			Cubo("Aspa", rotor.transform, Vector3.zero, new Vector3(0.5f, 0.01f, 0.06f), mRotor, null, 0);
			Cubo("Aspa", rotor.transform, Vector3.zero, new Vector3(0.06f, 0.01f, 0.5f), mRotor, null, 0);
			rotores[i] = rotor.transform;
		}
		var luz = Vacio("Luz", c, new Vector3(0f, -0.1f, 0.5f), Capas.Enemigos).AddComponent<Light>();
		luz.type = LightType.Point;
		luz.color = new Color(1f, 0.25f, 0.1f);
		luz.range = 3f;
		luz.intensity = 1.5f;
		luz.shadows = LightShadows.None;
		Capa(go, Capas.Enemigos);

		go.AddComponent<Destello>();
		var e = go.AddComponent<Enemigo>();
		e.tipo = TipoEnemigo.Dron;
		e.nombre = "Dron explorador";
		e.vida = 2;
		e.puntos = 60;
		e.velocidad = 4.2f;
		e.altura = 3f;
		e.radioContacto = 0.6f;
		e.cuerpo = c;
		e.rotores = rotores;
		pDron = Guardar(go, "Dron");
	}

	static void CrearCentinela() {
		var go = Vacio("Centinela", null, Vector3.zero, Capas.Enemigos);
		var t = go.transform;
		Transform Pierna(int s) {
			var p = Vacio(s < 0 ? "PiernaI" : "PiernaD", t, new Vector3(0.22f * s, 1.0f, 0f), Capas.Enemigos).transform;
			Cubo("Muslo", p, new Vector3(0f, -0.25f, 0f), new Vector3(0.24f, 0.5f, 0.28f), mRobot, null, 1);
			Cubo("Rodilla", p, new Vector3(0f, -0.5f, 0.06f), new Vector3(0.2f, 0.14f, 0.2f), mRobotOscuro, null, 0);
			Cubo("Canilla", p, new Vector3(0f, -0.72f, 0f), new Vector3(0.2f, 0.44f, 0.24f), mRobotOscuro, null, 1);
			Cubo("Pie", p, new Vector3(0f, -0.95f, 0.06f), new Vector3(0.26f, 0.1f, 0.4f), mRobot, null, 0);
			return p;
		}
		var piernaI = Pierna(-1);
		var piernaD = Pierna(1);
		Cubo("Cadera", t, new Vector3(0f, 1.05f, 0f), new Vector3(0.6f, 0.18f, 0.32f), mRobotOscuro, null, 1);
		var torso = Vacio("Torso", t, new Vector3(0f, 1.45f, 0f), Capas.Enemigos).transform;
		Cubo("Pecho", torso, Vector3.zero, new Vector3(0.78f, 0.66f, 0.46f), mRobot, null, 1);
		Cubo("Placa", torso, new Vector3(0f, 0.05f, 0.24f), new Vector3(0.6f, 0.4f, 0.06f), mRobotClaro, null, 0);
		Cubo("Franja", torso, new Vector3(0f, -0.12f, 0.275f), new Vector3(0.42f, 0.05f, 0.02f), mOjoRojo, null, 0);
		Cubo("Mochila", torso, new Vector3(0f, 0.05f, -0.3f), new Vector3(0.5f, 0.5f, 0.2f), mRobotOscuro, null, 0);
		Cilindro("Escape", torso, new Vector3(0.15f, 0.35f, -0.32f), new Vector3(0.1f, 0.12f, 0.1f), mRobotOscuro, null, 0);
		Cilindro("Escape", torso, new Vector3(-0.15f, 0.35f, -0.32f), new Vector3(0.1f, 0.12f, 0.1f), mRobotOscuro, null, 0);
		Transform Brazo(int s) {
			Cubo("Hombro", torso, new Vector3(0.5f * s, 0.25f, 0f), new Vector3(0.3f, 0.22f, 0.4f), mRobotClaro, null, 0);
			var b = Vacio(s < 0 ? "BrazoI" : "BrazoD", torso, new Vector3(0.5f * s, 0.2f, 0f), Capas.Enemigos).transform;
			Cubo("Brazo", b, new Vector3(0f, -0.3f, 0f), new Vector3(0.18f, 0.55f, 0.2f), mRobotOscuro, null, 1);
			if (s > 0) {
				Cilindro("Canon", b, new Vector3(0f, -0.62f, 0.12f), new Vector3(0.14f, 0.22f, 0.14f), mRobot, new Vector3(90f, 0f, 0f), 0);
				Cilindro("BocaCanon", b, new Vector3(0f, -0.62f, 0.34f), new Vector3(0.09f, 0.01f, 0.09f), mOjoRojo, new Vector3(90f, 0f, 0f), 0);
			} else {
				Cubo("Garra", b, new Vector3(0f, -0.64f, 0.04f), new Vector3(0.2f, 0.16f, 0.22f), mRobot, null, 0);
			}
			return b;
		}
		var brazoI = Brazo(-1);
		var brazoD = Brazo(1);
		var cabeza = Cubo("Cabeza", torso, new Vector3(0f, 0.55f, 0.02f), new Vector3(0.38f, 0.32f, 0.38f), mRobot, null, 1);
		var parte = cabeza.AddComponent<ParteEnemigo>();
		parte.multiplicador = 2;
		parte.etiqueta = "CABEZA";
		Cubo("Visor", torso, new Vector3(0f, 0.57f, 0.205f), new Vector3(0.3f, 0.07f, 0.03f), mVisor, null, 0);
		Cubo("Cresta", torso, new Vector3(0f, 0.74f, 0f), new Vector3(0.06f, 0.08f, 0.3f), mRobotClaro, null, 0);
		Cubo("Antena", torso, new Vector3(0.14f, 0.85f, -0.1f), new Vector3(0.02f, 0.3f, 0.02f), mRobotOscuro, null, 0);
		Capa(go, Capas.Enemigos);

		go.AddComponent<Destello>();
		var e = go.AddComponent<Enemigo>();
		e.tipo = TipoEnemigo.Centinela;
		e.nombre = "Centinela";
		e.vida = 4;
		e.puntos = 100;
		e.velocidad = 2.8f;
		e.radioContacto = 0.9f;
		e.cuerpo = torso;
		e.piernaI = piernaI;
		e.piernaD = piernaD;
		e.brazoI = brazoI;
		e.brazoD = brazoD;
		pCentinela = Guardar(go, "Centinela");
	}

	//El Coloso: robot gigante de casi 6 metros con un núcleo de energía en el pecho
	static void CrearColoso() {
		var go = Vacio("Coloso", null, Vector3.zero, Capas.Enemigos);
		var t = go.transform;
		Transform Pierna(int s) {
			var p = Vacio(s < 0 ? "PiernaI" : "PiernaD", t, new Vector3(0.95f * s, 2.7f, 0f), Capas.Enemigos).transform;
			Cubo("Muslo", p, new Vector3(0f, -0.7f, 0f), new Vector3(0.85f, 1.4f, 0.95f), mColoso, null, 1);
			Cubo("Rodilla", p, new Vector3(0f, -1.35f, 0.25f), new Vector3(0.75f, 0.5f, 0.6f), mColosoOscuro, null, 0);
			Cubo("Canilla", p, new Vector3(0f, -1.9f, 0f), new Vector3(0.75f, 1.2f, 0.85f), mColosoOscuro, null, 1);
			Cubo("Pie", p, new Vector3(0f, -2.5f, 0.2f), new Vector3(1.05f, 0.4f, 1.5f), mColoso, null, 1);
			Cubo("Piston", p, new Vector3(0f, -1.6f, -0.5f), new Vector3(0.18f, 1.1f, 0.18f), mAcero, null, 0);
			return p;
		}
		var piernaI = Pierna(-1);
		var piernaD = Pierna(1);
		Cubo("Pelvis", t, new Vector3(0f, 2.85f, 0f), new Vector3(2.1f, 0.6f, 1.2f), mColosoOscuro, null, 1);

		var torso = Vacio("Torso", t, new Vector3(0f, 3.1f, 0f), Capas.Enemigos).transform;
		Cubo("Abdomen", torso, new Vector3(0f, 0.3f, 0f), new Vector3(1.6f, 0.6f, 1.1f), mColosoOscuro, null, 1);
		Cubo("Pecho", torso, new Vector3(0f, 1.2f, 0f), new Vector3(2.8f, 1.6f, 1.7f), mColoso, null, 1);
		Cubo("Coraza", torso, new Vector3(0f, 1.35f, 0.82f), new Vector3(2.2f, 1.1f, 0.2f), mRobotClaro, null, 1);
		Cubo("MarcoNucleo", torso, new Vector3(0f, 1.2f, 0.95f), new Vector3(0.95f, 0.95f, 0.12f), mColosoOscuro, null, 0);
		var nucleo = Esfera("Nucleo", torso, new Vector3(0f, 1.2f, 1.02f), new Vector3(0.72f, 0.72f, 0.45f), mNucleo, 1);
		var parte = nucleo.AddComponent<ParteEnemigo>();
		parte.multiplicador = 3;
		parte.etiqueta = "NÚCLEO";
		var luz = Vacio("LuzNucleo", torso, new Vector3(0f, 1.2f, 1.6f), Capas.Enemigos).AddComponent<Light>();
		luz.type = LightType.Point;
		luz.color = new Color(0.3f, 0.85f, 1f);
		luz.range = 9f;
		luz.intensity = 2.5f;
		luz.shadows = LightShadows.None;
		for (int s = -1; s <= 1; s += 2) {
			Cubo("Rejilla", torso, new Vector3(0.75f * s, 0.7f, 0.86f), new Vector3(0.45f, 0.3f, 0.06f), mColosoOscuro, null, 0);
			Cilindro("Chimenea", torso, new Vector3(0.7f * s, 2.3f, -0.6f), new Vector3(0.35f, 0.5f, 0.35f), mColosoOscuro, null, 0);
			Cilindro("Brasa", torso, new Vector3(0.7f * s, 2.81f, -0.6f), new Vector3(0.25f, 0.02f, 0.25f), mNaranja, null, 0);
		}
		Cubo("Cuello", torso, new Vector3(0f, 2.1f, 0.1f), new Vector3(0.6f, 0.3f, 0.6f), mColosoOscuro, null, 0);
		Cubo("Cabeza", torso, new Vector3(0f, 2.45f, 0.2f), new Vector3(0.9f, 0.6f, 0.85f), mColoso, null, 1);
		Cubo("Visor", torso, new Vector3(0f, 2.48f, 0.63f), new Vector3(0.7f, 0.14f, 0.05f), mVisor, null, 0);
		Cubo("Cuerno", torso, new Vector3(0.35f, 2.85f, 0.1f), new Vector3(0.12f, 0.4f, 0.12f), mRobotClaro, new Vector3(0f, 0f, -20f), 0);
		Cubo("Cuerno", torso, new Vector3(-0.35f, 2.85f, 0.1f), new Vector3(0.12f, 0.4f, 0.12f), mRobotClaro, new Vector3(0f, 0f, 20f), 0);

		Transform Brazo(int s) {
			Cubo("Hombrera", torso, new Vector3(1.75f * s, 1.95f, 0f), new Vector3(1.1f, 0.6f, 1.3f), mRobotClaro, new Vector3(0f, 0f, -12f * s), 1);
			var b = Vacio(s < 0 ? "BrazoI" : "BrazoD", torso, new Vector3(1.75f * s, 1.65f, 0f), Capas.Enemigos).transform;
			Cubo("Brazo", b, new Vector3(0f, -0.75f, 0f), new Vector3(0.7f, 1.5f, 0.75f), mColosoOscuro, null, 1);
			Cubo("Antebrazo", b, new Vector3(0f, -1.75f, 0.1f), new Vector3(0.85f, 0.9f, 0.95f), mColoso, null, 1);
			if (s > 0) {
				Cilindro("Canon", b, new Vector3(0f, -2.3f, 0.4f), new Vector3(0.5f, 0.6f, 0.5f), mColosoOscuro, new Vector3(90f, 0f, 0f), 0);
				Cilindro("BocaCanon", b, new Vector3(0f, -2.3f, 1.01f), new Vector3(0.32f, 0.02f, 0.32f), mNaranja, new Vector3(90f, 0f, 0f), 0);
			} else {
				Cubo("Puno", b, new Vector3(0f, -2.4f, 0.1f), new Vector3(0.9f, 0.6f, 0.9f), mColosoOscuro, null, 0);
				for (int k = -1; k <= 1; k++) Cubo("Pua", b, new Vector3(0.28f * k, -2.75f, 0.35f), new Vector3(0.12f, 0.3f, 0.12f), mAcero, null, 0);
			}
			return b;
		}
		var brazoI = Brazo(-1);
		var brazoD = Brazo(1);
		Capa(go, Capas.Enemigos);

		go.AddComponent<Destello>();
		var j = go.AddComponent<Jefe>();
		j.nombre = "Coloso";
		j.vidaMax = 60;
		j.torso = torso;
		j.piernaI = piernaI;
		j.piernaD = piernaD;
		j.brazoI = brazoI;
		j.brazoD = brazoD;
		j.nucleo = nucleo.GetComponent<Renderer>();
		j.nucleoNormal = mNucleo;
		j.nucleoFurioso = mNucleoFurioso;
		j.luzNucleo = luz;
		j.dron = pDron.GetComponent<Enemigo>();
		pColoso = Guardar(go, "Coloso");
	}

	// ------------------------------------------------ Objetivos

	static Objetivo NuevoObjetivo(GameObject go, TipoObjetivo tipo, string nombre, int puntos, Color restos, bool cuenta = true, int resistencia = 1) {
		var o = go.AddComponent<Objetivo>();
		o.tipo = tipo;
		o.nombre = nombre;
		o.puntos = puntos;
		o.colorRestos = restos;
		o.cuenta = cuenta;
		o.resistencia = resistencia;
		return o;
	}

	static void Anillo(Transform padre, string nombre, float radio, float z, Material m, int puntos, string etiqueta, bool centro = false) {
		var a = Pieza(PrimitiveType.Cylinder, nombre, padre, new Vector3(0f, 0f, z), new Vector3(radio * 2f, 0.02f, radio * 2f), m,
			new Vector3(90f, 0f, 0f), 2, Capas.Objetivos);
		var zona = a.AddComponent<ZonaPuntos>();
		zona.puntos = puntos;
		zona.etiqueta = etiqueta;
		zona.esCentro = centro;
	}

	//Diana redonda sobre un caballete. Su frente mira hacia -Z.
	static GameObject Diana(string nombre, int[] puntos, Material medio, bool conPatas) {
		var go = Vacio(nombre, null, Vector3.zero, Capas.Objetivos);
		var t = go.transform;
		var cara = Vacio("Cara", t, new Vector3(0f, 1.55f, 0f), Capas.Objetivos).transform;
		Anillo(cara, "Borde", 0.58f, 0f, mDianaBlanca, puntos[0], "BORDE");
		Anillo(cara, "AroNegro", 0.5f, -0.006f, mDianaNegra, puntos[0], "BORDE");
		Anillo(cara, "AroBlanco", 0.46f, -0.012f, mDianaBlanca, puntos[0], "BORDE");
		Anillo(cara, "Medio", 0.34f, -0.018f, medio, puntos[1], "ANILLO");
		Anillo(cara, "Centro", 0.14f, -0.024f, mDianaAmarilla, puntos[2], "¡CENTRO!", true);
		Cilindro("Respaldo", cara, new Vector3(0f, 0f, 0.035f), new Vector3(1.22f, 0.025f, 1.22f), mMaderaOscura, new Vector3(90f, 0f, 0f), 0);
		if (conPatas) {
			Cubo("Pata", t, new Vector3(-0.45f, 0.62f, 0.12f), new Vector3(0.08f, 1.3f, 0.08f), mMadera, new Vector3(-6f, 0f, 8f), 0);
			Cubo("Pata", t, new Vector3(0.45f, 0.62f, 0.12f), new Vector3(0.08f, 1.3f, 0.08f), mMadera, new Vector3(-6f, 0f, -8f), 0);
			Cubo("Apoyo", t, new Vector3(0f, 0.95f, 0.15f), new Vector3(1.1f, 0.08f, 0.08f), mMadera, null, 0);
		} else {
			Cubo("Soporte", t, new Vector3(0f, 0.55f, 0.1f), new Vector3(0.1f, 1.1f, 0.1f), mMetalOscuro, null, 0);
			Cubo("Carro", t, new Vector3(0f, 0.08f, 0.1f), new Vector3(0.5f, 0.16f, 0.4f), mRayas, null, 0);
		}
		Capa(go, Capas.Objetivos);
		return go;
	}

	static void CrearObjetivos() {
		pDiana = Diana("Diana", new[] { 10, 25, 50 }, mDianaRoja, true);
		NuevoObjetivo(pDiana, TipoObjetivo.Diana, "Diana", 10, new Color(0.85f, 0.75f, 0.55f));
		pDiana = Guardar(pDiana, "Diana");

		pDianaMovil = Diana("DianaMovil", new[] { 20, 50, 100 }, mDianaAzul, false);
		NuevoObjetivo(pDianaMovil, TipoObjetivo.Diana, "Diana móvil", 20, new Color(0.85f, 0.75f, 0.55f));
		pDianaMovil = Guardar(pDianaMovil, "DianaMovil");

		//Botella de vidrio (va encima de un barril o una caja)
		var bot = Vacio("Botella", null, Vector3.zero, Capas.Objetivos);
		Cilindro("Cuerpo", bot.transform, new Vector3(0f, 0.13f, 0f), new Vector3(0.13f, 0.13f, 0.13f), mVidrio, null, 1);
		Cilindro("Hombro", bot.transform, new Vector3(0f, 0.28f, 0f), new Vector3(0.09f, 0.03f, 0.09f), mVidrio, null, 0);
		Cilindro("Cuello", bot.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.05f, 0.06f, 0.05f), mVidrio, null, 1);
		Cilindro("Tapa", bot.transform, new Vector3(0f, 0.42f, 0f), new Vector3(0.055f, 0.012f, 0.055f), mLaton, null, 0);
		Capa(bot, Capas.Objetivos);
		NuevoObjetivo(bot, TipoObjetivo.Botella, "Botella", 15, new Color(0.3f, 0.75f, 0.4f));
		pBotella = Guardar(bot, "Botella");

		GameObject Globo(string nombre, Material m) {
			var g = Vacio(nombre, null, Vector3.zero, Capas.Objetivos);
			var flota = Vacio("Flotante", g.transform, Vector3.zero, Capas.Objetivos);
			Esfera("Globo", flota.transform, new Vector3(0f, 0f, 0f), new Vector3(0.6f, 0.72f, 0.6f), m, 1);
			Cilindro("Nudo", flota.transform, new Vector3(0f, -0.38f, 0f), new Vector3(0.06f, 0.03f, 0.06f), m, null, 0);
			Cilindro("Hilo", flota.transform, new Vector3(0f, -0.9f, 0f), new Vector3(0.01f, 0.5f, 0.01f), mCuerda, null, 0);
			var mov = flota.AddComponent<Movimiento>();
			mov.modo = Movimiento.Modo.Flotar;
			mov.eje = Vector3.right;
			mov.distancia = 0.35f;
			mov.velocidad = 1.3f;
			Capa(g, Capas.Objetivos);
			NuevoObjetivo(g, TipoObjetivo.Globo, "Globo", 30, m.color);
			return Guardar(g, nombre);
		}
		pGlobo = Globo("Globo", mGlobo);
		pGloboAzul = Globo("GloboAzul", mGloboAzul);

		//Placa de acero pequeña y lejana: tiro de precisión
		var placa = Vacio("Placa", null, Vector3.zero, Capas.Objetivos);
		Cubo("Poste", placa.transform, new Vector3(0f, 1.1f, 0.05f), new Vector3(0.08f, 2.2f, 0.08f), mMetalOscuro, null, 0);
		Pieza(PrimitiveType.Cylinder, "Disco", placa.transform, new Vector3(0f, 2.2f, 0f), new Vector3(0.45f, 0.02f, 0.45f), mAcero, new Vector3(90f, 0f, 0f), 2);
		Cilindro("Centro", placa.transform, new Vector3(0f, 2.2f, -0.022f), new Vector3(0.12f, 0.005f, 0.12f), mDianaRoja, new Vector3(90f, 0f, 0f), 0);
		Capa(placa, Capas.Objetivos);
		NuevoObjetivo(placa, TipoObjetivo.Placa, "Placa de precisión", 100, new Color(0.8f, 0.8f, 0.85f));
		pPlaca = Guardar(placa, "Placa");

		//Silueta de civil: si le das, pierdes puntos
		var civil = Vacio("Civil", null, Vector3.zero, Capas.Objetivos);
		var c = civil.transform;
		Cubo("Torso", c, new Vector3(0f, 1.25f, 0f), new Vector3(0.62f, 0.75f, 0.04f), mCivil, null, 1);
		Pieza(PrimitiveType.Cylinder, "Cabeza", c, new Vector3(0f, 1.82f, 0f), new Vector3(0.36f, 0.02f, 0.36f), mCivil, new Vector3(90f, 0f, 0f), 2);
		Cubo("Franja", c, new Vector3(0f, 1.3f, -0.025f), new Vector3(0.62f, 0.1f, 0.01f), mDianaBlanca, null, 0);
		Cubo("Piernas", c, new Vector3(0f, 0.65f, 0f), new Vector3(0.45f, 0.5f, 0.04f), mCivil, null, 1);
		Cubo("Pie", c, new Vector3(0f, 0.2f, 0.1f), new Vector3(0.6f, 0.4f, 0.4f), mMadera, null, 0);
		Capa(civil, Capas.Objetivos);
		NuevoObjetivo(civil, TipoObjetivo.Civil, "Civil", -50, new Color(0.3f, 0.55f, 0.95f), false);
		Letrero(c, "NO DISPARAR", new Vector3(0f, 2.25f, -0.02f), 0.55f, new Color(0.85f, 0.1f, 0.1f), 1f, true);
		pCivil = Guardar(civil, "Civil");

		//Antena de comunicaciones enemiga (nivel 2): resiste 3 impactos
		var antena = Vacio("Antena", null, Vector3.zero, Capas.Objetivos);
		var a = antena.transform;
		Cubo("Base", a, new Vector3(0f, 0.25f, 0f), new Vector3(1.2f, 0.5f, 1.2f), mConcretoOscuro, null, 1);
		Cilindro("Mastil", a, new Vector3(0f, 2.5f, 0f), new Vector3(0.25f, 2f, 0.25f), mRobotOscuro, null, 1);
		Cubo("Caja", a, new Vector3(0f, 1.2f, -0.2f), new Vector3(0.6f, 0.7f, 0.3f), mRobot, null, 1);
		var plato = Pieza(PrimitiveType.Sphere, "Plato", a, new Vector3(0f, 3.6f, -0.25f), new Vector3(1.4f, 1.4f, 0.3f), mRobot, new Vector3(-20f, 0f, 0f), 1);
		Cubo("Emisor", a, new Vector3(0f, 3.65f, -0.65f), new Vector3(0.08f, 0.08f, 0.6f), mRobotOscuro, new Vector3(-20f, 0f, 0f), 0);
		Esfera("Baliza", a, new Vector3(0f, 4.6f, 0f), Vector3.one * 0.2f, mLuzRoja, 0);
		var giro = plato.AddComponent<Movimiento>();
		giro.modo = Movimiento.Modo.Girar;
		giro.eje = Vector3.up;
		giro.velocidad = 0f;
		Capa(antena, Capas.Objetivos);
		NuevoObjetivo(antena, TipoObjetivo.Antena, "Antena enemiga", 150, new Color(0.35f, 0.36f, 0.4f), false, 3);
		pAntena = Guardar(antena, "Antena");
	}

	// ------------------------------------------------ Recogibles

	static void CrearRecogibles() {
		var mun = Vacio("Municion", null, Vector3.zero);
		var modelo = Vacio("Modelo", mun.transform, new Vector3(0f, 0.55f, 0f));
		var m = modelo.transform;
		Cubo("Caja", m, Vector3.zero, new Vector3(0.6f, 0.32f, 0.36f), mMunicion, null, 0);
		Cubo("Tapa", m, new Vector3(0f, 0.17f, 0f), new Vector3(0.62f, 0.04f, 0.38f), mMetalOscuro, null, 0);
		Cubo("Franja", m, new Vector3(0f, 0f, -0.181f), new Vector3(0.5f, 0.06f, 0.01f), mBrilloAmarillo, null, 0);
		Cubo("Franja", m, new Vector3(0f, 0f, 0.181f), new Vector3(0.5f, 0.06f, 0.01f), mBrilloAmarillo, null, 0);
		for (int i = 0; i < 5; i++) {
			Cilindro("Bala", m, new Vector3(-0.2f + i * 0.1f, 0.27f, 0f), new Vector3(0.05f, 0.08f, 0.05f), mLaton, null, 0);
			Esfera("Punta", m, new Vector3(-0.2f + i * 0.1f, 0.36f, 0f), new Vector3(0.05f, 0.07f, 0.05f), mLaton, 0);
		}
		Cilindro("Aura", mun.transform, new Vector3(0f, 0.02f, 0f), new Vector3(1.4f, 0.01f, 1.4f), mBrilloAmarillo, null, 0);
		var luz = Vacio("Luz", mun.transform, new Vector3(0f, 1f, 0f)).AddComponent<Light>();
		luz.type = LightType.Point;
		luz.color = new Color(1f, 0.8f, 0.3f);
		luz.range = 4f;
		luz.intensity = 1.5f;
		var r = mun.AddComponent<Recogible>();
		r.tipo = TipoRecogible.Municion;
		r.cantidad = 20;
		r.modelo = m;
		pMunicion = Guardar(mun, "Municion");

		var bot = Vacio("Botiquin", null, Vector3.zero);
		modelo = Vacio("Modelo", bot.transform, new Vector3(0f, 0.6f, 0f));
		m = modelo.transform;
		Cubo("Caja", m, Vector3.zero, new Vector3(0.55f, 0.4f, 0.3f), mBotiquin, null, 0);
		foreach (float z in new[] { -0.155f, 0.155f }) {
			Cubo("Cruz", m, new Vector3(0f, 0f, z), new Vector3(0.28f, 0.08f, 0.01f), mCruz, null, 0);
			Cubo("Cruz", m, new Vector3(0f, 0f, z), new Vector3(0.08f, 0.28f, 0.01f), mCruz, null, 0);
		}
		Cubo("Asa", m, new Vector3(0f, 0.24f, 0f), new Vector3(0.22f, 0.06f, 0.06f), mMetalOscuro, null, 0);
		Cilindro("Aura", bot.transform, new Vector3(0f, 0.02f, 0f), new Vector3(1.4f, 0.01f, 1.4f), mBrilloVerde, null, 0);
		luz = Vacio("Luz", bot.transform, new Vector3(0f, 1f, 0f)).AddComponent<Light>();
		luz.type = LightType.Point;
		luz.color = new Color(0.4f, 1f, 0.5f);
		luz.range = 4f;
		luz.intensity = 1.5f;
		r = bot.AddComponent<Recogible>();
		r.tipo = TipoRecogible.Botiquin;
		r.cantidad = 1;
		r.modelo = m;
		pBotiquin = Guardar(bot, "Botiquin");
	}

	// ------------------------------------------------ Utilería del escenario

	static void CrearUtileria() {
		var go = Vacio("Caja", null, Vector3.zero);
		Cubo("Caja", go.transform, new Vector3(0f, 0.5f, 0f), Vector3.one, mCaja);
		pCaja = Guardar(go, "Caja");

		go = Vacio("CajaGrande", null, Vector3.zero);
		Cubo("Caja", go.transform, new Vector3(0f, 0.75f, 0f), new Vector3(2.2f, 1.5f, 1.5f), mCaja);
		pCajaGrande = Guardar(go, "CajaGrande");

		GameObject Barril(string nombre, Material m) {
			var b = Vacio(nombre, null, Vector3.zero);
			Cilindro("Barril", b.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.6f, 0.45f, 0.6f), m);
			Cilindro("Aro", b.transform, new Vector3(0f, 0.2f, 0f), new Vector3(0.62f, 0.02f, 0.62f), mMetalOscuro, null, 0);
			Cilindro("Aro", b.transform, new Vector3(0f, 0.7f, 0f), new Vector3(0.62f, 0.02f, 0.62f), mMetalOscuro, null, 0);
			Cilindro("Tapa", b.transform, new Vector3(0f, 0.905f, 0f), new Vector3(0.56f, 0.005f, 0.56f), mMetalOscuro, null, 0);
			return Guardar(b, nombre);
		}
		pBarril = Barril("Barril", mBarril);
		pBarrilRojo = Barril("BarrilRojo", mBarrilRojo);

		//Muro bajo de sacos de arena (3 m de largo)
		go = Vacio("Sacos", null, Vector3.zero);
		for (int fila = 0; fila < 3; fila++) {
			int n = 5 - (fila == 2 ? 1 : 0);
			for (int i = 0; i < n; i++) {
				float x = -1.2f + i * 0.6f + (fila % 2) * 0.3f;
				Capsula("Saco", go.transform, new Vector3(x, 0.17f + fila * 0.28f, 0f), new Vector3(0.36f, 0.33f, 0.5f), mSaco, new Vector3(0f, 0f, 90f), 0);
			}
		}
		var col = go.AddComponent<BoxCollider>();
		col.center = new Vector3(0f, 0.45f, 0f);
		col.size = new Vector3(3.1f, 0.9f, 0.55f);
		pSacos = Guardar(go, "Sacos");

		go = Vacio("Arbol", null, Vector3.zero);
		Cilindro("Tronco", go.transform, new Vector3(0f, 1.5f, 0f), new Vector3(0.4f, 1.5f, 0.4f), mTronco);
		Esfera("Copa", go.transform, new Vector3(0f, 3.6f, 0f), new Vector3(2.8f, 2.4f, 2.8f), mHoja, 0);
		Esfera("Copa", go.transform, new Vector3(0.7f, 3.1f, 0.4f), new Vector3(1.8f, 1.6f, 1.8f), mHojaOscura, 0);
		Esfera("Copa", go.transform, new Vector3(-0.6f, 4.2f, -0.3f), new Vector3(1.8f, 1.5f, 1.8f), mHoja, 0);
		pArbol = Guardar(go, "Arbol");

		go = Vacio("Pino", null, Vector3.zero);
		Cilindro("Tronco", go.transform, new Vector3(0f, 1f, 0f), new Vector3(0.35f, 1f, 0.35f), mTronco);
		for (int i = 0; i < 4; i++) {
			float s = 2.6f - i * 0.55f;
			Esfera("Rama", go.transform, new Vector3(0f, 2f + i * 1.05f, 0f), new Vector3(s, 1.3f, s), i % 2 == 0 ? mHojaOscura : mHoja, 0);
		}
		pPino = Guardar(go, "Pino");

		go = Vacio("Roca", null, Vector3.zero);
		Esfera("Roca", go.transform, new Vector3(0f, 0.3f, 0f), new Vector3(1.6f, 1.1f, 1.3f), mRoca, 1);
		Esfera("Roca", go.transform, new Vector3(0.6f, 0.2f, 0.3f), new Vector3(0.9f, 0.7f, 0.8f), mRoca, 0);
		pRoca = Guardar(go, "Roca");

		//Torre de vigilancia
		go = Vacio("Torre", null, Vector3.zero);
		var tt = go.transform;
		foreach (var p in new[] { new Vector3(-1.2f, 0f, -1.2f), new Vector3(1.2f, 0f, -1.2f), new Vector3(-1.2f, 0f, 1.2f), new Vector3(1.2f, 0f, 1.2f) })
			Cubo("Pata", tt, p + Vector3.up * 2.5f, new Vector3(0.22f, 5f, 0.22f), mMadera);
		Cubo("Riostra", tt, new Vector3(0f, 1.8f, -1.2f), new Vector3(2.6f, 0.12f, 0.12f), mMadera, new Vector3(0f, 0f, 35f), 0);
		Cubo("Riostra", tt, new Vector3(0f, 1.8f, 1.2f), new Vector3(2.6f, 0.12f, 0.12f), mMadera, new Vector3(0f, 0f, -35f), 0);
		Cubo("Piso", tt, new Vector3(0f, 5f, 0f), new Vector3(3.2f, 0.2f, 3.2f), mMadera);
		Cubo("Baranda", tt, new Vector3(0f, 5.55f, -1.55f), new Vector3(3.2f, 0.9f, 0.1f), mMaderaOscura);
		Cubo("Baranda", tt, new Vector3(0f, 5.55f, 1.55f), new Vector3(3.2f, 0.9f, 0.1f), mMaderaOscura);
		Cubo("Baranda", tt, new Vector3(-1.55f, 5.55f, 0f), new Vector3(0.1f, 0.9f, 3.2f), mMaderaOscura);
		Cubo("Baranda", tt, new Vector3(1.55f, 5.55f, 0f), new Vector3(0.1f, 0.9f, 3.2f), mMaderaOscura);
		Cubo("Techo", tt, new Vector3(0f, 7.3f, 0f), new Vector3(3.8f, 0.15f, 3.8f), mLona, new Vector3(0f, 0f, 4f));
		foreach (var p in new[] { new Vector3(-1.4f, 0f, -1.4f), new Vector3(1.4f, 0f, -1.4f), new Vector3(-1.4f, 0f, 1.4f), new Vector3(1.4f, 0f, 1.4f) })
			Cubo("Poste", tt, p + Vector3.up * 6.3f, new Vector3(0.12f, 2f, 0.12f), mMadera, null, 0);
		var foco = Vacio("Foco", tt, new Vector3(0f, 6.6f, -1.3f)).AddComponent<Light>();
		foco.type = LightType.Spot;
		foco.spotAngle = 50f;
		foco.range = 25f;
		foco.intensity = 1.2f;
		foco.color = new Color(1f, 0.95f, 0.8f);
		foco.transform.localRotation = Quaternion.Euler(35f, 180f, 0f);
		pTorre = Guardar(go, "Torre");

		//Carpa militar
		go = Vacio("Carpa", null, Vector3.zero);
		Cubo("LonaI", go.transform, new Vector3(-1.05f, 1.15f, 0f), new Vector3(0.1f, 2.7f, 5f), mLona, new Vector3(0f, 0f, -38f));
		Cubo("LonaD", go.transform, new Vector3(1.05f, 1.15f, 0f), new Vector3(0.1f, 2.7f, 5f), mLona, new Vector3(0f, 0f, 38f));
		Cubo("Fondo", go.transform, new Vector3(0f, 1f, 2.45f), new Vector3(2.6f, 2f, 0.08f), mLona, null, 0);
		Cubo("Cumbrera", go.transform, new Vector3(0f, 2.2f, 0f), new Vector3(0.12f, 0.12f, 5.2f), mMadera, null, 0);
		pCarpa = Guardar(go, "Carpa");

		GameObject Contenedor(string nombre, Material m) {
			var c = Vacio(nombre, null, Vector3.zero);
			Cubo("Cuerpo", c.transform, new Vector3(0f, 1.3f, 0f), new Vector3(2.45f, 2.6f, 6f), m);
			for (int i = -5; i <= 5; i++) {
				Cubo("Nervio", c.transform, new Vector3(1.24f, 1.3f, i * 0.5f), new Vector3(0.04f, 2.5f, 0.12f), m, null, 0);
				Cubo("Nervio", c.transform, new Vector3(-1.24f, 1.3f, i * 0.5f), new Vector3(0.04f, 2.5f, 0.12f), m, null, 0);
			}
			Cubo("Puerta", c.transform, new Vector3(0f, 1.3f, 3.01f), new Vector3(2.3f, 2.45f, 0.04f), mMetalOscuro, null, 0);
			return Guardar(c, nombre);
		}
		pContenedor = Contenedor("Contenedor", mContenedor);
		pContenedorAzul = Contenedor("ContenedorAzul", mContenedorAzul);

		go = Vacio("Bandera", null, Vector3.zero);
		Cilindro("Mastil", go.transform, new Vector3(0f, 3.5f, 0f), new Vector3(0.1f, 3.5f, 0.1f), mAcero);
		Esfera("Punta", go.transform, new Vector3(0f, 7.05f, 0f), Vector3.one * 0.18f, mLaton, 0);
		var tela = Vacio("Tela", go.transform, new Vector3(0f, 6.3f, 0f));
		Cubo("Tela", tela.transform, new Vector3(0.8f, 0f, 0f), new Vector3(1.6f, 1f, 0.02f), mBandera, null, 0);
		Cubo("Franja", tela.transform, new Vector3(0.8f, 0f, -0.012f), new Vector3(1.6f, 0.2f, 0.01f), mDianaAmarilla, null, 0);
		var ondea = tela.AddComponent<Movimiento>();
		ondea.modo = Movimiento.Modo.Girar;
		ondea.eje = Vector3.up;
		ondea.velocidad = 0.3f;
		pBandera = Guardar(go, "Bandera");

		go = Vacio("Farol", null, Vector3.zero);
		Cilindro("Poste", go.transform, new Vector3(0f, 2f, 0f), new Vector3(0.12f, 2f, 0.12f), mMetalOscuro);
		Cubo("Brazo", go.transform, new Vector3(0f, 3.95f, 0.35f), new Vector3(0.08f, 0.08f, 0.8f), mMetalOscuro, null, 0);
		Cubo("Lampara", go.transform, new Vector3(0f, 3.85f, 0.7f), new Vector3(0.35f, 0.12f, 0.25f), mBrilloAmarillo, null, 0);
		var luzFarol = Vacio("Luz", go.transform, new Vector3(0f, 3.6f, 0.7f)).AddComponent<Light>();
		luzFarol.type = LightType.Point;
		luzFarol.range = 12f;
		luzFarol.intensity = 1.6f;
		luzFarol.color = new Color(1f, 0.8f, 0.5f);
		pFarol = Guardar(go, "Farol");

		//Pilares de la guarida del Coloso
		go = Vacio("Pilar", null, Vector3.zero);
		Cilindro("Columna", go.transform, new Vector3(0f, 4f, 0f), new Vector3(2.4f, 4f, 2.4f), mRocaOscura);
		Cubo("Base", go.transform, new Vector3(0f, 0.3f, 0f), new Vector3(3.2f, 0.6f, 3.2f), mRocaOscura);
		Cubo("Capitel", go.transform, new Vector3(0f, 8f, 0f), new Vector3(3f, 0.6f, 3f), mRocaOscura, null, 0);
		Cilindro("Runa", go.transform, new Vector3(0f, 3f, 0f), new Vector3(2.45f, 0.08f, 2.45f), mLava, null, 0);
		pPilar = Guardar(go, "Pilar");

		go = Vacio("PilarRoto", null, Vector3.zero);
		Cilindro("Columna", go.transform, new Vector3(0f, 1.6f, 0f), new Vector3(2.4f, 1.6f, 2.4f), mRocaOscura);
		Cubo("Base", go.transform, new Vector3(0f, 0.3f, 0f), new Vector3(3.2f, 0.6f, 3.2f), mRocaOscura);
		Cilindro("Trozo", go.transform, new Vector3(2.4f, 0.6f, 1f), new Vector3(1.8f, 1.2f, 1.8f), mRocaOscura, new Vector3(80f, 30f, 0f));
		pPilarRoto = Guardar(go, "PilarRoto");

		//Tramo de valla de 4 m
		go = Vacio("Valla", null, Vector3.zero);
		Cubo("Poste", go.transform, new Vector3(-2f, 0.7f, 0f), new Vector3(0.12f, 1.4f, 0.12f), mMaderaOscura, null, 0);
		Cubo("Poste", go.transform, new Vector3(2f, 0.7f, 0f), new Vector3(0.12f, 1.4f, 0.12f), mMaderaOscura, null, 0);
		Cubo("Tabla", go.transform, new Vector3(0f, 1.1f, 0f), new Vector3(4.1f, 0.12f, 0.05f), mMadera, null, 0);
		Cubo("Tabla", go.transform, new Vector3(0f, 0.55f, 0f), new Vector3(4.1f, 0.12f, 0.05f), mMadera, null, 0);
		pValla = Guardar(go, "Valla");

		//Búnker de concreto con puerta y ventanas
		go = Vacio("Bunker", null, Vector3.zero);
		var bt = go.transform;
		Cubo("Cuerpo", bt, new Vector3(0f, 2f, 0f), new Vector3(10f, 4f, 7f), mConcreto);
		Cubo("Techo", bt, new Vector3(0f, 4.15f, 0f), new Vector3(10.6f, 0.3f, 7.6f), mConcretoOscuro);
		Cubo("Puerta", bt, new Vector3(0f, 1.3f, -3.51f), new Vector3(1.8f, 2.6f, 0.05f), mMetalOscuro, null, 0);
		Cubo("Ventana", bt, new Vector3(-3f, 2.6f, -3.51f), new Vector3(2f, 0.5f, 0.05f), mDianaNegra, null, 0);
		Cubo("Ventana", bt, new Vector3(3f, 2.6f, -3.51f), new Vector3(2f, 0.5f, 0.05f), mDianaNegra, null, 0);
		Cubo("Franja", bt, new Vector3(0f, 0.25f, -3.52f), new Vector3(10f, 0.5f, 0.05f), mRayas, null, 0);
		Cilindro("Antena", bt, new Vector3(3.5f, 5.5f, 2f), new Vector3(0.1f, 1.4f, 0.1f), mMetalOscuro, null, 0);
		pBunker = Guardar(go, "Bunker");

		//Muro de concreto (8 m de largo, 4 m de alto)
		go = Vacio("Muro", null, Vector3.zero);
		Cubo("Muro", go.transform, new Vector3(0f, 2f, 0f), new Vector3(8f, 4f, 0.8f), mConcreto);
		Cubo("Remate", go.transform, new Vector3(0f, 4.1f, 0f), new Vector3(8.1f, 0.2f, 1f), mConcretoOscuro, null, 0);
		pMuro = Guardar(go, "Muro");

		go = Vacio("Poste", null, Vector3.zero);
		Cubo("Poste", go.transform, new Vector3(0f, 1f, 0f), new Vector3(0.15f, 2f, 0.15f), mRayas);
		pPoste = Guardar(go, "Poste");
	}

	// ------------------------------------------------ Letreros con texto (lienzo en el mundo)

	static GameObject Letrero(Transform padre, string texto, Vector3 pos, float alto, Color colorTexto, float ancho = 3f, bool conFondo = false,
		Color? fondo = null, int tamano = 120) {
		var go = new GameObject("Letrero", typeof(RectTransform));
		go.transform.SetParent(padre, false);
		go.transform.localPosition = pos;
		go.transform.localRotation = Quaternion.identity;
		var canvas = go.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		var rt = (RectTransform)go.transform;
		rt.sizeDelta = new Vector2(ancho * 400f, alto * 400f);
		rt.localScale = Vector3.one / 400f;
		if (conFondo) {
			var img = UI("Fondo", go.transform).AddComponent<Image>();
			Estirar(img.rectTransform);
			img.color = fondo ?? Color.white;
			img.raycastTarget = false;
		}
		var t = UI("Texto", go.transform).AddComponent<Text>();
		Estirar(t.rectTransform);
		t.font = fuente;
		t.text = texto;
		t.fontSize = tamano;
		t.fontStyle = FontStyle.Bold;
		t.alignment = TextAnchor.MiddleCenter;
		t.color = colorTexto;
		t.resizeTextForBestFit = true;
		t.resizeTextMaxSize = tamano;
		t.resizeTextMinSize = 10;
		t.raycastTarget = false;
		return go;
	}
}
