using UnityEngine;

//El Coloso, jefe final. Camina hacia el jugador y cada pocos segundos se prepara (brilla en rojo y ruge)
//para embestir en línea recta. Si choca contra un pilar queda aturdido y su núcleo recibe más daño.
//Al bajar al 66 % y al 33 % de vida llama a drones de apoyo.
[RequireComponent(typeof(Destello))]
public class Jefe : MonoBehaviour, IImpactable {

	enum Estado { Caminar, Preparar, Embestir, Aturdido, Muerto }

	public string nombre = "Coloso";
	public int vidaMax = 60;
	public float velocidad = 2.6f;
	public float velocidadEmbestida = 15f;
	public float radioContacto = 2.4f;
	public int puntos = 2000;

	[Header("Partes")]
	public Transform torso;
	public Transform piernaI, piernaD, brazoI, brazoD;
	public Renderer nucleo;
	public Material nucleoNormal, nucleoFurioso;
	public Light luzNucleo;

	[Header("Refuerzos")]
	public Enemigo dron;

	int vida;
	Estado estado = Estado.Caminar;
	float temporizador = 5f;
	float fase;
	Vector3 direccionEmbestida;
	int refuerzos;
	Destello destello;

	public int Vida => vida;
	public bool Muerto => estado == Estado.Muerto;
	public bool Aturdido => estado == Estado.Aturdido;
	public float FraccionVida => vida / (float)vidaMax;
	public Vector3 PuntoNucleo => nucleo.transform.position;

	void Awake() {
		vida = vidaMax;
		destello = GetComponent<Destello>();
	}

	void Update() {
		if (estado == Estado.Muerto) return;
		float dt = Time.deltaTime;
		var j = Jugador.I;
		if (j == null || Nivel.I == null || !Nivel.I.EnJuego) {
			Animar(0f, dt);
			return;
		}

		Vector3 haciaJugador = j.transform.position - transform.position;
		haciaJugador.y = 0f;
		temporizador -= dt;
		float paso = 0f;

		switch (estado) {
			case Estado.Caminar:
				Girar(haciaJugador, 2.5f, dt);
				paso = Mover(transform.forward, velocidad, dt) ? velocidad : 0f;
				if (temporizador <= 0f && haciaJugador.magnitude > 6f) {
					estado = Estado.Preparar;
					temporizador = 1.3f;
					Juego.I.SonarEn(Juego.I.sRugido, transform.position);
					Furia(true);
				}
				break;

			case Estado.Preparar:
				Girar(haciaJugador, 5f, dt);
				j.Sacudir(0.08f);
				if (temporizador <= 0f) {
					estado = Estado.Embestir;
					temporizador = 2.2f;
					direccionEmbestida = haciaJugador.normalized;
				}
				break;

			case Estado.Embestir:
				paso = velocidadEmbestida;
				if (!Mover(direccionEmbestida, velocidadEmbestida, dt)) {
					//Chocó contra un pilar o un muro
					estado = Estado.Aturdido;
					temporizador = 3f;
					Furia(false);
					j.Sacudir(0.5f);
					Juego.I.SonarEn(Juego.I.sExplosion, transform.position + transform.forward * 2f);
					Efectos.Chispas(transform.position + transform.forward * 2.5f + Vector3.up * 2f, -transform.forward);
					if (HUD.I != null) HUD.I.Aviso("¡EL COLOSO ESTÁ ATURDIDO! DISPARA AL NÚCLEO", new Color(0.4f, 0.9f, 1f));
				} else if (temporizador <= 0f) {
					estado = Estado.Caminar;
					temporizador = Random.Range(4f, 6f);
					Furia(false);
				}
				break;

			case Estado.Aturdido:
				if (temporizador <= 0f) {
					estado = Estado.Caminar;
					temporizador = Random.Range(4f, 6f);
				}
				break;
		}
		Animar(paso, dt);

		//Contacto con el jugador
		Vector3 cerca = j.transform.position - transform.position;
		cerca.y = 0f;
		if (cerca.magnitude < radioContacto && j.RecibirGolpe(transform.position)) {
			if (estado == Estado.Embestir) {
				estado = Estado.Caminar;
				temporizador = 4f;
				Furia(false);
			}
		}
	}

