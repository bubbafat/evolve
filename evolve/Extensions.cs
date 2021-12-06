using System.Collections.Generic;
using System.Linq;

namespace evolve
{
    public static class Extensions
    {
        public static IList<T> Shuffle<T>(this IList<T> data)
        {
            var n = data.Count;
            while (n > 1)
            {
                n--;
                var k = RNG.Int(n + 1);
                (data[k], data[n]) = (data[n], data[k]);
            }

            return data;
        }
        
        public static T Random<T>(this IList<T> data)
        {
            return data[RNG.Int(data.Count)];
        }
        
        public static bool AddRange<T>(this HashSet<T> source, IEnumerable<T> items)
        {
            // return true if one or more items were added, false otherwise
            return items.Aggregate(false, (current, item) => current | source.Add(item));
        }
    }
}