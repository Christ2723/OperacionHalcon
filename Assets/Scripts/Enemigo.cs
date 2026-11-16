using UnityEngine;

public enum TipoEnemigo { Centinela, Dron }

//Robot de la Legión Ferro. Persigue al jugador esquivando obstáculos; si lo toca, le quita una vida.
//El centinela camina y retrocede tras golpear; el dron vuela y se lanza contra el jugador (explota al chocar).
[RequireComponent(typeof(Destello))]
public class Enemigo : MonoBehaviour, IImpactable {

	public TipoEnemigo tipo;
	public string nombre = "Centinela";
	public int vida = 4;
	public int puntos = 100;
	public float velocidad = 3f;
	public float altura = 2.5f;
	public float radioContacto = 1.1f;

	[Header("Animación")]
	public Transform cuerpo;
	public Transform piernaI, piernaD, brazoI, brazoD;
	public Transform[] rotores;

	int vidaMax;
	float aturdido;
	float fase;
	float alturaSuelo;
	bool muerto;
	Vector3 direccion;
	Vector3 retroceso;
	Destello destello;
	float aparicion = 1f;
	Vector3 escala;

	public bool Muerto => muerto;
	public int Vida => vida;
	public Vector3 Centro => cuerpo != null ? cuerpo.position : transform.position + Vector3.up;

	void Awake() {
		vidaMax = vida;
		destello = GetComponent<Destello>();
		fase = Random.value * 10f;
		escala = transform.localScale;
	}

	void OnEnable() {
		alturaSuelo = transform.position.y;
		if (tipo == TipoEnemigo.Dron) alturaSuelo = 0f;
		aparicion = 0f;
		if (Application.isPlaying && Time.timeSinceLevelLoad > 1f) Efectos.Chispas(transform.position + Vector3.up, Vector3.up);
	}

	void Update() {
		if (muerto) return;
		float dt = Time.deltaTime;
		fase += dt;
		aparicion = Mathf.Min(1f, aparicion + dt * 2.5f);
		transform.localScale = escala * Mathf.SmoothStep(0.2f, 1f, aparicion);
		Animar(dt);

		var j = Jugador.I;
		if (j == null || Nivel.I == null || !Nivel.I.EnJuego) return;

		aturdido -= dt;
		retroceso = Vector3.Lerp(retroceso, Vector3.zero, 4f * dt);
		Vector3 pos = transform.position;

		if (aturdido <= 0f) {
			Vector3 meta = j.transform.position;
			Vector3 plano = meta - pos;
			plano.y = 0f;
			float distancia = plano.magnitude;

			Vector3 deseada;
			if (tipo == TipoEnemigo.Dron) {
				//Vuela alto y, cerca del jugador, se lanza en picada contra él
				float y = distancia < 7f ? j.Centro.y : altura + Mathf.Sin(fase * 2f) * 0.3f;
				deseada = new Vector3(meta.x, y, meta.z) - pos;
			} else {
				deseada = plano;
			}
			if (deseada.sqrMagnitude > 0.0001f) {
				Vector3 d = Esquivar(deseada.normalized);
				direccion = Vector3.Lerp(direccion, d, 6f * dt);
			}
			float v = velocidad * (tipo == TipoEnemigo.Dron && distancia < 7f ? 1.5f : 1f);
			pos += direccion * v * dt;
			Vector3 mirar = plano.sqrMagnitude > 0.01f ? plano : transform.forward;
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(mirar), 8f * dt);
		}
		pos += retroceso * dt;
		if (tipo == TipoEnemigo.Centinela) pos.y = alturaSuelo;
		else pos.y = Mathf.Max(0.6f, pos.y);
		transform.position = pos;

