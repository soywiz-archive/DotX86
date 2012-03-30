using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using DotX86.Core.Cpu;
using DotX86.Hle;

namespace DotX86.Loader
{
	public class Win32PeLoader
	{
		[StructLayout(LayoutKind.Sequential), Serializable]
		public struct IMAGE_DOS_HEADER
		{
			public ushort Magic;

			// One page is 512 bytes, one paragraph is 16 bytes
			public ushort UsedBytesInTheLastPage;
			public ushort FileSizeInPages;
			public ushort NumberOfRelocationItems;
			public ushort HeaderSizeInParagraphs;
			public ushort MinimumExtraParagraphs;
			public ushort MaximumExtraParagraphs;
			public ushort InitialRelativeSS;
			public ushort InitialSP;
			public ushort Checksum;
			public ushort InitialIP;
			public ushort InitialRelativeCS;
			public ushort AddressOfRelocationTable;
			public ushort OverlayNumber;

			[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
			public ushort[] Reserved;
			public ushort OEMid;
			public ushort OEMinfo;

			[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 10)]
			public ushort[] Reserved2;
			public uint     AddressOfNewExeHeader;
		}

		public enum MACHINE : ushort
		{
			IMAGE_FILE_MACHINE_UNKNOWN = 0,
			IMAGE_FILE_MACHINE_I386 = 0x014c,
			IMAGE_FILE_MACHINE_AMD64 = 0x8664,
			IMAGE_FILE_MACHINE_ARM = 0x01c0,

			IMAGE_FILE_MACHINE_R3000 = 0x0162, // Rarely used
			IMAGE_FILE_MACHINE_R4000 = 0x0166,
			IMAGE_FILE_MACHINE_R10000 = 0x0168,
			IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x0169,
			IMAGE_FILE_MACHINE_ALPHA = 0x0184,
			IMAGE_FILE_MACHINE_SH3 = 0x01a2,
			IMAGE_FILE_MACHINE_SH3DSP = 0x01a3,
			IMAGE_FILE_MACHINE_SH3E = 0x01a4,
			IMAGE_FILE_MACHINE_SH4 = 0x01a6,
			IMAGE_FILE_MACHINE_SH5 = 0x01a8,
			IMAGE_FILE_MACHINE_THUMB = 0x01c2,
			IMAGE_FILE_MACHINE_AM33 = 0x01d3,
			IMAGE_FILE_MACHINE_POWERPC = 0x01F0,
			IMAGE_FILE_MACHINE_POWERPCFP = 0x01f1,
			IMAGE_FILE_MACHINE_IA64 = 0x0200,
			IMAGE_FILE_MACHINE_MIPS16 = 0x0266,
			IMAGE_FILE_MACHINE_ALPHA64 = 0x0284,
			IMAGE_FILE_MACHINE_MIPSFPU = 0x0366,
			IMAGE_FILE_MACHINE_MIPSFPU16 = 0x0466,
			IMAGE_FILE_MACHINE_TRICORE = 0x0520,
			IMAGE_FILE_MACHINE_CEF = 0x0CEF,
			IMAGE_FILE_MACHINE_EBC = 0x0EBC,
			IMAGE_FILE_MACHINE_M32R = 0x9041,
			IMAGE_FILE_MACHINE_CEE = 0xC0EE
		}

		public enum SUBSYSTEM : ushort
		{
		}

		public enum DLLCHARACTERISTICS : ushort
		{
		}

		public struct IMAGE_OPTIONAL_HEADER32 {
			// Standard fields.
			public ushort    Magic;
			public byte    MajorLinkerVersion;
			public byte    MinorLinkerVersion;
			public uint   SizeOfCode;
			public uint   SizeOfInitializedData;
			public uint   SizeOfUninitializedData;
			public uint   AddressOfEntryPoint;
			public uint   BaseOfCode;
			public uint   BaseOfData;
			// NT additional fields.
			public uint   ImageBase;
			public uint   SectionAlignment;
			public uint   FileAlignment;
			public ushort    MajorOperatingSystemVersion;
			public ushort    MinorOperatingSystemVersion;
			public ushort    MajorImageVersion;
			public ushort    MinorImageVersion;
			public ushort    MajorSubsystemVersion;
			public ushort    MinorSubsystemVersion;
			public uint   Win32VersionValue;
			public uint   SizeOfImage;
			public uint   SizeOfHeaders;
			public uint   CheckSum;
			public SUBSYSTEM Subsystem;
			public DLLCHARACTERISTICS DllCharacteristics;
			public int   SizeOfStackReserve;
			public uint   SizeOfStackCommit;
			public uint   SizeOfHeapReserve;
			public uint   SizeOfHeapCommit;
			public uint   LoaderFlags;
			public int   NumberOfRvaAndSizes;
			//IMAGE_DATA_DIRECTORIES DataDirectory;
		}

