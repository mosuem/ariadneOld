using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFactory
{
	private Material mat;
	private int counter;
	GameObject arrowPrefab;

	public PathFactory (Material pathMaterial, GameObject arrowPrefab)
	{
		this.arrowPrefab = arrowPrefab;
		mat = pathMaterial;
	}

	public Path newPath (Color color, GameObject from, GameObject to)
	{
		float height = counter * 0.001f;
		return new Path (counter++, mat, color, "Path " + counter, from, to, height, arrowPrefab);
	}

	public Path newPath (Color color, string name, GameObject from, GameObject to)
	{
		return new Path (counter++, mat, color, name, from, to, 0, arrowPrefab);
	}
}
