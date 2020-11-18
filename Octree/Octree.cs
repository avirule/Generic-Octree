using System;
using System.Runtime.CompilerServices;

namespace OctreeNS
{
    public class Octree<T> where T : notnull
    {
        private readonly uint _Extent;
        private readonly OctreeNode<T> _RootNode;

        public T? Value => _RootNode.Value;
        public bool IsUniform => _RootNode.IsUniform;

        public uint EdgeLength { get; }

        public Octree(uint edgeLength, T initialValue)
        {
            if ((edgeLength <= 0) || ((edgeLength & (edgeLength - 1)) != 0))
            {
                throw new ArgumentException($"Size must be a power of two ({edgeLength}).", nameof(edgeLength));
            }

            _Extent = edgeLength >> 1;
            _RootNode = new OctreeNode<T>(initialValue);

            EdgeLength = edgeLength;
        }

        public T GetPoint(uint x, uint y, uint z)
        {
            OctreeNode<T> currentNode = _RootNode;

            for (uint extent = _Extent; !currentNode!.IsUniform; extent >>= 1)
            {
                Octree.DetermineOctant(extent, ref x, ref y, ref z, out uint octant);

                currentNode = currentNode[(int)octant]!;
            }

            return currentNode.Value;
        }

        public void SetPoint(uint x, uint y, uint z, T value) => _RootNode.SetPoint(_Extent, x, y, z, value);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DetermineOctant(uint extent, ref uint x, ref uint y, ref uint z, out uint octant)
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
