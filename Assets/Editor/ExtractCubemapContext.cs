using UnityEngine;
using UnityEditor;

public class ExtractCubemapContext : MonoBehaviour{


	[MenuItem("CONTEXT/TextureImporter/Extract Cubemap", true)]
	public static bool ExtractCubemapValidate(MenuCommand mc){
		var path = AssetDatabase.GetAssetPath(mc.context);
		if (string.IsNullOrEmpty(path) == false){
			var assets = AssetDatabase.LoadAllAssetsAtPath(path);
			foreach (var asset in assets){
				if (asset is Cubemap){
					return true;
				}
			}
		}
		return false;
	}
	

	[MenuItem("CONTEXT/TextureImporter/Extract Cubemap")]
	public static void ExtractCubemap(MenuCommand mc){
		var path = AssetDatabase.GetAssetPath(mc.context);
		
		if (string.IsNullOrEmpty(path) == false){
			var assets = AssetDatabase.LoadAllAssetsAtPath(path);
			if (RemoveExtension(ref path) == true){
				foreach (var asset in assets){
					var cubemap = asset as Cubemap;
					if (cubemap != null){
						SaveCubemapFace(cubemap, CubemapFace.NegativeX, "PositiveX", path);
						SaveCubemapFace(cubemap, CubemapFace.NegativeY, "NegativeY", path);
						SaveCubemapFace(cubemap, CubemapFace.NegativeZ, "NegativeZ", path);
						SaveCubemapFace(cubemap, CubemapFace.PositiveX, "NegativeX", path);
						SaveCubemapFace(cubemap, CubemapFace.PositiveY, "PositiveY", path);
						SaveCubemapFace(cubemap, CubemapFace.PositiveZ, "PositiveZ", path);
					}
				}
			}
		}
	}
	

	private static bool RemoveExtension(ref string path){
		var lastDot = path.LastIndexOf(".");
		if (lastDot >= 0){
			path = path.Substring(0, lastDot);
			return true;
		}
		return false;
	}

	
	private static void SaveCubemapFace(Cubemap cubemap, CubemapFace face, string faceName, string basePath){
		var pixels  = GetPixels(cubemap, face);
		var texture = new Texture2D(cubemap.width, cubemap.height);
		var path    = basePath + "_" + faceName + ".png";//AssetDatabase.GenerateUniqueAssetPath(path + "_" + f + ".png");
		texture.SetPixels(pixels);
		texture.Apply();
		SaveTexture(texture, path);
		EditorDestroy(texture);
		Debug.Log("Saved cubemap face to: " + path);
	}


	public static T EditorDestroy<T> (T o)
		where T : Object {
		if (o != null) {
#if UNITY_EDITOR
			if (Application.isPlaying == true) {
				Object.Destroy (o);
			}
			else {
				Object.DestroyImmediate (o);
			}
#else
			Object.Destroy(o);
#endif
		}
		return null;
	}


	private static Color[] GetPixels(Cubemap c, CubemapFace f){
		var pixels = c.GetPixels(f);
		var halfH  = c.height / 2;
		
		for (var y = 0; y < halfH; y++){
			var o = c.width * y;
			var n = c.width * (c.height - y - 1);
			for (var x = 0; x < c.width; x++){
				var a = pixels[o + x];
				var b = pixels[n + x];
				pixels[o + x] = b;
				pixels[n + x] = a;
			}
		}
		return pixels;
	}
	

	private static void SaveTexture(Texture2D texture, string path){
		var bytes = texture.EncodeToPNG();
		var fs    = new System.IO.FileStream(path, System.IO.FileMode.Create);
		var bw    = new System.IO.BinaryWriter(fs);
		
		bw.Write(bytes);
		bw.Close();
		fs.Close();
		AssetDatabase.ImportAsset(path);
	}
}