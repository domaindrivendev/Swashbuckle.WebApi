using System;
using System.Linq;

namespace Swashbuckle.Models
{
    public static class TypeExtensions
    {
        public static Type AsGenericType(this Type source, Type genericType)
        {
            return source.GetInterfaces()
                .Union(new[] {source})
                .SingleOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericType);
        }
    }
}