using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JsonStore.Data
{
    [DataContract]
    public class DbMetadata
    {
        [DataMember(Name = "sv")]
        public string SchemaVersion { get; set; }

        [DataMember(Name = "bk")]
        public List<BackupInfo> Backups { get; set; }

        [DataMember(Name = "u")]
        public DateTimeOffset LastUpdated { get; set; }

        [DataMember(Name = "s")]
        public bool SyncActive { get; set; }

        [DataMember(Name = "sh")]
        public List<SyncInfo> SyncHistory { get; set; } 
    }

    [DataContract]
    public class SyncInfo
    {
        [DataMember(Name = "d")]
        public DateTimeOffset Date { get; set; } //when the sync happened
        [DataMember(Name = "sv")]
        public string SchemaVersion { get; set; }
        [DataMember(Name = "dn")]
        public string DeviceName { get; set; }
        [DataMember(Name = "p")]
        public string ProviderName { get; set; } //onedrive, google drive etc
        [DataMember(Name = "av")]
        public string AppVersion { get; set; }

        [DataMember(Name = "a")]
        public uint Added { get; set; }
        [DataMember(Name = "u")]
        public uint Updated { get; set; }
        [DataMember(Name = "x")]
        public uint Deleted { get; set; }
        [DataMember(Name = "c")]
        public uint ChangesUploaded { get; set; }
    }

    [DataContract]
    public class BackupInfo
    {
        [DataMember(Name = "sv")]
        public string SchemaVersion { get; set; }

        [DataMember(Name = "dt")]
        public DateTimeOffset Date { get; set; }

        [DataMember(Name = "f")]
        public string FilePath { get; set; }

        [DataMember(Name = "d")]
        public string DeviceName { get; set; } //device where the backup was taken
    }
}