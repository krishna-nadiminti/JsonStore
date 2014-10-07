using System.Runtime.Serialization;

namespace Money.Model
{
    [DataContract]
    public class EntityMetadata
    {
        [DataMember(Name = "_del")]
        private bool _markedForDeletion;

        [DataMember(Name = "_pk")]
        public string PartitionKey { get; internal set; }

        public bool MarkedForDeletion
        {
            get { return _markedForDeletion; }
            private set
            {
                _markedForDeletion = value;
                IsDirty = true;
            }
        }

        public bool IsNew { get; internal set; }
        public bool IsDirty { get; internal set; }

        internal void MarkForDeletion()
        {
            MarkedForDeletion = true; //also sets is dirty
        }
    }
}