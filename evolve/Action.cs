using System;

namespace evolve
{
    public enum ActionIds
    {
        MoveRandom
    }
    
    public struct ActionTypes
    {
        public readonly ActionIds Type;
        public readonly Guid Id;

        public ActionTypes(ActionIds type)
        {
            Type = type;
            Id = Guid.NewGuid();
        }
    }

    public static class Actions
    {
        public static ActionTypes MoveRandom = new ActionTypes(ActionIds.MoveRandom);
    }

    public class Action : IAction
    {
        private readonly ActionTypes _type;
        private float _weight;
        private readonly float _initialWeight;

        public Action(ActionTypes type)
        {
            _initialWeight = 0f;
            _type = type;
            _weight = _initialWeight;
        }
        
        public void UpdateWeight(float weight)
        {
            _weight += weight;
        }

        public void Reset()
        {
            _weight = _initialWeight;
        }

        public void Act(Node node)
        {
            bool act = Simulation.WeightToBool(Simulation.ActivationFunction(_weight));

            if (act)
            {
                switch (_type.Type)
                {
                    case ActionIds.MoveRandom:
                        node.MoveRandom();
                        break;
                    default:
                        throw new NotSupportedException("Invalid action");
                }
            }
        }

        public override int GetHashCode()
        {
            return _type.Id.GetHashCode();
        }
    }
}