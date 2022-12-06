using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
}
