using UnityEngine;

//Bala trazadora. Avanza cada fotograma y comprueba con un barrido esférico qué alcanza:
//así no atraviesa objetos delgados aunque vaya muy rápido.
public class Proyectil : MonoBehaviour {

	public float radio = 0.05f;
	public float duracion = 3f;
	public float caida = 0.6f;

	Vector3 velocidad;

	public void Lanzar(Vector3 v) => velocidad = v;

	void Update() {
		float dt = Time.deltaTime;
		Vector3 paso = velocidad * dt;
		float distancia = paso.magnitude;
		if (distancia > 0f && Physics.SphereCast(transform.position, radio, paso / distancia, out var golpe, distancia,
			Capas.Disparables, QueryTriggerInteraction.Ignore)) {
			Impactar(golpe);
			return;
		}
		transform.position += paso;
		velocidad += Vector3.down * caida * dt;
		if (velocidad.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(velocidad);
		duracion -= dt;
		if (duracion <= 0f) Destroy(gameObject);
	}

	void Impactar(RaycastHit golpe) {
		transform.position = golpe.point;
		var impactable = golpe.collider.GetComponentInParent<IImpactable>();
		if (impactable != null) {
			impactable.Impacto(golpe.point, golpe.collider);
		} else {
			//Pared, suelo o cualquier otra cosa del escenario
			Efectos.Chispas(golpe.point, golpe.normal);
			Juego.I.SonarEn(Juego.I.sRebote, golpe.point, 0.5f);
		}
		Destroy(gameObject);
	}
}
