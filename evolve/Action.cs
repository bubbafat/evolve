using System;

namespace evolve
{
    [Flags]
    public enum ActionType
    {
        MoveNorth  = (1 << 13),
        MoveSouth  = (1 << 14),
        MoveEast   = (1 << 15),
        MoveWest   = (1 << 16),
        MoveNorthEast = (1 << 17),
        MoveNorthWest = (1 << 18),
        MoveSouthEast = (1 << 19),
        MoveSouthWest = (1 << 20),
    }

    public class Action : IAction
    {
        private readonly ActionType _type;
        private float _weight;
        private float _initialWeight;

        public Action(ActionType type, float initialWeight)
        {
            _initialWeight = 0f;
            _type = type;
            _weight = _initialWeight;
        }

        public float InitialWeight => _initialWeight;
        
        public void UpdateWeight(float weight)
        {
            _weight += weight;
        }

        public float Weight => _weight;

        public void Reset()
        {
            _weight = _initialWeight;
        }

        public bool Act(Node node)
        {
            bool act = Simulation.WeightToBool(Simulation.ActivationFunction(_weight));

            if (act)
            {
                return _type switch
                {
                    ActionType.MoveNorth => node.Move(node.Location.North()),
                    ActionType.MoveSouth => node.Move(node.Location.South()),
                    ActionType.MoveEast => node.Move(node.Location.East()),
                    ActionType.MoveWest => node.Move(node.Location.West()),
                    ActionType.MoveNorthEast => node.Move(node.Location.North()) | node.Move(node.Location.East()),
                    ActionType.MoveNorthWest => node.Move(node.Location.North()) | node.Move(node.Location.West()),
                    ActionType.MoveSouthEast => node.Move(node.Location.South()) | node.Move(node.Location.East()),
                    ActionType.MoveSouthWest => node.Move(node.Location.South()) | node.Move(node.Location.West()),
                    _ => throw new NotSupportedException("Invalid action")
                };
            }

            return false;
        }

        public ActionType Type => _type;

        public int Fingerprint()
        {
            return (int) _type;
        }
        
        public string Description()
        {
            return $"Action ({Simulation.ActivationFunction(_weight)} - {_type})";
        }
        
        public void Mutate()
        {
            if (Simulation.WeightToBool(0.001f))
            {
                var up = RNG.Bool();
                if (up)
                {
                    _initialWeight += RNG.Float() * (1f - _initialWeight);
                }
                else
                {
                    _initialWeight *= RNG.Float();
                }
            }
        }
    }
}