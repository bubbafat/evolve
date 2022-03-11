using System;
using System.Collections.Concurrent;

namespace evolve
{
    public static class RNG
    {
        [ThreadStatic]
        private static RandomGenerator? _rng;

        private static RandomGenerator gen
        {
            get
            {
                if (_rng == null) _rng = new RandomGenerator();
                return _rng;
            }
        }

        public static int Int() => gen.Int();
        public static double Double() => gen.Double();
        public static bool Bool() => gen.Bool();

        public static int Int(int max) => gen.Int() % max;

        private class RandomGenerator
        {
            private readonly Random _rng = new Random();

            public int Int()
            {
                return _rng.Next();
            }

            public bool Bool()
            {
                return Int() % 2 == 0;
            }

            public double Double()
            {
                return _rng.NextDouble();
            }
        }
    }
}