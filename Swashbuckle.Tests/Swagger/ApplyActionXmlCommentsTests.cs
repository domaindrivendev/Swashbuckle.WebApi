using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.XPath;
using NUnit.Framework;
using Swashbuckle.Swagger;

namespace Swashbuckle.Tests.Swagger
{
    [TestFixture]
    public class ApplyActionXmlCommentsTests
    {
        private ApiExplorer _apiExplorer;
        private ApplyActionXmlComments _filter;

        [TestFixtureSetUp]
        public void Setup()
        {
            // Get dummy ApiExplorer
            var config = new HttpConfiguration();
            Dummy.WebApiConfig.Register(config);
            _apiExplorer = new ApiExplorer(config);
            config.EnsureInitialized();

            var xmlCommentsDoc = new XPathDocument(String.Format(@"{0}\XmlComments.xml", AppDomain.CurrentDomain.BaseDirectory));
            _filter = new ApplyActionXmlComments(xmlCommentsDoc);
        }
        
        [Test]
        public void It_should_apply_action_summary_if_available_otherwise_blank()
        {
            ApplyFilterFor("Products", "GetAll", operation =>
                Assert.IsNull(operation.Summary));

            ApplyFilterFor("Products", "GetByType", operation =>
                Assert.AreEqual("Returns a list of products filtered by type", operation.Summary));

            ApplyFilterFor("RandomStuff", "GetUnicorns", operation =>
                Assert.AreEqual("Returns a list of unicorns filtered by property names", operation.Summary));
        }

        [Test]
        public void It_should_apply_action_remarks_if_available_otherwise_blank()
        {
            ApplyFilterFor("Products", "GetAll", operation =>
                Assert.IsNull(operation.Notes));

            ApplyFilterFor("Products", "GetByType", operation =>
                Assert.AreEqual("There are several different Product types", operation.Notes));

            ApplyFilterFor("RandomStuff", "GetUnicorns", operation =>
                Assert.IsNull(operation.Notes));
        }

        [Test]
        public void It_should_apply_parameter_descriptions_if_available_otherwise_blank()
        {
            ApplyFilterFor("Products", "GetByType", operation =>
                {
                    Assert.IsNull(operation.Parameters[0].Description);
                    Assert.IsNull(operation.Parameters[1].Description);
                });

            ApplyFilterFor("RandomStuff", "GetUnicorns", operation =>
                Assert.AreEqual("A list of name/value pairs for filtering results", operation.Parameters[0].Description));
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