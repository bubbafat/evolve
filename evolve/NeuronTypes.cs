using System;

namespace evolve
{
    public interface IDescribable
    {
        public Guid Id { get; }

        public string Description();
        public int Fingerprint();
    }

    public interface IIdentifiable<TType>
    {
        public TType Type { get; }
    }

    public interface ICopyable<T>
    {
        public T DeepCopy();
    }

    public interface IActivatable : IDescribable, ICopyable<IActivatable>
    {
        public float Activate(Node node);
    }

    public interface ISensor : IActivatable, IIdentifiable<SensorType>
    {
    }

    public interface ISink : IDescribable, ICopyable<ISink>
    {
        public ISink Mutate();
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