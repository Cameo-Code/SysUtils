using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace libc
{
    // mntent.h
    public static class MountEnt 
    {
        // getmntent family (from <mntent.h>)
        // https://linux.die.net/man/3/setmntent
        // getmntent, setmntent, addmntent, endmntent, hasmntopt, getmntent_r
        //   - get file system descriptor file entry
        //
        [DllImport("libc", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr setmntent(string filename, string type); // returns FILE*

        [DllImport("libc", SetLastError = true)]
        public static extern int endmntent(IntPtr stream); // FILE* -> int

        [DllImport("libc", SetLastError = true)]
        public static extern IntPtr getmntent(IntPtr stream); // FILE* -> struct mntent*


        // struct mntent (we'll map char* fields as IntPtr)
        // See: man getmntent
        [StructLayout(LayoutKind.Sequential)]
        public struct mntent
        {
            public IntPtr mnt_fsname; // char* name of mounted file system
            public IntPtr mnt_dir;    // char* file system path prefix
            public IntPtr mnt_type;   // char* mount type (see mntent.h)
            public IntPtr mnt_opts;   // char* mount options (see mntent.h)
            public int mnt_freq;      // dump frequency in days
            public int mnt_passno;    // pass number on parallel fsck
        }

        public static mntent MarshalMntent(IntPtr p)
        {
            // getmntent returns pointer to static struct; copy it immediately
            return Marshal.PtrToStructure<mntent>(p);
        }
    }
}
