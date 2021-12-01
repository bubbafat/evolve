using System;
using System.Collections.Generic;

namespace evolve
{
    public class World
    {
        private readonly Node?[,] _grid;
        private readonly List<Node> _nodes = new List<Node>();
        private readonly int _dimensions;

        public World(int dimensions)
        {
            _dimensions = dimensions;
            _grid = new Node[_dimensions, _dimensions];
        }
        
        public IEnumerable<Node> Nodes
        {
            get => _nodes;
        }
        
        public int Dimension
        {
            get => _dimensions;
        }

        private bool hasNode(int x, int y)
        {
            return _grid[x, y] != null;
        }

        private bool hasNode(Location loc)
        {
            return hasNode(loc.X, loc.Y);
        }

        private bool onBoard(int x, int y)
        {
            return x >= 0 
                   && x < _dimensions 
                   && y >= 0 
                   && y < _dimensions;
        }

        private bool onBoard(Location loc)
        {
            return onBoard(loc.X, loc.Y);
        }

        public bool TryGetAt(Location loc, out Node? node)
        {
            if (onBoard(loc) && hasNode(loc))
            {
                node = _grid[loc.X, loc.Y];
                return true;
            }

            node = null;
            return false;
        }

        public Location RandomEmptyLocation()
        {
            while(true)
            {
                int x = RNG.Int() % _dimensions;
                int y = RNG.Int() % _dimensions;

                if (!hasNode(x, y))
                {
                    return new Location(x, y);
                }
            }
        }

        public void AddAtRandom(Node node)
        {
            node.Location = RandomEmptyLocation();
            _grid[node.Location.X, node.Location.Y] = node;
            _nodes.Add(node);
        }

        public void RemoveNode(Node node)
        {
            _grid[node.Location.X, node.Location.Y] = null;
            _nodes.RemoveAll(n => n.Id == node.Id);
        }

        public float ReadSensor(SensorType type, Location location)
        {
            return type switch
            {
                SensorType.Random => RNG.Float(),
                _ => throw new NotSupportedException("Invalid SensorType")
            };
        }
        
    }
}