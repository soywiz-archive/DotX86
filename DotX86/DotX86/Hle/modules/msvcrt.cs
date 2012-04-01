using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotX86.Core.Cpu;

namespace DotX86.Hle.modules
{
	class X86HaltException : Exception
	{
	}

	public class msvcrt : module
	{
		public void printf(ThreadContext ThreadContext)
		{
			int StackPos = 1;
			var Format = ThreadContext.MemoryStream.SliceWithLength(ThreadContext.ReadStack(StackPos++)).ReadStringz();
			var Str = "";
			for (int n = 0; n < Format.Length; n++)
			{
				if (Format[n] == '%')
				{
					switch (Format[n + 1])
					{
						case 'd':
							Str += ThreadContext.ReadStack(StackPos++).ToString();
							n++;
							break;
						default:
							throw(new InvalidOperationException("Invalid format!"));
					}
				}
				else
				{
					Str += Format[n];
				}
			}
			Console.Write("{0}", Str);
		}

		public void _controlfp(ThreadContext ThreadContext)
		{
			var New = ThreadContext.ReadStack(1);
			var Mask = ThreadContext.ReadStack(2);
			//Console.WriteLine("_controlfp (New=0x{0:X}, Mask=0x{1:X})", New, Mask);
		}

		/// <summary>
		/// int __getmainargs(int * _Argc,  char *** _Argv,  char *** _Env,  int _DoWildCard, _startupinfo * _StartInfo);
		/// </summary>
		/// <param name="ThreadContext"></param>
		public void __getmainargs(ThreadContext ThreadContext)
		{
			var ArgcPtr = ThreadContext.ReadStack(1);
			var ArgvPtr = ThreadContext.ReadStack(2);
			var EnvPtr = ThreadContext.ReadStack(3);
			var DoWildCard = ThreadContext.ReadStack(4);
			var StartInfo = ThreadContext.ReadStack(5);
			ThreadContext.Memory.Write4(ArgcPtr, 0);
			ThreadContext.Memory.Write4(ArgvPtr, 0);
		}

		public enum AppType
		{
			Unknown, Console, Windows
		}

		public void __set_app_type(ThreadContext ThreadContext)
		{
			//Console.WriteLine("__set_app_type: {0}", (AppType)ThreadContext.ReadStack(1));
		}

		public void exit(ThreadContext ThreadContext)
		{
			//throw (new X86HaltException());
			Console.ReadKey();
			Environment.Exit(0);
		}
	}
}
