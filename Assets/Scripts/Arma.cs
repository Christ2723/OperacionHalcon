using UnityEngine;

//Fusil del jugador: con clic izquierdo dispara un proyectil hacia el centro de la mira.
//Cada disparo gasta una bala; sin balas solo suena el gatillo.
public class Arma : MonoBehaviour {

	public Proyectil proyectil;
	public Transform boca;
	public GameObject destello;
	public Light luz;
	public Camera camara;

	public float cadencia = 0.17f;
	public float velocidadBala = 75f;

	float siguiente;
	float retroceso;
	float tiempoDestello;
	Vector3 posicionBase;
	Quaternion rotacionBase;

	void Awake() {
		posicionBase = transform.localPosition;
		rotacionBase = transform.localRotation;
		destello.SetActive(false);
		luz.enabled = false;
	}

	public void Esperar(float segundos) => siguiente = Time.time + segundos;

	void Update() {
		float dt = Time.deltaTime;
		if (Nivel.I != null && Nivel.I.EnJuego && Entrada.Disparo && Time.time >= siguiente) Disparar();

		//Retroceso y vaivén del arma
		retroceso = Mathf.Lerp(retroceso, 0f, 12f * dt);
		var j = Jugador.I;
		float rapidez = j != null ? new Vector2(j.Velocidad.x, j.Velocidad.z).magnitude : 0f;
		float t = Time.time * 7f;
		Vector3 vaiven = new Vector3(Mathf.Sin(t) * 0.006f, Mathf.Abs(Mathf.Cos(t)) * 0.006f, 0f) * Mathf.Clamp01(rapidez / 5f);
		transform.localPosition = posicionBase + vaiven + new Vector3(0f, retroceso * 0.02f, -retroceso * 0.09f);
		transform.localRotation = rotacionBase * Quaternion.Euler(-retroceso * 9f, 0f, 0f);

		if (tiempoDestello > 0f) {
			tiempoDestello -= dt;
			if (tiempoDestello <= 0f) {
				destello.SetActive(false);
				luz.enabled = false;
			}
		}
	}

	public void Disparar() {
		siguiente = Time.time + cadencia;
		var juego = Juego.I;
		if (juego.municion <= 0) {
			juego.Sonar(juego.sVacio);
			if (HUD.I != null) HUD.I.Aviso("¡SIN MUNICIÓN!", new Color(1f, 0.35f, 0.3f));
			return;
		}
		juego.municion--;
		juego.disparos++;

		//El proyectil sale de la boca del arma y va hacia lo que está en el centro de la mira
		Ray rayo = camara.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		Vector3 punto = Physics.Raycast(rayo, out var golpe, 400f, Capas.Disparables, QueryTriggerInteraction.Ignore)
			? golpe.point : rayo.GetPoint(400f);
		Vector3 origen = boca.position;
		//Si hay algo muy cerca, el proyectil sale desde la cámara para no atravesarlo
		if (Vector3.Distance(rayo.origin, punto) < 2.5f) origen = rayo.origin + rayo.direction * 0.2f;
		Vector3 direccion = (punto - origen).normalized;

		var p = Instantiate(proyectil, origen, Quaternion.LookRotation(direccion));
		p.Lanzar(direccion * velocidadBala);

		juego.Sonar(juego.sDisparo, 0.8f);
		retroceso = 1f;
		destello.SetActive(true);
		destello.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
		luz.enabled = true;
		tiempoDestello = 0.05f;
		if (Jugador.I != null) Jugador.I.Sacudir(0.05f);
	}
}
