using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using GameFormatReader.Common;
using Newtonsoft.Json;
using Assimp;

namespace LMCollisionDev
{
	public partial class Collision
	{
		public enum FileType
		{
			Compiled,
			Json,
			Obj
		}

		public void SaveFile(string fileName, FileType outputType)
		{
			switch (outputType)
			{
				case FileType.Compiled:
					SaveCompiled(fileName);
					break;
				case FileType.Json:
					SaveJson(fileName);
					break;
				case FileType.Obj:
					SaveObj(fileName);
					break;
				default:
					break;
			}
		}

		private void SaveCompiled(string fileName)
		{
			using (FileStream strm = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);

				// Static scale, which is always (256, 512, 256)
				writer.Write(StaticScale.X);
				writer.Write(StaticScale.Y);
				writer.Write(StaticScale.Z);

				BBox.WriteBoundingBox(writer); // Bounding box, min then max

				writer.Write((int)0x40); // Vertex offset, always 0x40
				writer.Write((int)0); // Placeholder for normal data offset
				writer.Write((int)0); // Placeholder for triangle data offset
				writer.Write((int)0); // Placeholder for ??? data offset
				writer.Write((int)0); // Placeholder for ??? data offset
				writer.Write((int)0); // Placeholder for ??? data offset
				writer.Write((int)0); // Placeholder for ??? data offset

				List<Vector3> vertexExport = new List<Vector3>();
				foreach (Triangle tri in Triangles)
				{
					for (int i = 0; i < 3; i++)
					{
						Vector3 vec = Vertexes[tri.VertexIndices[i]];

						if (!vertexExport.Contains(vec))
						{
							vertexExport.Add(vec);
						}

						tri.VertexIndices[i] = vertexExport.IndexOf(vec);
					}
				}

				// Write vertexes
				foreach (Vector3 vec in vertexExport)
				{
					writer.Write(vec.X);
					writer.Write(vec.Y);
					writer.Write(vec.Z);
				}

				// Write offset to normal data
				writer.BaseStream.Seek(0x28, SeekOrigin.Begin);
				writer.Write((int)writer.BaseStream.Length);
				writer.Seek(0, SeekOrigin.End);

				List<Vector3> normalsExport = new List<Vector3>();
				foreach (Triangle tri in Triangles)
				{
					Vector3 normal = Normals[tri.NormalIndex];
					if (!normalsExport.Contains(normal))
						normalsExport.Add(normal);
					tri.NormalIndex = normalsExport.IndexOf(normal);

					Vector3 edge1 = Normals[tri.Edge1TangentIndex];
					Vector3 neg = -edge1;
					if (!normalsExport.Contains(-edge1))
						normalsExport.Add(-edge1);
					tri.Edge1TangentIndex = normalsExport.IndexOf(-edge1);

					Vector3 edge2 = Normals[tri.Edge2TangentIndex];
					if (!normalsExport.Contains(edge2))
						normalsExport.Add(edge2);
					tri.Edge2TangentIndex = normalsExport.IndexOf(edge2);

					Vector3 edge3 = Normals[tri.Edge3TangentIndex];
					if (!normalsExport.Contains(edge3))
						normalsExport.Add(edge3);
					tri.Edge3TangentIndex = normalsExport.IndexOf(edge3);

					Vector3 unk1 = Normals[tri.PlanePointIndex];
					if (!normalsExport.Contains(-unk1))
						normalsExport.Add(-unk1);
					tri.PlanePointIndex = normalsExport.IndexOf(-unk1);
				}

				// Write normals
				foreach (Vector3 vec in normalsExport)
				{
					writer.Write(vec.X);
					writer.Write(vec.Y);
					writer.Write(vec.Z);
				}

				// Write offset to face data
				writer.BaseStream.Seek(0x2C, SeekOrigin.Begin);
				writer.Write((int)writer.BaseStream.Length);
				writer.Seek(0, SeekOrigin.End);

				foreach (Triangle tri in Triangles)
				{
					tri.WriteCompiledTriangle(writer);
				}

				int xCellCount, yCellCount, zCellCount;
				List<GridCell> grid = m_GenerateGrid(out xCellCount, out yCellCount, out zCellCount);

				List<short> allTriangleIndexesForGrid = new List<short>();
				List<int> gridTriangleIndexes = new List<int>();
				allTriangleIndexesForGrid.Add(-1);

				for (int z = 0; z < zCellCount; z++)
				{
					for (int y = 0; y < yCellCount; y++)
					{
						for (int x = 0; x < xCellCount; x++)
						{
							int index = x + (y * xCellCount) + (z * xCellCount * yCellCount);

							if (grid[index].AllIntersectingTris.Count != 0)
							{
								gridTriangleIndexes.Add(allTriangleIndexesForGrid.Count);
								foreach (Triangle tri in grid[index].AllIntersectingTris)
									allTriangleIndexesForGrid.Add((short)Triangles.IndexOf(tri));
								allTriangleIndexesForGrid.Add(-1);
							}
							else
							{
								gridTriangleIndexes.Add(0);
							}

							if (grid[index].FloorIntersectingTris.Count != 0)
							{
								gridTriangleIndexes.Add(allTriangleIndexesForGrid.Count);
								foreach (Triangle tri in grid[index].FloorIntersectingTris)
									allTriangleIndexesForGrid.Add((short)Triangles.IndexOf(tri));
								allTriangleIndexesForGrid.Add(-1);
							}
							else
								gridTriangleIndexes.Add(0);
						}
					}
				}

				allTriangleIndexesForGrid.Add(-1);

				// Write offset to triangle group data
				writer.BaseStream.Seek(0x30, SeekOrigin.Begin);
				writer.Write((int)writer.BaseStream.Length);
				writer.Seek(0, SeekOrigin.End);

				foreach (short shr in allTriangleIndexesForGrid)
					writer.Write(shr);

				// Write offset to grid indices (twice)
				writer.BaseStream.Seek(0x34, SeekOrigin.Begin);
				writer.Write((int)writer.BaseStream.Length);
				writer.Write((int)writer.BaseStream.Length);
				writer.Seek(0, SeekOrigin.End);

				foreach (int inte in gridTriangleIndexes)
					writer.Write(inte);

				Util.PadStream(writer, 32);
			}

