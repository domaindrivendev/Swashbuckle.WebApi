using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SchemaGenerator;

namespace UnitTests
{
    [TestClass]
    public class DescribeSchemaRegistry
    {
        [TestMethod]
        public void TestMethod1()
        {
            var sut = new SchemaRegistry();
            var actual = sut.GetOrRegister(typeof(SomeClass));
            
        }

        class SomeClass
        {
            public int ANumber { get; set; }
            public string SomeText { get; set; }
        }
    }
}
