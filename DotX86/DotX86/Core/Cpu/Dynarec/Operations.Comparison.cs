using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotX86.Core.Cpu.Dynarec
{
	public partial class Operations
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="ThreadContext"></param>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		static public void CMP_DWORD(ThreadContext ThreadContext, int Left, int Right)
		{
			int Result;

			Result = Left - Right;

			//Console.WriteLine("######## CMP: 0x{1:X} - 0x{2:X} = 0x{0:X}", Result, Left, Right);

			ThreadContext.Flags.OF = false;
			ThreadContext.Flags.ZF = (Result == 0);
			ThreadContext.Flags.SF = (Result < 0);

			//if (Right < Left) ThreadContext.Flags.OF = true;

			try
			{
				Result = checked(Left - Right);
			}
			catch (OverflowException)
			{
				ThreadContext.Flags.OF = true;
			}
		}

		/// <summary>
		/// In the x86 assembly language, the TEST instruction performs a bitwise AND on two operands.
		/// The flags SF, ZF, PF, CF, OF and AF are modified while the result of the AND is discarded.
		/// There are 9 different opcodes for the TEST instruction depending on the type and size of the operands.
		/// It can compare 8-bit, 16-bit, 32-bit or 64-bit values. It can also compare registers, immediate values and register indirect values.[1]
		/// </summary>
		/// <param name="ThreadContext"></param>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <see cref="http://en.wikipedia.org/wiki/TEST_(x86_instruction)"/>
		static public void TEST_DWORD(ThreadContext ThreadContext, int Left, int Right)
		{
			var Result = (Left & Right);

			ThreadContext.Flags.ZF = (Result == 0);

			// @TODO
			//SF, ZF, PF, CF, OF and AF 
			//throw new NotImplementedException();
		}
	}
}
