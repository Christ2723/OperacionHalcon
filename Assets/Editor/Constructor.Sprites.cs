using System;
using System.IO;
using UnityEditor;
using UnityEngine;

//Imágenes de la interfaz: corazón, bala, panel redondeado, marca de impacto, viñeta
public static partial class ConstructorProyecto {

	static Sprite sBlanco, sCorazon, sPanel, sVineta, sBala, sMarca, sCirculo, sDegradado, sDegradadoH;

	static Sprite CrearSprite(string nombre, int ancho, int alto, Func<int, int, Color> pixel, Vector4 borde = default) {
		var tex = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
		for (int y = 0; y < alto; y++)
			for (int x = 0; x < ancho; x++)
				tex.SetPixel(x, y, pixel(x, y));
		tex.Apply();
		string ruta = "Assets/Sprites/" + nombre + ".png";
		File.WriteAllBytes(ruta, tex.EncodeToPNG());
		UnityEngine.Object.DestroyImmediate(tex);
		AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceSynchronousImport);
		var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
		imp.textureType = TextureImporterType.Sprite;
		imp.spriteImportMode = SpriteImportMode.Single;
		imp.alphaIsTransparency = true;
		imp.mipmapEnabled = false;
		imp.filterMode = FilterMode.Bilinear;
		imp.wrapMode = TextureWrapMode.Clamp;
		imp.spriteBorder = borde;
		imp.SaveAndReimport();
		return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
	}

	//Borde suave de una figura a partir de su distancia (negativa = dentro)
	static float Borde(float d, float suavidad = 1.2f) => Mathf.Clamp01(0.5f - d / suavidad);

	static void CrearSprites() {
		sBlanco = CrearSprite("Blanco", 8, 8, (x, y) => Color.white);

		sCorazon = CrearSprite("Corazon", 64, 64, (x, y) => {
			float u = (x - 31.5f) / 26f, v = (y - 28f) / 26f;
			float f = Mathf.Pow(u * u + v * v - 1f, 3f) - u * u * v * v * v;
			float a = f <= 0f ? 1f : Mathf.Clamp01(1f - f * 40f);
			float brillo = Mathf.Clamp01(1f - new Vector2(u + 0.35f, v - 0.35f).magnitude * 2.2f) * 0.5f;
			var c = Color.Lerp(new Color(0.85f, 0.1f, 0.12f), Color.white, brillo);
			return new Color(c.r, c.g, c.b, a);
		});

		sPanel = CrearSprite("Panel", 64, 64, (x, y) => {
			Vector2 p = new Vector2(Mathf.Abs(x - 31.5f), Mathf.Abs(y - 31.5f)) - new Vector2(31.5f - 10f, 31.5f - 10f);
			float d = new Vector2(Mathf.Max(p.x, 0f), Mathf.Max(p.y, 0f)).magnitude + Mathf.Min(Mathf.Max(p.x, p.y), 0f) - 9f;
			float a = Borde(d);
			float linea = Borde(Mathf.Abs(d + 1.5f) - 1f);
			var c = Color.Lerp(Color.white, new Color(1f, 1f, 1f), linea);
			return new Color(c.r, c.g, c.b, a * (0.85f + 0.15f * linea));
		}, new Vector4(20, 20, 20, 20));

		sVineta = CrearSprite("Vineta", 128, 128, (x, y) => {
			float d = new Vector2((x - 63.5f) / 64f, (y - 63.5f) / 64f).magnitude;
			return new Color(0f, 0f, 0f, Mathf.Clamp01((d - 0.55f) * 1.6f));
		});

		sBala = CrearSprite("Bala", 32, 96, (x, y) => {
			float cx = Mathf.Abs(x - 15.5f);
			if (y < 40) return cx < 10f ? new Color(0.85f, 0.62f, 0.2f, Borde(cx - 10f)) : Color.clear;
			if (y < 44) return cx < 11f ? new Color(0.6f, 0.42f, 0.12f, 1f) : Color.clear;
			float radio = 10f * Mathf.Sqrt(Mathf.Clamp01(1f - (y - 44f) / 50f));
			return new Color(0.75f, 0.45f, 0.3f, Borde(cx - radio));
		});

		sMarca = CrearSprite("Marca", 64, 64, (x, y) => {
			float u = x - 31.5f, v = y - 31.5f;
			float r = new Vector2(u, v).magnitude;
			float d = Mathf.Min(Mathf.Abs(u - v), Mathf.Abs(u + v)) / 1.414f;
			bool tramo = r > 9f && r < 28f;
			return new Color(1f, 1f, 1f, tramo ? Borde(d - 2.2f) : 0f);
		});

		sCirculo = CrearSprite("Circulo", 64, 64, (x, y) => {
			float d = new Vector2(x - 31.5f, y - 31.5f).magnitude - 30f;
			return new Color(1f, 1f, 1f, Borde(d));
		});

		sDegradado = CrearSprite("Degradado", 4, 128, (x, y) => new Color(1f, 1f, 1f, y / 127f));
		sDegradadoH = CrearSprite("DegradadoH", 256, 4, (x, y) => { float t = x / 255f; return new Color(1f, 1f, 1f, t < 0.45f ? 1f : Mathf.SmoothStep(1f, 0f, (t - 0.45f) / 0.55f)); });
	}
}
