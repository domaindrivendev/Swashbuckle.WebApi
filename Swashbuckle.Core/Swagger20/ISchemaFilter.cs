using System;

namespace Swashbuckle.Swagger20
{
    public interface ISchemaFilter
    {
        void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type);
    }
}