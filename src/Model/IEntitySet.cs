using System.Collections.Generic;

namespace JsonStore.Model
{
    public interface IEntitySet<out T> : IEnumerable<T> where T : EntityBase
    {
        string Name { get; }
        bool IsDirty { get; }
        void DeleteAll();
        MergeResult Merge(IEnumerable<EntityBase> entitiesToMerge);
        void Purge();
        void ResetDirty();
    }
}