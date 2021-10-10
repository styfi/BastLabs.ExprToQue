using BastLabs.EprToQue.Tests.MockClasses;
using BastLabs.ExprToQue;
using NUnit.Framework;
using System;
using System.Linq.Expressions;

namespace BastLabs.EprToQue.Tests
{
    public class StringCompareTests : ExprToQueTest
    {
        [Test]
        public void String_Const()
        {
            Expression<Func<Product, bool>> expr;
            expr = p => p.Name == "abc";

            Assert.AreEqual("(Name = 'abc')", ExprToQueService.Translate(expr));
        }

        [Test]
        public void StringCoalesce_Const()
        {
            Expression<Func<Product, bool>> expr;
            expr = p => (p.Name ?? "") == "abc";

            Assert.AreEqual("(Coalesce(Name, '') = 'abc')", ExprToQueService.Translate(expr));
        }

        [Test]
        public void MemberString_MemberString()
        {
            Expression<Func<Product, bool>> expr;
            expr = p => p.Name == p.String;

            Assert.AreEqual("(Name = String)", ExprToQueService.Translate(expr));
        }

        [Test]
        public void IsNullOrEmpty()
        {
            Expression<Func<Product, bool>> expr;
            expr = p => string.IsNullOrEmpty(p.Name);

            Assert.AreEqual("Coalesce(Name, '') = ''", ExprToQueService.Translate(expr));
        }

        [Test]
        public void NotIsNullOrEmpty()
        {
            Expression<Func<Product, bool>> expr;
            expr = p => !string.IsNullOrEmpty(p.Name);

            Assert.AreEqual(" NOT Coalesce(Name, '') = ''", ExprToQueService.Translate(expr));
        }
    }
}