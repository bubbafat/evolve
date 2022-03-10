using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using evolve;

namespace evolvecli
{
    public class Simulator
    {
        private readonly World _world;
        private readonly Board _board;

        public Simulator(World world, Board board)
        {
            _world = world;
            _board = board;
        }
        
        private void endGeneration()
        {
            Console.WriteLine($"Step times: {StepTimer.ElapsedMilliseconds}");
            
            StepTimer.Reset();
        }

        public void Run()
        {
            int highWaterMark = 0;

            LinkedList<double> runningSurvivorRatio = new LinkedList<double>();
            
            for (Simulation.CurrentGeneration = 1;
                Simulation.CurrentGeneration <= Simulation.Generations;
                Simulation.CurrentGeneration++)
            {
                Console.WriteLine();
                Console.WriteLine($"Generation {Simulation.CurrentGeneration}");
                
                _world.BeginStep();


                StepTimer.Start();
                for (Simulation.CurrentStep = 1;
                    Simulation.CurrentStep < Simulation.StepsPerGeneration;
                    Simulation.CurrentStep++)
                {
                    renderFrame();
                    
                    Step();
                }
                StepTimer.Stop();

                renderFrame();

                if (Simulation.AllowKillers)
                {
                    int kills = Simulation.TotalNodes - _world.Nodes.Count();
                    Console.WriteLine($"Total Kills: {kills}");
                }

                _world.RemoveIf(n => !Simulation.InBreedingGrounds(n.X, n.Y));
                
                if (Simulation.WinThresholdExceeded)
                {
                    return;
                }

                var survivors = _world.Nodes.ToList();
                int survived = survivors.Count;
                if (highWaterMark < survived || Simulation.RenderFrame)
                {
                    highWaterMark = survived;

                    var groups = survivors
                        .GroupBy(n => n.Fingerprint())
                        .OrderByDescending(g => g.Count());

                    int diversity = groups.Count();
                    
                    int rank = 1;
                    foreach (var c in groups.Take(2))
                    {
                        Console.WriteLine("-----");
                        Console.WriteLine($"{rank++}: {c.Count()}/{survivors.Count}");
                        Console.WriteLine(c.First().Description());
                    }

                    Console.WriteLine($"Genetic diversity: {diversity}");
                }

                double survivalRatio = survived / (double) Simulation.TotalNodes;

                runningSurvivorRatio.AddFirst(survivalRatio * 100);

                double running10 = runningSurvivorRatio.Take(10).Average();
                double running100 = runningSurvivorRatio.Take(100).Average();

                Console.WriteLine($"Running averages (10, 100) {running10}, {running100}");
                
                
                Simulation.WinThresholdExceeded = survivalRatio >= Simulation.SuccessThreshold;

                Console.WriteLine($"{survived} survived ({survivalRatio * 100}%)");
                
                var children = reproduce(survivors);

                _world.Clear();
                _world.AddAtRandom(children);
                
                endGeneration();
            }
        }

        private IEnumerable<Node> reproduce(List<Node> survivers)
        {
            Node[] output = new Node[Simulation.TotalNodes];

            Parallel.For(0, Simulation.TotalNodes,
                (i, state) => { output[i] = survivers.Random().Reproduce(survivers.Random()); });

            return output;
        }

        private void renderFrame()
        {
            if (Simulation.RenderFrame)
            {
                int gen = Simulation.CurrentGeneration;
                int step = Simulation.CurrentStep;

                _board.ExportFrame(new FileInfo($"gen-{gen}{Path.DirectorySeparatorChar}frame-{step}.png"));
                Console.Write(".");
            }
        }

        public static readonly Stopwatch StepTimer = new Stopwatch();

        private void Step()
        {
            _world.Nodes.AsParallel().ForAll(n => n.Reset());
            _world.Nodes.AsParallel().ForAll(n => n.Evaluate());
            _world.Nodes.AsParallel().ForAll(n => n.Execute());
            
            _world.EndStep();
        }
    }
}