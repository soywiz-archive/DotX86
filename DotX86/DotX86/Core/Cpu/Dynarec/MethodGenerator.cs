//#define GENERATOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using DotX86.Core.Cpu.Tables;

namespace DotX86.Core.Cpu.Dynarec
{
	public class MethodGenerator
	{
		sealed protected class Generator
		{
			CpuContext CpuContext;
			Stream Stream;
			BinaryReader Reader;
			DynamicMethod DynamicMethod;
			ILGenerator ILGenerator;

			public Generator(CpuContext CpuContext, Stream Stream)
			{
				this.CpuContext = CpuContext;
				this.Stream = Stream;
				this.Reader = new BinaryReader(Stream);

				this.DynamicMethod = new DynamicMethod(
					"MethodGenerator.GenerateMethod",
					typeof(void),
					new Type[] { typeof(ThreadContext) },
					Assembly.GetExecutingAssembly().ManifestModule
				);
				this.ILGenerator = DynamicMethod.GetILGenerator();
			}

			public Action<ThreadContext> CreateDelegate()
			{
				return (Action<ThreadContext>)DynamicMethod.CreateDelegate(typeof(Action<ThreadContext>));
			}

			internal void GenerateMethod()
			{
				while (Stream.Position < Stream.Length)
				{
					var PC = (uint)Stream.Position;
#if GENERATOR
					Console.Write("# 0x{0:X}: ", PC);
#endif
					var Instruction = InstructionDecoder.DecodeInstruction(Reader);
					var nPC = (uint)Stream.Position;
#if GENERATOR
					Console.WriteLine("{0}", Instruction);
#endif

#if GENERATOR
					LoadThreadContext();
					ILGenerator.Emit(OpCodes.Ldc_I4, PC);
					ILGenerator.Emit(OpCodes.Ldstr, Instruction.ToString());
					CallFunction((Action<ThreadContext, uint, string>)Operations.TRACE0);
#endif

					bool Result = EmitInstruction(PC, nPC, Instruction, ILGenerator);

#if GENERATOR
					LoadThreadContext();
					ILGenerator.Emit(OpCodes.Ldc_I4, PC);
					ILGenerator.Emit(OpCodes.Ldstr, Instruction.ToString());
					CallFunction((Action<ThreadContext, uint, string>)Operations.TRACE1);
#endif

					if (!Result) break;
				}
				ILGenerator.Emit(OpCodes.Ret);
			}

			private FieldInfo GetRegisterField(Register Register)
			{
				return typeof(ThreadContext).GetField(((ERegister)Register).ToString());
			}

			private void LoadThreadContext()
			{
				ILGenerator.Emit(OpCodes.Ldarg_0);
			}

			private void LoadValue(uint Value)
			{
				ILGenerator.Emit(OpCodes.Ldc_I4, Value);
			}

			private void LoadRegisterAddress(Register Register)
			{
				ILGenerator.Emit(OpCodes.Ldarg_0);
				ILGenerator.Emit(OpCodes.Ldflda, GetRegisterField(Register));
			}

			private void LoadRegisterValue(Register Register)
			{
				ILGenerator.Emit(OpCodes.Ldarg_0);
				ILGenerator.Emit(OpCodes.Ldfld, GetRegisterField(Register));
			}

			private void CallFunction(Delegate Delegate)
			{
				ILGenerator.Emit(OpCodes.Call, Delegate.Method);
			}

			private void CallTailFunction(Delegate Delegate)
			{
				ILGenerator.Emit(OpCodes.Tailcall, Delegate.Method);
			}

