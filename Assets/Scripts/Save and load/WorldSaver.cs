using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class WorldSaver : MonoBehaviour
{
	public GameObject baseChunkObject;
	public void Save()
	{
		WorldData worldData = new WorldData();
		SaveWorld(worldData);
		SaveFile(worldData);
	}

	protected void SaveWorld(WorldData worldData)
	{
		TerrainGenerator terrainGenerator = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<TerrainGenerator>();
		worldData.noiseFrequency = terrainGenerator.noiseFrequency;
		worldData.noiseSeed = terrainGenerator.noiseSeed;
		Transform playerTransform = GameObject.FindWithTag("Player").transform;
		worldData.playerPositionX = playerTransform.position.x;
		worldData.playerPositionY = playerTransform.position.y;
		worldData.playerPositionZ = playerTransform.position.z;
		worldData.fillChunks();
	}

	protected void SaveFile(WorldData data)
	{
		string destination = Application.persistentDataPath + "/save.dat";
		FileStream file;

		if (File.Exists(destination))
			file = File.OpenWrite(destination);
		else
			file = File.Create(destination);

		BinaryFormatter bf = new BinaryFormatter();
		bf.Serialize(file, data);
		file.Close();
	}
}
