using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Prueba automática del juego en modo Play. Recorre los tres niveles disparando de verdad y comprueba
//puntos por zona, munición, civiles, vidas, recogibles, oleadas, daño crítico, jefe, pausa, victoria y
//las dos derrotas (sin munición y sin vidas). También toma las capturas para la documentación.
//Uso: Unity -batchmode -projectPath . -executeMethod PruebaAutomatica.Ejecutar   (sin -quit)
public static class PruebaAutomatica {

	const string CAPTURAS = "Documentacion/capturas";

	static IEnumerator<float> pasos;
	static double siguiente;
	static readonly List<string> resultados = new List<string>();
	static readonly List<string> errores = new List<string>();
	static int fallos;

	[MenuItem("Halcon/Prueba automática")]
	public static void Ejecutar() {
		EditorSettings.enterPlayModeOptionsEnabled = true;
		EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
		Directory.CreateDirectory(CAPTURAS);
		EditorSceneManager.OpenScene("Assets/Scenes/" + Juego.ESCENA_MENU + ".unity");
		Application.logMessageReceived += (msg, pila, tipo) => {
			if ((tipo == LogType.Exception || tipo == LogType.Error) && !pila.Contains("UnityEditor.Search")) errores.Add(msg + "\n" + pila);
		};
		resultados.Clear();
		errores.Clear();
		fallos = 0;
		pasos = Pasos();
		siguiente = EditorApplication.timeSinceStartup + 0.5;
		EditorApplication.update += Avanzar;
		EditorApplication.EnterPlaymode();
	}

