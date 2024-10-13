using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;


namespace Game;

/// <summary>
/// Prim Script.
/// </summary>
public class Prim
{
	public class Edge : Delaunay.Edge
	{
		public float Distance { get; private set; }
		public Edge(Delaunay.Point a, Delaunay.Point b) : base(a, b)
		{
			Distance = Vector3.Distance(a.VPoint, b.VPoint);
		}

		public Vertex GetOtherVertex(Vertex vertex, List<Vertex> vertices)
		{
			if (A == vertex.Point)
			{
				return FindVertex(B, vertices);
			}
			else
			{
				return FindVertex(A, vertices);
			}
		}
		public Vertex GetOtherVertex(Vertex vertex, HashSet<Vertex> vertices)
		{
			if (A == vertex.Point)
			{
				return FindVertex(B, vertices);
			}
			else
			{
				return FindVertex(A, vertices);
			}
		}

		public bool IsVertexInEdge(Vertex vertex)
		{
			return A == vertex.Point || B == vertex.Point;
		}

		public static bool operator ==(Edge a, Edge b)
		{
			return (a.A == b.A && a.B == b.B) || (a.B == b.B && a.B == b.B);
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

		public bool Equals(Edge other)
		{
			return this == other;
		}

		public override string ToString()
		{
			return $"Edge: ({A},{B}) - {Distance}";
		}
	}

	public class Vertex : IEquatable<Vertex>
	{
		public List<Prim.Edge> ConnectedEdges { get; private set; }
		public List<Vertex> Neighbors { get; private set; }
		public Delaunay.Point Point { get; private set; }

		public Vertex(Delaunay.Point point)
		{
			Neighbors = new List<Vertex>();
			ConnectedEdges = new List<Prim.Edge>();
			Point = point;
		}

		public void AddEdge(Prim.Edge edge)
		{
			// if (!ConnectedEdges.Contains(edge))
			ConnectedEdges.Add(edge);
		}

		public void AddNeighbor(Vertex vertex)
		{
			// if (!Neighbors.Contains(vertex))
			Neighbors.Add(vertex);
		}

		public override string ToString()
		{
			return $"Vertex: {Point} | Edges: {ConnectedEdges.Count} | Neighbors: {Neighbors.Count}";
		}

		public static bool operator ==(Vertex a, Vertex b)
		{
			return a.Point == b.Point;
		}
		public static bool operator !=(Vertex a, Vertex b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Vertex e)
			{
				return this == e;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Point.GetHashCode() ^ ConnectedEdges.GetHashCode();
		}

		public bool Equals(Vertex other)
		{
			return this == other;
		}
	}

	private static Vertex FindVertex(Delaunay.Point point, HashSet<Vertex> vertices)
	{
		foreach (var vertex in vertices)
		{
			if (vertex.Point == point)
			{
				return vertex;
			}
		}

		return null;
	}

	private static Vertex FindVertex(Delaunay.Point point, List<Vertex> vertices)
	{
		foreach (var vertex in vertices)
		{
			if (vertex.Point == point)
			{
				return vertex;
			}
		}

		return null;
	}

	public static void DebugMST(List<Prim.Edge> mst, Color color, float yOffset = 30f, float duration = 20f)
	{
		// Draw edges or any other post-processing
		foreach (var edge in mst)
		{
			Debug.Log(edge);
			Vector3 a = new Vector3(edge.A.X, yOffset, edge.A.Y);
			Vector3 b = new Vector3(edge.B.X, yOffset, edge.B.Y);
			DebugDraw.DrawLine(a, b, color, duration);
		}
	}


	public static List<Prim.Edge> MinimumSpanningTree(List<Prim.Edge> weightedEdges, Delaunay.Point start)
	{
		HashSet<Prim.Edge> mst = new HashSet<Prim.Edge>();

		HashSet<Vertex> vertices = CreateVertexSet(weightedEdges);
		List<Vertex> visited = new List<Vertex>();

		PriorityQueue<Prim.Edge, float> edgeQueue = new PriorityQueue<Prim.Edge, float>(); // Use a priority queue

		Vertex startingVertex = FindVertex(start, vertices); // Pick starting vertex
		visited.Add(startingVertex); // Add starting vertex to visited

		// Add all edges of the starting vertex to the queue
		foreach (var edge in startingVertex.ConnectedEdges)
		{
			edgeQueue.Enqueue(edge, edge.Distance);
		}

		while (mst.Count < vertices.Count - 1 && edgeQueue.Count > 0) // Ensure you don't exceed vertex count
		{
			Prim.Edge edge = edgeQueue.Dequeue(); // Get the smallest edge

			Vertex vertexA = FindVertex(edge.A, vertices);
			Vertex vertexB = FindVertex(edge.B, vertices);

			// Figure out which vertex is not on the visited list
			Vertex nextVertex = visited.Contains(vertexA) ? vertexB : vertexA;

			// Only add the next vertex if it hasn't been visited
			if (!visited.Contains(nextVertex))
			{
				mst.Add(edge); // Add the edge to the MST
				visited.Add(nextVertex); // Add the vertex to the visited list

				// Add all edges of the next vertex to the queue
				foreach (Prim.Edge e in nextVertex.ConnectedEdges)
				{
					if (!mst.Contains(e)) // Avoid adding edges already in MST
					{
						edgeQueue.Enqueue(e, e.Distance);
					}
				}
			}

		}

		// 	Debug.Log($"MST: {mst.Count} Should be {vertices.Count - 1}"); // Expect mst to be vertices.Count - 1
		// Debug.Log($"Visited: {visited.Count} should be {vertices.Count}");

		return mst.ToList();
	}

	private static HashSet<Vertex> CreateVertexSet(List<Edge> weightedEdges)
	{
		HashSet<Delaunay.Point> points = new HashSet<Delaunay.Point>();
		foreach (var edge in weightedEdges)
		{
			points.Add(edge.A);
			points.Add(edge.B);
		}


		HashSet<Vertex> vertices = new HashSet<Vertex>();
		foreach (var point in points)
		{
			Vertex vertex = new Vertex(point);
			foreach (var edge in weightedEdges)
			{
				if (edge.IsVertexInEdge(vertex))
				{
					vertex.AddEdge(edge);
				}
			}
			vertices.Add(vertex);
		}

		foreach (var vertex in vertices)
		{
			foreach (var edges in vertex.ConnectedEdges)
			{
				bool isA = edges.A == vertex.Point;
				Vertex neighbor = FindVertex(isA ? edges.B : edges.A, vertices);
				vertex.AddNeighbor(neighbor);

			}
		}

		return vertices;
	}

}