			string jmpFolderName = $"{ Path.GetDirectoryName(fileName) }\\jmp";

			if (!Directory.Exists(jmpFolderName))
				Directory.CreateDirectory(jmpFolderName);

			string colPropertiesFileName = $"{ jmpFolderName }\\polygoninfo";
			using (FileStream strm = new FileStream(colPropertiesFileName, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);

				m_WriteCompiledColPropertiesHeader(writer);

				foreach (Triangle tri in Triangles)
					tri.WriteCompiledColProperties(writer);
				Util.PadStream(writer, 32);
			}


			string sndPropertiesFileName = $"{ jmpFolderName }\\soundpolygoninfo";
			using (FileStream strm = new FileStream(sndPropertiesFileName, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);

				m_WriteCompiledSndPropertiesHeader(writer);

				foreach (Triangle tri in Triangles)
					tri.WriteCompiledSndProperties(writer);
				Util.PadStream(writer, 32);
			}
		}

		private List<GridCell> m_GenerateGrid(out int xCellCount, out int yCellCount, out int zCellCount)
		{
			List<GridCell> cells = new List<GridCell>();

			xCellCount = (int)(Math.Floor(BBox.AxisLengths.X / 256) + 1);
			yCellCount = (int)(Math.Floor(BBox.AxisLengths.Y / 512) + 1);
			zCellCount = (int)(Math.Floor(BBox.AxisLengths.Z / 256));

			float xCellSize = BBox.AxisLengths.X / xCellCount;
			float yCellSize = BBox.AxisLengths.Y / yCellCount;
			float zCellSize = BBox.AxisLengths.Z / zCellCount;

			float curX = BBox.Minimum.X;
			float curY = BBox.Minimum.Y;
			float curZ = BBox.Minimum.Z;

			for (int xCoord = 0; xCoord < xCellCount; xCoord++)
			{
				for (int yCoord = 0; yCoord < yCellCount; yCoord++)
				{
					for (int zCoord = 0; zCoord < zCellCount; zCoord++)
					{
						GridCell cell = new GridCell(new Vector3(curX, curY, curZ), new Vector3(curX + xCellSize, curY + yCellSize, curZ + zCellSize));
						cells.Add(cell);

						foreach (Triangle tri in Triangles)
							cell.CheckTriangle(tri, Vertexes, Normals);

						curZ += zCellSize;
					}

					curZ = BBox.Minimum.Z;
					curY += yCellSize;
				}

				curY = BBox.Minimum.Y;
				curX += xCellSize;
			}

			return cells;
		}

