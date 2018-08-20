using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//using UnityEditor;
using System;
using System.Linq;

//using UnityEditor;

class TargetJoint3D : MonoBehaviour
{

	public float damping;
	public Vector3 target;
	public float spring;

	void Awake ()
	{
		spring = 5f;
		damping = 10f;
	}

	void Update ()
	{
		var position = gameObject.transform.position;
		var body = gameObject.GetComponent<Rigidbody> ();

		float distance = Vector3.Distance (target, position);
		Vector3 direction = (target - position).normalized;
		body.velocity += (direction * distance * spring) / body.mass;

		//damping
		float dampingValue = 1 - (1 / (distance * damping + 1));
		body.velocity *= dampingValue;
	}
}
