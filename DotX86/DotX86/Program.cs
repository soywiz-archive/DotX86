using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotX86.Core;
using DotX86.Core.Cpu;
using DotX86.Core.Cpu.Dynarec;

namespace DotX86
{
	class Program
	{
		static Action<int> Test;

		static void Main(string[] args)
		{
			/*
			Test = (a) =>
			{
			};
			*/

			//Test(1);

			var CpuContext = new CpuContext();
			var ThreadContext = new ThreadContext(CpuContext);
			var MethodGenerator = new MethodGenerator();
			ThreadContext.ESP = 0x10000;
			//ThreadContext.EAX = 10;

			CpuContext.Memory.Write(0x00401020, new byte[] {
				0x55, 0x89, 0xE5, 0x81, 0xEC, 0x14, 0x00, 0x00, 0x00, 0x90, 0xB8, 0x00, 0x00, 0x00, 0x00,
				0x89, 0x45, 0xEC,
				0xB8, 0x00, 0x00, 0x03, 0x00,
				0x50,
				0xB8, 0x00, 0x00, 0x01, 0x00,
				0x50,
				0xE8, 0x55, 0x00, 0x00, 0x00,
				0x83, 0xC4, 0x08,
				0xB8, 0x01, 0x00, 0x00, 0x00,
				0x50,
			});

			CpuContext.Memory.Write(0x00401098, new byte[] {
				0xC3,
			});

			ThreadContext.PC = 0x00401020;
			//ThreadContext.NextFunction = Method;

			while (true)
			{
				//var CurrentFunction = ThreadContext.NextFunction;
				//ThreadContext.NextFunction = null;
				//CurrentFunction(ThreadContext);
				var Method = CpuContext.GenerateMethod(ThreadContext.PC);
				Method(ThreadContext);
			}
			Console.WriteLine("Ended!");
			Console.ReadKey();
		}
	}
}
