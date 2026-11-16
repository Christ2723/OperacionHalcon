using System.Collections;
using System.Linq;
using UnityEngine;

public enum Mision { Dianas, Enemigos, Jefe }

//Controla un nivel: presentación, pausa, objetivo de la misión, oleadas de enemigos,
//fin por falta de munición o de vidas y paso al siguiente nivel.
public class Nivel : MonoBehaviour {

	public static Nivel I;

	public int numero = 1;
	public string nombre = "Galería de disparos";
	[TextArea] public string descripcion = "";
	public Mision mision;
	[Tooltip("Balas que se reciben al empezar el nivel")]
	public int municionInicial = 20;
	[Tooltip("Escena siguiente; vacío = último nivel")]
	public string siguiente = "";
	public bool musicaJefe;

	[Header("Enemigos")]
	public GameObject[] oleadas;
	public Jefe jefe;

	public bool Iniciado { get; private set; }
	public bool Terminado { get; private set; }
	public bool Pausado { get; private set; }
	public bool EnJuego => Iniciado && !Terminado && !Pausado;
	public int Oleada => oleada;
	public int TotalObjetivos { get; private set; }
	public int ObjetivosRestantes { get; private set; }
	public int EnemigosRestantes { get; private set; }
	public int Bono { get; private set; }

	int oleada;
	bool estuvoBloqueado;
	float revisar;
	float sinBalas;

	void Awake() {
		I = this;
		Time.timeScale = 1f;
	}

	void Start() {
		var juego = Juego.I;
		juego.municion += municionInicial;
		juego.nivelAlcanzado = numero;
		juego.nombreNivel = nombre;
		juego.Musica(musicaJefe ? juego.musicaJefe : juego.musicaNivel, musicaJefe ? 0.5f : 0.38f);

		for (int i = 0; i < oleadas.Length; i++) oleadas[i].SetActive(i == 0);
		TotalObjetivos = FindObjectsByType<Objetivo>().Count(o => o.cuenta);
		Contar();
		if (HUD.I != null) HUD.I.Presentar(this);
		if (Entrada.Simulando) Comenzar();
	}

	public void Comenzar() {
		if (Iniciado) return;
		Iniciado = true;
		Bloquear(true);
		if (HUD.I != null) HUD.I.OcultarPresentacion();
		if (Jugador.I != null) Jugador.I.arma.Esperar(0.3f);
		if (oleadas.Length > 1 && HUD.I != null) HUD.I.Aviso("OLEADA 1 DE " + oleadas.Length, new Color(1f, 0.55f, 0.3f));
	}

	void Update() {
		if (!Iniciado) {
			if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) Comenzar();
			return;
		}
		if (Terminado) return;

		//Pausa con Esc o P, o si el navegador libera el ratón
		if (Pausado) {
			if (Input.GetKeyDown(KeyCode.P)) Continuar();
			return;
		}
		//En el navegador, Esc libera el ratón sin que el juego reciba la tecla
		if (Cursor.lockState == CursorLockMode.Locked) estuvoBloqueado = true;
		bool liberado = estuvoBloqueado && Cursor.lockState != CursorLockMode.Locked && !Entrada.Simulando;
		if (Entrada.Pausa || liberado) {
			Pausar();
			return;
		}

		revisar -= Time.deltaTime;
		if (revisar <= 0f) {
			revisar = 0.2f;
			Contar();
			RevisarMision();
			RevisarMunicion();
		}
	}

	void Contar() {
		ObjetivosRestantes = FindObjectsByType<Objetivo>().Count(o => o.cuenta && !o.Destruido);
		EnemigosRestantes = FindObjectsByType<Enemigo>().Count(e => !e.Muerto);
	}

	void RevisarMision() {
		switch (mision) {
			case Mision.Dianas:
				if (ObjetivosRestantes == 0) Completar();
				break;
			case Mision.Enemigos:
				if (EnemigosRestantes > 0) break;
				if (oleada < oleadas.Length - 1) {
					oleada++;
					oleadas[oleada].SetActive(true);
					Juego.I.Sonar(Juego.I.sAlerta);
					if (HUD.I != null) HUD.I.Aviso("OLEADA " + (oleada + 1) + " DE " + oleadas.Length, new Color(1f, 0.55f, 0.3f));
					Contar();
				} else {
					Completar();
				}
				break;
		}
	}

	//Sin balas, sin proyectiles en el aire y sin cajas de munición: la partida termina
	void RevisarMunicion() {
		bool sinRecursos = Juego.I.municion <= 0
			&& FindObjectsByType<Proyectil>().Length == 0
			&& !FindObjectsByType<Recogible>().Any(r => r.tipo == TipoRecogible.Municion);
		sinBalas = sinRecursos ? sinBalas + 0.2f : 0f;
		if (sinBalas >= 0.8f) Perder("Te quedaste sin munición antes de cumplir la misión.");
	}

	public void ObjetivoDestruido(Objetivo o) => revisar = 0f;
	public void EnemigoDestruido(Enemigo e) => revisar = 0f;

	public void JefeDerrotado() => Completar();

	void Completar() {
		if (Terminado) return;
		Terminado = true;
		var juego = Juego.I;
		int bono = juego.vidas * 100 + (mision == Mision.Dianas ? juego.municion * 10 : 0);
		Bono = bono;
		juego.SumarPuntos(bono);
		juego.Sonar(juego.sNivel);
		bool ultimo = string.IsNullOrEmpty(siguiente);
		if (HUD.I != null) HUD.I.Final(ultimo ? "¡MISIÓN CUMPLIDA!" : "NIVEL COMPLETADO",
			"Bonificación +" + bono + "  (vidas" + (mision == Mision.Dianas ? " y balas sobrantes)" : ")"), true);
		StartCoroutine(Salir(3.5f, () => {
			if (ultimo) juego.Terminar(true, "Derrotaste al Coloso y la Legión Ferro se retira.");
			else juego.Cargar(siguiente);
		}));
	}

	public void Perder(string motivo) {
		if (Terminado) return;
		Terminado = true;
		if (HUD.I != null) HUD.I.Final("MISIÓN FALLIDA", motivo, false);
		StartCoroutine(Salir(3f, () => Juego.I.Terminar(false, motivo)));
	}

	IEnumerator Salir(float segundos, System.Action accion) {
		yield return new WaitForSecondsRealtime(segundos);
		Bloquear(false);
		accion();
	}

	// ------------------------------------------------ Pausa

	public void Pausar() {
		if (Pausado || Terminado) return;
		Pausado = true;
		Time.timeScale = 0f;
		Bloquear(false);
		Juego.I.PausarMusica(true);
		if (HUD.I != null) HUD.I.MostrarPausa(true);
	}

	public void Continuar() {
		if (!Pausado) return;
		Pausado = false;
		estuvoBloqueado = false;
		Time.timeScale = 1f;
		Bloquear(true);
		Juego.I.PausarMusica(false);
		Juego.I.Sonar(Juego.I.sBoton);
		if (HUD.I != null) HUD.I.MostrarPausa(false);
		if (Jugador.I != null) Jugador.I.arma.Esperar(0.25f);
	}

	public void IrAlMenu() {
		Bloquear(false);
		Juego.I.Sonar(Juego.I.sBoton);
		Juego.I.Cargar(Juego.ESCENA_MENU);
	}

	public void Reiniciar() {
		Bloquear(false);
		Juego.I.Sonar(Juego.I.sBoton);
		Juego.I.NuevaPartida();
	}

	static void Bloquear(bool bloquear) {
		if (Entrada.Simulando) return;
		Cursor.lockState = bloquear ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = !bloquear;
	}
}
