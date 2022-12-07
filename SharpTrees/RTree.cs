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

    /// <summary>
    /// Represents bounds in one direction.
    /// </summary>
    internal struct Bounds
    {
        /// <summary>
        /// Gets lower bound.
        /// </summary>
        internal double Min { get; }

        /// <summary>
        /// Gets upper bound.
        /// </summary>
        internal double Max { get; }

        /// <summary>
        /// Gets length.
        /// </summary>
        internal double Length { get => Max - Min; }

        /// <summary>
        /// Creates Bounds instance.
        /// 
        /// min must be less than max. Otherwise IncorrectBoundsExceptions is thrown.
        /// </summary>
        /// <param name="min">Lower bound (minimum).</param>
        /// <param name="max">Upper bound (maximum).</param>
        internal Bounds(double min, double max)
        {
            if (min > max) throw new IncorrectBoundsException("Lower bound is greater than higher.");
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Checks if this bounds overlaps with the other.
        /// </summary>
        /// <param name="other">Other bounds.</param>
        /// <returns>True, if bounds overlaps.</returns>
        internal bool IsOverlapping(Bounds other)
        {
            if (Max < other.Min || Min > other.Max) return false;
            else return true;
        }

        /// <summary>
        /// Gets length of the interval that belongs to both bounds.
        /// </summary>
        /// <param name="other">Other bounds.</param>
        /// <returns>Length of the common interval.</returns>
        internal double GetOverlappingLength(Bounds other)
        {
            double lower = Math.Max(Min, other.Min);
            double upper = Math.Min(Max, other.Max);
            return Math.Max(0, upper - lower);
        }
    }

    /// <summary>
    /// Represents rectangle in multidimensional space. Rectangle sides are
    /// parallel to axes.
    /// </summary>
    internal struct Rectangle
    {
        /// <summary>
        /// Array of bounds of the rectangle in each dimension.
        /// </summary>
        private Bounds[] bounds;

        /// <summary>
        /// Gets the number of dimensions.
        /// </summary>
        internal int DimNumber { get => bounds.Length; }

        /// <summary>
        /// Total area of the rectangle.
        /// </summary>
        internal double Area
        {
            get
            {
                double area = 1.0;
                for (int i = 0; i < DimNumber; ++i)
                {
                    area *= bounds[i].Length;
                }
                return area;
            }
        }

        /// <summary>
        /// Creates uninitialized rectangle with specific number of dimensions.
        /// </summary>
        /// <param name="ndim">The number of dimensions.</param>
        internal Rectangle(int ndim)
        {
            bounds = new Bounds[ndim];
        }

        /// <summary>
        /// Creates rectangle with specified bounds.
        /// </summary>
        /// <param name="bounds">Bounds of the rectangle for each axis.</param>
        internal Rectangle(params Bounds[] bounds)
        {
            this.bounds = new Bounds[bounds.Length];
            for (int i = 0; i < bounds.Length; ++i)
            {
                this.bounds[i] = bounds[i];
            }
        }

        /// <summary>
        /// Gets bounds of the rectangle for the specified axis.
        /// </summary>
        /// <param name="dim">Axis for which bounds are requested.</param>
        /// <returns>Bounds instance.</returns>
        internal Bounds GetBounds(int dim)
        {
            if (dim < 0 || dim >= bounds.Length) throw new IndexOutOfRangeException("Rectangle does not contain requested dimension");
            return bounds[dim];
        }

        /// <summary>
        /// Sets new bounds for the specified axis.
        /// </summary>
        /// <param name="dim">Axis index for which bounds must be set.</param>
        /// <param name="new_bounds">New bounds instance.</param>
        internal void SetBounds(int dim, Bounds new_bounds)
        {
            if (dim < 0 || dim >= bounds.Length) throw new IndexOutOfRangeException("Rectangle does not contain requested dimension");
            bounds[dim] = new_bounds;
        }

        /// <summary>
        /// Checks if this rectangle overlaps with the other.
        /// </summary>
        /// <param name="other">Other rectangle.</param>
        /// <returns>True, if rectangles have overlap.</returns>
        internal bool IsOverlapping(Rectangle other)
        {
            if (DimNumber != other.DimNumber) throw new ArgumentException("Dimension of rectangles do not match.");
            for (int i = 0; i < DimNumber; ++i)
            {
                if (!bounds[i].IsOverlapping(other.GetBounds(i))) return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the area of the intersection of the rectangle with the other.
        /// </summary>
        /// <param name="other">Other rectangle.</param>
        /// <returns>The area of common part. Zero, if rectangles do not overlap.</returns>
        internal double GetOverlappingArea(Rectangle other)
        {
            if (DimNumber != other.DimNumber) throw new ArgumentException("Dimension of rectangles do not match.");
            double area = 1.0;
            for (int i = 0; i < DimNumber; ++i)
            {
                area *= bounds[i].GetOverlappingLength(other.GetBounds(i));
            }
            return area;
        }

        

    }

    public class RTree
    {

    }
}
