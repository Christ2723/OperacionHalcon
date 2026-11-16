using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//Efectos de sonido y música sintetizados aquí (composiciones originales)
public static partial class ConstructorProyecto {

	const int HZ = 22050;

	// ------------------------------------------------ WAV

	static AudioClip GuardarClip(string nombre, float[] datos, bool musica, float volumen = 0.9f) {
		float pico = datos.Max(m => Mathf.Abs(m));
		float escala = pico > 0f ? volumen / pico : 1f;
		string ruta = "Assets/Audio/" + nombre + ".wav";
		using (var bw = new BinaryWriter(File.Create(ruta))) {
			int bytes = datos.Length * 2;
			bw.Write("RIFF".ToCharArray());
			bw.Write(36 + bytes);
			bw.Write("WAVE".ToCharArray());
			bw.Write("fmt ".ToCharArray());
			bw.Write(16);
			bw.Write((short)1);
			bw.Write((short)1);
			bw.Write(HZ);
			bw.Write(HZ * 2);
			bw.Write((short)2);
			bw.Write((short)16);
			bw.Write("data".ToCharArray());
			bw.Write(bytes);
			foreach (float m in datos) bw.Write((short)(Mathf.Clamp(m * escala, -1f, 1f) * short.MaxValue));
		}
		AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
		var imp = (AudioImporter)AssetImporter.GetAtPath(ruta);
		var conf = imp.defaultSampleSettings;
		conf.loadType = musica ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
		conf.compressionFormat = AudioCompressionFormat.Vorbis;
		conf.quality = musica ? 0.55f : 0.75f;
		imp.defaultSampleSettings = conf;
		imp.SaveAndReimport();
		return AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
	}

	static float Seno(float f, float t) => Mathf.Sin(2f * Mathf.PI * f * t);
	static float Diente(float fase) => 2f * (fase - Mathf.Floor(fase + 0.5f));
	static float Cuadrada(float fase, float ciclo = 0.5f) => (fase - Mathf.Floor(fase)) < ciclo ? 1f : -1f;
	static float Midi(int nota) => 440f * Mathf.Pow(2f, (nota - 69) / 12f);

	static AudioClip Efecto(string nombre, float segundos, Func<float, System.Random, float> muestra, float volumen = 0.9f) {
		var r = new System.Random(nombre.GetHashCode());
		int n = (int)(segundos * HZ);
		var d = new float[n];
		for (int i = 0; i < n; i++) d[i] = muestra(i / (float)HZ, r);
		int f = Mathf.Min(300, n / 4);
		for (int i = 0; i < f; i++) d[n - 1 - i] *= i / (float)f;
		return GuardarClip(nombre, d, false, volumen);
	}

	//Filtro paso bajo de un polo aplicado a un generador
	class PasoBajo {
		float y;
		readonly float a;
		public PasoBajo(float corte) => a = 1f - Mathf.Exp(-2f * Mathf.PI * corte / HZ);
		public float this[float x] => y += a * (x - y);
	}

	static float R(System.Random r) => (float)(r.NextDouble() * 2.0 - 1.0);

