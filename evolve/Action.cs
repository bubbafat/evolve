using System;

namespace evolve
{
    [Flags]
    public enum ActionType
    {
        StayPut = (1 << 12),
        MoveNorth = (1 << 13),
        MoveSouth = (1 << 14),
        MoveEast = (1 << 15),
        MoveWest = (1 << 16),
        MoveRandom = (1 << 17),
    }

    public class Action : IAction
    {
        private float _weight;
        
        public Action(ActionType type, float initialWeight)
        {
            Type = type;
            _weight = InitialWeight;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        
        public float InitialWeight => 0f;

        public void UpdateWeight(float weight)
        {
            _weight += weight;
        }

        public float Weight => _weight;
        
        public void Reset()
        {
            _weight = InitialWeight;
        }

        public ActionType Type { get; }

        public int Fingerprint()
        {
            return (int) Type;
        }

        public string Description()
        {
            return $"Action ({Simulation.ActivationFunction(_weight)} - {Type} - {Id})";
        }

        public ISink Mutate()
        {
            // actions don't mutate
            return this;
        }

        public ISink DeepCopy()
        {
            return new Action(Type, InitialWeight);
        }
    }
}