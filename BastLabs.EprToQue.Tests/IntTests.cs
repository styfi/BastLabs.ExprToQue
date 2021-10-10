using BastLabs.EprToQue.Tests.MockClasses;
using BastLabs.ExprToQue;
using NUnit.Framework;
using System;
using System.Linq.Expressions;

namespace BastLabs.EprToQue.Tests
{
    public class IntTests : ExprToQueTest
    {
        [Test]
        public void IntToConstCompare()
        {
            Expression<Func<Product, bool>> expr = p => p.ID == 5;

            Assert.AreEqual("(ID = 5)", ExprToQueService.Translate(expr));
        }

        [Test]
        public void IntNullableToConstCompare()
        {
            Expression<Func<Product, bool>> expr = p => p.IDNullable == 5;

            Assert.AreEqual("(IDNullable = 5)", ExprToQueService.Translate(expr));
        }

        [Test]
        public void IntNullableCoalesceToConstCompare()
        {
            Expression<Func<Product, bool>> expr = p => (p.IDNullable ?? 0) == 5;

            Assert.AreEqual("(Coalesce(IDNullable, 0) = 5)", ExprToQueService.Translate(expr));
        }
    }
}