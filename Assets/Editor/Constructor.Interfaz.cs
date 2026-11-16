using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//HUD de los niveles, menú de entrada y pantalla final
public static partial class ConstructorProyecto {

	static readonly Color AMBAR = new Color(1f, 0.74f, 0.18f);
	static readonly Color FONDO_PANEL = new Color(0.05f, 0.07f, 0.09f, 0.86f);
	static readonly Color GRIS_TEXTO = new Color(0.72f, 0.78f, 0.84f);

	static GameObject UI(string nombre, Transform padre) {
		var go = new GameObject(nombre, typeof(RectTransform));
		go.transform.SetParent(padre, false);
		return go;
	}

	static RectTransform Rect(GameObject go, Vector2 ancla, Vector2 pos, Vector2 tam, Vector2? pivote = null) {
		var rt = go.GetComponent<RectTransform>();
		rt.anchorMin = rt.anchorMax = ancla;
		rt.pivot = pivote ?? ancla;
		rt.anchoredPosition = pos;
		rt.sizeDelta = tam;
		return rt;
	}

	static void Estirar(RectTransform rt) {
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = rt.offsetMax = Vector2.zero;
	}

	static Text Texto(Transform padre, string nombre, string contenido, int tamano, TextAnchor alineacion, Vector2 ancla, Vector2 pos, Vector2 tam,
		Color? color = null, bool sombra = true, FontStyle estilo = FontStyle.Bold) {
		var go = UI(nombre, padre);
		Rect(go, ancla, pos, tam);
		var t = go.AddComponent<Text>();
		t.font = fuente;
		t.text = contenido;
		t.fontSize = tamano;
		t.fontStyle = estilo;
		t.alignment = alineacion;
		t.color = color ?? Color.white;
		t.horizontalOverflow = HorizontalWrapMode.Wrap;
		t.verticalOverflow = VerticalWrapMode.Overflow;
		t.raycastTarget = false;
		t.supportRichText = true;
		if (sombra) {
			var s = go.AddComponent<Shadow>();
			s.effectColor = new Color(0f, 0f, 0f, 0.75f);
			s.effectDistance = new Vector2(2f, -3f);
		}
		return t;
	}

	static Image Imagen(Transform padre, string nombre, Sprite s, Vector2 ancla, Vector2 pos, Vector2 tam, Color? color = null) {
		var go = UI(nombre, padre);
		Rect(go, ancla, pos, tam);
		var img = go.AddComponent<Image>();
		img.sprite = s;
		img.color = color ?? Color.white;
		img.raycastTarget = false;
		if (s != null && s.border != Vector4.zero) img.type = Image.Type.Sliced;
		return img;
	}

	static Image Fondo(Transform padre, string nombre, Vector2 ancla, Vector2 pos, Vector2 tam, Color? color = null) {
		var img = Imagen(padre, nombre, sPanel, ancla, pos, tam, color ?? FONDO_PANEL);
		img.pixelsPerUnitMultiplier = 1.5f;
		return img;
	}

	static Image Cubrir(Transform padre, string nombre, Color color) {
		var img = UI(nombre, padre).AddComponent<Image>();
		Estirar(img.rectTransform);
		img.sprite = sBlanco;
		img.color = color;
		return img;
	}

	static Canvas Lienzo(string nombre, int orden) {
		var go = new GameObject(nombre, typeof(RectTransform));
		var canvas = go.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = orden;
		var escala = go.AddComponent<CanvasScaler>();
		escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		escala.referenceResolution = new Vector2(1920, 1080);
		escala.matchWidthOrHeight = 0.5f;
		go.AddComponent<GraphicRaycaster>();
		return canvas;
	}

	static void SistemaEventos() {
		var es = new GameObject("EventSystem");
		es.AddComponent<EventSystem>();
		es.AddComponent<StandaloneInputModule>();
	}

	static Button Boton(Transform padre, string texto, Vector2 ancla, Vector2 pos, Vector2 tam, bool principal, UnityAction accion) {
		var go = UI("Boton_" + texto.Replace(" ", ""), padre);
		Rect(go, ancla, pos, tam);
		var img = go.AddComponent<Image>();
		img.sprite = sPanel;
		img.type = Image.Type.Sliced;
		img.pixelsPerUnitMultiplier = 1.5f;
		img.color = principal ? AMBAR : new Color(0.16f, 0.2f, 0.24f, 0.95f);
		var b = go.AddComponent<Button>();
		var colores = b.colors;
		colores.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
		colores.selectedColor = new Color(1.1f, 1.1f, 1.1f);
		colores.pressedColor = new Color(0.75f, 0.75f, 0.75f);
		colores.fadeDuration = 0.08f;
		b.colors = colores;
		UnityEventTools.AddPersistentListener(b.onClick, accion);
		var t = Texto(go.transform, "Texto", texto, (int)Mathf.Min(40f, tam.y * 0.42f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), Vector2.zero, tam,
			principal ? new Color(0.08f, 0.07f, 0.04f) : Color.white, !principal);
		return b;
	}