		//Contacto con el jugador
		if (Vector3.Distance(Centro, j.Centro) < radioContacto + 0.4f) {
			if (j.RecibirGolpe(transform.position)) {
				if (tipo == TipoEnemigo.Dron) {
					Morir(false);
				} else {
					aturdido = 1.3f;
					Vector3 atras = transform.position - j.transform.position;
					atras.y = 0f;
					retroceso = atras.normalized * 6f;
				}
			}
		}
	}

	//Si hay un obstáculo delante, prueba direcciones a los lados
	Vector3 Esquivar(Vector3 d) {
		Vector3 origen = transform.position + Vector3.up * (tipo == TipoEnemigo.Dron ? 0f : 0.9f);
		float[] angulos = { 0f, 35f, -35f, 70f, -70f, 110f, -110f };
		foreach (float a in angulos) {
			Vector3 prueba = Quaternion.Euler(0f, a, 0f) * d;
			if (!Physics.SphereCast(origen, 0.45f, prueba, out _, 1.6f, Capas.Escenario, QueryTriggerInteraction.Ignore)) return prueba;
		}
		return -d;
	}

	void Animar(float dt) {
		if (tipo == TipoEnemigo.Centinela) {
			float paso = Mathf.Sin(fase * velocidad * 2.6f) * 28f;
			if (piernaI != null) piernaI.localRotation = Quaternion.Euler(paso, 0f, 0f);
			if (piernaD != null) piernaD.localRotation = Quaternion.Euler(-paso, 0f, 0f);
			if (brazoI != null) brazoI.localRotation = Quaternion.Euler(-paso * 0.6f, 0f, 0f);
			if (brazoD != null) brazoD.localRotation = Quaternion.Euler(paso * 0.6f, 0f, 0f);
		} else {
			if (rotores != null) foreach (var r in rotores) if (r != null) r.Rotate(0f, 1400f * dt, 0f, Space.Self);
			if (cuerpo != null) cuerpo.localRotation = Quaternion.Euler(Mathf.Sin(fase * 3f) * 6f, 0f, Mathf.Cos(fase * 2.3f) * 6f);
		}
	}

	public void Impacto(Vector3 punto, Collider parte) {
		if (muerto) return;
		var juego = Juego.I;
		var critica = parte != null ? parte.GetComponent<ParteEnemigo>() : null;
		int dano = critica != null ? critica.multiplicador : 1;
		vida -= dano;
		juego.aciertos++;
		destello.Activar();
		Efectos.Chispas(punto, (punto - transform.position).normalized);
		juego.SonarEn(critica != null ? juego.sCritico : juego.sGolpeEnemigo, punto);
		if (HUD.I != null) {
			HUD.I.Marca(critica != null);
			if (critica != null && vida > 0) HUD.I.Aviso("¡" + critica.etiqueta + "!  x" + critica.multiplicador, new Color(1f, 0.8f, 0.2f));
		}
		if (vida <= 0) {
			Morir(true);
		} else {
			Vector3 atras = transform.position - punto;
			atras.y = 0f;
			retroceso += atras.normalized * 2.5f;
			aturdido = Mathf.Max(aturdido, 0.15f);
		}
	}

	public void Morir(bool porElJugador) {
		if (muerto) return;
		muerto = true;
		var juego = Juego.I;
		if (porElJugador) {
			juego.SumarPuntos(puntos);
			juego.enemigos++;
			if (HUD.I != null) HUD.I.Puntos(puntos, nombre.ToUpper() + " DESTRUIDO");
		}
		juego.SonarEn(juego.sExplosion, Centro, tipo == TipoEnemigo.Dron ? 0.7f : 1f);
		Efectos.Explosion(Centro, tipo == TipoEnemigo.Dron ? 0.9f : 1.3f);
		Efectos.Restos(Centro, new Color(0.25f, 0.27f, 0.3f), 10);
		if (Nivel.I != null) Nivel.I.EnemigoDestruido(this);
		Destroy(gameObject);
	}

	public float FraccionVida => vidaMax > 0 ? vida / (float)vidaMax : 0f;
}
