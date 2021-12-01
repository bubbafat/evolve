using System;

namespace evolve
{
    public enum SensorIds
    {
        Random
    }

    public struct SensorTypes
    {
        public readonly SensorIds Type;
        public readonly Guid Id;

        public SensorTypes(SensorIds type)
        {
            Type = type;
            Id = Guid.NewGuid();
        }
    }

    public static class Sensors
    {
        public static SensorTypes Random = new SensorTypes(SensorIds.Random);
    }
    
    
    public class Sensor : ISensor
    {
        private readonly SensorTypes _type;

        public Sensor(SensorTypes type)
        {
            _type = type;
        }

        private float readWeight(Node node)
        {
            return node.World.ReadSensor(_type.Type, node.Location);
        }
        
        public float Activate(Node node)
        {
            return Simulation.ActivationFunction(readWeight(node));
        }

        public override int GetHashCode()
        {
            return _type.Id.GetHashCode();
        }
    }
}