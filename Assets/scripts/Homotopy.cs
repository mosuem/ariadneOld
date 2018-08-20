using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using System.Linq;
using System.Diagnostics;

public class Homotopy
{

	public Path path1;
	public Vector3 constpoint = new Vector3 (0, 0);
	//	public List<LineRenderer> homotopyLines = new List<LineRenderer> ();
	public Path midPath;
	public Material matPrefab;
	public Dictionary<int,Vector3> closestPositions = new Dictionary<int, Vector3> ();
	public List<int> snugglingNodePositions = new List<int> ();
	public List<Path> curveBundle = new List<Path> ();
	GameObject parentMeshObject = new GameObject ("Parent Mesh");

	static float spacer = Statics.sphereRadius * 10;

	List<Mesh> meshes = new List<Mesh> ();

	public Homotopy (Path p1, Path pMidPath, Material mat)
	{
		matPrefab = mat;
		path1 = p1;
		midPath = pMidPath;
		AddMesh ();
	}

	Mesh AddMesh ()
	{
		var meshObject = new GameObject ("Child Mesh");
		meshObject.transform.parent = parentMeshObject.transform;
		meshObject.AddComponent<MeshRenderer> ().material = matPrefab;
		var meshFilter = meshObject.AddComponent<MeshFilter> ();
		var mesh = meshFilter.mesh;
		meshes.Add (mesh);
		return mesh;
	}

	public void Clear ()
	{
		midPath.Clear ();
		var color = path1.color;
		color.a = color.a * 2f;
		path1.SetColor (color);
		GameObject.Destroy (parentMeshObject);
		meshes.Clear ();
	}

	public void setMesh (List<Vector3> staticPositions)
	{
		return;
		setMesh (staticPositions, midPath);
	}

	public void SetColor (Color color)
	{
		matPrefab.SetColor ("_Color", color);
	}

	public void AddCurveToBundle (PathFactory pf, int draggingPosition)
	{
		Path path;
		if (curveBundle.Count == 0) {
			path = path1;
		} else {
			path = curveBundle [curveBundle.Count - 1];
		}
		if (Vector3.Distance (path.line.GetPosition (draggingPosition), midPath.line.GetPosition (draggingPosition)) > Statics.bundleDist) {
			Path newPath = pf.newPath (path1.color, path1.dotFrom, path1.dotTo);
			newPath.line.SetPositions (midPath.line.GetPositions ());
			curveBundle.Add (newPath);
		}
	}

	public void setMesh (List<Vector3> staticPositions, Path path2)
	{
		if (Statics.isSphere) {
			setSphereMesh (staticPositions, path2);
		} else if (Statics.isTorus) {
			setTorusMesh (staticPositions, path2);
		} else {
			set2DMesh (staticPositions, path2);
		}
	}

	void set2DMesh (List<Vector3> staticPositions, Path path2)
	{
		var mesh = meshes [0];
		mesh.Clear ();
		var polygon = new Polygon ();
		var points = getPoints (path2);
		var vertices = ToVertex (points);
		var contour = new Contour (vertices);
		Point point = null;
		for (int i = 0; i < staticPositions.Count; i++) {
			if (IsPointInPolygon (staticPositions [i], vertices)) {
				point = new Point (staticPositions [i].x, staticPositions [i].y);
				UnityEngine.Debug.Log ("Is inside");
				break;
			} else {
				if (IsPointInPolygon (constpoint, vertices)) {
					UnityEngine.Debug.Log (constpoint + " Is inside");
				}
			}
		}
		if (point != null) {
			polygon.Add (contour, point);
		} else {
			polygon.Add (contour);
		}
		var options = new ConstraintOptions () {

		};
		var quality = new QualityOptions () {
//			MinimumAngle = 25,
//			MaximumArea = 0.01d
		};
		// Triangulate the polygon
		var polyMesh = polygon.Triangulate (options, quality);
		var polyVertices = polyMesh.Vertices;
		var polyTriangles = polyMesh.Triangles;
		List<Vector3> meshVertices = new List<Vector3> ();
		List<int> triangles = getTriangles (polyTriangles, meshVertices);
		mesh.vertices = meshVertices.ToArray ();
		mesh.triangles = triangles.ToArray ();
		//		Vector3[] normals = mesh.normals;
		//		for (int i = 0; i < normals.Length; i++)
		//			normals [i] = -normals [i];
		//		mesh.normals = normals;
		//		for (int m = 0; m < mesh.subMeshCount; m++) {
		//			int[] triangles2 = mesh.GetTriangles (m);
		//			for (int i = 0; i < triangles2.Length; i += 3) {
		//				int temp = triangles2 [i + 0];
		//				triangles2 [i + 0] = triangles2 [i + 1];
		//				triangles2 [i + 1] = temp;
		//			}
		//			mesh.SetTriangles (triangles2, m);
		//		}
	}


