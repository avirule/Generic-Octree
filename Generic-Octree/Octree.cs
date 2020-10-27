using System;
using System.Runtime.CompilerServices;

namespace Generic_Octree
{
    public class Octree<T> : INodeCollection<T>
    {
        private readonly int _Extent;
        private readonly OctreeNode<T> _RootNode;

        public Octree(int edgeLength, T initialValue)
        {
            if ((edgeLength <= 0) || ((edgeLength & (edgeLength - 1)) != 0))
                throw new ArgumentException($"Size must be a power of two ({edgeLength}).", nameof(edgeLength));

            _Extent = edgeLength >> 1;
            _RootNode = new OctreeNode<T>(initialValue);

            EdgeLength = edgeLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private T GetPointIterative(int x, int y, int z)
        {
            OctreeNode<T> currentNode = _RootNode;

            for (int extent = _Extent; !currentNode!.IsUniform; extent /= 2)
            {
                Octree.DetermineOctant(extent, ref x, ref y, ref z, out int octant);

                currentNode = currentNode[octant]!;
            }

            return currentNode.Value;
        }

        public int EdgeLength { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetPoint(int x, int y, int z) => GetPointIterative(x, y, z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPoint(int x, int y, int z, T value) => _RootNode.SetPoint(_Extent, x, y, z, value);

        public T Value => _RootNode.Value;
        public bool IsUniform => _RootNode.IsUniform;
    }

    internal static class Octree
    {
        // indexes:
        // bottom half quadrant indexes:
        // 1 3
        // 0 2
        // top half quadrant indexes:
        // 5 7
        // 4 6
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void DetermineOctant(int extent, ref int x, ref int y, ref int z, out int octant)
        {
            octant = 0;

            if (x >= extent)
            {
                x -= extent;
                octant += 1;
            }

            if (y >= extent)
            {
                y -= extent;
                octant += 4;
            }

            if (z >= extent)
            {
                z -= extent;
                octant += 2;
            }
        }
    }
}
