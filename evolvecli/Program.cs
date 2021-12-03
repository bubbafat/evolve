using System;
using System.IO;
using evolve;

namespace evolvecli
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            World world = new World(Simulation.BoardDimensions);

            int wallX = Simulation.BoardDimensions / 2 - 1;
            world.AddWall(Simulation.BoardDimensions / 2 - 1, 10, 3, Simulation.BoardDimensions - 20);
            
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