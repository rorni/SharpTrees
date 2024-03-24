using System;
using System.Collections.Generic;
using System.Collections;

namespace SharpTrees
{
    public class IncorrectBoundsException : Exception {
        public IncorrectBoundsException(string message) : base(message) { }
    }

    public class IncorrectLimitsOfEntriesException : Exception
    {
        public IncorrectLimitsOfEntriesException(string message) : base(message) { }
    }

    /// <summary>
    /// Represents bounds in one direction.
    /// </summary>
    public struct Bounds
    {
        /// <summary>
        /// Gets lower bound.
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// Gets upper bound.
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Gets length.
        /// </summary>
        public double Length { get => Max - Min; }

        /// <summary>
        /// Creates Bounds instance.
        /// 
        /// min must be less than max. Otherwise IncorrectBoundsExceptions is thrown.
        /// </summary>
        /// <param name="min">Lower bound (minimum).</param>
        /// <param name="max">Upper bound (maximum).</param>
        public Bounds(double min, double max)
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
        public bool IsOverlapping(Bounds other)
        {
            if (Max < other.Min || Min > other.Max) return false;
            else return true;
        }

        /// <summary>
        /// Gets length of the interval that belongs to both bounds.
        /// </summary>
        /// <param name="other">Other bounds.</param>
        /// <returns>Length of the common interval.</returns>
        public double GetOverlappingLength(Bounds other)
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
        public Bounds Merge(Bounds other)
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

    public class RTree<T> : IEnumerable<T> where T : IBounded
    {
        private Node root;

        private NodeSplitter splitter;

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
        /// <returns>A collection of items that have intersections with specified bounds.</returns>
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
        /// <param name="deleted">Deleted item.</param>
        /// <returns>True, if an item was deleted. False if there is no such item.</returns>
        public bool Delete(T target, out T deleted)
        {
            bool result = false;
            LeafEntry targetEntry = new LeafEntry(target);
            LeafEntry deletedEntry = root.DeleteItem(targetEntry, out List<Node> forReinsert);
            if (deletedEntry != null)
            {
                if (forReinsert != null && forReinsert.Count == 1)
                {
                    root = forReinsert[0];
                }
                deleted = (T)deletedEntry.DataItem;
                result = true;
            } else
            {
                deleted = target;
            }
            return result;
        }

        /// <summary>
        /// Adds new item to the RTree.
        /// </summary>
        /// <param name="item">Item to be inserted.</param>
        /// <returns>True, if the item was added to the tree. False
        /// if such item already exists.</returns>
        public bool Add(T item)
        {
            LeafEntry target = new LeafEntry(item);
            bool result = root.Search(target) == null;
            if (result)
            {
                Node afterSplit = root.AddItem(target);
                if (afterSplit != null)
                {
                    root = new NonLeafNode(splitter, root, afterSplit);
                }
            }
            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (LeafEntry entry in root)
            {
                yield return (T)entry.DataItem;
            }
        }

        /// <summary>
        /// Returns the number of elements stored in the tree.
        /// </summary>
        public int Count { 
            get
            {
                return root.CountItems();
            } 
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Collects data of RTree structure for diagnostics purposes.
        /// 
        /// It collects the level of each LeafEntry items (where actual data is stored).
        /// The length of leafEntryDepth equals to the number of items stored in the RTree.
        /// All its values must be equal.
        /// 
        /// entryCount contains information about the number of entries in each node.
        /// It is a dictionary of level -> the_number_of_entries_in_each_node_at_the_level.
        /// All values in the list must be in range from minEntries to maxEntries, except root Node.
        /// </summary>
        /// <param name="leafEntryDepth"></param>
        /// <param name="entryCount"></param>
        internal void CollectDiagnosticsData(out List<int> leafEntryDepth, out Dictionary<int, List<int>> entryCount)
        {
            leafEntryDepth = new List<int>();
            entryCount = new Dictionary<int, List<int>>();
            root.CollectDiagnosticsData(0, leafEntryDepth, entryCount);
        }
    }

    /// <summary>
    /// Represents RTree node.
    /// </summary>
    internal abstract class Node : IEnumerable<LeafEntry>
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
        /// <returns>Extra node if the node was splitted. null otherwise.</returns>
        internal abstract Node AddItem(LeafEntry target);

        /// <summary>
        /// Counts the number of items stored in the Node or its descendants.
        /// </summary>
        /// <returns>The number of items.</returns>
        internal abstract int CountItems();

        /// <summary>
        /// Deletes target from the Node. 
        /// 
        /// If the number of entries becomes less than minimum value,
        /// the node must be deleted.
        /// </summary>
        /// <param name="target">Item to be deleted.</param>
        /// <param name="forReinsert">All entries that belongs to the node and must be reinserted.</param>
        /// <returns>Entry that was deleted.</returns>
        internal abstract LeafEntry DeleteItem(LeafEntry target, out List<Node> forReinsert);

