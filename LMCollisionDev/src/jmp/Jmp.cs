using System;
using System.Collections.Generic;
using System.IO;
using GameFormatReader.Common;

namespace LMCollisionDev
{
	public class Jmp
	{
		public enum JmpDataTypes : byte
		{
			Integer,
			String,
			Float
		}

		public struct JmpField
		{
			public NameHashes Name;
			public int Bitmask;
			public ushort Offset;
			public byte BitShift;
			public JmpDataTypes DataType;

			public override string ToString()
			{
				return $"{ Name } : { DataType }";
			}
		}

		public Dictionary<NameHashes, JmpField> Fields { get; private set; }

		public Jmp()
		{
		}

		public Jmp(string fileName)
		{
			Fields = new Dictionary<NameHashes, JmpField>();

			using (FileStream strm = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				EndianBinaryReader reader = new EndianBinaryReader(strm, Endian.Big);

				int entryCount = reader.ReadInt32();
				int fieldCount = reader.ReadInt32();
				int entryDataOffset = reader.ReadInt32();
				int entryLength = reader.ReadInt32();

				for (int i = 0; i < fieldCount; i++)
				{
					JmpField field = new JmpField();

					field.Name = (NameHashes)reader.ReadInt32();
					field.Bitmask = reader.ReadInt32();
					field.Offset = reader.ReadUInt16();
					field.BitShift = reader.ReadByte();
					field.DataType = (JmpDataTypes)reader.ReadByte();

					Fields.Add(field.Name, field);
				}

				// Just in case. Can never be too sure
				reader.BaseStream.Seek(entryDataOffset, SeekOrigin.Begin);

				int curOffset = entryDataOffset;
				for (int i = 0; i < entryCount; i++)
				{
					
					curOffset += entryLength;
				}
			}
		}
	}
}
