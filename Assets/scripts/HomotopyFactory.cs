using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomotopyFactory
{
	Material pathMaterial;
	Material homotopyMaterial;

	public HomotopyFactory (Material pathMaterial, Material homotopyMaterial)
	{
		this.pathMaterial = pathMaterial;
		this.homotopyMaterial = homotopyMaterial;
	}

	public Homotopy newHomotopy (Path path1, Path midPath)
	{
		Homotopy hom = new Homotopy (path1, midPath, homotopyMaterial);
		Debug.Log (midPath.ToString ());
		Debug.Log (midPath.line.ToString ());
		Debug.Log (midPath.Count.ToString ());

		var color = path1.color;
		midPath.line.SetColor (color);
		color.a = color.a / 2f;
		path1.SetColor (color);
		//		for (int i = 0; i < Statics.numHomotopyLines; i++) {
//			GameObject lineObj = new GameObject ();
//			var line = lineObj.AddComponent<LineRenderer> ();
//			line.positionCount = hom.counter;
//			line.startWidth = Statics.lineThickness / 3f;
//			line.endWidth = Statics.lineThickness / 3f;
//			line.numCornerVertices = 0;
//			line.GetComponent<Renderer> ().material = pathMaterial;
//			line.GetComponent<Renderer> ().material.SetColor ("_Color", Statics.homotopyColor);
////			hom.homotopyLines.Add (line);
//		}
		for (int i = 0; i < path1.Count; i++) {
			midPath.line.SetPosition (i, path1.line.GetPosition (i));
		}
		midPath.line.SetMesh ();
//		int counter = 1;
//		float sum = 0f;
//		var length = midPath.Length ();
//		for (int i = 0; i < count - 1; i++) {
//			if (sum > counter * (length / (Statics.numHomotopyLines + 1))) {
//				var line = hom.homotopyLines [counter - 1];
//				line.SetPosition (0, midPath.line.GetPosition (i));
//				counter++;
//			}
//			sum += Vector3.Distance (midPath.line.GetPosition (i), midPath.line.GetPosition (i + 1));
//		}
		return hom;
	}

}
