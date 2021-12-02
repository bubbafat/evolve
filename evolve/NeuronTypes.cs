using System;

namespace evolve
{
    public interface IDescribable
    {
        public string Description();
        public int Fingerprint();
    }
    public interface IIdentifiable<TType>
    {
        public TType Type { get; }
    }
    
    public interface IActivatable : IDescribable
    {
        public float Activate(Node node);
    }

    public interface ISensor : IActivatable, IIdentifiable<SensorType>
    {
    }
    
    public interface ISink : IDescribable
    {
        public void Mutate();
        public float InitialWeight { get; }
        public void UpdateWeight(float weight);
        public void Reset();
    }

    public interface IInnerNeuron : IActivatable, ISink
    {
    }

    public interface IAction : ISink, IIdentifiable<ActionType>
    {
        public bool Act(Node node);
    }
}