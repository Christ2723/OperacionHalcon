using UnityEngine;

//Lectura del teclado y el ratón. Las pruebas automáticas pueden simular el movimiento y los disparos.
public static class Entrada {

	public static bool Simulando;
	static Vector2 movimientoSimulado;
	static bool disparoSimulado;

	public static void Simular(Vector2 movimiento) {
		Simulando = true;
		movimientoSimulado = movimiento;
	}

	public static void SimularDisparo() {
		Simulando = true;
		disparoSimulado = true;
	}

	public static void DejarDeSimular() {
		Simulando = false;
		movimientoSimulado = Vector2.zero;
		disparoSimulado = false;
	}

	//Flechas del teclado (también WASD)
	public static Vector2 Movimiento {
		get {
			if (Simulando) return movimientoSimulado;
			float x = 0f, y = 0f;
			if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) x -= 1f;
			if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) x += 1f;
			if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) y -= 1f;
			if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) y += 1f;
			return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
		}
	}

	public static Vector2 Raton => Simulando ? Vector2.zero : new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

	//Clic izquierdo
	public static bool Disparo {
		get {
			if (Simulando) {
				bool d = disparoSimulado;
				disparoSimulado = false;
				return d;
			}
			return Input.GetMouseButtonDown(0);
		}
	}

	public static bool Salto => !Simulando && Input.GetKeyDown(KeyCode.Space);
	public static bool Correr => !Simulando && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
	public static bool Pausa => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
}
