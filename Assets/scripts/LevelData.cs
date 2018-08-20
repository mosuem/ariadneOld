using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using UnityEngine.UI;

public class LevelData : MonoBehaviour
{
	public List<Path> paths = new List<Path> ();
	public List<IEnumerator> particleSystems = new List<IEnumerator> ();
	public List<GameObject> dots = new List<GameObject> ();
	public List<Path> homotopyPaths = new List<Path> ();
	public List<Homotopy> homotopies = new List<Homotopy> ();
	public Material pathMat;
	public Material homotopyMat;
	public Material trailMat;
	public Material manifoldMat;
	public int levelNumber;
	private string[] myHexColors = {
		"#E57373",//Red
		"#F06292",//Pink
		"#BA68C8",//Purple
		"#9575CD",//Deep Purple
		"#7986CB",//Indigo
		"#64B5F6",//Blue
		"#4FC3F7",//Light Blue
		"#4DD0E1",//Cyan
		"#4DB6AC",//Teal
		"#81C784",//Green
		"#AED581",//Light Green
		//"#DCE775",//lime
		"#FF8A65",//Deep Orange
		"#A1887F",//Brown
	};
	private List<Color> myColors = new List<Color> ();
	public bool dotSettingAllowed;
	public bool pathDrawingAllowed;
	public bool homotopiesAllowed;
	public bool concatAllowed;
	public bool staticsAllowed;
	private LevelObjects levelObjects = new LevelObjects ();
	public PathFactory pathFactory;
	private HomotopyFactory homotopyFactory;
	public GameObject dotPrefab;
	public GameObject staticCirclePrefab;
	public GameObject staticRectPrefab;
	public GameObject arrowPrefab;
	public GameObject textPrefab;
	public GameObject canvas;
	public GameObject hintCanvas;
	public List<GameObject> statics = new List<GameObject> ();
	public List<List<int>> dotHomClasses = new List<List<int>> ();
	public List<List<int>> pathHomClasses = new List<List<int>> ();
	private List<Dictionary<int, int>> windingNumbersForObstacles = new List<Dictionary<int, int>> ();
	private GameObject homTextObj;


	public GameObject dotPrefab3D;

	// Use this for initialization
	void Awake ()
	{
		Statics.folderPath = Application.streamingAssetsPath + "\\";
		setAllowedOperations ();
		if (Statics.levelType.Equals ("all") || (Statics.levelType.Equals ("sphere") || Statics.levelType.Equals ("torus"))) {
			GameObject.Find ("Lines").SetActive (false);
			GameObject.Find ("Dots").SetActive (false);
			GameObject.Find ("Homotopies").SetActive (false);

			GameObject.Find ("Circle").SetActive (false);
			GameObject.Find ("Rectangle").SetActive (false);
			GameObject.Find ("Exit2").SetActive (false);
		} else {
			GameObject.Find ("Lines").SetActive (false);
			GameObject.Find ("Dots").SetActive (false);
			GameObject.Find ("Homotopies").SetActive (false);
			GameObject.Find ("Exit1").SetActive (false);

			GameObject.Find ("Circle").SetActive (false);
			GameObject.Find ("Rectangle").SetActive (false);

			GameObject.Find ("Save").SetActive (false);
			GameObject.Find ("Draw").SetActive (false);
			GameObject.Find ("Delete").SetActive (false);
		}
		if (Statics.levelType.Equals ("dots")) {
			GameObject.Find ("Windung").SetActive (false);
		}
		levelNumber = Statics.nextSceneNumber;
		pathFactory = new PathFactory (pathMat, arrowPrefab);
		homotopyFactory = new HomotopyFactory (pathMat, homotopyMat);
		setColors ();

		homTextObj = Instantiate (textPrefab);
		homTextObj.transform.SetParent (canvas.transform);
		var position = new Vector3 (Screen.width / 2, Screen.height - 50);
		homTextObj.transform.position = position;

		LoadObjects (Statics.nextSceneNumber);
		trailMat.SetColor ("_Color", GetRandomColor ());
	}

