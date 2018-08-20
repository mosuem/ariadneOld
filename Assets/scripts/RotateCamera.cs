using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
	private Vector3 veryfirstpoint;
	private Vector3 firstpoint;
	//change type on Vector3
	private Vector3 secondpoint;
	private float xAngle = 0f;
	//angle for axes x for rotation
	private float yAngle = 0f;
	private float xAngTemp = 0f;
	//temp variable for angle
	private float yAngTemp = 0f;
	bool topdown = false;
	bool rightleft = false;
	int counter = 0;

	private GameObject _player;

	private Vector3 offset;

	void Start ()
	{
		//Initialization our angles of camera
		xAngle = 0f;
		yAngle = 0f;
		this.transform.rotation = Quaternion.Euler (yAngle, xAngle, 0f);
	}

	void Update ()
	{
		if (!Statics.isDragging) {
			//Check count touches
			if (Input.touchCount > 0) {
				Ray ray = Camera.main.ScreenPointToRay (Input.GetTouch (0).position);
				if (!Physics.Raycast (ray)) {
					//Touch began, save position
					if (Input.GetTouch (0).phase == TouchPhase.Began) {
						firstpoint = Input.GetTouch (0).position;
						secondpoint = Input.GetTouch (0).position;
						veryfirstpoint = Input.GetTouch (0).position;
						xAngTemp = xAngle;
						yAngTemp = yAngle;
					}
			//Move finger by screen
			else if (Input.GetTouch (0).phase == TouchPhase.Moved) {
						if (counter > 2) {
							firstpoint = secondpoint;
							secondpoint = Input.GetTouch (0).position;
							if (!topdown && !rightleft) {
								var diffX = Mathf.Abs (secondpoint.x - veryfirstpoint.x);
								var diffY = Mathf.Abs (secondpoint.y - veryfirstpoint.y);
								if (diffX > diffY) {
									rightleft = true;
								} else {
									topdown = true;
								}
							}
							//Rotate camera
							if (rightleft) {
								var angle = (secondpoint.x - firstpoint.x);
								if (angle < 0) {
									angle = 360 + angle;
								}
								var up = Vector3.up;
								this.transform.RotateAround (Vector3.zero, up, angle);
							} else if (topdown) {
								var angle = (secondpoint.y - firstpoint.y);
								if (angle < 0) {
									angle = 360 + angle;
								}
								var right = Vector3.Cross (this.transform.position, Vector3.up);
								right = this.transform.right;
								this.transform.RotateAround (Vector3.zero, right, angle);
							}
						} else {
							counter++;
						}
					} else if (Input.GetTouch (0).phase == TouchPhase.Ended || Input.GetTouch (0).phase == TouchPhase.Canceled) {
						topdown = false;
						rightleft = false;
						counter = 0;
					}
				}
			}
		}
	}

	void LateUpdate ()
	{
		if (Statics.firstPerson && _player != null) {
			var center = Misc.ManifoldCenter (_player.transform.position);
			var normal = _player.transform.position - center;
			transform.position = center + normal * 2f;
			transform.LookAt (_player.transform, _player.transform.up);
		}
	}

	public void SetPlayer (GameObject g)
	{
		_player = g;
		offset = transform.position - _player.transform.position;
//		transform.rotation = Quaternion.LookRotation (_player.transform.position);
	}
}
