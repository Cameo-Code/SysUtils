using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static libc.Sys.SysInfo;

namespace diskspace
{
    public class MemCheck
    {
        public sealed class RamStatus
        {
            // Raw values (bytes)
            public ulong TotalBytes { get; init; }
            public ulong FreeBytes { get; init; }        // sysinfo freeram * mem_unit
            public ulong BufferBytes { get; init; }      // bufferram * mem_unit
            public ulong SwapTotalBytes { get; init; }
            public ulong SwapFreeBytes { get; init; }

            // From sysconf (pages * pageSize)
            public ulong PhysTotalBytes_Sysconf { get; init; }
            public ulong PhysAvailBytes_Sysconf { get; init; }

            // Derived helpers
            public ulong UsedBytes => TotalBytes > FreeBytes ? (TotalBytes - FreeBytes) : 0;
            public double UsedRatio => TotalBytes == 0 ? 0 : (double)UsedBytes / TotalBytes;

            // "Available" approximation without /proc/meminfo's MemAvailable
            // (free + buffer). Page cache is not included here (sysinfo lacks it).
            public ulong ApproxAvailableBytes => FreeBytes + BufferBytes;
        }

        public static RamStatus Query()
        {
            // sysinfo
            if (sysinfo(out var si) != 0)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "sysinfo failed");

            // scale by mem_unit (>= Linux 2.4)
            ulong unit = si.mem_unit == 0 ? 1u : si.mem_unit;
            ulong toBytes(ulong v) => v * unit;

            // sysconf
            long pageSize = sysconf((int)SysconfName._SC_PAGESIZE);
            long physPages = sysconf((int)SysconfName._SC_PHYS_PAGES);
            long availPages = sysconf((int)SysconfName._SC_AVPHYS_PAGES);

            if (pageSize <= 0 || physPages <= 0 || availPages < 0)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "sysconf failed");

            checked
            {
                return new RamStatus
                {
                    TotalBytes = toBytes(si.totalram),
                    FreeBytes = toBytes(si.freeram),
                    BufferBytes = toBytes(si.bufferram),
                    SwapTotalBytes = toBytes(si.totalswap),
                    SwapFreeBytes = toBytes(si.freeswap),

                    PhysTotalBytes_Sysconf = (ulong)physPages * (ulong)pageSize,
                    PhysAvailBytes_Sysconf = (ulong)availPages * (ulong)pageSize
                };
            }
        }
    }
}
