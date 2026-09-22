using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace ParadiseGameLauncher.Data
{
    public static class JsonRepository
    {
        // object to hold write options that make JSON files readable
        private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

        // method for loading JSON data into objects
        public static T Load<T>(string filePath) where T : new()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<T>(json) ?? new T();
                }
            }
            catch
            {

            }
            return new T();
        }

        // method for saving objects as JSON data
        public static void Save<T>(string filePath, T data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data, WriteOptions);
                File.WriteAllText(filePath, json);
            }
            catch
            {
            }
        }
    }
}
