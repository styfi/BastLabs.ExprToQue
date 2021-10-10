using BastLabs.EprToQue.Tests.MockClasses;
using BastLabs.ExprToQue;
using NUnit.Framework;

namespace BastLabs.EprToQue.Tests
{
    public class ExprToQueTest
    {
        protected ExprToQueService ExprToQueService { get; set; }
        protected Product Product { get; set; }

        [SetUp]
        public void Setup()
        {
            ExprToQueService = new ExprToQueService();
            Product = new Product();
        }
    }
}
