using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomType
{
	public string name;
	public int groundHeight;
	public int terrainHeight;
	public float caveTreshold;
	Enums.CubeType groundType;

	public BiomType(string theName, int theGroundHeight, int theTerrainHeight, Enums.CubeType theGroundType, float theCaveTreshold)
	{
		name = theName;
		groundHeight = theGroundHeight;
		terrainHeight = theTerrainHeight;
		groundType = theGroundType;
		caveTreshold = theCaveTreshold;
	}
}
