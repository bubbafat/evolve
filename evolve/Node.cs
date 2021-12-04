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

        public Node(World world, Genome genome)
        {
            _genome = genome;
            World = world;
            Location = Location.InvalidLocation;
            Id = Guid.NewGuid();
            LastMoveStep = 0;
            Desire = new Desires();
        }
        
        public Desires Desire { get; }

        public void Evaluate()
        {
            _genome.Evaluate(this);
        }

        private void updateDesires()
        {
            var actions = _genome.GetActions();

            foreach (var action in actions)
            {
                float weight = action.Weight;

                switch (action.Type)
                {
                    case ActionType.MoveNorth:
                        Desire.MoveY += weight;
                        break;
                    case ActionType.MoveSouth:
                        Desire.MoveY -= weight;
                        break;
                    case ActionType.MoveEast:
                        Desire.MoveX += weight;
                        break;
                    case ActionType.MoveWest:
                        Desire.MoveX -= weight;
                        break;
                    case ActionType.StayPut:
                        Desire.StayPut += weight;
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
            updateDesires();
            
            if(Simulation.WeightToBool(Simulation.ActivationFunction(Math.Abs(Desire.StayPut))))
            {
                return;
            }
            
            bool north = Desire.MoveY > 0f;
            bool movey = Simulation.WeightToBool(Simulation.ActivationFunction(Math.Abs(Desire.MoveY)));

            bool east = Desire.MoveX > 0f;
            bool movex = Simulation.WeightToBool(Simulation.ActivationFunction(Math.Abs(Desire.MoveX)));

            if (movey)
            {
                Move(north ? Direction.North : Direction.South);
            }
            
            if (movex)
            {
                Move(east ? Direction.East : Direction.West);
            }
        }

        public void MoveRandom()
        {
            bool x = RNG.Bool();
            bool pos = RNG.Bool();

            switch (x, pos)
            {
                case (true, true): Desire.MoveX += RNG.Float();
                    break;
                case (true, false): Desire.MoveX -= RNG.Float();
                    break;
                case (false, true): Desire.MoveY += RNG.Float();
                    break;
                case (false, false): Desire.MoveY -= RNG.Float();
                    break;
            }
        }

        public bool Move(Direction direction)
        {
            World.MoveNodeTo(this, direction);
            LastMoveStep = Simulation.CurrentStep;

            return true;
        }

        public void Reset()
        {
            LastMoveStep = 0;
            _genome.Reset();
            Desire.Reset();
        }

        public Node Reproduce(Node other)
        {
            return new Node(this.World, _genome.Reproduce(other._genome));
        }

        public string Description()
        {
            return _genome.Description();
        }

        public int LastMoveStep { get; private set; }

        public Guid Id { get; }

        public World World { get; }
        public Location Location { get; set; }

        public int Fingerprint()
        {
            return _genome.Fingerprint();
        }
    }
}