	static void CrearEfectos(Juego j) {
		var pb = new PasoBajo(1800f);
		j.sDisparo = Efecto("Disparo", 0.45f, (t, r) => {
			float golpe = Seno(Mathf.Lerp(120f, 45f, Mathf.Clamp01(t / 0.12f)), t) * Mathf.Exp(-t * 14f);
			float chasquido = R(r) * Mathf.Exp(-t * 55f);
			float cola = pb[R(r)] * Mathf.Exp(-t * 7f) * 1.6f;
			return golpe * 0.9f + chasquido * 0.7f + cola;
		});
		j.sVacio = Efecto("Vacio", 0.09f, (t, r) => (Cuadrada(t * 2400f) * 0.3f + R(r) * 0.6f) * Mathf.Exp(-t * 90f), 0.6f);

		float faseRebote = 0f;
		j.sRebote = Efecto("Rebote", 0.3f, (t, r) => {
			faseRebote += Mathf.Lerp(3200f, 900f, t / 0.3f) / HZ;
			return Mathf.Sin(faseRebote * 2f * Mathf.PI) * Mathf.Exp(-t * 11f) * 0.5f + R(r) * Mathf.Exp(-t * 60f) * 0.6f;
		}, 0.6f);

		var pbMadera = new PasoBajo(900f);
		j.sDiana = Efecto("Diana", 0.35f, (t, r) =>
			Seno(190f, t) * Mathf.Exp(-t * 22f) + pbMadera[R(r)] * Mathf.Exp(-t * 16f) * 2.2f + Seno(420f, t) * Mathf.Exp(-t * 30f) * 0.4f);

		j.sBotella = Efecto("Botella", 0.7f, (t, r) => {
			float tintineo = (Seno(2150f, t) + Seno(3320f, t) * 0.7f + Seno(4710f, t) * 0.5f + Seno(2810f, t + 0.01f) * 0.6f) * Mathf.Exp(-t * 7f);
			float trozos = R(r) * Mathf.Exp(-t * 18f) * (0.6f + 0.4f * Mathf.Sin(t * 180f));
			return tintineo * 0.35f + trozos * 0.8f;
		});

		j.sGlobo = Efecto("Globo", 0.18f, (t, r) => R(r) * Mathf.Exp(-t * 45f) + Seno(110f, t) * Mathf.Exp(-t * 25f) * 0.6f);

		j.sMetal = Efecto("Metal", 1.1f, (t, r) =>
			(Seno(780f, t) + Seno(1265f, t) * 0.8f + Seno(2140f, t) * 0.5f + Seno(3010f, t) * 0.3f) * Mathf.Exp(-t * 4.5f) * 0.4f
			+ R(r) * Mathf.Exp(-t * 80f) * 0.7f);

		j.sCivil = Efecto("Civil", 0.6f, (t, r) => Cuadrada(t * (t < 0.3f ? 330f : 220f)) * 0.45f * (t < 0.28f || t > 0.32f ? 1f : 0f), 0.55f);

		j.sGolpeEnemigo = Efecto("GolpeEnemigo", 0.22f, (t, r) =>
			(Seno(520f, t) * 0.6f + Seno(1330f, t) * 0.3f) * Mathf.Exp(-t * 20f) + R(r) * Mathf.Exp(-t * 40f) * 0.7f);

		j.sCritico = Efecto("Critico", 0.35f, (t, r) =>
			(Seno(1560f, t) + Seno(2340f, t) * 0.6f) * Mathf.Exp(-t * 12f) * 0.5f + Seno(400f, t) * Mathf.Exp(-t * 25f) * 0.5f + R(r) * Mathf.Exp(-t * 45f) * 0.6f);

		var pbExplosion = new PasoBajo(500f);
		var pbExplosion2 = new PasoBajo(2500f);
		j.sExplosion = Efecto("Explosion", 1.6f, (t, r) => {
			float n = R(r);
			float grave = pbExplosion[n] * 3.5f * Mathf.Exp(-t * 2.6f);
			float estallido = pbExplosion2[n] * Mathf.Exp(-t * 14f);
			float retumbe = Seno(Mathf.Lerp(70f, 35f, t / 1.6f), t) * Mathf.Exp(-t * 3f) * 0.6f;
			return grave + estallido + retumbe;
		});

		float faseDano = 0f;
		var pbDano = new PasoBajo(1200f);
		j.sDano = Efecto("Dano", 0.45f, (t, r) => {
			faseDano += Mathf.Lerp(200f, 90f, t / 0.45f) / HZ;
			return pbDano[Diente(faseDano) * 0.8f + R(r) * 0.4f] * Mathf.Exp(-t * 5f) * 1.5f + R(r) * Mathf.Exp(-t * 50f) * 0.5f;
		});

		j.sMunicion = Efecto("Municion", 0.45f, (t, r) => {
			float c1 = R(r) * Mathf.Exp(-t * 80f) + Seno(1800f, t) * Mathf.Exp(-t * 60f) * 0.4f;
			float t2 = t - 0.16f;
			float c2 = t2 > 0f ? R(r) * Mathf.Exp(-t2 * 70f) + Seno(1200f, t2) * Mathf.Exp(-t2 * 40f) * 0.5f : 0f;
			float t3 = t - 0.26f;
			float tono = t3 > 0f ? Cuadrada(t3 * Mathf.Lerp(660f, 990f, t3 * 5f), 0.25f) * Mathf.Exp(-t3 * 9f) * 0.25f : 0f;
			return c1 * 0.8f + c2 * 0.8f + tono;
		});

		j.sVida = Efecto("Vida", 0.6f, (t, r) => {
			int paso = Mathf.Min(3, (int)(t / 0.12f));
			int[] notas = { 72, 76, 79, 84 };
			float tl = t - paso * 0.12f;
			return (Seno(Midi(notas[paso]), t) + Cuadrada(t * Midi(notas[paso]), 0.25f) * 0.2f) * Mathf.Exp(-tl * 6f) * 0.5f;
		});

		j.sNivel = Efecto("Nivel", 1.8f, (t, r) => {
			int[] notas = { 60, 64, 67, 72, 72, 76 };
			float[] inicio = { 0f, 0.15f, 0.3f, 0.45f, 0.9f, 0.9f };
			float s = 0f;
			for (int i = 0; i < notas.Length; i++) {
				float tl = t - inicio[i];
				if (tl < 0f) continue;
				float f = Midi(notas[i]);
				float env = Mathf.Min(1f, tl * 40f) * Mathf.Exp(-tl * (i >= 4 ? 1.6f : 5f));
				s += (Diente(t * f) * 0.5f + Diente(t * f * 1.005f) * 0.5f + Seno(f, t)) * env * 0.35f;
			}
			return s;
		});

		float faseRugido = 0f;
		var pbRugido = new PasoBajo(900f);
		j.sRugido = Efecto("Rugido", 1.6f, (t, r) => {
			float f = 68f + Mathf.Sin(t * 9f) * 9f + Mathf.Sin(t * 31f) * 4f;
			faseRugido += f / HZ;
			float env = Mathf.Min(1f, t * 6f) * Mathf.Exp(-Mathf.Max(0f, t - 0.9f) * 4f);
			return pbRugido[Diente(faseRugido) + Diente(faseRugido * 1.51f) * 0.6f + R(r) * 0.5f] * env * 2f;
		});

		var pbPaso = new PasoBajo(350f);
		j.sPaso = Efecto("Paso", 0.12f, (t, r) => pbPaso[R(r)] * Mathf.Exp(-t * 40f) * 3f, 0.6f);
		var pbSalto = new PasoBajo(1500f);
		j.sSalto = Efecto("Salto", 0.2f, (t, r) => pbSalto[R(r)] * Mathf.Sin(t / 0.2f * Mathf.PI) * 1.5f, 0.5f);
		j.sBoton = Efecto("Boton", 0.12f, (t, r) => Cuadrada(t * (t < 0.05f ? 880f : 1320f), 0.25f) * Mathf.Exp(-t * 20f), 0.5f);
		j.sAlerta = Efecto("Alerta", 1.2f, (t, r) => {
			float f = (int)(t / 0.3f) % 2 == 0 ? 880f : 660f;
			return (Cuadrada(t * f, 0.4f) * 0.4f + Seno(f, t) * 0.4f) * (t % 0.3f < 0.26f ? 1f : 0f);
		}, 0.6f);
	}

