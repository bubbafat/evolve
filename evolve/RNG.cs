using System;
using System.Collections.Concurrent;

namespace evolve
{
    public static class RNG
    {
        [ThreadStatic]
        private static RandomGenerator _rng = new RandomGenerator();

        private static RandomGenerator generator
        {
            get
            {
                if (_rng == null) 
                    _rng = new RandomGenerator();

                return _rng;
            }
        }

        public static int Int() => generator.Int();
        public static double Double() => generator.Double();
        public static bool Bool() => generator.Bool();

        public static int Int(int max) => generator.Int() % max;
    }

    public class RandomGenerator
    {
        private readonly Random _rng = new Random();

        public int Int()
        {
            return _rng.Next();
        }

        public bool Bool()
        {
            return Double() < 0.5;
        }

        public double Double()
        {
            return _rng.NextDouble();
        }
    }
}