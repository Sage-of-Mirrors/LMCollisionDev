using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using GameFormatReader.Common;
using Newtonsoft.Json;
using Assimp;

namespace LMCollisionDev
{
	public partial class Collision
	{
		public enum FileType
		{
			Compiled,
			Json,
			Obj
		}

		public void SaveFile(string fileName, FileType outputType)
		{
			switch (outputType)
			{
				case FileType.Compiled: // We'll be making a .mp file at (fileName) and its properties at (fileName/jmp)
					SaveCompiled(fileName);
					break;
				case FileType.Json: // We'll be making a JSON file for human readability of geometry + properties
					SaveJson(fileName);
					break;
				case FileType.Obj: // We'll be making an OBJ file containg only geometry
					SaveObj(fileName);
					break;
				default:
					break;
			}
		}
	}
}
