using System;
using System.IO;

namespace LMCollisionDev
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Collision col = new Collision(@"D:\SZS Tools\Luigi's Mansion\SpaceCats\col.mp");
			col.SaveFile(@"D:\SZS Tools\Luigi's Mansion\SpaceCats\col.obj", Collision.FileType.Obj);
		}
	}
}
