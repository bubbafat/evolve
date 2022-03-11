using evolve;
using System;

namespace evolvecli
{
    internal class Program
    {
        private static string arg(string[] args, string arg, string defaultValue)
        {
            foreach (string s in args)
            {
                if (s.StartsWith(arg))
                {
                    var v = s.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    return v[1];
                }
            }

            return defaultValue;
        }

        private static int arg(string[] args, string arg, int defaultValue)
        {
            foreach(string s in args)
            {
                if(s.StartsWith(arg))
                {
                    var v = s.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    return int.Parse(v[1]);
                }
            }

            return defaultValue;
        }

        private static bool arg(string[] args, string arg, bool defaultValue)
        {
            foreach (string s in args)
            {
                if (s.StartsWith(arg))
                {
                    var v = s.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    return bool.Parse(v[1]);
                }
            }

            return defaultValue;
        }

        public static void Main(string[] args)
        {
            Simulation.TotalNodes = arg(args, "nodes", Simulation.TotalNodes);
            Simulation.Generations = arg(args, "gens", Simulation.Generations);
            Simulation.GenesPerGenome = arg(args, "genes", Simulation.GenesPerGenome);
            Simulation.InnerNeurons = arg(args, "inner", Simulation.InnerNeurons);
            Simulation.AllowKillers = arg(args, "killers", Simulation.AllowKillers);
            Simulation.AllowBullies = arg(args, "bullies", Simulation.AllowBullies);
            Simulation.AllowDefense = arg(args, "defenders", Simulation.AllowDefense);
            Simulation.RenderImages = arg(args, "render", Simulation.RenderImages);
            Simulation.BreedingGroundId = arg(args, "grounds", Simulation.BreedingGroundId);
            var walls = arg(args, "walls", string.Empty);

            World world = new World();

            switch(walls)
            {
                case "cross":
                    world.AddWall(Simulation.BoardDimensions / 2 - 1, 10, 3, Simulation.BoardDimensions - 20);
                    world.AddWall(10, Simulation.BoardDimensions / 2 - 1, Simulation.BoardDimensions - 20, 3);
                    break;

                case "hard":
                    world.AddWall(Simulation.BoardDimensions / 3, Simulation.BoardDimensions / 3 + 5, 1, Simulation.BoardDimensions / 3 - 10);
                    world.AddWall((Simulation.BoardDimensions / 3) * 2, Simulation.BoardDimensions / 3 + 5, 1, Simulation.BoardDimensions / 3 - 10);
                    break;
            }

            for (int i = 0; i < Simulation.TotalNodes; i++)
            {
                Node n = new Node(world, Genome.CreateRandom(Simulation.GenesPerGenome));
                world.AddAtRandom(n);
            }

            using (Board board = new Board(world))
            {
                Simulator sim = new Simulator(world, board);
                sim.Run();
            }
        }
    }
}