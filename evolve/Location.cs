namespace evolve
{
    public struct Location
    {
        public static readonly Location InvalidLocation = new Location(-1, -1);

        private static readonly Location[,]
            cache = new Location[Simulation.BoardDimensions, Simulation.BoardDimensions];

        
        static Location()
        {
            for (int x = 0; x < Simulation.BoardDimensions; x++)
            for (int y = 0; y < Simulation.BoardDimensions; y++)
            {
                cache[x, y] = new Location(x, y);
            }
        }

        private Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public static Location From(int x, int y)
        {
            return getLocation(x, y);
        }

        private static Location getLocation(int x, int y)
        {
            if (x >= 0 && x < Simulation.BoardDimensions && y >= 0 && y < Simulation.BoardDimensions)
            {
                return cache[x, y];
            }

            return InvalidLocation;
        }

        public Location North()
        {
            return getLocation(X, Y + 1);
        }

        public Location South()
        {
            return getLocation(X, Y - 1);
        }

        public Location East()
        {
            return getLocation(X + 1, Y);
        }

        public Location West()
        {
            return getLocation(X - 1, Y);
        }

        public Location NorthEast()
        {
            return getLocation(X + 1, Y + 1);
        }

        public Location NorthWest()
        {
            return getLocation(X - 1, Y + 1);
        }

        public Location SouthEast()
        {
            return getLocation(X + 1, Y - 1);
        }

        public Location SouthWest()
        {
            return getLocation(X - 1, Y - 1);
        }
    }
}