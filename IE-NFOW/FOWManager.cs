using System.Diagnostics;
using System.Runtime.InteropServices;

namespace IE_NFOW
{
    internal class FOWManager
    {
        public bool IsEnabled { get; private set; }
        public string GameName { get; private set; }

        private readonly byte[] OriginalCode = { 0x48, 0x89, 0x5C, 0x24, 0x10 };
        private readonly byte[] ModifiedCode = { 0xC3, 0x90, 0x90, 0x90, 0x90 };

        public FOWManager(string gameName)
        {
            GameName = gameName;

            var handle = OpenProcess(0x0010 | 0x0020 | 0x0008, false, ProcessId);

            try
            {
                int temp;
                byte[] currentCode = new byte[5];

                ReadProcessMemory(handle, Baldur_CInfinity_RenderFog, currentCode, 5, out temp);

                if (currentCode[0] == OriginalCode[0])
                    IsEnabled = false;
                else
                    IsEnabled = true;
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        private int ProcessId
        {
            get
            {
                var processesData = Process.GetProcessesByName(GameName);

                if (processesData.Length == 0)
                    return 0;

                return processesData[0].Id;
            }
        }

        private IntPtr Baldur_CInfinity_RenderFog
        {
            get
            {
                var processesData = Process.GetProcessesByName(GameName);

                if (processesData.Length == 0)
                    return IntPtr.Zero;

                var process = processesData[0];

                if (process.MainModule != null)
                {
                    var mainModule = process.MainModule.BaseAddress;
                    var funcOffset = 0x2A14E0;

                    return new(mainModule.ToInt64() + funcOffset);
                }

                return IntPtr.Zero;
            }
        }

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hProcess);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, int flNewProtect, out int lpflOldProtect);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesRead);

        public bool Enable()
        {
            var handle = OpenProcess(0x0010 | 0x0020 | 0x0008, false, ProcessId);

            try
            {
                int temp, temp2;

                VirtualProtectEx(handle, Baldur_CInfinity_RenderFog, 5, 0x40, out temp);
                WriteProcessMemory(handle, Baldur_CInfinity_RenderFog, ModifiedCode, 5, out temp2);
                VirtualProtectEx(handle, Baldur_CInfinity_RenderFog, 5, temp, out temp2);
            }
            finally
            {
                CloseHandle(handle);
            }

            return IsEnabled = true;
        }

        public bool Disable()
        {
            var handle = OpenProcess(0x0010 | 0x0020 | 0x0008, false, ProcessId);

            try
            {
                int temp, temp2;

                VirtualProtectEx(handle, Baldur_CInfinity_RenderFog, 5, 0x40, out temp);
                WriteProcessMemory(handle, Baldur_CInfinity_RenderFog, OriginalCode, 5, out temp2);
                VirtualProtectEx(handle, Baldur_CInfinity_RenderFog, 5, temp, out temp2);
            }
            finally
            {
                CloseHandle(handle);
            }

            return IsEnabled = false;
        }

        public bool Switch()
        {
            if (IsEnabled)
                return Disable();

            return Enable();
        }
    }
}