		private void m_WriteCompiledColPropertiesHeader(EndianBinaryWriter writer)
		{
			writer.Write(Triangles.Count);
			writer.Write((int)3);
			writer.Write((int)0x34);
			writer.Write((int)4);

			writer.Write((int)0x002AAF7F);
			writer.Write((int)3);
			writer.Write((int)0);

			writer.Write((int)0x01C2B94A);
			writer.Write((int)4);
			writer.Write((int)0x200);

			writer.Write((int)0x00AF2BA5);
			writer.Write((int)8);
			writer.Write((int)0x300);
		}

		private void m_WriteCompiledSndPropertiesHeader(EndianBinaryWriter writer)
		{
			writer.Write(Triangles.Count);
			writer.Write((int)2);
			writer.Write((int)0x28);
			writer.Write((int)4);

			writer.Write((int)0x006064D7);
			writer.Write((int)0xF);
			writer.Write((int)0);

			writer.Write((int)0x005169FA);
			writer.Write((int)0x70);
			writer.Write((int)0x400);
		}

		private void SaveJson(string fileName)
		{
			StringWriter strWriter = new StringWriter();

			JsonSerializer ser = new JsonSerializer();
			//ser.Converters.Add(J

			List<Vector3D> simpleVerts = new List<Vector3D>();
			foreach (Vector3 vec in Vertexes)
				simpleVerts.Add(Util.Vec3ToVec3D(vec));

			string vertexes = JsonConvert.SerializeObject(simpleVerts, Formatting.Indented);
			strWriter.Write(vertexes);

			List<Vector3D> simpleNrms = new List<Vector3D>();
			foreach (Vector3 vec in Normals)
				simpleNrms.Add(Util.Vec3ToVec3D(vec));

			string normals = JsonConvert.SerializeObject(simpleNrms, Formatting.Indented);
			strWriter.Write(normals);

			string triangles = JsonConvert.SerializeObject(Triangles, Formatting.Indented);
			strWriter.Write(triangles);

			using (FileStream strm = new FileStream(fileName, FileMode.Create))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);
				writer.Write(strWriter.ToString().ToCharArray());
			}
		}

		private void SaveObj(string fileName)
		{
			StringWriter strWriter = new StringWriter();

			strWriter.WriteLine($"# This OBJ file was dumped from \"{ Path.GetFileName(OriginalFilename) }\" using Sage of Mirrors/Gamma's Luigi's Mansion collision tool.");
			strWriter.WriteLine($"# Created { DateTime.Now.ToString() }");
			strWriter.WriteLine($"# Vertex Count: { Vertexes.Count }");
			strWriter.WriteLine($"# Triangle Count: { Triangles.Count }");

			strWriter.WriteLine();

			foreach (Vector3 vec in Vertexes)
			{
				strWriter.WriteLine($"v { vec.X } { vec.Y } { vec.Z }");
			}

			strWriter.WriteLine();

			foreach (Triangle tri in Triangles)
			{
				strWriter.WriteLine($"vn { Normals[tri.NormalIndex].X } { Normals[tri.NormalIndex].Y } { Normals[tri.NormalIndex].Z }");
			}

			strWriter.WriteLine();

			int normalIndex = 1;
			foreach (Triangle tri in Triangles)
			{
				strWriter.Write($"f { tri.VertexIndices[0] + 1 }//{ normalIndex }");
				strWriter.Write($" { tri.VertexIndices[1] + 1 }//{ normalIndex }");
				strWriter.Write($" { tri.VertexIndices[2] + 1 }//{ normalIndex }");
				strWriter.Write(Environment.NewLine);

				normalIndex++;
			}

			using (FileStream strm = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);
				writer.Write(strWriter.ToString().ToCharArray());
			}
		}
	}
}
