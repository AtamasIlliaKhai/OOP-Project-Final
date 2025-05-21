using KabukiProject.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KabukiProject.Services
{
    public class LessonService
    {

        private static readonly object _lock = new object();

        public static LessonService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LessonService();
                        }
                    }
                }
                return _instance;
            }
        }

        private static LessonService _instance;
        private readonly string _lessonsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "lessons.json");
        private List<Lesson> _lessons;

        // Приватний конструктор для реалізації Singleton
        private LessonService()
        {
            _lessons = new List<Lesson>();
            LoadLessons();
        }

        private void LoadLessons()
        {
            if (File.Exists(_lessonsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_lessonsFilePath);
                    _lessons = JsonConvert.DeserializeObject<List<Lesson>>(json) ?? new List<Lesson>();
                }
                catch (JsonException ex)
                {
                    // Обробка помилок десеріалізації (наприклад, пошкоджений файл JSON)
                    System.Diagnostics.Debug.WriteLine($"Помилка завантаження уроків: {ex.Message}");
                    _lessons = new List<Lesson>(); // Ініціалізуємо порожній список, щоб уникнути NullReferenceException
                    MessageBox.Show($"Помилка завантаження файлу уроків. Файл може бути пошкоджений: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _lessons = new List<Lesson>();
            }
        }

        private void SaveLessons()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_lessons, Formatting.Indented);
                File.WriteAllText(_lessonsFilePath, json);
            }
            catch (IOException ex)
            {
                // Обробка помилок збереження (наприклад, файл зайнятий іншим процесом)
                System.Diagnostics.Debug.WriteLine($"Помилка збереження уроків: {ex.Message}");
                MessageBox.Show($"Помилка збереження файлу уроків: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для додавання нового уроку
        public void AddLesson(Lesson lesson)
        {
            if (lesson == null) return;
            _lessons.Add(lesson);
            SaveLessons();
        }

        // Метод для оновлення існуючого уроку
        public void UpdateLesson(Lesson updatedLesson)
        {
            if (updatedLesson == null) return;

            var existingLesson = _lessons.FirstOrDefault(l => l.Id == updatedLesson.Id);
            if (existingLesson != null)
            {
                // Оновлюємо всі властивості існуючого уроку
                existingLesson.TeacherId = updatedLesson.TeacherId;
                existingLesson.StudentId = updatedLesson.StudentId;
                existingLesson.DateTime = updatedLesson.DateTime;
                existingLesson.Price = updatedLesson.Price;
                existingLesson.Status = updatedLesson.Status;
                SaveLessons();
            }
        }

        // Метод для видалення уроку за ID
        public void DeleteLesson(string lessonId)
        {
            var lessonToRemove = _lessons.FirstOrDefault(l => l.Id == lessonId);
            if (lessonToRemove != null)
            {
                _lessons.Remove(lessonToRemove);
                SaveLessons();
            }
        }

        // Метод для отримання всіх уроків
        public List<Lesson> GetAllLessons()
        {
            return new List<Lesson>(_lessons); // Повертаємо копію, щоб уникнути зовнішніх модифікацій
        }

        // Метод для отримання уроків за ID студента
        public List<Lesson> GetLessonsByStudentId(string studentId)
        {
            return _lessons.Where(l => l.StudentId == studentId).ToList();
        }

        // Метод для отримання уроків за ID викладача
        public List<Lesson> GetLessonsByTeacherId(string teacherId)
        {
            return _lessons.Where(l => l.TeacherId == teacherId).ToList();
        }

        // Додаємо метод для отримання уроку за ID
        public Lesson GetLessonById(string lessonId)
        {
            return _lessons.FirstOrDefault(l => l.Id == lessonId);
        }
    }
}