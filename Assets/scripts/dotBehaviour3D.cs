using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dotBehaviour3D : MonoBehaviour
{

	private bool isTriggered = false;
	Collider other = null;
	public Vector3 center;

	public Vector3 direction;

	// Use this for initialization
	void Start ()
	{
		Debug.Log ("New Dot");
		var rb = this.GetComponent<Rigidbody> ();
		rb.freezeRotation = true;
		direction = this.transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		direction = (this.transform.position - direction).normalized;
		this.transform.LookAt (Misc.ManifoldCenter (this.transform.position), this.transform.up);
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.CompareTag ("Dot")) {
			isTriggered = true;
			other.gameObject.GetComponent<dotBehaviour3D> ().SetTriggered (true);
			this.other = other;
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (other.gameObject.CompareTag ("Dot")) {
			Debug.Log ("Exit");
			isTriggered = false;
			other.gameObject.GetComponent<dotBehaviour3D> ().SetTriggered (false);
			this.other = null;
		}
	}

	public bool IsTriggered ()
	{
		return isTriggered;
	}

	public Vector3 GetTríggerPosition ()
	{
		if (other != null) {
			return other.gameObject.transform.position;
		} else {
			return Vector3.zero;
		}
	}

	public void SetTriggered (bool b)
	{
		isTriggered = b;
	}

	public GameObject GetTriggerObject ()
	{
		if (other != null) {
			return other.gameObject;
		} else {
			return null;
		}
	}

}
