using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SharpTrees;

namespace SharpTreesTest
{
    [TestClass]
    public class RTreeTest
    {
        private Point[] GeneratePoints(int nx, int ny, double dx, double dy, double h)
        {
            int count = nx * ny;
            Point[] result = new Point[count];

            double x = 0;
            double y = 0;
            for (int i = 0; i < nx; ++i)
            {
                for (int j = 0; j < ny; ++j)
                {
                    int index = i * nx + j;
                    result[index] = new Point(x + i * dx, y + j * dy, h, index);
                }
            }
            return result;
        }

        private Point[] points;

        private void AssertBalance<T>(RTree<T> rtree, byte M, byte m) where T : IBounded
        {
            rtree.CollectDiagnosticsData(out List<int> leafEntryDepth, out Dictionary<int, List<int>> entryCount);
            int level = leafEntryDepth[0];
            for (int i = 1; i < leafEntryDepth.Count; ++i) Assert.AreEqual(level, leafEntryDepth[i]);
            int lower_bound;
            foreach (KeyValuePair<int, List<int>> kvp in entryCount)
            {
                if (level == 0) lower_bound = 0;
                else if (kvp.Key == 0) lower_bound = 2;
                else lower_bound = m;
                foreach (int value in kvp.Value)
                {
                    Assert.IsTrue(value >= lower_bound);
                    Assert.IsTrue(value <= M);
                }
            }
        }

        public RTreeTest()
        {
            points = GeneratePoints(30, 30, 1, 1, 0.1);
        }

