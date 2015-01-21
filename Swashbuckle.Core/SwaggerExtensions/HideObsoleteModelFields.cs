using System;
using System.Linq;
using Swashbuckle.Swagger;

namespace Swashbuckle.SwaggerExtensions
{
    class HideObsoleteModelFields : IModelFilter
    {
        public void Apply(DataType model, DataTypeRegistry dataTypeRegistry, Type type)
        {
            foreach (
                var name in
                    from name in model.Properties.Keys.ToArray()
                    let property = type.GetProperty(name)
                    where property != null && property.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any()
                    select name)
            {
                model.Properties.Remove(name);
            }
        }
    }
}