			private bool EmitInstruction(uint PC, uint nPC, Instruction Instruction, ILGenerator ILGenerator)
			{
				switch (Instruction.Opcode)
				{
					case Opcode.PUSH:
						{
							LoadThreadContext();
							LoadRegisterValue(Instruction.Register1);
							CallFunction((Action<ThreadContext, uint>)Operations.PUSH_DWORD);
						}
						break;
					case Opcode.MOV:
						//Console.WriteLine("     {0}", Instruction.Type);
						switch (Instruction.Type)
						{
							case InstructionType.RegisterValue:
								LoadRegisterAddress(Instruction.Register1);
								LoadValue(Instruction.Value);
								CallFunction((REF_DWORD_DELEGATE)Operations.MOV_DWORD);
								break;
							case InstructionType.RegisterRegister:
								LoadRegisterAddress(Instruction.Register1);
								LoadRegisterValue(Instruction.Register2);
								CallFunction((REF_DWORD_DELEGATE)Operations.MOV_DWORD);
								break;
							case InstructionType.RegisterOffsetRegister:
								LoadThreadContext();

								LoadRegisterValue(Instruction.Register1);
								LoadValue(Instruction.Value);
								ILGenerator.Emit(OpCodes.Add);

								LoadRegisterValue(Instruction.Register2);

								CallFunction((Action<ThreadContext, uint, uint>)Operations.STORE_DWORD);
								break;
							case InstructionType.RegisterRegisterOffset:
								LoadThreadContext();

								LoadRegisterAddress(Instruction.Register1);

								LoadRegisterValue(Instruction.Register2);
								LoadValue(Instruction.Value);
								ILGenerator.Emit(OpCodes.Add);

								CallFunction((TCTX_REF_DWORD_DELEGATE)Operations.LOAD_DWORD);
								break;
							default:
								throw(new NotImplementedException());
						}

						break;
					case Opcode.LEA:
						switch (Instruction.Type)
						{
							// LEA REG, [REG + OFF]
							case InstructionType.RegisterRegisterOffset:
								LoadRegisterAddress(Instruction.Register1);
								LoadRegisterValue(Instruction.Register2);
								LoadValue(Instruction.Value);
								ILGenerator.Emit(OpCodes.Add);

								CallFunction((REF_DWORD_DELEGATE)Operations.MOV_DWORD);
								break;
							default:
								throw (new NotImplementedException());
						}
						break;
					case Opcode.SUB:
						{
							switch (Instruction.Type)
							{
								case InstructionType.RegisterValue:
									LoadRegisterAddress(Instruction.Register1);
									LoadValue(Instruction.Value);
									CallFunction((REF_DWORD_DELEGATE)Operations.SUB_DWORD);
									break;
								default:
									throw (new NotImplementedException());
							}
						}
						break;
					case Opcode.ADD:
						{
							switch (Instruction.Type)
							{
								case InstructionType.RegisterValue:
									LoadRegisterAddress(Instruction.Register1);
									LoadValue(Instruction.Value);
									CallFunction((REF_DWORD_DELEGATE)Operations.ADD_DWORD);
									break;
								default:
									LoadRegisterAddress(Instruction.Register1);
									LoadRegisterValue(Instruction.Register2);
									CallFunction((REF_DWORD_DELEGATE)Operations.ADD_DWORD);
									break;
							}
						}
						break;
					case Opcode.CMP:
						{
							switch (Instruction.Type)
							{
								case InstructionType.RegisterValue:
									LoadThreadContext();
									LoadRegisterValue(Instruction.Register1);
									LoadValue(Instruction.Value);
									CallFunction((Action<ThreadContext, int, int>)Operations.CMP_DWORD);
									break;
								default:
									throw (new NotImplementedException());
							}
						}
						break;
					case Opcode.XCHG:
						if (Instruction.Register1 != Instruction.Register2)
						{
							LoadRegisterAddress(Instruction.Register1);
							LoadRegisterAddress(Instruction.Register2);
							CallFunction((REF_REF_DELEGATE)Operations.XCHG_DWORD);
						}
						break;
					case Opcode.RETN:
						{
							LoadThreadContext();
							CallFunction((Action<ThreadContext>)Operations.RETURN);

							return false;
						}
					case Opcode.JGE:
						{
							switch (Instruction.Type)
							{
								case InstructionType.Value:
									LoadThreadContext();
									LoadValue((uint)(nPC));
									LoadValue((uint)(nPC + Instruction.Value));
									CallFunction((Action<ThreadContext, uint, uint>)Operations.JUMP_GREATER_EQUAL);
									break;
								default:
									throw (new NotImplementedException());
							}
							return false;
						}
						break;
					case Opcode.JMP:
						{
							switch (Instruction.Type)
							{
								case InstructionType.Indirect:
									LoadThreadContext();
									LoadValue((uint)Instruction.Value);
									CallFunction((Action<ThreadContext, uint>)Operations.JUMP_INDIRECT);
									break;
								case InstructionType.Value:
									LoadThreadContext();
									LoadValue((uint)(nPC + Instruction.Value));
									CallFunction((Action<ThreadContext, uint>)Operations.JUMP);
									break;
								default:
									throw(new NotImplementedException());
							}
							return false;
						}
					case Opcode.INT:
						{
							LoadThreadContext();
							LoadValue(nPC);
							LoadValue(Instruction.Value);
							CallFunction((Action<ThreadContext, uint, uint>)Operations.INTERRUPT);
							return false;
						}
					case Opcode.LEAVE:
						{
							LoadThreadContext();
							CallFunction((Action<ThreadContext>)Operations.LEAVE);
							//throw(new NotImplementedException());
						}
						break;
					case Opcode.CALL:
						{
							uint ReturnPC = nPC;
							uint CallPC = ReturnPC + Instruction.Value;

							LoadThreadContext();
							LoadValue((uint)ReturnPC);
							LoadValue((uint)CallPC);
							CallFunction((Action<ThreadContext, uint, uint>)Operations.CALL);
							return false;

							/*
							int AllocatedIndex = 0;

							Console.WriteLine("CALL: {0:X}, {1:X}, {2:X}", CallPC, PC, Instruction.Value);

							Action<uint, int, Instruction> Context = (ContextCallPC, ContextAllocatedIndex, ContextInstruction) =>
							{
								CpuContext.MethodCache[ContextAllocatedIndex] = (ThreadState) =>
								{
									Console.WriteLine("Generating method : {0} : {1:X}", ContextInstruction, ContextCallPC);
									var Method = CpuContext.GenerateMethod(ContextCallPC);
									CpuContext.MethodCache[ContextAllocatedIndex] = Method;
									Method(ThreadState);
								};
							};
							Context(CallPC, AllocatedIndex, Instruction);

							LoadThreadContext();
							LoadValue((uint)AllocatedIndex);
							//CallTailFunction((Action<ThreadContext, int>)Operations.CALL_INDEX);
							CallFunction((Action<ThreadContext, int>)Operations.CALL_INDEX);
							ILGenerator.Emit(OpCodes.Ret);
							*/

							/*
							LoadThreadContext();
							LoadThreadContext();
							ILGenerator.Emit(OpCodes.Ldfld, typeof(ThreadContext).GetField("TestMethod"));
							//ILGenerator.Emit(OpCodes.Callvirt, typeof(Action<ThreadContext>).GetMethod("Invoke"));
							CallFunction((Action<ThreadContext, Action<ThreadContext>>)Operations.CALLVIRT);
							*/

							
						}
					default:
						throw (new NotImplementedException("Unimplemented opcode " + Instruction.Opcode));
				}
				return true;
			}
		}

		public Action<ThreadContext> GenerateMethod(CpuContext CpuContext, Stream Stream)
		{
			var Generator = new Generator(CpuContext, Stream);
			//Console.WriteLine("Generate");
			Generator.GenerateMethod();
			return Generator.CreateDelegate();
		}
	}
}
