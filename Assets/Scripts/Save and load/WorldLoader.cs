using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class WorldLoader : MonoBehaviour
{

	public GameObject baseChunkObject;
	protected TerrainGenerator terrainGenerator;

	public void Start()
	{
		terrainGenerator = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<TerrainGenerator>();
	}

	public void Load()
	{
		WorldData worldData;
		LoadFile(out worldData);
		LoadWorld(worldData);
	}

	protected void LoadWorld(WorldData worldData)
	{
		terrainGenerator.DestroyChunks();
		terrainGenerator.Reset();

		terrainGenerator.noiseFrequency = worldData.noiseFrequency;
		terrainGenerator.noiseSeed = worldData.noiseSeed;

		foreach (ChunkPos chunkPos in worldData.chunks.Keys)
		{
			Chunk chunk;
			GameObject chunkGO = Instantiate(baseChunkObject, new Vector3(chunkPos.x, 0, chunkPos.z), Quaternion.identity);
			chunk = chunkGO.GetComponent<Chunk>();
			chunk.name = "Chunk " + chunkPos.x + " " + chunkPos.z;

			chunk.voxelMap = worldData.chunks[chunkPos].data;
			if((Mathf.Abs(chunkPos.x - worldData.playerPositionX)) > terrainGenerator.chunkDrawDist * VoxelData.chunkWidth ||
			   (Mathf.Abs(chunkPos.z - worldData.playerPositionZ)) > terrainGenerator.chunkDrawDist * VoxelData.chunkWidth)
				terrainGenerator.unactiveChunks.Add(chunkPos, chunk);
			else
				terrainGenerator.activeChunks.Add(chunkPos, chunk);

		}

		foreach (ChunkPos chunkPos in terrainGenerator.activeChunks.Keys)
		{
			Chunk chunk = terrainGenerator.activeChunks[chunkPos];
			chunk.InitializeVoxelMesh();
		}

		foreach (ChunkPos chunkPos in terrainGenerator.unactiveChunks.Keys)
		{
			Chunk chunk = terrainGenerator.unactiveChunks[chunkPos];
			chunk.InitializeVoxelMesh();
			chunk.gameObject.SetActive(false);
		}



		Transform playerTransform = GameObject.FindWithTag("Player").transform;
		playerTransform.position = new Vector3(worldData.playerPositionX, worldData.playerPositionY, worldData.playerPositionZ);

		int curChunkPosX = Mathf.FloorToInt(playerTransform.position.x / VoxelData.chunkWidth) * VoxelData.chunkWidth;
		int curChunkPosZ = Mathf.FloorToInt(playerTransform.position.z / VoxelData.chunkWidth) * VoxelData.chunkWidth;
		ChunkPos curChunkPos = new ChunkPos(curChunkPosX, curChunkPosZ);
		// initialize chunk on which player is standing, if it wasn't in the save data
		if (! terrainGenerator.activeChunks.ContainsKey(curChunkPos))
		{
			terrainGenerator.BuildChunk(new Vector3(curChunkPosX, 0, curChunkPosZ));
			terrainGenerator.chunksToGenerate[0].InitializeCubeType();
			terrainGenerator.chunksToGenerate[0].InitializeVoxelMesh();
			terrainGenerator.chunksToGenerate.RemoveAt(0);
		}
		StartCoroutine(WaitTillChunksLoaded());
		
	}

	protected void LoadFile(out WorldData worldData)
	{
		string destination = Application.persistentDataPath + "/save.dat";
		FileStream file;

		if (File.Exists(destination)) file = File.OpenRead(destination);
		else
		{
			Debug.LogError("File not found");
			worldData = null;
			return;
		}

		BinaryFormatter bf = new BinaryFormatter();
		worldData = (WorldData)bf.Deserialize(file);
		file.Close();
	}

	IEnumerator WaitTillChunksLoaded()
	{
		PlayerMovement playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
		PlayerInteractions playerInteractions = GameObject.FindWithTag("MainCamera").GetComponent<PlayerInteractions>();
		playerMovement.enabled = false;
		playerInteractions.enabled = false;
		yield return new WaitForSeconds(2f); // This should ensure, that Update was called on TerrainGenerator and chunksToGenerate is populated
		yield return new WaitUntil(() => terrainGenerator.chunksToGenerate.Count == 0);
		playerMovement.enabled = true;
		playerInteractions.enabled = true;
	}

}
