using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpTrees
{
    public class IncorrectBoundsException : Exception { 
        public IncorrectBoundsException(string message) : base(message) { }
    }

    internal struct Bounds
    {
        internal double Min { get; }

        internal double Max { get; }

        internal double Length { get => Max - Min; }

        internal Bounds(double min, double max)
        {
            if (min > max) throw new IncorrectBoundsException("Lower bound is greater than higher.");
            Min = min;
            Max = max;
        }

        internal bool IsOverlapping(Bounds other)
        {
            if (Max < other.Min || Min > other.Max) return false;
            else return true;
        }

        internal double GetOverlappingLength(Bounds other)
        {
            double lower = Math.Max(Min, other.Min);
            double upper = Math.Min(Max, other.Max);
            return Math.Max(0, upper - lower);
        }
    }

    internal struct Rectangle
    {
        private Bounds[] bounds;

        internal int DimNumber { get => bounds.Length; }

        internal Rectangle(uint ndim)
        {
            bounds = new Bounds[ndim];
        }

        internal Bounds GetBounds(int dim)
        {
            if (dim < 0 || dim >= bounds.Length) throw new IndexOutOfRangeException("Rectangle does not contain requested dimension");
            return bounds[dim];
        }

        internal void SetBounds(int dim, Bounds new_bounds)
        {
            if (dim < 0 || dim >= bounds.Length) throw new IndexOutOfRangeException("Rectangle does not contain requested dimension");
            bounds[dim] = new_bounds;
        }

        internal bool IsOverlapping(Rectangle other)
        {
            if (DimNumber != other.DimNumber) throw new ArgumentException("Dimension of rectangles do not match.");
            for (int i = 0; i < DimNumber; ++i)
            {
                if (!bounds[i].IsOverlapping(other.GetBounds(i))) return false;
            }
            return true;
        }

        internal double GetOverlappingVolume(Rectangle other)
        {
            if (DimNumber != other.DimNumber) throw new ArgumentException("Dimension of rectangles do not match.");
            double volume = 1.0;
            for (int i = 0; i < DimNumber; ++i)
            {
                volume *= bounds[i].GetOverlappingLength(other.GetBounds(i));
            }
            return volume;
        }

    }

    public class RTree
    {

    }
}
