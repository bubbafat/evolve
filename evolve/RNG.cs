using System;
using System.Collections.Concurrent;

namespace evolve
{
    public static class RNG
    {
        private static readonly RandomGenerator _rng = new RandomGenerator();

        public static int Int() => _rng.Int();
        public static float Float() => _rng.Float();
        public static bool Bool() => _rng.Bool();

        public static int Int(int max) => _rng.Int() % max;
    }

    public class RandomGenerator
    {
        private readonly Random _rng = new Random();
        private readonly ConcurrentQueue<int> _ints = new ConcurrentQueue<int>();
        private readonly ConcurrentQueue<float> _floats = new ConcurrentQueue<float>();
        private const int CacheLimit = 10000;
        private readonly object _lock = new object();
        
        public int Int()
        {
            if (_ints.TryDequeue(out var result)) 
                return result;
            
            lock (_lock)
            {
                result = _rng.Next();

                if (_ints.IsEmpty)
                {
                    for (int i = 0; i < CacheLimit; i++)
                    {
                        _ints.Enqueue(_rng.Next());
                    }
                }
            }

            return result;
        }

        public bool Bool()
        {
            return Float() < 0.5f;
        }

        public float Float()
        {
            if (_floats.TryDequeue(out var result)) 
                return result;
            
            lock (_lock)
            {
                result = (float) _rng.NextDouble();

                if (_floats.IsEmpty)
                {
                    for (int i = 0; i < CacheLimit; i++)
                    {
                        _floats.Enqueue((float) _rng.NextDouble());
                    }
                }
            }

            return result;
        }
    }
}