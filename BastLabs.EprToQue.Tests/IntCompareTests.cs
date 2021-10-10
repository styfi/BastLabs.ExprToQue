using BastLabs.EprToQue.Tests.MockClasses;
using BastLabs.ExprToQue;
using NUnit.Framework;
using System;
using System.Linq.Expressions;

namespace BastLabs.EprToQue.Tests
{
    public class IntCompareTests : ExprToQueTest
    {
        [Test]
        public void Int_Const()
        {
            Expression<Func<Product, bool>> expr = p => p.ID == 5;

            Assert.AreEqual("(ID = 5)", ExprToQueService.Translate(expr));
        }

        [Test]
        public void Const_Int()
        {
            Expression<Func<Product, bool>> expr = p => 5 == p.ID;

            Assert.AreEqual("(5 = ID)", ExprToQueService.Translate(expr));
        }

        [Test]
        public void IntNullable_Const()
        {
            Expression<Func<Product, bool>> expr = p => p.IDNullable == 5;

            Assert.AreEqual("(IDNullable = 5)", ExprToQueService.Translate(expr));
        }

        [Test]
        public void Const_IntNullableCoalesce()
        {
            Expression<Func<Product, bool>> expr = p => 5 == (p.IDNullable ?? 0);

            Assert.AreEqual("(5 = Coalesce(IDNullable, 0))", ExprToQueService.Translate(expr));
        }

        [Test]
        public void IntNullableCoalesceToConstCompare()
        {
            Expression<Func<Product, bool>> expr = p => (p.IDNullable ?? 0) == 5;

            Assert.AreEqual("(Coalesce(IDNullable, 0) = 5)", ExprToQueService.Translate(expr));
        }
    }
}