using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode { NoiseMap, ColourMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 239;
    int heightMapWidth;

    [Range(0, 6)]
    public int editorPreviewLOD;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    public bool falloffUsed;
    public bool justFallOff;
    public bool justBottom;
    public bool justRight;
    public bool justTop;
    public bool justLeft;

    float[,] falloffMap;
    float[,] topEdgeMap;
    float[,] bottomEdgeMap;
    float[,] leftEdgeMap;
    float[,] rightEdgeMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
        
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero, 0);
        
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            if (justFallOff)
            {
                if (justBottom)
                {
                    display.DrawTexture(TextureGenerator.TextureFromHeightMap(bottomEdgeMap));
                }
                if (justRight)
                {
                    display.DrawTexture(TextureGenerator.TextureFromHeightMap(rightEdgeMap));
                }
                if (justTop)
                {    
                    display.DrawTexture(TextureGenerator.TextureFromHeightMap(topEdgeMap));
                }
                if (justLeft)
                {
                    display.DrawTexture(TextureGenerator.TextureFromHeightMap(leftEdgeMap));
                }

            }
            else
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            }
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback, int edge)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(centre, callback, edge);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback, int edge)
    {
        MapData mapData = GenerateMapData(centre, edge);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 centre, int edge)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, centre + offset, normalizeMode);
        
        topEdgeMap = GradientGenerator.GenerateTopGradient(mapChunkSize);
        bottomEdgeMap = GradientGenerator.GenerateBottomGradient(mapChunkSize);
        leftEdgeMap = GradientGenerator.GenerateLeftGradient(mapChunkSize);
        rightEdgeMap = GradientGenerator.GenerateRightGradient(mapChunkSize);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (falloffUsed)
                {
                    if (justBottom)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - bottomEdgeMap[x, y]);
                    }
                    if (justRight)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - rightEdgeMap[x, y]);
                    }
                    if (justTop)
                    {                        
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - topEdgeMap[x, y]);
                    }
                    if (justLeft)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - leftEdgeMap[x, y]);
                    }                  
                }

                if (edge == 0)
                {
                    noiseMap[x, y] = noiseMap[x, y];
                }
                if (edge == 1)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - leftEdgeMap[x, y]);
                }
                else if (edge == 2)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - topEdgeMap[x, y]);
                }
                else if (edge == 3)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - rightEdgeMap[x, y]);
                }
                else if (edge == 4)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - bottomEdgeMap[x, y]);
                }
                else if (edge == 5)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - (bottomEdgeMap[x, y] + leftEdgeMap[x, y]));
                }
                else if (edge == 6)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - (bottomEdgeMap[x, y] + rightEdgeMap[x, y]));
                }
                else if (edge == 7)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - (topEdgeMap[x, y] + leftEdgeMap[x, y]));
                }
                else if (edge == 8)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - (topEdgeMap[x, y] + rightEdgeMap[x, y]));
                }

                float currentHeight = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colourMap);
    }

    void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }

    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}