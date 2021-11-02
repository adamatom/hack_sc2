using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace PAL9002
{
    /// <summary>
    /// ProcessMemoryReader is a class that enables direct reading a process memory
    /// </summary>
    class ProcessMemoryReaderApi
    {
        // constants information can be found in <winnt.h>
        public const uint PROCESS_VM_READ = (0x0010);

        // function declarations are found in the MSDN and in <winbase.h> 

        //		HANDLE OpenProcess(
        //			DWORD dwDesiredAccess,  // access flag
        //			BOOL bInheritHandle,    // handle inheritance option
        //			DWORD dwProcessId       // process identifier
        //			);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

        //		BOOL CloseHandle(
        //			HANDLE hObject   // handle to object
        //			);
        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hObject);

        //		BOOL ReadProcessMemory(
        //			HANDLE hProcess,              // handle to the process
        //			LPCVOID lpBaseAddress,        // base of memory area
        //			LPVOID lpBuffer,              // data buffer
        //			SIZE_T nSize,                 // number of bytes to read
        //			SIZE_T * lpNumberOfBytesRead  // number of bytes read
        //			);
        [DllImport("kernel32.dll")]
        public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool IsBadReadPtr(IntPtr lp, uint ucb);

        //[DllImport("kernel32.dll")]
        //public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr buffer, int size, ref IntPtr lpNumberOfBytesRead);

    }

    public class ProcessMemoryReader
    {

        public ProcessMemoryReader()
        {
        }

        /// <summary>	
        /// Process from which to read		
        /// </summary>
        public Process ReadProcess
        {
            get
            {
                return m_ReadProcess;
            }
            set
            {
                m_ReadProcess = value;
            }
        }

        private Process m_ReadProcess = null;

        private IntPtr hProcess = IntPtr.Zero;
        private IntPtr eightBytes = IntPtr.Zero;
        public void OpenProcess()
        {
            hProcess = ProcessMemoryReaderApi.OpenProcess(ProcessMemoryReaderApi.PROCESS_VM_READ, 1, (uint)m_ReadProcess.Id);
        }

        public void CloseHandle()
        {
            int iRetValue;
            iRetValue = ProcessMemoryReaderApi.CloseHandle(hProcess);
            if (iRetValue == 0)
                throw new Exception("CloseHandle failed");
        }
        public bool IsBadReadPtr(IntPtr lp, UInt32 u)
        {
            return ProcessMemoryReaderApi.IsBadReadPtr(lp, u);
        }

        public byte[] ReadProcessMemory(IntPtr MemoryAddress, uint bytesToRead, out int bytesReaded)
        {
            byte[] buffer = new byte[bytesToRead];

            IntPtr ptrBytesReaded;
            ProcessMemoryReaderApi.ReadProcessMemory(hProcess, MemoryAddress, buffer, bytesToRead, out ptrBytesReaded);

            bytesReaded = ptrBytesReaded.ToInt32();

            return buffer;
        }


        /// <summary>
        /// Read an integer from the currently opened process
        /// </summary>
        /// <param name="Address">Address to read from</param>
        /// <returns>Integer read</returns>
        public Int32 ReadInteger(UInt32 Address)
        {
            byte[] buffer = new byte[4];

            IntPtr ptrBytesReaded;
            ProcessMemoryReaderApi.ReadProcessMemory(hProcess, (IntPtr)Address, buffer, 4, out ptrBytesReaded);

            return BitConverter.ToInt32(buffer, 0);
        }

        public byte ReadByte(UInt32 Address)
        {
            byte[] buffer = new byte[1];

            IntPtr ptrBytesReaded;
            ProcessMemoryReaderApi.ReadProcessMemory(hProcess, (IntPtr)Address, buffer, 1, out ptrBytesReaded);

            return buffer[0];
        }

        public char ReadChar(UInt32 Address)
        {
            byte[] buffer = new byte[2];

            IntPtr ptrBytesReaded;
            ProcessMemoryReaderApi.ReadProcessMemory(hProcess, (IntPtr)Address, buffer, 2, out ptrBytesReaded);

            return BitConverter.ToChar(buffer, 0);
        }

        /// <summary>
        /// Read a long from the currently opened process
        /// </summary>
        /// <param name="Address">Address to read from</param>
        /// <returns>Long read</returns>
        public long ReadLong(UInt32 Address)
        {
            byte[] buffer = new byte[8];

            IntPtr ptrBytesReaded;
            ProcessMemoryReaderApi.ReadProcessMemory(hProcess, (IntPtr)Address, buffer, 8, out ptrBytesReaded);

            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// Read a float from the currently opened process
        /// </summary>
        /// <param name="Address">Address to read from</param>
        /// <returns>Float read</returns>
        public float ReadFloat(UInt32 Address)
        {

            byte[] buffer = new byte[4];

            IntPtr ptrBytesReaded;
            ProcessMemoryReaderApi.ReadProcessMemory(hProcess, (IntPtr)Address, buffer, 4, out ptrBytesReaded);

            return BitConverter.ToSingle(buffer, 0);
        }

        /// <summary>
        /// Read a [null-terminated] string from the currently opened process
        /// </summary>
        /// <param name="Address">Address to read from</param>
        /// <param name="bytes">Maximum size</param>
        /// <returns>String read</returns>
        public string ReadString(UInt32 Address, UInt32 bytes)
        {
            byte[] buffer = new byte[bytes];

            IntPtr ptrBytesReaded;
            ProcessMemoryReaderApi.ReadProcessMemory(hProcess, (IntPtr)Address, buffer, bytes, out ptrBytesReaded);


            UTF8Encoding utf8 = new UTF8Encoding();
            string result = utf8.GetString(buffer);
            int nullpos = result.IndexOf("\0");
            if (nullpos != -1)
                result = result.Remove(nullpos, result.Length - nullpos);
            return result;
        }

    }
}
