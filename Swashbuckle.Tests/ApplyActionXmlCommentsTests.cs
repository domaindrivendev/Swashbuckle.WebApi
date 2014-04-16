using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.XPath;
using NUnit.Framework;
using Swashbuckle.Swagger;

namespace Swashbuckle.Tests
{
    [TestFixture]
    public class ApplyActionXmlCommentsTests
    {
        private ApiExplorer _apiExplorer;
        private ApplyActionXmlComments _filter;

        [SetUp]
        public void Setup()
        {
            // Get ApiExplorer for TestApp
            var httpConfiguration = new HttpConfiguration();
            TestApp.WebApiConfig.Register(httpConfiguration);
            _apiExplorer = new ApiExplorer(httpConfiguration);

            var xmlCommentsDoc = new XPathDocument(String.Format(@"{0}\XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory));
            _filter = new ApplyActionXmlComments(xmlCommentsDoc);
        }
        
        [Test]
        public void It_should_apply_action_summary_if_available_otherwise_blank()
        {
            ApplyFilterFor("Orders", "Post", operation =>
                Assert.IsNull(operation.Summary));

            ApplyFilterFor("Orders", "GetAll", operation =>
                Assert.AreEqual("Get all orders in the system", operation.Summary));

            ApplyFilterFor("OrderItems", "GetAll", operation =>
                Assert.AreEqual("Get all order items", operation.Summary));

            ApplyFilterFor("OrderItems", "GetById", operation =>
                Assert.AreEqual("Get order item by id", operation.Summary));

            ApplyFilterFor("OrderItems", "GetByPropertyValues", operation =>
                Assert.AreEqual("Retreive items in an order by property names and values", operation.Summary));
        }

        [Test]
        public void It_should_apply_action_remarks_if_available_otherwise_blank()
        {
            ApplyFilterFor("Orders", "Post", operation =>
                Assert.IsNull(operation.Notes));

            ApplyFilterFor("Orders", "GetAll", operation =>
                Assert.AreEqual("For power users only", operation.Notes));

            ApplyFilterFor("OrderItems", "GetAll", operation =>
                Assert.AreEqual("Returns all three order items we've got here", operation.Notes));

            ApplyFilterFor("OrderItems", "GetById", operation =>
                Assert.IsNull(operation.Notes));

            ApplyFilterFor("OrderItems", "GetByPropertyValues", operation =>
                Assert.IsNull(operation.Notes));
        }

        [Test]
        public void It_should_apply_parameter_descriptions_if_available_otherwise_blank()
        {
            ApplyFilterFor("Orders", "GetByParams", operation =>
            {
                Assert.IsNull(operation.Parameters[0].Description);
                Assert.IsNull(operation.Parameters[1].Description);
            });

            ApplyFilterFor("OrderItems", "GetAll", operation =>
            {
                Assert.IsNull(operation.Parameters[0].Description);
                Assert.IsNull(operation.Parameters[1].Description);
            });

            ApplyFilterFor("OrderItems", "GetById", operation =>
            {
                Assert.AreEqual("The identifier for the order", operation.Parameters[0].Description);
                Assert.AreEqual("The identifier for the requested order item", operation.Parameters[1].Description);
            });

            ApplyFilterFor("OrderItems", "GetByPropertyValues", operation =>
            {
                Assert.AreEqual("The identifier for the order", operation.Parameters[0].Description);
                Assert.AreEqual("Dictionary of property names and values", operation.Parameters[1].Description);
            });
        }

        private void ApplyFilterFor(string controllerName, string actionName, Action<Operation> applyAssertions)
        {
            var apiDescription = _apiExplorer.ApiDescriptions.Single(apiDesc => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName == controllerName
                && apiDesc.ActionDescriptor.ActionName == actionName);

            var operation = new Operation
                {
                    Summary = "foo",
                    Notes = "foo",
                    Parameters = apiDescription.ParameterDescriptions
                        .Select(paramDesc =>
                            new Parameter
                            {
                                Name = paramDesc.Name,
                                Description = "foo" 
                            })
                         .ToList()
                };

            _filter.Apply(operation, null, apiDescription);

            applyAssertions(operation);
        }
    }
}