using System;

namespace evolve
{
    public class InnerNeuron : IActivatable, ISink
    {
        private double _weight;
        private double _initialWeight;

        public InnerNeuron(double initialWeight)
        {
            Id = Guid.NewGuid();
            _initialWeight = Simulation.ActivationFunction(initialWeight);
            _weight = initialWeight;
        }

        ISink ISink.DeepCopy()
        {
            return new InnerNeuron(_initialWeight);
        }

        public void Mutate()
        {
            if (Simulation.WeightToBool(Simulation.MutationChance))
            {
                var mutateUp = RNG.Bool();
                var mutateAmount = RNG.Double();
                
                _initialWeight = mutateUp
                    ? mutateAmount * (1.0 - _initialWeight)
                    : mutateAmount;
            }
        }

        IActivatable IActivatable.DeepCopy()
        {
            return new InnerNeuron(_initialWeight);
        }

        public double Activate(Node node)
        {
            return Simulation.ActivationFunction(_weight);
        }
        
        public void UpdateWeight(double weight)
        {
            _weight += weight;
        }

        public Guid Id { get; }

        public void Reset()
        {
            _weight = _initialWeight;
        }

        public int Fingerprint()
        {
            return 0;
        }

        public string Description()
        {
            return $"InnerNeuron ({Activate(null!)} - {Id})";
        }
    }
}