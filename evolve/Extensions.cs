using System.Collections.Generic;

namespace evolve
{
    public static class Extensions
    {
        public static T[] Shuffle<T>(this T[] data)
        {
            int n = data.Length;
            while (n > 1)
            {
                n--;
                int k = RNG.Int(n + 1);
                (data[k], data[n]) = (data[n], data[k]);
            }

            return data;
        }

        public static IList<T> Shuffle<T>(this IList<T> data)
        {
            int n = data.Count;
            while (n > 1)
            {
                n--;
                int k = RNG.Int(n + 1);
                (data[k], data[n]) = (data[n], data[k]);
            }

            return data;
        }

        public static T Random<T>(this T[] data)
        {
            return data[RNG.Int(data.Length)];
        }

        public static T Random<T>(this IList<T> data)
        {
            return data[RNG.Int(data.Count)];
        }
    }
}