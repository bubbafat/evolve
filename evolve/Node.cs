using System;

namespace evolve
{
    public class Node
    {
        public Node(World world)
        {
            World = world;
            Location = Location.InvalidLocation;
            Id = Guid.NewGuid();
        }
        
        public Guid Id { get; private set; }
        
        public World World { get; private set; }
        public Location Location { get; set; }
    }
}