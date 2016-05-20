using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Swashbuckle.Swagger
{
    public class TypeExtensionsTests
    {
        [TestCase(typeof(DateTime), "DateTime")]
        [TestCase(typeof(IEnumerable<string>), "IEnumerable[String]")]
        [TestCase(typeof(IDictionary<string, decimal>), "IDictionary[String,Decimal]")]
        public void FriendlyId_ReturnsNonQualifiedFriendlyId_IfFullyQualifiedFlagIsUnset(
            Type systemType,
            string expectedReturnValue)
        {
            Assert.AreEqual(expectedReturnValue, systemType.FriendlyId());
        }

        [TestCase(typeof(DateTime), "System.DateTime")]
        [TestCase(typeof(IEnumerable<string>), "System.Collections.Generic.IEnumerable[System.String]")]
        [TestCase(typeof(IDictionary<string, decimal>), "System.Collections.Generic.IDictionary[System.String,System.Decimal]")]
        [TestCase(typeof(TypeExtensionsTests.InnerType), "Swashbuckle.Swagger.TypeExtensionsTests.InnerType")]
        public void FriendlyId_ReturnsFullQualifiedFriendlyId_IfFullyQualifiedFlagIsSet(
            Type systemType,
            string expectedReturnValue)
        {
            Assert.AreEqual(expectedReturnValue, systemType.FriendlyId(true));
        }

        [Test]
        public void GetEnumNamesForSerialization_HonorsEnumMemberAttributes()
        {
            var enumNames = typeof(EnumWithMemberAttributes).GetEnumNamesForSerialization();
            CollectionAssert.AreEqual(new[] { "value-1", "value-2" }, enumNames);
        }

        private class InnerType
        {
            public string Property1 { get; set; }
        }

        public enum EnumWithMemberAttributes
        {
            [EnumMember(Value = "value-1")]
            Value1 = 0,

            [EnumMember(Value = "value-2")]
            Value2 = 1
        }
    }
}