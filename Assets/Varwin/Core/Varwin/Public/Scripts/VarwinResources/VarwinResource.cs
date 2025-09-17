using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Varwin.Public
{
    /// <summary>
    /// Файл ресурса для Varwin.
    /// </summary>
    public class VarwinResource : ScriptableObject
    {
        /// <summary>
        /// Импортирован ли он из файла VWR.
        /// </summary>
        public bool IsImported { get; private set; }

        /// <summary>
        /// GUID ресурса.
        /// </summary>
        public string Guid;

        /// <summary>
        /// Метод импорта ресурса из файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>VarwinResource.</returns>
        public static VarwinResource CreateFromFile(string filePath)
        {
            var guid = "";
            if (!File.Exists(filePath))
            {
                return null;
            }

            using (var file = new FileStream(filePath, FileMode.Open))
            {
                using (var zipArchive = new ZipArchive(file, ZipArchiveMode.Read))
                {
                    var installEntry = zipArchive.GetEntry("install.json");
                    if (installEntry == null)
                    {
                        return null;
                    }

                    using (var readStream = new StreamReader(installEntry.Open()))
                    {
                        var installJson = readStream.ReadToEnd();
                        var json = JsonConvert.DeserializeObject<JObject>(installJson);
                        guid = json.GetValue("Guid")?.ToString();
                    }
                    
                }
            }
            
            var instance = CreateInstance<VarwinResource>();
            instance.IsImported = true;
            instance.Guid = guid;
            
            return instance;
        }
    }
}