namespace evolve
{
    public class Node
    {
        public Node(World world)
        {
            World = world;
            Location = Location.InvalidLocation;
        }
        
        public World World { get; private set; }
        public Location Location { get; set; }
    }
}