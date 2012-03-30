using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotX86.Core.Cpu.Dynarec
{
	delegate void TCTX_REF_DWORD_DELEGATE(ThreadContext ThreadContext, ref uint Pointer, uint Value);
	delegate void REF_DWORD_DELEGATE(ref uint Pointer, uint Value);
	delegate void REF_REF_DELEGATE(ref uint Pointer, ref uint Value);

	public partial class Operations
	{
		static public void PUSH_DWORD(ThreadContext ThreadContext, uint Value)
		{
			//Console.WriteLine("PUSH_DWORD");
			ThreadContext.ESP -= 4;
			ThreadContext.CpuContext.Memory.Write4(ThreadContext.ESP, Value);
		}

		static public uint POP_DWORD(ThreadContext ThreadContext)
		{
			//Console.WriteLine("PUSH_DWORD");
			try
			{
				return ThreadContext.CpuContext.Memory.Read4(ThreadContext.ESP);
			}
			finally
			{
				ThreadContext.ESP += 4;
			}
		}

		static public void STORE_DWORD(ThreadContext ThreadContext, uint Address, uint Value)
		{
			ThreadContext.CpuContext.Memory.Write4(Address, Value);
		}

		static public void LOAD_DWORD(ThreadContext ThreadContext, ref uint Pointer, uint Address)
		{
			Pointer = ThreadContext.CpuContext.Memory.Read4(Address);
		}

		static public void MOV_DWORD(ref uint Pointer, uint Value)
		{
			Pointer = Value;
		}

		static public void SUB_DWORD(ref uint Pointer, uint Value)
		{
			Pointer -= Value;
		}

		static public void ADD_DWORD(ref uint Pointer, uint Value)
		{
			Pointer += Value;
		}

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

		static public void XCHG_DWORD(ref uint Left, ref uint Right)
		{
			uint Temp = Left;
			Left = Right;
			Right = Temp;
		}

		static public void CALL(ThreadContext ThreadContext, uint ReturnPC, uint NewAddress)
		{
			PUSH_DWORD(ThreadContext, ReturnPC);
			ThreadContext.PC = NewAddress;
		}

		static public void JUMP_INDIRECT(ThreadContext ThreadContext, uint Address)
		{
			var JumpAddress = ThreadContext.CpuContext.Memory.Read4(Address);
			ThreadContext.PC = JumpAddress;
		}

		static public void JUMP(ThreadContext ThreadContext, uint JumpAddress)
		{
			ThreadContext.PC = JumpAddress;
		}

		internal static void JUMP_GREATER_EQUAL(ThreadContext ThreadContext, uint CurrentAddress, uint JumpAddress)
		{
			// SignFlag == OverflowFlag
			if (ThreadContext.Flags.SF == ThreadContext.Flags.OF)
			{
				ThreadContext.PC = JumpAddress;
			}
			else
			{
				ThreadContext.PC = CurrentAddress;
			}
		}


		static public void RETURN(ThreadContext ThreadContext)
		{
			ThreadContext.PC = POP_DWORD(ThreadContext);
		}

		static public void CALLVIRT(ThreadContext ThreadContext, Action<ThreadContext> Method)
		{
			Method(ThreadContext);
		}

		/*
		static public void CALL_INDEX(ThreadContext ThreadContext, int Index)
		{
			ThreadContext.NextFunction = ThreadContext.CpuContext.MethodCache[Index];
			//ThreadContext.CpuContext.MethodCache[Index](ThreadContext);
		}

		static public void RETURN(ThreadContext ThreadContext)
		{
			ThreadContext.NextFunction = ThreadContext.CpuContext.MethodCache[Index];
			//ThreadContext.CpuContext.MethodCache[Index](ThreadContext);
		}
		*/

		static public void INTERRUPT(ThreadContext ThreadContext, uint nPC, uint Code)
		{
			switch (Code)
			{
				case 1:
					var NativeMethod = ThreadContext.CpuContext.NativeMethodInfoList[ThreadContext.CpuContext.Memory.Read4(nPC)];
					NativeMethod.Method(NativeMethod, ThreadContext);
					//Console.WriteLine(NativeMethod.);
					//Console.WriteLine("INTERRUPT: {0}", Code);
					//Console.ReadKey(); Environment.Exit(0);
					RETURN(ThreadContext);
					break;
				default:
					throw(new NotImplementedException("Interrupt : " + Code));
			}
			//throw new NotImplementedException();
		}

		static public void LEAVE(ThreadContext ThreadContext)
		{
			ThreadContext.ESP = ThreadContext.EBP;
			ThreadContext.EBP = POP_DWORD(ThreadContext);
		}

		static public void TRACE0(ThreadContext ThreadContext, uint PC, string Instruction)
		{
			Console.WriteLine(" + 0x{0:X} : {1}", PC, Instruction);
		}

		static public void TRACE1(ThreadContext ThreadContext, uint PC, string Instruction)
		{
			Console.WriteLine(" | $$ {0}", ThreadContext);
		}
	}
}
