using System;

namespace evolve
{
    [Flags]
    public enum SensorType
    {
        DistanceFromNorth = (1 << 1),
        DistanceFromSouth = (1 << 2),
        DistanceFromEast  = (1 << 3),
        DistanceFromWest  = (1 << 4),
    }
    
    public class Sensor : ISensor
    {
        private readonly SensorType _type;

        public Sensor(SensorType type)
        {
            _type = type;
        }
        

        private float readWeight(Node node)
        {
            return node.World.ReadSensor(_type, node.Location);
        }
        
        public float Activate(Node node)
        {
            return Simulation.ActivationFunction(readWeight(node));
        }

        public SensorType Type => _type;

        public int Fingerprint()
        {
            return (int)_type;
        }

        public string Description()
        {
            return $"Sensor ({_type})";
        }
    }
}