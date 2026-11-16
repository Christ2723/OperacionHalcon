using UnityEngine;

//Luz de una explosión que se apaga en un instante
[RequireComponent(typeof(Light))]
public class LuzFugaz : MonoBehaviour {

	public float duracion = 0.35f;

	Light luz;
	float inicial;
	float t;

	void Awake() {
		luz = GetComponent<Light>();
		inicial = luz.intensity;
	}

	void Update() {
		t += Time.deltaTime;
		luz.intensity = Mathf.Lerp(inicial, 0f, t / duracion);
		if (t >= duracion) Destroy(gameObject);
	}
}
