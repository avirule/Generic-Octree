using System;
using System.Buffers;

namespace OctreeNS
{
    internal class OctreeNode<T> : IDisposable where T : notnull
    {
        public T Value { get; private set; }
        public OctreeNode<T>[]? Nodes { get; private set; }
        public bool IsUniform => Nodes is null;

        /// <summary>
        ///     Creates an in-memory compressed 3D representation of any unmanaged data type.
        /// </summary>
        /// <param name="value">Initial value of the collection.</param>
        public OctreeNode(T value) => Value = value;

        public void SetPoint(uint extent, uint x, uint y, uint z, T newValue)
        {
            if (IsUniform)
            {
                if (Value.Equals(newValue))
                {
                    return;
                }
                else if (extent < 1)
                {
                    // reached smallest possible depth (usually 1x1x1) so
                    // set value and return
                    Value = newValue;
                    return;
                }
                else
                {
                    Populate();
                }
            }

            Octree.DetermineOctant(extent, ref x, ref y, ref z, out uint octant);

            // recursively dig into octree and set
            Nodes![octant].SetPoint(extent >> 1, x, y, z, newValue);

            // on each recursion back-step, ensure integrity of node
            // and collapse if all child node values are equal
            if (CheckShouldCollapse())
            {
                Collapse();
            }
        }

        private void Populate()
        {
            Nodes = ArrayPool<OctreeNode<T>>.Shared.Rent(8);

            for (int index = 0; index < 8; index++)
            {
                Nodes[index].Value = Value;
            }
        }

        private bool CheckShouldCollapse()
        {
            // we elide an `IsUniform`(null) check for perf, but
            // must ensure _Nodes isn't null at the callsite.
            T value = Nodes![0].Value;

            // avoiding using linq here for performance sensitivity
            for (int index = 0; index < 8; index++)
            {
                if (!Nodes[index].Equals(value))
                {
                    return false;
                }
            }

            return true;
        }

        private bool Equals(T value) => IsUniform && Value.Equals(value);

        private void Collapse()
        {
            Value = Nodes![0].Value;
            ArrayPool<T>.Shared.Return((Nodes as T[])!);
        }


        #region IDisposable

        public void Dispose()
        {
            if (!IsUniform)
            {
                for (int index = 0; index < 8; index++)
                {
                    Nodes![index].Dispose();
                }

                ArrayPool<T>.Shared.Return((Nodes as T[])!);
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
