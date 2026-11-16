using UnityEngine;
using UnityEngine.UI;

//Pantalla final: dice si ganaste o perdiste, muestra la puntuación y las vidas restantes
//y permite volver a jugar o ir al menú.
public class Fin : MonoBehaviour {

	public Text txtTitulo;
	public Text txtMotivo;
	public Text txtPuntos;
	public Text txtEtiquetas;
	public Text txtValores;
	public Text txtRecord;
	public Transform camara;
	public Transform centro;

	void Start() {
		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		var j = Juego.I;
		j.Musica(j.victoria ? j.musicaVictoria : j.musicaDerrota, 0.5f);

		txtTitulo.text = j.victoria ? "¡MISIÓN CUMPLIDA!" : "MISIÓN FALLIDA";
		txtTitulo.color = j.victoria ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 0.35f, 0.3f);
		txtMotivo.text = j.motivo;
		txtPuntos.text = j.puntos.ToString("N0");

		int segundos = Mathf.RoundToInt(Time.time - j.tiempoInicio);
		txtEtiquetas.text = string.Join("\n", "Vidas restantes", "Munición restante", "Nivel alcanzado",
			"Dianas destruidas", "Enemigos destruidos", "Precisión", "Tiempo");
		txtValores.text = string.Join("\n", j.vidas, j.municion, j.nivelAlcanzado + " · " + j.nombreNivel, j.dianas, j.enemigos,
			Mathf.RoundToInt(j.Precision * 100f) + " %  (" + j.aciertos + " de " + j.disparos + ")",
			(segundos / 60) + ":" + (segundos % 60).ToString("00"));
		txtRecord.text = j.NuevoRecord ? "¡NUEVO RÉCORD!" : "RÉCORD  " + Juego.MejorPuntaje.ToString("N0");

		//También queda en la consola
		Debug.Log("FIN DE LA PARTIDA: " + txtTitulo.text + " | Puntos " + j.puntos + " | Vidas " + j.vidas + " | " + j.motivo);
	}

	void Update() {
		if (camara != null && centro != null) {
			camara.RotateAround(centro.position, Vector3.up, Time.deltaTime * 4f);
			camara.LookAt(centro.position + Vector3.up * 1.5f);
		}
		if (Input.GetKeyDown(KeyCode.Return)) VolverAJugar();
	}

	public void VolverAJugar() {
		Juego.I.Sonar(Juego.I.sBoton);
		Juego.I.NuevaPartida();
	}

	public void IrAlMenu() {
		Juego.I.Sonar(Juego.I.sBoton);
		Juego.I.Cargar(Juego.ESCENA_MENU);
	}
}
