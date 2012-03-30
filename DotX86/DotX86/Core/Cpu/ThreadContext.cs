using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotX86.Core.Cpu
{
	public class ThreadContext
	{
		public uint PC;

		public uint EAX;
		public uint ECX;
		public uint EDX;
		public uint EBX;
		public uint ESP;
		public uint EBP;
		public uint ESI;
		public uint EDI;

		public CpuContext CpuContext;

		//public Action<ThreadContext> NextFunction;

		//public Action<ThreadContext> TestMethod;

		public ThreadContext(CpuContext CpuContext)
		{
			this.CpuContext = CpuContext;
			/*
			this.TestMethod = (ThreadContext) =>
			{
				Console.WriteLine("TestMethod!");
			};
			*/
		}
	}
}
