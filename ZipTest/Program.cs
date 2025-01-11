using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PackTasks;

namespace ZipTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("please specify:");
                Console.WriteLine("  directory to pack");
                Console.WriteLine("  destination file");
                return;
            }
            string dir = args[0];
            string fn = args[1];
            IPackTasks pack = PackExternal.ZipBy7Zip();
            var res = await pack.PackDir(dir, fn);
            if (res.isSuccess)
                Console.WriteLine("success!");
            else
                Console.WriteLine($"error: {res.error}");
        }
    }
}
