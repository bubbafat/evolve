namespace evolve
{
    public struct Location
    {
        public static readonly Location InvalidLocation = new Location(-1, -1);

        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public Location North()
        {
            return new Location(X, Y + 1);
        }

        public Location South()
        {
            return new Location(X, Y - 1);
        }

        public Location East()
        {
            return new Location(X + 1, Y);
        }

        public Location West()
        {
            return new Location(X - 1, Y);
        }

        public Location NorthEast()
        {
            return new Location(X + 1, Y + 1);
        }

        public Location NorthWest()
        {
            return new Location(X - 1, Y + 1);
        }

        public Location SouthEast()
        {
            return new Location(X + 1, Y - 1);
        }

        public Location SouthWest()
        {
            return new Location(X - 1, Y - 1);
        }
    }
}