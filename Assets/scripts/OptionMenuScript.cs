using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenuScript : MonoBehaviour
{
	// Use this for initialization

	void Start ()
	{
		if (Statics.showingAlgebra) {
			GameObject.Find ("Algebra").GetComponentInChildren<Text> ().text = "Showing Algebra";
		} else {
			GameObject.Find ("Algebra").GetComponentInChildren<Text> ().text = "Hiding Algebra";
		}

		if (Statics.firstPerson) {
			GameObject.Find ("FirstPerson").GetComponentInChildren<Text> ().text = "First Person View";
		} else {
			GameObject.Find ("FirstPerson").GetComponentInChildren<Text> ().text = "Over View";
		}

		if (Statics.showingHints) {
			GameObject.Find ("ShowHints").GetComponentInChildren<Text> ().text = "Showing Hints";
		} else {
			GameObject.Find ("ShowHints").GetComponentInChildren<Text> ().text = "Hiding Hints";
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnGUI ()
	{
		if (Statics.isLoading) {
			//GUI.Label (new Rect (0, 0, Screen.currentResolution.width, Screen.currentResolution.height), "Loading...", guiStyle);
			GUI.Window (0, new Rect (0, 0, Screen.currentResolution.width, Screen.currentResolution.height), DoMyWindow, "", GUI.skin.GetStyle ("window"));
		}
	}

	void DoMyWindow (int windowID)
	{
		GUIStyle guiStyle = GUI.skin.GetStyle ("button");
		guiStyle.fontSize = 60;
		guiStyle.alignment = TextAnchor.MiddleCenter;
		Color color = Color.white;
		GUI.contentColor = color;
		var width = 400;
		var height = 100;
		GUI.Box (new Rect (Screen.currentResolution.width / 2 - width / 2, Screen.currentResolution.height / 2 - height / 2, width, height), "Loading...", guiStyle);
	}

}
