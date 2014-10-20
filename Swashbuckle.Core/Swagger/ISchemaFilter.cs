using System;

namespace Swashbuckle.Swagger
{
    public interface ISchemaFilter
    {
        void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type);
    }
}