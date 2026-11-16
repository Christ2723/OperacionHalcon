using UnityEngine;

//Capas físicas del juego. El escenario usa la capa Default.
public static class Capas {

	public const int Jugador = 8;
	public const int Enemigos = 9;
	public const int Proyectil = 10;
	public const int Objetivos = 11;

	//Lo que puede alcanzar un proyectil: todo menos el jugador y otros proyectiles
	public static readonly int Disparables = ~((1 << Jugador) | (1 << Proyectil) | (1 << 2));

	//Lo que bloquea el paso de enemigos y del jefe
	public static readonly int Escenario = 1 << 0;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void Configurar() {
		Physics.IgnoreLayerCollision(Proyectil, Jugador, true);
		Physics.IgnoreLayerCollision(Proyectil, Proyectil, true);
		Physics.IgnoreLayerCollision(Jugador, Enemigos, true);
	}
}

//Todo lo que reacciona cuando le llega un proyectil
public interface IImpactable {
	void Impacto(Vector3 punto, Collider parte);
}
