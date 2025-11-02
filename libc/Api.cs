using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace libc
{
    public class Api
    {
        // ===== libc bindings =====








        // open/close for ioctl
        [DllImport("libc", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern int open(string pathname, int flags);

        [DllImport("libc", SetLastError = true)]
        public static extern int close(int fd);

        // ioctl for BLKGETSIZE64 (from <linux/fs.h>)
        [DllImport("libc", SetLastError = true)]
        public static extern int ioctl(int fd, ulong request, out ulong argp);

        // ===== constants =====
        public const int O_RDONLY = 0x0000;
        public const int O_CLOEXEC = 0x80000; // Linux-specific
        public const ulong BLKGETSIZE64 = 0x80081272; // _IOR(0x12,114,size_t)

        // ===== helpers =====
        public static string PtrToAnsiString(IntPtr p) =>
            p == IntPtr.Zero ? string.Empty : Marshal.PtrToStringAnsi(p);


    }
}
