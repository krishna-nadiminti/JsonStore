using MetroLog;
using Money.Extensions;
using Money.UI.Services;
using PCLStorage;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace Money.DataAccess
{
    public class FileObjectStore<T> : IObjectStore<T>
    {
        private readonly IFolder _folder;
        private readonly IObjectSerializer<T> _serializer;

        private readonly string _filename;

        private readonly ILogger _logger;

        private const string defaultFileExtension = ".dat";
        public FileObjectStore(IFolder folder, string filename = null, IObjectSerializer<T> serializer = null)
        {
            _folder = folder;
            _logger = this.GetLogger();

            _serializer = serializer ?? new JsonObjectSerializer<T>();
            _filename = string.IsNullOrWhiteSpace(filename) ? GetFileName() : filename;
        }

        private static string GetFileName()
        {
            return (typeof (T).FullName) + defaultFileExtension;
        }

        public async Task<bool> SaveAsync(T objectGraph)
        {
            if (Equals(objectGraph, null))
                return false;
            
            //create a seperate file to write to
            //so if we crash in the middle of this, we should not have corrupted the original file
            IFile file = await _folder.CreateFileAsync(_filename, CreationCollisionOption.GenerateUniqueName);
            bool createdUniqueFile = file.Name != _filename;
            
            _logger.DebugEx("Created file " + file.Name);

            //TODOlater file add optional transacted writes to PCLStorage
            using (var stream = await file.OpenAsync(FileAccess.ReadAndWrite))
            {
                await _serializer.SerializeAsync(stream, objectGraph);
            }
            /*
            using (StorageStreamTransaction streamTx = await file.OpenTransactedWriteAsync())
            {
                using (Stream outStream = streamTx.Stream.AsStreamForWrite())
                {
                    await _serializer.SerializeAsync(outStream, objectGraph);
                }
                await streamTx.CommitAsync();
            }
            */

            if (createdUniqueFile)
            {
                //rename files
                IFile existingFile = await _folder.CreateFileAsync(_filename, CreationCollisionOption.OpenIfExists);

                _logger.DebugEx("Created or opened file: " + _filename);
                _logger.DebugEx("Moving file " + existingFile.Name + ", to backup ");

                await existingFile.MoveAsync(
                    PortablePath.Combine(_folder.Path, GetBackupFileName(_filename)),
                    NameCollisionOption.ReplaceExisting);

                _logger.DebugEx("Moving file " + file.Name + ", to " + _filename);

                await file.MoveAsync(
                    PortablePath.Combine(_folder.Path, _filename), 
                    NameCollisionOption.ReplaceExisting);
            }

            return true;
        }

        private string GetBackupFileName(string filename)
        {
            filename = Path.GetFileNameWithoutExtension(filename) + ".bak";

            return filename;
        }

        public async Task<T> LoadAsync()
        {
            Task<T> recoveryTask;
            try
            {
                return await LoadInternalAsync(_folder, _filename);
            }
            catch (Exception ex)
            {
                if (!(ex is XmlException || ex is SerializationException))
                {
                    throw;
                }

                _logger.Warn("Got an XmlException/SerializationException. Trying to recover older file for " + typeof(T).FullName, ex);

                recoveryTask = LoadFromBackupAsync(_folder, _filename);
            }

            var result = await recoveryTask;
            
            //Trace.FileRecovery(typeof(T).FullName);

            return result;
        }

        private async Task<T> LoadFromBackupAsync(IFolder folder, string filename)
        {
            var backupFileName = GetBackupFileName(filename);

            //if back up file doesn't exist, it is ok to fail - there is nothing we can do anyway

            var file = await folder.GetFileAsync(backupFileName);

            await file.CopyAsync(PortablePath.Combine(folder.Path, filename), NameCollisionOption.ReplaceExisting);

            return await LoadInternalAsync(folder, filename);
        }
        
        private async Task<T> LoadInternalAsync(IFolder folder, string filename)
        {
            try
            {
                IFile file = await folder.GetFileAsync(filename);

                var fileInfo = await file.GetFileInfoAsync();

                _logger.DebugEx("Loading file: {0}. T is: {1}", file.Path, typeof (T).FullName);

                if (fileInfo.Size == 0)
                {
                    _logger.DebugEx("No data in file: {0}",file.Path);
                    return default(T);
                }

                using (Stream readableStream = await file.OpenAsync(FileAccess.Read))
                {
                    return await _serializer.DeserializeAsync(readableStream);
                }
            }
            catch (FileNotFoundException ex)
            {
                _logger.Warn("File not found: " + _filename, ex);
                return default(T);
            }
        }

    }

}