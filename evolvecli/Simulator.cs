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
        private readonly List<long> _elapsed = new List<long>();

        public Simulator(World world, Board board)
        {
            _world = world;
            _board = board;
        }
        
        private void endGeneration()
        {
            Console.WriteLine($"Step times: {StepTimer.ElapsedMilliseconds}");
            _elapsed.Add(StepTimer.ElapsedMilliseconds);
            
            StepTimer.Reset();
        }

        public void Run()
        {
            int highWaterMark = 0;
            
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

                _world.RemoveIf(n => !Simulation.InBreedingGrounds(n.Location.X, n.Location.Y));
                
                if (Simulation.WinThresholdExceeded)
                {
                    break;
                }

                var survivors = _world.Nodes.ToList();
                int survived = survivors.Count;
                if (highWaterMark < survived || Simulation.RenderFrame)
                {
                    highWaterMark = survived;

                    var common = survivors
                        .GroupBy(n => n.Fingerprint())
                        .OrderByDescending(g => g.Count())
                        .Take(2);

                    int rank = 1;
                    foreach (var c in common)
                    {
                        Console.WriteLine("-----");
                        Console.WriteLine($"{rank++}: {c.Count()}/{survivors.Count}");
                        Console.WriteLine(c.First().Description());
                    }

                }

                double survivalRatio = survived / (double) Simulation.TotalNodes;
                Simulation.WinThresholdExceeded = survivalRatio >= Simulation.SuccessThreshold;

                Console.WriteLine($"{survived} survived ({survivalRatio}%)");
                
                var children = reproduce(survivors);

                _world.Clear();
                _world.AddAtRandom(children);
                
                endGeneration();
            }

            _elapsed.Sort();
            Console.WriteLine($"Generation average: {(int)_elapsed.Average()}, Median: {_elapsed[_elapsed.Count / 2]}, Min: {_elapsed.First()}, Max: {_elapsed.Last()}");
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
            foreach(var node in _world.Nodes) node.Reset();

            _world.Nodes.AsParallel().ForAll(n => n.Evaluate());
            _world.Nodes.AsParallel().ForAll(n => n.Execute());
            
            _world.EndStep();
        }
    }
}