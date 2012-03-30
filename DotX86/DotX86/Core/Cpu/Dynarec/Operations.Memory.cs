using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotX86.Core.Cpu.Dynarec
{
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

		static public void STORE_DWORD(ThreadContext ThreadContext, uint Pointer, uint Value)
		{
			ThreadContext.CpuContext.Memory.Write4(Pointer, Value);
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
	}
}