		public enum CHARACTERISTICS : ushort
		{
		}

		public struct time_t
		{
			public uint Value;
		}

		public struct IMAGE_FILE_HEADER {
			public MACHINE  Machine;
			public ushort NumberOfSections;
			public time_t   TimeDateStamp;
			public uint    PointerToSymbolTable;
			public uint NumberOfSymbols;
			public ushort     SizeOfOptionalHeader;
			public CHARACTERISTICS Characteristics;
		}

		public struct IMAGE_NT_HEADERS {
			public ushort PESignature;
			public IMAGE_FILE_HEADER FileHeader;
			public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
		}

		public struct DATA_DIR
		{
			public uint VirtualAddress;
			public uint Size;
		}

		public enum SECTION_FLAGS : uint
		{
		}

		public struct IMAGE_SECTION_HEADER
		{
			[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 8)]
			public string    Name;
			public uint   VirtualSize;
			public uint   VirtualAddress;
			public uint   SizeOfRawData;
			public uint   PointerToRawData;
			public uint   NonUsedPointerToRelocations;
			public uint   NonUsedPointerToLinenumbers;
			public ushort    NonUsedNumberOfRelocations;
			public ushort    NonUsedNumberOfLinenumbers;
			public SECTION_FLAGS Characteristics;

			public override string ToString()
			{
				return String.Format("IMAGE_SECTION_HEADER('{0}')", Name);
			}
		}

		public struct IMPORT_DIRECTORY_TABLE
		{
			public uint ImportLookupTableRVA;
			public uint TimeDateStamp;
			public uint ForwarderChain;
			public uint NameRVA;
			public uint ImportAddressTableRVA;
		}

		DATA_DIR Export;
		DATA_DIR Import;
		DATA_DIR Resource;
		DATA_DIR Exception;
		DATA_DIR Security;
		DATA_DIR BaseRelocationTable;
		DATA_DIR DebugDirectory;
		DATA_DIR CopyrightOrArchitectureSpecificData;
		DATA_DIR GlobalPtr;
		DATA_DIR TLSDirectory;
		DATA_DIR LoadConfigurationDirectory;
		DATA_DIR BoundImportDirectory;
		DATA_DIR ImportAddressTable;
		DATA_DIR DelayLoadImportDescriptors;
		DATA_DIR COMRuntimedescriptor;
		DATA_DIR Reserved;

		public void Load(string FileName, ThreadContext ThreadContext)
		{
			Load(new MemoryStream(File.ReadAllBytes(FileName)), ThreadContext);
		}

