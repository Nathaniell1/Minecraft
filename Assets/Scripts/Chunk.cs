using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
	public MeshRenderer meshRenderer;
	public MeshFilter meshFilter;
	public bool initializationDone = false;
	protected int vertexIdx = 0;
	protected List<Vector3> vertices = new List<Vector3>();
	protected List<int> triangles = new List<int>();  // list of vertex indexes from vertices
	protected List<Vector2> uvs = new List<Vector2>();
	protected const int faceCount = 6;
	protected static FastNoise noise;
	protected BiomType biomType;
	public VoxelState[,,] voxelMap = new VoxelState[VoxelData.chunkWidth, VoxelData.chunkHeight, VoxelData.chunkWidth];
	public bool modified = false;
	protected TerrainGenerator terrainGenerator;

	public void Awake()
	{
		terrainGenerator = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<TerrainGenerator>();
		noise = new FastNoise(terrainGenerator.noiseSeed);
		noise.SetFrequency(terrainGenerator.noiseFrequency);
	}

	public void Reset()
	{
		vertices.Clear();
		triangles.Clear();
		uvs.Clear();
		vertexIdx = 0;
	}
	public void InitializeCubeType()
	{
		generateBiomType();
		GenerateCubeTypes();
	}

	public void InitializeVoxelMesh()
	{
		Reset();
		GenerateChunkMesh();
		CreateMesh();
	}

	Enums.CubeType GetCubeType(Vector3 pos, int y, float perlin, int terrainHeight)
	{
		if (y == 0)
			return Enums.CubeType.WorldBottom;


		Enums.CubeType cubeType = Enums.CubeType.Stone;

		// first, generate basic terrain, without caves, just ground level and different type based on height
		if (y == terrainHeight)
		{
			float snowTreshhold = biomType.groundHeight + 0.7f * (biomType.terrainHeight - biomType.groundHeight);
			if (y >= (snowTreshhold))
				cubeType = Enums.CubeType.Snow;
			else
				cubeType = Enums.CubeType.Grass;
		}
		else if (y > terrainHeight)
			cubeType = Enums.CubeType.Air;
		else if (y < terrainHeight && y > terrainHeight - 4)
			cubeType = Enums.CubeType.Dirt;
		else
			cubeType = Enums.CubeType.Stone;


		//second pass, make caves inside the stone blocks
		if (cubeType == Enums.CubeType.Stone)
		{
			float perlin3D = noise.GetSimplex(pos.x, pos.y, pos.z);
			float perlin3DFractal = noise.GetSimplexFractal(pos.x, pos.y, pos.z);
			float perlinX = perlin3D * 2 * perlin3DFractal;

			if (perlinX > biomType.caveTreshold)
				cubeType = Enums.CubeType.Air;
		}

		return cubeType;
	}

	void GenerateCubeTypes()
	{

		for (int y = 0; y < VoxelData.chunkHeight; y++)
			for (int x = 0; x < VoxelData.chunkWidth; x++)
				for (int z = 0; z < VoxelData.chunkWidth; z++)
				{
					Vector3 pos = this.transform.position + new Vector3(x, 0, z);
					float perlin = noise.GetPerlin(pos.x, pos.z) * 1.65f; //noise never gets above cca 0.55 witout the multiplier
					{
						pos.y = pos.y + y;
						int terrainHeight = Mathf.FloorToInt(perlin * (biomType.terrainHeight - biomType.groundHeight)) + biomType.groundHeight;
						voxelMap[x, y, z].type = GetCubeType(pos, y, perlin, terrainHeight);
					}
				}

		// generate trees
		System.Random rand = new System.Random((int)this.transform.position.x + (int)this.transform.position.z);
		int treeCount = Mathf.FloorToInt((float)rand.NextDouble() * 7);
	
		for (int i = 0; i < treeCount; i++)
		{
			int xPos = (int)(rand.NextDouble() * VoxelData.chunkWidth);
			int zPos = (int)(rand.NextDouble() * VoxelData.chunkWidth);
			int yPos = VoxelData.chunkHeight - 1;

			// find the ground
			while (yPos > 0 && voxelMap[xPos, yPos, zPos].type == Enums.CubeType.Air)
				yPos--;

			createTree(xPos, yPos, zPos);

		}

	}

	void GenerateChunkMesh()
	{
		for (int y = 0; y < VoxelData.chunkHeight; y++)
			for (int x = 0; x < VoxelData.chunkWidth; x++)
				for (int z = 0; z < VoxelData.chunkWidth; z++)
				{
					if(voxelMap[x,y,z].type.isTypeSolid())
						AddVoxelToChunk(new Vector3(x, y, z));
				}
	}
	void AddVoxelToChunk(Vector3 pos)
	{
		// go through all the faces
		for (byte k = 0; k < faceCount; k++)
		{
			// if there isn't an adjecent voxel in given direction
			if (!isVoxelOccupied(pos, VoxelData.faceChecks[k]))
			{
				vertices.AddRange(VoxelData.GetVertices(k, pos));
				AddTexture(voxelMap[Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z)].type, k);
				triangles.AddRange(VoxelData.GetVertexIndices(vertexIdx));
				vertexIdx += 4;

			}
		}
	}

	void CreateMesh ()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();

		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	bool isVoxelOccupied(Vector3 pos, Vector3 offset)
	{
		int x = Mathf.FloorToInt(pos.x + offset.x);
		int y = Mathf.FloorToInt(pos.y + offset.y);
		int z = Mathf.FloorToInt(pos.z + offset.z);
		if (x < 0 || x > VoxelData.chunkWidth - 1)
			return isAdjecentChunkVoxelOccupied(transform.position, pos, offset);
		if (y < 0 || y > VoxelData.chunkHeight - 1)
			return false; // we don't grow in Y axis
		if (z < 0 || z > VoxelData.chunkWidth - 1)
			return isAdjecentChunkVoxelOccupied(transform.position, pos, offset);
		return voxelMap[x, y, z].type.isTypeSolid();
	}

	void AddTexture(Enums.CubeType type, byte face)
	{
		Vector2 baseUV = Enums.GetUvs(type, (Enums.FaceType)face);
		float x = baseUV.x;
		float y = baseUV.y;
		uvs.Add(baseUV);
		uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
		uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
		uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
	}

	public bool isAdjecentChunkVoxelOccupied(Vector3 chunkWorldPos, Vector3 voxelLocalPos, Vector3 offset)
	{
		if (offset.y < 0 || offset.y > VoxelData.chunkHeight)
			return false; // we don't generate new terrain up and down

		int chunkPosX = Mathf.FloorToInt((chunkWorldPos.x + voxelLocalPos.x+offset.x) / VoxelData.chunkWidth) * VoxelData.chunkWidth;
		int chunkPosZ = Mathf.FloorToInt((chunkWorldPos.z + voxelLocalPos.z+offset.z) / VoxelData.chunkWidth) * VoxelData.chunkWidth;
		ChunkPos cp = new ChunkPos(chunkPosX, chunkPosZ);

		if (!terrainGenerator.activeChunks.ContainsKey(cp))
		{
			// both return false and return true have its own problems
			// it should be true and then when generating new chunk, check if neighbouring chunks do not need refresh
			return false;
		}
		else
		{
			//get local position in the neighbouring chunk
			int localX = Mathf.FloorToInt(chunkWorldPos.x + voxelLocalPos.x + offset.x - chunkPosX);
			int localY = (int)voxelLocalPos.y;
			int localZ = Mathf.FloorToInt(chunkWorldPos.z + voxelLocalPos.z + offset.z - chunkPosZ);
			Chunk activeChunk;
			terrainGenerator.activeChunks.TryGetValue(cp, out activeChunk);
			return activeChunk.voxelMap[localX, localY, localZ].type.isTypeSolid();
		}
	}

	// this is just for demonstration purpouses how other biom types could work
	void generateBiomType()
	{
		biomType = new BiomType("grassy", 80, 95, Enums.CubeType.Grass, 0.5f);
	}


	void createTree(int posX, int posY, int posZ)
	{
		const int trunkHeight = 5;
		const int leavesThickness = 2;
		if (voxelMap[posX, posY, posZ].type != Enums.CubeType.Grass)
			return;

		if (posY + trunkHeight >= VoxelData.chunkHeight)
			return;
		if ((posX + (leavesThickness + 1)) >= VoxelData.chunkWidth)
			return;
		if ((posX - (leavesThickness + 1)) < 0)
			return;
		if ((posZ + (leavesThickness + 1)) >= VoxelData.chunkWidth)
			return;
		if ((posZ - (leavesThickness + 1)) < 0)
			return;

		System.Random rand = new System.Random();
		for (int x = posX - (leavesThickness + 1); x < posX + (leavesThickness + 1); x++)
			for (int y = posY + (trunkHeight - leavesThickness) +1; y < posY + (trunkHeight + leavesThickness); y++)
				for (int z = posZ - (leavesThickness + 1); z <= posZ + (leavesThickness + 1); z++)
				{
					if (rand.Next() % 3 == 0)
						voxelMap[x, y, z].type = Enums.CubeType.Leaves;
				}

		for (int i = posY + 1; i < (posY + trunkHeight); i++)
			voxelMap[posX, i, posZ].type = Enums.CubeType.Tree;
	}

}

[System.Serializable]
public struct VoxelState
{
	public Enums.CubeType type;
	public int hitCounter;
}
