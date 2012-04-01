using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotX86.Core.Cpu
{
	public class ThreadContext
	{
		public struct FlagsStruct
		{
			/*
			0	CF	Carry flag	S
			1	1	Reserved	 
			2	PF	Parity flag	S
			3	0	Reserved	 
			4	AF	Adjust flag	S
			5	0	Reserved	 
			6	ZF	Zero flag	S
			7	SF	Sign flag	S
			8	TF	Trap flag (single step)	X
			9	IF	Interrupt enable flag	C
			10	DF	Direction flag	C
			11	OF	Overflow flag	S
			12, 13	1,1 / IOPL	I/O privilege level (286+ only) always 1 on 8086 and 186	X
			14	1 / NT	Nested task flag (286+ only) always 1 on 8086 and 186	X
			15	1 on 8086 and 186, should be 0 above	Reserved	 
			EFLAGS
			16	RF	Resume flag (386+ only)	X
			17	VM	Virtual 8086 mode flag (386+ only)	X
			18	AC	Alignment check (486SX+ only)	X
			19	VIF	Virtual interrupt flag (Pentium+)	X
			20	VIP	Virtual interrupt pending (Pentium+)	X
			21	ID	Able to use CPUID instruction (Pentium+)	X
			22	0	Reserved	 
			23	0	Reserved	 
			24	0	Reserved	 
			25	0	Reserved	 
			26	0	Reserved	 
			27	0	Reserved	 
			28	0	Reserved	 
			29	0	Reserved	 
			30	0	Reserved	 
			31	0	Reserved	
			*/
			
			/// <summary>
			/// Carry Flag
			/// </summary>
			public bool CF;

			/// <summary>
			/// Parity Flag
			/// </summary>
			public bool PF;

			/// <summary>
			/// Adjust Flag
			/// </summary>
			public bool AF;

			/// <summary>
			/// Zero Flag
			/// </summary>
			public bool ZF;

			/// <summary>
			/// Sign Flag
			/// </summary>
			public bool SF;

			/// <summary>
			/// Trap Flag
			/// </summary>
			public bool TF;

			/// <summary>
			/// Interrupt Flag
			/// </summary>
			public bool IF;

			/// <summary>
			/// Direction Flag
			/// </summary>
			public bool DF;

			/// <summary>
			/// Overflow Flag
			/// </summary>
			public bool OF;
		}

		public FlagsStruct Flags;

		public uint PC;

		public uint EAX;
		public uint ECX;
		public uint EDX;
		public uint EBX;
		public uint ESP;
		public uint EBP;
		public uint ESI;
		public uint EDI;

		public long EDX_EAX
		{
			get
			{
				return (long)((EDX << 32) | (EAX << 0));
			}
			set
			{
				EDX = (uint)(value >> 32);
				EAX = (uint)(value >> 0);
			}
		}

		public CpuContext CpuContext;
		public Memory Memory { get { return CpuContext.Memory; } }
		public Stream MemoryStream { get { return Memory.GetStream(); } }

		public uint ReadStack(int Index)
		{
			return CpuContext.Memory.Read4((uint)(ESP + Index * 4));
		}

		//public Action<ThreadContext> NextFunction;

		//public Action<ThreadContext> TestMethod;

		public ThreadContext(CpuContext CpuContext)
		{
			this.CpuContext = CpuContext;
			/*
			this.TestMethod = (ThreadContext) =>
			{
				Console.WriteLine("TestMethod!");
			};
			*/
		}

		public override string ToString()
		{
			return String.Format("ThreadContext(EAX=0x{0:X}, ECX=0x{1:X}, EDX=0x{2:X}, EBX=0x{3:X}, ESP=0x{4:X}, EBP=0x{5:X}, ESI=0x{6:X}, EDI=0x{7:X})", EAX, ECX, EDX, EBX, ESP, EBP, ESI, EDI);
		}
	}
}
