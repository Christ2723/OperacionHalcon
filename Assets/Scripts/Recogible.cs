using UnityEngine;

public enum TipoRecogible { Municion, Botiquin }

//Caja de munición o botiquín. Se recoge al pasar por encima.
public class Recogible : MonoBehaviour {

	public TipoRecogible tipo;
	public int cantidad = 15;
	public Transform modelo;

	float fase;
	Vector3 baseModelo;

	void Start() {
		fase = Random.value * 6f;
		if (modelo != null) baseModelo = modelo.localPosition;
	}

	void Update() {
		fase += Time.deltaTime;
		if (modelo != null) {
			modelo.localPosition = baseModelo + Vector3.up * (Mathf.Sin(fase * 2.5f) * 0.12f);
			modelo.localRotation = Quaternion.Euler(0f, fase * 70f, 0f);
		}

		var j = Jugador.I;
		if (j == null || Nivel.I == null || !Nivel.I.EnJuego) return;
		Vector3 d = j.transform.position - transform.position;
		if (Mathf.Abs(d.y) > 2.2f) return;
		d.y = 0f;
		if (d.magnitude > 1.5f) return;

		var juego = Juego.I;
		if (tipo == TipoRecogible.Municion) {
			juego.municion += cantidad;
			juego.Sonar(juego.sMunicion);
			if (HUD.I != null) HUD.I.Aviso("+" + cantidad + " BALAS", new Color(1f, 0.85f, 0.3f));
		} else {
			if (juego.vidas >= Juego.VIDAS_MAX) return;
			j.GanarVida();
			juego.Sonar(juego.sVida);
			if (HUD.I != null) HUD.I.Aviso("+1 VIDA", new Color(0.45f, 1f, 0.5f));
		}
		Destroy(gameObject);
	}
}
