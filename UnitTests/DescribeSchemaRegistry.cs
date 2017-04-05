using Microsoft.VisualStudio.TestTools.UnitTesting;
using SchemaGenerator;

namespace UnitTests
{
    [TestClass]
    public class DescribeSchemaRegistry
    {
        [TestMethod]
        public void ItShouldCreateOneDefinition()
        {
            var sut = new SchemaRegistry();
            var actual = sut.GetOrRegister(typeof(SomeClass));
            Assert.AreEqual(sut.Definitions.Count, 1);
        }

        class SomeClass
        {
            public int ANumber { get; set; }
            public string SomeText { get; set; }
        }
    }
}