	static void NombreYMatricula(Transform lienzo) {
		Texto(lienzo, "NombreMatricula", NOMBRE + "\nMatrícula " + MATRICULA, 22, TextAnchor.UpperRight,
			new Vector2(1f, 1f), new Vector2(-24f, -18f), new Vector2(420f, 64f), new Color(1f, 1f, 1f, 0.75f));
	}

	// ------------------------------------------------ HUD de los niveles

	static HUD CrearHUD() {
		var lienzo = Lienzo("HUD", 10);
		var l = lienzo.transform;
		var hud = lienzo.gameObject.AddComponent<HUD>();

		var vineta = Cubrir(l, "Vineta", new Color(1f, 1f, 1f, 0.55f));
		vineta.sprite = sVineta;
		vineta.raycastTarget = false;
		hud.flashDano = Cubrir(l, "FlashDano", new Color(0.8f, 0f, 0f, 0f));
		hud.flashDano.sprite = sVineta;
		hud.flashDano.raycastTarget = false;

		//Arriba a la izquierda: nivel y misión
		var izq = new Vector2(0f, 1f);
		Fondo(l, "FondoMision", izq, new Vector2(24f, -20f), new Vector2(640f, 110f));
		hud.txtNivel = Texto(l, "Nivel", "NIVEL 1", 30, TextAnchor.UpperLeft, izq, new Vector2(48f, -36f), new Vector2(600f, 40f));
		hud.txtMision = Texto(l, "Mision", "", 28, TextAnchor.UpperLeft, izq, new Vector2(48f, -78f), new Vector2(600f, 40f), AMBAR);

		//Arriba al centro: puntos
		var arriba = new Vector2(0.5f, 1f);
		Texto(l, "EtiquetaPuntos", "PUNTOS", 24, TextAnchor.UpperCenter, arriba, new Vector2(0f, -18f), new Vector2(300f, 30f), GRIS_TEXTO);
		hud.txtPuntos = Texto(l, "Puntos", "0", 64, TextAnchor.UpperCenter, arriba, new Vector2(0f, -44f), new Vector2(500f, 80f));

		NombreYMatricula(l);

		//Abajo a la izquierda: vidas
		var abajoIzq = new Vector2(0f, 0f);
		Fondo(l, "FondoVidas", abajoIzq, new Vector2(24f, 24f), new Vector2(430f, 110f));
		Texto(l, "EtiquetaVidas", "VIDAS", 22, TextAnchor.UpperLeft, abajoIzq, new Vector2(48f, 104f), new Vector2(200f, 30f), GRIS_TEXTO);
		hud.corazones = new Image[Juego.VIDAS_MAX];
		for (int i = 0; i < Juego.VIDAS_MAX; i++) {
			Imagen(l, "CorazonVacio" + i, sCorazon, abajoIzq, new Vector2(48f + i * 76f, 34f), new Vector2(62f, 62f), new Color(0f, 0f, 0f, 0.45f));
			hud.corazones[i] = Imagen(l, "Corazon" + i, sCorazon, abajoIzq, new Vector2(48f + i * 76f, 34f), new Vector2(62f, 62f));
		}

		//Abajo a la derecha: munición
		var abajoDer = new Vector2(1f, 0f);
		Fondo(l, "FondoMunicion", abajoDer, new Vector2(-24f, 24f), new Vector2(330f, 130f));
		Imagen(l, "IconoBala", sBala, abajoDer, new Vector2(-294f, 40f), new Vector2(32f, 96f));
		hud.txtMunicion = Texto(l, "Municion", "0", 92, TextAnchor.LowerRight, abajoDer, new Vector2(-132f, 26f), new Vector2(158f, 110f));
		Texto(l, "EtiquetaMunicion", "BALAS", 24, TextAnchor.LowerLeft, abajoDer, new Vector2(-36f, 46f), new Vector2(92f, 30f), GRIS_TEXTO);

		//Mira en el centro
		var centro = new Vector2(0.5f, 0.5f);
		var mira = UI("Mira", l).transform;
		Rect(mira.gameObject, centro, Vector2.zero, new Vector2(80f, 80f));
		foreach (var (pos, tam) in new[] {
			(new Vector2(0f, 18f), new Vector2(3f, 14f)), (new Vector2(0f, -18f), new Vector2(3f, 14f)),
			(new Vector2(18f, 0f), new Vector2(14f, 3f)), (new Vector2(-18f, 0f), new Vector2(14f, 3f)) }) {
			Imagen(mira, "Linea", sBlanco, centro, pos, tam + new Vector2(2f, 2f), new Color(0f, 0f, 0f, 0.6f));
			Imagen(mira, "Linea", sBlanco, centro, pos, tam, new Color(1f, 1f, 1f, 0.9f));
		}
		Imagen(mira, "Punto", sCirculo, centro, Vector2.zero, new Vector2(5f, 5f), new Color(1f, 0.3f, 0.2f, 0.95f));
		hud.marca = Imagen(l, "Marca", sMarca, centro, Vector2.zero, new Vector2(56f, 56f));
		hud.txtPopup = Texto(l, "Popup", "", 60, TextAnchor.MiddleCenter, centro, new Vector2(0f, 120f), new Vector2(700f, 80f), AMBAR);
		hud.txtPopupDetalle = Texto(l, "PopupDetalle", "", 28, TextAnchor.MiddleCenter, centro, new Vector2(0f, 72f), new Vector2(900f, 40f));
		hud.txtAviso = Texto(l, "Aviso", "", 38, TextAnchor.MiddleCenter, centro, new Vector2(0f, -150f), new Vector2(1400f, 60f));

		//Barra de vida del jefe
		var barra = UI("BarraJefe", l);
		Rect(barra, arriba, new Vector2(0f, -140f), new Vector2(920f, 84f));
		hud.barraJefe = barra;
		hud.txtJefe = Texto(barra.transform, "Nombre", "COLOSO", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(600f, 36f), new Color(1f, 0.55f, 0.45f));
		Imagen(barra.transform, "Fondo", sBlanco, new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(920f, 28f), new Color(0f, 0f, 0f, 0.7f));
		var relleno = Imagen(barra.transform, "Relleno", sBlanco, new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(908f, 20f), new Color(0.9f, 0.18f, 0.12f));
		relleno.type = Image.Type.Filled;
		relleno.fillMethod = Image.FillMethod.Horizontal;
		relleno.fillAmount = 1f;
		hud.rellenoJefe = relleno;

		//Presentación del nivel
		var pres = UI("PanelPresentacion", l);
		Estirar((RectTransform)pres.transform);
		Cubrir(pres.transform, "Oscuro", new Color(0.02f, 0.03f, 0.04f, 0.72f)).raycastTarget = false;
		Fondo(pres.transform, "Caja", centro, new Vector2(0f, 10f), new Vector2(1240f, 600f), new Color(0.04f, 0.05f, 0.07f, 0.95f));
		hud.txtPresentacionTitulo = Texto(pres.transform, "Titulo", "NIVEL 1", 40, TextAnchor.MiddleCenter, centro, new Vector2(0f, 205f), new Vector2(1000f, 50f), AMBAR);
		hud.txtPresentacionNombre = Texto(pres.transform, "Nombre", "", 84, TextAnchor.MiddleCenter, centro, new Vector2(0f, 130f), new Vector2(1100f, 100f));
		Imagen(pres.transform, "Linea", sBlanco, centro, new Vector2(0f, 72f), new Vector2(700f, 3f), new Color(AMBAR.r, AMBAR.g, AMBAR.b, 0.6f));
		hud.txtPresentacionDescripcion = Texto(pres.transform, "Descripcion", "", 28, TextAnchor.UpperCenter, centro, new Vector2(0f, -40f), new Vector2(1150f, 200f), Color.white, true, FontStyle.Normal);
		hud.txtPresentacionClic = Texto(pres.transform, "Clic", "HAZ CLIC PARA COMENZAR", 40, TextAnchor.MiddleCenter, centro, new Vector2(0f, -225f), new Vector2(1000f, 60f), AMBAR);
		Texto(pres.transform, "Controles", "Flechas: moverse   ·   Ratón: apuntar   ·   Clic izquierdo: disparar   ·   Espacio: saltar   ·   Shift: correr   ·   Esc: pausa",
			24, TextAnchor.MiddleCenter, centro, new Vector2(0f, -320f), new Vector2(1600f, 40f), GRIS_TEXTO);
		hud.panelPresentacion = pres;

		//Pausa
		var pausa = UI("PanelPausa", l);
		Estirar((RectTransform)pausa.transform);
		Cubrir(pausa.transform, "Oscuro", new Color(0.02f, 0.03f, 0.04f, 0.75f));
		Fondo(pausa.transform, "Caja", centro, Vector2.zero, new Vector2(620f, 560f));
		Texto(pausa.transform, "Titulo", "PAUSA", 80, TextAnchor.MiddleCenter, centro, new Vector2(0f, 190f), new Vector2(600f, 100f), AMBAR);
		Boton(pausa.transform, "CONTINUAR", centro, new Vector2(0f, 70f), new Vector2(440f, 86f), true, hud.BotonContinuar);
		Boton(pausa.transform, "REINICIAR PARTIDA", centro, new Vector2(0f, -40f), new Vector2(440f, 86f), false, hud.BotonReiniciar);
		Boton(pausa.transform, "MENÚ PRINCIPAL", centro, new Vector2(0f, -150f), new Vector2(440f, 86f), false, hud.BotonMenu);
		Texto(pausa.transform, "Ayuda", "P para continuar", 24, TextAnchor.MiddleCenter, centro, new Vector2(0f, -232f), new Vector2(500f, 30f), GRIS_TEXTO);
		hud.panelPausa = pausa;

		//Final del nivel
		var final = UI("PanelFinal", l);
		Estirar((RectTransform)final.transform);
		Cubrir(final.transform, "Oscuro", new Color(0.02f, 0.03f, 0.04f, 0.6f)).raycastTarget = false;
		hud.txtFinalTitulo = Texto(final.transform, "Titulo", "", 110, TextAnchor.MiddleCenter, centro, new Vector2(0f, 60f), new Vector2(1700f, 140f), AMBAR);
		hud.txtFinalDetalle = Texto(final.transform, "Detalle", "", 36, TextAnchor.MiddleCenter, centro, new Vector2(0f, -40f), new Vector2(1500f, 60f));
		hud.panelFinal = final;

		SistemaEventos();
		return hud;
	}

