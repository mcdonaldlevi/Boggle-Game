using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Formulas;
namespace SS
{
    [TestClass]
    public class UnitTest1
    {
        [ExpectedException(typeof(InvalidNameException))]
        [TestMethod]
        public void TestEmptySpreadsheet()
        {
            Spreadsheet testSheet = new Spreadsheet();
            dynamic contents = testSheet.GetCellContents("a1");
        }
        [TestMethod]
        public void TestGetSetContents1()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("a1", "5");
            Assert.IsTrue((double)testSheet.GetCellContents("A1") == 5);
        }
        [TestMethod]
        public void TestGetSetContents2()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("a1", "hello");
            Assert.IsTrue((string)testSheet.GetCellContents("A1") == "hello");
        }
        [TestMethod]
        public void TestGetSetContents3()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("a2", "2");
            testSheet.SetContentsOfCell("a3", "3");
            testSheet.SetContentsOfCell("a1", "=a2+a3");
            Assert.IsTrue(testSheet.GetCellContents("A1").ToString() == "A2+A3");
            Assert.IsTrue((double)testSheet.GetCellValue("A1") == 5);
        }
        [ExpectedException(typeof(InvalidNameException))]
        [TestMethod]
        public void TestGetSetContents4()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("10", "5");
        }
        [ExpectedException(typeof(InvalidNameException))]
        [TestMethod]
        public void TestGetSetContents5()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("10", "hello");
        }
        [ExpectedException(typeof(InvalidNameException))]
        [TestMethod]
        public void TestGetSetContents6()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("10", "=a2+a3");
        }
        [TestMethod]
        public void TestGetSetContents7()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("a1", "5");
            testSheet.SetContentsOfCell("a1", "10");
            Assert.IsTrue((double)testSheet.GetCellContents("A1") == 10);
        }
        [TestMethod]
        public void TestGetSetContents8()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("a1", "hello");
            testSheet.SetContentsOfCell("a1", "goodbye");
            Assert.IsTrue((string)testSheet.GetCellContents("A1") == "goodbye");
        }
        [TestMethod]
        public void TestGetSetContents9()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("a2", "2");
            testSheet.SetContentsOfCell("a3", "3");
            testSheet.SetContentsOfCell("a4", "4");
            testSheet.SetContentsOfCell("a5", "5");
            testSheet.SetContentsOfCell("a6", "6");
            testSheet.SetContentsOfCell("a1", "=a5+a6");
            Assert.IsTrue(testSheet.GetCellContents("A1").ToString() == "A5+A6");
            Assert.IsTrue((double)testSheet.GetCellValue("A1") == 11);
        }
        [TestMethod]
        public void TestNamesofNonEmptyCells()
        {
            Spreadsheet testSheet = new Spreadsheet();
            IEnumerable<string> names = testSheet.GetNamesOfAllNonemptyCells();
            Assert.IsFalse(names.GetEnumerator().MoveNext());
        }
        [ExpectedException(typeof(CircularException))]
        [TestMethod]
        public void TestCircularDependency()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("a1", "1");
            testSheet.SetContentsOfCell("a2", "2");
            testSheet.SetContentsOfCell("a3", "=a1+a2");
            testSheet.SetContentsOfCell("a1", "=a2+a3");
        }
        [TestMethod]
        public void TestCellRecalculate()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetContentsOfCell("a1", "1");
            testSheet.SetContentsOfCell("a2", "2");
            testSheet.SetContentsOfCell("a3", "=a1+a2");
            Assert.IsTrue((double)testSheet.GetCellValue("A3") == 3);
            testSheet.SetContentsOfCell("A1", "10");
            Assert.IsTrue((double)testSheet.GetCellValue("A3") == 12);
        }
    }
}
