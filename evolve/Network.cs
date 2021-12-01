using System;
using System.Collections.Generic;

namespace evolve
{
    public static class Network
    {
        public static readonly NetworkBuilder Builder = new NetworkBuilder();
    }
    public class NetworkBuilder
    {
        private static SensorTypes[] _sensorTypes = new[]
        {
            Sensors.Random
        };

        private static ActionTypes[] _actionTypes = new[]
        {
            Actions.MoveRandom
        };

        private Dictionary<int, IInnerNeuron> _inners = new Dictionary<int, IInnerNeuron>();
        private Dictionary<SensorIds, ISensor> _sensors = new Dictionary<SensorIds, ISensor>();
        private Dictionary<ActionIds, IAction> _actions = new Dictionary<ActionIds, IAction>();

        private void Reset()
        {
            _inners.Clear();
            _sensors.Clear();
            _actions.Clear();
        }

        IInnerNeuron randomInner()
        {
            int innerIndex = RNG.Int() % Simulation.InnerNeurons;
            if (!_inners.TryGetValue(innerIndex, out IInnerNeuron neuron))
            {
                neuron = new InnerNeuron();
                _inners.Add(innerIndex, neuron);
            }

            return neuron;
        }

        IActivatable randomSource()
        {
            bool isSensor = RNG.Bool();
            if (isSensor)
            {
                int sensorIndex = RNG.Int() % _sensorTypes.Length;
                var type = _sensorTypes[sensorIndex];

                if (!_sensors.TryGetValue(type.Type, out ISensor sensor))
                {
                    sensor = new Sensor(type);
                    _sensors.Add(type.Type, sensor);
                }

                return sensor;
            }

            return randomInner();
        }
        
        ISink randomSink()
        {
            bool isAction = RNG.Bool();
            if (isAction)
            {
                int actionIndex = RNG.Int() % _actionTypes.Length;
                var type = _actionTypes[actionIndex];

                if (!_actions.TryGetValue(type.Type, out IAction action))
                {
                    action = new Action(type);
                    _actions.Add(type.Type, action);
                }

                return action;
            }

            return randomInner();
        }

        public List<Gene> Create(int connections)
        {
            List<Gene> genes = new List<Gene>(connections);

            for (int i = 0; i < connections; i++)
            {
                genes.Add(new Gene(randomSource(), randomSink()));
            }

            genes.Sort();
            
            Reset();

            return genes;
        }
    }
}