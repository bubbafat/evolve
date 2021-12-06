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
    
    public interface IActivatable : IDescribable
    {
        public IActivatable DeepCopy();

        public double Activate(Node node);
    }
    
    public interface ISink : IDescribable
    {
        public ISink DeepCopy();

        public void Mutate();
        public void UpdateWeight(double weight);
        public void Reset();
    }
}