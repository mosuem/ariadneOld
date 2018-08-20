using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
	private Material mat;

	public Color color;

	public int pathNumber;
	public Line line;
	//	public LinkedList<Vector3> points;
	public Dictionary<int, Vector3> colliderNormals;
	public GameObject dotFrom;
	public GameObject dotTo;
	public float depth;
	private GameObject arrowPrefab;
	public List<GameObject> arrows = new List<GameObject> ();
	public List<int> canonicalPath;

	public int sortingOrder { set { line.sortingOrder = value; } get { return line.sortingOrder; } }

	public int Count { get { return line.positionCount; } }

	public Path (Line pLine, LinkedList<Vector3> pPoints)
	{
		line = pLine;
	}

	public Path (int pPathNumber, Material pPathMaterial, Color color, string name, GameObject dot1, GameObject dot2, float height, GameObject arrowPrefab)
	{
		this.arrowPrefab = arrowPrefab;
		this.color = color;
		pathNumber = pPathNumber;
		mat = pPathMaterial;
		dotFrom = dot1;
		dotTo = dot2;
		line = new Line (new GameObject (name));
		line.SetMaterial (pPathMaterial);
		line.sortingOrder = Statics.LineSortingOrder;
		SetColor (color);

		line.width = Statics.lineThickness;
		var depthVector = Statics.lineDepth + Vector3.back * height;
		depth = depthVector.z;
		colliderNormals = new Dictionary<int, Vector3> ();
//
//		var outlineObject = new GameObject ("Outline to Path " + pathNumber);
//		outlineObject.SetActive (false);
//		outline = outlineObject.AddComponent<Line> ();
//		outlineObject.transform.position = Statics.lineDepth * 0.9f;
//
//		outline.startWidth = Statics.lineThickness * 2f;
//		outline.endWidth = Statics.lineThickness * 2f;
//		outline.GetComponent<Line> ().material = mat;
//		outline.GetComponent<Line> ().material.SetColor ("_Color", Statics.outlineColor);
	}

	public void Clear ()
	{
		GameObject.Destroy (line.gameObject);
	}

	public void SetColor (Color c)
	{
		line.SetColor (c);
		foreach (var arrow in arrows) {
			arrow.GetComponent <SpriteRenderer> ().color = c;
		}
	}

	public void smoothLine ()
	{
		Debug.Log ("Start SmoothLine");
		if (line.positionCount > 3) {
			for (int i = 0; i < line.positionCount - 2; i++) {
				bool isOnLeftEdge = colliderNormals.ContainsKey (i) && !colliderNormals.ContainsKey (i + 2);
				bool isOnRightEdge = colliderNormals.ContainsKey (i + 2) && !colliderNormals.ContainsKey (i);
				if (!isOnLeftEdge && !isOnRightEdge) {
					var p1 = line.GetPosition (i);
					var p = line.GetPosition (i + 1);
					var p2 = line.GetPosition (i + 2);
					if (Vector3.Distance (p1, p2) * Statics.smoothFactor < Vector3.Distance (p1, p) + Vector3.Distance (p, p2)) {
						var midPoint = Vector3.Lerp (p1, p2, 0.5f);
						RaycastHit2D hit = Physics2D.CircleCast (p1, Statics.lineThickness * 0.5f, p2 - p1, Vector2.Distance (p1, p2));
						if (hit.collider != null && hit.collider.isTrigger == false) {
							//Nothing
						} else {
							line.SetPosition (i + 1, Misc.SetOnSurface (midPoint, Statics.pathSpacer));
						}
					}
				}
			}
		}
		Debug.Log ("End SmoothLine");
	}

	public List<int> RefinePath ()
	{
		return RefinePath (null);
	}

	public List<int> RefinePath (Dictionary<int, Vector3> closestPositions)
	{
		return RefinePath (closestPositions, Statics.meanDist);
	}

	public List<int> RefinePath (Dictionary<int, Vector3> closestPositions, float meanDist)
	{
		Debug.Log ("Start RefinePath");
		float minCircleCirc = 2f / Statics.meanDist;
		int forecastDepth = (int)minCircleCirc;
		Debug.Log ("Path count = " + Count + ", refine with depth " + forecastDepth);
		if (Count > 0) {
			int j = 0;
			while (j < Count - 1) {
				var point = line.GetPosition (j);
				var next = true;
				for (int i = 1; i < forecastDepth; i++) {
					if (j + i < Count - 1) {
						var nextPoint = line.GetPosition (j + i);
						var distance = Vector3.Distance (point, nextPoint);
						if (i == 1 && distance > meanDist * 1.01f) {
							var midPoint = Vector3.Lerp (point, nextPoint, Mathf.Clamp01 (meanDist / distance));
							line.InsertPositionAt (j + 1, midPoint);
							next = false;
							break;
						}
						if (distance < meanDist * 0.99f) {
							bool removed = false;
							for (int k = j + 1; k <= j + i; k++) {
								if (k < Count - 1) {
									line.Remove (k);
									removed = true;
								}
							}
							if (removed) {
								next = false;
								break;
							}
						}
					} else {
						break;
					}
				}
				if (next) {
					j++;
				}
			}
		}

		var closeNodePositions = new List<int> ();
		if (closestPositions != null) {
			foreach (var pair in closestPositions) {
				var index = pair.Key;
				if (index < line.positionCount) {
					var vector3 = line.GetPosition (index);
					for (int node = 0; node < Count; node++) {
						if (Vector2.Distance (line.GetPosition (node), vector3) < meanDist) {
							closeNodePositions.Add (node);
							break;
						}
					}
				}
			}
		}

		Debug.Log ("Refined Path " + pathNumber + ", length after: " + Count);
		return closeNodePositions;
	}

	public void Simplify (float epsilon)
	{
		Debug.Log ("Start simplify with epsilon = " + epsilon + ", number of points before: " + line.positionCount);
		var newList = DouglasPeucker.DouglasPeuckerReduction (line.GetPositions (), epsilon);
		line.SetPositions (newList);
		Debug.Log ("End simplify with epsilon = " + epsilon + ", number of points after: " + line.positionCount);
	}

	public void DrawArrows (bool active = false)
	{
		return;
		foreach (var arrow in arrows) {
			GameObject.Destroy (arrow);
		}
		arrows = new List<GameObject> ();
		var length = Length ();
		float numberArrows = Mathf.FloorToInt (length / Statics.arrowDist);
		var arrowSpacing = length / (numberArrows + 1);
		var sum = 0f;
		for (int i = 1; i < line.positionCount; i++) {
			var linePos = line.GetPosition (i);
			var lineBeforePos = line.GetPosition (i - 1);
			sum += Vector2.Distance (lineBeforePos, linePos);
			if (sum > arrowSpacing) {
				sum = 0f;
				var arrow = GameObject.Instantiate (arrowPrefab);
				arrow.SetActive (active);
				arrow.transform.localScale = Statics.lineThickness * 2f * Vector3.one;
				arrow.GetComponent<SpriteRenderer> ().color = color;
				arrow.transform.position = lineBeforePos;
				var q = Quaternion.FromToRotation (arrow.transform.right, linePos - lineBeforePos);
				arrow.transform.rotation = q * arrow.transform.rotation;
				arrow.transform.parent = line.gameObject.transform;
				arrows.Add (arrow);
			}
		}
	}

	public float Length ()
	{
		float sum = 0f;
		if (line.positionCount > 1) {
			for (int i = 0; i < line.positionCount - 1; i++) {
				sum += Vector2.Distance (line.GetPosition (i), line.GetPosition (i + 1));
			}
		}
		return sum;
	}


	public void Concatenate (Path path2)
	{
		var list1 = line.GetPositions ();
		var list2 = path2.line.GetPositions ();
		list1.AddRange (list2);
		line.SetPositions (list1);
		path2.Clear ();
		dotTo = path2.dotTo;
	}
}
