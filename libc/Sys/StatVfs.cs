using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace libc.Sys
{
    public class StatVfs
    {
        // statvfs (from <sys/statvfs.h>)
        // https://man7.org/linux/man-pages/man3/statvfs.3.html
        // The function statvfs() returns information about a mounted
        // filesystem.path is the pathname of any file within the mounted
        // filesystem.buf is a pointer to a statvfs structure defined
        // approximately as follows: statvfs 
        //
        // RETURN VALUE
        //   On success, zero is returned.  On error, -1 is returned, and errno
        //   is set to indicate the error.

        [DllImport("libc", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern int statvfs(string path, out statvfs_t buf);

        // Linux/glibc statvfs layout (64-bit friendly)
        [StructLayout(LayoutKind.Sequential)]
        public struct statvfs_t
        {
            public ulong f_bsize;    // Filesystem block size
            public ulong f_frsize;   // Fragment size
            public ulong f_blocks;   // Size of fs in f_frsize units
            public ulong f_bfree;    // # free blocks
            public ulong f_bavail;   // # free blocks for unprivileged users
            public ulong f_files;    // # inodes
            public ulong f_ffree;    // # free inodes
            public ulong f_favail;   // # free inodes for unprivileged users
            public ulong f_fsid;     // Filesystem ID
            public ulong f_flag;     // Mount flags
            public ulong f_namemax;  // Maximum filename length
        }
    }
}
