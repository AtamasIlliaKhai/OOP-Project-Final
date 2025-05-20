// KabukiProject/Services/UserService.cs
using KabukiProject.Enums; // Переконайтесь, що цей using є, якщо UserRole використовується
using KabukiProject.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System; // Додано для Exception

namespace KabukiProject.Services
{
    public class UserService
    {
        // Приватне поле для зберігання єдиного екземпляра
        private static UserService _instance;
        private static readonly object _lock = new object();

        // Властивість для доступу до єдиного екземпляра (Singleton)
        public static UserService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UserService();
                        }
                    }
                }
                return _instance;
            }
        }

        private List<User> _users; // Всі користувачі (студенти та викладачі)
        private string _usersFilePath = "users.json"; // Шлях до файлу з усіма користувачами

        private UserService()
        {
            // Приватний конструктор для патерну Singleton
            LoadUsers();
        }

        private void LoadUsers()
        {
            if (File.Exists(_usersFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_usersFilePath);
                    _users = JsonConvert.DeserializeObject<List<User>>(json, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto // Важливо для десеріалізації успадкованих класів
                    });
                }
                catch (Exception ex)
                {
                    // Обробка помилок завантаження
                    MessageBox.Show($"Помилка завантаження користувачів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _users = new List<User>(); // Ініціалізуємо порожній список у разі помилки
                }
            }
            else
            {
                _users = new List<User>();
                // Додайте тут початкових користувачів, якщо файл не існує
                AddInitialUsers();
                SaveUsers(); // Зберігаємо початкових користувачів
            }
        }

        private void SaveUsers()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_users, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto // Важливо для серіалізації успадкованих класів
                });
                File.WriteAllText(_usersFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження користувачів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddInitialUsers()
        {
            // Додайте початкових користувачів, якщо їх немає
            if (!_users.Any(u => u.Username == "student1"))
            {
                _users.Add(new Student
                {
                    Username = "student1",
                    Password = "123", // Пароль має бути хешований у реальному застосунку
                    Role = UserRole.Student,
                    FirstName = "Олександр",
                    LastName = "Студентенко",
                    Balance = 1500.00m
                });
            }
            if (!_users.Any(u => u.Username == "teacher1"))
            {
                _users.Add(new Teacher
                {
                    Username = "teacher1",
                    Password = "123", // Пароль має бути хешований у реальному застосунку
                    Role = UserRole.Teacher,
                    FirstName = "Марія",
                    LastName = "Викладаченко",
                    Description = "Досвідчений викладач математики з 10-річним стажем.",
                    PricePerHour = 250.00m,
                    PhotoPath = "teacher1.jpg", // або шлях до реального зображення
                    Subjects = new List<string> { "Математика", "Алгебра", "Геометрія" },
                });
            }
            // ВИПРАВЛЕНО: Створюємо екземпляр класу Administrator
            if (!_users.Any(u => u.Username == "admin1"))
            {
                _users.Add(new Administrator // <--- Змінено на Administrator
                {
                    Username = "admin1",
                    Password = "123",
                    Role = UserRole.Administrator // Це буде встановлено конструктором Administrator, але можна явно вказати
                });
            }
        }

        // ====== МЕТОДИ, ЯКІ ВАМ ПОТРІБНО ПЕРЕВІРИТИ АБО ДОДАТИ ======

        public User AuthenticateUser(string username, string password) // Змінено на AuthenticateUser з LoginUser
        {
            var user = _users.FirstOrDefault(u => u.Username == username && u.Password == password);
            return user;
        }

        public bool IsUsernameTaken(string username)
        {
            return _users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        // МЕТОД, ЯКИЙ ПОТРІБЕН ДЛЯ STUDENTDASHBOARDVIEWMODEL
        public List<Teacher> GetAllTeachers()
        {
            return _users.OfType<Teacher>().ToList();
        }

        // МЕТОД, ЯКИЙ ПОТРІБЕН ДЛЯ STUDENTDASHBOARDVIEWMODEL ТА TEACHERDASHBOARDVIEWMODEL
        public User GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }

        public bool RegisterUser(User newUser) // Змінено на bool, щоб вказувати успіх/невдачу
        {
            if (!_users.Any(u => u.Username == newUser.Username))
            {
                _users.Add(newUser);
                SaveUsers();
                return true; // Реєстрація успішна
            }
            else
            {
                // Логін вже існує
                MessageBox.Show("Користувач з таким іменем вже існує.", "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false; // Реєстрація не вдалася
            }
        }


        // МЕТОД, ЯКИЙ ПОТРІБЕН ДЛЯ ЗБЕРЕЖЕННЯ ЗМІН ПРОФІЛЮ ВИКЛАДАЧА/СТУДЕНТА
        public void UpdateUser(User updatedUser)
        {
            var existingUser = _users.FirstOrDefault(u => u.Username == updatedUser.Username);
            if (existingUser != null)
            {
                // Замінюємо існуючого користувача оновленим
                var index = _users.IndexOf(existingUser);
                _users[index] = updatedUser;
                SaveUsers(); // Зберігаємо зміни у файл
            }
            else
            {
                MessageBox.Show($"Користувача з логіном '{updatedUser.Username}' не знайдено для оновлення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Додатковий метод для оновлення балансу студента, якщо він потрібен окремо
        public void UpdateStudentBalance(string username, decimal newBalance)
        {
            var student = _users.OfType<Student>().FirstOrDefault(s => s.Username == username);
            if (student != null)
            {
                student.Balance = newBalance;
                UpdateUser(student); // Використовуємо існуючий UpdateUser для збереження
            }
        }
    }
}