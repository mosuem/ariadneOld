using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using UnityEditor;
using System;
using System.Linq;

public class FreeDragging3D : MonoBehaviour
{
	// Class Variables
	public float frequency = 1000000;
	public float pathResolution;
	private int dot1;
	private GameObject dotMove;
	private int dot2;

	private Dictionary<Path, bool> isCollided = new Dictionary<Path,bool> ();
	private HashSet<int> angleGreaterThan90 = new HashSet<int> ();
	//private GameObject node;
	private Color trailColor;
	private TrailRenderer trail;

	Line newLine;
	private float minDist = Statics.lineThickness;
	public Material partMaterial;

	private float dotRadius = 0.3f;
	private LevelData level;

	private float touchDuration = 0f;
	private Vector3 touchStart;

	private Homotopy actHomotopy = null;

	GameObject circle = null;
	GameObject rectangle = null;

	private bool drawing = false;
	private Vector3 drawPos;
	private int draggingPath = -1;
	private int draggingPosition = -1;
	private Vector3 lastTouch = Vector3.zero;
	private float distSoFar = 0f;
	private int upperBound;
	private int lowerBound;
	private GameObject draggingObstacle;
	private Vector3 draggingOffset;

	bool isDrawingPath = false;


	//3D----------
	public GameObject manifold;
	static Vector3 farAwayVec = new Vector3 (100f, 100f, 100f);
	Vector3 touchPos = farAwayVec;

	void Start ()
	{
		Debug.Log (Statics.isSphere);
		level = Camera.main.GetComponent<LevelData> ();
		Debug.Log ("Draw Paths");
		for (int i = 0; i < level.paths.Count; i++) {
			Debug.Log ("Draw Path " + i);
			var path = level.paths [i];
			StartCoroutine (DrawPath (path, true));
		}
		Statics.isLoading = false;
		if (Statics.isSphere) {
			manifold = Misc.BuildSphere (level.manifoldMat, Color.grey);
		} else if (Statics.isTorus) {
			manifold = Misc.BuildTorus (level.manifoldMat, Color.grey);
		}
	}

