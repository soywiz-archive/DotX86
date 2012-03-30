using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotX86.Core.Cpu.Tables;

namespace DotX86.Core.Cpu.Assembler
{
	public enum DisassemblerMode
	{
		i386,
	}

	public class Disassembler
	{
		DisassemblerMode Mode;

		public Disassembler(DisassemblerMode Mode = DisassemblerMode.i386)
		{
			this.Mode = Mode;
		}

		public string Disassemble(Instruction Instruction)
		{
			switch (Instruction.Type)
			{
				case InstructionType.Empty: return String.Format("{0}", Instruction.Opcode);
				case InstructionType.Register: return String.Format("{0} {1}", Instruction.Opcode, (ERegister)Instruction.Register1);
				case InstructionType.Value: return String.Format("{0} {1}", Instruction.Opcode, (int)Instruction.Value);
				case InstructionType.Indirect: return String.Format("{0} [0x{1:X}]", Instruction.Opcode, Instruction.Value);
				case InstructionType.RegisterRegister: return String.Format("{0} {1}, {2}", Instruction.Opcode, (ERegister)Instruction.Register1, (ERegister)Instruction.Register2);
				case InstructionType.RegisterValue: return String.Format("{0} {1}, {2}", Instruction.Opcode, (ERegister)Instruction.Register1, Instruction.Value);
				case InstructionType.RegisterOffsetRegister: return String.Format("{0} [{1}+{2}], {3}", Instruction.Opcode, (ERegister)Instruction.Register1, (int)Instruction.Value, (ERegister)Instruction.Register2);
				case InstructionType.RegisterRegisterOffset: return String.Format("{0} {1}, [{2}+{3}]", Instruction.Opcode, (ERegister)Instruction.Register1, (ERegister)Instruction.Register2, (int)Instruction.Value);
				default: throw(new Exception("Not implemented " + Instruction.Type));
			}
		}
	}
}
