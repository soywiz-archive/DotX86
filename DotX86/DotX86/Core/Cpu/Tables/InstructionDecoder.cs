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
				Type = InstructionType.Register,
				Opcode = Opcode,
				Register1 = Register,
			};
		}

		static protected Instruction Instruction(Opcode Opcode, Register Register1, Register Register2)
		{
			return new Instruction()
			{
				Type = InstructionType.RegisterRegister,
				Opcode = Opcode,
				Register1 = Register1,
				Register2 = Register2,
			};
		}


		static protected Instruction Instruction(Opcode Opcode, Register Register1, uint Value)
		{
			return new Instruction()
			{
				Type = InstructionType.RegisterValue,
				Opcode = Opcode,
				Register1 = Register1,
				Value = Value,
			};
		}

		static protected Instruction Instruction(Opcode Opcode, uint Value)
		{
			return new Instruction()
			{
				Type = InstructionType.Value,
				Opcode = Opcode,
				Value = Value,
			};
		}

		static protected Instruction Instruction(Opcode Opcode, Register Register1, Register Register2, int Value)
		{
			return new Instruction()
			{
				Type = InstructionType.RegisterRegisterOffset,
				Opcode = Opcode,
				Register1 = Register1,
				Register2 = Register2,
				Value = (uint)Value,
			};
		}

		static protected Instruction Instruction(Opcode Opcode, Register Register1, int Value, Register Register2)
		{
			return new Instruction()
			{
				Type = InstructionType.RegisterOffsetRegister,
				Opcode = Opcode,
				Register1 = Register1,
				Register2 = Register2,
				Value = (uint)Value,
			};
		}

		static public Instruction DecodeInstruction_0F(uint PC, BinaryReader Reader)
		{
			var Byte = Reader.ReadByte();
			switch (Byte)
			{
				// JNL / JGE
				// http://en.wikibooks.org/wiki/X86_Assembly/Control_Flow
				case 0x8D:
					{
						var Value = Reader.ReadInt32();
						return Instruction(Opcode.JGE, (uint)Value);
					}
					throw (new NotImplementedException());
					break;
				default: throw (new NotImplementedException(String.Format("Unknown instruction with opcode 0x0F + 0x{0:X} at 0x{1:X}", Byte, PC)));
			}
		}

		static public Instruction DecodeInstruction(BinaryReader Reader)
		{
			var PC = (uint)Reader.BaseStream.Position;
			return DecodeInstruction(PC, Reader);
		}

		static public Instruction DecodeInstruction(uint PC, BinaryReader Reader)
		{
			var Byte = Reader.ReadByte();
			switch (Byte)
			{
				// ADD Ew, Gw
				case 0x01:
					{
						var Param = Reader.ReadByte();
						var Left = ((Param >> 0) & 7);
						var Right = ((Param >> 3) & 7);
						return Instruction(Opcode.ADD, (Register)Left, (Register)Right);
					}

				// Prefix 0x0F
				case 0x0F:
					{
						return DecodeInstruction_0F(PC, Reader);
					}

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
							return Instruction(Opcode.MOV, (Register)Left, Offset, (Register)Right);
						}
					}
				// MOV Gw,Ew
				case 0x8B:
					{
						var Param = Reader.ReadByte();
						var Right = (Register)((Param >> 0) & 7);
						var Left = (Register)((Param >> 3) & 7);

						// MOV REG1, REG2
						if (Param >= 0xC0)
						{
							throw(new NotImplementedException());
						}
						// MOV [REG1 + X], REG2
						else
						{
							var Offset = Reader.ReadSByte();
							return Instruction(Opcode.MOV, (Register)Left, (Register)Right, Offset);
						}
					}
					break;
				// LEA Gw
				case 0x8D:
					{
						var Param = Reader.ReadByte();
						var Right = ((Param >> 0) & 7);
						var Left = ((Param >> 3) & 7);
						var Offset = Reader.ReadSByte();
						return Instruction(Opcode.LEA, (Register)Left, (Register)Right, Offset);
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
				// LEAVE
				case 0xC9:
					{
						return Instruction(Opcode.LEAVE);
					}
					break;
				// INT Ib
				case 0xCD:
					{
						var Num = Reader.ReadByte();
						return Instruction(Opcode.INT, (uint)Num);
					}
				// CALL Jw
				case 0xE8:
					{
						var Value = Reader.ReadUInt32();
						return Instruction(Opcode.CALL, Value);
					}
				// JMP Jw
				case 0xE9:
				{
					var Address = Reader.ReadInt32();
					return new Instruction()
					{
						Type = InstructionType.Value,
						Opcode = Opcode.JMP,
						Value = (uint)Address,
					};
				}
				// JMP Jb
				case 0xEB:
					{
						var Address = Reader.ReadSByte();
						return new Instruction()
						{
							Type = InstructionType.Value,
							Opcode = Opcode.JMP,
							Value = (uint)Address,
						};
					}
					break;
				// GRP5 Ew
				case 0xFF:
					{
						var Param = Reader.ReadByte();
						var Which = (Param >> 3) & 7;
						switch (Which)
						{
							// INC Ew
							//case 0:
							//	break;
							// DEC Ew
							//case 1:
							//	break;
							// CALL Ev
							//case 2:
							//	break;
							// CALL Ep
							//case 3:
							//	break;
							// JMP Ev
							case 4:
								if (Param >= 0xC0)
								{
									throw (new NotImplementedException());
								}
								else
								{
									var Address = Reader.ReadInt32();
									//throw(new NotImplementedException());
									return new Instruction()
									{
										Type = InstructionType.Indirect,
										Opcode = Opcode.JMP,
										Value = (uint)Address,
									};
								}
							// JMP Ep
							//case 5:
							//	break;
							// PUSH Ev
							//case 6:
							//	break;
							default: throw (new NotImplementedException("" + Which));
						}
					}
				default: throw(new NotImplementedException(String.Format("Unknown instruction with opcode 0x{0:X} at 0x{1:X}", Byte, PC)));
			}
		}
	}
}
