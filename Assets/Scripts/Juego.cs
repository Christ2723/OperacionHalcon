using UnityEngine;
using UnityEngine.SceneManagement;

//Datos que se conservan entre escenas (puntos, vidas, munición, estadísticas) y todo el audio del juego.
//Vive en Resources/Juego.prefab y se crea solo la primera vez que alguien lo pide.
public class Juego : MonoBehaviour {

	public const int VIDAS_INICIALES = 3;
	public const int VIDAS_MAX = 5;

	public const string ESCENA_MENU = "00 Menu";
	public const string ESCENA_NIVEL1 = "01 Galeria de disparos";
	public const string ESCENA_NIVEL2 = "02 Base enemiga";
	public const string ESCENA_NIVEL3 = "03 Guarida del Coloso";
	public const string ESCENA_FIN = "04 Fin";

	static Juego instancia;

	public static Juego I {
		get {
			if (instancia == null) Instantiate(Resources.Load<Juego>("Juego")).name = "Juego";
			return instancia;
		}
	}

	[Header("Partida")]
	public int puntos;
	public int vidas = VIDAS_INICIALES;
	public int municion;
	public int disparos;
	public int aciertos;
	public int dianas;
	public int enemigos;
	public int nivelAlcanzado = 1;
	public string nombreNivel = "";
	public float tiempoInicio;

	[Header("Resultado")]
	public bool victoria;
	public string motivo = "";

	[Header("Fuentes de audio")]
	public AudioSource fuenteMusica;
	public AudioSource fuenteEfectos;

	[Header("Música")]
	public AudioClip musicaMenu;
	public AudioClip musicaNivel;
	public AudioClip musicaJefe;
	public AudioClip musicaVictoria;
	public AudioClip musicaDerrota;

	[Header("Efectos")]
	public AudioClip sDisparo;
	public AudioClip sVacio;
	public AudioClip sRebote;
	public AudioClip sDiana;
	public AudioClip sBotella;
	public AudioClip sGlobo;
	public AudioClip sMetal;
	public AudioClip sCivil;
	public AudioClip sGolpeEnemigo;
	public AudioClip sCritico;
	public AudioClip sExplosion;
	public AudioClip sDano;
	public AudioClip sMunicion;
	public AudioClip sVida;
	public AudioClip sNivel;
	public AudioClip sRugido;
	public AudioClip sPaso;
	public AudioClip sSalto;
	public AudioClip sBoton;
	public AudioClip sAlerta;

	void Awake() {
		if (instancia != null && instancia != this) {
			Destroy(gameObject);
			return;
		}
		instancia = this;
		DontDestroyOnLoad(gameObject);
		tiempoInicio = Time.time;
	}

	public static int MejorPuntaje {
		get => PlayerPrefs.GetInt("MejorPuntaje", 0);
		private set { PlayerPrefs.SetInt("MejorPuntaje", value); PlayerPrefs.Save(); }
	}

	public bool NuevoRecord { get; private set; }

	public float Precision => disparos == 0 ? 0f : Mathf.Clamp01(aciertos / (float)disparos);

	public void ReiniciarPartida() {
		puntos = 0;
		vidas = VIDAS_INICIALES;
		municion = 0;
		disparos = 0;
		aciertos = 0;
		dianas = 0;
		enemigos = 0;
		nivelAlcanzado = 1;
		nombreNivel = "";
		victoria = false;
		motivo = "";
		NuevoRecord = false;
		tiempoInicio = Time.time;
	}

	public void NuevaPartida() {
		ReiniciarPartida();
		Cargar(ESCENA_NIVEL1);
	}

	public void SumarPuntos(int cantidad) {
		puntos = Mathf.Max(0, puntos + cantidad);
	}

	//Fin de la partida: victoria tras el jefe, o derrota sin vidas o sin munición
	public void Terminar(bool gano, string porque) {
		victoria = gano;
		motivo = porque;
		NuevoRecord = puntos > MejorPuntaje;
		if (NuevoRecord) MejorPuntaje = puntos;
		Cargar(ESCENA_FIN);
	}

	public void Cargar(string escena) {
		Time.timeScale = 1f;
		SceneManager.LoadScene(escena);
	}

	// ------------------------------------------------ Audio

	public void Sonar(AudioClip clip, float volumen = 1f) {
		if (clip != null) fuenteEfectos.PlayOneShot(clip, volumen);
	}

	//Sonido con posición: se oye más fuerte cuanto más cerca está
	public void SonarEn(AudioClip clip, Vector3 posicion, float volumen = 1f) {
		if (clip == null) return;
		var oyente = Camera.main != null ? Camera.main.transform.position : posicion;
		float distancia = Vector3.Distance(oyente, posicion);
		float atenuacion = Mathf.Clamp01(1f - distancia / 60f);
		fuenteEfectos.PlayOneShot(clip, volumen * (0.25f + 0.75f * atenuacion));
	}

	public void Musica(AudioClip clip, float volumen = 0.45f) {
		if (fuenteMusica.clip == clip && fuenteMusica.isPlaying) {
			fuenteMusica.volume = volumen;
			return;
		}
		fuenteMusica.clip = clip;
		fuenteMusica.volume = volumen;
		fuenteMusica.loop = true;
		if (clip != null) fuenteMusica.Play();
	}

	public void PausarMusica(bool pausar) {
		if (pausar) fuenteMusica.Pause(); else fuenteMusica.UnPause();
	}
}
