using System;
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
            if (data.Count == 0)
                throw new NotSupportedException("Random from empty is not allowed");
            
            return data[RNG.Int(data.Count)];
        }
        
        public static bool TryGetRandom<T>(this IList<T> data, out T random)
        {
            random = default!;
            
            if (data.Count == 0)
                return false;
            
            random = data[RNG.Int(data.Count)];

            return true;
        }
        
        public static bool AddRange<T>(this HashSet<T> source, IEnumerable<T> items)
        {
            // return true if one or more items were added, false otherwise
            return items.Aggregate(false, (current, item) => current | source.Add(item));
        }
    }
}