using System;

namespace Swashbuckle.Core.Swagger
{
    public interface IModelFilter
    {
        void Apply(DataType model, DataTypeRegistry dataTypeRegistry, Type type);
    }
}
