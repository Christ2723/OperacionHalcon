using UnityEngine;

//Movimiento sencillo para objetivos y decoración: deslizarse, flotar o girar
public class Movimiento : MonoBehaviour {

	public enum Modo { Deslizar, Flotar, Girar }

	public Modo modo;
	public Vector3 eje = Vector3.right;
	public float distancia = 3f;
	public float velocidad = 1f;
	public float desfase;

	Vector3 inicio;

	void Start() => inicio = transform.localPosition;

	void Update() {
		float t = Time.time * velocidad + desfase;
		switch (modo) {
			case Modo.Deslizar:
				transform.localPosition = inicio + eje.normalized * Mathf.Sin(t) * distancia;
				break;
			case Modo.Flotar:
				transform.localPosition = inicio + Vector3.up * Mathf.Sin(t) * distancia + eje * Mathf.Sin(t * 0.7f) * distancia * 0.5f;
				break;
			case Modo.Girar:
				transform.Rotate(eje, velocidad * 60f * Time.deltaTime, Space.Self);
				break;
		}
	}
}
