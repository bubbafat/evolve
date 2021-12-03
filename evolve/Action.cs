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
        MoveNorthEast = (1 << 17),
        MoveNorthWest = (1 << 18),
        MoveSouthEast = (1 << 19),
        MoveSouthWest = (1 << 20),
        MoveRandom = (1 << 21),
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

        public bool Act(Node node)
        {
            bool act = Simulation.WeightToBool(Simulation.ActivationFunction(_weight));

            if (act)
            {
                return Type switch
                {
                    ActionType.StayPut => true,
                    ActionType.MoveRandom => node.MoveRandom(),
                    ActionType.MoveNorth => node.Move(node.Location.North()),
                    ActionType.MoveSouth => node.Move(node.Location.South()),
                    ActionType.MoveEast => node.Move(node.Location.East()),
                    ActionType.MoveWest => node.Move(node.Location.West()),
                    ActionType.MoveNorthEast => node.Move(node.Location.NorthEast()),
                    ActionType.MoveNorthWest => node.Move(node.Location.NorthWest()),
                    ActionType.MoveSouthEast => node.Move(node.Location.SouthEast()),
                    ActionType.MoveSouthWest => node.Move(node.Location.SouthWest()),
                    _ => throw new NotSupportedException("Invalid action")
                };
            }

            return false;
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