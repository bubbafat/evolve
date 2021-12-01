using System;
using System.Collections.Generic;
using System.Linq;

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
        
        public Location RandomEmptyNeighbor(Location loc)
        {
            int val = RNG.Int();
            
            for (int i = 0; i < 8; i++)
            {
                var xy = (val % 8) switch
                {
                    0 => (loc.X - 1, loc.Y - 1),
                    1 => (loc.X - 1, loc.Y ),
                    2 => (loc.X - 1, loc.Y + 1),
                    3 => (loc.X - 1, loc.Y),
                    4 => (loc.X + 1, loc.Y),
                    5 => (loc.X + 1, loc.Y - 1),
                    6 => (loc.X + 1, loc.Y),
                    7 => (loc.X + 1, loc.Y + 1),
                    _ => throw new NotSupportedException("BUG!")
                };

                if (onBoard(xy.Item1, xy.Item2) && !hasNode(xy.Item1, xy.Item2))
                {
                    return new Location(xy.Item1, xy.Item2);
                }

                val++;
            }

            return loc;
        }
        
        public void MoveNodeAt(Location from, Location to)
        {
            if (!onBoard(from) || !onBoard(to))
                return;

            if (hasNode(to))
                return;
            
            if (TryGetAt(from, out Node? node))
            {
                if (node == null)
                    throw new NotSupportedException("BUG!!  Node can't be null");
                
                _grid[from.X, from.Y] = null;
                _grid[to.X, to.Y] = node;
                node.Location = to;
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

        public float ReadSensor(SensorIds type, Location location)
        {
            return type switch
            {
                SensorIds.Random => RNG.Float(),
                _ => throw new NotSupportedException("Invalid SensorType")
            };
        }

        public IEnumerable<Node> Partition(Predicate<Node> func)
        {
            foreach (Node node in _nodes)
            {
                if (func(node))
                {
                    yield return node;
                }
            }
        }

        public int RemoveIf(Predicate<Node> filter)
        {
            var toRemove = _nodes.Where(n => filter(n)).ToList();
            foreach (Node n in toRemove)
            {
                RemoveNode(n);
            }

            return toRemove.Count;
        }
    }
}