	// Update
	void Update ()
	{
		int tapCount = Input.touchCount;
		if (tapCount == 1 && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (Input.GetTouch (0).fingerId)) {// && !EventSystem.current.IsPointerOverGameObject (Input.GetTouch (0).fingerId)
			Touch touch1 = Input.GetTouch (0);
			Vector3 touchPoint = Camera.main.ScreenToWorldPoint (touch1.position);
			touchPoint.z = 0;
			if (touch1.phase == TouchPhase.Began) {
				/**
				 * 
				 * 					START
				 * 
				 * 
				 * */

					touchStart = touchPoint;
					touchDuration = Time.time;
					bool isHit = false;
					RaycastHit hit3D = new RaycastHit ();
					var ray = Camera.main.ScreenPointToRay (touch1.position);
					isHit = Physics.Raycast (ray, out hit3D);
					RaycastHit2D hit = Physics2D.Raycast (touchPoint, Vector3.zero);
					 if (isHit) {
						Debug.Log ("Hit! 3d");
						if (hit3D.collider.gameObject.CompareTag ("Dot") && level.pathDrawingAllowed && !isDrawingPath) {
							DrawLine3D (hit3D);
							Statics.isDragging = true;
						} else if (hit3D.collider.gameObject.CompareTag ("Obstacle") && level.staticsAllowed) {
							var index = level.statics.IndexOf (hit.collider.gameObject);
							StartDraggingObstacle (index, touchPoint);	
							Statics.isDragging = true;					
						} else if (level.homotopiesAllowed) {
							int pathNumber = -1;
							bool isOnPath = IsOnPath (hit3D.point, ref pathNumber, actHomotopy != null);
							if (isOnPath) {
								Debug.Log ("Started Dragging Path");
								setPathToDrag (hit3D.point, pathNumber);
								Statics.isDragging = true;
								touchPos = farAwayVec;
							} else {
								draggingPath = -1;
								Debug.Log ("Tried Dragging, but no path there");
							}
						}
				
				}
			} else if (touch1.phase == TouchPhase.Moved) {
				/**
				 * 
				 * 					MOVE
				 * 
				 * 
				 * */
				if (Statics.drawCircle) {
					circle.transform.localScale = Vector3.one * Vector3.Magnitude (circle.transform.position - touchPoint);
				} else if (Statics.drawRectangle) {
					var xFactor = drawPos.x - touchPoint.x;
					var yFactor = drawPos.y - touchPoint.y;
					rectangle.transform.localScale = new Vector3 (xFactor, yFactor, 0);
					rectangle.transform.position = drawPos - new Vector3 (xFactor / 2f, yFactor / 2f, 0);
				} else if (draggingObstacle != null) {
					DragObstacle (touchPoint);
				} else {
					Ray ray = Camera.main.ScreenPointToRay (touch1.position);
					RaycastHit hit = new RaycastHit ();
					if (Physics.Raycast (ray, out hit)) {
						lastTouch = touchPos;
						touchPos = Misc.SetOnSurface (hit.point, Statics.dotSpacer);
						if (dotMove != null) {
							Statics.isDragging = true;
							var direction = touchPos - dotMove.transform.position;
							moveDot (direction);
						} else if (draggingPath != -1 && lastTouch != farAwayVec) {
							DragPath (touchPos, lastTouch);
						}
					}
				}
			} else if (touch1.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Canceled) {
				/**
				 * 
				 * 					END
				 * 
				 * 
				 * */
				if (drawing) {
					Debug.Log ("Drawing ended");
					drawing = false;
					Statics.drawCircle = false;
					Statics.drawRectangle = false;
				}
				touchDuration = Time.time - touchDuration;
				var touchDistance = Vector2.Distance (touchStart, touchPoint);
				Debug.Log ("Touch Duration = " + touchDuration + ", touch distance = " + touchDistance);
				if (touchDuration < 0.2f) {
					int pathNum = -1;
					bool isOnPath = IsOnPath (touchPoint, ref pathNum);
					if (level.dotSettingAllowed && !isOnPath) {
						DrawDot (touchPoint);
					}
				}
				if (dotMove != null) {
					Debug.Log ("Finish Line");
					dotMove.GetComponent<Rigidbody> ().velocity = Vector3.zero;
					FinishLine ();
				} else if (draggingObstacle != null) {
					Debug.Log ("Finish dragging gameObject");
					draggingObstacle = null;
					Statics.isDragging = false;
				} 

				if (draggingPath != -1) {
					Debug.Log ("Finish dragging Path");
					//Guess path which is trying do be reached, and snuggle this path against the other
					EndDraggingPath ();
					Statics.isDragging = false;
				}
				touchDuration = 0f;
			}
		} else if (tapCount == 2 && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (Input.GetTouch (0).fingerId) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (Input.GetTouch (1).fingerId)) {
			Touch touch1 = Input.GetTouch (0);
			Touch touch2 = Input.GetTouch (1);
			if (touch2.phase == TouchPhase.Began) {
				bool isHit1 = false;
				RaycastHit hit3D1 = new RaycastHit ();
				isHit1 = Physics.Raycast (Camera.main.ScreenPointToRay (touch1.position), out hit3D1);

				bool isHit2 = false;
				RaycastHit hit3D2 = new RaycastHit ();
				isHit2 = Physics.Raycast (Camera.main.ScreenPointToRay (touch2.position), out hit3D2);

				if (isHit1 && isHit2) {
					int pathNumber1 = -1;
					int pathNumber2 = -1;
					bool isOnPath1 = IsOnPath (hit3D1.point, ref pathNumber1);
					bool isOnPath2 = IsOnPath (hit3D2.point, ref pathNumber2);
					if (isOnPath1 && isOnPath2 && pathNumber1 != pathNumber2) {
						Debug.Log ("Try to concatenate Path " + pathNumber1 + " and Path " + pathNumber2);
						var path1 = level.paths [pathNumber1];
						var path2 = level.paths [pathNumber2];
						if (path1.dotTo.Equals (path2.dotFrom)) {
							actHomotopy.Clear ();
							actHomotopy = null;
							draggingPath = -1;
							Debug.Log ("Concatenate!");
							path1.Concatenate (path2);
							level.paths.Remove (path2);
						} else {
							Debug.Log ("Not concatenable");
						}
					}
				}
			}
		}
	}

	void FixedUpdate ()
	{
		if (Statics.firstPerson && dotMove != null && Input.touchCount == 0) {
			var moveHorizontal = Input.GetAxis ("Horizontal");
			var moveVertical = Input.GetAxis ("Vertical");
			Vector3 up;
			up = dotMove.transform.up;
//			if (dotMove.transform.rotation.x < 90) {
//			} else {
//				up = -dotMove.transform.up;
//			}
			var direction = up * moveVertical + dotMove.transform.right * moveHorizontal;
			moveDot (direction);
		}
	}

