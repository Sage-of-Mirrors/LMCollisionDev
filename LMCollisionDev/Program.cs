﻿using System;
using System.IO;

namespace LMCollisionDev
{
	class MainClass
	{
		public static void Main(string[] args)
		{
            Collision col = new Collision(@"D:\SZS Tools\Luigi's Mansion\map\map13\col_down.obj");
			col.SaveFile(@"D:\Dropbox\Public\Collision Tutorial Utilities\Lunaboy's RARC Tools\map13\col.mp", Collision.FileType.Compiled);
		}
	}
}
