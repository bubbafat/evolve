using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace evolve
{
    public static class Network
    {
        public static readonly NetworkBuilder Builder = new NetworkBuilder();
    }

    public class NetworkBuilder
    {
        private static readonly SensorType[] _sensorTypes = new[]
        {
            SensorType.DistanceFromNorth,
            SensorType.DistanceFromSouth,
            SensorType.DistanceFromEast,
            SensorType.DistanceFromWest,
            SensorType.LocalPopulation,
            SensorType.TimeSinceLastMove,
        };

        private static readonly ActionType[] _actionTypes = new[]
        {
            ActionType.StayPut,
            ActionType.MoveNorth,
            ActionType.MoveSouth,
            ActionType.MoveEast,
            ActionType.MoveWest,
            ActionType.MoveRandom,
        };

        private readonly Dictionary<int, IInnerNeuron> _inners = new Dictionary<int, IInnerNeuron>();
        private readonly Dictionary<SensorType, ISensor> _sensors = new Dictionary<SensorType, ISensor>();
        private readonly Dictionary<ActionType, IAction> _actions = new Dictionary<ActionType, IAction>();

        private void Reset()
        {
            _inners.Clear();
            _sensors.Clear();
            _actions.Clear();
        }

        IInnerNeuron randomInner(Func<float> weight)
        {
            int innerIndex = RNG.Int() % Simulation.InnerNeurons;
            if (!_inners.TryGetValue(innerIndex, out IInnerNeuron neuron))
            {
                neuron = new InnerNeuron(weight());
                _inners.Add(innerIndex, neuron);
            }

            return neuron;
        }

        IActivatable randomSource(Func<SensorType> sensorType, Func<float> weight)
        {
            bool isSensor = RNG.Bool();
            if (isSensor)
            {
                var type = sensorType();
                return cachedSensor(type);
            }

            return randomInner(weight);
        }

        SensorType randomSensorType()
        {
            return _sensorTypes[RNG.Int(_sensorTypes.Length)];
        }

        ActionType randomActionType()
        {
            return _actionTypes[RNG.Int(_actionTypes.Length)];
        }

        ISink randomSink(Func<ActionType> actionType, Func<float> weight)
        {
            bool isAction = RNG.Bool();
            if (isAction)
            {
                var type = actionType();
                return cachedAction(type, weight);
            }

            return randomInner(weight);
        }

        ISensor cachedSensor(SensorType type)
        {
            if (!_sensors.TryGetValue(type, out ISensor action))
            {
                action = new Sensor(type);
                _sensors.Add(type, action);
            }

            return action;
        }

        IAction cachedAction(ActionType type, Func<float> weight)
        {
            if (!_actions.TryGetValue(type, out IAction action))
            {
                action = new Action(type, weight());
                _actions.Add(type, action);
            }

            return action;
        }

        public List<Gene> CreateFromExisting(int connections, IList<Gene> existing)
        {
            Reset();
            var mix = existing.Shuffle().Take(connections).Select(g => g.DeepCopy()).ToList();


            // we now have potentially too many inner neurons and they are wired up between two sets
            // of genes. For each IN, if we haven't seen it before AND we are not at max inner neurons,
            // add it to the neuron cache and let it be used.  If we have seen it before, use it.  If
            // we have not seen it but we are at the limit, pick a random cached one.
            // Now each neuron in the network should be in-use and linked.
            List<InnerNeuron> neurons = new List<InnerNeuron>();
            foreach (Gene g in mix)
            {
                var source = g.Source as InnerNeuron;
                if (source != null)
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

                var sink = g.Sink as InnerNeuron;
                if (sink != null)
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


            optimizeNeurons(mix);

            return mix;
        }

        private static List<Gene> optimizeNeurons(List<Gene> mix)
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
                int before = inUse.Count;
                foreach (Gene g in mix)
                {
                    // if the sink contributes to an action, then add the source
                    if (inUse.Contains(g.Sink.Id))
                    {
                        inUse.Add(g.Source.Id);
                    }
                }

                // we didn't find anything
                if (before == inUse.Count)
                {
                    break;
                }
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

        public List<Gene> CreateRandom(int connections)
        {
            // clear caches from previous runs
            Reset();

            List<Gene> genes = new List<Gene>(connections);

            for (int i = 0; i < connections; i++)
            {
                genes.Add(new Gene(randomSource(randomSensorType, RNG.Float), randomSink(randomActionType, RNG.Float)));
            }

            return optimizeNeurons(genes);
        }
    }
}