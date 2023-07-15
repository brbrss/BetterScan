using BetterScan;
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
            var res = BetterScan.Helper.Search(path, plist);
            string cwd = Directory.GetCurrentDirectory();
            Assert.AreEqual(res.Count, 2);
            string s1 = res[0].FullName.Replace(cwd, "");
            Assert.AreEqual("\\test_resource\\bigboo\\run.exe", s1);
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
    }
}
