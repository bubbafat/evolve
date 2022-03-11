using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace evolve
{
    public class Genome
    {
        public readonly Gene[] Genes;

        private Genome(int genes)
            : this(NetworkBuilder.CreateRandom(genes))
        {
        }

        private Genome(IEnumerable<Gene> genes)
        {
            Genes = genes.ToArray();
        }

        public static Genome CreateRandom(int genes)
        {
            return new Genome(genes);
        }

        public void Evaluate(Node node)
        {
            // this works because the genes are sorted such that sensors evaluate first
            // then inner->inner and then sensor->action and inner->action
            foreach (var gene in Genes)
            {
                gene.Evaluate(node);
            }
        }
        
        public Genome Reproduce(Genome other)
        {
            var genes = NetworkBuilder.CreateFromExisting(
                Genes.Concat(other.Genes).ToList());
                
            var mutated = genes.Select(g => g.Mutate());
            
            return new Genome(mutated);
        }

        public void Reset()
        {
            foreach (var g in Genes)
                g.Reset();
        }

        public string Description()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Gene g in Genes)
            {
                sb.AppendLine(g.Description());
            }

            return sb.ToString();
        }

        public int Fingerprint()
        {
            int fingerPrint = 0;
            foreach (var g in Genes)
            {
                fingerPrint |= g.Fingerprint();
            }

            return fingerPrint;
        }
    }
}