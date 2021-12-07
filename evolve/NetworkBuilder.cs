using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace evolve
{
    public class NetworkBuilder
    {
        public static readonly IList<SensorType> SensorTypes = new List<SensorType>{
            SensorType.DistanceFromNorth,
            SensorType.DistanceFromSouth,
            SensorType.DistanceFromEast,
            SensorType.DistanceFromWest,
            SensorType.DistanceFromCenter,
            SensorType.LocalPopulation,
            SensorType.TimeSinceLastMove,
            SensorType.Blocked,
        };

        public static readonly IList<ActionType> ActionTypes = new List<ActionType> {
            ActionType.StayPut,
            ActionType.MoveNorth,
            ActionType.MoveSouth,
            ActionType.MoveEast,
            ActionType.MoveWest,
            ActionType.MoveRandom,
            ActionType.MoveToCenterX,
            ActionType.MoveToCenterY,
            ActionType.Bully,
            ActionType.Kill,
            ActionType.Defend,
        };

        static NetworkBuilder()
        {
            if (!Simulation.AllowKillers) ActionTypes.Remove(ActionType.Kill);
            if (!Simulation.AllowBullies) ActionTypes.Remove(ActionType.Bully);
            if (!Simulation.AllowDefense) ActionTypes.Remove(ActionType.Defend);
        }

        // we use these millions of times - so keep an MRU cache so we can minimize allocations
        private static readonly ConcurrentStack<NetworkBuilder> _builders = new ConcurrentStack<NetworkBuilder>();
        
        // each time a network is built, these are used to ensure that the network does not have
        // duplicate genes - if the network has the same type of sensor used 3 times, then there
        // should still only be one sensor instance for that type.  These are reset between genes.
        private readonly Dictionary<int, InnerNeuron> _inners = new Dictionary<int, InnerNeuron>();
        private readonly Dictionary<SensorType, Sensor> _sensors = new Dictionary<SensorType, Sensor>();
        private readonly Dictionary<ActionType, Action> _actions = new Dictionary<ActionType, Action>();

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

        private void buildActionSensorTypeCaches(List<Gene> genes)
        {
            // remove duplicate sensor and action instances - use the first one we found
            foreach (var g in genes)
            {
                if (g.Source is Sensor sensor)
                {
                    if (_sensors.ContainsKey(sensor.Type))
                    {
                        g.Source = _sensors[sensor.Type];
                    }
                    else
                    {
                        _sensors.Add(sensor.Type, sensor);
                    }
                }

                if (g.Sink is Action action)
                {
                    if (_actions.ContainsKey(action.Type))
                    {
                        g.Sink = _actions[action.Type];
                    }
                    else
                    {
                        _actions.Add(action.Type, action);
                    }
                }
            }
        }

        private void reduceInnerNeurons(List<Gene> genes)
        {
            // we now have potentially too many inner neurons and they are wired up between two sets
            // of genes. For each IN, if we haven't seen it before AND we are not at max inner neurons,
            // add it to the neuron cache and let it be used.  If we have seen it before, use it.  If
            // we have not seen it but we are at the limit, pick a random cached one.
            // Now each neuron in the network should be in-use and linked.
            List<InnerNeuron> neurons = new List<InnerNeuron>();
            foreach (Gene g in genes)
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
        }

        private IEnumerable<Gene> createFromExisting(List<Gene> existing)
        {
            var mix = removeDuplicatePairs(existing)
                .Shuffle()
                .Take(Simulation.GenesPerGenome)
                .Select(g => g.DeepCopy()).ToList();

            buildActionSensorTypeCaches(mix);
            
            reduceInnerNeurons(mix);

            return OptimizeNeurons(mix);
        }

        private static List<Gene> removeDuplicatePairs(List<Gene> mix)
        {
            // we'll now remove duplicates
            var duplicates = mix.GroupBy(g => (g.Source.Id, g.Sink.Id))
                .Where(grp => grp.Count() > 1)
                .ToList();

            foreach (var dups in duplicates)
            {
                var toRemove = dups.Skip(1).Select(g => g.Id);
                mix.RemoveAll(g => toRemove.Contains(g.Id));
            }

            return mix;
        }

        private static List<Gene> pruneNoncontributingPairs(List<Gene> mix)
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

            return mix;
        }

        private static List<Gene> OptimizeNeurons(List<Gene> mix)
        {
            mix = pruneNoncontributingPairs(mix);
            mix = removeDuplicatePairs(mix);
            

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

        private static NetworkBuilder getNetworkBuilder()
        {
            NetworkBuilder nb;
            if (!_builders.TryPop(out nb))
            {
                nb = new NetworkBuilder();
            }

            nb.Reset();

            return nb;
        }

        private static void returnNetworkBuilder(NetworkBuilder nb)
        {
            _builders.Push(nb);
        }

        private IEnumerable<Gene> createRandom(int connections)
        {
            List<Gene> genes = new List<Gene>(connections);
            for (int i = 0; i < connections; i++)
            {
                genes.Add(CreateRandomGene());
            }

            return OptimizeNeurons(genes);
        }

        public static IEnumerable<Gene> CreateFromExisting(List<Gene> existing)
        {
            var nb = getNetworkBuilder();

            var result = nb.createFromExisting(existing);

            returnNetworkBuilder(nb);
            
            return result;
        }

        public static IEnumerable<Gene> CreateRandom(int connections)
        {
            var nb = getNetworkBuilder();

            var result = nb.createRandom(connections);
            
            returnNetworkBuilder(nb);
            
            return result;
        }
    }
}