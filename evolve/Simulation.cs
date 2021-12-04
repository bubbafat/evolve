using System;

namespace evolve
{
    public static class Simulation
    {
        public static readonly int TotalNodes = 1000;
        public static readonly int Generations = 1000;
        public static readonly int StepsPerGeneration = 300;
        public static readonly double SuccessThreshold = 0.95;
        public static readonly int BoardDimensions = 128;
        public static readonly int GenesPerGenome = 6;
        public static readonly int InnerNeurons = 3;
        public static readonly float MutationChance = 0.001f;
        public static readonly bool EnableRendering = true;
        public static readonly bool PerformMostCompellingAction = true;
        public static bool WinThresholdExceeded = false;

        public static bool RenderFrame =>
            EnableRendering
                &&  (CurrentGeneration == 1
                    || WinThresholdExceeded
                    || CurrentGeneration == Generations);

        public static int CurrentGeneration = 0;
        public static int CurrentStep = 0;

        public static float ActivationFunction(float value)
        {
            return (float)Math.Tanh(value);
//            return 1.0f / (1.0f + (float) Math.Exp(-value));
        }

        public static bool WeightToBool(float weight)
        {
            return RNG.Float() < weight;
        }
    }
}