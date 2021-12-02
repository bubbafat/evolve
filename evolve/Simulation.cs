using System;

namespace evolve
{
    public class Simulation
    {
        public static readonly int TotalNodes = 1000;
        public static readonly int Generations = 50;
        public static readonly int StepsPerGeneration = 100;
        public static readonly double SuccessThreshold = 0.95;
        public static readonly int BoardDimensions = 128;
        public static readonly int GenesPerGenome = 32;
        public static readonly int InnerNeurons = 16;
        
        public static int CurrentGeneration = 0;
        public static int CurrentStep = 0;

        public static float ActivationFunction(float value) {
            return 1.0f / (1.0f + (float) Math.Exp(-value));
        }

        public static bool WeightToBool(float weight) {
            return RNG.Float() < weight;
        }
    }
}