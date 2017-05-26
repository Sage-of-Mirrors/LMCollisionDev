using System;
using System.IO;

namespace LMCollisionDev
{
	class MainClass
	{
		public static void Main(string[] args)
		{
Collision col = new Collision(@"D:\SZS Tools\Luigi's Mansion\map\map10\col.obj");
			col.SaveFile(@"D:\Dropbox\Public\Collision Tutorial Utilities\Lunaboy's RARC Tools\map10\col.mp", Collision.FileType.Compiled);
			//col.SaveFile(@"D:\SZS Tools\Luigi's Mansion\map\map13\col.obj", Collision.FileType.Compiled;
		}
	}
}
