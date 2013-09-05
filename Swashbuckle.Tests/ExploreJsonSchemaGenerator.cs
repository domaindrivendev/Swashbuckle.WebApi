using System;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Swashbuckle.TestApp.Models;

namespace Swashbuckle.Tests
{
    [TestFixture]
    public class ExploreJsonSchemaGenerator
    {
        [Test]
        public void TryItOut()
        {
            var generator = new JsonSchemaGenerator {UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseTypeName};

            var schema = generator.Generate(typeof (Order));

            Console.WriteLine(JsonConvert.SerializeObject(schema));
        }
    }
}