	static void Avanzar() {
		if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < siguiente) return;
		bool sigue;
		try {
			sigue = pasos.MoveNext();
		} catch (System.Exception e) {
			Revisar(false, "Excepción en la prueba: " + e);
			sigue = false;
		}
		if (sigue) {
			siguiente = EditorApplication.timeSinceStartup + pasos.Current;
			return;
		}
		EditorApplication.update -= Avanzar;
		Entrada.DejarDeSimular();
		foreach (var e in errores.Distinct()) Revisar(false, "Error en consola: " + e);
		resultados.Add(fallos == 0 ? "RESULTADO: TODO OK" : "RESULTADO: " + fallos + " FALLOS");
		Directory.CreateDirectory("Logs");
		File.WriteAllLines("Logs/prueba.txt", resultados);
		Debug.Log("PRUEBA\n" + string.Join("\n", resultados));
		EditorSettings.enterPlayModeOptionsEnabled = false;
		EditorApplication.ExitPlaymode();
		if (Application.isBatchMode) EditorApplication.Exit(fallos == 0 ? 0 : 1);
	}

	static void Revisar(bool ok, string que) {
		resultados.Add((ok ? "[OK]    " : "[FALLA] ") + que);
		if (!ok) fallos++;
	}

	static string Escena => SceneManager.GetActiveScene().name;

	// ------------------------------------------------ Ayudas

	static Arma Arma => Jugador.I.arma;

	//Apunta al punto y dispara un proyectil real
	static void Disparar(Vector3 punto) {
		Jugador.I.Mirar(punto);
		Arma.Disparar();
	}

	static Vector3 CentroDe(GameObject go) {
		var cols = go.GetComponentsInChildren<Collider>();
		var b = cols[0].bounds;
		foreach (var c in cols) b.Encapsulate(c.bounds);
		return b.center;
	}

	static Vector3 PuntoDeZona(Objetivo o, string zona) {
		var t = o.GetComponentsInChildren<Collider>().FirstOrDefault(c => c.name == zona);
		return t != null ? t.bounds.center : CentroDe(o.gameObject);
	}

	static void Congelar() {
		foreach (var e in Object.FindObjectsByType<Enemigo>()) e.velocidad = 0f;
		var jefe = Object.FindAnyObjectByType<Jefe>();
		if (jefe != null) {
			jefe.velocidad = 0f;
			jefe.velocidadEmbestida = 0f;
		}
	}

	static void Foto(string nombre) {
		const int ANCHO = 1280, ALTO = 720;
		var principal = Camera.main;
		if (principal == null) return;
		var camaras = principal.GetComponentsInChildren<Camera>().OrderBy(c => c.depth).ToList();
		var ultima = camaras.Last();
		var rt = new RenderTexture(ANCHO, ALTO, 24);
		var lienzos = Object.FindObjectsByType<Canvas>().Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay).ToList();
		int mascara = ultima.cullingMask;
		foreach (var l in lienzos) ultima.cullingMask |= 1 << l.gameObject.layer;
		foreach (var c in camaras) c.targetTexture = rt;
		foreach (var l in lienzos) {
			l.renderMode = RenderMode.ScreenSpaceCamera;
			l.worldCamera = ultima;
			l.planeDistance = Mathf.Min(1f, ultima.farClipPlane * 0.5f);
		}
		Canvas.ForceUpdateCanvases();
		foreach (var c in camaras) c.Render();
		RenderTexture.active = rt;
		var tex = new Texture2D(ANCHO, ALTO, TextureFormat.RGB24, false);
		tex.ReadPixels(new Rect(0, 0, ANCHO, ALTO), 0, 0);
		tex.Apply();
		File.WriteAllBytes(CAPTURAS + "/" + nombre + ".png", tex.EncodeToPNG());
		Object.DestroyImmediate(tex);
		RenderTexture.active = null;
		ultima.cullingMask = mascara;
		foreach (var c in camaras) c.targetTexture = null;
		foreach (var l in lienzos) l.renderMode = RenderMode.ScreenSpaceOverlay;
		rt.Release();
	}

	// ------------------------------------------------ Recorrido

	static IEnumerator<float> Pasos() {
		yield return 1.5f;

		//Menú
		var menu = Object.FindAnyObjectByType<Menu>();
		Revisar(menu != null && Escena == Juego.ESCENA_MENU, "El juego abre en el menú de entrada");
		Foto("menu");
		menu.Controles();
		yield return 0.2f;
		Foto("menu_como_jugar");
		menu.Personajes();
		yield return 0.2f;
		Foto("menu_personajes");
		menu.Volver();

		Entrada.Simulando = true;
		menu.Jugar();
		yield return 1.5f;

		// ---------------- Nivel 1
		Revisar(Escena == Juego.ESCENA_NIVEL1, "Jugar carga la escena «01 Galeria de disparos»");
		var juego = Juego.I;
		var nivel = Nivel.I;
		Revisar(nivel != null && nivel.numero == 1 && nivel.EnJuego, "El nivel 1 empieza");
		Revisar(juego.municion == 20 && juego.vidas == 3 && juego.puntos == 0, "Empieza con 20 balas, 3 vidas y 0 puntos (" + juego.municion + ", " + juego.vidas + ", " + juego.puntos + ")");
		Revisar(nivel.TotalObjetivos == 12, "El nivel tiene 12 objetivos que cuentan (" + nivel.TotalObjetivos + ")");
		foreach (var m in Object.FindObjectsByType<Movimiento>()) if (m.modo == Movimiento.Modo.Deslizar) m.enabled = false;

		//Captura de presentación
		var hud = HUD.I;
		hud.panelPresentacion.SetActive(true);
		Foto("nivel1_presentacion");
		hud.panelPresentacion.SetActive(false);

		var dianas = Object.FindObjectsByType<Objetivo>().Where(o => o.tipo == TipoObjetivo.Diana && o.nombre == "Diana")
			.OrderBy(o => Vector3.Distance(o.transform.position, Jugador.I.transform.position)).ToList();
		Jugador.I.Mirar(dianas[0].transform.position + Vector3.up * 1.55f);
		yield return 0.1f;
		Foto("nivel1_galeria");

		//Centro de una diana: 50
		var d0 = dianas[0];
		Disparar(PuntoDeZona(d0, "Centro"));
		Revisar(juego.municion == 19, "Cada disparo gasta una bala (" + juego.municion + ")");
		yield return 0.12f;
		Foto("nivel1_disparo");
		yield return 0.6f;
		Revisar(d0 == null, "La diana alcanzada se destruye junto con el proyectil");
		Revisar(juego.puntos == 50, "El centro de la diana da 50 puntos (" + juego.puntos + ")");
		Revisar(Object.FindObjectsByType<Proyectil>().Length == 0, "El proyectil desaparece al impactar");

		//Anillo medio: 25
		var d1 = dianas[1];
		var medio = d1.GetComponentsInChildren<Collider>().First(c => c.name == "Medio");
		Disparar(medio.bounds.center + medio.transform.up * 0f + d1.transform.right * 0.24f);
		yield return 0.8f;
		Revisar(juego.puntos == 75, "El anillo medio da 25 puntos (" + juego.puntos + ")");

		//Borde: 10
		var d2 = dianas[2];
		var borde = d2.GetComponentsInChildren<Collider>().First(c => c.name == "Borde");
		Disparar(borde.bounds.center + Vector3.up * 0.52f);
		yield return 0.8f;
		Revisar(juego.puntos == 85, "El borde da 10 puntos (" + juego.puntos + ")");

		//Civil: −50 y no cuenta para la misión
		var civil = Object.FindObjectsByType<Objetivo>().First(o => o.tipo == TipoObjetivo.Civil);
		Disparar(CentroDe(civil.gameObject) + Vector3.up * 0.2f);
		yield return 0.8f;
		Revisar(civil == null && juego.puntos == 35, "Disparar a un civil resta 50 puntos (85 → " + juego.puntos + ")");
		Revisar(juego.dianas == 3, "Los civiles no cuentan como dianas (" + juego.dianas + ")");

		//Botella, globo y placa
		var botella = Object.FindObjectsByType<Objetivo>().First(o => o.tipo == TipoObjetivo.Botella);
		int antes = juego.puntos;
		Disparar(CentroDe(botella.gameObject));
		yield return 0.8f;
		Revisar(juego.puntos - antes == 15, "La botella da 15 puntos (" + (juego.puntos - antes) + ")");
		var globo = Object.FindObjectsByType<Objetivo>().First(o => o.tipo == TipoObjetivo.Globo);
		globo.GetComponentInChildren<Movimiento>().enabled = false;
		antes = juego.puntos;
		Disparar(globo.GetComponentInChildren<SphereCollider>().bounds.center);
		yield return 0.8f;
		Revisar(juego.puntos - antes == 30, "El globo da 30 puntos (" + (juego.puntos - antes) + ")");

		//El resto de objetivos
		for (int intento = 0; intento < 30; intento++) {
			var resto = Object.FindObjectsByType<Objetivo>().Where(o => o.cuenta && !o.Destruido).ToList();
			if (resto.Count == 0 || nivel.Terminado) break;
			foreach (var m in Object.FindObjectsByType<Movimiento>()) if (m.modo != Movimiento.Modo.Girar) m.enabled = false;
			var o = resto.OrderBy(x => x.tipo == TipoObjetivo.Placa ? 1 : 0).First();
			antes = juego.puntos;
			bool esPlaca = o.tipo == TipoObjetivo.Placa;
			var zona = o.GetComponentsInChildren<Collider>().FirstOrDefault(c => c.name == "Centro");
			Disparar(esPlaca ? PuntoDeZona(o, "Disco") : zona != null ? zona.bounds.center : CentroDe(o.gameObject));
			yield return 0.9f;
			if (esPlaca) { int ganados = juego.puntos - antes - (nivel.Terminado ? nivel.Bono : 0); Revisar(o == null && ganados == 100, "La placa lejana da 100 puntos (" + ganados + ")"); }
		}
		Revisar(juego.dianas == 12, "Se destruyeron las 12 dianas (" + juego.dianas + ")");
		int municionNivel1 = juego.municion;
		yield return 0.5f;
		Revisar(nivel.Terminado, "Al destruir todo, el nivel se completa");
		Foto("nivel1_completado");
		int esperado = juego.vidas * 100 + municionNivel1 * 10;
		Revisar(nivel.Bono == esperado, "Bonificación de 100 por vida y 10 por bala sobrante (" + nivel.Bono + " = " + esperado + ")");
		for (int i = 0; i < 40 && Escena != Juego.ESCENA_NIVEL2; i++) yield return 0.25f;
		yield return 0.6f;

		// ---------------- Nivel 2
		Revisar(Escena == Juego.ESCENA_NIVEL2, "Se pasa al nivel 2 «Base enemiga»");
		nivel = Nivel.I;
		Revisar(juego.municion == municionNivel1 + 35, "La munición sobrante pasa al nivel 2 y se suman 35 balas (" + juego.municion + ")");
		Revisar(nivel.oleadas.Length == 3 && nivel.oleadas[0].activeSelf && !nivel.oleadas[1].activeSelf, "El nivel 2 tiene 3 oleadas y empieza la primera");

		//Captura con enemigos a la vista
		var centinelas = Object.FindObjectsByType<Enemigo>().Where(e => e.tipo == TipoEnemigo.Centinela).ToList();
		Jugador.I.Teletransportar(new Vector3(0f, 0.1f, 6f));
		yield return 0.3f;
		Jugador.I.Mirar(new Vector3(0f, 1.8f, 24f));
		yield return 0.3f;
		Foto("nivel2_base");

		//Golpe a la cabeza: daño doble
		Congelar();
		var c0 = centinelas[0];
		int vidaAntes = c0.Vida;
		Disparar(c0.GetComponentsInChildren<Collider>().First(c => c.GetComponent<ParteEnemigo>() != null).bounds.center);
		yield return 0.6f;
		Revisar(c0 != null && vidaAntes - c0.Vida == 2, "Un disparo a la cabeza del centinela quita 2 de vida (" + vidaAntes + " → " + (c0 != null ? c0.Vida : 0) + ")");
		Disparar(c0.GetComponentsInChildren<Collider>().First(c => c.name == "Pecho").bounds.center);
		yield return 0.6f;
		Revisar(c0 != null && c0.Vida == 1, "Un disparo al cuerpo quita 1 de vida");

		//Choque con un enemigo: pierde una vida
		int vidas = juego.vidas;
		var c1 = centinelas[1];
		c1.transform.position = Jugador.I.transform.position + Jugador.I.transform.forward * 0.6f;
		yield return 0.3f;
		Revisar(juego.vidas == vidas - 1, "Al chocar con un enemigo se pierde una vida (" + vidas + " → " + juego.vidas + ")");
		Foto("nivel2_golpe");
		yield return 1.7f;

		//Recoger munición y botiquín
		int balas = juego.municion;
		var caja = Object.FindObjectsByType<Recogible>().First(r => r.tipo == TipoRecogible.Municion);
		Jugador.I.Teletransportar(caja.transform.position + Vector3.up * 0.1f);
		yield return 0.3f;
		Revisar(juego.municion == balas + 20, "La caja de munición da 20 balas (" + balas + " → " + juego.municion + ")");
		vidas = juego.vidas;
		var botiquin = Object.FindObjectsByType<Recogible>().First(r => r.tipo == TipoRecogible.Botiquin);
		Jugador.I.Teletransportar(botiquin.transform.position + Vector3.up * 0.1f);
		yield return 0.3f;
		Revisar(juego.vidas == vidas + 1, "El botiquín da una vida (" + vidas + " → " + juego.vidas + ")");

		//Pausa
		nivel.Pausar();
		yield return 0.1f;
		Revisar(Time.timeScale == 0f && hudActivo("PanelPausa"), "Esc/P pausa el juego y muestra el menú de pausa");
		Foto("nivel2_pausa");
		nivel.Continuar();
		yield return 0.1f;
		Revisar(Time.timeScale == 1f && !hudActivo("PanelPausa"), "Continuar reanuda el juego");

		//Eliminar las tres oleadas
		Jugador.I.Teletransportar(new Vector3(0f, 0.1f, -8f));
		int oleadaMax = 0;
		bool antenaProbada = false;
		Enemigo ultimo = null;
		int tirosAlMismo = 0, bloqueados = 0;
		for (int i = 0; i < 400 && Escena == Juego.ESCENA_NIVEL2 && !nivel.Terminado; i++) {
			Congelar();
			if (juego.municion < 10) juego.municion += 30;
			juego.vidas = Mathf.Max(juego.vidas, 2);
			oleadaMax = Mathf.Max(oleadaMax, nivel.Oleada);
			if (!antenaProbada) {
				var antena = Object.FindObjectsByType<Objetivo>().Where(o => o.tipo == TipoObjetivo.Antena)
					.OrderBy(o => Vector3.Distance(o.transform.position, Jugador.I.transform.position)).FirstOrDefault();
				if (antena != null) {
					int p0 = juego.puntos;
					for (int k = 0; k < 3; k++) {
						Disparar(PuntoDeZona(antena, "Mastil"));
						yield return 0.5f;
					}
					Revisar(antena == null && juego.puntos - p0 == 150, "La antena resiste 3 impactos y da 150 puntos (" + (juego.puntos - p0) + ")");
					antenaProbada = true;
				}
			}
			var vivos = Object.FindObjectsByType<Enemigo>().Where(e => !e.Muerto)
				.OrderBy(e => Vector3.Distance(e.transform.position, Jugador.I.transform.position)).ToList();
			if (vivos.Count == 0) {
				yield return 0.3f;
				continue;
			}
			var objetivo = vivos[0];
			if (Vector3.Distance(objetivo.transform.position, Jugador.I.transform.position) > 18f) {
				Vector3 cerca = objetivo.transform.position - (objetivo.transform.position - Jugador.I.transform.position).normalized * 9f;
				cerca.y = 0.1f;
				Jugador.I.Teletransportar(cerca);
				yield return 0.1f;
			}
			tirosAlMismo = objetivo == ultimo ? tirosAlMismo + 1 : 0;
			ultimo = objetivo;
			if (tirosAlMismo >= 6) {
				//Algo del escenario lo tapa: se acerca a quemarropa
				bloqueados++;
				Jugador.I.Teletransportar(new Vector3(objetivo.transform.position.x, 0.1f, objetivo.transform.position.z) - Jugador.I.transform.forward * 3f);
				tirosAlMismo = 0;
				yield return 0.1f;
			}
			Disparar(objetivo.Centro);
			yield return 0.35f;
		}
		resultados.Add("        (enemigos tapados por el escenario, el bot se acercó: " + bloqueados + ")");
		Revisar(oleadaMax == 2, "Al limpiar cada oleada aparece la siguiente (llegó a la oleada " + (oleadaMax + 1) + ")");
		Revisar(juego.enemigos >= 21, "Se destruyeron todos los robots del nivel 2 (" + juego.enemigos + ")");
		for (int i = 0; i < 40 && Escena != Juego.ESCENA_NIVEL3; i++) yield return 0.25f;
		yield return 0.6f;

		// ---------------- Nivel 3
		Revisar(Escena == Juego.ESCENA_NIVEL3, "Se pasa al nivel 3 «Guarida del Coloso»");
		nivel = Nivel.I;
		var jefe = Object.FindAnyObjectByType<Jefe>();
		Revisar(jefe != null && jefe.Vida == 60, "El Coloso aparece con 60 de vida");
		Jugador.I.Mirar(jefe.PuntoNucleo);
		yield return 0.4f;
		Foto("nivel3_coloso");
		Congelar();
		Disparar(CentroDe(jefe.GetComponentsInChildren<Collider>().First(c => c.name == "Muslo").gameObject));
		yield return 0.6f;
		Revisar(jefe.Vida == 59, "Un disparo al cuerpo del Coloso quita 1 de vida (" + jefe.Vida + ")");
		Disparar(jefe.PuntoNucleo);
		yield return 0.6f;
		Revisar(jefe.Vida == 56, "Un disparo al núcleo quita 3 de vida (" + jefe.Vida + ")");
		bool refuerzos = false;
		for (int i = 0; i < 80 && jefe != null && !jefe.Muerto; i++) {
			Congelar();
			if (juego.municion < 10) juego.municion += 30;
			Disparar(jefe.PuntoNucleo);
			yield return 0.3f;
			if (!refuerzos && Object.FindObjectsByType<Enemigo>().Length >= 3) {
				refuerzos = true;
				Foto("nivel3_refuerzos");
			}
		}
		Revisar(refuerzos, "El Coloso llama drones de refuerzo al perder vida");
		Revisar(jefe == null || jefe.Muerto, "El Coloso cae al llegar a 0 de vida");
		yield return 1.2f;
		Foto("nivel3_derrota_coloso");
		for (int i = 0; i < 60 && Escena != Juego.ESCENA_FIN; i++) yield return 0.25f;
		yield return 1f;

		// ---------------- Victoria
		Revisar(Escena == Juego.ESCENA_FIN && juego.victoria, "Al derrotar al jefe aparece la pantalla de victoria");
		var fin = Object.FindAnyObjectByType<Fin>();
		Revisar(fin != null && fin.txtTitulo.text == "¡MISIÓN CUMPLIDA!" && fin.txtPuntos.text == juego.puntos.ToString("N0"),
			"La pantalla final muestra el título y la puntuación (" + juego.puntos + ")");
		Revisar(fin.txtValores.text.StartsWith(juego.vidas.ToString()), "La pantalla final muestra las vidas restantes (" + juego.vidas + ")");
		Foto("fin_victoria");

		// ---------------- Derrota sin munición
		fin.VolverAJugar();
		yield return 1.5f;
		Revisar(Escena == Juego.ESCENA_NIVEL1 && juego.puntos == 0 && juego.vidas == 3, "Volver a jugar reinicia la partida en el nivel 1");
		juego.municion = 1;
		Disparar(Jugador.I.transform.position + new Vector3(0f, 8f, 30f));
		yield return 0.3f;
		Revisar(juego.municion == 0, "Se gasta la última bala");
		Arma.Disparar();
		Revisar(juego.municion == 0, "Sin balas el arma no dispara");
		for (int i = 0; i < 40 && Escena != Juego.ESCENA_FIN; i++) yield return 0.25f;
		yield return 0.5f;
		Revisar(Escena == Juego.ESCENA_FIN && !juego.victoria && juego.motivo.Contains("munición"), "Sin munición la partida termina («" + juego.motivo + "»)");
		Foto("fin_derrota");

		// ---------------- Derrota sin vidas
		juego.ReiniciarPartida();
		juego.Cargar(Juego.ESCENA_NIVEL2);
		yield return 1.5f;
		juego.vidas = 1;
		var c = Object.FindObjectsByType<Enemigo>().First(e => e.tipo == TipoEnemigo.Centinela);
		c.transform.position = Jugador.I.transform.position + Jugador.I.transform.forward * 0.6f;
		yield return 0.4f;
		Revisar(juego.vidas == 0 && Nivel.I.Terminado, "Con 0 vidas aparece «MISIÓN FALLIDA»");
		Foto("nivel2_mision_fallida");
		for (int i = 0; i < 40 && Escena != Juego.ESCENA_FIN; i++) yield return 0.25f;
		yield return 0.5f;
		Revisar(Escena == Juego.ESCENA_FIN && !juego.victoria && juego.motivo.Contains("vidas"), "Sin vidas la partida termina («" + juego.motivo + "»)");

		Object.FindAnyObjectByType<Fin>().IrAlMenu();
		yield return 1f;
		Revisar(Escena == Juego.ESCENA_MENU, "Desde la pantalla final se vuelve al menú");
	}

	static bool hudActivo(string panel) {
		var h = HUD.I;
		if (h == null) return false;
		return panel == "PanelPausa" ? h.panelPausa.activeSelf : h.panelFinal.activeSelf;
	}
}
