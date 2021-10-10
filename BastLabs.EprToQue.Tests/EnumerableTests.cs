using BastLabs.EprToQue.Tests.MockClasses;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BastLabs.EprToQue.Tests
{
    public class EnumerableTests : ExprToQueTest
    {
        [Test]
        public void IntListContains()
        {
            IEnumerable<int> intList = new List<int>() { 1, 2, 3 };
            Expression<Func<Product, bool>> expr = p => intList.Contains(p.ID);

            Assert.AreEqual("(ID IN (1,2,3))", ExprToQueService.Translate(expr));
        }

        [Test]
        public void IntListNotContains()
        {
            IEnumerable<int> intList = new List<int>() { 1, 2, 3 };
            Expression<Func<Product, bool>> expr = p => !intList.Contains(p.ID);

            Assert.AreEqual(" NOT (ID IN (1,2,3))", ExprToQueService.Translate(expr));
        }

        [Test]
        public void EmptyIntListContains()
        {
            IEnumerable<int> intList = new List<int>();
            Expression<Func<Product, bool>> expr = p => intList.Contains(p.ID);

            Assert.AreEqual("(ID IN (SELECT 1 WHERE 1=0))", ExprToQueService.Translate(expr));
        }

        [Test]
        public void EmptyIntListNotContains()
        {
            IEnumerable<int> intList = new List<int>();
            Expression<Func<Product, bool>> expr = p => !intList.Contains(p.ID);

            Assert.AreEqual(" NOT (ID IN (SELECT 1 WHERE 1=0))", ExprToQueService.Translate(expr));
        }

        [Test]
        public void StringListContains()
        {
            IEnumerable<string> stringList = new List<string>() { "a", "b", "c" };
            Expression<Func<Product, bool>> expr = p => stringList.Contains(p.Name);

            Assert.AreEqual("(Name IN ('a','b','c'))", ExprToQueService.Translate(expr));
        }

        [Test]
        public void StringListNotContains()
        {
            IEnumerable<string> stringList = new List<string>() { "a", "b", "c" };
            Expression<Func<Product, bool>> expr = p => !stringList.Contains(p.Name);

            Assert.AreEqual(" NOT (Name IN ('a','b','c'))", ExprToQueService.Translate(expr));
        }

        [Test]
        public void EmptyStringListContains()
        {
            IEnumerable<string> stringList = new List<string>();
            Expression<Func<Product, bool>> expr = p => stringList.Contains(p.Name);

            Assert.AreEqual("(Name IN (SELECT 1 WHERE 1=0))", ExprToQueService.Translate(expr));
        }

        [Test]
        public void EmptyStringListNotContains()
        {
            IEnumerable<string> stringList = new List<string>();
            Expression<Func<Product, bool>> expr = p => !stringList.Contains(p.Name);

            Assert.AreEqual(" NOT (Name IN (SELECT 1 WHERE 1=0))", ExprToQueService.Translate(expr));
        }
    }
}
