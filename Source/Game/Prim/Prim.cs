using System;
using System.Collections.Generic;
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
		/// <summary>
		/// Connected Edges to the vertex
		/// </summary>
		public List<Delaunay.Edge> Edges { get; private set; }
		public List<Vertex> Neighbors { get; private set; }
		public Delaunay.Point Point { get; private set; }

		public Vertex(Delaunay.Point point)
		{
			Neighbors = new List<Vertex>();
			Edges = new List<Delaunay.Edge>();
			Point = point;
		}

		public void AddEdge(Delaunay.Edge edge)
		{
			if (!Edges.Contains(edge))
				Edges.Add(edge);
		}

		public void AddNeighbor(Vertex vertex)
		{
			if (!Neighbors.Contains(vertex))
				Neighbors.Add(vertex);
		}

		public override string ToString()
		{
			return $"Vertex: {Point} | Edges: {Edges.Count} | Neighbors: {Neighbors.Count}";
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
			return Point.GetHashCode() ^ Edges.GetHashCode();
		}

		public bool Equals(Vertex other)
		{
			return this == other;
		}
	}

	private Vertex FindVertex(Delaunay.Point point, HashSet<Vertex> vertices)
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

	public static List<Prim.Edge> MinimumSpanningTree(List<Prim.Edge> weightedEdges, Delaunay.Point start)
	{
		HashSet<Prim.Edge> mst = new HashSet<Prim.Edge>();
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
			// DebugDraw.DrawText(vertex.ToString(), vertex.Point.VPoint, Color.Black, 8, duration: 16.0f);
		}

		foreach (var vertex in vertices)
		{
			string edgesStr = "";
			int count = 0;
			foreach (var edges in vertex.Edges)
			{
				edgesStr += $"({count}) {edges} || ";
				count++;
			}
			Debug.Log($"{vertex} | {edgesStr}");
		}

		return null;
	}
}