        /// <summary>
        /// Gets all leaf entries of the node and its descendants.
        /// </summary>
        /// <param name="leafEntries">A list of leaf entries.</param>
        internal abstract void GetAllLeafEntries(List<LeafEntry> leafEntries);

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
        /// <returns>List of entry indices sorted by intersection area: from largest to lowest.</returns>
        protected static List<int> FindIntersectingEntries<T>(Rectangle target, List<T> entries) where T : Entry
        {
            List<AreaIndex> data = new List<AreaIndex>();
            for (int i = 0; i < entries.Count; ++i)
            {
                double area = entries[i].Rectangle.GetOverlappingArea(target);
                if (area > 0)
                {
                    data.Add(new AreaIndex(area, i));
                }
            }
            data.Sort(areaComparer);
            List<int> result = new List<int>();
            foreach (var ai in data) result.Add(ai.index);
            return result;
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

        private struct AreaIndex
        {
            public AreaIndex(double area, int index)
            {
                this.area = area;
                this.index = index;
            }

            public double area;
            public int index;
        }

        private static AreaDescendingComparer areaComparer = new AreaDescendingComparer();

        private class AreaDescendingComparer : IComparer<AreaIndex>
        {
            public int Compare(AreaIndex x, AreaIndex y)
            {
                return y.area.CompareTo(x.area);
            }
        }

        internal abstract void CollectDiagnosticsData(int level, List<int> leafEntryDepth, Dictionary<int, List<int>> entryCount);
        
        public abstract IEnumerator<LeafEntry> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
            List<int> indices = FindIntersectingEntries(target.Rectangle, entries);
            foreach (int i in indices)
            {
                if (entries[i].DataItem.IsEqual(target.DataItem)) return entries[i];
            }
            return null;
        }

        internal override void SearchIntersections(Rectangle target, ICollection<LeafEntry> foundObjects)
        {
            List<int> indices = FindIntersectingEntries(target, entries);
            foreach (int i in indices)
            {
                foundObjects.Add(entries[i]);
            }
        }

        internal override Rectangle GetRectangle()
        {
            return GetRectangle(entries);
        }

        internal override LeafEntry DeleteItem(LeafEntry target, out List<Node> forReinsert)
        {
            List<int> indices = FindIntersectingEntries(target.Rectangle, entries);
            LeafEntry result = null;
            forReinsert = null;
            foreach (int i in indices)
            {
                if (entries[i].DataItem.IsEqual(target.DataItem))
                {
                    result = entries[i];
                    entries.RemoveAt(i);
                    if (entries.Count < MinEntries) forReinsert = new List<Node>() { this };
                    break;
                }
            }
            return result;
        }

        internal override void GetAllLeafEntries(List<LeafEntry> leafEntries)
        {
            foreach (LeafEntry entry in entries)
            {
                leafEntries.Add(entry);
            }
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

        internal override int CountItems()
        {
            return entries.Count;
        }

        internal override void CollectDiagnosticsData(int level, List<int> leafEntryDepth, Dictionary<int, List<int>> entryCount)
        {
            if (!entryCount.ContainsKey(level)) entryCount.Add(level, new List<int>());
            entryCount[level].Add(entries.Count);

            leafEntryDepth.Add(level);
        }

        public override IEnumerator<LeafEntry> GetEnumerator()
        {
            return ((IEnumerable<LeafEntry>)entries).GetEnumerator();
        }
    }

    internal class NonLeafNode : Node
    {
        private List<NodeEntry> entries;

        internal NonLeafNode(NodeSplitter splitter) : this(splitter, new List<NodeEntry>())
        { }

        internal NonLeafNode(NodeSplitter splitter, Node node1, Node node2) : base(splitter)
        {
            entries = new List<NodeEntry>()
            {
                new NodeEntry(node1),
                new NodeEntry(node2)
            };
        }

        private NonLeafNode(NodeSplitter splitter, List<NodeEntry> entries) : base(splitter)
        {
            this.entries = entries;
        }

        internal override LeafEntry Search(LeafEntry target)
        {
            LeafEntry result = null;
            List<int> indices = FindIntersectingEntries(target.Rectangle, entries);
            foreach (int i in indices)
            {
                result = entries[i].Node.Search(target);
                if (result != null) break;
            }
            return result;
        }

        internal override void SearchIntersections(Rectangle target, ICollection<LeafEntry> foundObjects)
        {
            List<int> indices = FindIntersectingEntries(target, entries);
            foreach (int i in indices)
            {
                entries[i].Node.SearchIntersections(target, foundObjects);
            }
        }

        internal override Rectangle GetRectangle()
        {
            return GetRectangle(entries);
        }

