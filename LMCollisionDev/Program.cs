using System;

namespace LMCollisionDev
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Collision col = new Collision(@"D:\SZS Tools\Luigi's Mansion\map\map2\col.mp");
			col.SaveFile(@"D:\SZS Tools\Luigi's Mansion\map\map2\col.obj", Collision.FileType.Obj);
		}
	}
}
