using System.Runtime.CompilerServices;

namespace Generic_Octree
{
    public class OctreeNode<TNode> where TNode : notnull
    {
        private OctreeNode<TNode>[]? _Nodes;

        public TNode Value { get; private set; }
        public bool IsUniform => _Nodes == null;

        public OctreeNode<TNode>? this[int index] => _Nodes?[index];

        /// <summary>
        ///     Creates an in-memory compressed 3D representation of any unmanaged data type.
        /// </summary>
        /// <param name="value">Initial value of the collection.</param>
        public OctreeNode(TNode value) => Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPoint(int extent, int x, int y, int z, TNode newValue)
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

            Octree.DetermineOctant(extent, ref x, ref y, ref z, out int octant);

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
            _Nodes = new[]
            {
                new OctreeNode<TNode>(Value),
                new OctreeNode<TNode>(Value),
                new OctreeNode<TNode>(Value),
                new OctreeNode<TNode>(Value),
                new OctreeNode<TNode>(Value),
                new OctreeNode<TNode>(Value),
                new OctreeNode<TNode>(Value),
                new OctreeNode<TNode>(Value)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckShouldCollapse()
        {
            if (IsUniform)
            {
                return false;
            }

            TNode firstValue = _Nodes![0].Value;

            // avoiding using linq here for performance sensitivity
            foreach (OctreeNode<TNode> octreeNode in _Nodes)
            {
                if (!octreeNode.IsUniform || !octreeNode.Value.Equals(firstValue))
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Collapse()
        {
            Value = _Nodes![0].Value;
            _Nodes = null;
        }
    }
}