	void setAllowedOperations ()
	{
		switch (Statics.levelType) {
		case "all":
			dotSettingAllowed = true;
			pathDrawingAllowed = true;
			homotopiesAllowed = true;
			staticsAllowed = true;
			concatAllowed = true;
			break;
		case "paths":
			dotSettingAllowed = true;
			pathDrawingAllowed = true;
			homotopiesAllowed = false;
			staticsAllowed = false;
			concatAllowed = true;
			break;
		case "dots":
			dotSettingAllowed = true;
			pathDrawingAllowed = false;
			homotopiesAllowed = false;
			staticsAllowed = false;
			concatAllowed = true;
			break;
		case "homotopies":
			dotSettingAllowed = false;
			pathDrawingAllowed = false;
			homotopiesAllowed = true;
			staticsAllowed = false;
			concatAllowed = true;
			break;
		default:
			Statics.levelType = "all";
			dotSettingAllowed = true;
			pathDrawingAllowed = true;
			homotopiesAllowed = true;
			staticsAllowed = true;
			concatAllowed = true;
			break;
		}
	}

	void setColors ()
	{
		foreach (var hex in myHexColors) {
			Color color;
			ColorUtility.TryParseHtmlString (hex, out color);
			myColors.Add (color);
		}
	}

	public List<Vector3> GetStaticPositions ()
	{
		var points = new List<Vector3> ();
		for (int i = 0; i < statics.Count; i++) {
			points.Add (statics [i].transform.position);
		}
		return points;
	}


	public Color GetRandomColor ()
	{
		if (myColors.Count > 0) {
			var rand = Random.Range (0, myColors.Count);
			var color = myColors [rand];
			myColors.RemoveAt (rand);
			return color;
		} else {
			setColors ();
			return GetRandomColor ();
		}
	}

	public Path NewPath (Color notThisColor, GameObject from, GameObject to)
	{
		var color = GetRandomColor ();
		Debug.Log ("Returning new Path with color " + color + " from " + from + " to " + to);
		return pathFactory.newPath (color, from, to);
	}

	public Path NewPath (Color color, string name, GameObject from, GameObject to)
	{
		return pathFactory.newPath (color, name, from, to);
	}

	public Homotopy NewHomotopy (Path p1, Path midPath)
	{
		return homotopyFactory.newHomotopy (p1, midPath);
	}

	private bool LoadObjects (int levelNum)
	{
		Debug.Log ("Load Objects for level " + levelNum);
		string filePath = Statics.folderPath + Statics.levelType + "/" + "level" + levelNum + ".dat";
		if (File.Exists (filePath)) {
			using (Stream stream = File.Open (filePath, FileMode.Open)) {
				var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
				levelObjects = (LevelObjects)bformatter.Deserialize (stream);
				Debug.Log ("Load Dots");
				var numDots = levelObjects.dotColors.Count;
				for (int i = 0; i < numDots; i++) {
					Color color;
					ColorUtility.TryParseHtmlString (levelObjects.dotColors [i], out color);
					myColors.Remove (color);
					var dot = Instantiate (dotPrefab);
					dot.transform.position = levelObjects.dotPositions [i];
					dot.GetComponent<SpriteRenderer> ().color = color;
					dot.GetComponentInChildren <TrailRenderer> ().Clear ();
					dot.GetComponent <SpriteRenderer> ().sortingOrder = 10;
					addDot (dot);
				}
				Debug.Log ("Load Paths");
				for (int i = 0; i < levelObjects.paths.Count; i++) {
					Debug.Log ("Load Path " + i);
					Color color;
					ColorUtility.TryParseHtmlString (levelObjects.pathColors [i], out color);
					myColors.Remove (color);
					var dotNumber1 = levelObjects.dotsFrom [i];
					var dotNumber2 = levelObjects.dotsTo [i];
					var path = NewPath (color, dots [dotNumber1], dots [dotNumber2]);
					var loadedPath = levelObjects.paths [i];
					for (int j = 0; j < loadedPath.Count; j++) {
						path.line.SetPosition (j, loadedPath [j]);
					}
					addPath (path);
				}

				Debug.Log ("Load Statics");
				for (int i = 0; i < levelObjects.statics.Count; i++) {
					Debug.Log ("Load Static " + i);
					var myStatic = levelObjects.statics [i];
					GameObject gameObject;
					if (myStatic.type == 0) {
						gameObject = GameObject.Instantiate (staticRectPrefab);
					} else if (myStatic.type == 1) {
						gameObject = GameObject.Instantiate (staticCirclePrefab);
					} else {
						gameObject = GameObject.Instantiate (staticRectPrefab);
					}
					gameObject.transform.position = myStatic.position;
					gameObject.transform.localScale = myStatic.size;
					statics.Add (gameObject);

					if (Statics.showingAlgebra) {
						AddStaticAlgebra (i, gameObject);
					}
				}
			
				for (int i = 0; i < paths.Count; i++) {
					for (int j = i + 1; j < paths.Count; j++) {
						CheckIfHomotopic (i, j);
					}
				}
			}
		}
		return true;
	}

