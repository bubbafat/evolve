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
        MoveToCenterX = 1 << 18,
        MoveToCenterY = 1 << 19,
        Bully = 1 << 20,
        Kill = 1 << 21,
        Defend = 1 << 22,
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

        public ActionType Type { get; private set; }

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
            if (Simulation.WeightToBool(Simulation.MutationChance))
            {
                Type = NetworkBuilder.ActionTypes.Random();                
            }
        }

        public ISink DeepCopy()
        {
            return new Action(Type);
        }
    }
}