using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SharpTrees;

namespace SharpTreesTest
{
    [TestClass]
    public class RTreeTest
    {
        [TestMethod]
        public void TestMethod1()
        {
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
}