	void moveDot (Vector3 direction)
	{
		float distance = direction.magnitude;
		direction.Normalize ();
		var dotPos = dotMove.transform.position;
		var body = dotMove.GetComponent<Rigidbody> ();
		var distToSurface = (dotPos - Misc.SetOnSurface (dotPos, 0f)).magnitude - Statics.dotSpacer;
//		Debug.Log ("Distance to surface is " + distToSurface);
		var normal = (Misc.ManifoldCenter (dotPos) - dotPos).normalized;
		Debug.DrawRay (dotPos, normal, Color.red, 100000f);
		//damping
		var damping = 0.8f;
		float dampingValue = 1 - (1 / (distance * damping + 1));
		body.velocity += direction * distance;
		body.velocity += Mathf.SmoothStep (0, 10, distToSurface) * normal;
		body.velocity *= dampingValue * Statics.dotMoveSpeed;
	}

	void DragPath (Vector3 touchPosition, Vector3 touchBefore)
	{
		Debug.Log ("Start DragPath(" + touchPosition + ", " + touchBefore + ")");
		bool dragSnuggledPositions;
//		Debug.Log ("Homotopy has " + actHomotopy.closeNodePositions.Count + " snuggled Positions");
		if (actHomotopy.snugglingNodePositions.Contains (draggingPosition)) {
			dragSnuggledPositions = true;
		} else {
			dragSnuggledPositions = false;
		}

		var path = actHomotopy.midPath;
		var line = path.line;
		float t0 = (float)draggingPosition / path.Count;
		float distance;
		if (Statics.isSphere) {
			var dist = Vector3.Distance (touchPosition, touchBefore);
			var angle = Mathf.Asin (dist / Statics.sphereRadius);
			distance = angle * Statics.sphereRadius;
		} else if (Statics.isTorus) {
			distance = Vector3.Distance (touchPosition, touchBefore);
		} else {
			distance = Vector2.Distance (touchPosition, touchBefore);
		}
		Debug.Log ("Distance is " + distance);
		var lowerLimit = Statics.lineThickness / 2f;
		var maxUpperLimit = Mathf.Min (t0, 1f - t0);
		var t = Mathf.Clamp01 (distSoFar / 3f);
		float firstLowerSnugglePosition = maxUpperLimit;
		float firstUpperSnugglePosition = maxUpperLimit;

		if (!dragSnuggledPositions) {
			for (int i = draggingPosition; i < line.positionCount; i++) {
				if (i < upperBound) {
					if (actHomotopy.snugglingNodePositions.Contains (i)) {
						firstUpperSnugglePosition = (float)i / line.positionCount - t0;
						break;
					}
				} else {
					break;
				}
			}
			for (int i = draggingPosition - 1; i > -1; i--) {
				if (i > lowerBound) {
					if (actHomotopy.snugglingNodePositions.Contains (i)) {
						firstLowerSnugglePosition = t0 - (float)i / line.positionCount;
						break;
					}
				} else {
					break;
				}
			}
		}
		float lowerB = Mathf.SmoothStep (lowerLimit, Mathf.Min (maxUpperLimit, firstLowerSnugglePosition), t);
		float upperB = Mathf.SmoothStep (lowerLimit, Mathf.Min (maxUpperLimit, firstUpperSnugglePosition), t);
		Vector3 normal = (touchPosition - touchBefore).normalized;
		distSoFar += distance;

		for (int i = draggingPosition; i < line.positionCount; i++) {
			if (i < upperBound) {
				if (!setPoint (i, path, normal, distance, lowerB, upperB, t0, true)) {
					break;
				}
			} else {
				break;
			}
		}
		for (int i = draggingPosition - 1; i > -1; i--) {
			if (i > lowerBound) {
				if (!setPoint (i, path, normal, distance, lowerB, upperB, t0, false)) {
					break;
				}
			} else {
				break;
			}
		}
		if (Statics.mesh) {
			actHomotopy.setMesh (level.GetStaticPositions ());
		} else {
			actHomotopy.AddCurveToBundle (level.pathFactory, draggingPosition);
		}
		Debug.Log ("End DragPath(" + touchPosition + ", " + touchBefore + ")");
	}

	void StartDraggingObstacle (int indexOfStatic, Vector3 touchPoint)
	{
		if (indexOfStatic != -1) {
			draggingObstacle = level.statics [indexOfStatic];
			draggingOffset = draggingObstacle.transform.position - touchPoint;
		}
	}

	void DragObstacle (Vector3 touchPoint)
	{
		draggingObstacle.transform.position = touchPoint + draggingOffset;
	}

