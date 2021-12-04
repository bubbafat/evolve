using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace evolve
{
    public class World
    {
        private readonly Node?[,] _grid;
        private readonly bool[,] _blocks;
        private readonly List<Node> _nodes = new List<Node>();

        private readonly List<Move> moveQueue;
        private object _queueLock = new object();

        public World(int dimensions)
        {
            Dimension = dimensions;
            _grid = new Node[Dimension, Dimension];
            _blocks = new bool[Dimension, Dimension];
            moveQueue = new List<Move>();
        }

        class Move
        {
            public Node node;
            public Direction direction;
        }

        public void BeginStep()
        {
            moveQueue.Clear();
        }

        public void EndStep()
        {
            foreach (var action in moveQueue)
            {
                moveNodeTo(action.node, action.direction);
            }
        }
        
        

        public IEnumerable<Node> Nodes
        {
            get => _nodes;
        }

        public int Dimension { get; }

        private bool hasNode(int x, int y)
        {
            return _grid[x, y] != null;
        }

        private bool hasNode(Location loc)
        {
            return hasNode(loc.X, loc.Y);
        }

        private bool isWall(int x, int y)
        {
            return _blocks[x, y];
        }

        private bool isWall(Location loc)
        {
            return isWall(loc.X, loc.Y);
        }

        private bool onBoard(int x, int y)
        {
            return x >= 0
                   && x < Dimension
                   && y >= 0
                   && y < Dimension;
        }

        private bool availableForNode(int x, int y)
        {
            return onBoard(x, y) && !isWall(x, y) && !hasNode(x, y);
        }

        private bool availableForNode(Location loc)
        {
            return availableForNode(loc.X, loc.Y);
        }

        private bool onBoard(Location loc)
        {
            return onBoard(loc.X, loc.Y);
        }

        public void Clear()
        {
            RemoveIf(n => true);
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
            while (true)
            {
                int x = RNG.Int() % Dimension;
                int y = RNG.Int() % Dimension;

                if (availableForNode(x, y))
                {
                    return Location.From(x, y);
                }
            }
        }

        public IEnumerable<Location> Walls
        {
            get
            {
                for (int x = 0; x < Dimension; x++)
                for (int y = 0; y < Dimension; y++)
                {
                    if (isWall(x, y))
                    {
                        yield return Location.From(x, y);
                    }
                }
            }
        }

        public void AddWall(int x, int y, int width, int height)
        {
            for (; x < x + width; x++)
            for (; y < height; y++)
            {
                if (onBoard(x, y))
                {
                    _blocks[x, y] = true;
                }
            }
        }

        private IEnumerable<Location> surroundingLocations(Location loc)
        {
            yield return Location.From(loc.X - 1, loc.Y - 1);
            yield return Location.From(loc.X, loc.Y - 1);
            yield return Location.From(loc.X + 1, loc.Y - 1);

            yield return Location.From(loc.X - 1, loc.Y);
            yield return Location.From(loc.X + 1, loc.Y);

            yield return Location.From(loc.X - 1, loc.Y + 1);
            yield return Location.From(loc.X, loc.Y + 1);
            yield return Location.From(loc.X + 1, loc.Y + 1);
        }

        public Location RandomEmptyNeighbor(Location loc)
        {
            var neighbors = surroundingLocations(loc)
                .Where(l => availableForNode(l))
                .ToList();

            if (neighbors.Count > 0)
            {
                return neighbors.Random();
            }

            return loc;
        }
        
        private bool moveNodeTo(Node node, Direction direction)
        {
            var xy = direction switch
            {
                Direction.North => (0, 1),
                Direction.South => (0, -1),
                Direction.East => (1, 0),
                Direction.West => (-1, 0)
            };

            int x = node.Location.X + xy.Item1;
            int y = node.Location.Y + xy.Item2;
            
            if (availableForNode(x, y))
            {
                _grid[node.Location.X, node.Location.Y] = null;
                _grid[x, y] = node;
                node.Location = Location.From(x, y);
                
                return true;
            }
            
            return false;
        }

        public bool MoveNodeTo(Node node, Direction direction)
        {
            lock (_queueLock)
            {
                moveQueue.Add(new Move
                {
                    node = node,
                    direction = direction,
                });
            }

            return true;
        }

        public void AddAtRandom(Node node)
        {
            node.Location = RandomEmptyLocation();
            _grid[node.Location.X, node.Location.Y] = node;
            _nodes.Add(node);
        }

        public void AddAtRandom(IEnumerable<Node> nodes)
        {
            foreach (var n in nodes)
            {
                AddAtRandom(n);
            }
        }

        public void RemoveNode(Node node)
        {
            _grid[node.Location.X, node.Location.Y] = null;
            _nodes.RemoveAll(n => n.Id == node.Id);
        }

        private float populationAround(Location loc)
        {
            return surroundingLocations(loc)
                .Count(l => onBoard(loc) && hasNode(loc)) / 8f;
        }

        public float ReadSensor(SensorType type, Node node)
        {
            Location location = node.Location;
            return type switch
            {
                SensorType.TimeSinceLastMove => (float) node.LastMoveStep / (float) Simulation.CurrentStep,
                SensorType.DistanceFromNorth => 1f - (float) location.Y / (float) Dimension,
                SensorType.DistanceFromSouth => (float) location.Y / (float) Dimension,
                SensorType.DistanceFromWest => 1f - (float) location.X / (float) Dimension,
                SensorType.DistanceFromEast => (float) location.X / (float) Dimension,
                SensorType.LocalPopulation => populationAround(location),
                _ => throw new NotSupportedException("Invalid SensorType")
            };
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