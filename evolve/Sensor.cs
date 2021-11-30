namespace evolve
{
    public enum SensorType
    {
        Random
    }
    
    public class Sensor
    {
        private readonly SensorType _type;
        private readonly Node _node;

        public Sensor(SensorType type, Node node)
        {
            _type = type;
            _node = node;
        }

        private float readWeight()
        {
            return _node.World.ReadSensor(_type, _node.Location);
        }
        
        public float Activate()
        {
            return Simulation.ActivationFunction(readWeight());
        }
    }
}