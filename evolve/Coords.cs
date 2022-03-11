using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace evolve
{
    public class Coords : IComparable<Coords>
    {
        public Coords(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X;
        public int Y;

        public override int GetHashCode()
        {
            int hash = 23;
            hash = hash * 31 + X;
            return hash * 31 + Y;   
        }

        public override string ToString()
        {
            return $"Coord({X},{Y})";
        }

        public override bool Equals(object obj)
        {
            if(obj is Coords coord)
            {
                return CompareTo(coord) == 0;
            }

            return false;
        }

        public int CompareTo(Coords other)
        {
            if (other == null)
                return -1;

            int xcomp = X.CompareTo(other.X);
            if (xcomp != 0)
                return xcomp;

            return Y.CompareTo(other.Y);
        }
    }
}
