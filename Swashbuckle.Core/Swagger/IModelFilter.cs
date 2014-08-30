using System;

namespace Swashbuckle.Swagger
{
    public interface IModelFilter
    {
        void Apply(Model model, TypeSystem typeSystem, Type type);
    }
}