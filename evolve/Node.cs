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

        public void MoveRandom()
        {
            Move(World.RandomEmptyNeighbor(Location));
        }

        public void Move(Location target)
        {
            World.MoveNodeAt(Location, target);
        }

        public void Reset()
        {
            _genome.Reset();
        }
        
        public Guid Id { get; private set; }
        
        public World World { get; private set; }
        public Location Location { get; set; }
    }
}