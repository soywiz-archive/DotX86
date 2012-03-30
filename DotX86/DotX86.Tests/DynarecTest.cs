using System;
using System.IO;
using DotX86.Core;
using DotX86.Core.Cpu;
using DotX86.Core.Cpu.Dynarec;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotX86.Tests
{
	[TestClass]
	public class DynarecTest
	{
		[TestMethod]
		public void TestMethod1()
		{
			var CpuContext = new CpuContext();
			var ThreadContext = new ThreadContext(CpuContext);
			var MethodGenerator = new MethodGenerator();
			var Method = MethodGenerator.GenerateMethod(CpuContext, new MemoryStream(new byte[] { 0x50 }));
			Method(ThreadContext);
		}
	}
}
