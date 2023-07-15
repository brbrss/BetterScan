﻿using System;
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
        private static List<FileInfo> Search(string path, IEnumerable<Regex> patternList)
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

        public static string ToRelPath(string s1, string root, string varname)
        {
            char[] sep = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            string[] sp1 = s1.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] sp2 = root.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            int j = -1;
            for (int i = 0; i < sp1.Length && i < sp2.Length; i++)
            {
                if (sp1[i] != sp2[i])
                {
                    break;
                }
                j = i + 1;
            }
            if (j == -1)
            {
                return s1;
            }
            string res = varname;
            int nshort = sp2.Length - j;
            for (; nshort > 0; nshort--)
            {
                res += Path.DirectorySeparatorChar + "..";
            }
            for (; j < sp1.Length; j++)
            {
                res += Path.DirectorySeparatorChar + sp1[j];
            }
            return res;
        }
    }
}
