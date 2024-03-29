using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using UnityEditor;
using System;
using System.Linq;

public class StaticDragging3D : MonoBehaviour
{
	LevelData level;

	void Start ()
	{
		level = Camera.main.GetComponent<LevelData> ();
	}

	bool drawing;

	GameObject circle;

	GameObject rectangle;

	Vector3 drawPos;
	private GameObject draggingObstacle;
	private Vector3 draggingOffset;

	// Update
	void Update ()
	{
		int tapCount = Input.touchCount;
		if (tapCount == 1 && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject (Input.GetTouch (0).fingerId)) {// && !EventSystem.current.IsPointerOverGameObject (Input.GetTouch (0).fingerId)
			Touch touch1 = Input.GetTouch (0);
			Vector3 touchPoint = Camera.main.ScreenToWorldPoint (touch1.position);
			touchPoint.z = 0;
			if (touch1.phase == TouchPhase.Began) {
				if (Statics.drawCircle) {
					Ray ray = Camera.main.ScreenPointToRay (Input.GetTouch (0).position);
					RaycastHit hit;
					if (Physics.Raycast (ray, out hit) && hit.collider.gameObject.CompareTag ("Sphere")) {
						drawing = true;
						circle = Instantiate (level.staticCirclePrefab);
						drawPos = touchPoint;
						
						Debug.Log ("Touch not on some Path, so set Dot");

						circle.transform.position = Misc.SetOnSurface (hit.point, Statics.dotSpacer);
						var vector3 = Misc.ManifoldCenter (hit.point);
						circle.transform.LookAt (vector3);
			
					circle.transform.position = touchPoint;
					circle.transform.localScale = Vector3.zero;
					level.statics.Add (circle);
					}
				} else if (Statics.drawRectangle) {
					drawing = true;
					rectangle = Instantiate (level.staticRectPrefab);
					drawPos = touchPoint;
					rectangle.transform.position = touchPoint;
					rectangle.transform.localScale = Vector3.zero;
					level.statics.Add (rectangle);
				} else if (Statics.deleteObstacle) {
					Statics.deleteObstacle = false;
					RaycastHit2D hit = Physics2D.Raycast (touchPoint, Vector3.zero);
					if (hit.collider != null && hit.collider.CompareTag ("Obstacle")) {
						level.statics.Remove (hit.collider.gameObject);
						Destroy (hit.collider.gameObject);
					}
				} else {
					RaycastHit2D hit = Physics2D.Raycast (touchPoint, Vector3.zero);
					if (hit.collider != null) {
						if (hit.collider.gameObject.CompareTag ("Obstacle") && level.staticsAllowed) {
							var index = level.statics.IndexOf (hit.collider.gameObject);
							StartDraggingObstacle (index, touchPoint);						
						}
					}
				}
			} else if (touch1.phase == TouchPhase.Moved) {
				if (Statics.drawCircle) {
					circle.transform.localScale = Vector3.one * Vector3.Magnitude (circle.transform.position - touchPoint);
				} else if (Statics.drawRectangle) {
					var xFactor = drawPos.x - touchPoint.x;
					var yFactor = drawPos.y - touchPoint.y;
					rectangle.transform.localScale = new Vector3 (xFactor, yFactor, 0);
					rectangle.transform.position = drawPos - new Vector3 (xFactor / 2f, yFactor / 2f, 0);
				} else if (draggingObstacle != null) {
					DragObstacle (touchPoint);
				}
			} else if (touch1.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Canceled) {
				if (drawing) {
					Debug.Log ("Drawing ended");
					drawing = false;
					Statics.drawCircle = false;
					Statics.drawRectangle = false;
				}
				if (draggingObstacle != null) {
					Debug.Log ("Finish dragging gameObject");
					draggingObstacle = null;
				}
			}
		}
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
}