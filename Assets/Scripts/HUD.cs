using UnityEngine;
using UnityEngine.UI;

//Interfaz durante el juego: puntos, munición, vidas, misión, mira, avisos y paneles de pausa y final
public class HUD : MonoBehaviour {

	public static HUD I;

	[Header("Marcadores")]
	public Text txtPuntos;
	public Text txtMunicion;
	public Text txtNivel;
	public Text txtMision;
	public Image[] corazones;

	[Header("Mira")]
	public Image marca;
	public Text txtPopup;
	public Text txtPopupDetalle;
	public Text txtAviso;
	public Image flashDano;

	[Header("Jefe")]
	public GameObject barraJefe;
	public Image rellenoJefe;
	public Text txtJefe;

	[Header("Paneles")]
	public GameObject panelPresentacion;
	public Text txtPresentacionTitulo;
	public Text txtPresentacionNombre;
	public Text txtPresentacionDescripcion;
	public Text txtPresentacionClic;
	public GameObject panelPausa;
	public GameObject panelFinal;
	public Text txtFinalTitulo;
	public Text txtFinalDetalle;

	float tiempoMarca, tiempoPopup, tiempoAviso, tiempoDano;
	Vector2 posPopup;

	void Awake() {
		I = this;
		marca.enabled = false;
		txtPopup.text = txtPopupDetalle.text = txtAviso.text = "";
		flashDano.color = new Color(1f, 0f, 0f, 0f);
		panelPausa.SetActive(false);
		panelFinal.SetActive(false);
		posPopup = txtPopup.rectTransform.anchoredPosition;
	}

	void Update() {
		var juego = Juego.I;
		var nivel = Nivel.I;
		float dt = Time.unscaledDeltaTime;

		txtPuntos.text = juego.puntos.ToString("N0");
		txtMunicion.text = juego.municion.ToString();
		txtMunicion.color = juego.municion <= 5 ? new Color(1f, 0.35f, 0.3f) : Color.white;
		for (int i = 0; i < corazones.Length; i++) corazones[i].enabled = i < juego.vidas;

		if (nivel != null) {
			switch (nivel.mision) {
				case Mision.Dianas:
					txtMision.text = "DIANAS  " + (nivel.TotalObjetivos - nivel.ObjetivosRestantes) + " / " + nivel.TotalObjetivos;
					break;
				case Mision.Enemigos:
					txtMision.text = "ENEMIGOS  " + nivel.EnemigosRestantes + "     OLEADA  " + (nivel.Oleada + 1) + " / " + nivel.oleadas.Length;
					break;
				case Mision.Jefe:
					txtMision.text = nivel.EnemigosRestantes > 0 ? "DRONES  " + nivel.EnemigosRestantes : "DERROTA AL COLOSO";
					break;
			}
			bool hayJefe = nivel.jefe != null;
			barraJefe.SetActive(hayJefe && nivel.Iniciado);
			if (hayJefe) {
				rellenoJefe.fillAmount = Mathf.Lerp(rellenoJefe.fillAmount, nivel.jefe.FraccionVida, 10f * dt);
				rellenoJefe.color = nivel.jefe.Aturdido ? new Color(0.35f, 0.85f, 1f) : new Color(0.9f, 0.18f, 0.12f);
			} else if (barraJefe.activeSelf && rellenoJefe.fillAmount > 0f) {
				rellenoJefe.fillAmount = 0f;
			}
		}

		if (tiempoMarca > 0f) {
			tiempoMarca -= dt;
			marca.enabled = tiempoMarca > 0f;
		}
		if (tiempoPopup > 0f) {
			tiempoPopup -= dt;
			float a = Mathf.Clamp01(tiempoPopup / 0.4f);
			txtPopup.color = new Color(txtPopup.color.r, txtPopup.color.g, txtPopup.color.b, a);
			txtPopupDetalle.color = new Color(1f, 1f, 1f, a);
			txtPopup.rectTransform.anchoredPosition = posPopup + Vector2.up * (1.3f - tiempoPopup) * 40f;
		}
		if (tiempoAviso > 0f) {
			tiempoAviso -= dt;
			var c = txtAviso.color;
			txtAviso.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(tiempoAviso / 0.5f));
		}
		if (tiempoDano > 0f) {
			tiempoDano -= dt;
			flashDano.color = new Color(0.8f, 0f, 0f, Mathf.Clamp01(tiempoDano) * 0.45f);
		}
		if (panelPresentacion.activeSelf) {
			txtPresentacionClic.color = new Color(1f, 0.85f, 0.3f, 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 4f));
		}
	}

	public void Presentar(Nivel nivel) {
		txtNivel.text = "NIVEL " + nivel.numero + "  ·  " + nivel.nombre.ToUpper();
		txtPresentacionTitulo.text = "NIVEL " + nivel.numero;
		txtPresentacionNombre.text = nivel.nombre.ToUpper();
		txtPresentacionDescripcion.text = nivel.descripcion;
		panelPresentacion.SetActive(true);
		if (nivel.jefe != null) txtJefe.text = nivel.jefe.nombre.ToUpper();
	}

	public void OcultarPresentacion() => panelPresentacion.SetActive(false);

	//Puntos ganados (o perdidos) junto a la mira
	public void Puntos(int cantidad, string detalle) {
		txtPopup.text = (cantidad >= 0 ? "+" : "") + cantidad;
		txtPopup.color = cantidad >= 0 ? new Color(1f, 0.86f, 0.3f) : new Color(1f, 0.3f, 0.25f);
		txtPopupDetalle.text = detalle;
		tiempoPopup = 1.3f;
	}

	//Marca de impacto en la mira (roja si fue crítico)
	public void Marca(bool critico) {
		marca.color = critico ? new Color(1f, 0.25f, 0.2f) : Color.white;
		marca.rectTransform.localScale = Vector3.one * (critico ? 1.3f : 1f);
		marca.enabled = true;
		tiempoMarca = 0.18f;
	}

	public void Aviso(string texto, Color color) {
		txtAviso.text = texto;
		txtAviso.color = color;
		tiempoAviso = 2f;
	}

	public void Dano() => tiempoDano = 1f;

	public void Final(string titulo, string detalle, bool gano) {
		panelFinal.SetActive(true);
		txtFinalTitulo.text = titulo;
		txtFinalTitulo.color = gano ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 0.35f, 0.3f);
		txtFinalDetalle.text = detalle;
	}

	public void MostrarPausa(bool mostrar) => panelPausa.SetActive(mostrar);

	// ------------------------------------------------ Botones

	public void BotonContinuar() => Nivel.I.Continuar();
	public void BotonReiniciar() => Nivel.I.Reiniciar();
	public void BotonMenu() => Nivel.I.IrAlMenu();
}
