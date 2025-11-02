using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace libc.Sys
{
    public static class SysInfo
    {
        // WARNING this is for linux-64 bit only!
        // (C# doesn't support linux-32 bit anyway)


        // https://linux.die.net/man/2/sysinfo
        // The structure must be 64-bytes in total
        [StructLayout(LayoutKind.Sequential)]
        public struct sysinfo_native
        {
            public long uptime;                // seconds
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public ulong[] loads;              // 1,5,15 min load averages (fixed-point)
            public ulong totalram;             // total usable main memory size
            public ulong freeram;              // available memory size
            public ulong sharedram;            // amount of shared memory
            public ulong bufferram;            // memory used by buffers
            public ulong totalswap;            // total swap space size
            public ulong freeswap;             // swap space still available
            public ushort procs;               // number of current processes
            public ulong totalhigh;            // total high memory size
            public ulong freehigh;             // available high memory size
            public uint mem_unit;              // memory unit size in bytes
            // Padding to 64 bytes boundary in glibc; ignore
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8 /*SizeConst = (20 - 2 * sizeof(long) - sizeof(int))*/ )]
            public byte[] _f;
        }

        [DllImport("libc", SetLastError = true)]
        public static extern int sysinfo(out sysinfo_native info);

        // Todo: those are Linux Kernel defines. For other OS-es these might be different!
        public enum SysconfName : int
        {
            _SC_PAGESIZE = 30,   // or _SC_PAGE_SIZE
            _SC_PHYS_PAGES = 85,
            _SC_AVPHYS_PAGES = 86
        }

        [DllImport("libc", SetLastError = true)]
        public static extern long sysconf(int name);
    }
}
