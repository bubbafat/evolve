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

        private bool inBreedingGrounds(int x, int y)
        {
            return (x < 10);
        }

        private void endGeneration()
        {
            Console.WriteLine();
            Console.WriteLine($"Eval times: {EvalTimer.ElapsedMilliseconds}");
            Console.WriteLine($"Exec times: {ExecTimer.ElapsedMilliseconds}");
            Console.WriteLine($"Step times: {StepTimer.ElapsedMilliseconds}");
            Console.WriteLine($"Repro times: {ReproTimer.ElapsedMilliseconds}");
            Console.WriteLine($"Child times: {AddChildrenTimer.ElapsedMilliseconds}");
            
            EvalTimer.Reset();
            ExecTimer.Reset();
            ReproTimer.Reset();
            StepTimer.Reset();
            AddChildrenTimer.Reset();
        }

        public void Run()
        {
            int highWaterMark = 0;
            
            for (Simulation.CurrentGeneration = 1;
                Simulation.CurrentGeneration <= Simulation.Generations;
                Simulation.CurrentGeneration++)
            {
                Console.WriteLine($"Generation {Simulation.CurrentGeneration}");
                
                _world.BeginStep();


                StepTimer.Start();
                for (Simulation.CurrentStep = 1;
                    Simulation.CurrentStep < Simulation.StepsPerGeneration;
                    Simulation.CurrentStep++)
                {
                    renderFrame();
                    
                    step();
                    
                    _world.EndStep();
                }
                StepTimer.Stop();

                renderFrame();

                
                EvalTimer.Start();
                
                _world.RemoveIf(n => !inBreedingGrounds(n.X, n.Y));
                
                if (Simulation.WinThresholdExceeded)
                {
                    return;
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

                double survivalRatio = (float) survived / (float) Simulation.TotalNodes;
                Simulation.WinThresholdExceeded = survivalRatio >= Simulation.SuccessThreshold;

                Console.WriteLine($"{survived} survived ({survivalRatio}%)");
                
                var children = reproduce(survivors);

                _world.Clear();
                _world.AddAtRandom(children);

                EvalTimer.Stop();
                
                endGeneration();
            }
        }

        private IEnumerable<Node> reproduce(List<Node> survivers)
        {
            ReproTimer.Start();
            Node[] output = new Node[Simulation.TotalNodes];

            Parallel.For(0, Simulation.TotalNodes,
                (i, state) => { output[i] = survivers.Random().Reproduce(survivers.Random()); });

            ReproTimer.Stop();
            
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

        public static readonly Stopwatch EvalTimer = new Stopwatch();
        public static readonly Stopwatch ExecTimer = new Stopwatch();
        public static readonly Stopwatch ReproTimer = new Stopwatch();
        public static readonly Stopwatch StepTimer = new Stopwatch();
        public static readonly Stopwatch AddChildrenTimer = new Stopwatch();

        private void step()
        {
            _world.Nodes.AsParallel().ForAll(n => n.Reset());
            
            _world.Nodes.AsParallel().ForAll(n => n.Evaluate());

            foreach (var node in _world.Nodes)
            {
                node.Execute();
            }
        }
    }
}