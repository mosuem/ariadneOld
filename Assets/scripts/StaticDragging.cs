using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using UnityEditor;
using System;
using System.Linq;

public class StaticDragging : MonoBehaviour
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
			var cameraPosition = Camera.main.transform.position;
			if (touch1.phase == TouchPhase.Began) {
				if (Statics.drawCircle) {
					drawing = true;
					circle = Instantiate (level.staticCirclePrefab);
					drawPos = touchPoint + Statics.dotDepth;
					circle.transform.position = drawPos;
					circle.transform.localScale = Vector3.zero;
					level.statics.Add (circle);
				} else if (Statics.drawRectangle) {
					drawing = true;
					rectangle = Instantiate (level.staticRectPrefab);
					drawPos = touchPoint + Statics.dotDepth;
					rectangle.transform.position = drawPos;
					rectangle.transform.localScale = Vector3.zero;
					level.statics.Add (rectangle);
				} else if (Statics.deleteObstacle) {
					RaycastHit hit;
					bool hasHit = Physics.Raycast (cameraPosition, touchPoint - cameraPosition, out hit);
					if (hasHit && hit.collider.CompareTag ("Obstacle")) {
						level.statics.Remove (hit.collider.gameObject);
						Destroy (hit.collider.gameObject);
					}
					Statics.deleteObstacle = false;
				} else {
					RaycastHit hit;
					bool hasHit = Physics.Raycast (cameraPosition, touchPoint - cameraPosition, out hit);
					if (hasHit) {
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
					rectangle.transform.localScale = new Vector3 (Mathf.Abs (xFactor), Mathf.Abs (yFactor), 0);
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