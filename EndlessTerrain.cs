using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour {

	const float scale = 5f;

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	public LODInfo[] detailLevels;
	public static float maxViewDst;

	public Transform viewer;
	public Material mapMaterial;

    public int numberOfChunkRows;
    public int numberOfChunkColumns;

    int currentChunkCoordX;
    int currentChunkCoordY;

    public static Vector2 viewerPosition;
	Vector2 viewerPositionOld;
	static MapGenerator mapGenerator;
	int chunkSize;
	int chunksVisibleInViewDst;
    int chunkNumber = 0;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start() {
		mapGenerator = FindObjectOfType<MapGenerator> ();

		maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;
        
		chunkSize = MapGenerator.mapChunkSize - 1;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        
        currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int y = 0; y < numberOfChunkColumns; y++)
        {
            for (int x = 0; x < numberOfChunkRows; x++)
            {
                if (y == 0 && x == 0)
                {
                    //bottom left corner
                    int edge = 5;
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, edge, chunkNumber));
                    chunkNumber++;
                }
                else if (y == 0 && x == numberOfChunkRows - 1)
                {
                    //bottom right corner
                    int edge = 6;
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, edge, chunkNumber));
                    chunkNumber++;
                }
                else if (y == numberOfChunkColumns - 1 && x == 0)
                {
                    //top left corner
                    int edge = 7;
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, edge, chunkNumber));
                    chunkNumber++;
                }
                else if (y == numberOfChunkColumns - 1 && x == numberOfChunkRows - 1)
                {
                    //top right corner
                    int edge = 8;
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, edge, chunkNumber));
                    chunkNumber++;
                }
                else if (x == 0)
                {
                    //left edge
                    int edge = 1;
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, edge, chunkNumber));
                    chunkNumber++;
                }
                else if (y == numberOfChunkColumns - 1)
                {
                    //top edge
                    int edge = 2;
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, edge, chunkNumber));
                    chunkNumber++;
                }
                else if (x == numberOfChunkRows - 1)
                {
                    //right edge
                    int edge = 3;
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, edge, chunkNumber));
                    chunkNumber++;
                }
                else if (y == 0)
                {
                    //bottom edge
                    int edge = 4;
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, edge, chunkNumber));
                    chunkNumber++;
                }
                else
                {
                    int edge = 0;
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, edge, chunkNumber));
                    chunkNumber++;
                }
                
            }
        }
        UpdateVisibleChunks();
	}

	void Update() {
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z) / scale;

		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks ();
		}
    }
		
	void UpdateVisibleChunks() {

		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
			terrainChunksVisibleLastUpdate [i].SetVisible (false);
		}
		terrainChunksVisibleLastUpdate.Clear ();
			
		currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
		currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunkSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                
                if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
					terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
				} /*else {
					terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
				}*/

			}
		}
	}

    public class TerrainChunk {

		GameObject meshObject;
		Vector2 position;
		Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;
        MeshCollider meshCollider;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;

		MapData mapData;
		bool mapDataReceived;
		int previousLODIndex = -1;
        int edge;

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material, int edge, int chunkNumber) {
			this.detailLevels = detailLevels;
            this.edge = edge;
            
			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x, 0, position.y);

			meshObject = new GameObject("Terrain Chunk" + chunkNumber);
            meshCollider = meshObject.AddComponent<MeshCollider>();
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = material;
            

            meshObject.transform.position = positionV3 * scale;
			meshObject.transform.parent = parent;
			meshObject.transform.localScale = Vector3.one * scale;
			SetVisible(false);

			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++) {
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}
            chunkNumber++;
			mapGenerator.RequestMapData(position, OnMapDataReceived, edge);
		}

		void OnMapDataReceived(MapData mapData) {
			this.mapData = mapData;
			mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColourMap (mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			meshRenderer.material.mainTexture = texture;
           
			UpdateTerrainChunk ();
		}

	

		public void UpdateTerrainChunk() {
			if (mapDataReceived) {
				float viewerDstFromNearestEdge = Mathf.Sqrt (bounds.SqrDistance (viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxViewDst;

				if (visible) {
					int lodIndex = 0;

					for (int i = 0; i < detailLevels.Length - 1; i++) {
						if (viewerDstFromNearestEdge > detailLevels [i].visibleDstThreshold) {
							lodIndex = i + 1;
						} else {
							break;
						}
					}

					if (lodIndex != previousLODIndex) {
						LODMesh lodMesh = lodMeshes [lodIndex];
						if (lodMesh.hasMesh) {
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
                            meshCollider.sharedMesh = meshFilter.mesh;
                        } else if (!lodMesh.hasRequestedMesh) {
							lodMesh.RequestMesh (mapData);
						}
					}

					terrainChunksVisibleLastUpdate.Add (this);
				}

				SetVisible (visible);
			}
		}

		public void SetVisible(bool visible) {
			meshObject.SetActive (visible);
		}

		public bool IsVisible() {
			return meshObject.activeSelf;
		}

	}

	class LODMesh {

		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;
		System.Action updateCallback;

		public LODMesh(int lod, System.Action updateCallback) {
			this.lod = lod;
			this.updateCallback = updateCallback;
		}

		void OnMeshDataReceived(MeshData meshData) {
			mesh = meshData.CreateMesh ();
			hasMesh = true;

			updateCallback ();
		}

		public void RequestMesh(MapData mapData) {
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData (mapData, lod, OnMeshDataReceived);
		}

	}

	[System.Serializable]
	public struct LODInfo {
		public int lod;
		public float visibleDstThreshold;
	}

}
