using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace BetterScan
{
    public class Helper
    {
        public static String WildCardToRegular(String value)
        {
            value = value.ToLower();
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
        public static bool IsMatch(string s, IEnumerable<Regex> plist)
        {
            s = s.ToLower();
            foreach (var p in plist)
            {
                if (p.IsMatch(s))
                {
                    return true;
                }
            }
            return false;
        }
        public static List<FileInfo> Search(string path,
            IEnumerable<string> excludedFolderList,
            IEnumerable<string> pattern,
            IEnumerable<string> skipPattern,
            bool earlyStop
            )
        {
            List<Regex> patternList = new List<Regex>();
            foreach (string s in pattern)
            {
                string ss = WildCardToRegular(s);
                Regex p = new Regex(ss);
                patternList.Add(p);
            }
            List<Regex> skipList = new List<Regex>();
            foreach (string s in skipPattern)
            {
                string ss = WildCardToRegular(s);
                Regex p = new Regex(ss);
                skipList.Add(p);
            }
            return Search(path, excludedFolderList, patternList, skipList, earlyStop);
        }
        private static List<FileInfo> Search(
            string path,
            IEnumerable<string> excludedFolderList,
            IEnumerable<Regex> patternList,
            IEnumerable<Regex> skipList,
            bool earlyStop
            )
        {
            List<FileInfo> res = new List<FileInfo>();
            DirectoryInfo di = new DirectoryInfo(path);

            if (excludedFolderList.Contains(di.FullName))
            {
                return new List<FileInfo> { };
            }
            foreach (FileInfo fi in di.EnumerateFiles())
            {
                try
                {
                    if (IsMatch(fi.FullName, skipList))
                    {
                    }
                    else if (IsMatch(fi.FullName, patternList))
                    {
                        res.Add(fi);
                    }
                }
                catch (Exception)
                {
                }
            }
            if (res.Count() > 0 && earlyStop)
            {
                return res;
            }
            foreach (var fi in di.EnumerateDirectories())
            {
                try
                {
                    if (!excludedFolderList.Contains(fi.FullName))
                    {
                        var subRes = Search(fi.FullName,
                            excludedFolderList, patternList, skipList, earlyStop);
                        res.AddRange(subRes);
                    }
                }
                catch (Exception)
                {
                }
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

        ///<summary>
        /// fpname is file component of file path
        ///  e.g. if fp is "C:\mythings\meow.png" then fpname is "meow.png"
        ///</summary>
        public static string ConcatPath(string root, string fpname)
        {
            return root + Path.DirectorySeparatorChar + fpname;
        }

        public static string ResolveRelPath(string basePath, string relPath)
        {
            string fp = ConcatPath(basePath, relPath);
            return Path.GetFullPath(fp);
        }
        public static string ResolveRelPath(string rawPath)
        {
            return Path.GetFullPath(rawPath);
        }

        public static string SearchIconFile(string fp)
        {
            string folderName = Path.GetDirectoryName(fp);
            DirectoryInfo di = new DirectoryInfo(folderName);
            foreach (FileInfo fi in di.EnumerateFiles())
            {
                if (fi.Extension.ToLower() == ".ico")
                {
                    return fi.FullName;
                }
            }
            return null;
        }

        public static System.Windows.Media.ImageSource GetIconData(string fp)
        {
            try
            {
                TsudaKageyu.IconExtractor iconex = new TsudaKageyu.IconExtractor(fp);
                System.Drawing.Icon icon = iconex.Count > 0 ? iconex.GetIcon(0) : null;
                if (icon is null)
                {
                    return null;
                }
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                 icon.Handle,
                 new Int32Rect(0, 0, icon.Width, icon.Height),
                 BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string CreateIconTemp(string fp, BetterScanSettings settings)
        {
            string res = null;
            try
            {
                var iconex = new TsudaKageyu.IconExtractor(fp);
                if (iconex.Count > 0)
                {
                    string tempfp = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
                    var f = new FileStream(tempfp, FileMode.CreateNew);
                    iconex.Save(0, f);
                    f.Close();
                    res = tempfp;
                }

            }
            catch (Exception)
            {

            }
            if (res == null && settings.OptionSearchAdjacentIco)
            {
                string iconPath = Helper.SearchIconFile(fp);
                if (iconPath == null)
                {
                    return res;
                }
                string tempfp = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
                File.Copy(iconPath, tempfp);
                res = tempfp;
            }
            return res;
        }

        static public string GetFolderPath(string fp)
        {
            return new FileInfo(fp).Directory.ToString();
        }
        static public string GetFolderName(string fp)
        {
            return new FileInfo(fp).Directory.Name;
        }
    }
}
