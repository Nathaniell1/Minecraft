using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
	public Transform player;
	public GameObject baseChunkObject;
	public Dictionary<ChunkPos, Chunk> activeChunks = new Dictionary<ChunkPos, Chunk>();
	public Dictionary<ChunkPos, Chunk> unactiveChunks = new Dictionary<ChunkPos, Chunk>();
	public List<Chunk> chunksToGenerate = new List<Chunk>();
	public float noiseFrequency = 0.0095f;
	public int noiseSeed = 1265483;
	public byte chunkDrawDist;
	protected bool generating = false;
	protected ChunkPos curChunk;
	protected int counter = 0;

	public void Reset()
	{
		curChunk = new ChunkPos(-1, -1);
		counter = 0;
		activeChunks.Clear();
		unactiveChunks.Clear();
		chunksToGenerate.Clear();
		generating = false;
	}

	// Start is called before the first frame update
	void Start()
	{
		
		curChunk = new ChunkPos(-1, -1);
		LoadChunks();
		foreach (Chunk ch in chunksToGenerate)
		{
			ch.InitializeCubeType();
		}
		foreach (Chunk ch in chunksToGenerate)
		{
			ch.InitializeVoxelMesh();
			ch.initializationDone = true;

		}
		chunksToGenerate.Clear();
	}

	IEnumerator activateChunks()
	{
		generating = true;

		while (counter < chunksToGenerate.Count)
		{
			Chunk chunk = chunksToGenerate[counter];
			chunk.InitializeCubeType();
			++counter;
			yield return null;
		}
		while (counter > 0)
		{
			Chunk chunk = chunksToGenerate[0];
			chunk.InitializeVoxelMesh();
			chunk.initializationDone = true;
			chunksToGenerate.RemoveAt(0);
			--counter;
			yield return null;
		}
		// still missing refresh of the adjecent meshes

		counter = 0;
		generating = false;

	}

	// Update is called once per frame
	void Update()
	{
		LoadChunks();
		if (chunksToGenerate.Count > 0 && !generating)
			StartCoroutine(activateChunks());
	}

	public void DestroyChunks()
	{
		foreach(Chunk chunk in activeChunks.Values)
		{
			Object.Destroy(chunk.gameObject);
		}
		foreach (Chunk chunk in unactiveChunks.Values)
		{
			Object.Destroy(chunk.gameObject);
		}
		foreach (Chunk chunk in chunksToGenerate)
		{
			Object.Destroy(chunk.gameObject);
		}
	}
	void LoadChunks()
	{

		int curChunkPosX = Mathf.FloorToInt(player.position.x / VoxelData.chunkWidth) * VoxelData.chunkWidth;
		int curChunkPosZ = Mathf.FloorToInt(player.position.z / VoxelData.chunkWidth) * VoxelData.chunkWidth;
		// new chunk entered
		if (curChunk.x != curChunkPosX || curChunk.z != curChunkPosZ)
		{

			for (int i = curChunkPosX - chunkDrawDist * VoxelData.chunkWidth; i <= curChunkPosX + chunkDrawDist* VoxelData.chunkWidth; i += VoxelData.chunkWidth)
			{
				for (int j = curChunkPosZ - chunkDrawDist * VoxelData.chunkWidth; j <= curChunkPosZ + chunkDrawDist * VoxelData.chunkWidth; j += VoxelData.chunkWidth)
				{
					ChunkPos cp = new ChunkPos(i, j);
					// create new chunk
					if (!activeChunks.ContainsKey(cp) && !unactiveChunks.ContainsKey(cp))
					{
						BuildChunk(new Vector3(cp.x, 0, cp.z));
					}
					// activate old chunk
					else if(unactiveChunks.ContainsKey(cp))
					{
						Chunk chunk;
						unactiveChunks.TryGetValue(cp, out chunk);
						unactiveChunks.Remove(cp);
						chunk.gameObject.SetActive(true);
						activeChunks.Add(cp, chunk);
					}
				}
			}

			// deactivate chunks out of drawDist
			List<ChunkPos> activeToRemove = new List<ChunkPos>();
			foreach (ChunkPos chunkPos in activeChunks.Keys)
			{
				if (Mathf.Abs(curChunkPosX - chunkPos.x) > (chunkDrawDist * VoxelData.chunkWidth) || Mathf.Abs(curChunkPosZ - chunkPos.z) > (chunkDrawDist * VoxelData.chunkWidth))
				{
					Chunk chunk;
					activeChunks.TryGetValue(chunkPos, out chunk);
					chunk.gameObject.SetActive(false);
					unactiveChunks.Add(chunkPos, chunk);
					activeToRemove.Add(chunkPos);
				}
			}
			foreach (ChunkPos toRemovePos in activeToRemove)
			{
				activeChunks.Remove(toRemovePos);
			}

			curChunk.x = curChunkPosX;
			curChunk.z = curChunkPosZ;

		}
	}

	public void BuildChunk(Vector3 pos)
	{
		Chunk chunk;
		GameObject chunkGO = Instantiate(baseChunkObject, new Vector3(pos.x, 0, pos.z), Quaternion.identity);
		chunk = chunkGO.GetComponent<Chunk>();
		chunk.name = "Chunk " + pos.x + " " + pos.z;
		chunksToGenerate.Add(chunk);
		activeChunks.Add(new ChunkPos(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z)), chunk);
	}
}


[System.Serializable]
public struct ChunkPos
{
	public int x, z;
	public ChunkPos(int x, int z)
	{
		this.x = x;
		this.z = z;
	}
}