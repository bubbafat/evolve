using System;

namespace evolve
{
    public class World
    {
        private readonly Node[,] _grid;

        public World(int dimensions)
        {
            _grid = new Node[dimensions, dimensions];
        }

        private bool hasNode(Location loc)
        {
            throw new NotImplementedException();
        }

        private bool onBoard(Location loc)
        {
            throw new NotImplementedException();
        }

        public bool TryGetAt(Location loc, out Node node)
        {
            throw new NotImplementedException();
        }
        
        public float ReadSensor(SensorType type, Location location)
        {
            return type switch
            {
                SensorType.Random => RNG.Float()
            };
        }
        
    }
}