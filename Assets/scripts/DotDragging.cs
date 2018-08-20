using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DotDragging : MonoBehaviour
{

	// Class Variables
	public float frequency = 1000000;
	private TargetJoint2D targetJoint = null;
	public float pathResolution;
	public List<GameObject> dots = new List<GameObject> ();
	private int dot1;
	private GameObject dotMove;
	private int dot2;

	public List<LinkedList<Vector2>> paths = new List<LinkedList<Vector2>> ();
	private List<LineRenderer> lines = new List<LineRenderer> ();
	//	private Dictionary<int, Vector3> colliderPos = new Dictionary<int, Vector3> ();
	private List<bool> isCollided = new List<bool> ();
	private List<Dictionary<int, Vector2>> colliderNormals = new List<Dictionary<int, Vector2>> ();
	public GameObject node;
	public Material mat;
	private Color trailColor;
	private List<Color> myColors = new List<Color> ();
	private string[] myHexColors = {
		"#E57373",//Red
		"#F06292",//Pink
		"#BA68C8",//Purple
		"#9575CD",//Deep Purple
		"#7986CB",//Indigo
		"#64B5F6",//Blue
		"#4FC3F7",//Light Blue
		"#4DD0E1",//Cyan
		"#4DB6AC",//Teal
		"#81C784",//Green
		"#AED581",//Light Green
		"#DCE775",//lime
		"#FF8A65",//Deep Orange
		"#A1887F",//Brown
	};

	LineRenderer newLine;
	const float lineThickness = 0.05f;
	const float minDist = lineThickness / 5f;


	void Start ()
	{
		setColors ();
		for (int i = 0; i < dots.Count; i++) {
			dots [i].GetComponent<SpriteRenderer> ().color = GetRandomColor ();
		}
		//CreateRope ();
	}

	// Update
	void Update ()
	{
		int tapCount = Input.touchCount;
		if (tapCount == 1) {// && !EventSystem.current.IsPointerOverGameObject (Input.GetTouch (0).fingerId)
			Touch touch1 = Input.GetTouch (0);
			Vector2 touchOrigin = touch1.position;
			Vector2 worldPoint = Camera.main.ScreenToWorldPoint (touchOrigin);
			if (touch1.phase == TouchPhase.Began) {
				RaycastHit2D hit = Physics2D.Raycast (worldPoint, Vector2.zero);
				if (hit.collider != null) {
					dot1 = dots.IndexOf (hit.collider.gameObject);
					if (dot1 != -1) {
						paths.Add (new LinkedList<Vector2> ());
						paths [paths.Count - 1].AddFirst (hit.collider.gameObject.transform.position);
						Debug.Log ("Instantiate");
						dotMove = Instantiate (dots [dot1]);
						Debug.Log ("Set Color");
						dotMove.GetComponent<Collider2D> ().isTrigger = false;
						TrailRenderer trail = dotMove.GetComponentInChildren<TrailRenderer> ();
						trail.material = mat;
						trailColor = GetRandomColor ();
						trail.material.SetColor ("_Color", trailColor);
						if (!targetJoint) {
							Debug.Log ("add2");
							targetJoint = dotMove.AddComponent<TargetJoint2D> () as TargetJoint2D;
						}

						targetJoint.transform.position = hit.point;

						targetJoint.dampingRatio = 1f;// there is no damper in SpringJoint2D but there is a dampingRatio
						targetJoint.maxForce = 1000f;
						targetJoint.frequency = frequency;
						targetJoint.autoConfigureTarget = false;
					} else {
						dotMove = null;
					}
				}
			} else if (touch1.phase == TouchPhase.Moved) {
				Vector3 touchPos = Camera.main.ScreenToWorldPoint (new Vector3 (touch1.position.x, touch1.position.y, 0));
				touchPos.z = 0;
				var target = new Vector2 (touchPos.x, touchPos.y);
				if (dotMove != null) {
					targetJoint.target = target;
					var path = paths [paths.Count - 1];
					if (Vector2.Distance (path.Last.Value, target) > pathResolution) {
						path.AddLast (target);
					}
				}
				//dotMove.transform.position = touchPos;
			} else if (touch1.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Canceled) {
				if (dotMove != null) {
					if (dotMove.GetComponent<dotBehaviour> ().IsTriggered ()) {
						var dotBehaviourDotMove = dotMove.GetComponent<dotBehaviour> ();
						dot2 = dots.IndexOf (dotBehaviourDotMove.GetTriggerObject ());
						Debug.Log ("Triggered");
						if (dot2 != dot1) {
							dotMove.transform.position = dotBehaviourDotMove.GetTríggerPosition ();
							var path = paths [paths.Count - 1];
							path.AddLast (dotMove.transform.position);
							//dotMove.transform.DetachChildren ();

							//Set Colors
							var color = dots [dot1].GetComponent<SpriteRenderer> ().color;
							dots [dot2].GetComponent<SpriteRenderer> ().color = dots [dot1].GetComponent<SpriteRenderer> ().color;

							DrawPath (paths.Count - 1);
						} else {
							paths.RemoveAt (paths.Count - 1);
						}
					}
					Destroy (dotMove);
				} 
			}
		}
	}

	void setColors ()
	{
		foreach (var hex in myHexColors) {
			Color color;
			ColorUtility.TryParseHtmlString (hex, out color);
			myColors.Add (color);
		}
	}

	Color GetRandomColor ()
	{
		var rand = Random.Range (0, myColors.Count);
		var color = myColors [rand];
		myColors.RemoveAt (rand);
		return color;
	}

	// end of update

	//					GameObject circle = GameObject.CreatePrimitive (PrimitiveType.Sphere);
	//					circle.transform.localScale = Vector3.one * 0.05f;
	//					circle.transform.position = goalPoint;
	void smoothLine (int pathNum)
	{
		var line = lines [pathNum];
		if (line.positionCount > 3) {
			var colliderNormal = colliderNormals [pathNum];
			for (int i = 0; i < line.positionCount - 2; i++) {
				bool isOnLeftEdge = colliderNormal.ContainsKey (i) && !colliderNormal.ContainsKey (i + 2);
				bool isOnRightEdge = colliderNormal.ContainsKey (i + 2) && !colliderNormal.ContainsKey (i);
				if (!isOnLeftEdge && !isOnRightEdge) {
					var p1 = line.GetPosition (i);
					var p2 = line.GetPosition (i + 2);
					var midPoint = Vector3.Lerp (p1, p2, 0.5f);
					RaycastHit2D hit = Physics2D.CircleCast (p1, lineThickness * 0.5f, p2 - p1, Vector2.Distance (p1, p2));
					if (hit.collider != null && hit.collider.isTrigger == false) {
						//Nothing
					} else {
						Debug.DrawLine (p1, p2, Color.red, 2);
						line.SetPosition (i + 1, midPoint);
					}
				}
			}
		}
	}

	void RefinePath (int i)
	{
		float meanDist = 0.01f;
		var path = paths [i];
		Debug.Log ("Refining Path " + i + ", length before: " + path.Count);
		Debug.Log ("Path count = " + path.Count);
		if (path.Count > 0) {
			var node = path.First;
			var nextStepNode = node.Next;
			while (node.Next != null) {
				var point1 = node.Value;
				var point2 = node.Next.Value;
				var distance = Vector2.Distance (point1, point2);
				if (distance > meanDist) {
					var vector2 = Vector2.Lerp (point1, point2, 0.5f);
					path.AddAfter (node, vector2);
				} else if (distance < meanDist / 2.5f) {
					path.Remove (node.Next);
				} else {
					node = node.Next;
				}
			}
		}
		var line = lines [i];
		line.positionCount = path.Count;
		var node2 = path.First;
		for (int j = 0; j < path.Count; j++) {
			line.SetPosition (j, node2.Value);
			node2 = node2.Next;
		}
		Debug.Log ("Refined Path " + i + ", length after: " + path.Count);
	}

	public float SamplePathLength (int pathNum)
	{
		var list = paths [pathNum];
		float sum = 0f;
		var node = list.First;
		var count = (list.Count - 1);
		var numSamples = Mathf.Max (count / 5, 30);
		int k = Mathf.Min (count, numSamples);
		int step = count / k;
		for (int i = 0; i < k; i++) {
			sum += Vector2.Distance (node.Value, node.Next.Value);
			for (int j = 0; j < step; j++) {
				node = node.Next;
			}
		}
		if (sum == 0) {
			return 1f;
		}
		sum = sum / (float)k;
		Debug.Log ("Sampled Length is " + sum);
		return sum;
	}

	void DrawPath (int pathNum)
	{
		isCollided.Add (false);
		colliderNormals.Add (new Dictionary<int, Vector2> ());
		var path = paths [pathNum];
		var line = new GameObject ("Path " + pathNum).AddComponent<LineRenderer> ();
		line.GetComponent<Renderer> ().material = mat;
		line.GetComponent<Renderer> ().material.SetColor ("_Color", trailColor);
		lines.Add (line);
		RefinePath (pathNum);
		line.startWidth = lineThickness;
		line.endWidth = lineThickness;
	}

}

