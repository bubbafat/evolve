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
        private readonly List<Node> _nodes = new List<Node>();

        private readonly ConcurrentQueue<Move> _moveQueue;

        public World(int dimensions)
        {
            Dimension = dimensions;
            _grid = new Node[Dimension, Dimension];
            _blocks = new bool[Dimension, Dimension];
            _moveQueue = new ConcurrentQueue<Move>();
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
        }

        public void EndStep()
        {
            while(!_moveQueue.IsEmpty)
            {
                if(_moveQueue.TryDequeue(out Move action))
                {
                    PerformNodeMove(action.Node, action.Direction);
                }
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
            _nodes.RemoveAll(n => n.Id == node.Id);
        }
        private void PerformNodeMove(Node node, Direction direction)
        {
            if (!node.Alive)
            {
                return;
            }
            
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
            
            if (OnBoard(x, y) && !IsWall(x, y))
            {
                // a bully will make the other node swap locations with them
                if (HasNode(x, y) && Do(node.Desire.Bully) && !Do(_grid[x,y]!.Desire.Defend))
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
                
                // a killer will kill the other node (and a killer bully will bully first)
                // unless the node is defensive
                if (HasNode(x, y) && Do(node.Desire.Kill))
                {
                    if (Do(_grid[x, y]!.Desire.Defend))
                    {
                        // the killer picked the wrong victim
                        KillAt(node.X, node.Y);
                        return;
                    }

                    // kills and frees up the spot for the subsequent move
                    KillAt(x, y);
                }

                if (!HasNode(x, y))
                {
                    _grid[node.X, node.Y] = null;
                    _grid[x, y] = node;
                    node.X = x;
                    node.Y = y;
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

        IEnumerable<(int, int)> neighbors(int x, int y)
        {
            yield return (x - 1, y + 1);
            yield return (x, y + 1);
            yield return (x + 1, y + 1);
            
            yield return (x - 1, y);
            yield return (x + 1, y);

            yield return (x - 1, y - 1);
            yield return (x, y - 1);
            yield return (x + 1, y - 1);
        }
        private int NeighborsSatisfying(int x, int y, Func<int,int,bool> test)
        {
            return neighbors(x, y).Count(c => test(c.Item1, c.Item2));
        }

        private IEnumerable<(int, int)> emptyNeighbors(int x, int y)
        {
            return neighbors(x, y).Where((coord) => AvailableForNode(coord.Item1, coord.Item2));
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
                SensorType.DistanceFromNorth => 1.0 - node.Y / (double) Dimension,
                SensorType.DistanceFromSouth => node.Y / (double) Dimension,
                SensorType.DistanceFromWest => 1.0 - node.X / (double) Dimension,
                SensorType.DistanceFromEast => node.X / (double) Dimension,
                SensorType.LocalPopulation => NeighborsSatisfying(node.X, node.Y, (x,y) => OnBoard(x, y) && HasNode(x, y)) / 8.0,
                SensorType.DistanceFromCenter => DistanceFromCenter(node.X, node.Y),
                SensorType.Blocked => NeighborsSatisfying(node.X, node.Y, (x,y) => !AvailableForNode(x, y)) / 8.0,
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