		public void Load(Stream Stream, ThreadContext ThreadContext)
		{
			var Memory = ThreadContext.CpuContext.Memory;

			var DosHeader = Stream.ReadStruct<IMAGE_DOS_HEADER>();

			Stream.Position = DosHeader.AddressOfNewExeHeader;

			var NtHeader = Stream.ReadStruct<IMAGE_NT_HEADERS>();

			int len = NtHeader.OptionalHeader.NumberOfRvaAndSizes;

			if (len >= 1) Export = Stream.ReadStruct<DATA_DIR>();
			if (len >= 2) Import = Stream.ReadStruct<DATA_DIR>();
			if (len >= 3) Resource = Stream.ReadStruct<DATA_DIR>();
			if (len >= 4) Exception = Stream.ReadStruct<DATA_DIR>();
			if (len >= 5) Security = Stream.ReadStruct<DATA_DIR>();
			if (len >= 6) BaseRelocationTable = Stream.ReadStruct<DATA_DIR>();
			if (len >= 7) DebugDirectory = Stream.ReadStruct<DATA_DIR>();
			if (len >= 8) CopyrightOrArchitectureSpecificData = Stream.ReadStruct<DATA_DIR>();
			if (len >= 9) GlobalPtr = Stream.ReadStruct<DATA_DIR>();
			if (len >= 10) TLSDirectory = Stream.ReadStruct<DATA_DIR>();
			if (len >= 11) LoadConfigurationDirectory = Stream.ReadStruct<DATA_DIR>();
			if (len >= 12) BoundImportDirectory = Stream.ReadStruct<DATA_DIR>();
			if (len >= 13) ImportAddressTable = Stream.ReadStruct<DATA_DIR>();
			if (len >= 14) DelayLoadImportDescriptors = Stream.ReadStruct<DATA_DIR>();
			if (len >= 15) COMRuntimedescriptor = Stream.ReadStruct<DATA_DIR>();
			if (len >= 16) Reserved = Stream.ReadStruct<DATA_DIR>();

			var Sections = new List<IMAGE_SECTION_HEADER>();

			for (int n = 0; n < NtHeader.FileHeader.NumberOfSections; n++)
			{
				Sections.Add(Stream.ReadStruct<IMAGE_SECTION_HEADER>());
			}

			var ImageBase = NtHeader.OptionalHeader.ImageBase;

			foreach (var Section in Sections)
			{
				Stream.Position = Section.PointerToRawData;
				var Data = new byte[Section.VirtualSize];
				Stream.Read(Data, 0, Data.Length);
				Memory.Write(ImageBase + Section.VirtualAddress, Data);
			}

			ThreadContext.PC = ImageBase + NtHeader.OptionalHeader.AddressOfEntryPoint;
			ThreadContext.ESP = (uint)(Memory.AllocStack(NtHeader.OptionalHeader.SizeOfStackReserve) + NtHeader.OptionalHeader.SizeOfStackReserve);

			var VirtualStream = Memory.GetStream().SliceWithLength(ImageBase);

			VirtualStream.Position = Import.VirtualAddress;
			var ImportDirectoryCount = Import.Size / Marshal.SizeOf(typeof(IMPORT_DIRECTORY_TABLE));
			for (int n = 0; n < ImportDirectoryCount; n++)
			{
				var ImportDirectory = VirtualStream.ReadStruct<IMPORT_DIRECTORY_TABLE>();
				if (ImportDirectory.NameRVA != 0)
				{
					var DllName = VirtualStream.SliceWithLength(ImportDirectory.NameRVA).ReadStringz();
					var Imports = VirtualStream.SliceWithLength(ImportDirectory.ImportLookupTableRVA);
					var ImportsReader = new BinaryReader(Imports);
					uint POS = ImportDirectory.ImportAddressTableRVA;

					uint JumpAddress = 0x100;

					while (true)
					{
						var ImportLookupAddress = ImportsReader.ReadUInt32();
						if (ImportLookupAddress == 0) break;
						var ImportLookupStream = VirtualStream.SliceWithLength(ImportLookupAddress);
						ImportLookupStream.ReadByte();
						ImportLookupStream.ReadByte();
						var Name = ImportLookupStream.ReadStringz();
						
						//Console.WriteLine("{0} : 0x{1:X} : {2} <-- 0x{3:X}", DllName, POS, Name, JumpAddress);
						new BinaryWriter(VirtualStream.SliceWithLength(POS)).Write((uint)JumpAddress);
						var JumpStream = new BinaryWriter(Memory.GetStream().SliceWithLength(JumpAddress));
						JumpStream.Write(new byte[] { 0xCD, 0x01 });
						JumpStream.Write((uint)JumpAddress);

						ThreadContext.CpuContext.NativeMethodInfoList[JumpAddress] = CreateNativeMethodInfo(DllName, Name);
						POS += 4;
						JumpAddress += 6;
					}
				}
			}

			//Console.WriteLine(DosHeader.Magic);
			//BinaryFormatter BinaryFormatter = new BinaryFormatter();
			//var Header = (IMAGE_DOS_HEADER)BinaryFormatter.Deserialize(Stream);
			//Marshal.StructureToPtr
		}

		static Dictionary<string, Type> HleModuleTypes = new Dictionary<string, Type>();
		static Dictionary<string, module> HleModules = new Dictionary<string, module>();

		static Win32PeLoader()
		{
			foreach (var Type in Assembly.GetCallingAssembly().GetExportedTypes())
			{
				if (Type.BaseType == typeof(module))
				{
					HleModuleTypes.Add(Type.Name + ".dll", Type);
					HleModules.Add(Type.Name + ".dll", (module)Activator.CreateInstance(Type));
				}
			}
		}

		public NativeMethodInfo CreateNativeMethodInfo(string DllName, string Name)
		{
			var Module = HleModules[DllName];
			var MethodInfo = Module.GetType().GetMethod(Name);
			//var Method = MethodInfo

			return new NativeMethodInfo()
			{
				DllName = DllName,
				Name = Name,
				Method = (NativeMethodInfo, LocalThreadContext) =>
				{
					MethodInfo.Invoke(Module, new object[] { LocalThreadContext });
					/*
					Console.WriteLine("STACK0: {0:X}", LocalThreadContext.ReadStack(0));
					Console.WriteLine("STACK4: {0:X}", LocalThreadContext.ReadStack(1));
					Console.WriteLine("STACK8: {0:X}", LocalThreadContext.ReadStack(2));
					try
					{
						Console.WriteLine("VALUE: '{0}'", Memory.GetStream().SliceWithLength(LocalThreadContext.ReadStack(1)).ReadStringz());
					}
					catch
					{
					}
					Console.WriteLine("Method called! : {0} : {1}", NativeMethodInfo.DllName, NativeMethodInfo.Name);
					*/
				}
			};
		}
	}
}
