using System;

namespace evolve
{
    [Flags]
    public enum ActionType
    {
        StayPut = 1 << 12,
        MoveNorth = 1 << 13,
        MoveSouth = 1 << 14,
        MoveEast = 1 << 15,
        MoveWest = 1 << 16,
        MoveRandom = 1 << 17,
    }

    public class Action : ISink
    {
        private const double InitialWeight = 0d;
        public Action(ActionType type)
        {
            Type = type;
            Weight = InitialWeight;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        
        public void UpdateWeight(double weight)
        {
            Weight += weight;
        }

        public double Weight { get; private set; }

        public void Reset()
        {
            Weight = InitialWeight;
        }

        public ActionType Type { get; }

        public int Fingerprint()
        {
            return (int) Type;
        }

        public string Description()
        {
            return $"Action ({Simulation.ActivationFunction(Weight)} - {Type} - {Id})";
        }

        public void Mutate()
        {
            // actions don't mutate
        }

        public ISink DeepCopy()
        {
            return new Action(Type);
        }
    }
}