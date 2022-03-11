using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace evolve
{
    public class Genome
    {
        internal readonly Gene[] _genes;

        private Genome(int genes)
            : this(NetworkBuilder.CreateRandom(genes))
        {
        }

        private Genome(Gene[] genes)
        {
            _genes = new Gene[genes.Length];
            genes.CopyTo(_genes, 0);
        }

        public static Genome CreateRandom(int genes)
        {
            return new Genome(genes);
        }

        public void Evaluate(Node node)
        {
            // this works because the genes are sorted such that sensors evaluate first
            // then inner->inner and then sensor->action and inner->action
            foreach (var gene in _genes)
            {
                gene.Evaluate(node);
            }
        }
        
        public Genome Reproduce(Genome other)
        {
            var genes = NetworkBuilder.CreateFromExisting(
                _genes.Concat(other._genes).ToList());

            var mutated = genes.Select(g => g.Mutate()).ToArray();
            
            return new Genome(mutated);
        }

        public void Reset()
        {
            foreach(var g in _genes)
            {
                g.Reset();
            }
        }

        public string Description()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Gene g in _genes)
            {
                sb.AppendLine(g.Description());
            }

            return sb.ToString();
        }

        public int Fingerprint()
        {
            int fingerPrint = 0;
            foreach (var g in _genes)
            {
                fingerPrint |= g.Fingerprint();
            }

            return fingerPrint;
        }

        public IEnumerable<Action> GetActions()
        {
            return _genes
                .Select(g => g.Sink)
                .OfType<Action>();
        }
    }
}