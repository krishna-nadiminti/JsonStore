using Money.Common;
using Money.Common.Extensions;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Money.Model
{
    public abstract class EntityBase : BindableBase
    {
        [DataMember(Name = "id")] 
        protected string _uid;

        [DataMember(Name = "_md")]
        public EntityMetadata MetaData { get; private set; }

        [DataMember(Name = "_ai")] private AuditInfo _auditInfo;
        public AuditInfo AuditInfo
        {
            get
            {
                return _auditInfo ?? (_auditInfo = AuditInfo.CreateNew()); //to make sure it works for deserialised txs that don't have an audit-info 
            }
            private set { _auditInfo = value; }
        }
        
        protected EntityBase()
        {
            AuditInfo = AuditInfo.CreateNew();
            MetaData = new EntityMetadata
            {
                IsNew = true
            };
        }

        private bool _isBatchUpdating;
        private bool _setDirtyOnBatchUpdate;
        internal void BeginUpdate(bool setDirtyOnUpdate = true)
        {
            //a bit of a hack to prevent firing INPC notifications
            _isBatchUpdating = true;
            //hack to prevent setting dirty when batch updating
            _setDirtyOnBatchUpdate = setDirtyOnUpdate;
        }

        internal void EndUpdate(bool notify = false)
        {
            _isBatchUpdating = false;
            if (notify)
            {
                OnPropertyChanged(string.Empty);
            }
        }

        public virtual bool IsSameForSync(EntityBase other)
        {
            if (other == null) return false;
            if (other.GetType() == GetType())
            {
                return Id == other.Id;
            }
            return false;
        }

        protected bool SetPropertyWithoutDirty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            return SetPropertyInternal(ref storage, value, propertyName, updateDirtyOnChange: false);            
        }

        protected sealed override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            return SetPropertyInternal(ref storage, value, propertyName);
        }

        //TODO seperate out concerns of storage & (de)-serialisation) & change tracking for storage, auditing, INPC
        //there are a few seperate concerns here:
        //1. change tracking. [ is an object dirty? should we persist it when SaveChanges is called? ]
        //    - shouldn't happen when an object is hydrated from persistent store
        //    - should happen during a merge/sync, so we know to save
        //    - should happen when UpdateFrom is called, or properties are set otherwise.
        //2. audit / versioning [ via a last updated UTC timestamp ] [ there are related but perhaps seperate concerns? ]
        //    - shouldn't happen when an object is hydrated from persistent store
        //    - should happen when UpdateFrom is called, or properties are set otherwise.
        //3. INPC - property change notifications [ reqd mostly for the UI ]
        //    - shouldn't happen when an object is hydrated from persistent store
        //    - shouldn't happen when merging as part of sync: because it causes incorrect thread issues: due to the way we've mixed up entities and INPC
        //    - currently happens when UpdateFrom is called, or properties are set otherwise from UI.
        private bool SetPropertyInternal<T>(ref T storage, T value, [CallerMemberName] string propertyName = null, bool updateDirtyOnChange = true)
        {
            //this is quite hacky! TODOLATER: clean up
            bool changed;
            if (_isBatchUpdating)
            {
                //no notification during batch update
                changed = !Equals(storage, value);
                if (changed)
                {
                    storage = value;
                }
                if (_setDirtyOnBatchUpdate && updateDirtyOnChange)
                {
                    MetaData.IsDirty = MetaData.IsDirty || changed;
                }
            }
            else
            {
                changed = base.SetProperty(ref storage, value, propertyName);
                if (updateDirtyOnChange)
                {
                    MetaData.IsDirty = MetaData.IsDirty || changed;
                }
            }
            return changed;
        }

        public string Id
        {
            get { return _uid; }
            set
            {
                _uid = value;
                MetaData.IsNew = _uid.IsEmpty();
                OnPropertyChanged(() => MetaData);
            }
        }

        /// <summary>
        /// Expected to be called from the deserialization init method of subclasses
        /// </summary>
        /// <param name="defaultUid">The default value of the Id property to use if deserialization sets it to null/empty. If not provided, will create a new guid string</param>
        protected virtual void InitOnDeserialized(string defaultUid = null)
        {
            if (MetaData == null)
                MetaData = new EntityMetadata();

            if (_auditInfo == null)
                _auditInfo = AuditInfo.CreateNew();

            if (_uid.IsEmpty())
            {
                _uid = defaultUid; // ?? Guid.NewGuid().ToString();
            }
            
            MetaData.IsNew = Id.IsEmpty();
        }

        protected internal void MarkForDeletion()
        {
            MetaData.MarkForDeletion();
            AuditInfo.LastModified = DateTimeOffset.UtcNow;
        }

        public virtual void PurgeOrphanLinks()
        {
            
        }

        public abstract void UpdateFrom(EntityBase other);
    }
}
