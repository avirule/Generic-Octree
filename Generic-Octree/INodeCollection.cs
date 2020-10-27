namespace Generic_Octree
{
    public interface INodeCollection<T>
    {
        int EdgeLength { get; }
        bool IsUniform { get; }
        T Value { get; }

        T GetPoint(int x, int y, int z);
        void SetPoint(int x, int y, int z, T value);
    }
}
