using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkManager : MonoBehaviour {

	private Texture2D[,] textureChunks;
	private float[,][,] noiseMapChunks;
	private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary;

	private int chunkCols, chunkRows;
	private Texture2D worldMap;
	private LibNoise.Unity.Noise2D noiseMap;

	public int ChunkCols{  get{ return chunkCols;  } }
	public int ChunkRows{  get{ return chunkRows;  } }
	int chunkWidth, chunkHeight;
	public Transform chunkParent;

	[Header ("Player Stuff")]
	public Transform viewer;
	public Vector3 viewerPosition;
	private Vector3 viewerStartPosition;
	private float movementSpeed;

	public int currentChunkX;
	public int currentChunkY;
	private int visibleChunks;

	public static float maxViewDistance;
	public static readonly int lodSteps = 4;
	public static readonly float lodStepFactor = 200f;

	//Useful chunk retrieval functions:
	public float[,] GetNoiseMapChunk(int x, int y){
		return noiseMapChunks[x,y];
	}
	public Color[] GetColorMapChunk(int x, int y){
		return textureChunks[x,y].GetPixels();
	}
	public Texture2D GetTextureChunk(int x, int y){
		return textureChunks[x,y];
	}


	private void Start () {
		viewerStartPosition = new Vector3(0f, 25f, 0f);
		viewer.transform.position = viewerStartPosition;
		movementSpeed = 15f;

		maxViewDistance = 600;
		visibleChunks = 3;
	}


	private void Update () {
		
		if (Input.GetKeyDown (KeyCode.A)) {
			CheckPlayerPosition(KeyCode.A);
		}
		if (Input.GetKeyDown (KeyCode.W)) {
			CheckPlayerPosition (KeyCode.W);
		}
		if (Input.GetKeyDown (KeyCode.D)) {
			CheckPlayerPosition (KeyCode.D);
		}
		if (Input.GetKeyDown (KeyCode.S)) {
			CheckPlayerPosition (KeyCode.S);
		}
	}


	public void SplitWorldMap(Texture2D wMap, LibNoise.Unity.Noise2D nMap, int rows, int cols){
		chunkCols = cols;
		chunkRows = rows;
		worldMap = wMap;
		noiseMap = nMap;
		SplitTexture ();
	}


	private void UpdateVisibleChunks(){

		for (int j = 0; j < chunkRows; j++){
			for(int i = 0; i < chunkCols; i++){
				terrainChunkDictionary[new Vector2(i,j)].SetVisible(false);
			}
		}

		if(currentChunkX > chunkCols - 1){
			currentChunkX = 0;
			viewer.transform.position = new Vector3 ((currentChunkX * chunkWidth) - chunkWidth / 2, viewerPosition.y, viewerPosition.z);
		}
		else if(currentChunkX < 0){
			currentChunkX = chunkCols - 1;
			viewer.transform.position = new Vector3 ((currentChunkX * chunkWidth) + chunkWidth / 2, viewerPosition.y, viewerPosition.z);
		}
		if(currentChunkY > chunkRows - 1){
			currentChunkY = 0;
			viewer.transform.position = new Vector3 (viewerPosition.x, viewerPosition.y, (currentChunkY * chunkHeight) - chunkHeight / 2);
		}
		else if(currentChunkY < 0){
			currentChunkY = chunkRows - 1;
			viewer.transform.position = new Vector3 (viewerPosition.x, viewerPosition.y, (currentChunkY * chunkHeight) + chunkHeight / 2);
		}
		Vector2 index = new Vector2(currentChunkX, currentChunkY);


		for (int y = currentChunkY - visibleChunks; y <= currentChunkY + visibleChunks; y++){
			for(int x = currentChunkX - visibleChunks; x <= currentChunkX + visibleChunks; x++){
				index.x = (x < 0) ? chunkCols + x : (x >= chunkCols) ? x - chunkCols : x;
				index.y = (y < 0) ? chunkRows + y : (y >= chunkRows) ? y - chunkRows : y;
				
				for(int i = 0; i < lodSteps; i++){
					if((Mathf.Abs(x - currentChunkX) == i) || (Mathf.Abs(y - currentChunkY) == i)){
						terrainChunkDictionary[index].UpdateChunkLOD(i);
						terrainChunkDictionary[index].SetVisible(true);
					}
				}
				terrainChunkDictionary[index].MeshObject.transform.position = new Vector3 (chunkWidth * x, 0, chunkHeight * y);
			}
		}
	}


	private void CheckPlayerPosition(KeyCode keyPressed){
		switch (keyPressed){
			case KeyCode.A:		viewer.transform.position += new Vector3 (-movementSpeed, 0f, 0f);	break;
			case KeyCode.W:		viewer.transform.position += new Vector3 (0f, 0f, movementSpeed);	break;
			case KeyCode.D:		viewer.transform.position += new Vector3 (movementSpeed, 0f, 0f);	break;
			case KeyCode.S:		viewer.transform.position += new Vector3 (0f, 0f, -movementSpeed);	break;
		}
		viewerPosition = viewer.transform.position;
		currentChunkX = Mathf.RoundToInt(viewerPosition.x / chunkWidth);
		currentChunkY = Mathf.RoundToInt(viewerPosition.z / chunkHeight);
		UpdateVisibleChunks ();
	}


	private void SplitTexture(){
		textureChunks = new Texture2D[chunkCols, chunkRows];
		noiseMapChunks = new float[ChunkCols, chunkRows][,];

		int textureHeight = worldMap.height;
		int textureWidth = worldMap.width;
		chunkWidth = textureWidth / chunkCols;
		chunkHeight = textureHeight / chunkRows;

		for(int y = 0; y < chunkRows; y++){
			for(int x = 0; x < chunkCols; x++){
				textureChunks[x,y] = new Texture2D(chunkWidth, chunkHeight);
				noiseMapChunks[x,y] = new float[chunkWidth, chunkHeight];

				for (int j = 0; j < chunkHeight; j++){
					for(int i = 0; i < chunkWidth; i++){
						textureChunks[x,y].SetPixel(i, j, worldMap.GetPixel ((x * chunkWidth) + i, (y * chunkHeight) + j));
						noiseMapChunks[x,y][i,j] = noiseMap[(x * chunkWidth) + i, (y * chunkHeight) + j];
					}
				}
				textureChunks[x,y].wrapMode = TextureWrapMode.Clamp;
				textureChunks[x,y].filterMode = FilterMode.Point;
				textureChunks[x,y].Apply();
			}
		}
	}


	public void GenerateAllChunks(float hMultiplier, AnimationCurve curve, int lod){
		terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
		
		for(int y = 0; y < chunkRows; y++){
			for(int x = 0; x < chunkCols; x++){
				Material m = new Material(Shader.Find("Standard"));
				m.SetFloat("_Glossiness", 0f);
				Vector2 index = new Vector2(x,y);
				terrainChunkDictionary[index] = new TerrainChunk(x, y, chunkWidth, chunkHeight, chunkParent, m, lodSteps, lod);
				terrainChunkDictionary[index].CreateMesh(noiseMapChunks[x,y], textureChunks[x,y], hMultiplier, curve, lod);
			}
		}
		UpdateVisibleChunks();
	}



	class TerrainChunk {

		GameObject meshObject;
		Vector2 position;
		Bounds bounds;
		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
		MeshData meshData;

		int xCoord;
		int yCoord;

		float[,] noiseMapChunk;
		Texture2D colorMapChunk;
		float heightMultiplier;
		AnimationCurve heightCurve;

		LODMesh[] lodMeshes;

		public GameObject MeshObject { get { return meshObject; } }


		public TerrainChunk (int xC, int yC, int sizeX, int sizeY, Transform parent, Material material, int lodSteps, int lod) {
			xCoord = xC;
			yCoord = yC;
			position = new Vector2 (xC * sizeX, yC * sizeY);
			bounds = new Bounds (position, new Vector2 (sizeX, sizeY));
			Vector3 positionV3 = new Vector3 (position.x, 0f, position.y);

			meshObject = new GameObject ("Terrain Chunk [" + xC + "," + yC + "]");
			meshRenderer = meshObject.AddComponent<MeshRenderer> ();
			meshFilter = meshObject.AddComponent<MeshFilter> ();
			meshRenderer.material = material;

			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;
			Quaternion q = meshObject.transform.localRotation;
			meshObject.transform.localRotation = Quaternion.Euler (q.x, 0f, q.z);
			meshObject.transform.localScale = new Vector3 (1f, 1f, -1f);
			SetVisible (false);

			lodMeshes = new LODMesh[lodSteps];
			for(int i = 0; i < lodSteps; i++){
				lodMeshes[i].hasBeenCreated = false;
			}
		}

		public void CreateMesh (float[,] nMap, Texture2D cMap, float hMultiplier, AnimationCurve curve, int lod) {
			noiseMapChunk = nMap;
			colorMapChunk = cMap;
			heightMultiplier = hMultiplier;
			heightCurve = curve;
			MeshData meshData = MeshGenerator.GenerateTerrainMesh (noiseMapChunk, heightMultiplier, heightCurve, lod);
			meshObject.GetComponent<MeshFilter> ().mesh = meshData.CreateMesh ();
			meshObject.GetComponent<MeshRenderer> ().sharedMaterial.mainTexture = colorMapChunk;

			meshObject.name = "Terrain Chunk [" + this.xCoord + "," + this.yCoord + "]  -> LOD: " + lod * 2;  
			lodMeshes[lod].mesh = meshData.CreateMesh();
			lodMeshes[lod].hasBeenCreated = true;
		}

		public void SetVisible (bool visible) {
			meshObject.SetActive (visible);
		}

		public bool IsVisible () {
			return meshObject.activeSelf;
		}

		public void UpdateChunkLOD(int lodIndex){
			
			if(lodMeshes[lodIndex].hasBeenCreated){
				meshFilter.mesh = lodMeshes[lodIndex].mesh;
				meshObject.name = "Terrain Chunk [" + this.xCoord + "," + this.yCoord + "]  -> LOD: " + lodIndex * 2;
			}
			else{
				lodMeshes[lodIndex].mesh = MeshGenerator.GenerateTerrainMesh(noiseMapChunk, heightMultiplier, heightCurve, lodIndex * 2).CreateMesh();
				lodMeshes[lodIndex].hasBeenCreated = true;
				meshFilter.mesh = lodMeshes[lodIndex].mesh;
				meshObject.name = "Terrain Chunk [" + this.xCoord + "," + this.yCoord + "]  -> LOD: " + lodIndex * 2;
			}
		}
	}


	struct LODMesh{
		public Mesh mesh;
		public bool hasBeenCreated;
	}
}