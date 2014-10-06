using MetroLog;
using JsonStore.Common.Extensions;
using JsonStore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace JsonStore.Model
{
    [DataContract]
    public partial class EntitySet<T> : ICollection<T>, IEntitySet<T> where T : EntityBase
    {
        protected readonly ILogger Logger;
        private bool _isDirty;

        public EntitySet(IEnumerable<T> list = null)
        {
            Logger = this.GetLogger();
            _list = new List<T>(list ?? Enumerable.Empty<T>());
            if (_list.Any())
            {
                //we've been initialised with something: set dirty, so next time it gets saved
                IsDirty = true;
            }
        }

        public string Name
        {
            get { return typeof (T).Name; }
        }

        public bool IsDirty
        {
            get { return _isDirty || this.Any(e => e.MetaData.IsDirty); }
            protected set { _isDirty = value; }
        }

        public void DeleteAll()
        {
            //not sure why begin batch update is called here. seems un-necessary
            foreach (var entity in this)
            {
                entity.BeginUpdate();
                entity.MarkForDeletion();
                entity.EndUpdate();
            }
            IsDirty = true;
        }

        public MergeResult Merge(IEnumerable<EntityBase> entitiesToMerge)
        {
            Logger.Info("Merging {0} entities. Is dirty at start of merge: {1}", Name, IsDirty);

            var mergeResult = new MergeResult();

            var entitiesToAdd = new List<T>();
            var entitiesToDelete = new List<T>();

            int tempLoggedCount = 0;
            foreach (var entity in entitiesToMerge.OfType<T>())
            {
                //is this entity present locally?
                var localEntity = this.FirstOrDefault(e => e.IsSameForSync(entity));

                //TODOLATER consider using NodaTime and timestamp for versioning
                var remoteVersion =
                    entity.AuditInfo.LastModified.GetValueOrDefault(entity.AuditInfo.Created);
                var localVersion = 
                    localEntity == null 
                        ? default(DateTimeOffset) 
                        : localEntity.AuditInfo.LastModified.GetValueOrDefault(localEntity.AuditInfo.Created);

                //Logger.DebugEx(" Local  version: {0} {1}", localVersion, Name);
                //Logger.DebugEx(" Remote version: {0} {1}", remoteVersion, Name);

                if (localEntity == null)
                {
                    //make sure this is not a remote entity that is marked for deletion
                    if (!entity.MetaData.MarkedForDeletion)
                        entitiesToAdd.Add(entity);
                }
                else if (remoteVersion.IsGreaterThan(localVersion)) 
                {
                    //remote entity is newer
                    if (entity.MetaData.MarkedForDeletion)
                    {
                        entitiesToDelete.Add(localEntity);
                    }
                    else
                    {
                        //TODOLATER: perhaps seperate out: persistence model, domain model, INPC model, dtos
                        //hack-ish: batch update without property changed notifications
                        localEntity.BeginUpdate();
                        localEntity.UpdateFrom(entity);
                        localEntity.EndUpdate();

                        mergeResult.Updates += 1;
                    }
                }
                else if (remoteVersion.IsLessThan(localVersion))
                {
#if DEBUG
                    if (tempLoggedCount < 5)
                    {
                        Logger.DebugEx("Remote < local. remote {0}. local {1}. {2} ", remoteVersion, localVersion, localEntity.GetType().Name);
                    }
                    tempLoggedCount++;
#endif
                    //else local entity is newer: nothing to do during Merge. 
                    mergeResult.OutgoingChanges += 1;
                }
            }

            _list.AddRange(entitiesToAdd);
            foreach (var localEntity in entitiesToDelete)
            {
                localEntity.BeginUpdate();
                localEntity.MarkForDeletion();
                localEntity.EndUpdate();
            }

            mergeResult.Adds = (uint)entitiesToAdd.Count;
            mergeResult.Deletes = (uint)entitiesToDelete.Count;
            
            IsDirty = (mergeResult.Adds + mergeResult.Deletes + mergeResult.Updates) > 0;

            mergeResult.IsSuccess = true;

            Logger.DebugEx("{4} Merge result: +{0}. -{1}. ~{2}. Is now dirty: {3}",
                mergeResult.Adds, mergeResult.Updates, mergeResult.Deletes,
                IsDirty,
                Name);

            return mergeResult;
        }

        public void Purge()
        {
            //this will set the dirty flag
            RemoveAll(e => e.MetaData.MarkedForDeletion);
            foreach (var entity in this)
            {
                entity.BeginUpdate();
                entity.PurgeOrphanLinks();
                entity.EndUpdate();
            }
        }

        public void ResetDirty()
        {
            foreach (var entity in this)
            {
                entity.MetaData.IsDirty = false;
                entity.MetaData.IsNew = entity.Id.IsEmpty();
            }
            IsDirty = false;
        }
    }

}
