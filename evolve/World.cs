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

        private readonly ThreadLocal<Queue<Move>> _moveQueue = new ThreadLocal<Queue<Move>>(() => new Queue<Move>(), true);
        public readonly static int Dimension = Simulation.BoardDimensions;

        public World()
        {
            _grid = new Node[Dimension, Dimension];
            _blocks = new bool[Dimension, Dimension];
            primeNeighborCache();
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
            foreach(var queue in _moveQueue.Values)
            {
                if (queue.Count > 0)
                {
                    throw new NotSupportedException();
                }
            }
        }

        public void EndStep()
        {
            foreach (var queue in _moveQueue.Values)
            {
                while (queue.Count > 0)
                {
                    var action = queue.Dequeue();
                    PerformNodeMove(action.Node, action.Direction);
                }
            }
        }
        
        public IEnumerable<Node> Nodes => _nodes;

        private bool HasNode(int x, int y)
        {
            return _grid[x, y] != null;
        }
        
        private bool IsWall(int x, int y)
        {
            return _blocks[x, y];
        }
        
        private static bool OnBoard(int x, int y)
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
        
        private Coords RandomEmptyLocation()
        {
            while (true)
            {
                int x = RNG.Int() % Dimension;
                int y = RNG.Int() % Dimension;

                if (AvailableForNode(x, y))
                {
                    return new Coords(x, y);
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
            (_grid[x1, y1]!.Location, _grid[x2, y2]!.Location) = (_grid[x2, y2]!.Location, _grid[x1, y1]!.Location);
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

            int x = node.Location.X + dirX;
            int y = node.Location.Y + dirY;

            if (!OnBoard(x, y) || IsWall(x, y))
                return;

            Coords loc = new Coords(x, y);

            if (!HasNode(loc.X, loc.Y))
            {
                _grid[node.Location.X, node.Location.Y] = null;
                _grid[loc.X, loc.Y] = node;
                node.Location = new Coords(loc.X, loc.Y);
            }

            // a bully will make the other node swap locations with them
            if (HasNode(loc.X, loc.Y) && Do(node.Desire.Bully) && !Do(_grid[loc.X, loc.Y]!.Desire.Defend))
            {
                if (emptyNeighbors(loc).ToList().TryGetRandom(out Coords location))
                {
                    // push the blocking node to one of it's empty neighbors
                    // freeing the slot needed for the node to move to
                    var n = _grid[loc.X, loc.Y];
                    _grid[location.X, location.Y] = _grid[loc.X, loc.Y];
                    _grid[loc.X, loc.Y] = null;
                    n!.Location = new Coords(location.X, location.Y);
                }
                else
                {
                    // we can't push so do a swap - no subsequent move is needed
                    SwapNodes(loc.X, loc.Y, node.Location.X, node.Location.Y);
                    return;
                }
            }

            // a killer will kill the other node (and a killer bully will bully first)
            // unless the node is defensive
            if (HasNode(loc.X, loc.Y) && Do(node.Desire.Kill))
            {
                if (Do(_grid[loc.X, loc.Y]!.Desire.Defend))
                {
                    // the killer picked the wrong victim
                    KillAt(node.Location.X, node.Location.Y);
                    return;
                }

                // kills and frees up the spot for the subsequent move
                KillAt(loc.X, loc.Y);
            }
        }

        public void MoveNodeTo(Node node, Direction direction)
        {
            _moveQueue.Value.Enqueue(new Move(node, direction));
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

        private void RemoveNode(Node node)
        {
            _grid[node.Location.X, node.Location.Y] = null;
            _nodes.RemoveAll(n => n.Id == node.Id);
        }

        static Dictionary<Coords, Coords[]> neighborCache = new Dictionary<Coords, Coords[]>();
        static void primeNeighborCache()
        {
            for(int x = 0; x < Simulation.BoardDimensions; x++)
                for (int y = 0; y < Simulation.BoardDimensions; y++)
                {
                    neighborCache.Add(new Coords(x, y), neighborsFor(x, y));
                }

        }

        static Coords[] neighborsFor(int x, int y)
        {
            return new Coords[] {
                new Coords(x - 1, y + 1),
                new Coords(x, y + 1),
                new Coords(x + 1, y + 1),

                new Coords(x - 1, y),
                new Coords(x + 1, y),

                new Coords(x - 1, y - 1),
                new Coords(x, y - 1),
                new Coords(x + 1, y - 1)
            }.Where(x => OnBoard(x.X, x.Y)).ToArray();
        }

        public Coords[] neighbors(Coords coord)
        {
            return neighborCache[coord];
        }

        private int NeighborsSatisfying(Coords coord, Func<Coords,bool> test)
        {
            int count = 0;
            foreach(var c in neighbors(coord))
            {
                if(test(c))
                {
                    count++;
                }
            }

            return count;
        }

        private IEnumerable<Coords> emptyNeighbors(Coords coord)
        {
            return neighbors(coord).Where((coord) => AvailableForNode(coord.X, coord.Y));
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
                SensorType.DistanceFromNorth => 1.0 - node.Location.Y / (double) Dimension,
                SensorType.DistanceFromSouth => node.Location.Y / (double) Dimension,
                SensorType.DistanceFromWest => 1.0 - node.Location.X / (double) Dimension,
                SensorType.DistanceFromEast => node.Location.X / (double) Dimension,
                SensorType.LocalPopulation => NeighborsSatisfying(node.Location, (coord) => OnBoard(coord.X, coord.Y) && HasNode(coord.X, coord.Y)) / 8.0,
                SensorType.DistanceFromCenter => DistanceFromCenter(node.Location.X, node.Location.Y),
                SensorType.Blocked => NeighborsSatisfying(node.Location, (coord) => !AvailableForNode(coord.X, coord.Y)) / 8.0,
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