	void setSphereMesh (List<Vector3> staticPositions, Path path2)
	{
		foreach (var meshFilter in parentMeshObject.GetComponentsInChildren<MeshFilter> ()) {
			GameObject.Destroy (meshFilter.gameObject);
		}
		meshes.Clear ();
		var points = getPoints (path2);
		var northLists = new List<List<Vector3>> ();
		var southLists = new List<List<Vector3>> ();
		bool fromNorth = points [0].z > 0 ? true : false;
		if (fromNorth) {
			northLists.Add (new List<Vector3> ());
		} else {
			southLists.Add (new List<Vector3> ());
		}
		List<int> switchedBefore = new List<int> ();
		for (int i = 0; i < points.Count; i++) {
			var vector = points [i];
			if (vector.z > 0) {
				if (!fromNorth) {
					switchedBefore.Add (i);
					fromNorth = true;
					northLists.Add (new List<Vector3> ());
				}
				northLists [northLists.Count - 1].Add (vector);
			} else {
				if (fromNorth) {
					switchedBefore.Add (i);
					fromNorth = false;
					southLists.Add (new List<Vector3> ());
				}
				southLists [southLists.Count - 1].Add (vector);
			}
		}

		UnityEngine.Debug.Log ("Number Northlists: " + northLists.Count);
		UnityEngine.Debug.Log ("Switched before: ");
		foreach (var point in switchedBefore) {
			UnityEngine.Debug.Log (point);
		}
		for (int i = 0; i < switchedBefore.Count / 2; i++) {
			var cross11 = points [switchedBefore [2 * i] - 1];
			var cross12 = points [switchedBefore [2 * i]];
			var mid1 = Vector3.Lerp (cross11, cross12, 0.5f);
			mid1.z = 0;

			var cross21 = points [switchedBefore [2 * i + 1] - 1];
			var cross22 = points [switchedBefore [2 * i + 1]];
			var mid2 = Vector3.Lerp (cross21, cross22, 0.5f);
			mid2.z = 0;
		}
		foreach (var pointsNorth in northLists) {
			var verticesNorth = ToVertex (pointsNorth);
			if (verticesNorth.Count > 0) {
				var polygonNorth = new Polygon ();
				UnityEngine.Debug.Log ("Number of vertices north: " + verticesNorth.Count);
				var contourNorth = new Contour (verticesNorth);

				Point point = null;
				for (int i = 0; i < staticPositions.Count; i++) {
					if (IsPointInPolygon (staticPositions [i], verticesNorth)) {
						point = new Point (staticPositions [i].x, staticPositions [i].y);
						UnityEngine.Debug.Log ("Is inside");
						break;
					} else {
						if (IsPointInPolygon (constpoint, verticesNorth)) {
							UnityEngine.Debug.Log (constpoint + " Is inside");
						}
					}
				}
				if (point != null) {
					polygonNorth.Add (contourNorth, point);
				} else {
					polygonNorth.Add (contourNorth);
				}

				List<int> trianglesNorth;
				List<Vector3> meshVerticesNorth;
				SetMesh (polygonNorth, out meshVerticesNorth, out trianglesNorth);
				var meshNorth = AddMesh ();
				meshNorth.vertices = meshVerticesNorth.ToArray ();
				meshNorth.triangles = trianglesNorth.ToArray ();

				Vector3[] normals = meshNorth.normals;
				for (int i = 0; i < normals.Length; i++)
					normals [i] = -normals [i];
				meshNorth.normals = normals;
				for (int m = 0; m < meshNorth.subMeshCount; m++) {
					int[] triangles2 = meshNorth.GetTriangles (m);
					for (int i = 0; i < triangles2.Length; i += 3) {
						int temp = triangles2 [i + 0];
						triangles2 [i + 0] = triangles2 [i + 1];
						triangles2 [i + 1] = temp;
					}
					meshNorth.SetTriangles (triangles2, m);
				}
			}
		}
		UnityEngine.Debug.Log ("Number Southlists: " + southLists.Count);
		foreach (var pointsSouth in southLists) {
			var verticesSouth = ToVertex (pointsSouth);
			if (verticesSouth.Count > 0) {
				var polygonSouth = new Polygon ();
				UnityEngine.Debug.Log ("Number of vertices south: " + verticesSouth.Count);
				var contourSouth = new Contour (verticesSouth);

				Point point = null;
				for (int i = 0; i < staticPositions.Count; i++) {
					if (IsPointInPolygon (staticPositions [i], verticesSouth)) {
						point = new Point (staticPositions [i].x, staticPositions [i].y);
						UnityEngine.Debug.Log ("Is inside");
						break;
					} else {
						if (IsPointInPolygon (constpoint, verticesSouth)) {
							UnityEngine.Debug.Log (constpoint + " Is inside");
						}
					}
				}
				if (point != null) {
					polygonSouth.Add (contourSouth, point);
				} else {
					polygonSouth.Add (contourSouth);
				}

				List<int> trianglesSouth;
				List<Vector3> meshVerticesSouth;
				SetMesh (polygonSouth, out meshVerticesSouth, out trianglesSouth);
				var meshSouth = AddMesh ();
				meshSouth.vertices = meshVerticesSouth.ToArray ();
				meshSouth.triangles = trianglesSouth.ToArray ();
			}
		}
	}

