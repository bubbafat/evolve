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

        public Gene DeepCopy()
        {
            return new Gene(Source.DeepCopy(), Sink.DeepCopy());
        }

        public void Evaluate(Node node)
        {
            Sink.UpdateWeight(Source.Activate(node));
        }

        private int sortValue()
        {
            // sensor -> sink = 1
            // inner -> inner (same) = 2
            // inner -> inner (different)
            // inner -> action = 3

            if (Source is ISensor)
            {
                return 1;
            }
            
            // inner -> inner (same)
            // can't be sensor -> inner because we removed sensors already
            if (Sink is IInnerNeuron && Source.Id == Sink.Id)
            {
                return 2;
            }

            // inner -> inner (different)
            if (Sink is InnerNeuron)
            {
                return 3;
            }
            
            // inner -> action
            return 4;
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
        
        public Gene Mutate()
        {
            (Source as IInnerNeuron)?.Mutate();
            Sink.Mutate();

            return this;
        }
    }
}