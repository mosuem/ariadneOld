using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Line
{
	private List<Vector3> positions;

	public GameObject gameObject;

	public int positionCount{ get { return positions.Count; } }

	private MeshRenderer renderer;
	private Mesh mesh;
	Vector3[] vertices;
	int[] triangles;

	//	private MeshCollider coll;
	public float width;


	public int sortingOrder{ set { renderer.sortingOrder = value; } get { return renderer.sortingOrder; } }

	//	public List<Vector3> PositionList ()
	//	{
	//		List<Vector3> list = new List<Vector3> ();
	//		foreach (var point in positions) {
	//			list.Add (point);
	//		}
	//		return list;
	//	}

	public Line (GameObject parent)
	{
		positions = new List<Vector3> ();
		gameObject = parent;

		mesh = parent.AddComponent <MeshFilter> ().mesh;
		renderer = parent.AddComponent <MeshRenderer> ();
		renderer.sortingOrder = sortingOrder;
		mesh.vertices = vertices;
		mesh.triangles = triangles;
//		coll = parent.AddComponent <MeshCollider> ();
	}

	public void ClearPositions ()
	{
		positions.Clear ();
	}

	public void SetMesh ()
	{
		if (positionCount > 1) {
			mesh.Clear ();
			int triCounter = 0;
			int verCounter = 0;
			var triangleCount = (positionCount - 1) * 6 + (positionCount - 1) * 6;
//			Debug.Log ("Number triangleCount: " + triangleCount);
			triangles = new int[triangleCount];
			var vertexCount = (positionCount - 1) * 4;
//			Debug.Log ("Number vertexCount: " + vertexCount);
			vertices = new Vector3[vertexCount];
			for (int j = 0; j < positionCount - 1; j++) {
				var vertex1 = positions [j];
				var vertex2 = positions [j + 1];
				var direction = vertex2 - vertex1;
				Vector3 normal;
				if (Statics.isSphere) {
					normal = positions [j];
				} else if (Statics.isTorus) {
					normal = positions [j] - Misc.ManifoldCenter (positions [j]);
				} else {
					normal = Vector3.back;
				}
				var side = Vector3.Cross (direction, normal);
				side.Normalize ();
				side *= width / 2f;

				//Quad
				var corner1 = vertex1 + side;
				var corner2 = vertex1 - side;
				var corner3 = vertex2 + side;
				var corner4 = vertex2 - side;

				vertices [verCounter] = corner1;
				vertices [verCounter + 1] = corner2;
				vertices [verCounter + 2] = corner3;
				vertices [verCounter + 3] = corner4;
				//Tris between the points
				triangles [triCounter] = verCounter + 2;
				triangles [triCounter + 1] = verCounter + 1;
				triangles [triCounter + 2] = verCounter;

				triangles [triCounter + 3] = verCounter + 2;
				triangles [triCounter + 4] = verCounter + 3;
				triangles [triCounter + 5] = verCounter + 1;
				//Tris to back
				if (j < positionCount - 2) {
					triangles [triCounter + 6] = verCounter + 2;
					triangles [triCounter + 7] = verCounter + 4;
					triangles [triCounter + 8] = verCounter + 3;

					triangles [triCounter + 9] = verCounter + 4;
					triangles [triCounter + 10] = verCounter + 5;
					triangles [triCounter + 11] = verCounter + 3;
				}
				triCounter += 12;
				verCounter += 4;
			}
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.RecalculateBounds ();
			mesh.RecalculateNormals ();
//			coll.sharedMesh = mesh;
		}
	}

	public Vector3 GetPosition (int i)
	{
		return positions [i];
	}

	public List<Vector3> GetPositions ()
	{
		return new List<Vector3> (positions);
	}

	public void InsertPositionAt (int index, Vector3 item)
	{
		positions.Insert (index, item);
	}

	public void SetPosition (int j, Vector3 arg, bool setMesh = false)
	{
		if (j == positions.Count) {
			positions.Add (arg);
		} else {
			positions [j] = arg;
		}
		if (setMesh) {
			SetMesh ();
		}
//		if (resize) {
//			var middleVector = positions [j];
//			if (j > 0 && Vector3.Distance (positions [j - 1], middleVector) > Statics.meanDist) {
//				Vector3 betweenVector = Vector3.Lerp (positions [j - 1], middleVector, 0.5f);
//				InsertPositionAt (j, betweenVector);
//			} else if (j < positionCount - 1 && Vector3.Distance (middleVector, positions [j + 1]) > Statics.meanDist) {
//				Vector3 betweenVector = Vector3.Lerp (middleVector, positions [j + 1], 0.5f);
//				InsertPositionAt (j + 1, betweenVector);
//			}
//		}
	}

	public void SetPositions (List<Vector3> list)
	{
		positions = list;
		SetMesh ();
	}

	public void Remove (int i)
	{
		if (i >= 0 && i < positionCount) {
			positions.RemoveAt (i);
		}
	}

	public Color GetColor ()
	{
		return renderer.material.GetColor ("_Color");
	}

	public void SetColor (Color color)
	{
		renderer.material.SetColor ("_Color", color);
	}

	public void SetMaterial (Material pPathMaterial)
	{
		renderer.material = pPathMaterial;
	}
}
