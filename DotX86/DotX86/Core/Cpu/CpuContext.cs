using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotX86.Core.Cpu.Dynarec;

namespace DotX86.Core.Cpu
{
	public class CpuContext
	{
		public Memory Memory = new Memory();

		public MethodGenerator MethodGenerator = new MethodGenerator();

		public Action<ThreadContext>[] MethodCache = new Action<ThreadContext>[16 * 1024];

		public Action<ThreadContext> GenerateMethod(uint PC)
		{
			return MethodGenerator.GenerateMethod(this, Memory.GetStreamAt(PC));
		}
	}
}
