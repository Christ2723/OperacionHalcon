using UnityEngine;
using UnityEngine.UI;

//Menú de entrada: Jugar, Cómo jugar, Personajes y Salir. La cámara gira despacio alrededor de los personajes.
public class Menu : MonoBehaviour {

	public GameObject panelPrincipal;
	public GameObject panelControles;
	public GameObject panelPersonajes;
	public GameObject botonSalir;
	public Text txtRecord;
	public Transform camara;
	public Transform centro;

	float angulo;
	float radio;
	float altura;

	void Start() {
		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Entrada.DejarDeSimular();
		var juego = Juego.I;
		juego.Musica(juego.musicaMenu, 0.5f);
		int record = Juego.MejorPuntaje;
		txtRecord.text = record > 0 ? "RÉCORD  " + record.ToString("N0") : "";
		if (Application.platform == RuntimePlatform.WebGLPlayer) botonSalir.SetActive(false);
		Mostrar(panelPrincipal);

		Vector3 d = camara.position - centro.position;
		altura = d.y;
		d.y = 0f;
		radio = d.magnitude;
		angulo = Mathf.Atan2(d.x, d.z);
	}

	void Update() {
		float a = angulo + Mathf.Sin(Time.time * 0.25f) * 0.22f;
		camara.position = centro.position + new Vector3(Mathf.Sin(a) * radio, altura, Mathf.Cos(a) * radio);
		camara.LookAt(centro.position + Vector3.up * 1.6f);
		if (Input.GetKeyDown(KeyCode.Escape) && !panelPrincipal.activeSelf) Volver();
		if (Input.GetKeyDown(KeyCode.Return) && panelPrincipal.activeSelf) Jugar();
	}

	void Mostrar(GameObject panel) {
		panelPrincipal.SetActive(panel == panelPrincipal);
		panelControles.SetActive(panel == panelControles);
		panelPersonajes.SetActive(panel == panelPersonajes);
	}

	public void Jugar() {
		Juego.I.Sonar(Juego.I.sBoton);
		Juego.I.NuevaPartida();
	}

	public void Controles() {
		Juego.I.Sonar(Juego.I.sBoton);
		Mostrar(panelControles);
	}

	public void Personajes() {
		Juego.I.Sonar(Juego.I.sBoton);
		Mostrar(panelPersonajes);
	}

	public void Volver() {
		Juego.I.Sonar(Juego.I.sBoton);
		Mostrar(panelPrincipal);
	}

	public void Salir() {
		Juego.I.Sonar(Juego.I.sBoton);
		Application.Quit();
	}
}
