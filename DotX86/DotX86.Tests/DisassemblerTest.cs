using System;
using System.IO;
using DotX86.Core.Cpu.Assembler;
using DotX86.Core.Cpu.Tables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotX86.Tests
{
	[TestClass]
	public class DisassemblerTest
	{
		[TestMethod]
		public void TestDisassemble()
		{
			var Stream = new MemoryStream(new byte[] { 0x50 });
			var Reader = new BinaryReader(Stream);
			var Disassembler = new Disassembler();
			Assert.AreEqual("PUSH AX", Disassembler.Disassemble(InstructionDecoder.DecodeInstruction(Reader)));
		}
	}
}
