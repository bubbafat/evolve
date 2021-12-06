using System;

namespace evolve
{
    public class Desires
    {
        public Desires()
        {
            Reset();
        }
        public double MoveX { get; set; }
        public double MoveY { get; set; }
        
        public double StayPut { get; set; }
        
        public void Reset()
        {
            MoveX = 0d;
            MoveY = 0d;
            StayPut = 0d;
        }
    }
    public class Node
    {
        private readonly Genome _genome;
        private Desires _desire;

        public Node(World world, Genome genome)
        {
            _genome = genome;
            World = world;
            Id = Guid.NewGuid();
            LastMoveStep = 0;
            _desire = new Desires();
            X = -1;
            Y = -1;
        }
        
        public int X { get; set; }
        public int Y { get; set; }
        
        public void Evaluate()
        {
            _genome.Evaluate(this);
        }

        private void UpdateDesires()
        {
            var actions = _genome.GetActions();

            foreach (var action in actions)
            {
                double weight = action.Weight;

                switch (action.Type)
                {
                    case ActionType.MoveNorth:
                        _desire.MoveY += weight;
                        break;
                    case ActionType.MoveSouth:
                        _desire.MoveY -= weight;
                        break;
                    case ActionType.MoveEast:
                        _desire.MoveX += weight;
                        break;
                    case ActionType.MoveWest:
                        _desire.MoveX -= weight;
                        break;
                    case ActionType.StayPut:
                        _desire.StayPut += weight;
                        break;
                    case ActionType.MoveRandom:
                        MoveRandom();
                        break;
                    case ActionType.MoveToCenter:
                        MoveToCenter();
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown action: {action.Type}");
                }
            }
        }

        public void Execute()
        {
            UpdateDesires();

            double stayWeight = Simulation.ActivationFunction(Math.Abs(_desire.StayPut));
            double moveXWeight = Simulation.ActivationFunction(Math.Abs(_desire.MoveY));
            double moveYWeight = Simulation.ActivationFunction(Math.Abs(_desire.MoveX));

            // if the desire to stay is stronger than the desire to move check if you stay
            if (stayWeight > moveXWeight && stayWeight > moveYWeight)
            {
                if (Simulation.WeightToBool(stayWeight))
                    return;
            }
            
            if (Simulation.WeightToBool(moveYWeight))
            {
                bool north = _desire.MoveY > 0d;
                Move(north ? Direction.North : Direction.South);
            }
            
            if (Simulation.WeightToBool(moveXWeight))
            {
                bool east = _desire.MoveX > 0d;
                Move(east ? Direction.East : Direction.West);
            }
        }

        private void MoveRandom()
        {
            bool x = RNG.Bool();
            bool pos = RNG.Bool();

            switch (x, pos)
            {
                case (true, true): _desire.MoveX += RNG.Double();
                    break;
                case (true, false): _desire.MoveX -= RNG.Double();
                    break;
                case (false, true): _desire.MoveY += RNG.Double();
                    break;
                case (false, false): _desire.MoveY -= RNG.Double();
                    break;
            }
        }

        private void MoveToCenter()
        {
            bool westOfCenter = X < World.Dimension / 2 - 5;
            bool eastOfCenter = X > World.Dimension / 2 + 5;
            
            bool southOfCenter = Y < World.Dimension / 2 - 5;
            bool northOfCenter = Y > World.Dimension / 2 + 5;

            if (westOfCenter)
            {
                _desire.MoveX += RNG.Double();
            } 
            else if (eastOfCenter)
            {
                _desire.MoveX -= RNG.Double();
            }

            if (southOfCenter)
            {
                _desire.MoveY += RNG.Double();
            }
            else if (northOfCenter)
            {
                _desire.MoveY -= RNG.Double();
            }
        }

        private void Move(Direction direction)
        {
            World.MoveNodeTo(this, direction);
            LastMoveStep = Simulation.CurrentStep;
        }

        public void Reset()
        {
            LastMoveStep = 0;
            _genome.Reset();
            _desire.Reset();
        }

        public Node Reproduce(Node other)
        {
            return new Node(World, _genome.Reproduce(other._genome));
        }

        public string Description()
        {
            return _genome.Description();
        }

        public int LastMoveStep { get; private set; }

        public Guid Id { get; }

        public World World { get; }
        public int Fingerprint()
        {
            return _genome.Fingerprint();
        }
    }
}