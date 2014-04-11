using System;
using System.Collections.Generic;
using System.Xml.XPath;
using NUnit.Framework;
using Swashbuckle.Core.Swagger;
using Swashbuckle.TestApp.Core.Models;

namespace Swashbuckle.Tests
{
    [TestFixture]
    public class ApplyTypeXmlCommentsTests
    {
        private ApplyTypeXmlComments _filter;
        private DataType _customerModel;

        [SetUp]
        public void Setup()
        {
            var xmlCommentsDoc = new XPathDocument(String.Format(@"{0}\XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory));
            _filter = new ApplyTypeXmlComments(xmlCommentsDoc);
        }
        
        [Test]
        public void It_should_apply_the_type_summary_if_available_otherwise_blank()
        {
            var customerModel = GetDummyCustomerModel();
            _filter.Apply(customerModel, typeof(Customer));

            Assert.AreEqual("Represents a registered customer", customerModel.Description);
        }

        [Test]
        public void It_should_apply_property_summaries_if_available_otherwise_blank()
        {
            var customerModel = GetDummyCustomerModel();
            _filter.Apply(customerModel, typeof(Customer));

            Assert.AreEqual("Unique identifier for the customer", customerModel.Properties["Id"].Description);
            Assert.AreEqual("Lists all registered associates", customerModel.Properties["Associates"].Description);
        }

        private static DataType GetDummyCustomerModel()
        {
            return new DataType()
                {
                    Properties = new Dictionary<string, DataType>()
                        {
                            {"Id", new DataType()},
                            {"Associates", new DataType()}
                        }
                };
        }
    }
}