	bool setPoint (int i, Path path, Vector3 normal, float distance, float lowerB, float upperB, float t0, bool up)
	{
		var line = path.line;
		var maxAngle = 90f;
		var smoothAngle = 120f;
		var k = smoothAngle / (smoothAngle - maxAngle);
		var draggingPoint = line.GetPosition (draggingPosition);
		Vector3 linePoint = line.GetPosition (i);
		var direction = linePoint - draggingPoint;
		direction.Normalize ();
		var f = GetFactor (i, t0, lowerB, upperB);

		var factor = distance * f;

		var goalPoint = linePoint + factor * normal;

		if (Statics.isSphere) {
			goalPoint.Normalize ();
			goalPoint = Misc.SetOnSurface (goalPoint, Statics.pathSpacer);
		} else if (Statics.isTorus) {
			goalPoint = Misc.SetOnSurface (goalPoint, Statics.pathSpacer);
		}
//
//		RaycastHit2D hit = Physics2D.CircleCast (linePoint, Statics.midLineThickness * 0.5f, normal, factor * 1.1f);
//		if (hit.collider != null && hit.collider.isTrigger == false) {
//			var staticNormal = Misc.GetNormal (hit.collider, hit.point);
//			float staticAngle = Vector3.Angle (staticNormal, direction);
//			if (staticAngle < 90) {
//				//pull
//				if (up) {
//					upperBound = i;
//				} else {
//					lowerBound = i;
//				}
//			}
//			return false;
//		}
		if (up) {
			if (upperBound == i + 1 && upperBound < line.positionCount - 1) {
				var linePointNext = line.GetPosition (i + 1);
				if (Vector2.Distance (linePoint, linePointNext) > Statics.meanDist) {
					var middlePos = Vector3.Lerp (linePoint, linePointNext, 0.5f);
					line.InsertPositionAt (i + 1, middlePos);
					upperBound = i + 2;
//					var node = path.points.First;
//					var counter = 0;
//					while (counter < i) {
//						node = node.Next;
//						counter++;
//					}
//					path.points.AddAfter (node, middlePos);
				}
			}
		} else {
			if (lowerBound == i - 1 && lowerBound > 0) {
				var linePointNext = line.GetPosition (i - 1);
				if (Vector2.Distance (linePoint, linePointNext) > Statics.meanDist) {
					var middlePos = Vector3.Lerp (linePoint, linePointNext, 0.5f);
					line.InsertPositionAt (i, middlePos);
					lowerBound = i - 2;
//					var node = path.points.First;
//					var counter = 0;
//					while (counter < i) {
//						node = node.Next;
//						counter++;
//					}
//					path.points.AddBefore (node, middlePos);
				}
				draggingPosition++;
			}
		}
		line.SetPosition (i, goalPoint);
		return true;
	}

	float GetFactor (int i, float t0, float lowerB, float upperB)
	{
		Path path = actHomotopy.midPath;
		var count = path.line.positionCount;
		float t = (float)i / count;
		if (i == 0 || i == count - 1) {
			return 0;
		}
		float f;
		if (t < t0) {
			f = Mathf.SmoothStep (0f, 1f, (t - (t0 - lowerB)) / lowerB);
		} else {
			f = Mathf.SmoothStep (0f, 1f, -(t - (t0 + upperB)) / upperB);
		}
		return f;
	}

	void EndDraggingPath ()
	{
		Debug.Log ("End Dragging Path");
		Path path = actHomotopy.midPath;
		Debug.Log ("Refine");
		path.RefinePath ();
		Debug.Log ("Smoothline");
		path.smoothLine ();
		Debug.Log ("Start check if snuggling");
		StartCoroutine (CheckIfPathsAreSnuggling ());
		Debug.Log ("End check if snuggling, start snuggletopath");
	}

	IEnumerator CheckIfPathsAreSnuggling ()
	{
		var midLine = actHomotopy.midPath.line;
		Debug.Log ("Check if snuggling");
		var closestPositions = actHomotopy.closestPositions;
		closestPositions.Clear ();
		for (int i = draggingPosition; i < midLine.positionCount; i++) {
			var pos = midLine.GetPosition (i);
			float closest = 9999f;
			foreach (var path in level.paths) {
				if (!actHomotopy.path1.Equals (path)) {
					var otherLine = path.line;
					for (int k = 0; k < otherLine.positionCount; k++) {
						Vector3 pos2 = otherLine.GetPosition (k);
						var dist = Vector2.Distance (pos, pos2);
						if (dist < closest) {
							closestPositions [i] = pos2;
							closest = dist;
						}
					}
				}
			}
			if (closest < 9999f && closest < minDist) {
			} else {
				closestPositions.Remove (i);
				break;
			}
		}

		for (int i = draggingPosition - 1; i >= 0; i--) {
			var pos = midLine.GetPosition (i);
			float closest = 9999f;
			foreach (var path in level.paths) {
				if (!actHomotopy.path1.Equals (path)) {
					var otherLine = path.line;
					for (int k = 0; k < otherLine.positionCount; k++) {
						Vector3 pos2 = otherLine.GetPosition (k);
						var dist = Vector2.Distance (pos, pos2);
						if (dist < closest) {
							closestPositions [i] = pos2;
							closest = dist;
						}
					}
				}
			}
			if (closest < 9999f && closest < minDist) {
			} else {
				closestPositions.Remove (i);
				break;
			}
		}
		Debug.Log ("End check if snuggling, start snuggletopath");
		StartCoroutine (SnuggleToPath ());
		yield break;
	}

