using JsonStore.DataAcces;
using JsonStore.Model;
using PCLStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JsonStore.Data
{
    struct EntityTypeInfo
    {
        public Type Type { get; private set; }
        public string Filename { get; private set; }

        public EntityTypeInfo(Type type) : this()
        {
            Type = type;

            //changed to use hardcoded namespace to make sure we always write to the same file 
            //and accidental refactoring to change namespace keeps backward compat
            Filename = "Money.Model." + type.Name + ".dat";
        }
    }

    class FileEntityStore
    {
        private readonly Dictionary<Type, object> _fileStores = new Dictionary<Type, object>(); 

        public FileEntityStore(
            IFolder baseFolder, 
            IEnumerable<EntityTypeInfo> entityTypes)
        {
            if (baseFolder == null) throw new ArgumentNullException("baseFolder");
            if (entityTypes == null) throw new ArgumentNullException("entityTypes");
            var typesToSerialise = entityTypes.ToList();

            if (!typesToSerialise.Any()) 
                throw new ArgumentException("There should be atleast one entity type.", "entityTypes");
            if (typesToSerialise.Any(t => t.Type == null))
                throw new ArgumentException("All entity types should be non-null", "entityTypes");

            foreach (var entityType in typesToSerialise)
            {
                var fileStore = CreateFileStore(baseFolder, entityType);
                _fileStores.Add(entityType.Type, fileStore);
            }
        }

        private static object CreateFileStore(IFolder baseFolder, EntityTypeInfo entityType)
        {
            var entitySetType = typeof (EntitySet<>).MakeGenericType(entityType.Type);
            var fileObjStoreType = typeof(FileObjectStore<>).MakeGenericType(entitySetType);

            //pass in ctor params: folder, filename, serialiser
            var store = Activator.CreateInstance(fileObjStoreType, baseFolder, entityType.Filename, null);
            return store;
        }

        private FileObjectStore<EntitySet<T>> FindObjectStore<T>() 
            where T : EntityBase
        {
            var entityType = typeof (T);

            object store;
            _fileStores.TryGetValue(entityType, out store);

            var fileStore = store as FileObjectStore<EntitySet<T>>;
            if (fileStore == null)
            {
                throw new InvalidOperationException("Could not find file store for type: " + typeof (T));
            }
            return fileStore;
        }

        //we've made the save and load take concrete EntitySet<T> instead of IEnumerable<T> or IList<T>
        //because we might have to store additional properties from the entity set in the serialised files.

        public async Task<bool> SaveAsync<T>(EntitySet<T> entities)
            where T : EntityBase
        {
            var fileStore = FindObjectStore<T>();

            return await fileStore.SaveAsync(entities);
        }

        public Task<EntitySet<T>> LoadAsync<T>()
            where T : EntityBase
        {
            var fileStore = FindObjectStore<T>();

            return fileStore.LoadAsync();
        }
    }
}
