using System;

namespace evolve
{
    public static class Simulation
    {
        public const int TotalNodes = 1000;
        public const int Generations = 200;
        public const int StepsPerGeneration = 150;
        public const double SuccessThreshold = 0.95;
        public const int BoardDimensions = 128;
        public const int GenesPerGenome = 12;
        public const int InnerNeurons = 6;
        public const float MutationChance = 0.001f;
        public const bool RequireExactGenesPerGenome = true;

        public static bool WinThresholdExceeded = false;

        public static bool RenderFrame =>
            true
                &&  (CurrentGeneration == 1
                    || WinThresholdExceeded
                    || CurrentGeneration == Generations);

        public static int CurrentGeneration = 0;
        public static int CurrentStep = 0;

        public static float ActivationFunction(float value)
        {
            return (float)Math.Tanh(value);
        }

        public static bool WeightToBool(float weight)
        {
            return RNG.Float() < weight;
        }
    }
}