	// ------------------------------------------------ Menú de entrada

	static void CrearInterfazMenu(Menu menu) {
		var lienzo = Lienzo("Interfaz", 10);
		var l = lienzo.transform;
		var vineta = Cubrir(l, "Vineta", new Color(1f, 1f, 1f, 0.7f));
		vineta.sprite = sVineta;
		vineta.raycastTarget = false;
		var degradado = Imagen(l, "Degradado", sDegradadoH, new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(1300f, 1080f), new Color(0.02f, 0.03f, 0.04f, 0.85f));
		degradado.rectTransform.pivot = new Vector2(0f, 0.5f);
		NombreYMatricula(l);

		var izq = new Vector2(0f, 0.5f);
		//Panel principal
		var principal = UI("PanelPrincipal", l);
		Estirar((RectTransform)principal.transform);
		var p = principal.transform;
		Texto(p, "Escuadron", "ESCUADRÓN HALCÓN", 34, TextAnchor.MiddleLeft, izq, new Vector2(120f, 330f), new Vector2(800f, 50f), AMBAR);
		Texto(p, "Titulo", "OPERACIÓN\nHALCÓN", 130, TextAnchor.UpperLeft, izq, new Vector2(112f, 300f), new Vector2(900f, 300f)).rectTransform.pivot = new Vector2(0f, 1f);
		Imagen(p, "Linea", sBlanco, izq, new Vector2(120f, 30f), new Vector2(520f, 4f), AMBAR).rectTransform.pivot = new Vector2(0f, 0.5f);
		Texto(p, "Lema", "Contra la Legión Ferro", 32, TextAnchor.MiddleLeft, izq, new Vector2(120f, -10f), new Vector2(800f, 44f), GRIS_TEXTO);
		var tamBoton = new Vector2(440f, 84f);
		void B(string texto, float y, bool prin, UnityAction a) {
			var b = Boton(p, texto, izq, new Vector2(120f, y), tamBoton, prin, a);
			b.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
		}
		B("JUGAR", -110f, true, menu.Jugar);
		B("CÓMO JUGAR", -210f, false, menu.Controles);
		B("PERSONAJES", -310f, false, menu.Personajes);
		var salir = Boton(p, "SALIR", izq, new Vector2(120f, -410f), tamBoton, false, menu.Salir);
		salir.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
		menu.botonSalir = salir.gameObject;
		menu.txtRecord = Texto(p, "Record", "", 30, TextAnchor.MiddleLeft, izq, new Vector2(600f, -110f), new Vector2(360f, 44f), AMBAR);
		menu.txtRecord.rectTransform.pivot = new Vector2(0f, 0.5f);
		menu.panelPrincipal = principal;

		//Cómo jugar
		var centro = new Vector2(0.5f, 0.5f);
		var controles = UI("PanelControles", l);
		Estirar((RectTransform)controles.transform);
		var c = controles.transform;
		Cubrir(c, "Oscuro", new Color(0f, 0f, 0f, 0.45f));
		Fondo(c, "Caja", centro, Vector2.zero, new Vector2(1500f, 900f), new Color(0.04f, 0.05f, 0.07f, 0.96f));
		Texto(c, "Titulo", "CÓMO JUGAR", 64, TextAnchor.MiddleCenter, centro, new Vector2(0f, 370f), new Vector2(1000f, 80f), AMBAR);
		Texto(c, "TituloControles", "CONTROLES", 32, TextAnchor.UpperLeft, centro, new Vector2(-340f, 290f), new Vector2(620f, 40f), AMBAR);
		Texto(c, "Controles",
			"<b>Flechas</b> (o WASD)   moverse\n<b>Ratón</b>   apuntar\n<b>Clic izquierdo</b>   disparar\n<b>Espacio</b>   saltar\n<b>Shift</b>   correr\n<b>Esc</b> o <b>P</b>   pausa",
			30, TextAnchor.UpperLeft, centro, new Vector2(-340f, 130f), new Vector2(620f, 280f), Color.white, true, FontStyle.Normal);
		Texto(c, "TituloReglas", "REGLAS", 32, TextAnchor.UpperLeft, centro, new Vector2(-340f, -60f), new Vector2(620f, 40f), AMBAR);
		Texto(c, "Reglas",
			"Cada disparo gasta una bala. Recoge cajas de munición.\nSi un enemigo te toca, pierdes una vida.\n" +
			"La partida termina sin vidas o sin balas.\n3 niveles: galería de tiro, base enemiga y jefe final.",
			28, TextAnchor.UpperLeft, centro, new Vector2(-340f, -200f), new Vector2(620f, 220f), Color.white, true, FontStyle.Normal);
		Texto(c, "TituloPuntos", "PUNTUACIÓN", 32, TextAnchor.UpperLeft, centro, new Vector2(390f, 290f), new Vector2(560f, 40f), AMBAR);
		Texto(c, "Puntos",
			"Diana: borde <b>10</b> · anillo <b>25</b> · centro <b>50</b>\n" +
			"Diana móvil: <b>20</b> · <b>50</b> · <b>100</b>\n" +
			"Botella <b>15</b> · Globo <b>30</b> · Placa lejana <b>100</b>\n" +
			"<color=#ff6655>Civil (silueta azul) −50</color>\n" +
			"Dron <b>60</b> · Centinela <b>100</b> · Antena <b>150</b>\n" +
			"Coloso <b>2.000</b>\n\n" +
			"Cabeza del centinela y ojo del dron: <b>daño x2</b>\n" +
			"Núcleo del Coloso: <b>daño x3</b>\n" +
			"Si el Coloso choca con un pilar queda aturdido\ny recibe el doble de daño.",
			28, TextAnchor.UpperLeft, centro, new Vector2(390f, 50f), new Vector2(600f, 440f), Color.white, true, FontStyle.Normal);
		Boton(c, "VOLVER", centro, new Vector2(0f, -370f), new Vector2(360f, 80f), true, menu.Volver);
		menu.panelControles = controles;

		//Personajes
		var personajes = UI("PanelPersonajes", l);
		Estirar((RectTransform)personajes.transform);
		var pe = personajes.transform;
		Fondo(pe, "Caja", new Vector2(0f, 0.5f), new Vector2(60f, 0f), new Vector2(780f, 940f), new Color(0.04f, 0.05f, 0.07f, 0.94f)).rectTransform.pivot = new Vector2(0f, 0.5f);
		Texto(pe, "Titulo", "PERSONAJES", 60, TextAnchor.MiddleLeft, izq, new Vector2(110f, 400f), new Vector2(700f, 80f), AMBAR).rectTransform.pivot = new Vector2(0f, 0.5f);
		string[,] fichas = {
			{ "SARGENTO HALCÓN", "Protagonista. Soldado del Escuadrón Halcón con fusil de asalto y mira de punto rojo. 3 vidas (máximo 5)." },
			{ "DRON EXPLORADOR", "Vuela en grupo y se lanza en picada contra el jugador. 2 impactos (el ojo recibe daño doble)." },
			{ "CENTINELA", "Robot de infantería de la Legión Ferro. Camina hacia ti y te golpea. 4 impactos (la cabeza recibe daño doble)." },
			{ "COLOSO", "Jefe final de 6 metros. Embiste en línea recta y llama drones. Su núcleo de energía recibe daño triple." },
		};
		for (int i = 0; i < 4; i++) {
			float y = 290f - i * 190f;
			Texto(pe, "Nombre" + i, fichas[i, 0], 36, TextAnchor.UpperLeft, izq, new Vector2(110f, y), new Vector2(680f, 44f), Color.white).rectTransform.pivot = new Vector2(0f, 1f);
			Texto(pe, "Texto" + i, fichas[i, 1], 26, TextAnchor.UpperLeft, izq, new Vector2(110f, y - 48f), new Vector2(680f, 120f), GRIS_TEXTO, true, FontStyle.Normal).rectTransform.pivot = new Vector2(0f, 1f);
		}
		var volver = Boton(pe, "VOLVER", izq, new Vector2(110f, -410f), new Vector2(320f, 76f), true, menu.Volver);
		volver.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
		menu.panelPersonajes = personajes;

		SistemaEventos();
	}

