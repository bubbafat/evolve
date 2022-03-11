using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace evolve
{
    public class World
    {
        private readonly Node?[,] _grid;
        private readonly bool[,] _blocks;
        private readonly Node?[] _nodeCache;

        private readonly ConcurrentQueue<Move> _moveQueue;

        public World(int dimensions, int nodes)
        {
            Dimension = dimensions;
            _grid = new Node[Dimension, Dimension];
            _blocks = new bool[Dimension, Dimension];
            _moveQueue = new ConcurrentQueue<Move>();
            _nodeCache = new Node[nodes];
            primeCache();
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
            if (_moveQueue.Count > 0)
            {
                throw new NotSupportedException();
            }

            populateNodeCache();
        }

        public void EndStep()
        {
            while(!_moveQueue.IsEmpty)
            {
                if(_moveQueue.TryDequeue(out var action))
                {
                    PerformNodeMove(action.Node, action.Direction);
                }
            }

            populateNodeCache();
        }

        private void populateNodeCache()
        {
            for(int i = 0; i < _nodeCache.Length; i++)
            {
                _nodeCache[i] = null;
            }

            int index = 0;
            foreach (Node? node in _grid)
            {
                if (node != null)
                {
                    _nodeCache[index++] = node;
                }
            }
        }
        
        public IEnumerable<Node> Nodes => _nodeCache;

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
            return !IsWall(x, y) && !HasNode(x, y);
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

        private bool Do(double weight)
        {
            return Simulation.WeightToBool(Simulation.ActivationFunction(weight));
        }

        private void SwapNodes(int x1, int y1, int x2, int y2)
        {
            (_grid[x1, y1], _grid[x2, y2]) = (_grid[x2, y2], _grid[x1, y1]);
            (_grid[x1, y1]!.X, _grid[x1, y1]!.Y) = (x1, y1);
            (_grid[x2, y2]!.X, _grid[x2, y2]!.Y) = (x2, y2);
        }

        private void KillAt(int x, int y)
        {
            var node = _grid[x, y]!;
            
            _grid[x, y] = null;
            node.Alive = false;
        }

        (int x, int y) getDirection(Direction direction)
        {
            return direction switch
            {
                Direction.North => (0, 1),
                Direction.South => (0, -1),
                Direction.East => (1, 0),
                Direction.West => (-1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        private void beBully(Node node, int x, int y)
        {
            if (emptyNeighbors(x, y).ToList().TryGetRandom(out (int, int) location))
            {
                // push the blocking node to one of it's empty neighbors
                // freeing the slot needed for the node to move to
                var n = _grid[x, y];
                _grid[location.Item1, location.Item2] = _grid[x, y];
                _grid[x, y] = null;
                (n!.X, n!.Y) = (location.Item1, location.Item2);
            }
            else
            {
                // we can't push so do a swap - no subsequent move is needed
                SwapNodes(x, y, node.X, node.Y);
                return;
            }
        }

        private bool beKillerAndLive(Node node, int x, int y)
        {
            if (Do(_grid[x, y]!.Desire.Defend))
            {
                // the killer picked the wrong victim
                KillAt(node.X, node.Y);
                return false;
            }

            // kills and frees up the spot for the subsequent move
            KillAt(x, y);
            return true;
        }
        private void PerformNodeMove(Node node, Direction direction)
        {
            if (!node.Alive)
            {
                return;
            }

            var (dirX, dirY) = getDirection(direction);

            int x = node.X + dirX;
            int y = node.Y + dirY;
            
            if (OnBoard(x, y) && !IsWall(x, y))
            {
                if (!HasNode(x, y))
                {
                    _grid[node.X, node.Y] = null;
                    _grid[x, y] = node;
                    node.X = x;
                    node.Y = y;
                }

                // a bully will make the other node swap locations with them
                if (Do(node.Desire.Bully) && !Do(_grid[x,y]!.Desire.Defend))
                {
                    beBully(node, x, y);
                    return;
                }
                
                // a killer will kill the other node (and a killer bully will bully first)
                // unless the node is defensive
                if (Do(node.Desire.Kill))
                {
                    if(!beKillerAndLive(node, x, y))
                    {
                        return;
                    }
                }
            }
        }

        public void MoveNodeTo(Node node, Direction direction)
        {
            _moveQueue.Enqueue(new Move(node, direction));
        }

        public void AddAtRandom(Node node)
        {
            (node.X, node.Y) = RandomEmptyLocation();
            _grid[node.X, node.Y] = node;
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
        }

        private Dictionary<(int, int), (int, int)[]> neighborCache = new Dictionary<(int,int), (int,int)[]>();
        (int, int)[] neighborsGenerator(int x, int y)
        {
            return new []
            {
                (x - 1, y + 1),
                (x, y + 1),
                (x + 1, y + 1),

                (x - 1, y),
                (x + 1, y),

                (x - 1, y - 1),
                (x, y - 1),
                (x + 1, y - 1)
            };
        }

        readonly object primeLock = new object();
        void primeCache()
        {
            if (neighborCache.Count == 0)
            {
                lock (primeLock)
                {
                    if (neighborCache.Count == 0)
                    {
                        Console.WriteLine("Priming cache");
                        for (int x = 0; x < Dimension; x++)
                        {
                            for (int y = 0; y < Dimension; y++)
                            {
                                neighborCache.Add((x, y), neighborsGenerator(x, y).Where(n => OnBoard(n.Item1, n.Item2) && !IsWall(n.Item1, n.Item2)).ToArray());
                            }
                        }
                    }
                }
            }
        }

        (int, int)[]? validNeighbors(int x, int y)
        {
            return neighborCache[(x, y)];
        }

        private int ValidNeighborsSatisfying(int x, int y, Func<int,int,bool> test)
        {
            return validNeighbors(x, y).Count(c => test(c.Item1, c.Item2));
        }

        private IEnumerable<(int, int)> emptyNeighbors(int x, int y)
        {
            return validNeighbors(x, y).Where((coord) => AvailableForNode(coord.Item1, coord.Item2));
        }

        private double DistanceFromCenter(int x, int y)
        {
            int x2 = Dimension / 2;
            int y2 = Dimension / 2;

            return Math.Sqrt(Math.Pow(x2 - x, 2) + Math.Pow(y2 - y, 2));
        }

        public double ReadSensor(SensorType type, Node node)
        {
            return type switch
            {
                SensorType.TimeSinceLastMove => node.LastMoveStep / (double) Simulation.CurrentStep,
                SensorType.DistanceFromNorth => node.Y / (double) Dimension,
                SensorType.DistanceFromSouth => 1.0 - node.Y / (double) Dimension,
                SensorType.DistanceFromWest => node.X / (double) Dimension,
                SensorType.DistanceFromEast => 1.0 - node.X / (double) Dimension,
                SensorType.LocalPopulation => ValidNeighborsSatisfying(node.X, node.Y, (x,y) => HasNode(x, y)) / 8.0,
                SensorType.DistanceFromCenter => DistanceFromCenter(node.X, node.Y) / (Dimension / 2.0),
                SensorType.Blocked => ValidNeighborsSatisfying(node.X, node.Y, (x,y) => !HasNode(node.X, node.Y)) / 8.0,
                _ => throw new NotSupportedException("Invalid SensorType")
            };
        }

        public void RemoveIf(Predicate<Node> filter)
        {
            foreach (Node? n in _grid)
            {
                if (n != null)
                {
                    if (filter(n))
                    {
                        RemoveNode(n);
                    }
                }
            }

            populateNodeCache();
        }
    }
}