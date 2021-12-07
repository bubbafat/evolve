using System;

namespace evolve
{
    public enum Direction
    {
        North,
        South,
        East,
        West
    }
    public interface IDescribable
    {
        public Guid Id { get; }

        public string Description();
        public int Fingerprint();
    }

    public interface IMutable
    {
        public void Mutate();
    }
    
    public interface IActivatable : IDescribable, IMutable
    {
        public IActivatable DeepCopy();

        public double Activate(Node node);
    }
    
    public interface ISink : IDescribable, IMutable
    {
        public ISink DeepCopy();

        public void UpdateWeight(double weight);
        public void Reset();
    }
}