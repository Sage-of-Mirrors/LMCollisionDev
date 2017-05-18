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

				// Write vertexes
				foreach (Vector3 vec in Vertexes)
				{
					writer.Write(vec.X);
					writer.Write(vec.Y);
					writer.Write(vec.Z);
				}

				// Write offset to normal data
				writer.BaseStream.Seek(0x28, SeekOrigin.Begin);
				writer.Write((int)writer.BaseStream.Length);
				writer.Seek(0, SeekOrigin.End);

				// Write normals
				foreach (Vector3 vec in Normals)
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
			}
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
