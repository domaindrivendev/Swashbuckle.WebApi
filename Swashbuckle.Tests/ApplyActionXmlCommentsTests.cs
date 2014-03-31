using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.XPath;
using NUnit.Framework;
using Swashbuckle.Core.Swagger;

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
            TestApp.Core.WebApiConfig.Register(httpConfiguration);
            _apiExplorer = new ApiExplorer(httpConfiguration);

            var xmlCommentsDoc = new XPathDocument(String.Format(@"{0}\XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory));
            _filter = new ApplyActionXmlComments(xmlCommentsDoc);
        }
        
        [Test]
        public void It_should_apply_action_summary_if_available_otherwise_blank()
        {
            ApplyFilterFor("Orders", "GetAll", operationSpec =>
                Assert.IsNull(operationSpec.Summary));

            ApplyFilterFor("OrderItems", "GetAll", operationSpec =>
                Assert.AreEqual("Get all order items", operationSpec.Summary));

            ApplyFilterFor("OrderItems", "GetById", operationSpec =>
                Assert.AreEqual("Get order item by id", operationSpec.Summary));

            ApplyFilterFor("OrderItems", "GetByPropertyValues", operationSpec =>
                Assert.AreEqual("Retreive items in an order by property names and values", operationSpec.Summary));
        }

        [Test]
        public void It_should_apply_action_remarks_if_available_otherwise_blank()
        {
            ApplyFilterFor("Orders", "GetAll", operationSpec =>
                Assert.IsNull(operationSpec.Notes));

            ApplyFilterFor("OrderItems", "GetAll", operationSpec =>
                Assert.AreEqual("Returns all three order items we've got here", operationSpec.Notes));

            ApplyFilterFor("OrderItems", "GetById", operationSpec =>
                Assert.IsNull(operationSpec.Notes));

            ApplyFilterFor("OrderItems", "GetByPropertyValues", operationSpec =>
                Assert.IsNull(operationSpec.Notes));
        }

        [Test]
        public void It_should_apply_parameter_descriptions_if_available_otherwise_blank()
        {
            ApplyFilterFor("Orders", "GetByParams", operationSpec =>
            {
                Assert.IsNull(operationSpec.Parameters[0].Description);
                Assert.IsNull(operationSpec.Parameters[1].Description);
            });

            ApplyFilterFor("OrderItems", "GetAll", operationSpec =>
            {
                Assert.IsNull(operationSpec.Parameters[0].Description);
                Assert.IsNull(operationSpec.Parameters[1].Description);
            });

            ApplyFilterFor("OrderItems", "GetById", operationSpec =>
            {
                Assert.AreEqual("The identifier for the order", operationSpec.Parameters[0].Description);
                Assert.AreEqual("The identifier for the requested order item", operationSpec.Parameters[1].Description);
            });

            ApplyFilterFor("OrderItems", "GetByPropertyValues", operationSpec =>
            {
                Assert.AreEqual("The identifier for the order", operationSpec.Parameters[0].Description);
                Assert.AreEqual("Dictionary of property names and values", operationSpec.Parameters[1].Description);
            });
        }

        private void ApplyFilterFor(string controllerName, string actionName, Action<OperationSpec> applyAssertions)
        {
            var apiDescription = _apiExplorer.ApiDescriptions.Single(apiDesc => apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName == controllerName
                && apiDesc.ActionDescriptor.ActionName == actionName);

            var operationSpec = new OperationSpec
                {
                    Summary = "foo",
                    Notes = "foo",
                    Parameters = apiDescription.ParameterDescriptions
                        .Select(paramDesc =>
                            new ParameterSpec
                            {
                                Name = paramDesc.Name,
                                Description = "foo" 
                            })
                         .ToList()
                };

            _filter.Apply(operationSpec, null, null, apiDescription);

            applyAssertions(operationSpec);
        }
    }
}