using System;

namespace LMCollisionDev
{
	class MainClass
	{
		public static void Main(string[] args)
		{
Collision col = new Collision(@"D:\SZS Tools\Luigi's Mansion\testobj.json");
			col.SaveFile(@"D:\SZS Tools\Luigi's Mansion\testobj.json", Collision.FileType.Json);
		}
	}
}
