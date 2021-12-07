using System;

namespace evolve
{
    public class Gene : IComparable<Gene>
    {
        public IActivatable Source { get; internal set; }
        public ISink Sink { get; set; }

        public readonly Guid Id;
        
        public Gene(IActivatable source, ISink sink)
        {
            Id = Guid.NewGuid();
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

        private int SortValue()
        {
            // sensor -> sink = 1
            // inner -> inner (same) = 2
            // inner -> inner (different)
            // inner -> action = 3

            if (Source is Sensor)
            {
                return 1;
            }
            
            // inner -> inner (same)
            // can't be sensor -> inner because we removed sensors already
            if (Sink is InnerNeuron && Source.Id == Sink.Id)
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
            return SortValue().CompareTo(other.SortValue());
        }

        public int Fingerprint()
        {
            return Source.Fingerprint() | Sink.Fingerprint();
        }

        public string Description()
        {
            return $"{Source.Description()} -> {Sink.Description()}";
        }
        
        public Gene Mutate()
        {
            Source.Mutate();
            Sink.Mutate();

            return this;
        }
    }
}