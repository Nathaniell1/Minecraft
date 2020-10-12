using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public static class Enums
{
	public enum CubeType
	{
		Air,
		Grass,
		Dirt,
		Stone,
		Wood,
		Leaves,
		Water,
		Tree,
		Snow,
		WorldBottom,
	};
	public enum FaceType : byte
	{
		back	= 0,
		front	= 1,
		top	= 2,
		bottom	= 3,
		left	= 4,
		right	= 5,
	};

	public enum InteractionState : sbyte
	{
		None,
		Destroy,
		Stone,
		Dirt,
		Wood,
		Snow,
		Count,
	}

	public static bool isBuildingActive(this InteractionState state)
	{
		if (state != InteractionState.None && state != InteractionState.Destroy)
			return true;
		else
			return false;
	}

	public static bool isInteractionActive(this InteractionState state)
	{
		if (state != InteractionState.None)
			return true;
		else
			return false;
	}

	public static bool isDestroyActive(this InteractionState state)
	{
		if (state == InteractionState.Destroy)
			return true;
		else
			return false;
	}

	public static InteractionState getNext(this InteractionState state)
	{
		if (state == (InteractionState.Count - 1))
			return state;
		sbyte sbyte_state = (sbyte)state;
		return (InteractionState)((sbyte_state + 1) % (sbyte)InteractionState.Count);
	}

	public static InteractionState getPrevious(this InteractionState state)
	{
		if (state == InteractionState.None)
			return state;
		sbyte sbyte_state = (sbyte)state;
		return (InteractionState)((sbyte_state - 1) % (sbyte)InteractionState.Count);
	}

	public static CubeType GetMaterial(this InteractionState state)
	{
		switch (state)
		{
			case InteractionState.Stone:
				return CubeType.Stone;
			case InteractionState.Dirt:
				return CubeType.Dirt;
			case InteractionState.Wood:
				return CubeType.Wood;
			case InteractionState.Snow:
				return CubeType.Snow;
			default:
				Debug.Assert(false, "Trying to get unsupported material");
				return CubeType.Dirt;
		}
	}

	public static byte getLife(this CubeType type)
	{
		switch (type)
		{
			case CubeType.Air:
				return 0;
			case CubeType.Grass:
				return 1;
			case CubeType.Dirt:
				return 2;
			case CubeType.Stone:
				return 4;
			case CubeType.Wood:
				return 3;
			case CubeType.Leaves:
				return 1;
			case CubeType.Water:
				return 1;
			case CubeType.Tree:
				return 3;
			case CubeType.Snow:
				return 3;
			case CubeType.WorldBottom:
				return 255;
			default:
				return 255;
		}
	}


	public static bool isTypeSolid(this CubeType type)
	{
		return type != CubeType.Air;
	}

	public static Vector2 GetUvs(this CubeType type, FaceType face)
	{
		const float TpA = 4; // Textures per Axis in the atlas

		if (type == CubeType.Grass)
		{
			if (face == FaceType.top)
			{
				return new Vector2(3 / TpA, 2 / TpA);
			}
			if (face == FaceType.bottom)
			{
				return new Vector2(1 / TpA, 3 / TpA);
			}
			return new Vector2(2 / TpA, 3 / TpA);
		}

		switch (type)
		{
			case CubeType.Air:
				break;
			case CubeType.Dirt:
				return new Vector2( 1 / TpA, 3 / TpA );
			case CubeType.Stone:	  
				return new Vector2( 0 / TpA, 3 / TpA );
			case CubeType.Wood:	  
				return new Vector2( 0 / TpA, 2 / TpA );
			case CubeType.Leaves:
				return new Vector2(0 / TpA, 0 / TpA);
			case CubeType.Water:
				break;
			case CubeType.Tree:
				if(face == FaceType.top || face == FaceType.bottom)
					return new Vector2 ( 2 / TpA, 2 / TpA );
				else
					return new Vector2 ( 1 / TpA, 2 / TpA );
			case CubeType.Snow:
				return new Vector2(2 / TpA, 1 / TpA);
			case CubeType.WorldBottom:
				return new Vector2(1 / TpA, 1 / TpA);
			default:
				break;
		}

		return new Vector2(2/TpA, 1/TpA); // not everything is defined for now
	}

}
