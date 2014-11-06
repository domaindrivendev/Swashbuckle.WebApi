using System.Collections.Generic;

namespace Swashbuckle
{
    internal static class ListExtensions
    {
        public static void AddTo<T>(this IEnumerable<T> items, IList<T> list)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        } 
    }
}