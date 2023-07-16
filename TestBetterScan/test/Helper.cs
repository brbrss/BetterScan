﻿using BetterScan;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace TestBetterScan
{
    //[DeploymentItem("test_resource\\", "test_resource")]
    [TestClass]
    public class HelperTest
    {
        [TestMethod]
        public void TestScan()
        {
            string path = "test_resource/";
            List<string> plist = new List<string> { "*" };
            IEnumerable<string> exfolder = new List<String>();
            var res = BetterScan.Helper.Search(path, exfolder, plist);
            string cwd = Directory.GetCurrentDirectory();
            Assert.AreEqual(res.Count, 2);
            string s1 = res[0].FullName.Replace(cwd, "");
            Assert.AreEqual("\\test_resource\\bigboo\\run.exe", s1);
        }
        [TestMethod]
        public void TestScan_exclude()
        {
            string cwd = Directory.GetCurrentDirectory();
            string path = "test_resource/";
            List<string> plist = new List<string> { "*" };
            IEnumerable<string> exfolder
                = new List<String> { cwd + "\\test_resource\\real" };
            var res = Helper.Search(path, exfolder, plist);
            Assert.AreEqual(1, res.Count);
        }

        [TestMethod]
        public void TestToRelPath_basic()
        {
            string root = "x:/";
            string s1 = "x:/a1/r.bat";
            string symbol = "{f}";
            string res1 = Helper.ToRelPath(s1, root, symbol);
            Assert.AreEqual("{f}\\a1\\r.bat", res1);
        }

        [TestMethod]
        public void TestToRelPath_dot()
        {
            string root = "x:/root/";
            string s1 = "x:/a1/r.bat";
            string symbol = "{f}";
            string res1 = Helper.ToRelPath(s1, root, symbol);
            Assert.AreEqual("{f}\\..\\a1\\r.bat", res1);
        }

        [TestMethod]
        public void TestToRelPath_dot_noslash()
        {
            string root = "x:/root";
            string s1 = "x:\\a1\\r.bat";
            string symbol = "{f}";
            string res1 = Helper.ToRelPath(s1, root, symbol);
            Assert.AreEqual("{f}\\..\\a1\\r.bat", res1);
        }

        [TestMethod]
        public void TestToRelPath_dot_backslash()
        {
            string root = "x:\\a/b\\c/d\\root";
            string s1 = "x:/a\\b\\c/d\\a1\\r.bat";
            string symbol = "{f}";
            string res1 = Helper.ToRelPath(s1, root, symbol);
            Assert.AreEqual("{f}\\..\\a1\\r.bat", res1);
        }
        [TestMethod]
        public void TestToRelPath_more_dot()
        {
            string root = "x:\\a/b\\w/w\\root";
            string s1 = "x:/a\\b\\c/d\\a1\\r.bat";
            string symbol = "{f}";
            string res1 = Helper.ToRelPath(s1, root, symbol);
            Assert.AreEqual("{f}\\..\\..\\..\\c\\d\\a1\\r.bat", res1);
        }
        [TestMethod]
        public void TestToRelPath_diff_drive()
        {
            string root = "u:\\root";
            string s1 = "x:/d\\a1\\r.bat";
            string symbol = "{f}";
            string res1 = Helper.ToRelPath(s1, root, symbol);
            Assert.AreEqual(s1, res1);
        }

        [TestMethod]
        public void TestResolveRelPath_00()
        {
            string root = "u:\\root";
            string name = "r.bat";
            string target = "u:\\root\\r.bat";
            string res1 = Helper.ResolveRelPath(root, name);
            Assert.AreEqual(target, res1);
            string res2 = Helper.ResolveRelPath(root + "\\", name);
            Assert.AreEqual(target, res2);
        }

        [TestMethod]
        public void TestResolveRelPath_dot()
        {
            string root = "u:\\root";
            string name = ".\\r.bat";
            string target = "u:\\root\\r.bat";
            string res1 = Helper.ResolveRelPath(root, name);
            Assert.AreEqual(target, res1);
            string res2 = Helper.ResolveRelPath(root + "\\", name);
            Assert.AreEqual(target, res2);
        }
        [TestMethod]
        public void TestResolveRelPath_dotdot()
        {
            string root = "u:\\root";
            string name = "..\\r.bat";
            string target = "u:\\r.bat";
            string res1 = Helper.ResolveRelPath(root, name);
            Assert.AreEqual(target, res1);
            string res2 = Helper.ResolveRelPath(root + "\\", name);
            Assert.AreEqual(target, res2);
        }
    }
}