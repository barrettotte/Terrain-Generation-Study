using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;
using LibNoise.Unity;
using System;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MapGenerator : MonoBehaviour{

    [Range(1, 6)] public int planetLOD;
    public float planetRadius;
    public enum RenderType {Greyscale, Color };
    
    public bool seamless = false;						//bool to select between seamless and non seamless (expensive) map generation
    public bool clamped = true;
    
    public RenderType renderType;
	public static int mapWidth = 968;					// 968, 1928, 3856
    public static int mapHeight = 968;
    public bool autoUpdate;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public string seed;
    public bool useRandomSeed;
    public TerrainType[] regions;
    [HideInInspector]   public int seedValue;
    [HideInInspector]	public Texture2D worldMap;
    [HideInInspector]	public NoiseFunctions[] noiseFunctions;

	[Range(0,5)]public int editorPreviewLOD;
	Noise2D planarNoiseMap;
	ModuleBase module;
	public bool worldMapNeedsGenerating;



	private void Start () {
		worldMapNeedsGenerating = true;
	}


	private void FixedUpdate () {
		if(worldMapNeedsGenerating){
			StartCoroutine (ThreadGenerateWorldMap());
		}
	}


	public void EditorGenerateMap(){
		GenerateWorldMap();
		SetWorldMap();
	}


	private void GenerateWorldMap(){
		planarNoiseMap = null;	
		module = moduleCreation();
		planarNoiseMap = new Noise2D(mapWidth, mapHeight, module);
		planarNoiseMap.GeneratePlanar (-1, 1, -1, 1, true);
	}


	private IEnumerator ThreadGenerateWorldMap(){
		print("World Map Generation Thread Started.");
		worldMapNeedsGenerating = false;
		float startTime = Time.unscaledTime;
		float endTime = 0f;

		Thread t = new Thread(() => {  GenerateWorldMap ();  });
		t.IsBackground = true;
		t.Start();
		while(t.IsAlive){  yield return null;  }
		endTime = Time.unscaledTime;
		SetWorldMap();
		print("World Map Generation Ended. Thread took " + (endTime - startTime) + " seconds.");
	}


	private void SetWorldMap(){
		worldMap = new Texture2D (planarNoiseMap.Width, planarNoiseMap.Height);
		MapDisplay display = FindObjectOfType<MapDisplay> ();

		if (renderType == RenderType.Greyscale) {
			worldMap = planarNoiseMap.GetTexture (LibNoise.Unity.Gradient.Grayscale);
		}
		else {
			worldMap.SetPixels (ColorMapCreation (planarNoiseMap));
		}
		worldMap.Apply ();
		display.DrawPlane (worldMap);
		//display.DrawPlanetMesh (OctahedronCreator.CreatePlanet (planetLOD, planetRadius, module, 0f, regions), worldMap);
		display.DrawMesh (planarNoiseMap, worldMap, heightMultiplier, heightCurve, editorPreviewLOD);
	}


	Color[] ColorMapCreation(Noise2D map){
		Color[] colorMap = new Color[map.Width * map.Height];
		for (int y = 0; y < map.Height; y++) {
			for (int x = 0; x < map.Width; x++) {
				float currentHeight = map[x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight <= regions[i].height) {
						colorMap[y * map.Width + x] = regions[i].color;
						break;
					}
				}
			}
		}
		return colorMap;
	}


	ModuleBase moduleCreation(){
		ModuleBase baseModule = null;

		if (!useRandomSeed) {
			seedValue = seed.GetHashCode ();
		}
		else {
			seedValue = UnityEngine.Random.Range (0, 10000000);
		}
		for (int i = 0; i < noiseFunctions.Length; i++) {
			noiseFunctions[i].seed = seedValue + i;
		}
		//generates noise for every noisefunction
		for (int i = 0; i < noiseFunctions.Length; i++) {
			if (noiseFunctions[i].enabled) {
				noiseFunctions[i].MakeNoise ();
			}
		}
		//manipulates the base module based on the noise modules
		for (int i = 0; i < noiseFunctions.Length; i++) {
			//for first valid noise pattern simply pass the noise function
			if (baseModule == null && noiseFunctions[i].enabled) {
				baseModule = noiseFunctions[i].moduleBase;
			}
			//all others valid add to the previous iteration of the baseModule
			else if (noiseFunctions[i].enabled) {
				baseModule = new Add (baseModule, noiseFunctions[i].moduleBase);
			}
		}
		if (clamped) {
			baseModule = new Clamp (0, 1, baseModule);
		}
		return baseModule;
	}


	#region Saving and loading
	public void SavePresets(NoiseFunctions[] savedPresets, string destpath){
        NoisePresets[] presetsToSave = new NoisePresets[savedPresets.Length];
        for (int i = 0; i < savedPresets.Length; i++){
            presetsToSave[i] = noiseFunctions[i].GetPresets();
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(destpath);
        bf.Serialize(file, presetsToSave);
        file.Close();
    }
    


    public void SaveImage(string filePath){
		Texture2D tex = new Texture2D(worldMap.width, worldMap.height);
		tex.SetPixels32(worldMap.GetPixels32());
		tex.Apply();

		byte[] data = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, data);
    }



    public void LoadPresets(string filePath){
        if (File.Exists(filePath)){
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            NoisePresets[] loadedPresets = (NoisePresets[])bf.Deserialize(file);
            NoiseFunctions[] holder = new NoiseFunctions[loadedPresets.Length];
            for (int i = 0; i < loadedPresets.Length; i++){
                holder[i] = new NoiseFunctions(loadedPresets[i]);
            }
            noiseFunctions = new NoiseFunctions[holder.Length];
            noiseFunctions = holder;
            file.Close();
        }
    }
