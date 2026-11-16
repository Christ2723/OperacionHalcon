using UnityEngine;

//Pinta el modelo de blanco un instante cuando recibe un impacto
public class Destello : MonoBehaviour {

	Renderer[] renderers;
	Material[][] originales;
	Material golpe;
	float tiempo;

	void Awake() {
		renderers = GetComponentsInChildren<Renderer>();
		originales = new Material[renderers.Length][];
		for (int i = 0; i < renderers.Length; i++) originales[i] = renderers[i].sharedMaterials;
		golpe = Resources.Load<Material>("Golpe");
	}

	public void Activar(float segundos = 0.08f) {
		if (golpe == null) return;
		foreach (var r in renderers) {
			if (r == null || r is TrailRenderer || r is ParticleSystemRenderer) continue;
			var m = r.sharedMaterials;
			for (int i = 0; i < m.Length; i++) m[i] = golpe;
			r.sharedMaterials = m;
		}
		tiempo = segundos;
	}

	void Update() {
		if (tiempo <= 0f) return;
		tiempo -= Time.deltaTime;
		if (tiempo <= 0f) {
			for (int i = 0; i < renderers.Length; i++) if (renderers[i] != null) renderers[i].sharedMaterials = originales[i];
		}
	}
}
