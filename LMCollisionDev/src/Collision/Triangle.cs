using System;
using System.Collections.Generic;
using OpenTK;
using GameFormatReader.Common;
using Assimp;
using Newtonsoft.Json;

namespace LMCollisionDev
{
	public class Triangle
	{
		public class CollisionProperties
		{
			public enum CollisionMaterials
			{
				Normal,
				Ice,
				Unused1,
				Unused2
			}

			public CollisionMaterials ColMaterial;
			public bool IsLadder;
			public bool IgnorePointer;
		}

		public class SoundProperties
		{
			public enum SoundMaterials
			{
				dummy
			}

			public SoundMaterials SndMaterial;
			public int SndEchoSwitch;
		}

		public List<int> VertexIndices { get; private set; }
		public int NormalIndex { get; private set; }
		public int TangentIndex { get; private set; }
		public int BinormalIndex { get; private set; }
		public int Unknown1Index { get; private set; }
		public int PlanePointIndex { get; private set; }
		public float PlaneDValue { get; private set; }
		public int Unknown1 { get; private set; }
		public int Unknown2 { get; private set; }

		public CollisionProperties ColProperties { get; private set; }
		public SoundProperties SndProperties { get; private set; }

		public Triangle()
		{
			ColProperties = new CollisionProperties();
			SndProperties = new SoundProperties();
			VertexIndices = new List<int>();
		}

		public Triangle(EndianBinaryReader reader)
		{
			ColProperties = new CollisionProperties();
			SndProperties = new SoundProperties();
			VertexIndices = new List<int>();
			VertexIndices.AddRange(new int[] { (int)reader.ReadInt16(), (int)reader.ReadInt16(), (int)reader.ReadInt16() });

			NormalIndex = (int)reader.ReadInt16();
			TangentIndex = (int)reader.ReadInt16();
			BinormalIndex = (int)reader.ReadInt16();
			Unknown1Index = (int)reader.ReadInt16();
			PlanePointIndex = (int)reader.ReadInt16();
			PlaneDValue = reader.ReadSingle();
			Unknown1 = (int)reader.ReadInt16();
			Unknown2 = (int)reader.ReadInt16();
		}

		public Triangle(Face face, List<Vector3> vertexes, List<Vector3> normals)
		{
			ColProperties = new CollisionProperties();
			SndProperties = new SoundProperties();
			VertexIndices = new List<int>();
			VertexIndices = face.Indices;

			// Get normal
			NormalIndex = normals.Count;
			normals.Add(Util.GetSurfaceNormal(
				vertexes[VertexIndices[0]],
				vertexes[VertexIndices[1]],
				vertexes[VertexIndices[2]]));

			// Get tangent
			TangentIndex = normals.Count;
			normals.Add(Util.GetTangentVector(normals[NormalIndex]));

			// Get binormal
			BinormalIndex = normals.Count;
			normals.Add(Util.GetBinormalVector(normals[NormalIndex], normals[TangentIndex]));

			// Dunno
			Unknown1Index = 0;
			PlanePointIndex = TangentIndex;
			PlaneDValue = 50.0f;
			Unknown1 = 0x8000;
			Unknown2 = 0;
		}

		[JsonConstructor]
		public Triangle(List<int> VertexIndices, int NormalIndex, int TangentIndex, int BinormalIndex, int Unknown1Index, int PlanePointIndex, float PlaneDValue, int Unknown1, int Unknown2, CollisionProperties ColProperties, SoundProperties SndProperties)
		{
			this.VertexIndices = VertexIndices;
			this.NormalIndex = NormalIndex;
			this.TangentIndex = TangentIndex;
			this.BinormalIndex = BinormalIndex;
			this.Unknown1Index = Unknown1Index;
			this.PlanePointIndex = PlanePointIndex;
			this.PlaneDValue = PlaneDValue;
			this.Unknown1 = Unknown1;
			this.Unknown2 = Unknown2;
			this.ColProperties = ColProperties;
			this.SndProperties = SndProperties;
		}

		public void ReadCompiledColProperties(EndianBinaryReader reader)
		{
			int bitField = reader.ReadInt32();
			ColProperties.ColMaterial = (CollisionProperties.CollisionMaterials)(bitField & 3);
			ColProperties.IsLadder = Convert.ToBoolean(((bitField & 4) >> 2));
			ColProperties.IgnorePointer = Convert.ToBoolean(((bitField & 8) >> 3));
		}

		public void ReadCompiledSndProperties(EndianBinaryReader reader)
		{
			int bitField = reader.ReadInt32();
			SndProperties.SndMaterial = (SoundProperties.SoundMaterials)(bitField & 0xF);
			SndProperties.SndEchoSwitch = (bitField & 0x70) >> 4;
		}

		public void WriteCompiledTriangle(EndianBinaryWriter writer)
		{
			writer.Write((ushort)VertexIndices[0]);
			writer.Write((ushort)VertexIndices[1]);
			writer.Write((ushort)VertexIndices[2]);
			writer.Write((ushort)NormalIndex);
			writer.Write((ushort)TangentIndex);
			writer.Write((ushort)BinormalIndex);
			writer.Write((ushort)Unknown1Index);
			writer.Write((ushort)PlanePointIndex);
			writer.Write(PlaneDValue);
			writer.Write((ushort)Unknown1);
			writer.Write((ushort)Unknown2);
		}

		public void WriteCompiledColProperties(EndianBinaryWriter writer)
		{
			uint bitField = 0;

			uint ignorePointer = Convert.ToUInt32(ColProperties.IgnorePointer);
			bitField |= (uint)(ignorePointer << 3);

			uint isLadder = Convert.ToUInt32(ColProperties.IsLadder);
			bitField |= (uint)(isLadder << 2);

			bitField |= (uint)ColProperties.ColMaterial;
			writer.Write(bitField);
		}

		public void WriteCompiledSndProperties(EndianBinaryWriter writer)
		{
			uint bitField = 0;
			bitField |= (uint)(SndProperties.SndEchoSwitch << 4);
			bitField |= (uint)(SndProperties.SndMaterial);
			writer.Write(bitField);
		}
	}
}
