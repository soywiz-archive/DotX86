using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotX86.Core;
using DotX86.Core.Cpu;
using DotX86.Core.Cpu.Dynarec;
using DotX86.Loader;

namespace DotX86
{
	class Program
	{
		//static Action<int> Test;

		static void Main(string[] args)
		{
			/*
			Test = (a) =>
			{
			};
			*/
			var CpuContext = new CpuContext();
			var ThreadContext = new ThreadContext(CpuContext);

			Console.SetWindowSize(160, 60);

			var Loader = new Win32PeLoader();
			if (args.Length > 0)
			{
				Loader.Load(File.OpenRead(args[0]), ThreadContext);
			}
			else
			{
				//Loader.Load(File.OpenRead(@"..\..\..\Samples\test.exe"), ThreadContext);
				Loader.Load(@"c:\dev\tcc\test.exe", ThreadContext);
			}

			//Console.WriteLine("$$ {0}", ThreadContext);

			while (true)
			{
				var Method = CpuContext.GetMethod(ThreadContext.PC);
				Method(ThreadContext);
			}
			//Console.WriteLine("Ended!");
			//Console.ReadKey();
		}
	}
}
