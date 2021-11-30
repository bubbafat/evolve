using System;

namespace evolve
{
    public class Simulation
    {
        public static float ActivationFunction(float value) {
            return 1.0f / (1.0f + (float) Math.Exp(-value));
        }
    }
}