	void Girar(Vector3 hacia, float rapidez, float dt) {
		if (hacia.sqrMagnitude < 0.01f) return;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(hacia), rapidez * dt);
	}

	//Avanza si no hay obstáculos. Devuelve false si algo lo bloquea.
	bool Mover(Vector3 direccion, float rapidez, float dt) {
		Vector3 origen = transform.position + Vector3.up * 1.5f;
		if (Physics.SphereCast(origen, 1.4f, direccion, out _, 1.2f + rapidez * dt, Capas.Escenario, QueryTriggerInteraction.Ignore)) return false;
		transform.position += direccion * rapidez * dt;
		return true;
	}

	void Furia(bool activa) {
		nucleo.sharedMaterial = activa ? nucleoFurioso : nucleoNormal;
		if (luzNucleo != null) luzNucleo.color = activa ? new Color(1f, 0.2f, 0.1f) : new Color(0.3f, 0.85f, 1f);
	}

	void Animar(float rapidez, float dt) {
		fase += dt * (0.6f + rapidez * 0.35f);
		float a = rapidez > 0f ? Mathf.Sin(fase * 3f) * 22f : 0f;
		if (piernaI != null) piernaI.localRotation = Quaternion.Slerp(piernaI.localRotation, Quaternion.Euler(a, 0f, 0f), 10f * dt);
		if (piernaD != null) piernaD.localRotation = Quaternion.Slerp(piernaD.localRotation, Quaternion.Euler(-a, 0f, 0f), 10f * dt);
		float b = estado == Estado.Preparar ? -50f : estado == Estado.Embestir ? -80f : -a * 0.5f;
		if (brazoI != null) brazoI.localRotation = Quaternion.Slerp(brazoI.localRotation, Quaternion.Euler(b, 0f, 0f), 8f * dt);
		if (brazoD != null) brazoD.localRotation = Quaternion.Slerp(brazoD.localRotation, Quaternion.Euler(estado == Estado.Caminar ? a * 0.5f : b, 0f, 0f), 8f * dt);
		if (torso != null) {
			float inclinacion = estado == Estado.Embestir ? 18f : estado == Estado.Aturdido ? -12f + Mathf.Sin(fase * 9f) * 4f : 0f;
			torso.localRotation = Quaternion.Slerp(torso.localRotation, Quaternion.Euler(inclinacion, 0f, Mathf.Sin(fase * 1.5f) * 2f), 6f * dt);
		}
		if (luzNucleo != null) luzNucleo.intensity = 2.5f + Mathf.Sin(Time.time * (estado == Estado.Preparar ? 30f : 4f)) * 1f;
	}

	public void Impacto(Vector3 punto, Collider parte) {
		if (estado == Estado.Muerto) return;
		var juego = Juego.I;
		var critica = parte != null ? parte.GetComponent<ParteEnemigo>() : null;
		int dano = critica != null ? critica.multiplicador : 1;
		if (estado == Estado.Aturdido) dano *= 2;
		vida = Mathf.Max(0, vida - dano);
		juego.aciertos++;
		juego.SumarPuntos(critica != null ? 10 * dano : 5);
		destello.Activar(0.06f);
		Efectos.Chispas(punto, (punto - transform.position).normalized);
		juego.SonarEn(critica != null ? juego.sCritico : juego.sGolpeEnemigo, punto);
		if (HUD.I != null) {
			HUD.I.Marca(critica != null);
			if (critica != null) HUD.I.Aviso("¡NÚCLEO!  -" + dano, new Color(0.4f, 0.9f, 1f));
		}

		if ((refuerzos == 0 && FraccionVida <= 0.66f) || (refuerzos == 1 && FraccionVida <= 0.33f)) {
			refuerzos++;
			LlamarRefuerzos();
		}
		if (vida <= 0) Morir();
	}

	void LlamarRefuerzos() {
		if (dron == null) return;
		Juego.I.SonarEn(Juego.I.sRugido, transform.position);
		if (HUD.I != null) HUD.I.Aviso("¡EL COLOSO LLAMA REFUERZOS!", new Color(1f, 0.45f, 0.3f));
		for (int i = 0; i < 3; i++) {
			Vector3 p = transform.position + Quaternion.Euler(0f, 120f * i, 0f) * transform.forward * 4f + Vector3.up * 4.5f;
			var d = Instantiate(dron, p, Quaternion.identity);
			d.gameObject.SetActive(true);
			Efectos.Explosion(p, 0.6f);
		}
	}

	void Morir() {
		estado = Estado.Muerto;
		Furia(false);
		var juego = Juego.I;
		juego.SumarPuntos(puntos);
		juego.enemigos++;
		if (HUD.I != null) HUD.I.Puntos(puntos, "¡COLOSO DERROTADO!");
		//Los drones que queden se destruyen con él
		foreach (var e in FindObjectsByType<Enemigo>()) e.Morir(false);
		StartCoroutine(Derrumbe());
	}

	System.Collections.IEnumerator Derrumbe() {
		var juego = Juego.I;
		for (int i = 0; i < 6; i++) {
			Vector3 p = transform.position + Vector3.up * Random.Range(1.5f, 5.5f) + Random.insideUnitSphere * 1.8f;
			Efectos.Explosion(p, 1.4f);
			juego.SonarEn(juego.sExplosion, p);
			if (Jugador.I != null) Jugador.I.Sacudir(0.3f);
			yield return new WaitForSeconds(0.28f);
		}
		Efectos.Explosion(transform.position + Vector3.up * 3f, 3f);
		Efectos.Restos(transform.position + Vector3.up * 3f, new Color(0.3f, 0.3f, 0.33f), 30);
		juego.SonarEn(juego.sExplosion, transform.position, 1f);
		if (Nivel.I != null) Nivel.I.JefeDerrotado();
		Destroy(gameObject);
	}
}
