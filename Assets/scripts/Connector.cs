using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour
{
	LevelData level;

	Path path1;

	Path path2;

	bool isHomotopic;

	public void StartConnector (Path path1, Path path2, int path1Num, int path2Num, LevelData level)
	{
		this.level = level;
		this.path1 = path1;
		this.path2 = path2;
		isHomotopic = level.GetHomotopyClass (path2Num).Contains (path1Num);
		Debug.Log ("Are homotopic? " + isHomotopic);
		StartCoroutine (drawConnector (true));
	}

	float width;

	Line connector;
	GameObject dot1;
	GameObject dot2;

	IEnumerator drawConnector (bool space)
	{
		Debug.Log ("Start Checking Paths");
		var connectorObject = new GameObject ("ConnectionTest between " + path1.pathNumber + " and " + path2.pathNumber);
		connector = new Line (connectorObject);

		connector.SetMaterial (level.pathMat);
		connector.sortingOrder = Statics.LineSortingOrder + 1;
		width = Statics.lineThickness / 2f;
		connector.width = width;
		connector.SetColor (Color.red);

		connector.SetPosition (0, path1.line.GetPosition (0));
		connector.SetPosition (1, path2.line.GetPosition (0));

		dot1 = Instantiate (level.dotPrefab);
		dot1.transform.position = path1.line.GetPosition (0);
		dot1.GetComponent<SpriteRenderer> ().color = connector.GetColor ();
		GameObject.Destroy (dot1.GetComponentInChildren <TrailRenderer> ());
		dot1.GetComponent <SpriteRenderer> ().sortingOrder = connector.sortingOrder;
		dot1.transform.localScale = width * 2 * Vector3.one;

		dot2 = Instantiate (level.dotPrefab);
		dot2.transform.position = path2.line.GetPosition (0);
		dot2.GetComponent<SpriteRenderer> ().color = connector.GetColor ();
		GameObject.Destroy (dot2.GetComponentInChildren <TrailRenderer> ());
		dot2.GetComponent <SpriteRenderer> ().sortingOrder = connector.sortingOrder;
		dot2.transform.localScale = width * 2 * Vector3.one;

		var maxDistance = 1f;
		var speed = 10f;
		var meanTime = 1f / speed;
		var timeForStep1 = meanTime;
		var timeForStep2 = meanTime;
		if (space == false) {
			if (path1.Count < path2.Count) {
				timeForStep2 = ((float)path1.Count / path2.Count) * timeForStep1;
			} else {
				timeForStep1 = ((float)path2.Count / path1.Count) * timeForStep2;
			}
		}
		Debug.Log (path1.Count * timeForStep1 + " vs. " + path2.Count * timeForStep2);
		Debug.Log (timeForStep1 + " vs. " + timeForStep2);
		var timeGone1 = 0f;
		var timeGone2 = 0f;

		var i = 1;
		var j = 1;
		while (i < path1.Count - 1 || j < path2.Count - 1) {
			if (i < path1.Count - 1 && connector.GetPosition (0) == path1.line.GetPosition (i)) {
				i++;
				timeGone1 = timeGone1 - timeForStep1;
			}
			if (j < path2.Count - 1 && connector.GetPosition (1) == path2.line.GetPosition (j)) {
				j++;
				timeGone2 = timeGone2 - timeForStep2;
			}

			timeGone1 += Time.deltaTime;
			timeGone2 += Time.deltaTime;

			//			var t = GetPercentage (path1, i) - GetPercentage (path2, j);
			//			timeForStep1 = 1f / (speed + Mathf.Lerp (-speed, speed, 0.5f - t / 2));
			//			timeForStep2 = 1f / (speed + Mathf.Lerp (-speed, speed, 0.5f + t / 2));
			//			Debug.Log ("t = " + t + ", factor= " + (0.5f - t / 2) + ", lerp = " + Mathf.Lerp (-speed, speed, 0.5f - t / 2) + ", timeForStep1-meantime = " + (timeForStep1 - meanTime));
			connector.SetPosition (0, Vector3.Lerp (path1.line.GetPosition (i - 1), path1.line.GetPosition (i), timeGone1 / timeForStep1));
			connector.SetPosition (1, Vector3.Lerp (path2.line.GetPosition (j - 1), path2.line.GetPosition (j), timeGone2 / timeForStep2));
			connector.width = width * (1 - Vector3.Distance (connector.GetPosition (0), connector.GetPosition (1)) / (maxDistance * 1.1f));
			connector.SetMesh ();

			dot1.transform.position = connector.GetPosition (0);
			dot2.transform.position = connector.GetPosition (1);

			if (Vector3.Distance (dot1.transform.position, dot2.transform.position) > maxDistance) {
				StartCoroutine (ShrinkConnector (3f));
				yield return new WaitForSeconds (5);
				Destroy (connectorObject);
				Destroy (dot1);
				Destroy (dot2);
				yield break;
			}
			yield return null;
		}
		if (isHomotopic) {
			level.ShowHintCanvas ();
			LevelData.showHint ("The paths are homotopic!");
			yield return new WaitForSeconds (5);
			level.HideHintCanvas ();
		}
		yield break;
	}



	IEnumerator ShrinkConnector (float time)
	{
		float timeSum = 0f;
		var lastWidth = connector.width;
		while (timeSum < time) {
			var f = Mathf.SmoothStep (lastWidth, 0, timeSum / time);
			var f2 = Mathf.SmoothStep (width * 2, 0, timeSum / time);
			connector.width = f;
			dot1.transform.localScale = f2 * Vector3.one;
			dot2.transform.localScale = f2 * Vector3.one;
			connector.SetMesh ();
			timeSum += Time.deltaTime;
			yield return null;
		}
		yield break;
	}

	static float GetPercentage (Path path1, int i)
	{
		return (float)i / path1.Count;
	}
}
