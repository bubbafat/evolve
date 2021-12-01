using System;
using System.Collections.Generic;
using System.Linq;

namespace evolve
{
    public class Genome
    {
        private readonly List<Gene> _genes;
        
        private Genome(int genes)
        {
            _genes = Network.Builder.Create(genes);
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
            foreach (var action in _genes.Where(g => g.Sink is IAction).Select(g => g.Sink as IAction))
            {
                if (action == null)
                    throw new NotSupportedException("BUG! - Action can't be null");
                
                action.Act(node);
            }
        }

        public void Reset()
        {
            _genes.ForEach(g => g.Reset());
        }
    }
}