	// ------------------------------------------------ Música

	//Mezclador simple: suma notas de varios instrumentos sobre un búfer
	class Pista {
		public readonly float[] d;
		public readonly float pulso;
		public Pista(float bpm, int compases) {
			pulso = 60f / bpm;
			d = new float[(int)(pulso * 4f * compases * HZ)];
		}
		public float Duracion => d.Length / (float)HZ;

		public void Nota(float inicioPulsos, float duracionPulsos, Func<float, float, float> instrumento, float volumen, int nota) {
			float f = Midi(nota);
			int a = (int)(inicioPulsos * pulso * HZ);
			int n = (int)((duracionPulsos * pulso + 0.4f) * HZ);
			float dur = duracionPulsos * pulso;
			for (int i = 0; i < n; i++) {
				int k = (a + i) % d.Length;
				float t = i / (float)HZ;
				float env = t < dur ? 1f : Mathf.Exp(-(t - dur) * 14f);
				d[k] += instrumento(t, f) * env * volumen;
			}
		}

		public void Golpe(float inicioPulsos, Func<float, float> sonido, float volumen, float largo = 0.5f) {
			int a = (int)(inicioPulsos * pulso * HZ);
			int n = (int)(largo * HZ);
			for (int i = 0; i < n; i++) d[(a + i) % d.Length] += sonido(i / (float)HZ) * volumen;
		}
	}

