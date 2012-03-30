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
					Console.Write("{0:X} ", PC);
					var Instruction = InstructionDecoder.DecodeInstruction(Reader);
					Console.WriteLine("{0}", Instruction);
					if (!EmitInstruction(PC, Instruction, ILGenerator))
					{
						break;
					}
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

			private bool EmitInstruction(uint PC, Instruction Instruction, ILGenerator ILGenerator)
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
						switch (Instruction.Type)
						{
							case InstructionType.Register2:
								LoadRegisterAddress(Instruction.Register1);
								LoadRegisterValue(Instruction.Register2);
								CallFunction((REF_DWORD_DELEGATE)Operations.MOV_DWORD);
								break;
							case InstructionType.Register1Value1:
								LoadRegisterAddress(Instruction.Register1);
								LoadValue(Instruction.Value);
								CallFunction((REF_DWORD_DELEGATE)Operations.MOV_DWORD);
								break;
							case InstructionType.Register2Offset:
								LoadThreadContext();

								LoadRegisterValue(Instruction.Register1);
								LoadValue(Instruction.Value);
								ILGenerator.Emit(OpCodes.Add);

								LoadRegisterValue(Instruction.Register2);

								CallFunction((Action<ThreadContext, uint, uint>)Operations.STORE_DWORD);
								break;
							default:
								throw(new InvalidDataException());
						}

						break;
					case Opcode.SUB:
						{
							LoadRegisterAddress(Instruction.Register1);
							LoadValue(Instruction.Value);
							CallFunction((REF_DWORD_DELEGATE)Operations.SUB_DWORD);
						}
						break;
					case Opcode.ADD:
						{
							LoadRegisterAddress(Instruction.Register1);
							LoadValue(Instruction.Value);
							CallFunction((REF_DWORD_DELEGATE)Operations.ADD_DWORD);
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
						break;
					case Opcode.CALL:
						{
							uint ReturnPC = PC + 5;
							uint CallPC = ReturnPC + Instruction.Value;

							LoadThreadContext();
							LoadValue((uint)ReturnPC);
							LoadValue((uint)CallPC);
							CallFunction((Action<ThreadContext, uint, uint>)Operations.CALL);

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

							return false;
						}
						break;
					default:
						throw (new NotImplementedException("Unimplemented opcode " + Instruction.Opcode));
				}
				return true;
			}
		}

		public Action<ThreadContext> GenerateMethod(CpuContext CpuContext, Stream Stream)
		{
			var Generator = new Generator(CpuContext, Stream);
			Console.WriteLine("Generate");
			Generator.GenerateMethod();
			return Generator.CreateDelegate();
		}
	}
}
