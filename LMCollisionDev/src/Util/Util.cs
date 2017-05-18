using System;
using Assimp;
using OpenTK;

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
	}
}
