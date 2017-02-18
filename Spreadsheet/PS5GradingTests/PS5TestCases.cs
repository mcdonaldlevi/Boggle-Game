using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using Formulas;
using System.Collections.Generic;

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
        [ExpectedException(typeof(InvalidNameException))]
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

        [TestMethod]
        public void Dependencies1()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("B1", new Formula("A1*2"));
            sheet.SetCellContents("C1", new Formula("B1+A1"));
            ISet<string> set = sheet.SetCellContents("A1", 5);

            List<string> expected = new List<string> { "A1", "B1", "C1" };
            Assert.IsTrue(expected.Count == set.Count);
            foreach (var item in set)
            {
                Assert.IsTrue(expected.Contains(item));
            }
        }

        [TestMethod]
        public void Dependencies2()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A1", new Formula("B1 + B2"));
            sheet.SetCellContents("B1", new Formula("A2 + A3"));
            sheet.SetCellContents("C1", new Formula("D1 + D2"));
            sheet.SetCellContents("D2", new Formula("A1"));
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void Circular1()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A1", new Formula("B1+5"));
            sheet.SetCellContents("B1", new Formula("A1+3"));
        }

        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void Circular2()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A1", new Formula("B1+1"));
            sheet.SetCellContents("B1", new Formula("C1-43"));
            sheet.SetCellContents("C1", new Formula("A1*3"));
        }

        [TestMethod]
        public void nonEmptyCells1()
        {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetCellContents("A1", 1);
            sheet.SetCellContents("B1", "A1 * A1");
            sheet.SetCellContents("B1", "B1 + A1");

            List<string> expected = new List<string> { "A1", "B1" };

            int cell_count = 0;
            foreach (var cell in sheet.GetNamesOfAllNonemptyCells())
            {
                Assert.IsTrue(expected.Contains(cell));
                cell_count++;
            }
            Assert.IsTrue(cell_count == expected.Count);
        }
    }
}
