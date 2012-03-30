using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotX86.Core.Cpu.Tables
{
	public enum Opcode
	{
		PUSH,
		MOV,
		ADD,
		SUB,
		XCHG,
		CALL,
		RETN,
		OR,
		ADC,
		SBB,
		AND,
		XOR,
		CMP,
		JMP,
		INT,
		LEA,
		LEAVE,
		JGE,
	}
}
