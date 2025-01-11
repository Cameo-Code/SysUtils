using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PackTasks
{
    public static class PathUtils
    {
        public static string RemoveTrailSlash(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;
            int i = s.Length - 1;
            while ((i >= 0) && (s[i] == Path.DirectorySeparatorChar))
                i--;
            return s.Substring(0, i);
        }
    }
}