	public void DeletePath (Path path2)
	{
		var pathNum = paths.IndexOf (path2);
		Debug.Log ("Delete Path " + pathNum);

		List<int> hom = null;
		foreach (var homClass in pathHomClasses) {
			if (homClass.Contains (pathNum)) {
				hom = homClass;
				break;
			}
		}

		if (hom != null) {
			hom.Remove (pathNum);
			if (hom.Count == 0) {
				pathHomClasses.Remove (hom);
			}
		}
		paths.Remove (path2);
		particleSystems.RemoveAt (pathNum);
		path2.Clear ();

		UpdateHomAlgebra ();
	}

	void UpdateHomAlgebra ()
	{
		Text text = homTextObj.GetComponentInChildren<Text> ();
		string str = "";
		for (int k = 0; k < pathHomClasses.Count; k++) {
			var group = pathHomClasses [k];
			for (int j = 0; j < group.Count; j++) {
				var i = group [j];
				str += "[f" + i + "]";
				if (j < group.Count - 1) {
					str += " = ";
				}
			}
			if (k < pathHomClasses.Count - 1) {
				str += ", ";
			}
		}
		text.text = str;
	}

	void AddDotAlgebra (int i, GameObject dot)
	{
		Debug.Log ("Set Text " + i);
		var textObj = Instantiate (textPrefab);
		Text text = textObj.GetComponentInChildren<Text> ();
		text.text = "p" + i;
		textObj.transform.SetParent (canvas.transform);
		var position = Camera.main.WorldToScreenPoint (dot.transform.position + new Vector3 (1, 1, 0) * dotPrefab.transform.localScale.x / 2f);
		textObj.transform.position = position;
	}

	void AddPathAlgebra (int i, Path path)
	{
		if (path.Count > 3) {
			Debug.Log ("Set Text " + i);
			var textObj = Instantiate (textPrefab);
			Text text = textObj.GetComponentInChildren<Text> ();
			var str = "f" + i;
			if (path.dotFrom.Equals (path.dotTo)) {
				str += "; ";
				var dictionary = windingNumbersForObstacles [i];
				for (int j = 0; j < statics.Count; j++) {
					var obstacle = statics [j];
					str += "Ind(x" + j + ") = " + dictionary [j] + ", ";
				}
			}
			text.text = str;
			textObj.transform.SetParent (canvas.transform);
			Vector3 normal = Vector3.zero;
			var k = path.Count / 2;
			Debug.Log (path.Count);
			Debug.Log (k);
			normal = Vector3.Cross (path.line.GetPosition (k) - path.line.GetPosition (k - 1), Vector3.forward);
			var position = Camera.main.WorldToScreenPoint (path.line.GetPosition (k) + Statics.lineThickness * 2f * normal.normalized);
			textObj.transform.position = position + Vector3.right * dotPrefab.transform.lossyScale.x;
			UpdateHomAlgebra ();
		}
	}

	void AddStaticAlgebra (int i, GameObject gameObject)
	{
		Debug.Log ("Set Text " + i);
		var textObj = Instantiate (textPrefab);
		Text text = textObj.GetComponentInChildren<Text> ();
		text.text = "x" + i;
		textObj.transform.SetParent (canvas.transform);
		text.color = Color.white;
		var position = Camera.main.WorldToScreenPoint (gameObject.transform.position);
		textObj.transform.position = position + Vector3.right * dotPrefab.transform.lossyScale.x;
	}