	IEnumerator SnuggleToPath ()
	{
		Debug.Log ("Snuggle To Path");
		Dictionary<int, Vector3> goalPoints = new Dictionary<int, Vector3> ();
		Dictionary<int, Vector3> startPoints = new Dictionary<int, Vector3> ();
		var closestPositions = actHomotopy.closestPositions;
		var path = actHomotopy.midPath;
		foreach (var pair in closestPositions) {
			var position = pair.Key;
			var closestPosition = pair.Value;
			float range = 30f;
			float counter = 1f;
			for (int i = 1; i < range; i++) {
				var index = position - i;
				if (index > -1) {
					var backwardsPosition = path.line.GetPosition (index);
					if (closestPositions.ContainsKey (index)) {
						break;
					}
					var factor = Mathf.SmoothStep (1, 0, Mathf.Clamp01 (counter / range));
					var direction = (closestPosition - backwardsPosition);
					var goalPoint = backwardsPosition + factor * direction;
					goalPoints [index] = goalPoint;
					counter++;
				}
			}
			counter = 1f;
			for (int i = 1; i < range; i++) {
				var index = position + i;
				if (index < path.line.positionCount) {
					var backwardsPosition = path.line.GetPosition (index);
					if (closestPositions.ContainsKey (index)) {
						break;
					}
					var factor = Mathf.SmoothStep (1, 0, Mathf.Clamp01 (counter / range));
					var direction = (closestPosition - backwardsPosition);
					var goalPoint = backwardsPosition + factor * direction;
					goalPoints [index] = goalPoint;
					counter++;
				}
			}
			goalPoints.Add (position, closestPosition);
		}
		foreach (var entry in goalPoints) {
			startPoints.Add (entry.Key, path.line.GetPosition (entry.Key));
		}
		float duration = 1;
		bool localMorph = true;
		var time = 0f;
		while (localMorph) {
			localMorph = false;
			time += Time.deltaTime;
			var t = Mathf.Clamp01 (time / duration);

			foreach (var entry in goalPoints) {
				var index = entry.Key;
				var goalPoint = entry.Value;
				var startPoint = startPoints [index];
				var goalPointStep = Vector3.Lerp (startPoint, goalPoint, t);

				var distance = Vector2.Distance (goalPointStep, goalPoint);

				path.line.SetPosition (index, goalPointStep);
				if (t < 0.99f) {
					localMorph = true;
				}
			}
			if (Statics.mesh) {
				actHomotopy.setMesh (level.GetStaticPositions ());
			} else {
				actHomotopy.AddCurveToBundle (level.pathFactory, draggingPosition);
			}
			yield return null;
		}
		Debug.Log ("Done Snuggle To Path, start refinePath");
		actHomotopy.snugglingNodePositions = path.RefinePath (actHomotopy.closestPositions);
		if (Statics.mesh) {
			actHomotopy.setMesh (level.GetStaticPositions ());
		} else {
			actHomotopy.AddCurveToBundle (level.pathFactory, draggingPosition);
		}
		upperBound = actHomotopy.midPath.Count;
		lowerBound = -1;
		distSoFar = 0f;
		draggingPath = -1;
		draggingPosition = -1;
		CheckIfHomotopyDone ();
		yield break;
	}

