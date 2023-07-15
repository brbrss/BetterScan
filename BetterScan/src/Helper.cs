using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BetterScan
{
    public class Helper
    {
        private static String WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
        private static bool IsMatch(string s, IEnumerable<Regex> plist)
        {
            foreach (var p in plist)
            {
                if (p.IsMatch(s))
                {
                    return true;
                }
            }
            return false;
        }
        public static List<FileInfo> Search(string path, IEnumerable<string> pattern)
        {
            List<Regex> patternList = new List<Regex>();
            foreach (string s in pattern)
            {
                string ss = WildCardToRegular(s);
                Regex p = new Regex(ss);
                patternList.Add(p);
            }

            return Search(path, patternList);
        }
        public static List<FileInfo> Search(string path, IEnumerable<Regex> patternList)
        {
            List<FileInfo> res = new List<FileInfo>();
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo fi in di.EnumerateFiles())
            {
                if (IsMatch(fi.FullName, patternList))
                {
                    res.Add(fi);
                }
            }
            foreach (var fi in di.EnumerateDirectories())
            {
                var subRes = Search(fi.FullName, patternList);
                res.AddRange(subRes);
            }
            return res;
        }

    }
}
