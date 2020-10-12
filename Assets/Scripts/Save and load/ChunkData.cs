using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData 
{
	[HideInInspector]
	public VoxelState[,,] data = new VoxelState[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];
}
