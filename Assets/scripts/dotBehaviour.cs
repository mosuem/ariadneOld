using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dotBehaviour : MonoBehaviour
{

	private bool isTriggered = false;
	Collider other = null;
	// Use this for initialization
	void Start ()
	{
		Debug.Log ("New Dot");
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.gameObject.CompareTag ("Dot")) {
			isTriggered = true;
			other.gameObject.GetComponent<dotBehaviour> ().SetTriggered (true);
			this.other = other;
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (other.gameObject.CompareTag ("Dot")) {
			Debug.Log ("Exit");
			isTriggered = false;
			other.gameObject.GetComponent<dotBehaviour> ().SetTriggered (false);
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
