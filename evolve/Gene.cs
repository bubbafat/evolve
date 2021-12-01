using System;

namespace evolve
{
    public class Gene : IComparable<Gene>
    {
        private readonly IActivatable _source;
        private readonly ISink _sink;

        public IActivatable Source => _source;
        public ISink Sink => _sink;

        public Gene(IActivatable source, ISink sink)
        {
            _source = source;
            _sink = sink;
        }

        public void Evaluate(Node node)
        {
            _sink.UpdateWeight(_source.Activate(node));
        }

        private int sortValue()
        {
            // sensor -> sink = 1
            // inner -> inner = 2
            // inner -> action = 3

            if (Source is ISensor)
            {
                return 1;
            }

            if (Sink is IInnerNeuron)
            {
                return 2;
            }

            return 3;
        }

        public void Reset()
        {
            _sink.Reset();
        }
        
        public int CompareTo(Gene other)
        {
            return sortValue().CompareTo(other.sortValue());
        }

        public override int GetHashCode()
        {
            return Source.GetHashCode() + Sink.GetHashCode();
        }
    }
}