using System;

namespace evolve
{
    public class InnerNeuron : IInnerNeuron
    {
        private float _initialWeight;
        private float _weight;
        private readonly Guid _id;

        public InnerNeuron(float initialWeight)
        {
            _id = Guid.NewGuid();
            _initialWeight = Simulation.ActivationFunction(RNG.Float());
            _weight = initialWeight;
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
        
        public float Activate(Node node)
        {
            return Simulation.ActivationFunction(_weight);
        }

        public float InitialWeight => _initialWeight;

        public void UpdateWeight(float weight)
        {
            _weight += weight;
        }
        
        public Guid Id => _id;

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
            return $"InnerNeuron ({Activate(null)} - {Id})";
        }
    }
}