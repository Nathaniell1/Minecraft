using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData
{
	public int noiseSeed;
	public float noiseFrequency;
	public float playerPositionX;
	public float playerPositionY;
	public float playerPositionZ;

	public Dictionary<ChunkPos, ChunkData> chunks = new Dictionary<ChunkPos, ChunkData>();
	[System.NonSerialized] protected TerrainGenerator terrainGenerator;

	public WorldData()
	{
		terrainGenerator = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<TerrainGenerator>();
	}
	public void fillChunks()
	{
		foreach (ChunkPos chunkPos in terrainGenerator.activeChunks.Keys)
		{
			if (!terrainGenerator.activeChunks[chunkPos].modified)
				continue;
			ChunkData chunk = new ChunkData();
			chunk.data = terrainGenerator.activeChunks[chunkPos].voxelMap;
			chunks.Add(chunkPos, chunk);
		}

		foreach (ChunkPos chunkPos in terrainGenerator.unactiveChunks.Keys)
		{
			if (!terrainGenerator.unactiveChunks[chunkPos].modified)
				continue;
			ChunkData chunk = new ChunkData();
			chunk.data = terrainGenerator.unactiveChunks[chunkPos].voxelMap;
			chunks.Add(chunkPos, chunk);
		}
	}
}
