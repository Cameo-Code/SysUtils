using System;
using System.Threading.Tasks;

namespace PackTasks
{
    public class PackTaskResult
    {
        public bool isSuccess;
        public string error;
        public string resultFileName;
    }

    public class PackDirParams
    {
        public string srcDir;
        public string dstFileName;
        public bool autoFileExt;
    }

    public interface IPackTasks
    { 
        // packs the entire content of a specified directory
        // THE DIRECTORY name itself should not be part of the file
        Task<PackTaskResult> PackDir(PackDirParams prm);
    }

    public static class PackHelpers
    {
        public static Task<PackTaskResult> PackDir(this IPackTasks task, string srcDir, string dstFileName)
        {
            PackDirParams inp = new PackDirParams
            {
                srcDir = srcDir,
                dstFileName = dstFileName,
                autoFileExt = true
            };
            return task.PackDir(inp);
        }

    }
}
