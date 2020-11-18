using System;
using System.Buffers;

namespace OctreeNS
{
    public class OctreeNode<T> : IDisposable where T : notnull
    {
        private OctreeNode<T>[]? _Nodes;

        public T Value { get; private set; }
        public bool IsUniform => _Nodes is null;

        public OctreeNode<T>? this[int index] => _Nodes?[index];

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
            _Nodes![octant].SetPoint(extent >> 1, x, y, z, newValue);

            // on each recursion back-step, ensure integrity of node
            // and collapse if all child node values are equal
            if (CheckShouldCollapse())
            {
                Collapse();
            }
        }

        private void Populate()
        {
            _Nodes = ArrayPool<OctreeNode<T>>.Shared.Rent(8);

            for (int index = 0; index < 8; index++)
            {
                _Nodes[index].Value = Value;
            }
        }

        private bool CheckShouldCollapse()
        {
            // we elide an `IsUniform`(null) check for perf, but
            // must ensure _Nodes isn't null at the callsite.
            T value = _Nodes![0].Value;

            // avoiding using linq here for performance sensitivity
            for (int index = 0; index < 8; index++)
            {
                if (!_Nodes[index].Equals(value))
                {
                    return false;
                }
            }

            return true;
        }

        private bool Equals(T value) => IsUniform && Value.Equals(value);

        private void Collapse()
        {
            Value = _Nodes![0].Value;
            ArrayPool<T>.Shared.Return((_Nodes as T[])!);
        }


        #region IDisposable

        public void Dispose()
        {
            if (!IsUniform)
            {
                ArrayPool<T>.Shared.Return((_Nodes as T[])!);
            }

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
