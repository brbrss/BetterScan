using BetterScan;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TestBetterScan
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestTest()
        {
            string path = "";
            List<string> plist = new List<string> { };
            //BetterScan.Helper.Search(path, plist);
            Assert.IsTrue(true);
        }

    }
}
