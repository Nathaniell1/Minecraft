using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
	public static readonly byte chunkWidth = 16;
	public static readonly byte chunkHeight = 128;


	public static readonly byte TextureAtlasBlockMulti = 4; // how many textures per atlas axis
	public static float NormalizedBlockTextureSize = 1f/TextureAtlasBlockMulti;

	public static readonly Vector3[] voxelVerts = new Vector3[8] {
		new Vector3(0.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 1.0f, 0.0f),
		new Vector3(0.0f, 1.0f, 0.0f),
		new Vector3(0.0f, 0.0f, 1.0f),
		new Vector3(1.0f, 0.0f, 1.0f),
		new Vector3(1.0f, 1.0f, 1.0f),
		new Vector3(0.0f, 1.0f, 1.0f),
	};

	// stores indexes of vertices from VoxelVerts
	public static readonly int[,] voxelTris = new int[6, 6]
	{
		{0, 3, 1, 1, 3, 2}, // Back face
		{5, 6, 4, 4, 6, 7}, // Front face
		{3, 7, 2, 2, 7, 6}, // Top face
		{1, 5, 0, 0, 5, 4}, // Bottom Face
		{4, 7, 0, 0, 7, 3}, // Left face
		{1, 2, 5, 5, 2, 6}  // right face
	};

	public static readonly Vector2[] voxelUVs = new Vector2[6]
	{
		new Vector2(0.0f, 0.0f),
		new Vector2(0.0f, 1.0f),
		new Vector2(1.0f, 0.0f),
		new Vector2(1.0f, 0.0f),
		new Vector2(0.0f, 1.0f),
		new Vector2(1.0f, 1.0f)
	};

	// where to look to test if adjecent face exists
	public static readonly Vector3[] faceChecks = new Vector3[6] {

		new Vector3(0.0f, 0.0f, -1.0f),
		new Vector3(0.0f, 0.0f, 1.0f),
		new Vector3(0.0f, 1.0f, 0.0f),
		new Vector3(0.0f, -1.0f, 0.0f),
		new Vector3(-1.0f, 0.0f, 0.0f),
		new Vector3(1.0f, 0.0f, 0.0f)

	};

	public static Vector3[] GetVertices(int faceIndex, Vector3 pos)
	{
		Vector3 test = voxelVerts[0];
		Vector3[] ret = new Vector3[4];
		ret[0] = voxelVerts[voxelTris[faceIndex, 0]] + pos;
		ret[1] = voxelVerts[voxelTris[faceIndex, 1]] + pos;
		ret[2] = voxelVerts[voxelTris[faceIndex, 2]] + pos;
		ret[3] = voxelVerts[voxelTris[faceIndex, 5]] + pos;
		return ret;
	}

	public static Vector2[] GetUvs()
	{
		Vector2[] ret = new Vector2[4];
		ret[0] = voxelUVs[0];
		ret[1] = voxelUVs[1];
		ret[2] = voxelUVs[2];
		ret[3] = voxelUVs[5];
		return ret;
	}

	public static int[] GetVertexIndices(int index)
	{
		int[] indices = new int[6];
		indices[0] = index;
		indices[1] = index + 1;
		indices[2] = index + 2;
		indices[3] = index + 2;
		indices[4] = index + 1;
		indices[5] = index + 3;
		return indices;
	}

}
