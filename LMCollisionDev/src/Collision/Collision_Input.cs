using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using Assimp;
using GameFormatReader.Common;
using Newtonsoft.Json;

namespace LMCollisionDev
{
	public partial class Collision
	{
		public string OriginalFilename { get; private set; }
		public static Vector3 StaticScale;
		public BoundingBox BBox { get; private set; }
		public List<Vector3> Vertexes { get; private set; }
		public List<Vector3> Normals { get; private set; }
		public List<Triangle> Triangles { get; private set; }

		public Collision()
		{
			StaticScale = new Vector3(256.0f, 512.0f, 256.0f);
			Vertexes = new List<Vector3>();
			Normals = new List<Vector3>();
			Triangles = new List<Triangle>();
		}

		public Collision(string fileName)
		{
			if (!File.Exists(fileName))
				throw new FormatException($"File \"{ fileName }\" does not exist.");

			OriginalFilename = fileName;
			StaticScale = new Vector3(256.0f, 512.0f, 256.0f);
			Vertexes = new List<Vector3>();
			Normals = new List<Vector3>();
			Triangles = new List<Triangle>();

			string fileExt = Path.GetExtension(fileName).ToLower();
			switch (fileExt)
			{
				case ".mp": // We were passed compiled collision
					m_OpenCompiled(fileName);
					break;
				case ".json": // We were passed a json file
					m_OpenJson(fileName);
					break;
				default: // We may have been passed a model file or an incompatibile file, we'll check later
					m_OpenModelFile(fileName);
					break;
			}

			BBox = new BoundingBox(Vertexes);
			m_GenerateGrid_Obj();
		}

		private void m_GenerateGrid_Obj()
		{
			int xCellCount = (int)(Math.Floor(BBox.AxisLengths.X / 256) + 1);
			int yCellCount = (int)(Math.Floor(BBox.AxisLengths.Y / 512) + 1);
			int zCellCount = (int)(Math.Floor(BBox.AxisLengths.Z / 256) + 1);

			float xCellSize = BBox.AxisLengths.X / xCellCount;
			float yCellSize = BBox.AxisLengths.Y / yCellCount;
			float zCellSize = BBox.AxisLengths.Z / zCellCount;

			float curX = BBox.Minimum.X;
			float curY = BBox.Minimum.Y;
			float curZ = BBox.Minimum.Z;

			StringWriter wrtr = new StringWriter();

			for (int k = 0; k <= zCellCount; k++)
			{
				for (int j = 0; j <= yCellCount; j++)
				{
					for (int i = 0; i <= xCellCount; i++)
					{
						wrtr.WriteLine($"v { curX } { curY } { curZ }");

						curX += xCellSize;
					}

					curX = BBox.Minimum.X;
					curY += yCellSize;
				}

				/*
				wrtr.WriteLine($"v { curX } { curY } { curZ }");
				wrtr.WriteLine($"v { curX + xCellSize } { curY } { curZ }");
				wrtr.WriteLine($"v { curX } { curY + yCellSize } { curZ }");
				wrtr.WriteLine($"v { curX } { curY } { curZ + zCellSize }");
				wrtr.WriteLine($"v { curX + xCellSize } { curY + yCellSize } { curZ }");
				wrtr.WriteLine($"v { curX + xCellSize } { curY } { curZ + zCellSize }");
				wrtr.WriteLine($"v { curX } { curY + yCellSize } { curZ + zCellSize }");
				wrtr.WriteLine($"v { curX + xCellSize } { curY + yCellSize } { curZ + zCellSize }");
				*/

				curY = BBox.Minimum.Y;
				curZ += zCellSize;
			}

			/*
			for (int i = 0; i < xCellCount + yCellCount + zCellCount; i++)
			{
				wrtr.WriteLine($"o { i }");

				wrtr.WriteLine($"f { (i * 8) + 1 } { (i * 8) + 2 } { (i * 8) + 3 }");
				wrtr.WriteLine($"f { (i * 8) + 2 } { (i * 8) + 5 } { (i * 8) + 3 }");

				wrtr.WriteLine($"f { (i * 8) + 4 } { (i * 8) + 1 } { (i * 8) + 3 }");
				wrtr.WriteLine($"f { (i * 8) + 4 } { (i * 8) + 3 } { (i * 8) + 7 }");

				wrtr.WriteLine($"f { (i * 8) + 2 } { (i * 8) + 5 } { (i * 8) + 6 }");
				wrtr.WriteLine($"f { (i * 8) + 6 } { (i * 8) + 5 } { (i * 8) + 8 }");

				wrtr.WriteLine($"f { (i * 8) + 6 } { (i * 8) + 4 } { (i * 8) + 7 }");
				wrtr.WriteLine($"f { (i * 8) + 6 } { (i * 8) + 7 } { (i * 8) + 8 }");

				wrtr.WriteLine($"f { (i * 8) + 5 } { (i * 8) + 3 } { (i * 8) + 7 }");
				wrtr.WriteLine($"f { (i * 8) + 5 } { (i * 8) + 7 } { (i * 8) + 8 }");

				wrtr.WriteLine($"f { (i * 8) + 1 } { (i * 8) + 2 } { (i * 8) + 6 }");
				wrtr.WriteLine($"f { (i * 8) + 1 } { (i * 8) + 6 } { (i * 8) + 4 }");
			}*/

			using (FileStream strm = new FileStream(@"D:\SZS Tools\Luigi's Mansion\ManGrid.obj", FileMode.Create, FileAccess.Write))
			{
				EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);
				writer.Write(wrtr.ToString().ToCharArray());
			}
		}

