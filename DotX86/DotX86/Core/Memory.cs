using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotX86.Core
{
	public class Memory
	{
		//public byte[] MemoryPointer = new byte[0x80000];
		public Dictionary<uint, byte> MemoryPointer = new Dictionary<uint, byte>();

		public uint Alloc(int Size)
		{
			//return (uint)Marshal.AllocHGlobal(Size).ToInt32();
			return 0;
		}

		public void Free(uint Address)
		{
			//Marshal.FreeHGlobal(new IntPtr((int)Address));
		}

		public void Write4(uint Address, uint Value)
		{
			MemoryPointer[Address + 0] = (byte)((Value >> 0) & 0xFF);
			MemoryPointer[Address + 1] = (byte)((Value >> 8) & 0xFF);
			MemoryPointer[Address + 2] = (byte)((Value >> 16) & 0xFF);
			MemoryPointer[Address + 3] = (byte)((Value >> 24) & 0xFF);
			//Console.WriteLine("{0:X} <- {1:X}", Address, Value);
		}

		public uint Read4(uint Address)
		{
			return 
				(uint)(MemoryPointer[Address + 0] << 0) |
				(uint)(MemoryPointer[Address + 1] << 8) |
				(uint)(MemoryPointer[Address + 2] << 16) |
				(uint)(MemoryPointer[Address + 3] << 24)
			;
		}

		public void Write(uint Address, byte[] Data)
		{
			foreach (var Byte in Data)
			{
				MemoryPointer[Address++] = Byte;
			}
		}

		public class MemorySliceStream : Stream
		{
			Memory Memory;
			uint Address;

			public MemorySliceStream(Memory Memory, uint Address)
			{
				this.Memory = Memory;
				this.Address = Address;
			}

			public override bool CanRead
			{
				get { return true; }
			}

			public override bool CanSeek
			{
				get { return true;  }
			}

			public override bool CanWrite
			{
				get { return true; }
			}

			public override void Flush()
			{
			}

			public override long Length
			{
				get
				{
					return Memory.Length;
				}
			}

			public override long Position
			{
				get
				{
					return Address;
				}
				set
				{
					Address = (uint)value;
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				switch (origin)
				{
					case SeekOrigin.Begin: Address = (uint)offset; break;
					case SeekOrigin.Current: Address = (uint)(Address + offset); break;
					case SeekOrigin.End: Address = (uint)(Length - offset); break;
				}
				return Position;
			}

			public override void SetLength(long value)
			{
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				for (int n = 0; n < count; n++)
				{
					buffer[offset + n] = Memory.MemoryPointer[Address++];
				}
				return count;
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				for (int n = 0; n < count; n++)
				{
					Memory.MemoryPointer[Address++] = buffer[offset + n];
				}
			}
		}

		public Stream GetStreamAt(uint Address)
		{
			return new MemorySliceStream(this, Address);
		}

		public long Length { get {
			//return Memory.Length;
			return uint.MaxValue;
		} }
	}
}
