using System;

namespace evolve
{
    [Flags]
    public enum SensorType
    {
        DistanceFromNorth = 1 << 1,
        DistanceFromSouth = 1 << 2,
        DistanceFromEast = 1 << 3,
        DistanceFromWest = 1 << 4,
        LocalPopulation = 1 << 5,
        TimeSinceLastMove = 1 << 6,
    }

    public class Sensor : IActivatable
    {
        public Sensor(SensorType type)
        {
            Type = type;
            Id = Guid.NewGuid();
        }

        public IActivatable DeepCopy()
        {
            return new Sensor(Type);
        }

        public Guid Id { get; }


        private float ReadWeight(Node node)
        {
            return node.World.ReadSensor(Type, node);
        }

        public float Activate(Node node)
        {
            return Simulation.ActivationFunction(ReadWeight(node));
        }

        private SensorType Type { get; }

        public int Fingerprint()
        {
            return (int) Type;
        }

        public string Description()
        {
            return $"Sensor ({Type})";
        }
    }
}