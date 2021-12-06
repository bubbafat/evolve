using System;
using System.Collections.Generic;
using System.Linq;

namespace evolve
{
    public class World
    {
        private readonly Node?[,] _grid;
        private readonly bool[,] _blocks;
        private readonly List<Node> _nodes = new List<Node>();

        private readonly List<Move> _moveQueue;

        public World(int dimensions)
        {
            Dimension = dimensions;
            _grid = new Node[Dimension, Dimension];
            _blocks = new bool[Dimension, Dimension];
            _moveQueue = new List<Move>();
        }

        private class Move
        {
            public Move(Node node, Direction dir)
            {
                Node = node;
                Direction = dir;
            }
            
            public readonly Node Node;
            public readonly Direction Direction;
        }

        public void BeginStep()
        {
            _moveQueue.Clear();
        }

        public void EndStep()
        {
            foreach (var action in _moveQueue)
            {
                PerformNodeMove(action.Node, action.Direction);
            }
        }
        
        public IEnumerable<Node> Nodes => _nodes;

        public int Dimension { get; }

        private bool HasNode(int x, int y)
        {
            return _grid[x, y] != null;
        }
        
        private bool IsWall(int x, int y)
        {
            return _blocks[x, y];
        }
        
        private bool OnBoard(int x, int y)
        {
            bool offBoard = x < 0 || x > Dimension-1 || y < 0 || y > Dimension-1;
            return !offBoard;
        }

        private bool AvailableForNode(int x, int y)
        {
            return OnBoard(x, y) && !IsWall(x, y) && !HasNode(x, y);
        }
        
        public void Clear()
        {
            RemoveIf(n => true);
        }
        
        private (int,int) RandomEmptyLocation()
        {
            while (true)
            {
                int x = RNG.Int() % Dimension;
                int y = RNG.Int() % Dimension;

                if (AvailableForNode(x, y))
                {
                    return (x, y);
                }
            }
        }

        public IEnumerable<(int,int)> Walls
        {
            get
            {
                for (int x = 0; x < Dimension; x++)
                for (int y = 0; y < Dimension; y++)
                {
                    if (IsWall(x, y))
                    {
                        yield return (x, y);
                    }
                }
            }
        }

        public void AddWall(int locx, int locy, int width, int height)
        {
            for (int x = locx; x < locx + width; x++)
            for (int y = locy; y < locy + height; y++)
            {
                if (OnBoard(x, y))
                {
                    _blocks[x, y] = true;
                }
            }
        }
        private void PerformNodeMove(Node node, Direction direction)
        {
            var (dirX, dirY) = direction switch
            {
                Direction.North => (0, 1),
                Direction.South => (0, -1),
                Direction.East => (1, 0),
                Direction.West => (-1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };

            int x = node.X + dirX;
            int y = node.Y + dirY;
            
            if (AvailableForNode(x, y))
            {
                _grid[node.X, node.Y] = null;
                _grid[x, y] = node;
                node.X = x;
                node.Y = y;
            }
        }

        public void MoveNodeTo(Node node, Direction direction)
        {
            _moveQueue.Add(new Move(node, direction));
        }

        public void AddAtRandom(Node node)
        {
            (node.X, node.Y) = RandomEmptyLocation();
            _grid[node.X, node.Y] = node;
            _nodes.Add(node);
        }

        public void AddAtRandom(IEnumerable<Node> nodes)
        {
            foreach (var n in nodes)
            {
                AddAtRandom(n);
            }
        }

        private void RemoveNode(Node node)
        {
            _grid[node.X, node.Y] = null;
            _nodes.RemoveAll(n => n.Id == node.Id);
        }

        private int PopulationAround(int x, int y)
        {
            int count = 0;
            count += OnBoard(x-1, y+1) && HasNode(x-1, y+1) ? 1 : 0;
            count += OnBoard(x, y+1) && HasNode(x, y+1) ? 1 : 0;
            count += OnBoard(x+1, y+1) && HasNode(x+1, y+1) ? 1 : 0;

            count += OnBoard(x-1, y) && HasNode(x-1, y) ? 1 : 0;
            count += OnBoard(x+1, y) && HasNode(x+1, y) ? 1 : 0;

            count += OnBoard(x-1, y-1) && HasNode(x-1, y-1) ? 1 : 0;
            count += OnBoard(x, y-1) && HasNode(x, y-1) ? 1 : 0;
            count += OnBoard(x+1, y-1) && HasNode(x+1, y-1) ? 1 : 0;

            return count;
        }

        public float ReadSensor(SensorType type, Node node)
        {
            return type switch
            {
                SensorType.TimeSinceLastMove => node.LastMoveStep / (float) Simulation.CurrentStep,
                SensorType.DistanceFromNorth => 1f - node.Y / (float) Dimension,
                SensorType.DistanceFromSouth => node.Y / (float) Dimension,
                SensorType.DistanceFromWest => 1f - node.X / (float) Dimension,
                SensorType.DistanceFromEast => node.X / (float) Dimension,
                SensorType.LocalPopulation => PopulationAround(node.X, node.Y),
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