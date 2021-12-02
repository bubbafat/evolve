using System;
using System.Text;

namespace evolve
{
    public class Gene : IComparable<Gene>
    {
        public IActivatable Source { get; internal set; }
        public ISink Sink { get; set; }

        public Gene(IActivatable source, ISink sink)
        {
            Source = source;
            Sink = sink;
        }
        
        public void Evaluate(Node node)
        {
            Sink.UpdateWeight(Source.Activate(node));
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
            (Source as ISink)?.Reset();
            Sink.Reset();
        }
        
        public int CompareTo(Gene other)
        {
            return sortValue().CompareTo(other.sortValue());
        }

        public int Fingerprint()
        {
            return Source.Fingerprint() | Sink.Fingerprint();
        }

        public string Description()
        {
            StringBuilder sb = new StringBuilder();
            return $"{Source.Description()} -> {Sink.Description()}";

        }

        public void Mutate()
        {
            (Source as IInnerNeuron)?.Mutate();
            Sink.Mutate();
        }
    }
}