	public bool CheckIfHomotopic (int i, int j)
	{
		if (paths [i].dotFrom.Equals (paths [j].dotFrom) && paths [i].dotTo.Equals (paths [j].dotTo) && CheckIfPathsHomotopic (i, j)) {
			List<int> group1 = new List<int> ();
			List<int> group2 = new List<int> ();
			for (int k = 0; k < pathHomClasses.Count; k++) {
				var group = pathHomClasses [k];
				if (group.Contains (i)) {
					group1 = group;
				} else if (group.Contains (j)) {
					group2 = group;
				}
			}
			for (int k = 0; k < group2.Count; k++) {
				var item = group2 [k];
//				var pathColor = paths [group1 [0]].color;
//				paths [item].SetColor (pathColor);
				group1.Add (item);
			}
			pathHomClasses.Remove (group2);
			Debug.Log ("Paths " + i + " and " + j + " are homotopic");
			return true;
		} else {
			Debug.Log ("Paths " + i + " and " + j + " are not homotopic");
			return false;
		}
	}

	private bool CheckIfPathsHomotopic (int i, int j)
	{
		var path1 = paths [i];
		var path2 = paths [j];
		if (path1.canonicalPath != null && path1.canonicalPath.SequenceEqual (path2.canonicalPath)) {
			return true;
		} else {
			return false;
		}
//		var windingNumbersForPath1 = windingNumbersForObstacles [i];
//		var windingNumbersForPath2 = windingNumbersForObstacles [j];
//		List<Vector3> combinedPath = getCombinedPath (i, j);
//		for (int k = 0; k < statics.Count; k++) {
//			var gameObject = statics [k];
//			var gameObjectPos = gameObject.transform.position;
//			var windingNumberPath1 = windingNumbersForPath1 [k];
//			var windingNumberPath2 = windingNumbersForPath2 [k];
//			if (windingNumberPath1 != windingNumberPath2) {
//				Debug.Log ("Paths " + paths [i].pathNumber + " and " + paths [j].pathNumber + " are not homotopic, as the winding Number around " + gameObjectPos + " of Path 1 is " + windingNumberPath1 + " and of Path 2 is " + windingNumberPath2);
//				return false;
//			} else if (ContainsPoint (combinedPath, gameObjectPos)) {
//				Debug.Log ("Paths " + paths [i].pathNumber + " and " + paths [j].pathNumber + " are not homotopic, as they enclose the gameObject at " + gameObjectPos);
//				return false;
//			}
//		}
//		Debug.Log ("Paths " + paths [i].pathNumber + " and " + paths [j].pathNumber + " are homotopic, they do not enclose one of the " + statics.Count + " gameObjects");
//		return true;
	}

	List<Vector3> getCombinedPath (int i, int j)
	{
		var path1 = paths [i];
		var path2 = paths [j];
		List<Vector3> vertices = new List<Vector3> ();
		for (int k = 0; k < path1.Count; k++) {
			vertices.Add (path1.line.GetPosition (k));
		}
		for (int k = path2.Count - 1; k >= 0; k--) {
			vertices.Add (path2.line.GetPosition (k));
		}
		return vertices;
	}

	static bool ContainsPoint (List<Vector3> polyPoints, Vector3 p)
	{ 
		var j = polyPoints.Count - 1; 
		var inside = false; 
		for (int i = 0; i < polyPoints.Count; j = i++) { 
			if (((polyPoints [i].y <= p.y && p.y < polyPoints [j].y) || (polyPoints [j].y <= p.y && p.y < polyPoints [i].y)) &&
			    (p.x < (polyPoints [j].x - polyPoints [i].x) * (p.y - polyPoints [i].y) / (polyPoints [j].y - polyPoints [i].y) + polyPoints [i].x))
				inside = !inside; 
		} 
		return inside; 
	}

