using System;

namespace Swashbuckle.Swagger
{
    public interface IModelFilter
    {
        void Apply(DataType model, DataTypeRegistry dataTypeRegistry, Type type);
    }
}