		private void m_OpenCompiled(string fileName)
		{
			using (FileStream strm = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				EndianBinaryReader reader = new EndianBinaryReader(strm, Endian.Big);

				// If these turn out to change, this will tell us. Otherwise they should always be these values
				if (reader.ReadSingle() != 256.0f)
					throw new FormatException("Static X scale was not 256!");
				if (reader.ReadSingle() != 512.0f)
					throw new FormatException("Static Y scale was not 512!");
				if (reader.ReadSingle() != 256.0f)
					throw new FormatException("Static Z scale was not 256!");

				BBox = new BoundingBox(
					new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
					new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));

				int vertexDataOffset = reader.ReadInt32();
				int normalDataOffset = reader.ReadInt32();
				int faceDataOffset = reader.ReadInt32();
				int unk1DataOffset = reader.ReadInt32();
				int unk2DataOffset = reader.ReadInt32();
				int unk3DataOffset = reader.ReadInt32();
				int unk4DataOffset = reader.ReadInt32();

				int numVertexes = (normalDataOffset - vertexDataOffset) / 0x0C;
				int numNormals = (faceDataOffset - normalDataOffset) / 0x0C;
				int numFaces = (unk1DataOffset - faceDataOffset) / 0x18;

				reader.BaseStream.Seek(vertexDataOffset, SeekOrigin.Begin);

				for (int i = 0; i < numVertexes; i++)
				{
					Vertexes.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
				}

				BBox = new BoundingBox(Vertexes);

				reader.BaseStream.Seek(normalDataOffset, SeekOrigin.Begin);

				for (int i = 0; i < numNormals; i++)
				{
					Normals.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
				}

				reader.BaseStream.Seek(faceDataOffset, SeekOrigin.Begin);

				for (int i = 0; i < numFaces; i++)
				{
					Triangles.Add(new Triangle(reader));

					/*
					Console.WriteLine(Normals[Triangles[i].NormalIndex]);
					Console.WriteLine(Normals[Triangles[i].Edge1TangentIndex]);
					Console.WriteLine(Normals[Triangles[i].Edge2TangentIndex]);
					Console.WriteLine(Normals[Triangles[i].Edge3TangentIndex]);
					Console.WriteLine(Normals[Triangles[i].PlanePointIndex]);
					Console.WriteLine(Triangles[i].PlaneDValue);
					Console.WriteLine();*/
				}

				reader.BaseStream.Seek(unk2DataOffset, SeekOrigin.Begin);

				StringWriter strWriter = new StringWriter();

				int xCellCount = (int)(Math.Floor(BBox.AxisLengths.X / 256.0f) + 1);
				int yCellCount = (int)(Math.Floor(BBox.AxisLengths.Y / 512.0f) + 1);
				int zCellCount = (int)(Math.Floor(BBox.AxisLengths.Z / 256.0f) + 1);

				float xCellSize = BBox.AxisLengths.X / xCellCount;
				float yCellSize = BBox.AxisLengths.Y / yCellCount;
				float zCellSize = BBox.AxisLengths.Z / zCellCount;

				float curX = BBox.Minimum.X;
				float curY = BBox.Minimum.Y;
				float curZ = BBox.Minimum.Z;

				for (int zCoord = 0; zCoord < zCellCount; zCoord++)
				{
					for (int yCoord = 0; yCoord < yCellCount; yCoord++)
					{
						for (int xCoord = 0; xCoord < xCellCount; xCoord++)
						{
							int index1 = reader.ReadInt32();
							int index2 = reader.ReadInt32();

							if (index1 != 0 && index2 != 0)
								strWriter.WriteLine($"v { curX } { curY } { curZ }");

							curX += xCellSize;
						}

						curX = BBox.Minimum.X;
						curY += yCellSize;
					}

					curY = BBox.Minimum.Y;
					curZ += zCellSize;
				}

				using (FileStream objOut = new FileStream(@"D:\SZS Tools\Luigi's Mansion\gridOut.obj", FileMode.Create, FileAccess.Write))
				{
					EndianBinaryWriter objWriter = new EndianBinaryWriter(objOut, Endian.Big);
					objWriter.Write(strWriter.ToString().ToCharArray());
				}
			}