        internal override LeafEntry DeleteItem(LeafEntry target, out List<Node> forReinsert)
        {
            List<int> indices = FindIntersectingEntries(target.Rectangle, entries);
            LeafEntry result = null;
            forReinsert = null;
            foreach (int i in indices)
            {
                result = entries[i].Node.DeleteItem(target, out List<Node> extraNodes);
                if (result != null)
                {
                    entries[i].AdjustRectangle();
                    if (extraNodes != null)
                    {
                        entries.RemoveAt(i);
                        forReinsert = ReinsertNodes(extraNodes);
                    }
                    break;
                }
            }
            return result;
        }

        private List<Node> ReinsertNodes(List<Node> forReinsert)
        {
            List<Node> result = null;
            List<LeafEntry> entriesForReinsert = new List<LeafEntry>();
            foreach (Node node in forReinsert) node.GetAllLeafEntries(entriesForReinsert);
            foreach (LeafEntry entry in entriesForReinsert) AddItem(entry);
            if (entries.Count < MinEntries)
            {
                result = new List<Node>();
                foreach (NodeEntry entry in entries) result.Add(entry.Node);
            }
            return result;
        }

        internal override void GetAllLeafEntries(List<LeafEntry> leafEntries)
        {
            foreach (NodeEntry entry in entries)
            {
                entry.Node.GetAllLeafEntries(leafEntries);
            }
        }

        internal override Node AddItem(LeafEntry target)
        {
            Node result = null;
            NodeEntry toAdd = ChooseEntryToInsert(target.Rectangle);
            Node afterSplit = toAdd.Node.AddItem(target);
            if (afterSplit != null)
            {
                entries.Add(new NodeEntry(afterSplit));
                if (entries.Count > MaxEntries)
                {
                    EntrySplit<NodeEntry> split = splitter.Split(entries);
                    entries = split.group1;
                    result = new NonLeafNode(splitter, split.group2);
                }
            }
            toAdd.AdjustRectangle();
            return result;
        }

        internal override int CountItems()
        {
            int result = 0;
            foreach (var entry in entries)
            {
                result += entry.Node.CountItems();
            }
            return result;
        }

        private NodeEntry ChooseEntryToInsert(Rectangle target)
        {
            NodeEntry result = null;
            double smallestEnlargement = double.PositiveInfinity;
            foreach (NodeEntry entry in entries)
            {
                Rectangle r = entry.Rectangle.Merge(target);
                double enlargment = r.Area - entry.Rectangle.Area;
                if (enlargment < smallestEnlargement || 
                    enlargment == smallestEnlargement && 
                    entry.Rectangle.Area < result.Rectangle.Area)
                {
                    smallestEnlargement = enlargment;
                    result = entry;
                }
            }
            return result;
        }

        internal override void CollectDiagnosticsData(int level, List<int> leafEntryDepth, Dictionary<int, List<int>> entryCount)
        {
            if (!entryCount.ContainsKey(level)) entryCount.Add(level, new List<int>());
            entryCount[level].Add(entries.Count);

            foreach (NodeEntry entry in entries)
            {
                entry.Node.CollectDiagnosticsData(level + 1, leafEntryDepth, entryCount);
            }
        }

        public override IEnumerator<LeafEntry> GetEnumerator()
        {
            foreach (NodeEntry nodeEntry in entries)
            {
                Node node = nodeEntry.Node;
                foreach (LeafEntry entry in node)
                {
                    yield return entry;
                }
            }
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

        internal void AdjustRectangle()
        {
            Rectangle = Node.GetRectangle();
        }
    }

    /// <summary>
    /// Strategy to split node during Add operation.
    /// </summary>
    public enum NodeSplitStrategy
    {
        Exhaustive, Quadratic, Linear
    }

    internal abstract class NodeSplitter
    {
        private const string incorrectMinEntriesMessage = "minEntries must not be less than 2. {0} was obtained.";
        private const string incorrectMaxEntriesMessage = "Inequality maxEntries >= 2 * minEntries must hold. But, minEntries={0}, maxEntries={1}.";

        internal uint MinEntries { get; }
        internal uint MaxEntries { get; }

        internal NodeSplitter(uint minEntries, uint maxEntries)
        {
            if (minEntries < 2) {
                throw new IncorrectLimitsOfEntriesException(
                    string.Format(incorrectMinEntriesMessage, minEntries));
            } else if (2 * minEntries > maxEntries)
            {
                throw new IncorrectLimitsOfEntriesException(
                    string.Format(incorrectMaxEntriesMessage, minEntries, maxEntries));
            }

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
            while (2 * m <= MaxEntries)
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
        {
            throw new NotImplementedException();
        }

        internal override EntrySplit<T> Split<T>(List<T> entries)
        {
            throw new NotImplementedException();
        }
    }

    internal class LinearSplitter : NodeSplitter
    {
        internal LinearSplitter(uint minEntries, uint maxEntries) : base(minEntries, maxEntries)
        {
            throw new NotImplementedException();
        }

        internal override EntrySplit<T> Split<T>(List<T> entries)
        {
            throw new NotImplementedException();
        }
    }
}
