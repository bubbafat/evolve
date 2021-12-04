using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace evolve
{
    public class Genome
    {
        private readonly List<Gene> _genes;

        private Genome(int genes)
            : this(Network.Builder.CreateRandom(genes))
        {
        }

        private Genome(IEnumerable<Gene> genes)
        {
            _genes = genes.ToList();
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
            var genes = Network.Builder.CreateFromExisting(
                Simulation.GenesPerGenome,
                _genes.Concat(other._genes).ToList())
                .Select(g => g.Mutate());
            
            return new Genome(genes);
        }

        public void Reset()
        {
            _genes.ForEach(g => g.Reset());
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

        public IEnumerable<IAction> GetActions()
        {
            foreach (var g in _genes)
            {
                var action = g.Sink as IAction;
                if (action != null)
                {
                    yield return action;
                }
            }
        }
    }
}