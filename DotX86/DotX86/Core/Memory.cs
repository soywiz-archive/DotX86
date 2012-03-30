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

		public uint AllocStack(int Size)
		{
			//return (uint)Marshal.AllocHGlobal(Size).ToInt32();
			return 0x9000;
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
			//Console.WriteLine("WRITE4[0x{0:X}] = 0x{1:X}", Address, Value);
		}

		public uint Read4(uint Address)
		{
			try
			{
				var Value =
					(uint)(MemoryPointer[Address + 0] << 0) |
					(uint)(MemoryPointer[Address + 1] << 8) |
					(uint)(MemoryPointer[Address + 2] << 16) |
					(uint)(MemoryPointer[Address + 3] << 24)
				;

				//Console.WriteLine("READ4[0x{0:X}] -> 0x{1:X}", Address, Value);

				return Value;
			}
			catch (KeyNotFoundException)
			{
				//throw (new InvalidDataException(String.Format("Invalid address 0x{0:X}", Address)));
				return 0;
			}
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
			uint BaseAddress;
			uint _Position;

			public MemorySliceStream(Memory Memory, uint BaseAddress)
			{
				this.Memory = Memory;
				this.BaseAddress = BaseAddress;
				this._Position = 0;
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
					return Memory.Length - BaseAddress;
				}
			}

			public override long Position
			{
				get
				{
					return _Position;
				}
				set
				{
					_Position = (uint)value;
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				switch (origin)
				{
					case SeekOrigin.Begin: _Position = (uint)offset; break;
					case SeekOrigin.Current: _Position = (uint)(_Position + offset); break;
					case SeekOrigin.End: _Position = (uint)(Length - offset); break;
				}
				return Position;
			}

			public override void SetLength(long value)
			{
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				try
				{
					for (int n = 0; n < count; n++)
					{
						buffer[offset + n] = Memory.MemoryPointer[BaseAddress + _Position];
						_Position++;
					}
					return count;
				}
				catch (KeyNotFoundException)
				{
					throw(new InvalidDataException(String.Format("Invalid address 0x{0:X}", BaseAddress + _Position)));
				}
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				for (int n = 0; n < count; n++)
				{
					Memory.MemoryPointer[BaseAddress + _Position] = buffer[offset + n];
					_Position++;
				}
			}
		}

		public Stream GetStream()
		{
			return new MemorySliceStream(this, 0);
		}

		public long Length { get {
			//return Memory.Length;
			return uint.MaxValue;
		} }
	}
}
