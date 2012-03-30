using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotX86.Core.Cpu.Dynarec;

namespace DotX86.Core.Cpu
{
	public class NativeMethodInfo
	{
		public string DllName;
		public string Name;
		public Action<NativeMethodInfo, ThreadContext> Method;
	}

	public class CpuContext
	{
		public Memory Memory = new Memory();

		public MethodGenerator MethodGenerator = new MethodGenerator();

		public Dictionary<uint, NativeMethodInfo> NativeMethodInfoList = new Dictionary<uint, NativeMethodInfo>();

		//public Action<ThreadContext>[] MethodCache = new Action<ThreadContext>[16 * 1024];

		public Action<ThreadContext> GenerateMethod(uint PC)
		{
			var Stream = Memory.GetStream().Slice();
			Stream.Position = PC;
			return MethodGenerator.GenerateMethod(this, Stream);
		}
	}
}
