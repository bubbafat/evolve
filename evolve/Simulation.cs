using System;

namespace evolve
{
    public static class Simulation
    {
        public static int TotalNodes = 1000;
        public static int Generations = 1000;
        public const int StepsPerGeneration = FPS * 10;
        public static double SuccessThreshold = 0.95;
        public static int BoardDimensions = 128;
        public static int GenesPerGenome = 6;
        public static int InnerNeurons = 3;
        public static readonly double MutationChance = 1d / TotalNodes; 
        public static bool AllowKillers = false;
        public static bool AllowBullies = true;
        public static bool AllowDefense = false;
        public static int BreedingGroundId = 1;

        public static int RenderOnMultiple = Generations / 10;
        private const int FPS = 30;

        public static bool WinThresholdExceeded = false;

        public static bool RenderImages = false;

        public static bool RenderFrame =>
            RenderImages
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
            return BreedingGroundId switch
            {
                1 => x < 15, // left side
                2 => x > BoardDimensions - 15, // right
                3 => y < 15, // top
                4 => y > BoardDimensions -15, // bottom
                5 => x < 15 && y > 15 && y < BoardDimensions - 15, // part of left
                6 => x > BoardDimensions / 3 && x < (BoardDimensions / 3) * 2 // middle
                         && y > BoardDimensions / 3 && y < (BoardDimensions / 3) * 2,
                _ => x < 15 // default to left side
            };
        }
    }
}