	void CheckIfHomotopyDone ()
	{
		Debug.Log ("Start CheckIfHomotopyDone");
		var midPath = actHomotopy.midPath;
		var dotFromPos = midPath.dotFrom.transform.position;
		var dotToPos = midPath.dotTo.transform.position;
		var circleSize = level.dotPrefab.transform.localScale.x;
		var pathHomClasses = level.pathHomClasses;
		List<int> homClass = null;
		var indexOfPath1 = level.paths.IndexOf (actHomotopy.path1);
		foreach (var item in pathHomClasses) {
			if (item.Contains (indexOfPath1)) {
				homClass = item;
			}
		}
		Debug.Log ("Homclass of this homotopy contains " + string.Join (",", homClass.Select (x => x.ToString ()).ToArray ()));
		foreach (var otherPathNum in homClass) {
			if (otherPathNum != indexOfPath1) {
				var otherPath = level.paths [otherPathNum];
				Debug.Log ("Check Path " + otherPath.pathNumber);
				bool homotopic = true;
				Debug.Log ("Check if homotopic");
				for (int i = 0; i < midPath.Count; i++) {
					var midPathPos = midPath.line.GetPosition (i);
					if (Vector2.Distance (midPathPos, dotFromPos) > circleSize && Vector2.Distance (midPathPos, dotToPos) > circleSize) {
						bool existsNearNode = false;
						for (int j = 0; j < otherPath.Count; j++) {
							var otherPathPos = otherPath.line.GetPosition (j);
							var dist = Vector2.Distance (midPathPos, otherPathPos);
							if (dist < Statics.homotopyNearness) {
								existsNearNode = true;
								break;
							}
						}
						if (!existsNearNode) {
							homotopic = false;
							break;
						}	
					}
				}
				if (homotopic) {
					for (int i = 0; i < otherPath.Count; i++) {
						var otherPathPos = otherPath.line.GetPosition (i);
						if (Vector2.Distance (otherPathPos, dotFromPos) > circleSize && Vector2.Distance (otherPathPos, dotToPos) > circleSize) {
							bool existsNearNode = false;
							for (int j = 0; j < midPath.Count; j++) {
								var midPathPos = midPath.line.GetPosition (j);
								var dist = Vector2.Distance (otherPathPos, midPathPos);
								if (dist < Statics.homotopyNearness) {
									existsNearNode = true;
									break;
								}
							}
							if (!existsNearNode) {
								homotopic = false;
								break;
							}	
						}
					}

				}
				Debug.Log ("Check if homotopic, done : " + homotopic.ToString ());	
				if (homotopic) {
					if (Statics.mesh) {
						actHomotopy.setMesh (level.GetStaticPositions ());
					} else {
						actHomotopy.AddCurveToBundle (level.pathFactory, draggingPosition);
					}
					actHomotopy.SetColor (level.GetRandomColor ());
					otherPath.SetColor (midPath.color);
					Destroy (actHomotopy.midPath.line.gameObject);
					actHomotopy = null;
					break;
				}
			}
		}
	}

	void DrawLine3D (RaycastHit hit)
	{
		dot1 = level.dots.IndexOf (hit.collider.gameObject);
		if (dot1 != -1) {
			isDrawingPath = true;
			Debug.Log ("Instantiate");
			dotMove = Instantiate (level.dots [dot1]);
			dotMove.GetComponent<BoxCollider> ().isTrigger = false;
			dotMove.GetComponent<Rigidbody> ().collisionDetectionMode = CollisionDetectionMode.Continuous;
			dotMove.transform.position = Misc.SetOnSurface (hit.point, Statics.dotSpacer);
			Debug.Log ("DotMove is at Height " + dotMove.transform.position.magnitude);
//			dotMove.transform.position = new Vector3 (level.dots [dot1].transform.position.x, level.dots [dot1].transform.position.y, Statics.dotDepth.z * 1.1f);
			Debug.Log ("Set Color");
			trail = dotMove.GetComponentInChildren<TrailRenderer> ();
			trail.textureMode = LineTextureMode.Tile;
			trail.material = level.trailMat;
			trail.startWidth = Statics.lineThickness;
			trail.endWidth = Statics.lineThickness;
			trailColor = level.GetRandomColor ();
			Camera.main.GetComponent <RotateCamera> ().SetPlayer (dotMove);
		} else {
			Debug.Log ("Set DotMove to null");
			dotMove = null;
		}
	}

	void setPathToDrag (Vector3 touchPoint, int pathNumber)
	{
		int numOnPath = -1;
		Vector3 vector = new Vector3 ();
		bool isOnPath = GetPositionOnPath (pathNumber, touchPoint, ref numOnPath, ref vector);
		Path path;
		if (pathNumber == -2) {
			path = actHomotopy.midPath;
		} else {
			path = level.paths [pathNumber];
		}
		if (isOnPath && numOnPath != 0 && numOnPath != path.Count - 1) {
			Debug.Log ("Start Dragging Line " + pathNumber);
			if (pathNumber != -2) {
				var path1 = level.paths [pathNumber];
				if (actHomotopy != null) {
					Destroy (GameObject.Find ("MidPath"));
					actHomotopy.Clear ();
					actHomotopy = null;
				}
				var actMidPath = level.NewPath (path1.color, "MidPath", path1.dotFrom, path1.dotTo);
				actHomotopy = level.NewHomotopy (path1, actMidPath);
			}
			upperBound = actHomotopy.midPath.line.positionCount;
			lowerBound = -1;
			draggingPath = -2;
			draggingPosition = numOnPath;
		} else {
			Debug.Log ("Not on Line");
		}
	}


