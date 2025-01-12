using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace PackTasks
{
    public class PackExternal : IPackTasks
    {
        public string ExeName;

        // should be:
        //   a %src% %dst%
        // where %src% would be replaced with the directory
        // and %dst% with the destination file. 
        // Quotes should be part of the template
        public string ArgsPattern;
        public string resultExt;

        public int[] SuccessExitCodes;

        // Timeout is infinite
        public int TimeoutMs = -1; // 60 * 1000 * 1000; //// one hour is the time out

        public Task<PackTaskResult> PackDir(PackDirParams prm)
        {
            PackTaskResult res = new PackTaskResult();
            string dir = prm.srcDir;
            dir = PathUtils.RemoveTrailSlash(dir);
            string fn = prm.dstFileName;
            if (prm.autoFileExt)
                fn = Path.ChangeExtension(fn, resultExt);

            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = false;
            info.FileName = ExeName;

            string args = ArgsPattern;
            args = args.Replace("%src%", prm.srcDir);
            args = args.Replace("%dst%", fn);
            info.Arguments = args;

            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.RedirectStandardError = false;
            info.RedirectStandardOutput = false;

            // Console.Write($">>> {info.FileName}");
            // Console.Write(" ");
            // Console.WriteLine(string.Join(" ", info.Arguments));
            Process p = new Process();
            p.StartInfo = info;
            if (!p.Start())
            {
                res.isSuccess = false;
                res.resultFileName = $"failed to start the external packer: {ExeName}";
                return Task.FromResult(res);
            }

            var timeout = TimeoutMs;

            return Task.Run(() =>
            {
                res.isSuccess = p.WaitForExit(timeout);
                //Console.WriteLine($"error code: {p.ExitCode}");
                if (!res.isSuccess)
                {
                    res.error = "packing action timed out"; 
                    return res;
                }
                if ((SuccessExitCodes != null)&&(SuccessExitCodes.Length > 0))
                {
                    res.isSuccess = false;
                    foreach(var ec in SuccessExitCodes)
                    {
                        if (p.ExitCode == ec)
                        {
                            res.isSuccess = true;
                            break;
                        }
                    }
                }

                res.resultFileName = fn;
                return res;
            });
        }

        public static PackExternal ZipBy7Zip (string exeName = "7z")
        {
            PackExternal ext = new PackExternal();
            ext.ExeName = exeName;
            ext.ArgsPattern = $" a -r \"%dst%\" \"%src%{Path.DirectorySeparatorChar}*\"";
            ext.resultExt = ".zip";
            ext.SuccessExitCodes = new[] { 0 };
            return ext;
        }
    }
}
