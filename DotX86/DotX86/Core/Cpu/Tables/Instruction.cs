using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotX86.Core.Cpu.Assembler;

namespace DotX86.Core.Cpu.Tables
{
	public enum InstructionType
	{
		Empty,
		Value,
		Register,
		RegisterRegister,
		RegisterValue,
		RegisterRegisterOffset,
		RegisterOffsetRegister,
		Indirect,
		//Relative,
	}

	public struct Instruction
	{
		public InstructionType Type;
		public Opcode Opcode;
		public Register Register1;
		public Register Register2;
		public uint Value;

		public override string ToString()
		{
			return new Disassembler().Disassemble(this);
		}
	}
}
