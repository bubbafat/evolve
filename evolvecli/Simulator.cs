using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using evolve;

namespace evolvecli
{
    public class Simulator
    {
        private readonly World _world;
        private readonly Board _board;
        private bool _thresholdExceeded = false;
        
        public Simulator(World world, Board board)
        {
            _world = world;
            _board = board;
        }

        private bool inBreedingGrounds(Location loc)
        {
            return (loc.X < 10);
        }

        public void Run()
        {
            int highWaterMark = 0;
            
            for (Simulation.CurrentGeneration = 1;
                Simulation.CurrentGeneration <= Simulation.Generations;
                Simulation.CurrentGeneration++)
            {
                Console.Write($"Generation {Simulation.CurrentGeneration} ");

                bool render = Simulation.CurrentGeneration == 1
                              || Simulation.CurrentGeneration == Simulation.Generations
                              || _thresholdExceeded;

                if (render)
                {
                    Console.Write("(rendering) ");
                }

                for (Simulation.CurrentStep = 1;
                    Simulation.CurrentStep < Simulation.StepsPerGeneration;
                    Simulation.CurrentStep++)
                {
                    step(render);
                }
                
                _world.RemoveIf(n => !inBreedingGrounds(n.Location));

                if (render)
                {
                    renderFrame(Simulation.CurrentGeneration, Simulation.CurrentStep);
                }

                if (_thresholdExceeded)
                {
                    return;
                }

                var survivors = _world.Nodes.ToList();
                int survived = survivors.Count;
                if (highWaterMark < survived || render)
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
                _thresholdExceeded = survivalRatio >= Simulation.SuccessThreshold;

                Console.WriteLine($"{survived} survived ({survivalRatio}%)");
                
                var children = reproduce(survivors);
                _world.Clear();
                _world.AddAtRandom(children);
            }
        }

        private IEnumerable<Node> reproduce(List<Node> survivers)
        {
            for (int i = 0; i < Simulation.TotalNodes; i++)
            {
                yield return survivers.Random().Reproduce(survivers.Random());
            }
        }

        private void renderFrame(int gen, int step)
        {
            _board.ExportFrame(new FileInfo($"gen-{gen}{Path.DirectorySeparatorChar}frame-{step}.png"));
            Console.Write(".");
        }

        private void step(bool render)
        {
            foreach (var node in _world.Nodes)
            {
                node.Reset();
            }

            foreach (var node in _world.Nodes)
            {
                node.Evaluate();
            }
            
            foreach (var node in _world.Nodes)
            {
                node.Execute();
            }
            
            if (render)
            {
                renderFrame(Simulation.CurrentGeneration, Simulation.CurrentStep);
            }
        }
    }
}