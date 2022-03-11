using System;

namespace evolve
{
    public static class Simulation
    {
        public const int TotalNodes = 1000;
        public const int Generations = 5000;
        public const int StepsPerGeneration = FPS * 10;
        public const double SuccessThreshold = 0.90;
        public const int BoardDimensions = 128;
        public const int GenesPerGenome = 16;
        public const int InnerNeurons = 5;
        public const double MutationChance = 2.0 / TotalNodes / GenesPerGenome;
        public const bool AllowKillers = false;
        public const bool AllowDefense = false;
        public const bool AllowBullies = true;

        private const int RenderOnMultiple = 500;
        private const int FPS = 30;

        public static bool WinThresholdExceeded = false;

        public static bool RenderFrame =>
            true
                &&  (CurrentGeneration == 1
                    || WinThresholdExceeded
                    || CurrentGeneration == Generations
                    || CurrentGeneration % RenderOnMultiple == 0);

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
            // right side
            return x > BoardDimensions - 15;
            
            // left side but not the corners
            // return x < 15 && y > 15 && y < BoardDimensions - 15;

            // the middle
            return x > BoardDimensions / 3 && x < (BoardDimensions / 3) * 2
                                           && y > BoardDimensions / 3 && y < (BoardDimensions / 3) * 2;
        }
    }
}