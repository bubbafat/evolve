using System;

namespace evolve
{
    public record Coords : IComparable<Coords>
    {
        public Coords(int x, int y)
        {
            X = x;
            Y = y;

            int hash = 23;
            hash = hash * 31 + X;
            _hashCode = hash * 31 + Y;
        }

        public readonly int X;
        public readonly int Y;
        private readonly int _hashCode;

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return $"Coord({X},{Y})";
        }

        public int CompareTo(Coords other)
        {
            if (X < other.X)
                return -1;

            if (X > other.X)
                return 1;

            if (Y < other.Y)
                return -1;

            if (Y > other.Y)
                return 1;

            return 0;
        }
    }
}
