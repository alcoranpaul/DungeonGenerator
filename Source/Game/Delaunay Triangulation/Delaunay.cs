using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// Generate a Delaunay triangulation from a list of points
/// <para>
/// Reference: https://en.wikipedia.org/wiki/Bowyer–Watson_algorithm
/// </para>
/// </summary>
public class Delaunay
{

	public List<Triangle> Triangulation { get; private set; }
	public List<Edge> Edges { get; private set; }

	public Delaunay()
	{
		Triangulation = new List<Triangle>();
	}

	/// <summary>
	/// Triangulate a list of points
	/// </summary>
	/// <param name="points"></param>
	/// <returns></returns>
	public static Delaunay Triangulate(List<Point> points)
	{
		Delaunay delaunay = new Delaunay();
		delaunay.Generate(points);
		return delaunay;
	}

	/// <summary>
	/// Generate the Delaunay triangulation
	/// </summary>
	/// <param name="points"></param>
	public void Generate(List<Point> points)
	{
		// Calculate bounding box using the input points
		float minX = float.MaxValue, minY = float.MaxValue;
		float maxX = float.MinValue, maxY = float.MinValue;

		foreach (var p in points)
		{
			if (p.X < minX) minX = p.X;
			if (p.Y < minY) minY = p.Y;
			if (p.X > maxX) maxX = p.X;
			if (p.Y > maxY) maxY = p.Y;
		}

		// Define a supertriangle that encompasses the input points
		float dx = maxX - minX;
		float dy = maxY - minY;
		float deltaMax = Math.Max(dx, dy) * 2;

		Point superA = new Point(minX - deltaMax, minY - deltaMax);
		Point superB = new Point(maxX + deltaMax, minY - deltaMax);
		Point superC = new Point(minX + deltaMax, maxY + deltaMax);

		Triangle superTriangle = new Triangle(superA, superB, superC);
		Triangulation.Add(superTriangle);


		foreach (var point in points)
		{
			InsertPoint(point);
		}

		// Cleanup: Remove triangles that contain vertices of the supertriangle
		Triangulation.RemoveAll(triangle =>
			triangle.A == superA || triangle.A == superB || triangle.A == superC ||
			triangle.B == superA || triangle.B == superB || triangle.B == superC ||
			triangle.C == superA || triangle.C == superB || triangle.C == superC
		);


		// For Debugging
		// foreach (var triangle in Triangulation)
		// {
		// 	DebugDraw.DrawLine(triangle.A.VPoint, triangle.B.VPoint, Color.Red, 16f);
		// 	DebugDraw.DrawLine(triangle.B.VPoint, triangle.C.VPoint, Color.Red, 16f);
		// 	DebugDraw.DrawLine(triangle.C.VPoint, triangle.A.VPoint, Color.Red, 16f);

		// 	BoundingSphere sphereA = new BoundingSphere(triangle.A.VPoint, 0.5f);
		// 	BoundingSphere sphereB = new BoundingSphere(triangle.B.VPoint, 0.5f);
		// 	BoundingSphere sphereC = new BoundingSphere(triangle.C.VPoint, 0.5f);
		// 	DebugDraw.DrawSphere(sphereA, Color.Red, 16f);
		// 	DebugDraw.DrawSphere(sphereB, Color.Red, 16f);
		// 	DebugDraw.DrawSphere(sphereC, Color.Red, 16f);
		// }


	}

	/// <summary>
	/// Insert a point into the triangulation
	/// </summary>
	/// <param name="point"></param>
	private void InsertPoint(Point point)
	{
		List<Triangle> badTriangles = new List<Triangle>();
		foreach (var triangle in Triangulation) // for each triangle in triangulation do
		{
			//  if point is inside circumcircle of triangle
			if (triangle.IsPointInCircumcircle(point))
			{
				badTriangles.Add(triangle); //  add triangle to badTriangles
			}
		}

		List<Edge> polygon = new List<Edge>();

		for (int i = 0; i < badTriangles.Count; i++) // for each triangle in badTriangles do
		{
			// Get the edges of the triangle
			Edge[] edges =
			[
				new Edge(badTriangles[i].A, badTriangles[i].B),
				new Edge(badTriangles[i].B, badTriangles[i].C),
				new Edge(badTriangles[i].C, badTriangles[i].A),
			];
			foreach (Edge edge in edges) // for each edge in triangle do
			{
				bool shared = false;
				for (int k = 0; k < badTriangles.Count; k++)
				{
					if (k != i && badTriangles[k].IsEdgeInTriangle(edge))
					{
						shared = true;
						break; // Exit loop if the edge is found in another triangle
					}
				}
				if (!shared)
				{
					// add edge to polygon
					polygon.Add(edge);
				}
			}
		}


		// for each triangle in badTriangles do 
		foreach (var triangle in badTriangles)
		{
			Triangulation.Remove(triangle); // remove triangle from triangulation
		}

		//for each edge in polygon do
		foreach (var edge in polygon)
		{
			// newTri := form a triangle from edge to point
			Triangle newTriangle = new Triangle(edge.A, edge.B, point);
			//  add newTri to triangulation
			Triangulation.Add(newTriangle);
		}

	}

	public class Point
	{
		public float X;
		public float Y;
		public Vector3 VPoint => new Vector3(X, 0, Y);

		public Point(float x, float y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return $"Point: ({X}, {Y})";
		}
	}

	public class Edge
	{
		public Point A { get; private set; }
		public Point B { get; private set; }
		public Edge(Point a, Point b)
		{
			A = a;
			B = b;
		}

		public static bool operator ==(Edge a, Edge b)
		{
			return (a.A == b.A || a.A == b.B) && (a.B == b.A || a.B == b.B);
		}
		public static bool operator !=(Edge a, Edge b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Edge e)
			{
				return this == e;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return A.GetHashCode() ^ B.GetHashCode();
		}
	}

	public class Triangle
	{
		public Point A { get; }
		public Point B { get; }
		public Point C { get; }

		public Triangle(Point a, Point b, Point c)
		{
			A = a;
			B = b;
			C = c;
		}

		// Check if a point is inside the circumcircle of this triangle
		// From @ https://en.wikipedia.org/wiki/Circumcircle
		public bool IsPointInCircumcircle(Point point)
		{
			// Using the circumcircle formula
			// From matrix @ "Using the polarization identity, these equations reduce to the condition that the matrix" from wikipedia website above
			float ax = A.X - point.X;
			float ay = A.Y - point.Y;
			float bx = B.X - point.X;
			float by = B.Y - point.Y;
			float cx = C.X - point.X;
			float cy = C.Y - point.Y;

			// Determinant of the matrix
			float det = (ax * ax + ay * ay) * (bx * cy - by * cx) -
						(bx * bx + by * by) * (ax * cy - ay * cx) +
						(cx * cx + cy * cy) * (ax * by - ay * bx);

			return det > 0 || float.IsNaN(det); // true if point is inside circumcircle
		}

		public bool IsEdgeInTriangle(Edge edge)
		{
			Edge[] edges =
			[
				new Edge(A, B),
				new Edge(B, C),
				new Edge(C, A),
			];

			foreach (var e in edges)
			{
				if (e == edge) return true;
			}
			return false;
		}

		public override string ToString()
		{
			return $"Triangle: {A}, {B}, {C}";
		}
	}
}