	void setTorusMesh (List<Vector3> staticPositions, Path path2)
	{
		
	}

	void SetMesh (Polygon polygon, out List<Vector3> vertices, out List<int> triangles)
	{
		var options = new ConstraintOptions () {

		};
		var quality = new QualityOptions () {
//			MinimumAngle = 25,
//			MaximumArea = 0.01d
		};
		// Triangulate the polygon
		var polyMesh = polygon.Triangulate (options, quality);
		var polyVertices = polyMesh.Vertices;
		var polyTriangles = polyMesh.Triangles;
		vertices = new List<Vector3> ();
		triangles = getTriangles (polyTriangles, vertices);
	}

	List<Vector3> getPoints (Path path2)
	{
		var points = new List<Vector3> ();
		List<Vertex> vertices = new List<Vertex> ();
		for (int i = 0; i < path2.line.positionCount; i++) {
			var point = path2.line.GetPosition (i);
			points.Add (point);
		}
		for (int i = path1.line.positionCount - 2; i > 0; i--) {
			var point = path1.line.GetPosition (i);
			points.Add (point);
		}
		points = DouglasPeucker.DouglasPeuckerReduction (points, 0.01d);
		return points;
	}

	List<int> getTriangles (ICollection<Triangle> polyTriangles, List<Vector3> vertices)
	{
		List<int> triangles = new List<int> ();
		var triEnum = polyTriangles.GetEnumerator ();
		while (triEnum.MoveNext ()) {
			Triangle tri = triEnum.Current;
			triangles.Add (vertices.Count);
			triangles.Add (vertices.Count + 1);
			triangles.Add (vertices.Count + 2);

			var vector2 = ToVector3 (tri.GetVertex (2));
			var vector1 = ToVector3 (tri.GetVertex (1));
			var vector0 = ToVector3 (tri.GetVertex (0));

			vertices.Add (vector2);
			vertices.Add (vector1);
			vertices.Add (vector0);
		}
		return triangles;
	}

	private static Vector3 ToVector3 (Vertex v)
	{
		var vector3 = new Vector3 ((float)v.X, (float)v.Y, 0);
		if (Statics.isSphere) {
			if (v.X >= spacer / 2) {
				float x = (float)v.X - spacer;
				float y = (float)v.Y;
				var stereo = new Vector3 (2 * Statics.sphereRadius * x, 2 * Statics.sphereRadius * y, -Statics.sphereRadius * Statics.sphereRadius + x * x + y * y);
				stereo = stereo / (Statics.sphereRadius * Statics.sphereRadius + x * x + y * y);
				return stereo * (Statics.sphereRadius + Statics.meshSpacer);
			} else {
				float x = (float)v.X + spacer;
				float y = (float)v.Y;
				var stereo = new Vector3 (2 * Statics.sphereRadius * x, 2 * Statics.sphereRadius * y, Statics.sphereRadius * Statics.sphereRadius - x * x - y * y);
				stereo = stereo / (Statics.sphereRadius * Statics.sphereRadius + x * x + y * y);
				return stereo * (Statics.sphereRadius + Statics.meshSpacer);
			}
		}
		return vector3;
	}

	private static Vertex ToVertex (Vector3 point)
	{
		var vertex = new Vertex (point.x, point.y);
		if (Statics.isSphere) {
			if (point.z < 0) {
				var x = point.x;
				var y = point.y;
				var z = point.z;

				var newX = (Statics.sphereRadius * x) / (Statics.sphereRadius - z);
				var newY = (Statics.sphereRadius * y) / (Statics.sphereRadius - z);
				return new Vertex (newX + spacer, newY);
			} else {
				var x = point.x;
				var y = point.y;
				var z = -point.z;

				var newX = (Statics.sphereRadius * x) / (Statics.sphereRadius - z);
				var newY = (Statics.sphereRadius * y) / (Statics.sphereRadius - z);
				return new Vertex (newX - spacer, newY);
			}
		}
		return vertex;
	}

	private static List<Vertex> ToVertex (List<Vector3> points)
	{
		var vertices = new List<Vertex> (points.Count);
		foreach (var point in points) {
			vertices.Add (ToVertex (point));
		}
		return vertices;
	}

	public static bool IsPointInPolygon (Vector3 point, List<Vertex> poly)
	{
		bool oddNodes = false;

		double x = point.x;
		double y = point.y;

		int count = poly.Count;

		for (int i = 0, j = count - 1; i < count; i++) {
			if (((poly [i].Y < y && poly [j].Y >= y) || (poly [j].Y < y && poly [i].Y >= y))
			    && (poly [i].X <= x || poly [j].X <= x)) {
				oddNodes ^= (poly [i].X + (y - poly [i].Y) / (poly [j].Y - poly [i].Y) * (poly [j].X - poly [i].X) < x);
			}
			j = i;
		}

		return oddNodes;
	}
}