        [TestMethod]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 0, 10)]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 0, 100)]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 0, 900)]
        [DataRow((byte)5, (byte)2, NodeSplitStrategy.Exhaustive, 0, 10)]
        [DataRow((byte)5, (byte)2, NodeSplitStrategy.Exhaustive, 0, 100)]
        [DataRow((byte)5, (byte)2, NodeSplitStrategy.Exhaustive, 0, 900)]
        [DataRow((byte)6, (byte)3, NodeSplitStrategy.Exhaustive, 0, 10)]
        [DataRow((byte)6, (byte)3, NodeSplitStrategy.Exhaustive, 0, 100)]
        [DataRow((byte)6, (byte)3, NodeSplitStrategy.Exhaustive, 0, 900)]
        public void TestAdd(byte M, byte m, NodeSplitStrategy strategy, int start, int end)
        {
            RTree<Point> rtree = new RTree<Point>(M, m, strategy);
            for (int i = start; i < end; ++i)
            {
                rtree.Add(points[i]);
                AssertBalance(rtree, M, m);
            }
        }

        [TestMethod]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 0, 10)]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 0, 100)]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 0, 900)]
        [DataRow((byte)5, (byte)2, NodeSplitStrategy.Exhaustive, 5, 10)]
        [DataRow((byte)5, (byte)2, NodeSplitStrategy.Exhaustive, 50, 100)]
        [DataRow((byte)5, (byte)2, NodeSplitStrategy.Exhaustive, 300, 900)]
        [DataRow((byte)6, (byte)3, NodeSplitStrategy.Exhaustive, 0, 10)]
        [DataRow((byte)6, (byte)3, NodeSplitStrategy.Exhaustive, 0, 100)]
        [DataRow((byte)6, (byte)3, NodeSplitStrategy.Exhaustive, 700, 900)]
        public void TestSearch(byte M, byte m, NodeSplitStrategy strategy, int start, int end)
        {
            RTree<Point> rtree = new RTree<Point>(M, m, strategy);
            for (int i = start; i < end; ++i) rtree.Add(points[i]);

            for (int i = 0; i < points.Length; ++i)
            {
                bool expectToFind = i >= start && i < end;
                bool found = rtree.Search(points[i], out Point foundPoint);
                Assert.AreEqual(expectToFind, found);
                if (expectToFind) Assert.IsTrue(points[i].IsEqual(foundPoint));
            }
        }

        private List<int> GetPointIndex(double xmin, double xmax, double ymin, double ymax)
        {
            List<int> result = new List<int>();
            for (int i = 0; i < points.Length; ++i)
            {
                Point p = points[i];
                if (p.X >= xmin && p.X <= xmax && p.Y >= ymin && p.Y <= ymax)
                {
                    result.Add(i);
                }
            }
            result.Sort();
            return result;
        }

        [TestMethod]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 10.5, 20.5, 5.5, 25.5)]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, -10.5, -5.5, 5.5, 25.5)]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 12.5, 18.5, 5.5, 25.5)]
        public void TestSearchIntersections(byte M, byte m, NodeSplitStrategy strategy, 
            double xmin, double xmax, double ymin, double ymax)
        {
            RTree<Point> rtree = new RTree<Point>(M, m, strategy);
            foreach (Point p in points) rtree.Add(p);

            List<int> pointsIndexInside = GetPointIndex(xmin, xmax, ymin, ymax);
            
            Bounds[] bounds = new Bounds[] { new Bounds(xmin, xmax), new Bounds(ymin, ymax) };
            ICollection<Point> foundPoints = rtree.SearchIntersections(bounds);

            List<int> foundIndices = new List<int>();
            foreach (var p in foundPoints) foundIndices.Add(p.Index);
            foundIndices.Sort();

            Assert.AreEqual(pointsIndexInside.Count, foundPoints.Count);
            for (int i = 0; i < pointsIndexInside.Count; ++i)
            {
                Assert.AreEqual(pointsIndexInside[i], foundIndices[i]);
            }
        }

        [TestMethod]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 0, 10, 4, 8)]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 0, 100, 1, 100)]
        [DataRow((byte)4, (byte)2, NodeSplitStrategy.Exhaustive, 0, 900, 100, 890)]
        [DataRow((byte)5, (byte)2, NodeSplitStrategy.Exhaustive, 5, 10, 0, 15)]
        [DataRow((byte)5, (byte)2, NodeSplitStrategy.Exhaustive, 50, 100, 50, 99)]
        [DataRow((byte)5, (byte)2, NodeSplitStrategy.Exhaustive, 300, 900, 800, 900)]
        [DataRow((byte)6, (byte)3, NodeSplitStrategy.Exhaustive, 0, 10, 8, 11)]
        [DataRow((byte)6, (byte)3, NodeSplitStrategy.Exhaustive, 0, 100, 0, 90)]
        [DataRow((byte)6, (byte)3, NodeSplitStrategy.Exhaustive, 700, 900, 800, 830)]
        public void TestDelete(byte M, byte m, NodeSplitStrategy strategy, int start, int end, int del_start, int del_end)
        {
            RTree<Point> rtree = new RTree<Point>(M, m, strategy);
            for (int i = start; i < end; ++i) rtree.Add(points[i]);

            for (int i = del_start; i < del_end; ++i)
            {
                bool result = rtree.Delete(points[i], out Point deletedPoint);
                bool expectToDelete = i >= start && i < end;
                Assert.AreEqual(expectToDelete, result);
                if (expectToDelete) {
                    Assert.IsTrue(points[i].IsEqual(deletedPoint));
                    AssertBalance(rtree, M, m);
                }
            }
        }

        [TestMethod]
        [DataRow((byte)0, (byte)4, NodeSplitStrategy.Exhaustive)]
        [DataRow((byte)1, (byte)4, NodeSplitStrategy.Exhaustive)]
        [DataRow((byte)2, (byte)3, NodeSplitStrategy.Exhaustive)]
        [DataRow((byte)3, (byte)4, NodeSplitStrategy.Exhaustive)]
        [DataRow((byte)3, (byte)5, NodeSplitStrategy.Exhaustive)]
        [DataRow((byte)0, (byte)1, NodeSplitStrategy.Exhaustive)]
        [DataRow((byte)1, (byte)2, NodeSplitStrategy.Exhaustive)]
        public void TestIncorrectLimitsOfEntriesException(byte minEntries, byte maxEntries, NodeSplitStrategy strategy)
        {
            Assert.ThrowsException<IncorrectLimitsOfEntriesException>(() => new RTree<Point>(maxEntries, minEntries, strategy));
        }
    }

    internal class Point : IBounded
    {
        internal double X { get; }
        internal double Y { get; }
        internal double H { get; }

        internal int Index { get; }

        internal Point(double x, double y, double h)
        {
            X = x;
            Y = y;
            H = h;
        }

        internal Point(double x, double y, double h, int index) : this(x, y, h)
        {
            Index = index;
        }

        public Bounds[] GetBounds()
        {
            Bounds[] result = new Bounds[2];
            result[0] = new Bounds(X - H, X + H);
            result[1] = new Bounds(Y - H, Y + H);
            return result;
        }

        public bool IsEqual(IBounded other)
        {
            Point otherPoint = other as Point;
            if (Math.Abs(X - otherPoint.X) >= H) return false;
            return Math.Abs(Y - otherPoint.Y) < H;
        }
    }

    [TestClass]
    public class LeafNodeTest
    {
        internal static LeafNode CreateLeafNode(int number)
        {
            NodeSplitter splitter = null;
            switch (number)
            {
                case 0:
                    splitter = new ExhaustiveSlitter(2, 4);
                    break;
                case 1:
                    splitter = new ExhaustiveSlitter(2, 5);
                    break;
                case 2:
                    splitter = new ExhaustiveSlitter(2, 6);
                    break;
                case 3:
                    splitter = new ExhaustiveSlitter(3, 6);
                    break;
            }
            return new LeafNode(splitter);
        }

        internal static Point[] points =
        {
            new Point(1, 1, 0.1),
            new Point(1, 2, 0.1),
            new Point(2, 1, 0.1),
            new Point(2, 2, 0.1),
            new Point(3, 1, 0.1),
            new Point(1, 3, 0.1),
            new Point(3, 3, 0.1)
        };

        [TestMethod]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 0, true, false)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 1, true, false)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 2, true, false)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 3, true, false)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 4, false, false)]
        [DataRow(0, new int[] { 0, 1, 2 }, 2, true, false)]
        [DataRow(0, new int[] { 0, 1 }, 1, true, true)]
        [DataRow(0, new int[] { 0, 1 }, 2, false, false)]
        public void TestDeleteItem(int si, int[] add_index, int delete_index, bool wasDeleted, bool hasReinsert)
        {
            LeafNode node = CreateLeafNode(si);
            foreach (int i in add_index) node.AddItem(new LeafEntry(points[i]));
            LeafEntry targetEntry = new LeafEntry(points[delete_index]);
            LeafEntry deletedEntry = node.DeleteItem(targetEntry, out List<Node> forReinsert);
            if (wasDeleted) Assert.IsTrue(targetEntry.DataItem.IsEqual(deletedEntry.DataItem));
            else Assert.IsNull(deletedEntry);
            Assert.AreEqual(hasReinsert, forReinsert != null);
            LeafEntry result = node.Search(new LeafEntry(points[delete_index]));
            Assert.IsNull(result);
        }

        [TestMethod]
        [DataRow(0, new int[] {0}, 0.9, 1.1, 0.9, 1.1)]
        [DataRow(0, new int[] { 0, 1, 4 }, 0.9, 3.1, 0.9, 2.1)]
        [DataRow(0, new int[] { 3, 6 }, 1.9, 3.1, 1.9, 3.1)]
        public void TestGetRectangle(int si, int[] add_index, double xmin, double xmax, double ymin, double ymax)
        {
            LeafNode node = CreateLeafNode(si);
            foreach (int i in add_index) node.AddItem(new LeafEntry(points[i]));
            Rectangle rect = node.GetRectangle();
            Bounds xbounds = rect.GetBounds(0);
            Bounds ybounds = rect.GetBounds(1);
            Assert.AreEqual(xmin, xbounds.Min);
            Assert.AreEqual(xmax, xbounds.Max);
            Assert.AreEqual(ymin, ybounds.Min);
            Assert.AreEqual(ymax, ybounds.Max);
        }

        [TestMethod]
        [DataRow(0, new int[] { 0 }, 0, true)]
        [DataRow(0, new int[] { 0 }, 1, false)]
        [DataRow(0, new int[] { }, 0, false)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 0, true)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 1, true)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 2, true)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 3, true)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 4, false)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 5, false)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 6, false)]
        public void TestSearch(int si, int[] add_index, int searchIndex, bool hasResult)
        {
            LeafNode node = CreateLeafNode(si);
            foreach (int i in add_index) node.AddItem(new LeafEntry(points[i]));
            LeafEntry result = node.Search(new LeafEntry(points[searchIndex]));
            Assert.AreEqual(hasResult, result != null);
            if (hasResult)
            {
                Assert.IsTrue(result.DataItem.IsEqual(points[searchIndex]));
            }

        }

        [TestMethod]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 0.99, 2.01, 0.99, 2.01, new int[] {0, 1, 2, 3})]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 0.99, 2.01, 0.99, 1.01, new int[] {0, 1})]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 1.99, 2.01, 0.99, 1.01, new int[] { 2 })]
        public void TestSearchIntersections(int si, int[] add_index, double xmin, double xmax, double ymin, double ymax, int[] found_index)
        {
            LeafNode node = CreateLeafNode(si);
            foreach (int i in add_index) node.AddItem(new LeafEntry(points[i]));
            Rectangle rect = new Rectangle(new Bounds(xmin, xmax), new Bounds(ymin, ymax));
            List<LeafEntry> searchResult = new List<LeafEntry>();
            node.SearchIntersections(rect, searchResult);

            Assert.AreEqual(found_index.Length, searchResult.Count);
        }

        [TestMethod]
        [DataRow(0, new int[] { 0 }, 1, false)]
        [DataRow(0, new int[] { 0, 1, 2, 3 }, 4, true)]
        [DataRow(0, new int[] { 1, 2, 3, 4 }, 5, true)]
        [DataRow(1, new int[] { 0, 1, 2, 3 }, 4, false)]
        [DataRow(1, new int[] { 0, 1, 2, 3, 4 }, 5, true)]
        public void TestAddItem(int si, int[] initial_add, int final_add, bool hasSplit)
        {
            LeafNode node = CreateLeafNode(si);
            foreach (int i in initial_add) node.AddItem(new LeafEntry(points[i]));
            Node splitNode = node.AddItem(new LeafEntry(points[final_add]));
            Assert.AreEqual(hasSplit, splitNode != null);
            if (hasSplit)
            {
                List<int> index = new List<int>(initial_add);
                index.Add(final_add);
                foreach (int i in index)
                {
                    LeafEntry search1 = node.Search(new LeafEntry(points[i]));
                    LeafEntry search2 = splitNode.Search(new LeafEntry(points[i]));
                    Assert.IsTrue(search1 != null ^ search2 != null);
                }
            }
        }
    }

    [TestClass]
    public class BoundsTest
    {
        [TestMethod]
        [DataRow(1.0, 2.0, 1.0)]
        [DataRow(-3.4, 5.6, 9.0)]
        [DataRow(-3, -1, 2)]
        [DataRow(1000, 2000, 1000)]
        public void TestCreation(double min, double max, double length)
        {
            Bounds bounds = new Bounds(min, max);
            Assert.AreEqual(min, bounds.Min);
            Assert.AreEqual(max, bounds.Max);
            Assert.AreEqual(length, bounds.Length);
        }

        [TestMethod]
        [DataRow(3, -2)]
        [DataRow(-4, -5)]
        [DataRow(6, 3)]
        public void TestCreationFailed(double min, double max)
        {
            Assert.ThrowsException<IncorrectBoundsException>(() => new Bounds(min, max));
        }

        [TestMethod]
        [DataRow(0, 2, 0.5, 1.5, true)]
        [DataRow(0, 2, 1, 2.5, true)]
        [DataRow(0, 2, 2, 3, true)]
        [DataRow(1, 2, -1, 4, true)]
        [DataRow(1, 3, -1, 1, true)]
        [DataRow(1, 3, 4, 5, false)]
        [DataRow(1, 3, -1, 0, false)]
        public void TestIsOverlapping(double min1, double max1, double min2, double max2, bool result)
        {
            Bounds bounds1 = new Bounds(min1, max1);
            Bounds bounds2 = new Bounds(min2, max2);
            Assert.AreEqual(result, bounds1.IsOverlapping(bounds2));
            Assert.AreEqual(result, bounds2.IsOverlapping(bounds1));
        }

        [TestMethod]
        [DataRow(0, 2, 0.5, 1.5, 1.0)]
        [DataRow(0, 2, 1, 2.5, 1.0)]
        [DataRow(0, 2, 2, 3, 0)]
        [DataRow(0, 2, -1, 4, 2)]
        [DataRow(1, 3, -1, 1, 0)]
        [DataRow(4, 16, -3, 12, 8)]
        public void TestOverlappingLength(double min1, double max1, double min2, double max2, double result)
        {
            Bounds bounds1 = new Bounds(min1, max1);
            Bounds bounds2 = new Bounds(min2, max2);
            Assert.AreEqual(result, bounds1.GetOverlappingLength(bounds2), 1.0e-10);
            Assert.AreEqual(result, bounds2.GetOverlappingLength(bounds1), 1.0e-10);
        }

        [TestMethod]
        [DataRow(-1, 3, 0, 2, -1, 3)]
        [DataRow(-1, 3, 2, 4, -1, 4)]
        [DataRow(-1, 3, 4, 5, -1, 5)]
        [DataRow(-5, -1, 1, 5, -5, 5)]
        public void TestMerge(double min1, double max1, double min2, double max2, double minr, double maxr)
        {
            Bounds bounds1 = new Bounds(min1, max1);
            Bounds bounds2 = new Bounds(min2, max2);
            Bounds result = bounds1.Merge(bounds2);
            Assert.AreEqual(minr, result.Min, 1.0e-10);
            Assert.AreEqual(maxr, result.Max, 1.0e-10);
        }
    }

    [TestClass]
    public class RectangleTest
    {
        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        public void TestCreateVoidRectangle(int ndim)
        {
            Rectangle rect = new Rectangle(ndim);
            Assert.AreEqual(ndim, rect.DimNumber);
        }

        [TestMethod]
        [DataRow(1, new double[] { 1.0, 3.0 })]
        [DataRow(2, new double[] { 1.0, 3.0, -2, 4 })]
        [DataRow(3, new double[] { 1.0, 3.0, -2, 4, 8, 13 })]
        public void TestCreateRectange(int ndim, double[] bound_values)
        {
            Bounds[] bounds = new Bounds[ndim];
            for (int i = 0; i < ndim; ++i)
            {
                bounds[i] = new Bounds(bound_values[2 * i], bound_values[2 * i + 1]);
            }
            Rectangle rect = new Rectangle(bounds);
            Assert.AreEqual(ndim, rect.DimNumber);

            for (int i = 0; i < ndim; ++i)
            {
                Bounds b = rect.GetBounds(i);
                Assert.AreEqual(bound_values[2 * i], b.Min, 1.0e-10);
                Assert.AreEqual(bound_values[2 * i + 1], b.Max, 1.0e-10);
            }
        }

        [TestMethod]
        [DataRow(1, 0, 5, 16)]
        [DataRow(5, 0, 3, 8)]
        [DataRow(5, 4, 8, 19)]
        [DataRow(5, 2, -1, 3)]
        public void TestSetBounds(int dim, int i, double min, double max)
        {
            Bounds to_set = new Bounds(min, max);
            Rectangle rect = new Rectangle(dim);
            rect.SetBounds(i, to_set);
            Bounds got = rect.GetBounds(i);
            Assert.AreEqual(min, got.Min, 1.0e-10);
            Assert.AreEqual(max, got.Max, 1.0e-10);
        }

        [TestMethod]
        [DataRow(0, 1, true)]
        [DataRow(0, 2, true)]
        [DataRow(0, 3, true)]
        [DataRow(0, 4, true)]
        [DataRow(0, 5, true)]
        [DataRow(1, 2, false)]
        [DataRow(1, 3, false)]
        [DataRow(1, 4, true)]
        [DataRow(1, 5, true)]
        [DataRow(2, 3, false)]
        [DataRow(2, 4, false)]
        [DataRow(2, 5, false)]
        [DataRow(3, 4, false)]
        [DataRow(3, 5, false)]
        [DataRow(4, 5, false)]
        [DataRow(6, 7, true)]
        public void TestIsOverlapping(int i1, int i2, bool result)
        {
            Rectangle rect1 = GetRectangle(i1);
            Rectangle rect2 = GetRectangle(i2);
            Assert.AreEqual(result, rect1.IsOverlapping(rect2));
            Assert.AreEqual(result, rect2.IsOverlapping(rect1));
        }

        [TestMethod]
        [DataRow(0, 1, 12)]
        [DataRow(0, 2, 0)]
        [DataRow(0, 3, 0)]
        [DataRow(0, 4, 2)]
        [DataRow(0, 5, 12)]
        [DataRow(1, 2, 0)]
        [DataRow(1, 3, 0)]
        [DataRow(1, 4, 0)]
        [DataRow(1, 5, 6)]
        [DataRow(2, 3, 0)]
        [DataRow(2, 4, 0)]
        [DataRow(2, 5, 0)]
        [DataRow(3, 4, 0)]
        [DataRow(3, 5, 0)]
        [DataRow(4, 5, 0)]
        [DataRow(6, 7, 9)]
        public void TestGetOverlappingArea(int i1, int i2, double result)
        {
            Rectangle rect1 = GetRectangle(i1);
            Rectangle rect2 = GetRectangle(i2);
            Assert.AreEqual(result, rect1.GetOverlappingArea(rect2), 1.0e-10);
            Assert.AreEqual(result, rect2.GetOverlappingArea(rect1), 1.0e-10);
        }

        [TestMethod]
        [DataRow(0, 36)]
        [DataRow(1, 12)]
        [DataRow(2, 6)]
        [DataRow(3, 6)]
        [DataRow(4, 9)]
        [DataRow(5, 60)]
        [DataRow(6, 54)]
        [DataRow(7, 18)]
        public void TestRectangleArea(int i, double area)
        {
            Rectangle rect = GetRectangle(i);
            Assert.AreEqual(area, rect.Area, 1.0e-10);
        }

        [TestMethod]
        [DataRow(0, 1, 36)]
        [DataRow(0, 2, 72)]
        [DataRow(0, 3, 60)]
        [DataRow(0, 4, 60)]
        [DataRow(0, 5, 180)]
        [DataRow(1, 2, 50)]
        [DataRow(1, 3, 54)]
        [DataRow(1, 4, 40)]
        [DataRow(1, 5, 120)]
        [DataRow(2, 3, 91)]
        [DataRow(2, 4, 40)]
        [DataRow(2, 5, 200)]
        [DataRow(3, 4, 77)]
        [DataRow(3, 5, 120)]
        [DataRow(4, 5, 160)]
        [DataRow(6, 7, 80)]
        public void TestRectangleMerge(int i1, int i2, double area)
        {
            Rectangle rect1 = GetRectangle(i1);
            Rectangle rect2 = GetRectangle(i2);
            Rectangle rect = rect1.Merge(rect2);
            Assert.AreEqual(area, rect.Area, 1.0e-10);
            rect = rect2.Merge(rect1);
            Assert.AreEqual(area, rect.Area, 1.0e-10);
        }

        private List<Rectangle> rectangles = null;

        private void GenerateRectangles()
        {
            rectangles = new List<Rectangle>();

            Bounds[] bounds1 = new Bounds[] { new Bounds(1, 10), new Bounds(-2, 2) };
            rectangles.Add(new Rectangle(bounds1));

            Bounds[] bounds2 = new Bounds[] { new Bounds(2, 8), new Bounds(-1, 1) };
            rectangles.Add(new Rectangle(bounds2));

            Bounds[] bounds3 = new Bounds[] { new Bounds(-2, 1), new Bounds(-4, -2) };
            rectangles.Add(new Rectangle(bounds3));

            Bounds[] bounds4 = new Bounds[] { new Bounds(10, 11), new Bounds(-3, 3) };
            rectangles.Add(new Rectangle(bounds4));

            Bounds[] bounds5 = new Bounds[] { new Bounds(0, 3), new Bounds(1, 4) };
            rectangles.Add(new Rectangle(bounds5));

            Bounds[] bounds6 = new Bounds[] { new Bounds(5, 8), new Bounds(-10, 10) };
            rectangles.Add(new Rectangle(bounds6));

            // 3D rectangles
            Bounds[] bounds7 = new Bounds[] { new Bounds(1, 10), new Bounds(2, 4), new Bounds(-4, -1) };
            rectangles.Add(new Rectangle(bounds7));

            Bounds[] bounds8 = new Bounds[] { new Bounds(0, 4), new Bounds(2, 3.5), new Bounds(-3, 0) };
            rectangles.Add(new Rectangle(bounds8));
        }

        private Rectangle GetRectangle(int i)
        {
            if (rectangles == null) GenerateRectangles();
            return rectangles[i];
        }
    }

    [TestClass]
    public class GroupSplitIndexProducerTest
    {
        private class TestCase
        {
            public List<int[]> group1 = new List<int[]>();
            public List<int[]> group2 = new List<int[]>();

            public TestCase(uint N, uint m)
            {
                if (N == 4 && m == 2) Create4_2();
                else if (N == 5 && m == 2) Create5_2();
                else if (N == 6 && m == 2) Create6_2();
                else if (N == 6 && m == 3) Create6_3();
            }

            private void Create4_2()
            {
                group1.Add(new int[] { 0, 1 });
                group1.Add(new int[] { 0, 2 });
                group1.Add(new int[] { 0, 3 });
                group2.Add(new int[] { 2, 3 });
                group2.Add(new int[] { 1, 3 });
                group2.Add(new int[] { 1, 2 });
            }

            private void Create5_2()
            {
                group1.Add(new int[] { 0, 1 });
                group1.Add(new int[] { 0, 2 });
                group1.Add(new int[] { 0, 3 });
                group1.Add(new int[] { 0, 4 });
                group1.Add(new int[] { 1, 2 });
                group1.Add(new int[] { 1, 3 });
                group1.Add(new int[] { 1, 4 });
                group1.Add(new int[] { 2, 3 });
                group1.Add(new int[] { 2, 4 });
                group1.Add(new int[] { 3, 4 });
                group2.Add(new int[] { 2, 3, 4 });
                group2.Add(new int[] { 1, 3, 4 });
                group2.Add(new int[] { 1, 2, 4 });
                group2.Add(new int[] { 1, 2, 3 });
                group2.Add(new int[] { 0, 3, 4 });
                group2.Add(new int[] { 0, 2, 4 });
                group2.Add(new int[] { 0, 2, 3 });
                group2.Add(new int[] { 0, 1, 4 });
                group2.Add(new int[] { 0, 1, 3 });
                group2.Add(new int[] { 0, 1, 2 });
            }

            private void Create6_2()
            {
                group1.Add(new int[] { 0, 1 });
                group1.Add(new int[] { 0, 2 });
                group1.Add(new int[] { 0, 3 });
                group1.Add(new int[] { 0, 4 });
                group1.Add(new int[] { 0, 5 });
                group1.Add(new int[] { 1, 2 });
                group1.Add(new int[] { 1, 3 });
                group1.Add(new int[] { 1, 4 });
                group1.Add(new int[] { 1, 5 });
                group1.Add(new int[] { 2, 3 });
                group1.Add(new int[] { 2, 4 });
                group1.Add(new int[] { 2, 5 });
                group1.Add(new int[] { 3, 4 });
                group1.Add(new int[] { 3, 5 });
                group1.Add(new int[] { 4, 5 });
                group2.Add(new int[] { 2, 3, 4, 5 });
                group2.Add(new int[] { 1, 3, 4, 5 });
                group2.Add(new int[] { 1, 2, 4, 5 });
                group2.Add(new int[] { 1, 2, 3, 5 });
                group2.Add(new int[] { 1, 2, 3, 4 });
                group2.Add(new int[] { 0, 3, 4, 5 });
                group2.Add(new int[] { 0, 2, 4, 5 });
                group2.Add(new int[] { 0, 2, 3, 5 });
                group2.Add(new int[] { 0, 2, 3, 4 });
                group2.Add(new int[] { 0, 1, 4, 5 });
                group2.Add(new int[] { 0, 1, 3, 5 });
                group2.Add(new int[] { 0, 1, 3, 4 });
                group2.Add(new int[] { 0, 1, 2, 5 });
                group2.Add(new int[] { 0, 1, 2, 4 });
                group2.Add(new int[] { 0, 1, 2, 3 });
            }
            private void Create6_3()
            {
                group1.Add(new int[] { 0, 1, 2 });
                group1.Add(new int[] { 0, 1, 3 });
                group1.Add(new int[] { 0, 1, 4 });
                group1.Add(new int[] { 0, 1, 5 });
                group1.Add(new int[] { 0, 2, 3 });
                group1.Add(new int[] { 0, 2, 4 });
                group1.Add(new int[] { 0, 2, 5 });
                group1.Add(new int[] { 0, 3, 4 });
                group1.Add(new int[] { 0, 3, 5 });
                group1.Add(new int[] { 0, 4, 5 });

                group2.Add(new int[] { 3, 4, 5 });
                group2.Add(new int[] { 2, 4, 5 });
                group2.Add(new int[] { 2, 3, 5 });
                group2.Add(new int[] { 2, 3, 4 });
                group2.Add(new int[] { 1, 4, 5 });
                group2.Add(new int[] { 1, 3, 5 });
                group2.Add(new int[] { 1, 3, 4 });
                group2.Add(new int[] { 1, 2, 5 });
                group2.Add(new int[] { 1, 2, 4 });
                group2.Add(new int[] { 1, 2, 3 });
            }
        }


        [TestMethod]
        [DataRow((uint)4, (uint)2, 3)]
        [DataRow((uint)5, (uint)2, 10)]
        [DataRow((uint)6, (uint)2, 15)]
        [DataRow((uint)6, (uint)3, 10)]
        public void TestIndexProducer(uint N, uint m, int casenum)
        {
            GroupSplitIndexProducer producer = new GroupSplitIndexProducer(N, m);
            TestCase answer = new TestCase(N, m);
            for (int i = 0; i < casenum; ++i)
            {
                CollectionAssert.AreEqual(answer.group1[i], producer.group1);
                CollectionAssert.AreEqual(answer.group2[i], producer.group2);
                bool next = producer.Next();
                if (i != casenum - 1) Assert.IsTrue(next);
                else Assert.IsFalse(next);
            }
        }
    }

}
