using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SS
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestEmptySpreadsheet()
        {
            Spreadsheet testSheet = new Spreadsheet();
            dynamic contents = testSheet.GetCellContents("a1");
        }
    }
}