	static readonly System.Random azar = new System.Random(7);
	static float N() => (float)(azar.NextDouble() * 2.0 - 1.0);

	static float Bombo(float t) => Seno(Mathf.Lerp(150f, 42f, Mathf.Clamp01(t * 14f)), t) * Mathf.Exp(-t * 9f);
	static float Caja(float t) => N() * Mathf.Exp(-t * 22f) * 0.8f + Seno(185f, t) * Mathf.Exp(-t * 25f) * 0.6f;
	static float Platillo(float t) => N() * Mathf.Exp(-t * 60f) * 0.5f;
	static float Timbal(float t) => Seno(Mathf.Lerp(110f, 70f, Mathf.Clamp01(t * 4f)), t) * Mathf.Exp(-t * 4f) + N() * Mathf.Exp(-t * 30f) * 0.3f;

	static float Bajo(float t, float f) {
		float s = Diente(t * f) * 0.6f + Seno(f, t) * 0.8f;
		return s * Mathf.Min(1f, t * 200f) * (0.6f + 0.4f * Mathf.Exp(-t * 6f));
	}
	static float Metales(float t, float f) {
		float vib = 1f + Mathf.Sin(t * 30f) * 0.004f * Mathf.Min(1f, t * 3f);
		float s = Diente(t * f * vib) * 0.5f + Diente(t * f * 1.004f) * 0.4f + Seno(f * 2f, t) * 0.15f;
		return s * Mathf.Min(1f, t * 25f);
	}
	static float Pulso(float t, float f) => Cuadrada(t * f, 0.3f) * Mathf.Min(1f, t * 300f) * (0.5f + 0.5f * Mathf.Exp(-t * 4f));
	static float Colchon(float t, float f) =>
		(Diente(t * f) + Diente(t * f * 1.007f) + Diente(t * f * 0.994f)) * 0.3f * Mathf.Min(1f, t * 2f);
	static float Campana(float t, float f) => (Seno(f, t) + Seno(f * 2.76f, t) * 0.3f) * Mathf.Exp(-t * 2.5f);

	static float[] Suavizar(float[] d, float corte) {
		var pb = new PasoBajo(corte);
		var r = new float[d.Length];
		for (int i = 0; i < d.Length; i++) r[i] = pb[d[i]];
		return r;
	}