			string jmpFolderName = $"{ Path.GetDirectoryName(fileName) }\\jmp";
			if (!Directory.Exists(jmpFolderName))
			{
				Console.WriteLine("No jmp folder was found. Skipping collision properties...");
				return;
			}

			string colPropsFileName = $" { jmpFolderName }\\polygoninfo";
			if (File.Exists(colPropsFileName))
			{
				m_LoadCompiledColProperties(colPropsFileName);
			}
			else
			{
				Console.WriteLine("No polygoninfo file was found. Skipping collision properties...");
			}

			string sndPropsFileName = $" { jmpFolderName }\\soundpolygoninfo";
			if (File.Exists(sndPropsFileName))
			{
				m_LoadCompiledSndProperties(sndPropsFileName);
			}
			else
			{
				Console.WriteLine("No soundpolygoninfo file was found. Skipping sound properties...");
			}
		}

		private void m_LoadCompiledColProperties(string fileName)
		{
			using (FileStream strm = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				EndianBinaryReader reader = new EndianBinaryReader(strm, Endian.Big);

				if (reader.ReadInt32() != Triangles.Count)
				{
					Console.WriteLine("Triangle-collision property count mismatch!");
					return;
				}

				reader.BaseStream.Seek(0x34, SeekOrigin.Begin);

				for (int i = 0; i < Triangles.Count; i++)
				{
					Triangles[i].ReadCompiledColProperties(reader);
				}
			}
		}

		private void m_LoadCompiledSndProperties(string fileName)
		{
			using (FileStream strm = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				EndianBinaryReader reader = new EndianBinaryReader(strm, Endian.Big);

				if (reader.ReadInt32() != Triangles.Count)
				{
					Console.WriteLine("Triangle-sound property count mismatch!");
					return;
				}

				reader.BaseStream.Seek(0x28, SeekOrigin.Begin);

				for (int i = 0; i < Triangles.Count; i++)
				{
					Triangles[i].ReadCompiledSndProperties(reader);
				}
			}
		}

		private void m_OpenJson(string fileName)
		{
			using (StreamReader strmReader = File.OpenText(fileName))
			{
				using (JsonTextReader jsonReader = new JsonTextReader(strmReader))
				{
					jsonReader.SupportMultipleContent = true;

					JsonSerializer srl = new JsonSerializer();

					jsonReader.Read();
					List<Vector3D> simpleVerts = srl.Deserialize<List<Vector3D>>(jsonReader);
					foreach (Vector3D vec in simpleVerts)
						Vertexes.Add(Util.Vec3DToVec3(vec));

					jsonReader.Read();
					List<Vector3D> simpleNrms = srl.Deserialize<List<Vector3D>>(jsonReader);
					foreach (Vector3D vec in simpleNrms)
						Normals.Add(Util.Vec3DToVec3(vec));

					jsonReader.Read();
					Triangles = srl.Deserialize<List<Triangle>>(jsonReader);
				}
			}
		}

		private void m_OpenModelFile(string fileName)
		{
			AssimpContext cont = new AssimpContext();
			Scene scene;

			// We'll make sure the file loads. If it doesn't, we'll throw an exception
			try
			{
				scene = cont.ImportFile(fileName, PostProcessSteps.Triangulate);
			}
			catch
			{
				throw new FormatException($"Failed to open file \"{ fileName }\". The file is either not a supported model file or corrupted.");
			}

			m_LoadModelFile(scene);
		}

		private void m_LoadModelFile(Scene scene)
		{
			foreach (Mesh msh in scene.Meshes)
			{
				// Extract vertexes
				foreach (Vector3D vert in msh.Vertices)
				{
					Vertexes.Add(Util.Vec3DToVec3(vert));
				}

				// Extract triangles
				foreach (Face face in msh.Faces)
				{
					Triangles.Add(new Triangle(face, Vertexes, Normals));
				}
			}
		}
	}
}
