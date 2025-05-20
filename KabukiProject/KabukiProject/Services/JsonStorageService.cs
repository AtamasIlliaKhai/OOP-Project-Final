using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KabukiProject.Interfaces; // Змінено на KabukiProject.Interfaces
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace KabukiProject.Services
{
    public class JsonStorageService<T> : IJsonStorageService<T> where T : class
    {
        public List<T> LoadData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
                return new List<T>();
            }
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }

        public void SaveData(List<T> data, string filePath)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}