using System;

namespace evolve
{
    public class Node
    {
        private readonly Genome _genome;
        
        public Node(World world, Genome genome)
        {
            _genome = genome;
            World = world;
            Location = Location.InvalidLocation;
            Id = Guid.NewGuid();
        }

        public void Evaluate()
        {
            _genome.Evaluate(this);
        }

        public void Execute()
        {
            _genome.Execute(this);
        }

        public bool MoveRandom()
        {
            return Move(World.RandomEmptyNeighbor(Location));
        }

        public bool Move(Location target)
        {
            return World.MoveNodeAt(Location, target);
        }

        public void Reset()
        {
            _genome.Reset();
        }

        public Node Reproduce(Node other)
        {
            return new Node(this.World, _genome.Reproduce(other._genome));
        }

        public string Description()
        {
            return _genome.Description();
        }
        
        public Guid Id { get; private set; }
        
        public World World { get; private set; }
        public Location Location { get; set; }

        public int Fingerprint()
        {
            return _genome.Fingerprint();
        }
    }
}