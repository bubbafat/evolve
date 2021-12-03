using System;

namespace evolve
{
    public class InnerNeuron : IInnerNeuron
    {
        private float _weight;

        public InnerNeuron(float initialWeight)
        {
            Id = Guid.NewGuid();
            InitialWeight = Simulation.ActivationFunction(RNG.Float());
            _weight = initialWeight;
        }
        
        public ISink Mutate()
        {
            if (Simulation.WeightToBool(Simulation.MutationChance))
            {
                var mutateUp = RNG.Bool();
                
                InitialWeight = (mutateUp)
                    ? RNG.Float() * (1f - InitialWeight)
                    : RNG.Float();
            }

            return this;
        }

        public float Activate(Node node)
        {
            return Simulation.ActivationFunction(_weight);
        }

        public float InitialWeight { get; private set; }

        public void UpdateWeight(float weight)
        {
            _weight += weight;
        }

        public Guid Id { get; }

        public void Reset()
        {
            _weight = InitialWeight;
        }

        public int Fingerprint()
        {
            return 0;
        }

        public string Description()
        {
            return $"InnerNeuron ({Activate(null!)} - {Id})";
        }

        IActivatable ICopyable<IActivatable>.DeepCopy()
        {
            return new InnerNeuron(InitialWeight);
        }

        ISink ICopyable<ISink>.DeepCopy()
        {
            return new InnerNeuron(InitialWeight);
        }
    }
}