	static void CrearMusica(Juego j) {
		//Menú: marcha lenta en re menor con redoble y metales
		var menu = new Pista(92f, 8);
		int[] acordesMenu = { 50, 46, 48, 45, 50, 46, 43, 45 };
		for (int c = 0; c < 8; c++) {
			float b = c * 4;
			menu.Nota(b, 4f, Colchon, 0.12f, acordesMenu[c] + 12);
			menu.Nota(b, 4f, Colchon, 0.09f, acordesMenu[c] + 19);
			menu.Nota(b, 1.5f, Bajo, 0.35f, acordesMenu[c] - 12);
			menu.Nota(b + 2f, 1.5f, Bajo, 0.3f, acordesMenu[c] - 12);
			menu.Golpe(b, Timbal, 0.5f, 1f);
			menu.Golpe(b + 2.5f, Timbal, 0.3f, 1f);
			for (int k = 0; k < 8; k++) menu.Golpe(b + k * 0.5f + (k % 2) * 0.25f, Caja, k % 4 == 3 ? 0.18f : 0.07f, 0.3f);
		}
		int[] melodia = { 62, 65, 69, 67, 65, 64, 62, 64, 65, 62, 58, 60, 62, 62, 57, 61 };
		float[] tiempos = { 0, 1, 2, 3.5f, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 15, 15.5f };
		for (int k = 0; k < melodia.Length; k++) {
			float dur = k + 1 < tiempos.Length ? tiempos[k + 1] - tiempos[k] : 0.5f;
			menu.Nota(16 + tiempos[k], dur * 0.9f, Metales, 0.22f, melodia[k]);
		}
		j.musicaMenu = GuardarClip("MusicaMenu", Suavizar(menu.d, 5000f), true, 0.8f);

		//Combate: ritmo rápido con bajo ostinato en re menor
		var nivel = new Pista(124f, 8);
		int[] raiz = { 38, 38, 34, 36, 38, 38, 41, 40 };
		for (int c = 0; c < 8; c++) {
			float b = c * 4;
			for (int k = 0; k < 4; k++) nivel.Golpe(b + k, Bombo, 0.75f, 0.4f);
			nivel.Golpe(b + 1, Caja, 0.4f, 0.3f);
			nivel.Golpe(b + 3, Caja, 0.4f, 0.3f);
			if (c % 2 == 1) nivel.Golpe(b + 3.5f, Caja, 0.25f, 0.3f);
			for (int k = 0; k < 8; k++) nivel.Golpe(b + k * 0.5f, Platillo, k % 2 == 1 ? 0.25f : 0.12f, 0.1f);
			int[] patron = { 0, 0, 12, 0, 0, 10, 0, 7 };
			for (int k = 0; k < 8; k++) nivel.Nota(b + k * 0.5f, 0.4f, Bajo, 0.32f, raiz[c] + patron[k]);
			nivel.Nota(b, 4f, Colchon, 0.06f, raiz[c] + 24);
			nivel.Nota(b, 4f, Colchon, 0.05f, raiz[c] + 27 + (raiz[c] == 34 || raiz[c] == 41 ? 1 : 0));
		}
		int[] riff = { 74, 72, 74, 77, 76, 74, 72, 69 };
		for (int c = 4; c < 8; c++) for (int k = 0; k < 8; k++) if ((k + c) % 3 != 2) nivel.Nota(c * 4 + k * 0.5f, 0.35f, Pulso, 0.1f, riff[k] - (c == 6 ? 2 : 0));
		j.musicaNivel = GuardarClip("MusicaNivel", Suavizar(nivel.d, 6000f), true, 0.8f);

		//Jefe: más rápido y pesado, en do menor, con metales graves
		var jefe = new Pista(138f, 8);
		int[] raizJefe = { 36, 36, 32, 34, 36, 36, 39, 35 };
		for (int c = 0; c < 8; c++) {
			float b = c * 4;
			for (int k = 0; k < 8; k++) if (k % 2 == 0 || k == 7) jefe.Golpe(b + k * 0.5f, Bombo, 0.8f, 0.4f);
			jefe.Golpe(b + 1, Caja, 0.5f, 0.3f);
			jefe.Golpe(b + 3, Caja, 0.5f, 0.3f);
			for (int k = 0; k < 16; k++) jefe.Golpe(b + k * 0.25f, Platillo, k % 4 == 2 ? 0.2f : 0.08f, 0.08f);
			for (int k = 0; k < 16; k++) jefe.Nota(b + k * 0.25f, 0.2f, Bajo, 0.3f, raizJefe[c] + (k % 8 == 6 ? 12 : 0) + (k % 8 == 7 ? 6 : 0));
			jefe.Nota(b, 2f, Metales, 0.14f, raizJefe[c] + 12);
			jefe.Nota(b + 2.5f, 1.5f, Metales, 0.12f, raizJefe[c] + 15);
			jefe.Golpe(b, Timbal, 0.4f, 1f);
		}
		for (int c = 4; c < 8; c++) {
			jefe.Nota(c * 4, 3.5f, Metales, 0.12f, raizJefe[c] + 24);
			jefe.Nota(c * 4, 3.5f, Metales, 0.1f, raizJefe[c] + 31);
		}
		j.musicaJefe = GuardarClip("MusicaJefe", Suavizar(jefe.d, 6500f), true, 0.8f);

		//Victoria: fanfarria en re mayor
		var victoria = new Pista(100f, 4);
		int[] fanfarria = { 62, 66, 69, 74, 74, 76, 78, 81 };
		float[] tf = { 0, 0.5f, 1, 1.5f, 3, 4, 5, 6 };
		for (int k = 0; k < fanfarria.Length; k++) victoria.Nota(tf[k], k < 3 ? 0.45f : 1.4f, Metales, 0.22f, fanfarria[k]);
		int[] acordesV = { 50, 55, 57, 50 };
		for (int c = 0; c < 4; c++) {
			victoria.Nota(c * 4, 4f, Colchon, 0.1f, acordesV[c] + 12);
			victoria.Nota(c * 4, 4f, Colchon, 0.08f, acordesV[c] + 16 + (c == 2 ? 0 : 0));
			victoria.Nota(c * 4, 4f, Bajo, 0.3f, acordesV[c] - 12);
			victoria.Golpe(c * 4, Timbal, 0.5f, 1f);
			for (int k = 0; k < 4; k++) victoria.Golpe(c * 4 + k, Caja, 0.12f, 0.3f);
		}
		for (int k = 0; k < 6; k++) victoria.Nota(8 + k * 1.33f, 1f, Campana, 0.15f, 74 + (k % 3) * 4);
		j.musicaVictoria = GuardarClip("MusicaVictoria", Suavizar(victoria.d, 6000f), true, 0.8f);

		//Derrota: lenta y grave
		var derrota = new Pista(70f, 4);
		int[] acordesD = { 50, 46, 43, 45 };
		for (int c = 0; c < 4; c++) {
			derrota.Nota(c * 4, 4f, Colchon, 0.13f, acordesD[c] + 12);
			derrota.Nota(c * 4, 4f, Colchon, 0.1f, acordesD[c] + 15 + (c == 3 ? 1 : 0));
			derrota.Nota(c * 4, 4f, Bajo, 0.25f, acordesD[c] - 12);
			derrota.Golpe(c * 4, Timbal, 0.35f, 1.5f);
		}
		int[] lamento = { 69, 67, 65, 64, 62, 61, 62 };
		for (int k = 0; k < lamento.Length; k++) derrota.Nota(2 + k * 2f, 1.8f, Metales, 0.13f, lamento[k]);
		j.musicaDerrota = GuardarClip("MusicaDerrota", Suavizar(derrota.d, 3500f), true, 0.8f);
	}

	static void CrearJuegoPrefab() {
		var go = new GameObject("Juego");
		var j = go.AddComponent<Juego>();
		j.fuenteMusica = go.AddComponent<AudioSource>();
		j.fuenteMusica.playOnAwake = false;
		j.fuenteMusica.loop = true;
		j.fuenteEfectos = go.AddComponent<AudioSource>();
		j.fuenteEfectos.playOnAwake = false;
		CrearEfectos(j);
		CrearMusica(j);
		PrefabUtility.SaveAsPrefabAsset(go, "Assets/Resources/Juego.prefab");
		UnityEngine.Object.DestroyImmediate(go);
	}
}
