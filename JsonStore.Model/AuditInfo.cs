using System;
using System.Runtime.Serialization;
using Money.Common;

namespace Money.Model
{
    //TODO: should we have a property that keeps track of who changed it / on which machine?

    [DataContract]
    public class AuditInfo : BindableBase
    {
        private AuditInfo() 
        {
            Created = DateTimeOffset.UtcNow;
        }

        [DataMember(Name = "_c")]
        private DateTimeOffset _created;
        public DateTimeOffset Created
        {
            get { return _created; }
            private set { SetProperty(ref _created, value); }
        }

        //[DataMember(Name = "_cb")]
        //private string _createdBy;
        //public string CreatedBy
        //{
        //    get { return _createdBy; }
        //    set { SetProperty(ref _createdBy, value); }
        //}

        [DataMember(Name="_l")]
        private DateTimeOffset? _lastModified;
        public DateTimeOffset? LastModified
        {
            get { return _lastModified; }
            set { SetProperty(ref _lastModified, value); }
        }

        //[DataMember(Name="_mb")]
        //private string _lastModifiedBy;
        //public string LastModifiedBy
        //{
        //    get { return _lastModifiedBy; }
        //    set { SetProperty(ref _lastModifiedBy, value); }
        //}

        public static AuditInfo CreateNew()
        {
            return new AuditInfo();
        }
    }
}