	void DrawDot (Vector3 touchPoint)
	{
		Ray ray = Camera.main.ScreenPointToRay (Input.GetTouch (0).position);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit) && hit.collider.gameObject.CompareTag ("Sphere")) {
			Debug.Log ("Touch not on some Path, so set Dot");
			GameObject dot;
			dot = Instantiate (level.dotPrefab3D);
			dot.transform.position = Misc.SetOnSurface (hit.point, Statics.dotSpacer);
			var vector3 = Misc.ManifoldCenter (hit.point);
			dot.transform.LookAt (vector3);
			dot.GetComponent<SpriteRenderer> ().color = level.GetRandomColor ();
			level.addDot (dot);
		}
	}

	void FinishLine ()
	{
		if (dotMove.GetComponent<dotBehaviour3D> ().IsTriggered ()) {
			Debug.Log ("Triggered");
			var dotBehaviourDotMove = dotMove.GetComponent<dotBehaviour3D> ();
			var bumpedDot = dotBehaviourDotMove.GetTriggerObject ();
			if (bumpedDot != level.dots [dot1]) {
				dot2 = level.dots.IndexOf (bumpedDot);
				var path = level.NewPath (trailColor, level.dots [dot1], level.dots [dot2]);
				for (int i = 0; i < trail.positionCount; i++) {
					path.line.SetPosition (i, trail.GetPosition (i));
				}
				path.line.InsertPositionAt (0, level.dots [dot1].transform.position);
				path.line.SetPosition (path.Count, bumpedDot.transform.position);
				path.line.SetMesh ();
				//Set Colors
				dotMove.transform.position = bumpedDot.transform.position;
				dotMove.GetComponent <SpriteRenderer> ().enabled = false;
				StartCoroutine (DrawPath (path, false));
			}
		} else {
//			Destroy (dotMove);
		}
		Statics.isDragging = false;
	}

	void SetDotColors (int dot1, int dot2)
	{
		var group1 = new List<int> ();
		var group2 = new List<int> ();
		for (int i = 0; i < level.dotHomClasses.Count; i++) {
			var group = level.dotHomClasses [i];
			if (group.Contains (dot1)) {
				group1 = group;
			} else if (group.Contains (dot2)) {
				group2 = group;
			}
		}
		for (int i = 0; i < group2.Count; i++) {
			var item = group2 [i];
			level.dots [item].GetComponent<SpriteRenderer> ().color = level.dots [dot1].GetComponent<SpriteRenderer> ().color;
			group1.Add (item);
		}
	}

	void ShortenPath (Path path)
	{
		Debug.Log ("Shorten from start");
		var startPosition = path.dotFrom.transform.position;
		var node = 0;
		var counter = 0;
		while (node < path.Count) {
			if (Vector2.Distance (path.line.GetPosition (node), startPosition) >= dotRadius) {
				break;
			}
			counter++;
			node++;
		}
		for (int i = 0; i < node; i++) {
			path.line.Remove (i);
		}
		Debug.Log ("Shorten " + counter + ", now end");
		counter = 0;
		var endPosition = path.dotTo.transform.position;
		node = path.Count - 1;
		while (node >= 0) {
			if (Vector2.Distance (path.line.GetPosition (node), endPosition) >= dotRadius) {
				break;
			}
			counter++;
			node--;
		}
		for (int i = 1; i < node; i++) {
			path.line.Remove (path.Count - i);
		}
		Debug.Log ("Shorten " + counter);
	}


	// end of update
	//Returns number of path, index on path
	bool GetPositionOnPath (int pathNumber, Vector3 touchPoint, ref int numOnPath, ref Vector3 posOnPath)
	{
		numOnPath = -1;
		float distToPath = Statics.lineThickness * 2f;
		float minDist = distToPath;
		if (pathNumber == -2) {
			var path = actHomotopy.midPath;
			for (int j = 0; j < path.Count; j++) {
				var value = path.line.GetPosition (j);
				var dist = Vector2.Distance (touchPoint, value);
				if ((dist < distToPath && numOnPath == -1) || (dist < minDist && numOnPath != -1)) {
					numOnPath = j;
					minDist = dist;
					posOnPath = value;
				}
			}
			if (numOnPath != -1) {
				return true;
			} else {
				return false;
			}
		} else {
			var path = level.paths [pathNumber];
			for (int j = 0; j < path.Count; j++) {
				var value = path.line.GetPosition (j);
				var dist = Vector2.Distance (touchPoint, value);
				if ((dist < distToPath && numOnPath == -1) || (dist < minDist && numOnPath != -1)) {
					numOnPath = j;
					minDist = dist;
					posOnPath = value;
				}
			}
			if (numOnPath != -1) {
				return true;
			} else {
				return false;
			}
		}
	}

	bool IsOnPath (Vector3 touchPoint, ref int pathNumber, bool checkMidPathFirst = false)
	{
		pathNumber = -1;
		if (checkMidPathFirst) {
			Debug.Log ("Check Midpath");
			var path = actHomotopy.midPath;
			var node = 0;
			while (node < path.Count) {
				var dist = Vector2.Distance (touchPoint, path.line.GetPosition (node));
				if (dist < minDist) {
					pathNumber = -2;
					return true;
				}
				node++;
			}
		}
		Debug.Log ("Check other Paths");
		for (int i = 0; i < level.paths.Count; i++) {
			var path = level.paths [i];
			var node = 0;
			while (node < path.Count) {
				var dist = Vector2.Distance (touchPoint, path.line.GetPosition (node));
				if (dist < minDist) {
					pathNumber = i;
					return true;
				}
				node++;
			}
		}
		return false;
	}

	bool IsOnMidPath (Vector3 worldPoint, ref int numOnPath, ref Vector3 vector)
	{
		numOnPath = -1;
		float distToPath = Statics.lineThickness * 3f;
		float minDist = distToPath;
		var path = actHomotopy.midPath;
		var node = 0;
		int counter = 0;
		while (node < path.Count) {
			var value = path.line.GetPosition (node);
			var dist = Vector2.Distance (worldPoint, value);
			if ((dist < distToPath && numOnPath == -1) || (dist < minDist && numOnPath != -1)) {
				numOnPath = counter;
				minDist = dist;
				vector = value;
			}
			counter++;
			node++;
		}
		if (numOnPath != -1) {
			return true;
		} else {
			return false;
		}
	}

	IEnumerator DrawPath (Path path, bool fast)
	{
		Debug.Log ("DrawPath " + path);
		isCollided [path] = false;
		var line = path.line;
		line.gameObject.SetActive (false);
		path.smoothLine ();
		path.RefinePath ();
		ShortenPath (path);
		path.DrawArrows ();
		var savedPositions = path.line.GetPositions ();
		var node = 0;
		line.gameObject.SetActive (true);
		int sum = 0;
		var duration = 300;
		if (fast) {
			duration = 50;
		}
		var arrowCounter = 0;
		for (int i = 0; i < savedPositions.Count; i++) {
			if (arrowCounter < path.arrows.Count && Vector2.Distance (path.arrows [arrowCounter].transform.position, savedPositions [node]) < Statics.meanDist) {
				path.arrows [arrowCounter].SetActive (true);
				arrowCounter++;
			}
			line.SetPosition (i, savedPositions [node]);
			node++;
			sum++;
			if (sum > path.Count / duration) {
				sum = 0;
				line.SetMesh ();
				yield return null;
			}
		}

		SetDotColors (dot1, dot2);
		Destroy (dotMove);
		level.addPath (path);
		isDrawingPath = false;
		//		var pathObject = line.gameObject;
		//		var partsystem = pathObject.AddComponent<ParticleSystem> ();
		//		var renderer = partsystem.GetComponent<ParticleSystemRenderer> ();
		//		renderer.maxParticleSize = 0.01f;
		//		renderer.minParticleSize = 0.01f;
		//		renderer.material = partMaterial;
		//		var main = partsystem.main;
		//		main.maxParticles = 1;
		//		main.startSpeed = 0;
		//		main.startLifetime = 1000;
		//		main.playOnAwake = true;
		//		var emission = partsystem.emission;
		//		emission.rateOverTime = 0.5f;
		//		StartCoroutine (AddParticleSystem (path));
		yield break;
	}


	int GetNearestPointTo (Vector3 point, Line line)
	{
		float smallestDistance = 100000f;
		int index = 0;
		for (int i = 0; i < line.positionCount; i++) {
			var dist = Vector2.Distance (point, line.GetPosition (i));
			if (dist < smallestDistance) {
				index = i;
				smallestDistance = dist;
			}
		}
		return index;
	}

}