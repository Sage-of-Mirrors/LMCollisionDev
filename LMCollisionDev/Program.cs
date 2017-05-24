using System;
using System.IO;

namespace LMCollisionDev
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Collision col = new Collision(@"D:\Dropbox\Public\Collision Tutorial Utilities\Lunaboy's RARC Tools\map13\col_test.mp");
			col.SaveFile(@"D:\Dropbox\Public\Collision Tutorial Utilities\Lunaboy's RARC Tools\map13\col_test.obj", Collision.FileType.Obj);
		}
	}
}