	//	public float getWindingNumber (List<Vector3> polyPoints, Vector3 p)
	//	{
	//		float w = 0f;
	//
	//		for (int i = 0; i < polyPoints.Count; i++) {
	//			var vecN = polyPoints [i];
	//			Vector3 vecNPlus1;
	//			if (i + 1 == polyPoints.Count) {
	//				vecNPlus1 = polyPoints [0];
	//			} else {
	//				vecNPlus1 = polyPoints [i + 1];
	//			}
	//			w += Vector3.Angle (p - vecN, p - vecNPlus1) * Mathf.Deg2Rad;
	//		}
	//
	//		w *= 1f / (2f * Mathf.PI);
	//		return w;
	//	}

	public static int WindingNumber (List<Vector3> poly, Vector3 p)
	{
		int n = poly.Count;

		poly.Add (new Vector3 { x = poly [0].x, y = poly [0].y });
		Vector3[] v = poly.ToArray ();

		var wn = 0;    // the winding number counter

		// loop through all edges of the polygon
		for (int i = 0; i < n; i++) {   // edge from V[i] to V[i+1]
			if (v [i].x <= p.x) {         // start y <= P.y
				if (v [i + 1].x > p.x)      // an upward crossing
				if (isLeft (v [i], v [i + 1], p) > 0)  // P left of edge
					++wn;            // have a valid up intersect
			} else {                       // start y > P.y (no test needed)
				if (v [i + 1].x <= p.x)     // a downward crossing
				if (isLeft (v [i], v [i + 1], p) < 0)  // P right of edge
					--wn;            // have a valid down intersect
			}
		}
//		if (wn != 0)
//			return true;
//		else
//			return false;
		return wn;
	}

	private static int isLeft (Vector3 P0, Vector3 P1, Vector3 P2)
	{
		var calc = ((P1.y - P0.y) * (P2.x - P0.x)
		           - (P2.y - P0.y) * (P1.x - P0.x));
		if (calc > 0)
			return 1;
		else if (calc < 0)
			return -1;
		else
			return 0;
	}

	public void addDot (GameObject dot)
	{
		var dotGroup = new List<int> ();
		dotGroup.Add (dots.Count);
		dotHomClasses.Add (dotGroup);

		dot.GetComponentInChildren <TrailRenderer> ().Clear ();
		dots.Add (dot);
		if (Statics.showingAlgebra) {
			AddDotAlgebra (dots.Count - 1, dot);
		}
	}

	public void addPath (Path path)
	{
		var pathGroup = new List<int> ();
		pathGroup.Add (paths.Count);
		pathHomClasses.Add (pathGroup);

		paths.Add (path);

		Dictionary<int, int> dict = new Dictionary<int, int> ();
		var list = path.line.GetPositions ();
		for (int i = 0; i < statics.Count; i++) {
			dict.Add (i, WindingNumber (list, statics [i].transform.position));
		}
		windingNumbersForObstacles.Add (dict);

		ConstructCanonicalPath (path);
		var canstring = "";
		foreach (var num in path.canonicalPath) {
			canstring += num + " ";
		}
		Debug.Log ("Canonical path " + path.pathNumber + " is: " + canstring);

		for (int i = 0; i < paths.Count - 1; i++) {
			CheckIfHomotopic (i, paths.Count - 1);
		}

		if (Statics.showingAlgebra) {
			AddPathAlgebra (paths.Count - 1, path);
		}
	}

