using System;

namespace Money.Model
{
    public struct MergeResult
    {
        public bool IsSuccess;

        public bool HasOutgoingChanges
        {
            get { return OutgoingChanges > 0; }
        }

        public MergeConflict[] Conflicts;

        public uint Updates;
        public uint Adds;
        public uint Deletes;
        public uint OutgoingChanges;
    }

    public struct MergeConflict
    {
        public string Filename;
        public EntityConflict[] Entities;
    }

    public struct EntityConflict
    {
        public string EntityName;
        public Tuple<EntityBase, EntityBase> Values;
        public ConflictType Type;
    }

    public enum ConflictType
    {
        LocalUpdatedRemoteDeleted,
        LocalDeletedRemoteUpdated
    }
}
