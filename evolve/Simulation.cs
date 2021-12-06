using System;

namespace evolve
{
    public static class Simulation
    {
        public const int TotalNodes = 1000;
        public const int Generations = 20;
        public const int StepsPerGeneration = 150;
        public const double SuccessThreshold = 0.95;
        public const int BoardDimensions = 128;
        public const int GenesPerGenome = 12;
        public const int InnerNeurons = 6;
        public const double MutationChance = 0.001;
        public const bool RequireExactGenesPerGenome = true;

        public static bool WinThresholdExceeded = false;

        public static bool RenderFrame =>
            true
                &&  (CurrentGeneration == 1
                    || WinThresholdExceeded
                    || CurrentGeneration == Generations);

        public static int CurrentGeneration = 0;
        public static int CurrentStep = 0;

        public static double ActivationFunction(double value)
        {
            return Math.Tanh(value);
        }

        public static bool WeightToBool(double weight)
        {
            return RNG.Double() < weight;
        }

        public static bool InBreedingGrounds(int x, int y)
        {
            return x < 10 || x > BoardDimensions - 10 || y < 10 || y > BoardDimensions - 10;
        }
    }
}