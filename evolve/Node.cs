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
        
        public double Bully { get; set; }
        public double Kill { get; set; }
        
        public double Defend { get; set; }
        
        public void Reset()
        {
            MoveX = 0d;
            MoveY = 0d;
            StayPut = 0d;
            Bully = 0d;
            Kill = 0d;
            Defend = 0d;
        }
    }
    public class Node
    {
        public bool Alive { get; set; }
        private readonly Genome _genome;
        public Desires Desire { get; private set; }

        public Node(World world, Genome genome)
        {
            _genome = genome;
            World = world;
            Id = Guid.NewGuid();
            LastMoveStep = 0;
            Desire = new Desires();
            Location = new Coords(-1, -1);
            Alive = true;
        }

        public Coords Location { get; set; }
                
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
                        MoveRandom(weight);
                        break;
                    case ActionType.MoveToCenterX:
                        MoveToCenter(true, false, weight);
                        break;
                    case ActionType.MoveToCenterY:
                        MoveToCenter(false, true, weight);
                        break;
                    case ActionType.Bully:
                        Desire.Bully += weight;
                        break;
                    case ActionType.Kill:
                        Desire.Kill += weight;
                        break;
                    case ActionType.Defend:
                        Desire.Defend += weight;
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown action: {action.Type}");
                }
            }
        }

        public void Execute()
        {
            UpdateDesires();

            double stayWeight = Simulation.ActivationFunction(Math.Abs(Desire.StayPut));
            double moveXWeight = Simulation.ActivationFunction(Math.Abs(Desire.MoveY));
            double moveYWeight = Simulation.ActivationFunction(Math.Abs(Desire.MoveX));

            // if the desire to stay is stronger than the desire to move check if you stay
            if (stayWeight > moveXWeight && stayWeight > moveYWeight)
            {
                if (Simulation.WeightToBool(stayWeight))
                    return;
            }
            
            if (Simulation.WeightToBool(moveYWeight))
            {
                bool north = Desire.MoveY > 0d;
                Move(north ? Direction.North : Direction.South);
            }
            
            if (Simulation.WeightToBool(moveXWeight))
            {
                bool east = Desire.MoveX > 0d;
                Move(east ? Direction.East : Direction.West);
            }
        }

        private void MoveRandom(double weight)
        {
            bool x = RNG.Bool();
            bool pos = RNG.Bool();

            switch (x, pos)
            {
                case (true, true): Desire.MoveX += weight;
                    break;
                case (true, false): Desire.MoveX -= weight;
                    break;
                case (false, true): Desire.MoveY += weight;
                    break;
                case (false, false): Desire.MoveY -= weight;
                    break;
            }
        }

        private void MoveToCenter(bool centerX, bool centerY, double weight)
        {

            if (centerX)
            {
                bool westOfCenter = Location.X < World.Dimension / 2;
                bool eastOfCenter = Location.X > World.Dimension / 2;

                if (westOfCenter)
                {
                    Desire.MoveX += weight;
                }
                else if (eastOfCenter)
                {
                    Desire.MoveX -= weight;
                }
            }

            if (centerY)
            {
                bool southOfCenter = Location.Y < World.Dimension / 2;
                bool northOfCenter = Location.Y > World.Dimension / 2;


                if (southOfCenter)
                {
                    Desire.MoveY += weight;
                }
                else if (northOfCenter)
                {
                    Desire.MoveY -= weight;
                }
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
            Desire.Reset();
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