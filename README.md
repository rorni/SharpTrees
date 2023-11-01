# SharpTrees
Implementation of tree data structures for C#

## RTree

R-trees are tree data structures used for indexing multi-dimensional information. 
This project presents R-tree implementation with support of an arbitrary number 
of dimensions. The implementation is based on the article [1]. There is a R-tree 
modification, so called R-star tree, which allows to produce more optimal 
bounding rectangles [2]. It utilizes re-insertion approach when new item is 
added. However, this strategy is not implemented yet.

[1] A. Guttman. R-Trees: A dynamic index structure for spatial searching. 
ACM SIGMOD Record. Vol. 14, Issue 2, 1984. pp 47–57.
https://doi.org/10.1145/971697.602266

[2] N. Beckmann, et al. The R*-tree: An efficient and Robust Access Method for 
Points and Rectangles. Proceedings of the 1990 ACM SIGMOD international 
conference on Management of dataMay. 1990. pp 322–331. 
https://doi.org/10.1145/93597.98741

### How to use

R-tree is represented by generic class `RTree<T>`. `T` is a type of data that is 
stored in the R-tree. It must implement `IBounded` interface, which exposes two 
methods:

1. `Bounds[] GetBounds()` - returns bounds of the object.
2. `bool IsEqual(IBounded other)` - method to check equality of the objects.

`Bounds` is a structure that sets lower and upper bounds in a signle dimension 
(direction). The number of `Bounds` object returned by `GetBounds()` method sets 
the number of data dimensions.

Let's we need to store and retriewe 2D points. It has two coordinates (X and Y) 
and tolerance e.g. delta=0.01. If coordinates of two points differ less than
delta, these points are considered as equal. 

```
using SharpTrees;

class Point : IBounded {
    public double X { get; }
    public double Y { get; }
    private double const delta = 0.01;
    
    Point(double x, double y) {
        X = x;
        Y = y;
    }
    
    // IBounded interface implementation
    public Bounds[] GetBounds() {
        Bounds[] result = new Bounds[2];
        result[0] = new Bounds(X - delta, X + delta);
        result[1] = new Bounds(Y - delta, Y + delta);
        return result;
    }
    
    public bool IsEqual(IBounded other) {
        bool result = false;
        Point otherPoint = other as Point;
        if (otherPoint != null) {
            result = Math.Abs(X - otherPoint.X) < H && Math.Abs(Y - otherPoint.Y) < H;
        }
        return result;
    }
}
```

Now we can create RTree instance:

```
byte M = 4;
byte m = 2;

RTree<Point> rtree = new RTree<Point>(M, m, NodeSplitStrategy.Exhaustive);
```

The first parameter is the maximum number of entries that node can possess. 
The second parameter is the minimum number of entries. There are two constraints
on these parameters: 

1. m >= 2; 
2. M >= 2 * m.

If these constraints are not respected, IncorrectLimitsOfEntriesException will be thrown.
The third parameter is node splitting strategy. For now only Exhaustive strategy [1] is
implemented. Therefore it is better to set small values of M, since complexity of 
exhaustive strategy growth exponentially. 

Now we can add new items to the tree, search, delete.

```
bool result = rtree.Add(new Point(2, 3)); // True, if new item was added. False, if not - such
                                          // item already exists.
rtree.Add(new Point(4, 5));

bool del_result = rtree.Delete(new Point(2, 3), out Point deletedPoint); // True, if the point was 
               // deleted; false, if rtree does not contain such point. 

bool ser_result = rtree.Search(new Point(4, 5), out Point found);

Bounds[] bounds = new Bounds[] { new Bounds(3, 5), new Bounds(4, 6) };
ICollection<Point> points = rtree.SearchIntersections(Bounds[] bounds); // Finds all points that
              // have bounding rectangles intersecting specified bounds.
```

It is possible to iterate over all items contained in the tree.

```
foreach (Point point in rtree) {
    Console.WriteLine(String.Format("x={0:f}, y={1:f}", point.X, point.Y));
}
```
