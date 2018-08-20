using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Retractor : MonoBehaviour
{
	Path path;

	float maxTime;

	public void StartRetractor (Path path, float maxTime)
	{
		this.maxTime = maxTime;
		this.path = path;
		bool isHomotopic = true;
		if (isHomotopic) {
			StartCoroutine (retraction ());
		}
	}

	IEnumerator retraction ()
	{
		for (int j = 1; j < path.Count - 1; j++) {
			Debug.Log ("Start retracting position " + j);
			path.line.SetPosition (j, path.dotFrom.transform.position);
			AdjustPositionsRetraction (j);
			if (j % 5 == 0) {
//				UnityEditor.EditorApplication.isPaused = true;
				path.line.SetMesh ();
				yield return null;
			}
		}
		yield break;
	}

	void AdjustPositionsRetraction (int j)
	{
		var start = j + 1;
		for (int i = start; i < path.Count - 1; i++) {
			var posBefore = path.line.GetPosition (i - 1);
			var position = path.line.GetPosition (i);
			float factor = (i - start) / (path.Count - start);
			var dist = Vector3.Distance (posBefore, position);
			if (dist >= Statics.meanDist && factor < 0.999f) {
//				var newPos = Vector3.Lerp (posBefore, position, 0.5f + 0.5f * Mathf.Clamp01 (factor));
				var newPos = Vector3.Lerp (posBefore, position, Statics.meanDist / dist);

				Collider collider1;
				bool hasHit1 = Misc.HasHit (newPos, out collider1);
				if (hasHit1) {
					newPos = Misc.PushOutOfObstacle (collider1, newPos);
				}
				path.line.SetPosition (i, new Vector3 (newPos.x, newPos.y, position.z));
			} else {
				break;
			}
		}
		var beforeLast = path.line.GetPosition (path.Count - 2);
		var last = path.line.GetPosition (path.Count - 1);
		if (Vector3.Distance (beforeLast, last) > Statics.meanDist) {
			path.line.InsertPositionAt (path.Count - 1, Vector3.Lerp (beforeLast, last, Statics.meanDist / Vector3.Distance (beforeLast, last)));
		}
	}
}
