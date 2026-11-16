using UnityEngine;

//Efectos visuales creados al momento: explosiones, chispas de impacto y restos que salen volando
public static class Efectos {

	static Material fuego, humo, restos;
	static MaterialPropertyBlock bloque;

	static void Cargar() {
		if (fuego != null) return;
		fuego = Resources.Load<Material>("Fuego");
		humo = Resources.Load<Material>("Humo");
		restos = Resources.Load<Material>("Restos");
		bloque = new MaterialPropertyBlock();
	}

	static ParticleSystem Sistema(Vector3 pos, Material mat, float duracion) {
		var go = new GameObject("Efecto");
		go.transform.position = pos;
		go.layer = Capas.Proyectil;
		var ps = go.AddComponent<ParticleSystem>();
		ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		go.GetComponent<ParticleSystemRenderer>().sharedMaterial = mat;
		var main = ps.main;
		main.loop = false;
		main.playOnAwake = false;
		main.duration = 0.5f;
		main.simulationSpace = ParticleSystemSimulationSpace.World;
		main.maxParticles = 200;
		var emision = ps.emission;
		emision.rateOverTime = 0f;
		Object.Destroy(go, duracion);
		return ps;
	}

	static void Desvanecer(ParticleSystem ps, Color inicio, Color fin) {
		var col = ps.colorOverLifetime;
		col.enabled = true;
		var g = new Gradient();
		g.SetKeys(
			new[] { new GradientColorKey(inicio, 0f), new GradientColorKey(fin, 1f) },
			new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0f, 1f) });
		col.color = g;
	}

	static void Rafaga(ParticleSystem ps, int cantidad) {
		var emision = ps.emission;
		emision.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)cantidad) });
		ps.Play();
	}

	public static void Explosion(Vector3 pos, float tam) {
		Cargar();

		var llamas = Sistema(pos, fuego, 2f);
		var m = llamas.main;
		m.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
		m.startSpeed = new ParticleSystem.MinMaxCurve(2f * tam, 7f * tam);
		m.startSize = new ParticleSystem.MinMaxCurve(0.6f * tam, 1.5f * tam);
		m.startColor = new Color(1f, 0.75f, 0.3f);
		var forma = llamas.shape;
		forma.shapeType = ParticleSystemShapeType.Sphere;
		forma.radius = 0.3f * tam;
		var tamano = llamas.sizeOverLifetime;
		tamano.enabled = true;
		tamano.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.6f, 1f, 1.4f));
		Desvanecer(llamas, new Color(1f, 0.9f, 0.5f), new Color(0.9f, 0.25f, 0.05f));
		Rafaga(llamas, 26);

		var nube = Sistema(pos, humo, 3.5f);
		m = nube.main;
		m.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.8f);
		m.startSpeed = new ParticleSystem.MinMaxCurve(0.8f * tam, 2.5f * tam);
		m.startSize = new ParticleSystem.MinMaxCurve(1f * tam, 2.2f * tam);
		m.gravityModifier = -0.08f;
		m.startColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
		forma = nube.shape;
		forma.shapeType = ParticleSystemShapeType.Sphere;
		forma.radius = 0.4f * tam;
		tamano = nube.sizeOverLifetime;
		tamano.enabled = true;
		tamano.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.7f, 1f, 1.8f));
		Desvanecer(nube, new Color(0.35f, 0.33f, 0.3f), new Color(0.15f, 0.15f, 0.15f));
		Rafaga(nube, 14);

		Chispas(pos, Vector3.up, 24, 9f * tam);

		var luz = new GameObject("Luz").AddComponent<Light>();
		luz.transform.position = pos;
		luz.type = LightType.Point;
		luz.color = new Color(1f, 0.6f, 0.25f);
		luz.range = 9f * tam;
		luz.intensity = 4f;
		luz.shadows = LightShadows.None;
		luz.gameObject.AddComponent<LuzFugaz>();
	}

	public static void Chispas(Vector3 pos, Vector3 normal) => Chispas(pos, normal, 12, 5f);

	static void Chispas(Vector3 pos, Vector3 normal, int cantidad, float rapidez) {
		Cargar();
		var ps = Sistema(pos, fuego, 1.5f);
		ps.transform.rotation = Quaternion.LookRotation(normal.sqrMagnitude > 0.001f ? normal : Vector3.up);
		var m = ps.main;
		m.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.4f);
		m.startSpeed = new ParticleSystem.MinMaxCurve(rapidez * 0.4f, rapidez);
		m.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
		m.gravityModifier = 1.2f;
		m.startColor = new Color(1f, 0.85f, 0.45f);
		var forma = ps.shape;
		forma.shapeType = ParticleSystemShapeType.Cone;
		forma.angle = 40f;
		forma.radius = 0.02f;
		var r = ps.GetComponent<ParticleSystemRenderer>();
		r.renderMode = ParticleSystemRenderMode.Stretch;
		r.velocityScale = 0.04f;
		r.lengthScale = 1.5f;
		Desvanecer(ps, new Color(1f, 0.95f, 0.7f), new Color(1f, 0.45f, 0.1f));
		Rafaga(ps, cantidad);
	}

	//Pedazos que salen volando al destruir un objetivo
	public static void Restos(Vector3 pos, Color color, int cantidad) {
		Cargar();
		bloque.SetColor("_Color", color);
		for (int i = 0; i < cantidad; i++) {
			var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
			go.name = "Resto";
			go.layer = Capas.Proyectil;
			go.transform.position = pos + Random.insideUnitSphere * 0.2f;
			go.transform.rotation = Random.rotation;
			go.transform.localScale = Vector3.one * Random.Range(0.05f, 0.16f);
			var rend = go.GetComponent<Renderer>();
			rend.sharedMaterial = restos;
			rend.SetPropertyBlock(bloque);
			rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			var rb = go.AddComponent<Rigidbody>();
			rb.mass = 0.1f;
			rb.linearVelocity = Random.insideUnitSphere * 4f + Vector3.up * Random.Range(2f, 5f);
			rb.angularVelocity = Random.insideUnitSphere * 12f;
			Object.Destroy(go, Random.Range(1.6f, 2.6f));
		}
	}
}
