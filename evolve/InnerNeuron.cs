using System;

namespace evolve
{
    public class InnerNeuron : IActivatable, ISink
    {
        private float _weight;
        private float _initialWeight;

        public InnerNeuron(float initialWeight)
        {
            Id = Guid.NewGuid();
            _initialWeight = Simulation.ActivationFunction(RNG.Float());
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
                
                _initialWeight = mutateUp
                    ? RNG.Float() * (1f - _initialWeight)
                    : RNG.Float();
            }
        }

        IActivatable IActivatable.DeepCopy()
        {
            return new InnerNeuron(_initialWeight);
        }

        public float Activate(Node node)
        {
            return Simulation.ActivationFunction(_weight);
        }
        
        public void UpdateWeight(float weight)
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