using System.Text;
using static libc.Api;
using static libc.MountEnt;
using static libc.Sys.StatVfs;

namespace diskspace
{
    public class Program
    {

        private static IEnumerable<(string dev, string dir, string type, string opts)> EnumerateMounts()
        {
            // Prefer /proc/self/mounts: it reflects the current namespace.
            IntPtr f = setmntent("/proc/self/mounts", "r");
            if (f == IntPtr.Zero)
            {
                // Fallback to /proc/mounts
                f = setmntent("/proc/mounts", "r");
                if (f == IntPtr.Zero)
                    yield break;
            }

            try
            {
                while (true)
                {
                    IntPtr p = getmntent(f);
                    if (p == IntPtr.Zero)
                        break;

                    var e = MarshalMntent(p);
                    string dev = PtrToAnsiString(e.mnt_fsname);
                    string dir = PtrToAnsiString(e.mnt_dir);
                    string type = PtrToAnsiString(e.mnt_type);
                    string opts = PtrToAnsiString(e.mnt_opts);
                    yield return (dev, dir, type, opts);
                }
            }
            finally
            {
                endmntent(f);
            }
        }
        private static bool TryGetFsStat(string path, out statvfs_t s)
        {
            if (statvfs(path, out s) == 0) return true;
            s = default;
            return false;
        }

        private static bool TryGetBlockDeviceSizeBytes(string devPath, out ulong bytes)
        {
            bytes = 0;
            // Only attempt for /dev/* paths
            if (string.IsNullOrEmpty(devPath) || !devPath.StartsWith("/dev/"))
                return false;

            int fd = open(devPath, O_RDONLY | O_CLOEXEC);
            if (fd < 0) return false;

            try
            {
                if (ioctl(fd, BLKGETSIZE64, out bytes) == 0)
                    return true;
                bytes = 0;
                return false;
            }
            finally
            {
                close(fd);
            }
        }

        private static string Human(ulong bytes)
        {
            const double KB = 1024.0, MB = KB * 1024, GB = MB * 1024, TB = GB * 1024;
            double b = bytes;
            if (b >= TB) return $"{b / TB:0.##} TiB";
            if (b >= GB) return $"{b / GB:0.##} GiB";
            if (b >= MB) return $"{b / MB:0.##} MiB";
            if (b >= KB) return $"{b / KB:0.##} KiB";
            return $"{bytes} B";
        }
        private static string Trunc(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= max) return s ?? "";
            if (max <= 1) return s.Substring(0, max);
            return s.Substring(0, max - 1) + "…";
        }

        static void MainDisk(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Mounts (from /proc/self/mounts):");
            Console.WriteLine(new string('-', 120));
            Console.WriteLine("{0,-28}  {1,-32}  {2,-8}  {3,-12}  {4,-12}  {5,-12}",
                "Device", "Mountpoint", "Type", "FS Total", "FS Avail", "Dev Size");

            foreach (var (dev, dir, type, _opts) in EnumerateMounts())
            {
                // Query filesystem sizes via statvfs (for the mountpoint)
                ulong fsTotal = 0, fsAvail = 0;
                if (TryGetFsStat(dir, out var st))
                {
                    ulong fr = st.f_frsize != 0 ? st.f_frsize : st.f_bsize;
                    fsTotal = st.f_blocks * fr;
                    fsAvail = st.f_bavail * fr;
                }

                // If it's a block device (/dev/...), also try to get raw device size via ioctl
                ulong devBytes = 0;
                TryGetBlockDeviceSizeBytes(dev, out devBytes);

                Console.WriteLine("{0,-28}  {1,-32}  {2,-8}  {3,12}  {4,12}  {5,12}",
                    Trunc(dev, 28),
                    Trunc(dir, 32),
                    Trunc(type, 8),
                    fsTotal == 0 ? "-" : Human(fsTotal),
                    fsAvail == 0 ? "-" : Human(fsAvail),
                    devBytes == 0 ? "-" : Human(devBytes));
            }
        }

        static void MainRam(string[] args)
        {
            // This program must run on Linux (uses libc). On Windows it will fail to DllImport("libc").
            var s = MemCheck.Query();

            Console.WriteLine("== RAM via sysinfo(2) ==");
            Console.WriteLine($"Total:       {Human(s.TotalBytes)}");
            Console.WriteLine($"Free:        {Human(s.FreeBytes)}");
            Console.WriteLine($"Buffers:     {Human(s.BufferBytes)}");
            Console.WriteLine($"ApproxAvail: {Human(s.ApproxAvailableBytes)}");
            Console.WriteLine($"Used:        {Human(s.UsedBytes)}  ({s.UsedRatio:P1})");

            Console.WriteLine();
            Console.WriteLine("== Swap via sysinfo(2) ==");
            Console.WriteLine($"Swap Total:  {Human(s.SwapTotalBytes)}");
            Console.WriteLine($"Swap Free:   {Human(s.SwapFreeBytes)}");

            Console.WriteLine();
            Console.WriteLine("== RAM via sysconf(3) ==");
            Console.WriteLine($"Phys Total:  {Human(s.PhysTotalBytes_Sysconf)}");
            Console.WriteLine($"Phys Avail:  {Human(s.PhysAvailBytes_Sysconf)}");
        }

        static void Main(string[] args)
        {
            MainRam(args);
        }
    }
}
