using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

	[Header ("Planar Map")]
	public Renderer mapRendererPlane;

	[Header ("Planet Mesh")]
	public MeshFilter planetMeshFilter;
    public MeshRenderer planetMeshRenderer;

	[Header ("Terrain Mesh")]
	public MeshFilter terrainMeshFilter;
	public MeshRenderer terrainMeshRenderer;

	public int chunkRows;
	public int chunkCols;

	public void DrawPlanetMesh(Mesh mesh, Texture2D texture){
        planetMeshFilter.sharedMesh = mesh;
		texture.filterMode = FilterMode.Point;
        planetMeshRenderer.sharedMaterial.mainTexture = texture;
	}

	public void DrawPlane(Texture2D texture){
		texture.filterMode = FilterMode.Point;
		Material m = new Material(Shader.Find("Unlit/Texture"));
		m.mainTexture = texture;
		mapRendererPlane.sharedMaterial = m;
		planetMeshRenderer.sharedMaterial.mainTexture = texture;
	}

	public void DrawMesh(LibNoise.Unity.Noise2D noiseMap, Texture2D texture, float hMultiplier, AnimationCurve curve, int lod){
		ChunkManager chunkMang = FindObjectOfType<ChunkManager>();
		chunkMang.SplitWorldMap(texture, noiseMap, chunkRows, chunkCols);
		chunkMang.GenerateAllChunks (hMultiplier, curve, lod);
	}
}