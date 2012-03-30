using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotX86.Core.Cpu.Tables
{
	public enum Register
	{
		AX = 0,
		CX = 1,
		DX = 2,
		BX = 3,
		SP = 4,
		BP = 5,
		SI = 6,
		DI = 7,
	}

	public enum ERegister
	{
		EAX = 0,
		ECX = 1,
		EDX = 2,
		EBX = 3,
		ESP = 4,
		EBP = 5,
		ESI = 6,
		EDI = 7,
	}
}
