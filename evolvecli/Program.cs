using evolve;
using System;

namespace evolvecli
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            World world = new World(Simulation.BoardDimensions);

            // middle vert
            //world.AddWall(Simulation.BoardDimensions / 2 - 1, 10, 3, Simulation.BoardDimensions - 20);
            
            // middle horz
            //world.AddWall(10, Simulation.BoardDimensions / 2 - 1, Simulation.BoardDimensions - 20, 3);


            //world.AddWall(Simulation.BoardDimensions / 3, Simulation.BoardDimensions / 3 + 5, 1, Simulation.BoardDimensions / 3 - 10);
            //world.AddWall((Simulation.BoardDimensions / 3) * 2, Simulation.BoardDimensions / 3 + 5, 1, Simulation.BoardDimensions / 3 - 10);

            
            
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