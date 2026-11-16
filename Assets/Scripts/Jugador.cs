using UnityEngine;

//Soldado en primera persona: se mueve con las flechas, apunta con el ratón y salta con Espacio.
//Pierde una vida al chocar con un enemigo y queda invulnerable un momento.
[RequireComponent(typeof(CharacterController))]
public class Jugador : MonoBehaviour {

	public static Jugador I;

	public Transform camara;
	public Arma arma;

	[Header("Movimiento")]
	public float velocidad = 5.5f;
	public float velocidadCorrer = 8.5f;
	public float alturaSalto = 1.2f;
	public float gravedad = -22f;
	public float sensibilidad = 2f;

	[Header("Daño")]
	public float tiempoInvulnerable = 1.5f;

	CharacterController cc;
	float yaw, pitch;
	Vector3 velocidadVertical;
	Vector3 empuje;
	float invulnerable;
	float sacudida;
	float balanceo;
	float siguientePaso;
	Vector3 posicionCamara;

	public bool Invulnerable => invulnerable > 0f;
	public Vector3 Centro => transform.position + Vector3.up * 0.9f;
	public Vector3 Velocidad { get; private set; }

	void Awake() {
		I = this;
		cc = GetComponent<CharacterController>();
		yaw = transform.eulerAngles.y;
		posicionCamara = camara.localPosition;
	}

	void Update() {
		float dt = Time.deltaTime;
		bool activo = Nivel.I == null || Nivel.I.EnJuego;

		//Apuntar con el ratón
		if (activo) {
			var r = Entrada.Raton * sensibilidad;
			yaw += r.x;
			pitch = Mathf.Clamp(pitch - r.y, -85f, 85f);
		}
		transform.rotation = Quaternion.Euler(0f, yaw, 0f);

		//Moverse con las flechas
		Vector2 m = activo ? Entrada.Movimiento : Vector2.zero;
		bool corriendo = activo && Entrada.Correr && m.y > 0.1f;
		Vector3 direccion = (transform.right * m.x + transform.forward * m.y) * (corriendo ? velocidadCorrer : velocidad);

		if (cc.isGrounded && velocidadVertical.y < 0f) velocidadVertical.y = -2f;
		if (activo && cc.isGrounded && Entrada.Salto) {
			velocidadVertical.y = Mathf.Sqrt(alturaSalto * -2f * gravedad);
			Juego.I.Sonar(Juego.I.sSalto, 0.5f);
		}
		velocidadVertical.y += gravedad * dt;
		empuje = Vector3.Lerp(empuje, Vector3.zero, 5f * dt);

		Vector3 antes = transform.position;
		cc.Move((direccion + empuje + velocidadVertical) * dt);
		Velocidad = dt > 0f ? (transform.position - antes) / dt : Vector3.zero;

		//Balanceo de la cámara al caminar y pasos
		float rapidez = new Vector2(Velocidad.x, Velocidad.z).magnitude;
		if (cc.isGrounded && rapidez > 0.5f) {
			balanceo += dt * rapidez * 1.6f;
			if (Time.time >= siguientePaso) {
				siguientePaso = Time.time + (corriendo ? 0.3f : 0.42f);
				Juego.I.Sonar(Juego.I.sPaso, 0.35f);
			}
		} else {
			balanceo = Mathf.Lerp(balanceo, Mathf.Round(balanceo / Mathf.PI) * Mathf.PI, 6f * dt);
		}
		Vector3 bob = new Vector3(Mathf.Cos(balanceo) * 0.035f, Mathf.Abs(Mathf.Sin(balanceo)) * 0.05f, 0f);

		sacudida = Mathf.Max(0f, sacudida - dt);
		Vector3 temblor = Random.insideUnitSphere * sacudida * 0.25f;
		camara.localPosition = posicionCamara + bob + temblor;
		camara.localRotation = Quaternion.Euler(pitch, 0f, 0f);

		invulnerable = Mathf.Max(0f, invulnerable - dt);
	}

	//Orienta la mirada hacia un punto (lo usa la prueba automática)
	public void Mirar(Vector3 punto) {
		Vector3 d = punto - camara.position;
		yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
		pitch = -Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0f, yaw, 0f);
		camara.localRotation = Quaternion.Euler(pitch, 0f, 0f);
	}

	public void Teletransportar(Vector3 posicion) {
		cc.enabled = false;
		transform.position = posicion;
		cc.enabled = true;
	}

	public void Sacudir(float fuerza) => sacudida = Mathf.Max(sacudida, fuerza);

	//Choque con un enemigo: resta una vida. Devuelve false si no hizo daño (invulnerable).
	public bool RecibirGolpe(Vector3 desde) {
		if (invulnerable > 0f || Nivel.I == null || !Nivel.I.EnJuego) return false;
		invulnerable = tiempoInvulnerable;
		var j = Juego.I;
		j.vidas = Mathf.Max(0, j.vidas - 1);
		Vector3 d = transform.position - desde;
		d.y = 0f;
		empuje = (d.sqrMagnitude > 0.01f ? d.normalized : -transform.forward) * 11f;
		velocidadVertical.y = 4f;
		sacudida = 0.5f;
		j.Sonar(j.sDano);
		if (HUD.I != null) HUD.I.Dano();
		if (j.vidas <= 0) Nivel.I.Perder("Te quedaste sin vidas: los enemigos te alcanzaron.");
		return true;
	}

	public void GanarVida() {
		var j = Juego.I;
		j.vidas = Mathf.Min(Juego.VIDAS_MAX, j.vidas + 1);
	}
}
