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

        public void Execute(Node node)
        {
            if (Simulation.PerformMostCompellingAction)
            {
                performMostCompellingAction(node);
            }
            else
            {
                performAllActions((node));
            }
        }

        private void performAllActions(Node node)
        {
            foreach (var g in _genes)
            {
                var action = g.Sink as Action;
                if (action != null)
                {
                    action.Act(node);
                }
            }
        }

        private void performMostCompellingAction(Node node)
        {
            var ordered = _genes.Select(g => g.Sink)
                .OfType<Action>()
                .OrderByDescending(a => a.Weight);

            foreach (Action a in ordered)
            {
                if (a.Act(node))
                {
                    break;
                }
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
    }
}