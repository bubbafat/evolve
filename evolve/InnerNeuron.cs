namespace evolve
{
    public class InnerNeuron : IInnerNeuron
    {
        private readonly float _initialWeight;
        private float _weight;

        public InnerNeuron()
        {
            _initialWeight = RNG.Float();
            _weight = 0f;
        }
        public float Activate(Node node)
        {
            return Simulation.ActivationFunction(_weight);
        }

        public void UpdateWeight(float weight)
        {
            _weight += weight;
        }

        public void Reset()
        {
            _weight = _initialWeight;
        }
    }
}