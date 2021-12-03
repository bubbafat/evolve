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
            LastMoveStep = 0;
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
            if (World.MoveNodeTo(this, target))
            {
                LastMoveStep = Simulation.CurrentStep;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            LastMoveStep = 0;
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

        public int LastMoveStep { get; private set; }

        public Guid Id { get; }

        public World World { get; }
        public Location Location { get; set; }

        public int Fingerprint()
        {
            return _genome.Fingerprint();
        }
    }
}