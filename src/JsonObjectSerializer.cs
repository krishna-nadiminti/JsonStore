using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JsonStore;
using Newtonsoft.Json;

namespace JsonStore.DataAcces
{
    public class JsonObjectSerializer<T> : IObjectSerializer<T>
    {
        private readonly JsonSerializerSettings _settings;
        
        public JsonObjectSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.Default,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        public async Task<T> DeserializeAsync(Stream inStream)
        {
            if (inStream == null || !inStream.CanRead)
                throw new InvalidOperationException("Cannot read from stream");

            using (var reader = new StreamReader(inStream))
            {
                Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff tt zz") + " - In the read task now ");
                var content = await reader.ReadToEndAsync();
                Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff tt zz") + " - Finished reading");
                return JsonConvert.DeserializeObject<T>(content, _settings);
            }
        }

        public async Task SerializeAsync(Stream outStream, object graph)
        {
            if (outStream == null || !outStream.CanWrite)
                throw new InvalidOperationException("Cannot write to stream");

            using (var streamWriter = new StreamWriter(outStream))
            {
                var content = JsonConvert.SerializeObject(graph, Formatting.None, _settings);
                await streamWriter.WriteAsync(content);
            }
        }
    }
}
