using System.Collections.Generic;
using System;
using UnityEngine;


public class DouglasPeucker
{

	/// <summary>
	/// Uses the Douglas Peucker algorithm to reduce the number of points.
	/// </summary>
	/// <param name="Points">The points.</param>
	/// <param name="Tolerance">The tolerance.</param>
	/// <returns></returns>
	public static List<Vector3> DouglasPeuckerReduction
	(List<Vector3> Points, Double Tolerance)
	{
		if (Points == null || Points.Count < 3)
			return Points;

		Int32 firstPoint = 0;
		Int32 lastPoint = Points.Count - 1;
		List<Int32> pointIndexsToKeep = new List<Int32> ();

		//Add the first and last index to the keepers
		pointIndexsToKeep.Add (firstPoint);
		pointIndexsToKeep.Add (lastPoint);

		//The first and the last point cannot be the same
		while (Points [firstPoint].Equals (Points [lastPoint])) {
			lastPoint--;
		}

		DouglasPeuckerReduction (Points, firstPoint, lastPoint, 
			Tolerance, ref pointIndexsToKeep);

		List<Vector3> returnPoints = new List<Vector3> ();
		pointIndexsToKeep.Sort ();
		foreach (Int32 index in pointIndexsToKeep) {
			returnPoints.Add (Points [index]);
		}

		return returnPoints;
	}

	/// <summary>
	/// Douglases the peucker reduction.
	/// </summary>
	/// <param name="points">The points.</param>
	/// <param name="firstVector3">The first point.</param>
	/// <param name="lastPoint">The last point.</param>
	/// <param name="tolerance">The tolerance.</param>
	/// <param name="pointIndexsToKeep">The point index to keep.</param>
	private static void DouglasPeuckerReduction (List<Vector3> 
		points, Int32 firstPoint, Int32 lastPoint, Double tolerance, 
	                                             ref List<Int32> pointIndexsToKeep)
	{
		Double maxDistance = 0;
		Int32 indexFarthest = 0;

		for (Int32 index = firstPoint; index < lastPoint; index++) {
			Double distance = PerpendicularDistance
				(points [firstPoint], points [lastPoint], points [index]);
			if (distance > maxDistance) {
				maxDistance = distance;
				indexFarthest = index;
			}
		}

		if (maxDistance > tolerance && indexFarthest != 0) {
			//Add the largest point that exceeds the tolerance
			pointIndexsToKeep.Add (indexFarthest);

			DouglasPeuckerReduction (points, firstPoint, 
				indexFarthest, tolerance, ref pointIndexsToKeep);
			DouglasPeuckerReduction (points, indexFarthest, 
				lastPoint, tolerance, ref pointIndexsToKeep);
		}
	}

	/// <summary>
	/// The distance of a point from a line made from point1 and point2.
	/// </summary>
	/// <param name="pt1">The PT1.</param>
	/// <param name="pt2">The PT2.</param>
	/// <param name="p">The p.</param>
	/// <returns></returns>
	public static Double PerpendicularDistance
	(Vector3 Point1, Vector3 Point2, Vector3 Point)
	{
		float line_dist = Vector3.Distance (Point1, Point2);
		if (line_dist == 0)
			return Vector3.Distance (Point, Point1);
		float t = ((Point.x - Point1.x) * (Point2.x - Point1.x) + (Point.y - Point1.y) * (Point2.y - Point1.y) + (Point.z - Point1.z) * (Point2.z - Point1.z)) / line_dist;
		t = Mathf.Clamp01 (t);
		return Vector3.Distance (Point, new Vector3 (Point1.x + t * (Point2.x - Point1.x), Point1.y + t * (Point2.y - Point1.y), Point1.z + t * (Point2.z - Point1.z)));
	}
}