	public void RecalculateHomClasses ()
	{
		dotHomClasses.Clear ();
		pathHomClasses.Clear ();
		windingNumbersForObstacles.Clear ();
		for (int i = 0; i < dots.Count; i++) {
			var list = new List<int> ();
			list.Add (i);
			dotHomClasses.Add (list);
		}
		for (int i = 0; i < paths.Count; i++) {
			var path = paths [i];
			ConstructCanonicalPath (path);
			var pathGroup = new List<int> ();
			pathGroup.Add (i);
			pathHomClasses.Add (pathGroup);
			//Dot Homotopy classes
			var dotNumber1 = dots.IndexOf (path.dotFrom);
			var dotNumber2 = dots.IndexOf (path.dotTo);
			List<int> group1 = new List<int> ();
			List<int> group2 = new List<int> ();
			for (int j = 0; j < dotHomClasses.Count; j++) {
				var group = dotHomClasses [j];
				if (group.Contains (dotNumber1)) {
					group1 = group;
				} else if (group.Contains (dotNumber2)) {
					group2 = group;
				}
			}
			for (int j = 0; j < group2.Count; j++) {
				var item = group2 [j];
				var dotColor = dots [group1 [0]].GetComponent<SpriteRenderer> ().color;
				dots [item].GetComponent<SpriteRenderer> ().color = dotColor;
				group1.Add (item);
			}
			dotHomClasses.Remove (group2);
			//Winding Numbers
			Dictionary<int, int> dict = new Dictionary<int, int> ();
			for (int j = 0; j < statics.Count; j++) {
				dict.Add (j, WindingNumber (path.line.GetPositions (), statics [j].transform.position));
			}
			windingNumbersForObstacles.Add (dict);
		}
		for (int i = 0; i < paths.Count; i++) {
			for (int j = i + 1; j < paths.Count; j++) {
				CheckIfHomotopic (i, j);
			}
		}
	}

	public void ConstructCanonicalPath (Path path)
	{
		path.canonicalPath = new List<int> ();
		for (int j = 0; j < path.Count - 1; j++) {
			var point1 = path.line.GetPosition (j);
			var point2 = path.line.GetPosition (j + 1);

			var comparedXIndex = CompareXIndex (point1, point2);
			if (comparedXIndex != -1) {
				bool over = overObstacle (comparedXIndex, point1, point2);
				if (over) {
					path.canonicalPath.Add (comparedXIndex * 2 + 1);
				} else {
					path.canonicalPath.Add (comparedXIndex * 2);
				}
			}
		}
		var can = path.canonicalPath;
		int i = 0;
		while (i < can.Count - 1) {
			if (can [i] == can [i + 1]) {
				can.RemoveAt (i);
				can.RemoveAt (i);
				if (i > 0) {
					i = i - 1;
				}
			} else {
				i++;
			}
		}
	}

	int CompareXIndex (Vector3 point1, Vector3 point2)
	{
		for (int i = 0; i < statics.Count; i++) {
			var gameObject = statics [i];
			if (gameObject.GetComponent<Collider> ().GetType () == typeof(SphereCollider)) {
				var position = gameObject.transform.position;
				if (point1.x < position.x && point2.x >= position.x) {
					return i;
				} else if (point2.x < position.x && point1.x >= position.x) {
					return i;
				} else if (point1.x < position.x && point2.x < position.x) {
					return -1;
				}
			}
		}
		return -1;
	}

	bool overObstacle (int comparedXIndex, Vector3 point1, Vector3 point2)
	{
		var gameObject = statics [comparedXIndex];
		if (point2.y > gameObject.transform.position.y) {
			return true;
		} else {
			return false;
		}
	}

	public void SaveObjects (string levelType)
	{
//		int count = Misc.DirCount (levelType, "dat");
		SaveObjects (levelType, Misc.MaxLevel (levelType) + 1);
	}

