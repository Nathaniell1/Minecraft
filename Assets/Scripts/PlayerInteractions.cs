using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
	public Transform highlightBlock;
	public Transform placeBlock;
	public LayerMask groundLayer;
	public float checkIncrement = 0.1f;
	public float reach = 5;
	public Transform player;
	public CharacterController controller;
	public float selfBoost;
	protected float actionRate = 0.35f;
	protected float nextAction = 0.0F;
	protected Vector3 inactiveHighlightPos = new Vector3(0, -100, 0);
	public AudioClip buildSound;
	public AudioClip destroySound;
	protected TerrainGenerator terrainGenerator;
	protected Enums.InteractionState interactionState = Enums.InteractionState.None;

	public void Start()
	{
		terrainGenerator = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<TerrainGenerator>();
	}

	void resetHightlights()
	{
		highlightBlock.position = inactiveHighlightPos;
		placeBlock.position = inactiveHighlightPos;
	}

	void Update()
	{
		RaycastHit hitInfo;
		if (!Physics.Raycast(transform.position, transform.forward, out hitInfo, reach, groundLayer))
		{
			resetHightlights();
		}
		else
		{

			bool scrollDown = Input.mouseScrollDelta.y < 0;
			bool scrollUp = Input.mouseScrollDelta.y > 0;
			bool leftClick = Input.GetMouseButton(0);
			bool rightClick = Input.GetMouseButton(1);

			if (!scrollDown && !scrollUp && !interactionState.isInteractionActive())
			{
				resetHightlights();
				return;
			}

			if(scrollDown)
			{
				resetHightlights();
				interactionState = interactionState.getNext();
				return;
			}

			if (scrollUp)
			{
				resetHightlights();
				interactionState = interactionState.getPrevious();
				return;
			}

			Vector3 pointInTargetBlock = new Vector3(0, 0, 0);
			Vector3 pointInTargetBlockIn = hitInfo.point + transform.forward * .01f;
			Vector3 pointInTargetBlockOut = hitInfo.point + transform.forward * .01f;
			Vector3 normal = hitInfo.normal;

			if (rightClick)
				pointInTargetBlock = hitInfo.point + transform.forward * .001f;//move a little inside the block
			else if (leftClick)
				pointInTargetBlock = hitInfo.point - transform.forward * .001f;

			//get the terrain chunk
			int chunkPosX = Mathf.FloorToInt(pointInTargetBlock.x / VoxelData.chunkWidth) * VoxelData.chunkWidth;
			int chunkPosZ = Mathf.FloorToInt(pointInTargetBlock.z / VoxelData.chunkWidth) * VoxelData.chunkWidth;

			if(interactionState.isDestroyActive())
				highlightBlock.position = new Vector3(Mathf.FloorToInt(pointInTargetBlockIn.x), Mathf.FloorToInt(pointInTargetBlockIn.y), Mathf.FloorToInt(pointInTargetBlockIn.z));
			if(interactionState.isBuildingActive())
				placeBlock.position = new Vector3(Mathf.FloorToInt(pointInTargetBlockOut.x), Mathf.FloorToInt(pointInTargetBlockOut.y), Mathf.FloorToInt(pointInTargetBlockOut.z)) + normal;

			if (Time.time >= nextAction && (leftClick || rightClick))
			{
				nextAction = Time.time + actionRate;
				ChunkPos cp = new ChunkPos(chunkPosX, chunkPosZ);
				Chunk chunk = terrainGenerator.activeChunks[cp];

				//index of the target block
				int bix = Mathf.FloorToInt(pointInTargetBlock.x) - chunkPosX;
				int biy = Mathf.FloorToInt(pointInTargetBlock.y);
				int biz = Mathf.FloorToInt(pointInTargetBlock.z) - chunkPosZ;

				// remove the block and recalculate mesh
				if (rightClick && interactionState.isDestroyActive())
				{
					AudioSource.PlayClipAtPoint(destroySound, hitInfo.point);
					HandleRightClick(bix, biy, biz, chunk, cp);
				}
				if (leftClick && interactionState.isBuildingActive())
				{
					AudioSource.PlayClipAtPoint(buildSound, hitInfo.point);
					HandleLeftClick(bix, biy, biz, chunk, cp);
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.F5))
		{
			Debug.Log("SAVING");
			WorldSaver saver = FindObjectOfType<WorldSaver>();
			saver.Save();
		}

		if (Input.GetKeyDown(KeyCode.F8))
		{
			Debug.Log("LOADING");
			WorldLoader loader = FindObjectOfType<WorldLoader>();
			loader.Load();
		}

	}

	void HandleLeftClick(int bix, int biy, int biz, Chunk chunk, ChunkPos cp)
	{
		// dont build over max height
		if (biy >= VoxelData.chunkHeight)
			return;

		int playerx = Mathf.FloorToInt(player.position.x) - cp.x;
		int playery = Mathf.FloorToInt(player.position.y);
		int playerz = Mathf.FloorToInt(player.position.z) - cp.z;


		// if player build under himself
		if (new Vector3(playerx, playery, playerz) == new Vector3(bix, biy, biz))
		{
			// if there is no space above, no block build
			if (biy == VoxelData.chunkHeight-1 || chunk.voxelMap[bix, biy + 1, biz].type.isTypeSolid())
				return;

			//boost him up so he won't get stuck in the block
			controller.enabled = false;
			player.position += new Vector3(0, selfBoost, 0);
			controller.enabled = true;
		}
		chunk.voxelMap[bix, biy, biz].type = interactionState.GetMaterial();
		chunk.voxelMap[bix, biy, biz].hitCounter = 0;
		chunk.InitializeVoxelMesh(); // this is not 100% needed, but without it, building would just add triangles for new blocks and never remove old unneeded triangles
		chunk.modified = true;

	}

	void HandleRightClick(int bix, int biy, int biz, Chunk chunk, ChunkPos cp)
	{
		chunk.voxelMap[bix, biy, biz].hitCounter++;
		if (chunk.voxelMap[bix, biy, biz].hitCounter != chunk.voxelMap[bix, biy, biz].type.getLife())
			return;
		chunk.voxelMap[bix, biy, biz].type = Enums.CubeType.Air;
		chunk.InitializeVoxelMesh();
		chunk.modified = true;
		
		Vector3 originalPos = new Vector3(cp.x, 0, cp.z);
		Vector3 localPos = new Vector3(bix, biy, biz);

		// regenerate mesh of neighbouring chunks, if it might be neccessary
		if (bix == 0)
		{
			Vector3 offset = new Vector3(-1, 0, 0);
			updateAdjecentChunkMesh(chunk, originalPos, localPos, offset);
		}
		else if (bix == VoxelData.chunkWidth-1)
		{
			Vector3 offset = new Vector3(1, 0, 0);
			updateAdjecentChunkMesh(chunk, originalPos, localPos, offset);
		}

		if (biz == 0)
		{
			Vector3 offset = new Vector3(0, 0, -1);
			updateAdjecentChunkMesh(chunk, originalPos, localPos, offset);
		}
		else if (biz == VoxelData.chunkWidth - 1)
		{
			Vector3 offset = new Vector3(0, 0, 1);
			updateAdjecentChunkMesh(chunk, originalPos, localPos, offset);
		}
	}

	void updateAdjecentChunkMesh(Chunk originalChunk, Vector3 originalPos, Vector3 localPos, Vector3 offset)
	{
		if (originalChunk.isAdjecentChunkVoxelOccupied(originalPos, localPos, offset))
		{
			int neighbourChunkPosX = Mathf.FloorToInt((originalPos.x + localPos.x + offset.x) / VoxelData.chunkWidth) * VoxelData.chunkWidth;
			int neighbourChunkPosZ = Mathf.FloorToInt((originalPos.z + localPos.z + offset.z) / VoxelData.chunkWidth) * VoxelData.chunkWidth;
			ChunkPos neighbourChunk = new ChunkPos(neighbourChunkPosX, neighbourChunkPosZ);
			terrainGenerator.activeChunks[neighbourChunk].InitializeVoxelMesh();
		}
	}
}


