using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text.RegularExpressions;

public class levelChoiceBehavior : MonoBehaviour
{
	public GameObject buttonPrefab;
	public GameObject onClickObject;
	public GameObject canvas;
	public GameObject preview;
	public GameObject deletePrefab;
	private List<LevelChoiceButton> buttons;
	// Use this for initialization


	void Start ()
	{
		int nextRow = 0;
		int nextCol = -1;
		var width = Screen.currentResolution.width;
		var files = Misc.GetFiles (Statics.levelType, "dat");
		var images = Misc.GetFiles (Statics.levelType, "png");
		buttons = new List<LevelChoiceButton> ();
		Debug.Log ("Number of Levels is " + files.Count);
		for (int i = 0; i < files.Count; i++) {
			Debug.Log (files [i].Name);
			string number = Regex.Replace (files [i].Name, "[^0-9]", "");
			Debug.Log (number);
			var temp = int.Parse (number);
			var newButton = Instantiate (buttonPrefab);
			newButton.transform.SetParent (canvas.transform, false);
			if ((nextCol + 2) * 400 > width) {
				if (nextRow == 0) {
					nextRow++;
					nextCol = 0;
				} else {
					nextRow = 0;
					break;
				}
			} else {
				nextCol++;
			}
			newButton.transform.position += Vector3.right * nextCol * 400;
			newButton.transform.position += Vector3.down * nextRow * 350;
			var button = newButton.GetComponent<Button> ();
			var script = onClickObject.GetComponent<ExitScript> ();
			newButton.GetComponentInChildren<Text> ().text = "Level " + number;
			button.onClick.AddListener (() => {
				script.NextLevelButton (temp);
			});
			//Image
			var newImage = Instantiate (preview);
			newImage.transform.SetParent (canvas.transform, false);
			newImage.transform.position += Vector3.right * nextCol * 400;
			newImage.transform.position += Vector3.down * nextRow * 350;
			var image = newImage.GetComponentInChildren<Image> ();
			Texture2D tex = new Texture2D (2, 2);
			byte[] data = File.ReadAllBytes (images [i].FullName);
			var boolean = tex.LoadImage (data);
			Debug.Log (boolean);
			image.sprite = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0.5f, 0.5f), 1f);
			var imageClick = newImage.AddComponent<ImageClick> ();
			imageClick.i = temp;
			imageClick.button = newButton;
			//Delete
			var deleteButton = Instantiate (deletePrefab);
			deleteButton.transform.SetParent (canvas.transform, false);
			deleteButton.transform.position += Vector3.right * nextCol * 400;
			deleteButton.transform.position += Vector3.down * nextRow * 350;
			var button2 = deleteButton.GetComponent<Button> ();
			Destroy (deleteButton.GetComponentInChildren<Text> ());
			button2.onClick.AddListener (() => {
				Statics.isLoading = true;
				script.DeleteLevel (temp);
				foreach (var item in buttons) {
					item.Delete ();
				}
				Start ();
			});

			LevelChoiceButton lvlButton = new LevelChoiceButton (newButton, deleteButton, newImage);
			buttons.Add (lvlButton);
		}
		Statics.isLoading = false;
	}

	void OnGUI ()
	{
		if (Statics.isLoading) {
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
