using UnityEngine;

public enum TipoObjetivo { Diana, Botella, Globo, Placa, Civil, Antena }

//Algo a lo que se puede disparar. Al alcanzarlo da sus puntos y se destruye junto con el proyectil.
//Si la parte alcanzada tiene ZonaPuntos (los anillos de una diana), da los puntos de esa zona.
public class Objetivo : MonoBehaviour, IImpactable {

	public TipoObjetivo tipo;
	public string nombre = "Diana";
	public int puntos = 10;
	[Tooltip("Cuenta para completar la misión (los civiles y las antenas no cuentan)")]
	public bool cuenta = true;
	[Tooltip("Impactos necesarios para destruirlo")]
	public int resistencia = 1;
	public Color colorRestos = Color.white;

	bool destruido;
	Renderer[] renderers;
	Material[][] originales;
	Material golpe;
	float destello;

	void Awake() {
		renderers = GetComponentsInChildren<Renderer>();
		originales = new Material[renderers.Length][];
		for (int i = 0; i < renderers.Length; i++) originales[i] = renderers[i].sharedMaterials;
		golpe = Resources.Load<Material>("Golpe");
	}

	public bool Destruido => destruido;

	public void Impacto(Vector3 punto, Collider parte) {
		if (destruido) return;
		var juego = Juego.I;

		resistencia--;
		if (resistencia > 0) {
			juego.SonarEn(juego.sMetal, punto);
			Efectos.Chispas(punto, -transform.forward);
			Destellar();
			if (HUD.I != null) HUD.I.Marca(false);
			return;
		}
		destruido = true;

		int ganados = puntos;
		string etiqueta = nombre.ToUpper();
		var zona = parte != null ? parte.GetComponent<ZonaPuntos>() : null;
		if (zona != null) {
			ganados = zona.puntos;
			etiqueta += " · " + zona.etiqueta;
		}
		juego.SumarPuntos(ganados);
		if (tipo != TipoObjetivo.Civil) juego.aciertos++;
		if (cuenta) juego.dianas++;
		if (HUD.I != null) {
			HUD.I.Puntos(ganados, etiqueta);
			HUD.I.Marca(zona != null && zona.esCentro);
		}

		switch (tipo) {
			case TipoObjetivo.Botella: juego.SonarEn(juego.sBotella, punto); break;
			case TipoObjetivo.Globo: juego.SonarEn(juego.sGlobo, punto); break;
			case TipoObjetivo.Placa: juego.SonarEn(juego.sMetal, punto); break;
			case TipoObjetivo.Civil: juego.Sonar(juego.sCivil); break;
			case TipoObjetivo.Antena: juego.SonarEn(juego.sExplosion, punto, 0.8f); break;
			default: juego.SonarEn(juego.sDiana, punto); break;
		}
		if (tipo == TipoObjetivo.Antena) Efectos.Explosion(transform.position + Vector3.up * 3f, 1.6f);
		Efectos.Restos(punto, colorRestos, tipo == TipoObjetivo.Globo ? 6 : 12);
		Efectos.Chispas(punto, -transform.forward);

		if (Nivel.I != null) Nivel.I.ObjetivoDestruido(this);
		Destroy(gameObject);
	}

	void Destellar() {
		if (golpe == null) return;
		foreach (var r in renderers) {
			var m = r.sharedMaterials;
			for (int i = 0; i < m.Length; i++) m[i] = golpe;
			r.sharedMaterials = m;
		}
		destello = 0.08f;
	}

	void Update() {
		if (destello <= 0f) return;
		destello -= Time.deltaTime;
		if (destello <= 0f) {
			for (int i = 0; i < renderers.Length; i++) renderers[i].sharedMaterials = originales[i];
		}
	}
}
