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
            testSheet.SetCellContents("a1", 5);
            Assert.IsTrue((double)testSheet.GetCellContents("a1") == 5);
        }
        [TestMethod]
        public void TestGetSetContents2()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetCellContents("a1", "hello");
            Assert.IsTrue((string)testSheet.GetCellContents("a1") == "hello");
        }
        [TestMethod]
        public void TestGetSetContents3()
        {
            Spreadsheet testSheet = new Spreadsheet();
            Formula myFormula = new Formula("a2+a3");
            testSheet.SetCellContents("a1", myFormula);
            Assert.IsTrue(testSheet.GetCellContents("a1").ToString() == "a2+a3");
        }
        [ExpectedException(typeof(InvalidNameException))]
        [TestMethod]
        public void TestGetSetContents4()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetCellContents("10", 5);
        }
        [ExpectedException(typeof(InvalidNameException))]
        [TestMethod]
        public void TestGetSetContents5()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetCellContents("10", "hello");
        }
        [ExpectedException(typeof(InvalidNameException))]
        [TestMethod]
        public void TestGetSetContents6()
        {
            Spreadsheet testSheet = new Spreadsheet();
            Formula myFormula = new Formula("a2+a3");
            testSheet.SetCellContents("10", myFormula);
        }
        [TestMethod]
        public void TestGetSetContents7()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetCellContents("a1", 5);
            testSheet.SetCellContents("a1", 10);
            Assert.IsTrue((double)testSheet.GetCellContents("a1") == 10);
        }
        [TestMethod]
        public void TestGetSetContents8()
        {
            Spreadsheet testSheet = new Spreadsheet();
            testSheet.SetCellContents("a1", "hello");
            testSheet.SetCellContents("a1", "goodbye");
            Assert.IsTrue((string)testSheet.GetCellContents("a1") == "goodbye");
        }
        [TestMethod]
        public void TestGetSetContents9()
        {
            Spreadsheet testSheet = new Spreadsheet();
            Formula myFormula = new Formula("a2+a3");
            Formula myFormula2 = new Formula("a5+a6");
            testSheet.SetCellContents("a1", myFormula);
            testSheet.SetCellContents("a1", myFormula2);
            Assert.IsTrue(testSheet.GetCellContents("a1").ToString() == "a5+a6");
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
            Formula myFormula1 = new Formula("a1+a2");
            Formula myFormula2 = new Formula("a2+a3");
            testSheet.SetCellContents("a3", myFormula1);
            testSheet.SetCellContents("a1", myFormula2);
        }
    }
}
