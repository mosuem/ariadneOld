using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class ExitScript : MonoBehaviour
{
	public GameObject hintCanvas;

	public void MainMenuAndSave ()
	{
		Debug.Log ("Saving");
		var level = Camera.main.GetComponent<LevelData> ();
		level.SaveObjects (Statics.levelType, level.levelNumber);
		SceneManager.LoadScene ("mainMenu");
	}

	public void SaveTo (string levelType)
	{
		Debug.Log ("Saving");
		var level = Camera.main.GetComponent<LevelData> ();
		level.SaveObjects (levelType);
		SceneManager.LoadScene ("mainMenu");
		showSaveButtons ();
	}

	public void ShowWindingNumber ()
	{
		Statics.showWindingNumber = !Statics.showWindingNumber;
		hintCanvas.SetActive (Statics.showWindingNumber && Statics.showingHints);
		Statics.hintCanvasActive = Statics.showWindingNumber && Statics.showingHints;
		LevelData.showHint ("Touch a path");
		Misc.flipButtonSprites ("Windung");
	}

	public void checkPaths ()
	{
		Statics.checkPath = !Statics.checkPath;
		Misc.flipButtonSprites ("Check");
		hintCanvas.SetActive (Statics.checkPath && Statics.showingHints);
		Statics.hintCanvasActive = Statics.checkPath && Statics.showingHints;
		LevelData.showHint ("Touch a path");
	}

	public void collapsePath ()
	{
		Statics.retractPath = !Statics.retractPath;
		Misc.flipButtonSprites ("Collapse");
		hintCanvas.SetActive (Statics.retractPath && Statics.showingHints);
		Statics.hintCanvasActive = Statics.retractPath && Statics.showingHints;
		LevelData.showHint ("Touch a path");
	}

	public void MainMenuNoSave ()
	{
		SceneManager.LoadScene ("mainMenu");
	}

	public void LoadScene (string str)
	{
		SceneManager.LoadScene (str);
	}

	public void LoadLevelChoice ()
	{
		SceneManager.LoadScene ("levelChoice");
	}

	public void LoadOptions ()
	{
		SceneManager.LoadScene ("optionMenu");
	}

	public void AlgebraButton ()
	{
		if (Statics.showingAlgebra) {
			Statics.showingAlgebra = false;
			GameObject.Find ("Algebra").GetComponentInChildren<Text> ().text = "Hiding Algebra";
		} else {
			Statics.showingAlgebra = true;
			GameObject.Find ("Algebra").GetComponentInChildren<Text> ().text = "Showing Algebra";
		}
	}

	public void FirstPersonButton ()
	{
		if (Statics.firstPerson) {
			Statics.firstPerson = false;
			GameObject.Find ("FirstPerson").GetComponentInChildren<Text> ().text = "Over View";
		} else {
			Statics.firstPerson = true;
			GameObject.Find ("FirstPerson").GetComponentInChildren<Text> ().text = "First Person View";
		}
	}

	public void ShowHintsButton ()
	{
		if (Statics.showingHints) {
			Statics.showingHints = false;
			GameObject.Find ("ShowHints").GetComponentInChildren<Text> ().text = "Hiding Hints";
		} else {
			Statics.showingHints = true;
			GameObject.Find ("ShowHints").GetComponentInChildren<Text> ().text = "Showing Hints";
		}
	}

	public void MeshTypeButton ()
	{
		if (Statics.mesh) {
			Statics.mesh = false;
			GameObject.Find ("MeshType").GetComponentInChildren<Text> ().text = "Family of curves";
		} else {
			Statics.mesh = true;
			GameObject.Find ("MeshType").GetComponentInChildren<Text> ().text = "Filled-In Mesh";
		}
	}



	public void NextLevelButton (int level)
	{
		Statics.nextSceneNumber = level;
		SceneManager.LoadScene ("all");
	}

	public void setLevel (string levelName)
	{
		Statics.levelType = levelName;
		Statics.isLoading = true;
		Statics.isSphere = false;
		if (levelName.Equals ("all")) {
			SceneManager.LoadScene ("all");	
		} else if (levelName.Equals ("sphere")) {
			Statics.isSphere = true;
			SceneManager.LoadScene ("sphere");
		} else if (levelName.Equals ("torus")) {
			Statics.isTorus = true;
			SceneManager.LoadScene ("sphere");
		} else {
			SceneManager.LoadScene ("levelChoice");
		}
	}

	public void Exit ()
	{
		Application.Quit ();
	}

	public void DeleteLevel (int levelNum)
	{
		string path = Statics.folderPath + Statics.levelType + "/";
		Debug.Log (Statics.levelType);
		Debug.Log ("Delete " + path + "level" + levelNum + ".dat");
		File.Delete (path + "level" + levelNum + ".dat");
		Debug.Log ("Delete " + path + "level" + levelNum + ".png");
		File.Delete (path + "level" + levelNum + ".png");
	}

	public void drawCircle ()
	{
		Statics.drawCircle = true;
		showDrawButtons ();
	}

	public void drawRectangle ()
	{
		Statics.drawRectangle = true;
		showDrawButtons ();
	}

	public void showDrawButtons ()
	{
		var circle = FindObject ("Circle");
		if (circle.activeSelf) {
			circle.SetActive (false);
			FindObject ("Rectangle").SetActive (false);
		} else {
			circle.SetActive (true);
			FindObject ("Rectangle").SetActive (true);
		}
	}

	public void showSaveButtons ()
	{
		var lines = FindObject ("Lines");
		if (lines.activeSelf) {
			lines.SetActive (false);
			FindObject ("Dots").SetActive (false);
			FindObject ("Homotopies").SetActive (false);
		} else {
			lines.SetActive (true);
			FindObject ("Dots").SetActive (true);
			FindObject ("Homotopies").SetActive (true);
		}
	}

	private GameObject FindObject (string name)
	{
		Transform[] trs = GameObject.Find ("Canvas").GetComponentsInChildren<Transform> (true);
		foreach (Transform t in trs) {
			if (t.name == name) {
				return t.gameObject;
			}
		}
		return null;
	}

	public void DeleteObject ()
	{
		Statics.deleteObstacle = true;
	}

}
