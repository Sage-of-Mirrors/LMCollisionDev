﻿using System;
using System.Collections.Generic;
using GameFormatReader.Common;
using OpenTK;

namespace LMCollisionDev
{
	public class GridCell
	{
		public BoundingBox Bounds { get; private set; }
		public List<Triangle> AllIntersectingTris { get; private set; }
		public List<Triangle> FloorIntersectingTris { get; private set; }

		public GridCell()
		{
			Bounds = new BoundingBox();
			AllIntersectingTris = new List<Triangle>();
			FloorIntersectingTris = new List<Triangle>();
		}

		public GridCell(Vector3 min, Vector3 max)
		{
			Bounds = new BoundingBox(min, max);
			AllIntersectingTris = new List<Triangle>();
			FloorIntersectingTris = new List<Triangle>();
		}

		// Code copied from https://stackoverflow.com/questions/17458562/efficient-aabb-triangle-intersection-in-c-sharp
		public void CheckTriangle(Triangle tri, List<Vector3> vertexes, List<Vector3> normals)
		{
			List<Vector3> triVerts = new List<Vector3>();
			triVerts.Add(vertexes[tri.VertexIndices[0]]);
			triVerts.Add(vertexes[tri.VertexIndices[1]]);
			triVerts.Add(vertexes[tri.VertexIndices[2]]);
			Vector3 normal = normals[tri.NormalIndex];

			// This will test the nox normals' axes
			float triangleMin, triangleMax;
			Vector3[] boxNormals = new Vector3[]
			{
				new Vector3(1, 0, 0),
				new Vector3(0, 1, 0),
				new Vector3(0, 0, 1)
			};

			for (int i = 0; i < 3; i++)
			{
				Vector3 n = boxNormals[i];
				m_Project(triVerts, boxNormals[i], out triangleMin, out triangleMax);

				if (triangleMax < Bounds.Minimum[i] || triangleMin > Bounds.Maximum[i])
					return;
			}

			List<Vector3> boxVertices = new List<Vector3>();
			boxVertices.Add(Bounds.Minimum);
			boxVertices.Add(new Vector3(Bounds.Maximum.X, Bounds.Minimum.Y, Bounds.Minimum.Z));
			boxVertices.Add(new Vector3(Bounds.Minimum.X, Bounds.Maximum.Y, Bounds.Minimum.Z));
			boxVertices.Add(new Vector3(Bounds.Minimum.X, Bounds.Minimum.Y, Bounds.Maximum.Z));
			boxVertices.Add(new Vector3(Bounds.Maximum.X, Bounds.Maximum.Y, Bounds.Minimum.Z));
			boxVertices.Add(new Vector3(Bounds.Maximum.X, Bounds.Minimum.Y, Bounds.Maximum.Z));
			boxVertices.Add(new Vector3(Bounds.Minimum.X, Bounds.Maximum.Y, Bounds.Maximum.Z));
			boxVertices.Add(Bounds.Maximum);

			float boxMin, boxMax;
			float triangleOffset = Vector3.Dot(normal, triVerts[0]);
			m_Project(boxVertices, normal, out boxMin, out boxMax);

			if (boxMax < triangleOffset || boxMin > triangleOffset)
				return;

			List<Vector3> triangleEdges = new List<Vector3>();
			triangleEdges.Add(vertexes[tri.VertexIndices[0]] - vertexes[tri.VertexIndices[1]]);
			triangleEdges.Add(vertexes[tri.VertexIndices[1]] - vertexes[tri.VertexIndices[2]]);
			triangleEdges.Add(vertexes[tri.VertexIndices[2]] - vertexes[tri.VertexIndices[0]]);

			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					Vector3 axis = Vector3.Cross(triangleEdges[i], boxNormals[j]);
					m_Project(boxVertices, axis, out boxMin, out boxMax);
					m_Project(triVerts, axis, out triangleMin, out triangleMax);

					if (boxMax < triangleMin || boxMin > triangleMax)
						return;
				}
			}

			// If we got this far, that means there's an intersection! Hooray!
			AllIntersectingTris.Add(tri);

			if (m_IsFloor(normal))
				FloorIntersectingTris.Add(tri);
		}

		private void m_Project(List<Vector3> verts, Vector3 axis, out float min, out float max)
		{
			min = float.PositiveInfinity;
			max = float.NegativeInfinity;
			foreach (Vector3 vec in verts)
			{
				float val = Vector3.Dot(axis, vec);
				if (val < min) min = val;
				if (val > max) max = val;
			}
		}

		private bool m_IsFloor(Vector3 normal)
		{
			Vector3 upAxis = Vector3.UnitY;

			float numerator = Vector3.Dot(normal, upAxis);
			float denomenator = normal.Length * upAxis.Length;
			float angle = (float)Math.Acos(numerator / denomenator);

			angle *= (float)(180 / Math.PI);
			if (Math.Abs(angle) < 65.0f)
				return true;
			
			return false;
		}

		public override string ToString()
		{
			return $"Triangles (All): { AllIntersectingTris.Count }; Triangles (Floor): { FloorIntersectingTris.Count }";
		}
	}
}