	// ------------------------------------------------ Pantalla final

	static void CrearInterfazFin(Fin fin) {
		var lienzo = Lienzo("Interfaz", 10);
		var l = lienzo.transform;
		var vineta = Cubrir(l, "Vineta", new Color(1f, 1f, 1f, 0.8f));
		vineta.sprite = sVineta;
		vineta.raycastTarget = false;
		NombreYMatricula(l);
		var centro = new Vector2(0.5f, 0.5f);
		Fondo(l, "Caja", centro, Vector2.zero, new Vector2(1100f, 920f), new Color(0.04f, 0.05f, 0.07f, 0.9f));
		fin.txtTitulo = Texto(l, "Titulo", "¡MISIÓN CUMPLIDA!", 96, TextAnchor.MiddleCenter, centro, new Vector2(0f, 360f), new Vector2(1080f, 120f), AMBAR);
		fin.txtMotivo = Texto(l, "Motivo", "", 30, TextAnchor.MiddleCenter, centro, new Vector2(0f, 280f), new Vector2(1000f, 50f), Color.white, true, FontStyle.Normal);
		Texto(l, "EtiquetaPuntos", "PUNTUACIÓN", 28, TextAnchor.MiddleCenter, centro, new Vector2(0f, 215f), new Vector2(600f, 40f), GRIS_TEXTO);
		fin.txtPuntos = Texto(l, "Puntos", "0", 110, TextAnchor.MiddleCenter, centro, new Vector2(0f, 140f), new Vector2(800f, 120f));
		fin.txtRecord = Texto(l, "Record", "", 30, TextAnchor.MiddleCenter, centro, new Vector2(0f, 68f), new Vector2(600f, 40f), AMBAR);
		Imagen(l, "Linea", sBlanco, centro, new Vector2(0f, 38f), new Vector2(820f, 2f), new Color(1f, 1f, 1f, 0.2f));
		fin.txtEtiquetas = Texto(l, "Etiquetas", "", 30, TextAnchor.UpperRight, centro, new Vector2(-225f, -125f), new Vector2(430f, 300f), GRIS_TEXTO, true, FontStyle.Normal);
		fin.txtValores = Texto(l, "Valores", "", 30, TextAnchor.UpperLeft, centro, new Vector2(225f, -125f), new Vector2(430f, 300f));
		fin.txtEtiquetas.lineSpacing = fin.txtValores.lineSpacing = 1.12f;
		Boton(l, "VOLVER A JUGAR", centro, new Vector2(-230f, -370f), new Vector2(420f, 90f), true, fin.VolverAJugar);
		Boton(l, "MENÚ PRINCIPAL", centro, new Vector2(230f, -370f), new Vector2(420f, 90f), false, fin.IrAlMenu);
		SistemaEventos();
	}
}
