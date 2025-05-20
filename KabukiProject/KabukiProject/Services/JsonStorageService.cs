using KabukiProject.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KabukiProject.Services
{
    // Додаємо обмеження 'where T : class', хоча це часто вже неявно
    // при використанні з моделями, але робить код більш явним.
    public class JsonStorageService<T> : IJsonStorageService<T> where T : class
    {
        public List<T> LoadData(string filePath)
        {
            try
            {
                // Перевіряємо та створюємо директорію, якщо її немає
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.WriteLine($"Created directory: {directory} for {typeof(T).Name} data.");
                }

                if (File.Exists(filePath))
                {
                    Debug.WriteLine($"Attempting to load data for {typeof(T).Name} from: {Path.GetFullPath(filePath)}");
                    string json = File.ReadAllText(filePath);

                    // Десеріалізуємо з TypeNameHandling.Auto
                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        // Додамо Error, щоб бачити проблеми з десеріалізацією
                        Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                        {
                            Debug.WriteLine($"Deserialization error for {typeof(T).Name}: {args.ErrorContext.Error.Message}");
                            args.ErrorContext.Handled = true; // Можна обробити помилку, щоб продовжити
                        }
                    };

                    var data = JsonConvert.DeserializeObject<List<T>>(json, settings);
                    Debug.WriteLine($"Successfully loaded data for {typeof(T).Name} from {filePath}. Items loaded: {data?.Count ?? 0}");
                    return data ?? new List<T>(); // Повертаємо новий список, якщо десеріалізація повернула null
                }

                Debug.WriteLine($"File does not exist: {Path.GetFullPath(filePath)}. Returning empty list for {typeof(T).Name}.");
                return new List<T>(); // Повертаємо порожній список, якщо файл не існує
            }
            catch (JsonSerializationException ex)
            {
                // Це обробить помилки, коли JSON не може бути розібраний
                MessageBox.Show($"Помилка десеріалізації даних для {typeof(T).Name} з {filePath}: {ex.Message}. Файл може бути пошкоджений. Створюємо новий порожній список.", "Помилка завантаження даних", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"JSON Deserialization Error for {typeof(T).Name} from {filePath}: {ex.Message}");
                return new List<T>();
            }
            catch (Exception ex)
            {
                // Для всіх інших неочікуваних помилок
                MessageBox.Show($"Неочікувана помилка при завантаженні даних для {typeof(T).Name} з {filePath}: {ex.Message}. Створюємо новий порожній список.", "Помилка завантаження", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Unexpected Error loading data for {typeof(T).Name} from {filePath}: {ex.Message}");
                return new List<T>();
            }
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
                    Debug.WriteLine($"Created directory: {directory} for {typeof(T).Name} data.");
                }

                // Серіалізуємо з TypeNameHandling.Auto
                string json = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    // Додамо Error, щоб бачити проблеми з серіалізацією
                    Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                    {
                        Debug.WriteLine($"Serialization error for {typeof(T).Name}: {args.ErrorContext.Error.Message}");
                        args.ErrorContext.Handled = true; // Можна обробити помилку, щоб продовжити
                    }
                });

                File.WriteAllText(filePath, json);
                Debug.WriteLine($"Successfully saved data for {typeof(T).Name} to {Path.GetFullPath(filePath)}. Items saved: {data?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження даних для {typeof(T).Name} у {filePath}: {ex.Message}", "Помилка збереження", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Error saving data for {typeof(T).Name} to {filePath}: {ex.Message}");
            }
        }
    }
}