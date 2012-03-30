using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotX86.Core.Cpu.Tables
{
	public class InstructionDecoder
	{
		static protected Instruction Instruction(Opcode Opcode)
		{
			return new Instruction()
			{
				Type = InstructionType.Empty,
				Opcode = Opcode,
			};
		}

		static protected Instruction Instruction(Opcode Opcode, Register Register)
		{
			return new Instruction()
			{
				Type = InstructionType.Register1,
				Opcode = Opcode,
				Register1 = Register,
			};
		}

		static protected Instruction Instruction(Opcode Opcode, Register Register1, Register Register2)
		{
			return new Instruction()
			{
				Type = InstructionType.Register2,
				Opcode = Opcode,
				Register1 = Register1,
				Register2 = Register2,
			};
		}


		static protected Instruction Instruction(Opcode Opcode, Register Register1, uint Value)
		{
			return new Instruction()
			{
				Type = InstructionType.Register1Value1,
				Opcode = Opcode,
				Register1 = Register1,
				Value = Value,
			};
		}

		static protected Instruction Instruction(Opcode Opcode, uint Value)
		{
			return new Instruction()
			{
				Type = InstructionType.Value1,
				Opcode = Opcode,
				Value = Value,
			};
		}

		static protected Instruction Instruction(Opcode Opcode, Register Register1, Register Register2, int Value)
		{
			return new Instruction()
			{
				Type = InstructionType.Register2Offset,
				Opcode = Opcode,
				Register1 = Register1,
				Register2 = Register2,
				Value = (uint)Value,
			};
		}

		static public Instruction DecodeInstruction(BinaryReader Reader)
		{
			var Byte = Reader.ReadByte();
			switch (Byte)
			{
				// PUSH AX/CX/DX/BX/SP/BP/SI/DI
				case 0x50:
				case 0x51:
				case 0x52:
				case 0x53:
				case 0x54:
				case 0x55:
				case 0x56:
				case 0x57:
					{
						return Instruction(Opcode.PUSH, (Register)(Byte - 0x50));
					}

				// Grpl Ew,Iw // 0x81
				// Grpl Ew,Ix // 0x83
				case 0x81:
				case 0x83:
					{
						var Param = Reader.ReadByte();
						var Which = (Param >> 3) & 7;

						if (Param >= 0xC0)
						{
							var Left = ((Param >> 0) & 7);
							int Value;
							
							switch (Byte)
							{
								case 0x81: Value = Reader.ReadInt32(); break;
								case 0x83: Value = Reader.ReadSByte(); break;
								default: throw(new NotImplementedException());
							}

							switch (Which)
							{
								case 0: return Instruction(Opcode.ADD, (Register)Left, (uint)Value);
								case 1: return Instruction(Opcode.OR, (Register)Left, (uint)Value);
								case 2: return Instruction(Opcode.ADC, (Register)Left, (uint)Value);
								case 3: return Instruction(Opcode.SBB, (Register)Left, (uint)Value);
								case 4: return Instruction(Opcode.AND, (Register)Left, (uint)Value);
								case 5: return Instruction(Opcode.SUB, (Register)Left, (uint)Value);
								case 6: return Instruction(Opcode.XOR, (Register)Left, (uint)Value);
								case 7: return Instruction(Opcode.CMP, (Register)Left, (uint)Value);
								default: throw (new NotImplementedException());
							}
							//throw (new NotImplementedException());
						}
						else
						{
							throw (new NotImplementedException());
						}
					}
				// MOV Ew,Gw
				case 0x89:
					{
						var Param = Reader.ReadByte();
						var Left = ((Param >> 0) & 7);
						var Right = ((Param >> 3) & 7);

						// MOV REG1, REG2
						if (Param >= 0xC0)
						{
							return Instruction(Opcode.MOV, (Register)Left, (Register)Right);
						}
						// MOV [REG1 + X], REG2
						else
						{
							var Offset = Reader.ReadSByte();
							return Instruction(Opcode.MOV, (Register)Left, (Register)Right, Offset);
						}
					}
				// XCHG AX, AX = NOP
				// XCHG AX/CX/DX/BX/SP/BP/SI/DI, AX
				case 0x90:
				case 0x91:
				case 0x92:
				case 0x93:
				case 0x94:
				case 0x95:
				case 0x96:
				case 0x97:
					{
						return Instruction(Opcode.XCHG, (Register)(Byte - 0x90), Register.AX);
					}

				//  MOV AX,Iw
				case 0xB8:
				case 0xB9:
				case 0xBA:
				case 0xBB:
				case 0xBC:
				case 0xBD:
				case 0xBE:
				case 0xBF:
					{
						var Value = Reader.ReadUInt32();
						return Instruction(Opcode.MOV, (Register)(Byte - 0xB8), Value);
					}
				// RETN
				case 0xC3:
					{
						return Instruction(Opcode.RETN);
					}
					break;
				// CALL Jw
				case 0xE8:
					{
						var Value = Reader.ReadUInt32();
						return Instruction(Opcode.CALL, Value);
					}
				default: throw(new NotImplementedException(String.Format("Unknown instruction with opcode 0x{0:X}", Byte)));
			}
		}
	}
}
