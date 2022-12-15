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
    public struct Bounds
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

        /// <summary>
        /// Gets Bounds that include both this one and other bounds.
        /// </summary>
        /// <param name="other">Other bounds.</param>
        /// <returns>Wide bounds.</returns>
        internal Bounds Merge(Bounds other)
        {
            double lower = Math.Min(Min, other.Min);
            double upper = Math.Max(Max, other.Max);
            return new Bounds(lower, upper);
        }
    }

    /// <summary>
    /// Represents rectangle in multidimensional space. Rectangle sides are
    /// parallel to axes.
    /// </summary>
    internal class Rectangle
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

        /// <summary>
        /// Creates new extended rectangle that includes both this one and the other.
        /// </summary>
        /// <param name="other">Other rectangle.</param>
        /// <returns>Extended rectangle.</returns>
        internal Rectangle Merge(Rectangle other)
        {
            Bounds[] new_bounds = new Bounds[DimNumber];
            for (int i = 0; i < DimNumber; ++i)
            {
                new_bounds[i] = bounds[i].Merge(other.GetBounds(i));
            }
            return new Rectangle(new_bounds);
        }

    }

    public interface IBounded
    {
        Bounds[] GetBounds();
        bool IsEqual(IBounded other);
    }

    public class RTree<T> where T : IBounded
    {
        private Node root;

        private NodeSplitter splitter;

        private uint MaxEntries { get => splitter.MaxEntries; }
        private uint MinEntries { get => splitter.MinEntries; }

        public RTree(byte maxEntries, byte minEntries, NodeSplitStrategy strategy)
        {
            switch (strategy)
            {
                case NodeSplitStrategy.Exhaustive:
                    splitter = new ExhaustiveSlitter(minEntries, maxEntries);
                    break;
                case NodeSplitStrategy.Quadratic:
                    splitter = new QuadraticSplitter(minEntries, maxEntries);
                    break;
                case NodeSplitStrategy.Linear:
                    splitter = new LinearSplitter(minEntries, maxEntries);
                    break;
            }
            root = new LeafNode(splitter);
        }

        /// <summary>
        /// Searches for items that cross the rectangle. 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public ICollection<T> SearchIntersections(Bounds[] bounds)
        {
            Rectangle targetRectangle = new Rectangle(bounds);
            ICollection<LeafEntry> entryResult = new List<LeafEntry>();
            root.SearchIntersections(targetRectangle, entryResult);
            ICollection<T> result = new List<T>();
            foreach (var entry in entryResult) result.Add((T)entry.DataItem);
            return result;
        }

        /// <summary>
        /// Searchs for the specified item.
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <param name="found">Found object</param>
        /// <returns>True, if target object was found, false otherwise.</returns>
        public bool Search(T target, out T found)
        {
            LeafEntry entry = new LeafEntry(target);
            LeafEntry foundEntry = root.Search(entry);
            bool result = false;
            if (foundEntry != null)
            {
                found = (T)foundEntry.DataItem;
                result = true;
            } else
            {
                found = target;
            }
            return result;
        }

        /// <summary>
        /// Deletes item from the RTree.
        /// </summary>
        /// <param name="target">Item to be deleted.</param>
        /// <returns>If the item was deleted the item is returned.
        /// null if RTree does not contain such item.</returns>
        public T Delete(T target)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds new item to the RTree.
        /// </summary>
        /// <param name="item">Item to be inserted.</param>
        public void Add(T item)
        {
            throw new NotImplementedException();
        }

    }

    /// <summary>
    /// Represents RTree node.
    /// </summary>
    internal abstract class Node
    {
        /// <summary>
        /// Maximum number of entries in the node.
        /// </summary>
        protected uint MaxEntries { get => splitter.MaxEntries; }
        /// <summary>
        /// Minimum number of entries in the node.
        /// </summary>
        protected uint MinEntries { get => splitter.MinEntries; }

        /// <summary>
        /// Node splitter object.
        /// </summary>
        protected NodeSplitter splitter;

        protected Node(NodeSplitter splitter)
        {
            this.splitter = splitter;
        }

        /// <summary>
        /// Finds all Leaf entries that have intersections with the target rectangle.
        /// </summary>
        /// <param name="target">Target rectangle.</param>
        /// <param name="foundObjects">Found entries.</param>
        internal abstract void SearchIntersections(Rectangle target, ICollection<LeafEntry> foundObjects);
        
        /// <summary>
        /// Searches for the entry that equals to the target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        internal abstract LeafEntry Search(LeafEntry target);

        /// <summary>
        /// Adds target to the Node.
        /// </summary>
        /// <param name="target">The target entry to be added.</param>
        /// <returns></returns>
        internal abstract Node AddItem(LeafEntry target);

        /// <summary>
        /// Performes correction of bounding rectangles.
        /// </summary>
        internal abstract void AdjustRectangles();

        /// <summary>
        /// Calculates rectangle that bounds all node entries.
        /// </summary>
        /// <returns></returns>
        internal abstract Rectangle GetRectangle();

        /// <summary>
        /// Finds entries, that intersect the target rectangle.
        /// </summary>
        /// <typeparam name="T">Type of Entry: LeafEntry or NodeEntry.</typeparam>
        /// <param name="target">Target rectangle.</param>
        /// <param name="entries">Entries among which the search will be performed.</param>
        /// <returns>List of entries sorted by intersection area: from largest to lowes.</returns>
        protected static List<T> FindIntersectingEntries<T>(Rectangle target, List<T> entries) where T : Entry
        {
            SortedDictionary<double, T> data = new SortedDictionary<double, T>(new AreaDescendingComparer());
            foreach (T entry in entries)
            {
                double area = entry.Rectangle.GetOverlappingArea(target);
                if (area > 0)
                {
                    data.Add(area, entry);
                }
            }
            return data.Values.ToList();
        }

        /// <summary>
        /// Calculates rectangle that bounds the node all entries.
        /// </summary>
        /// <typeparam name="T">Entry type: LeafEntry or NodeEntry.</typeparam>
        /// <param name="entries">Node entries.</param>
        /// <returns>Bounding rectangle.</returns>
        protected static Rectangle GetRectangle<T>(List<T> entries) where T : Entry
        {
            Rectangle result = entries[0].Rectangle;
            for (int i = 1; i < entries.Count; ++i)
            {
                result = result.Merge(entries[i].Rectangle);
            }
            return result;
        }
    }

    /// <summary>
    /// Represents Node that contains only leaf entries.
    /// </summary>
    internal class LeafNode : Node
    {
        private List<LeafEntry> entries;

        internal LeafNode(NodeSplitter splitter) : this(splitter, new List<LeafEntry>())
        { }

        protected LeafNode(NodeSplitter splitter, List<LeafEntry> entries) : base(splitter)
        {
            this.entries = entries;
        }

        internal override LeafEntry Search(LeafEntry target)
        {
            List<LeafEntry> candidates = FindIntersectingEntries<LeafEntry>(target.Rectangle, entries);
            foreach (LeafEntry entry in candidates)
            {
                if (entry.DataItem.IsEqual(target.DataItem)) return entry;
            }
            return null;
        }

        internal override void SearchIntersections(Rectangle target, ICollection<LeafEntry> foundObjects)
        {
            List<LeafEntry> candidates = FindIntersectingEntries<LeafEntry>(target, entries);
            foreach (LeafEntry entry in candidates)
            {
                foundObjects.Add(entry);
            }
        }

        internal override void AdjustRectangles() { }

        internal override Rectangle GetRectangle()
        {
            return GetRectangle(entries);
        }

        internal override Node AddItem(LeafEntry target)
        {
            entries.Add(target);
            if (entries.Count > MaxEntries)
            {
                EntrySplit<LeafEntry> split = splitter.Split(entries);
                entries = split.group1;
                return new LeafNode(splitter, split.group2);
            }
            return null;
        }
    }

    internal class NonLeafNode : Node
    {
        private List<NodeEntry> entries;

        internal NonLeafNode(NodeSplitter splitter) : this(splitter, new List<NodeEntry>())
        { }

        private NonLeafNode(NodeSplitter splitter, List<NodeEntry> entries) : base(splitter)
        {
            this.entries = entries;
        }

        internal override LeafEntry Search(LeafEntry target)
        {
            LeafEntry result = null;
            List<NodeEntry> candidates = FindIntersectingEntries(target.Rectangle, entries);
            foreach (NodeEntry entry in candidates)
            {
                result = entry.Node.Search(target);
                if (result != null) break;
            }
            return result;
        }

        internal override void SearchIntersections(Rectangle target, ICollection<LeafEntry> foundObjects)
        {
            List<NodeEntry> candidates = FindIntersectingEntries(target, entries);
            foreach (NodeEntry entry in candidates)
            {
                entry.Node.SearchIntersections(target, foundObjects);
            }
        }

        internal override void AdjustRectangles()
        {
            foreach (NodeEntry entry in entries)
            {
                entry.Node.AdjustRectangles();
                entry.Rectangle = entry.Node.GetRectangle();
            }
        }

        internal override Rectangle GetRectangle()
        {
            return GetRectangle(entries);
        }

        internal override Node AddItem(LeafEntry target)
        {
            // Find best node
            int i = 0;
            Node afterSplit = entries[i].Node.AddItem(target);
            if (afterSplit != null)
            {
                entries.Add(new NodeEntry(afterSplit));
                if (entries.Count > MaxEntries)
                {
                    EntrySplit<NodeEntry> split = splitter.Split(entries);
                    entries = split.group1;
                    return new NonLeafNode(splitter, split.group2);
                }
            }
            return null;
        }
    }

    internal abstract class Entry
    {
        internal Rectangle Rectangle { get; set; }
    }

    internal class LeafEntry : Entry
    {
        internal IBounded DataItem { get; }

        internal LeafEntry(IBounded dataItem)
        {
            DataItem = dataItem;
            Rectangle = new Rectangle(dataItem.GetBounds());
        }
    }

    internal class NodeEntry : Entry
    {
        internal Node Node { get; set; }

        internal NodeEntry(Node node)
        {
            Node = node;
            Rectangle = node.GetRectangle();
        }
    }

    internal class AreaDescendingComparer : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            return Compare(y, x);
        }
    }

    public enum NodeSplitStrategy
    {
        Exhaustive, Quadratic, Linear
    }

    internal abstract class NodeSplitter
    {
        internal uint MinEntries { get; }
        internal uint MaxEntries { get; }

        internal NodeSplitter(uint minEntries, uint maxEntries)
        {
            MinEntries = minEntries;
            MaxEntries = maxEntries;
        }

        internal abstract EntrySplit<T> Split<T>(List<T> entries) where T : Entry;
    }

    internal class EntrySplit<T> where T : Entry
    {
        internal List<T> group1;
        internal List<T> group2;

        internal EntrySplit(List<T> group1, List<T> group2)
        {
            this.group1 = group1;
            this.group2 = group2;
        }
    }

    internal class ExhaustiveSlitter : NodeSplitter
    {
        internal ExhaustiveSlitter(uint minEntries, uint maxEntries) : base(minEntries, maxEntries) 
        { }

        internal override EntrySplit<T> Split<T>(List<T> entries) 
        {
            List<EntrySplit<T>> splitGroups = FillEntrySplitCases(entries);
            EntrySplit<T> bestCase = splitGroups[0];
            double smallestArea = CalculateCaseArea(splitGroups[0]);
            for (int i = 1; i < splitGroups.Count; ++i)
            {
                double area = CalculateCaseArea(splitGroups[i]);
                if (area < smallestArea)
                {
                    smallestArea = area;
                    bestCase = splitGroups[i];
                }
            }
            return bestCase;
        }

        private static double CalculateCaseArea<T>(EntrySplit<T> splitCase) where T : Entry
        {
            double area1 = CalculateGroupArea(splitCase.group1);
            double area2 = CalculateGroupArea(splitCase.group2);
            return area1 + area2;
        }

        private static double CalculateGroupArea<T>(List<T> group) where T : Entry
        {
            Rectangle rect = group[0].Rectangle;
            for (int i = 1; i < group.Count; ++i)
            {
                rect.Merge(group[i].Rectangle);
            }
            return rect.Area;
        }

        private List<EntrySplit<T>> FillEntrySplitCases<T>(List<T> entries) where T : Entry
        {
            List<EntrySplit<T>> result = new List<EntrySplit<T>>();
            uint m = MinEntries;
            uint N = (uint)entries.Count;
            while (2 * m < MaxEntries)
            {
                GroupSplitIndexProducer iproducer = new GroupSplitIndexProducer(N, m);
                do
                {
                    List<T> group1 = new List<T>();
                    List<T> group2 = new List<T>();
                    foreach (int i in iproducer.group1) group1.Add(entries[i]);
                    foreach (int i in iproducer.group2) group2.Add(entries[i]);
                    result.Add(new EntrySplit<T>(group1, group2));
                } while (iproducer.Next());
                ++m;
            }
            return result;
        }
    }

    internal class GroupSplitIndexProducer
    {
        internal int[] group1;
        internal int[] group2;
        private uint N;
        private int min_index = 0;

        internal GroupSplitIndexProducer(uint N, uint m)
        {
            group1 = new int[m];
            group2 = new int[N - m];
            if (m * 2 == N) min_index = 1;
            this.N = N;
            for (int i = 0; i < m; ++i) group1[i] = i;
            for (int i = 0; i < N - m; ++i) group2[i] = (int)m + i;
        }

        internal bool Next()
        {
            int i = group1.Length - 1;
            group1[i] += 1;
            while (i >= min_index + 1 && group1[i] >= N - (group1.Length - i - 1))
            {
                group1[--i] += 1;
            }
            if (group1[min_index] == N - (group1.Length - min_index - 1)) return false;
            else if (i < group1.Length - 1)
            {
                while (i < group1.Length - 1)
                {
                    group1[i + 1] = group1[i] + 1;
                    ++i;
                }
            }
            SetGroup2();
            return true;
        }

        private void SetGroup2()
        {
            int j = 0;
            int index = 0;
            for (int i = 0; i < group1.Length; ++i)
            {
                while (index < group1[i])
                {
                    group2[j++] = index++;
                }
                ++index;
            }
            for (int i = j; i < group2.Length; ++i) group2[i] = index++;
        }
    }

    internal class QuadraticSplitter : NodeSplitter
    {
        internal QuadraticSplitter(uint minEntries, uint maxEntries) : base(minEntries, maxEntries)
        { }

        internal override EntrySplit<T> Split<T>(List<T> entries)
        {
            throw new NotImplementedException();
        }
    }

    internal class LinearSplitter : NodeSplitter
    {
        internal LinearSplitter(uint minEntries, uint maxEntries) : base(minEntries, maxEntries)
        { }

        internal override EntrySplit<T> Split<T>(List<T> entries)
        {
            throw new NotImplementedException();
        }
    }
}