#endregion
}



#region Libnoise Stuff
[System.Serializable]
public class NoiseFunctions{

    public enum NoiseType { Perlin, Billow, RiggedMultifractal, Voronoi, None };
    public NoiseType type = NoiseType.Perlin;
    public bool enabled = false;
    public ModuleBase moduleBase;
    
    [Range(0f,20f)]					 public double frequency;
    [Range(2.0000000f, 2.5000000f)]  public double lacunarity;
    [Range(0f, 1f)]					 public double persistence;
    [Range(1,18)]					 public int octaves;

    public int seed;
    public QualityMode qualityMode;
    public double displacement;
    public bool distance;

    public NoiseFunctions(){
        enabled = true;
        frequency = 1;
        lacunarity = 2.2;
        persistence = 0.5;
        octaves = 1;
        qualityMode = QualityMode.Low;
        displacement = 1;
        distance = true;
    }



    public NoiseFunctions(NoisePresets presets){
        enabled = presets.enabled;
        frequency = presets.frequency;
        lacunarity = presets.lacunarity;
        persistence = presets.persistence;
        octaves = presets.octaves;
        if (presets.qualityMode == NoisePresets.QualityMode.High){
            qualityMode = QualityMode.High;
        }
        else if (presets.qualityMode == NoisePresets.QualityMode.Medium){
            qualityMode = QualityMode.Medium;
        }
        else {
            qualityMode = QualityMode.Low;
        }

        if (presets.noiseType == NoisePresets.NoiseType.Billow){
            type = NoiseType.Billow;
        }
        else if (presets.noiseType == NoisePresets.NoiseType.Perlin){
            type = NoiseType.Perlin;
        }
        else if (presets.noiseType == NoisePresets.NoiseType.RiggedMultifractal){
            type = NoiseType.RiggedMultifractal;
        }
        else if (presets.noiseType == NoisePresets.NoiseType.Voronoi){
            type = NoiseType.Voronoi;
        }
        else {
            type = NoiseType.None;
        }
        displacement = presets.displacement;
        distance = presets.distance;
    }



    public NoisePresets GetPresets(){
        NoisePresets preset = new NoisePresets();
        preset.enabled = enabled;
        preset.frequency = frequency;
        preset.lacunarity = lacunarity;
        preset.persistence = persistence;
        preset.octaves = octaves;
        preset.displacement = displacement;
        preset.distance = distance;

        if (qualityMode == QualityMode.High){
            preset.qualityMode = NoisePresets.QualityMode.High;
        }
        else if (qualityMode == QualityMode.Medium){
            preset.qualityMode = NoisePresets.QualityMode.Medium;
        }
        else {
            preset.qualityMode = NoisePresets.QualityMode.Low;
        }

        if (type == NoiseType.Perlin){
            preset.noiseType = NoisePresets.NoiseType.Perlin;
        }
        else if (type == NoiseType.Billow){
            preset.noiseType = NoisePresets.NoiseType.Billow;
        }
        else if (type == NoiseType.RiggedMultifractal){
            preset.noiseType = NoisePresets.NoiseType.RiggedMultifractal;
        }
        else if (type == NoiseType.Voronoi){
            preset.noiseType = NoisePresets.NoiseType.Voronoi;
        }
        else{
            preset.noiseType = NoisePresets.NoiseType.None;
        }
        return preset;
    }



    //generates the mesh based on selected noise type
    public void MakeNoise(){
        if (type == NoiseType.Billow) { moduleBase = new Billow(frequency, lacunarity, persistence, octaves, seed, qualityMode);}
        else if (type == NoiseType.Perlin) { moduleBase = new Perlin(frequency, lacunarity, persistence, octaves, seed, qualityMode);}
        else if (type == NoiseType.Voronoi) { moduleBase = new Voronoi(frequency, displacement, seed, distance);}
        else if (type == NoiseType.RiggedMultifractal) { moduleBase = new RiggedMultifractal(frequency, lacunarity, octaves, seed, qualityMode);}
        else moduleBase = null;
    }
}



//used to save an .npr file
[System.Serializable]
public struct NoisePresets{
    public enum NoiseType {		Perlin, Billow, RiggedMultifractal, Voronoi, None };
    public enum QualityMode{	Low, Medium, High, };
    public NoiseType noiseType;
    public bool enabled;
    public double frequency;
    public double lacunarity;
    public double persistence;
    public int octaves;
    public QualityMode qualityMode;
    public double displacement;
    public bool distance;
}

#endregion




[System.Serializable]
public struct TerrainType{
    public string name;
    public double height;
    public Color color;
}


