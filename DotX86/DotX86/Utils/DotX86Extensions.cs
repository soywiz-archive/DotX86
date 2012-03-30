using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
	static public class DotX86Extensions
	{
		static public TType ReadStruct<TType>(this Stream Stream)
		{
			var Type = typeof(TType);
			byte[] Buffer = new byte[Marshal.SizeOf(Type)];
			Stream.Read(Buffer, 0, Marshal.SizeOf(Type));
			GCHandle Handle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
			TType Temp = (TType)Marshal.PtrToStructure(Handle.AddrOfPinnedObject(), Type);
			Handle.Free();
			return Temp;
		}

		static public string ReadStringz(this Stream Stream, Encoding Encoding = null)
		{
			if (Encoding == null) Encoding = Encoding.UTF8;
			var Out = new MemoryStream();
			byte Byte;
			while ((Byte = (byte)Stream.ReadByte()) != 0) Out.WriteByte(Byte);
			return Encoding.GetString(Out.ToArray());
		}
	}
}