	public void SaveObjects (string levelType, int levelNum)
	{
		Debug.Log ("Save Objects to " + Statics.folderPath + levelType + "/" + "level" + levelNum + ".dat");
		levelObjects = new LevelObjects ();
		string filePath = Statics.folderPath + levelType + "/" + "level" + levelNum + ".dat";
		ScreenCapture.CaptureScreenshot (Statics.folderPath + levelType + "/" + "level" + levelNum + ".png");
		Debug.Log ("Save Dots");
		for (int i = 0; i < dots.Count; i++) {
			myVector3 position = dots [i].transform.position;
			levelObjects.dotPositions.Add (position);
			var color = dots [i].GetComponent<SpriteRenderer> ().color;
			levelObjects.dotColors.Add ("#" + ColorUtility.ToHtmlStringRGB (color));
		}
		Debug.Log ("Save Paths");
		for (int i = 0; i < paths.Count; i++) {
			Debug.Log ("Save Path " + i);
			levelObjects.paths.Add (new List<myVector3> ());
			var path = paths [i];
			for (int k = 0; k < path.Count; k++) {
				levelObjects.paths [i].Add (path.line.GetPosition (k));
			}
			var line = paths [i].line;
			var color = line.GetColor ();
			levelObjects.pathColors.Add ("#" + ColorUtility.ToHtmlStringRGB (color));
			Debug.Log ("Save From: " + path.dotFrom + " at " + dots.IndexOf (path.dotFrom));
			levelObjects.dotsFrom.Add (dots.IndexOf (path.dotFrom));
			Debug.Log ("Save To: " + path.dotTo + " at " + dots.IndexOf (path.dotTo));
			levelObjects.dotsTo.Add (dots.IndexOf (path.dotTo));
		}

		//		Debug.Log ("Save Homotopies");

		Debug.Log ("Save Statics");
		for (int i = 0; i < statics.Count; i++) {
			Debug.Log ("Save Static " + i);
			levelObjects.statics.Add (new myStatic (statics [i]));
		}
		FileStream fs = new FileStream (filePath, FileMode.Create);
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (fs, levelObjects);
		fs.Close ();
	}

	public Homotopy HomotopyExists (int path1, int path2)
	{
//		for (int i = 0; i < homotopies.Count; i++) {
//			if (homotopies [i].path1.pathNumber == path1 && homotopies [i].path2.pathNumber == path2) {
//				return homotopies [i];
//			}
//		}
		return null;
	}


	public List<int> GetHomotopyClass (int indexOfPath1)
	{
		foreach (var item in pathHomClasses) {
			Debug.Log ("Homclass " + string.Join (",", item.Select (x => x.ToString ()).ToArray ()));
			if (item.Contains (indexOfPath1)) {
				return item;
			}
		}
		return null;
	}

	public void HideHintCanvas ()
	{
		hintCanvas.SetActive (false);
		Statics.hintCanvasActive = false;
	}

	public void ShowHintCanvas ()
	{
		if (Statics.showingHints) {
			hintCanvas.SetActive (true);
			Statics.hintCanvasActive = true;
		}
	}

	public static void showHint (string hint)
	{
		if (Statics.showingHints && Statics.hintCanvasActive) {
			GameObject.Find ("HintCanvas").GetComponentInChildren<Text> ().text = hint;
		}
	}
}

[System.Serializable]
class LevelObjects
{
	public List<myVector3> dotPositions = new List<myVector3> ();
	public List<string> dotColors = new List<string> ();
	public List<List<myVector3>> paths = new List<List<myVector3>> ();
	public Dictionary<int, int> homotopies = new Dictionary<int, int> ();
	public List<string> pathColors = new List<string> ();
	public List<int> dotsFrom = new List<int> ();
	public List<int> dotsTo = new List<int> ();
	public List<myStatic> statics = new List<myStatic> ();
}

[System.Serializable]
class myStatic
{
	public myVector3 position;
	public myVector3 size;
	public int type;
	//1= circle, 0 = rect

	public myStatic (GameObject obj)
	{
		position = obj.transform.position;
		size = obj.transform.localScale;
		if (obj.GetComponent<Collider> ().GetType () == typeof(SphereCollider)) {
			type = 1;
		} else if (obj.GetComponent<Collider> ().GetType () == typeof(BoxCollider)) {
			type = 0;
		} else {
			type = 0;
		}
	}
}


[System.Serializable]
struct myVector3
{
	public float x;
	public float y;
	public float z;

	public myVector3 (float pX, float pY, float pZ)
	{
		x = pX;
		y = pY;
		z = pZ;
	}

	public static implicit operator Vector3 (myVector3 rValue)
	{
		return new Vector3 (rValue.x, rValue.y, rValue.z);
	}


	/// <summary>
	/// Automatic conversion from Vector3 to SerializableVector3
	/// </summary>
	/// <param name="rValue"></param>
	/// <returns></returns>
	public static implicit operator myVector3 (Vector3 rValue)
	{
		return new myVector3 (rValue.x, rValue.y, rValue.z);
	}

}
