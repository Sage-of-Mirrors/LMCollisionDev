using System;
using System.IO;

namespace LMCollisionDev
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				m_ShowHelpMessage();
				return;
			}
			if (args.Length % 2 != 0)
			{
				Console.WriteLine("There was a mis-match in the number of arguments. Please see the help message below...");
				Console.WriteLine();
				m_ShowHelpMessage();
				return;
			}

			string inputFile = "";
			string outputFile = "";
			Collision.FileType outputType = Collision.FileType.none;

			bool canOutput = m_CheckArgs(args, out inputFile, out outputFile, out outputType);

			if (!canOutput)
				return;

			Collision col = new Collision(inputFile);
			col.SaveFile(outputFile, outputType);

			/*
			Collision col = new Collision(@"C:\Users\Dylan\Downloads\testcol.obj");
            //Collision col = new Collision(@"D:\Dropbox\Public\Collision Tutorial Utilities\Lunaboy's RARC Tools\map13\col.mp");
			//col.SaveFile(@"D:\Dropbox\Public\Collision Tutorial Utilities\Lunaboy's RARC Tools\map2\col.mp", Collision.FileType.Compiled);
			col.SaveFile(@"D:\SZS Tools\Luigi's Mansion\col_space_2.mp", Collision.FileType.Compiled);
			*/
		}

		private static void m_ShowHelpMessage()
		{
			Console.WriteLine("Luigi's Mansion Collision Converter written by Gamma/Sage of Mirrors.");
			Console.WriteLine("For any questions or issues, go to the github repo or contact @SageOfMirrors on twitter.");
			Console.WriteLine();
			Console.WriteLine("Usage:\t-inputfile <InFileName> -outputfile <OutFileName> -outtype <OutputType>");
			Console.WriteLine();
			Console.WriteLine("Output Types:");
			Console.WriteLine();
			Console.WriteLine("\tCompiled:\tOutputs compiled collision (.mp file and jmp folder) for use in Luigi's Mansion.");
			Console.WriteLine("\tJson:\t\tOutputs a human-readable JSON file based on the input for easy editing of properties.");
			Console.WriteLine("\tObj:\t\tOutputs a Wavefront OBJ file based on the input model.");
		}

		private static bool m_CheckArgs(string[] args, out string inputFile, out string outputFile, out Collision.FileType outputType)
		{
			inputFile = "";
			outputFile = "";
			outputType = Collision.FileType.none;

			for (int i = 0; i < args.Length; i += 2)
			{
				switch (args[i].ToLower())
				{
					case "-inputfile":
						inputFile = args[i + 1];
						break;
					case "-outputfile":
						outputFile = args[i + 1];
						break;
					case "-outtype":
						outputType = (Collision.FileType)Enum.Parse(typeof(Collision.FileType), args[i + 1]);
						break;
					default:
						Console.WriteLine($"Unidentified argument { args[i] }.");
						return false;
				}
			}

			if (inputFile == "")
			{
				Console.WriteLine("No input file was specified. Aborting...");
				return false;
			}

			if (outputFile == "")
			{
				string inputWithoutExt = $"{ Path.GetDirectoryName(inputFile) }\\{ Path.GetFileNameWithoutExtension(inputFile) }";
				string inputExt = Path.GetExtension(inputFile);
				string outputExt = "";

				if (outputType == Collision.FileType.none)
				{
					switch (inputExt)
					{
						case ".mp":
							outputType = Collision.FileType.obj;
							outputExt = ".obj";
							break;
						case ".json":
						default:
							outputType = Collision.FileType.compiled;
							outputExt = ".mp";
							break;
					}
				}
				else
				{
					switch (outputType)
					{
						case Collision.FileType.compiled:
							outputExt = ".mp";
							break;
						case Collision.FileType.json:
							outputExt = ".json";
							break;
						case Collision.FileType.obj:
							outputExt = ".obj";
							break;
					}
				}

				outputFile = inputWithoutExt + outputExt;
			}

			return true;
		}
	}
}
