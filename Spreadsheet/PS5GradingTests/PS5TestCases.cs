using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using Formulas;

namespace PS5GradingTests
{
    [TestClass]
    public class PS5TestCases
    {
        [TestMethod]
        public void SetCell1()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A1", 4);
            
            Assert.IsTrue(Convert.ToInt16(sheet.GetCellContents("A1")) == 4);
        }

        [TestMethod]
        public void SetCell2()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            Formula f = new Formula("5 + x");
            sheet.SetCellContents("C4", f);
        }

        [TestMethod]
        public void SetCell3()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("B44", "asdf");
            Assert.IsTrue(Convert.ToString(sheet.GetCellContents("B44")).Equals("asdf"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCells4()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("44", "C");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCells5()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("X04", 3);
        }
    }
}
