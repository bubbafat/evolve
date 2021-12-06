using System;

namespace evolve
{
    public class Desires
    {
        public Desires()
        {
            Reset();
        }
        public float MoveX { get; set; }
        public float MoveY { get; set; }
        
        public float StayPut { get; set; }
        
        public void Reset()
        {
            MoveX = 0f;
            MoveY = 0f;
            StayPut = 0f;
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
                float weight = action.Weight;

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
                    default:
                        throw new InvalidOperationException($"Unknown action: {action.Type}");
                }
            }
        }

        public void Execute()
        {
            UpdateDesires();

            float stayWeight = Simulation.ActivationFunction(Math.Abs(_desire.StayPut));
            float moveXWeight = Simulation.ActivationFunction(Math.Abs(_desire.MoveY));
            float moveYWeight = Simulation.ActivationFunction(Math.Abs(_desire.MoveX));

            // if the desire to stay is stronger than the desire to move check if you stay
            if (stayWeight > moveXWeight && stayWeight > moveYWeight)
            {
                if (Simulation.WeightToBool(stayWeight))
                    return;
            }
            
            if (Simulation.WeightToBool(moveYWeight))
            {
                bool north = _desire.MoveY > 0f;
                Move(north ? Direction.North : Direction.South);
            }
            
            if (Simulation.WeightToBool(moveXWeight))
            {
                bool east = _desire.MoveX > 0f;
                Move(east ? Direction.East : Direction.West);
            }
        }

        private void MoveRandom()
        {
            bool x = RNG.Bool();
            bool pos = RNG.Bool();

            switch (x, pos)
            {
                case (true, true): _desire.MoveX += RNG.Float();
                    break;
                case (true, false): _desire.MoveX -= RNG.Float();
                    break;
                case (false, true): _desire.MoveY += RNG.Float();
                    break;
                case (false, false): _desire.MoveY -= RNG.Float();
                    break;
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