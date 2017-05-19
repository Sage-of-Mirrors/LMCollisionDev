using System;
using Assimp;
using OpenTK;
using GameFormatReader.Common;

namespace LMCollisionDev
{
	public static class Util
	{
		public static Vector3 Vec3DToVec3(Vector3D assimpVec)
		{
			return new Vector3(assimpVec.X, assimpVec.Y, assimpVec.Z);
		}

		public static Vector3D Vec3ToVec3D(Vector3 openTKVec)
		{
			return new Vector3D(openTKVec.X, openTKVec.Y, openTKVec.Z);
		}

		public static Vector3 GetSurfaceNormal(Vector3 v1, Vector3 v2, Vector3 v3)
		{
			Vector3 p1 = v2 - v1;
			Vector3 p2 = v3 - v1;
			return Vector3.Cross(p1, p2).Normalized();
		}

		public static Vector3 GetTangentVector(Vector3 normal)
		{
			Vector3 t1 = Vector3.Cross(normal, Vector3.UnitY);
			Vector3 t2 = Vector3.Cross(normal, Vector3.UnitZ);
			if (t1.Length > t2.Length)
				return t1.Normalized();
			else
				return t2.Normalized();
		}

		public static Vector3 GetBinormalVector(Vector3 normal, Vector3 tangent)
		{
			return Vector3.Cross(normal, tangent).Normalized();
		}

		public static uint CalculateHash(string input)
		{
			return CalculateHash(System.Text.Encoding.ASCII.GetBytes(input.ToCharArray()));
		}

		public static uint CalculateHash(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}

			// this code is so shitty
			uint hash = 0;
			for (int i = 0; i < data.Length; ++i)
			{
				hash <<= 8;
				hash += data[i];
				var r6 = unchecked((uint)((4993ul * hash) >> 32));
				var r0 = unchecked((byte)((((hash - r6) / 2) + r6) >> 24));
				hash -= r0 * 33554393u;
			}
			return hash;
		}

		public static void PadStream(EndianBinaryWriter writer, int padVal)
		{
			// Pad up to a 32 byte alignment
			// Formula: (x + (n-1)) & ~(n-1)
			long nextAligned = (writer.BaseStream.Length + (padVal - 1)) & ~(padVal - 1);

			long delta = nextAligned - writer.BaseStream.Length;
			writer.BaseStream.Position = writer.BaseStream.Length;
			for (int i = 0; i < delta; i++)
			{
				writer.Write((byte)0x40);
			}
		}
	}
}
