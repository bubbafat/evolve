using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace evolve
{
    public class NetworkBuilder
    {
        private static readonly SensorType[] SensorTypes = {
            SensorType.DistanceFromNorth,
            SensorType.DistanceFromSouth,
            SensorType.DistanceFromEast,
            SensorType.DistanceFromWest,
            SensorType.DistanceFromCenter,
            SensorType.LocalPopulation,
            SensorType.TimeSinceLastMove,
        };

        private static readonly ActionType[] ActionTypes = {
            ActionType.StayPut,
            ActionType.MoveNorth,
            ActionType.MoveSouth,
            ActionType.MoveEast,
            ActionType.MoveWest,
            ActionType.MoveRandom,
            ActionType.MoveToCenter,
        };

        // we create millions of these - so keep an MRU cache so we can minimize allocations
        private static readonly ConcurrentStack<NetworkBuilder> _builders = new ConcurrentStack<NetworkBuilder>();
        private readonly Dictionary<ActionType, Action> _actions = new Dictionary<ActionType, Action>();


        // each time a network is built, these are used to ensure that the network does not have
        // duplicate genes - if the network has the same type of sensor used 3 times, then there
        // should still only be one sensor instance for that type.  These are reset between genes.
        private readonly Dictionary<int, InnerNeuron> _inners = new Dictionary<int, InnerNeuron>();
        private readonly Dictionary<SensorType, Sensor> _sensors = new Dictionary<SensorType, Sensor>();

        private void Reset()
        {
            _inners.Clear();
            _sensors.Clear();
            _actions.Clear();
        }

        private InnerNeuron RandomInner()
        {
            int innerIndex = RNG.Int() % Simulation.InnerNeurons;
            if (!_inners.TryGetValue(innerIndex, out InnerNeuron neuron))
            {
                neuron = new InnerNeuron(RNG.Double());
                _inners.Add(innerIndex, neuron);
            }

            return neuron;
        }

        private IActivatable RandomSource()
        {
            bool createSensor = RNG.Bool();
            if (createSensor)
            {
                var type = SensorTypes.Random();
                return CachedSensor(type);
            }

            return RandomInner();
        }

        private ISink RandomSink()
        {
            bool createAction = RNG.Bool();
            if (createAction)
            {
                var type = ActionTypes.Random();
                return CachedAction(type);
            }

            return RandomInner();
        }

        private Sensor CachedSensor(SensorType type)
        {
            if (!_sensors.TryGetValue(type, out Sensor action))
            {
                action = new Sensor(type);
                _sensors.Add(type, action);
            }

            return action;
        }

        private Action CachedAction(ActionType type)
        {
            if (!_actions.TryGetValue(type, out Action action))
            {
                action = new Action(type);
                _actions.Add(type, action);
            }

            return action;
        }

        private IEnumerable<Gene> createFromExisting(IList<Gene> existing)
        {
            Reset();
            
            var mix = existing.Shuffle()
                .Take(Simulation.GenesPerGenome)
                .Select(g => g.DeepCopy()).ToList();

            if (Simulation.RequireExactGenesPerGenome)
            {
                while (mix.Count < Simulation.GenesPerGenome)
                {
                    mix.Add(CreateRandomGene());
                }
            }

            // we now have potentially too many inner neurons and they are wired up between two sets
            // of genes. For each IN, if we haven't seen it before AND we are not at max inner neurons,
            // add it to the neuron cache and let it be used.  If we have seen it before, use it.  If
            // we have not seen it but we are at the limit, pick a random cached one.
            // Now each neuron in the network should be in-use and linked.
            List<InnerNeuron> neurons = new List<InnerNeuron>();
            foreach (Gene g in mix)
            {
                if (g.Source is InnerNeuron source)
                {
                    if (neurons.All(n => n.Id != source.Id))
                    {
                        if (neurons.Count < Simulation.InnerNeurons)
                        {
                            neurons.Add(source);
                        }
                        else
                        {
                            g.Source = neurons.Random();
                        }
                    }
                }

                if (g.Sink is InnerNeuron sink)
                {
                    if (neurons.All(n => n.Id != sink.Id))
                    {
                        if (neurons.Count < Simulation.InnerNeurons)
                        {
                            neurons.Add(sink);
                        }
                        else
                        {
                            g.Sink = neurons.Random();
                        }
                    }
                }
            }


            OptimizeNeurons(mix);

            return mix;
        }

        private static List<Gene> OptimizeNeurons(List<Gene> mix)
        {
            var inUse = new HashSet<Guid>();

            // action sinks and their sources are active
            foreach (var g in mix)
            {
                if (g.Sink is Action)
                {
                    inUse.Add(g.Sink.Id);
                    inUse.Add(g.Source.Id);
                }
            }

            // now find the nodes that contribute to the actions

            while (true)
            {
                bool foundMore = inUse.AddRange(
                    mix.Where(g => inUse.Contains(g.Sink.Id))
                        .Select(g => g.Source.Id));

                if (!foundMore) break;
            }

            // remove the genes that don't contribute to actions
            mix.RemoveAll(g => !inUse.Contains(g.Sink.Id));

            // sort so that it is
            // sensor -> *
            // inner -> inner (same)
            // inner -> inner (different)
            // inner -> action
            // this allows us to evaluate the genes in order and still have the correct ordering of actions
            mix.Sort();

            return mix;
        }

        private Gene CreateRandomGene()
        {
            return new Gene(RandomSource(), RandomSink());
        }

        private IEnumerable<Gene> createRandom(int connections)
        {
            // clear caches from previous runs
            Reset();

            List<Gene> genes = new List<Gene>(connections);

            for (int i = 0; i < connections; i++)
            {
                genes.Add(CreateRandomGene());
            }

            return OptimizeNeurons(genes);
        }

        public static IEnumerable<Gene> CreateFromExisting(IList<Gene> existing)
        {
            NetworkBuilder nb;
            if (!_builders.TryPop(out nb))
            {
                nb = new NetworkBuilder();
            }

            var result = nb.createFromExisting(existing);

            nb.Reset();
            _builders.Push(nb);

            return result;
        }

        public static IEnumerable<Gene> CreateRandom(int connections)
        {
            if (!_builders.TryPop(out NetworkBuilder nb))
            {
                nb = new NetworkBuilder();
            }

            var result = nb.createRandom(connections);

            nb.Reset();
            _builders.Push(nb);

            return result;
        }
    }
}