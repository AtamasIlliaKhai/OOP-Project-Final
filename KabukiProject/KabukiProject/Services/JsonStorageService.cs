using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Interfaces;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace KabukiProject.Services
{
    public class JsonStorageService<T> : IJsonStorageService<T> where T : class
    {
        public List<T> LoadData(string filePath)
        {
            // Перевіряємо та створюємо директорію, якщо її немає
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<List<T>>(json, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto // Ключово для похідних класів
                    }) ?? new List<T>(); // Повертаємо новий список, якщо десеріалізація повернула null
                }
                catch (JsonSerializationException ex)
                {
                    MessageBox.Show($"Помилка десеріалізації даних з {filePath}: {ex.Message}. Файл може бути пошкоджений. Створюємо новий порожній список.", "Помилка завантаження даних", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<T>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Неочікувана помилка при завантаженні даних з {filePath}: {ex.Message}. Створюємо новий порожній список.", "Помилка завантаження", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<T>();
                }
            }
            return new List<T>(); // Повертаємо порожній список, якщо файл не існує
        }

        public void SaveData(List<T> data, string filePath)
        {
            try
            {
                // Перевіряємо та створюємо директорію, якщо її немає
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto // Ключово для похідних класів
                });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження даних у {filePath}: {ex.Message}", "Помилка збереження", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}