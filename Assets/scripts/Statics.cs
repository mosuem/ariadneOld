using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Statics
{
	public static bool checkPath = false;

	public static bool retractPath = false;

	public static bool showWindingNumber = false;

	public static bool showingSpiral = false;

	public static int LineSortingOrder = 0;

	public static int nextSceneNumber = 0;
	public static string levelType;
	public static bool isLoading = false;
	public static bool drawCircle = false;
	public static bool drawRectangle = false;
	public static bool deleteObstacle = false;
	public const float lineThickness = 0.15f;
	public const float lineRadius = lineThickness / 2f;
	public const float homotopyNearness = Statics.lineThickness * 1.5f;
	public static Vector3 lineDepth = Vector3.back * 0.5f;
	public static Vector3 outlineDepth = Vector3.back * 0.25f;
	public static Vector3 dotDepth = Vector3.back;
	public static Color outlineColor = Color.magenta;
	public static Color homotopyColor = Color.red;
	public static int numHomotopyLines = 9;
	public static string folderPath;
	public static float meanDist = 0.05f;
	//0.025f;
	public static float coarseDist = 0.1f;

	public static float bundleDist = 1f;

	public static float dotSpacer = 0.05f;
	public static float pathSpacer = 0.02f;
	public static float meshSpacer = 0.01f;


	public static float sphereRadius = 5f;

	public static float torusRadius1 = 4f;
	public static float torusRadius2 = 2f;

	public static float dotMoveSpeed = 1f;


	public static bool showingAlgebra = false;

	public static bool showingHints = true;

	public static bool hintCanvasActive = false;

	public static bool firstPerson = false;

	public static bool mesh = true;

	public static float smoothFactor = 1.5f;
	//1f is always smooth, larger is less smoothing

	public static bool isSphere = false;
	public static bool isTorus = false;

	public static bool isDragging = false;

	public static float arrowDist = 2;

	public static float maxMoveDistance = 0.1f;

	public static float dotRadius = 0.4f;

	//	public enum LevelType
	//	{
	//		Everything,
	//		Dots,
	//		Lines,
	//		Homtopies}
	//
	//	;
}
