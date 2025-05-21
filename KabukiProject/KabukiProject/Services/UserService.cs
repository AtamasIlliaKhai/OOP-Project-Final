using KabukiProject.Enums;
using KabukiProject.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System;

namespace KabukiProject.Services
{
    public class UserService
    {
        private static UserService _instance;
        private static readonly object _lock = new object();

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

        private List<User> _users;
        private string _usersFilePath = "users.json";

        private UserService()
        {
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
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                    if (_users == null)
                    {
                        _users = new List<User>();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка завантаження користувачів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _users = new List<User>();
                }
            }
            else
            {
                _users = new List<User>();
                AddInitialUsers();
                SaveUsers();
            }
        }

        public void SaveUsers()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_users, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
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
            if (!_users.Any(u => u.Username == "admin"))
            {
                _users.Add(new Administrator
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "admin",
                    Password = "admin",
                    FirstName = "Головний",
                    LastName = "Адміністратор",
                    Role = UserRole.Administrator
                });
            }

            if (!_users.Any(u => u.Username == "student1"))
            {
                _users.Add(new Student
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "student1",
                    Password = "123",
                    FirstName = "Олександр",
                    LastName = "Студентенко",
                    Balance = 1500.00m,
                    Role = UserRole.Student
                });
            }

            if (!_users.Any(u => u.Username == "teacher_unverified"))
            {
                _users.Add(new Teacher
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "teacher_unverified",
                    Password = "123",
                    FirstName = "Анна",
                    LastName = "Ковальчук",
                    Description = "Неверифікований викладач математики.",
                    PricePerHour = 150.00m,
                    PhotoPath = "",
                    Subjects = new List<string> { "Математика", "Алгебра" },
                    IsVerified = false,
                    Role = UserRole.Teacher
                });
            }

            if (!_users.Any(u => u.Username == "teacher_verified"))
            {
                _users.Add(new Teacher
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "teacher_verified",
                    Password = "123",
                    FirstName = "Петро",
                    LastName = "Іванов",
                    Description = "Верифікований викладач фізики з досвідом.",
                    PricePerHour = 300.00m,
                    PhotoPath = "",
                    Subjects = new List<string> { "Фізика", "Астрономія" },
                    IsVerified = true,
                    Role = UserRole.Teacher
                });
            }
        }

        public User AuthenticateUser(string username, string password)
        {
            return _users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public bool IsUsernameTaken(string username)
        {
            return _users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public List<User> GetAllUsers()
        {
            return _users;
        }

        public List<Teacher> GetAllTeachers()
        {
            return _users.OfType<Teacher>().Where(t => t.IsVerified).ToList();
        }

        public User GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }
        public User GetUserById(string id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public bool RegisterUser(User newUser)
        {
            if (newUser == null)
            {
                MessageBox.Show("Не вдалося зареєструвати: об'єкт користувача null.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrEmpty(newUser.Id) || newUser.Id == Guid.Empty.ToString())
            {
                newUser.Id = Guid.NewGuid().ToString();
            }

            if (!IsUsernameTaken(newUser.Username))
            {
                _users.Add(newUser);
                SaveUsers();
                return true;
            }
            else
            {
                MessageBox.Show("Користувач з таким іменем вже існує.", "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        public void UpdateUser(User updatedUser)
        {
            if (updatedUser == null)
            {
                Console.WriteLine("UpdateUser: updatedUser is null.");
                return;
            }

            var existingUser = _users.FirstOrDefault(u => u.Id == updatedUser.Id);

            if (existingUser != null)
            {
                int index = _users.IndexOf(existingUser);
                _users[index] = updatedUser;
                SaveUsers();
            }
            else
            {
                MessageBox.Show($"Користувача з ID '{updatedUser.Id}' не знайдено для оновлення. Спроба оновити неіснуючого користувача.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"UpdateUser: User with ID {updatedUser.Id} not found.");
            }
        }

        public void UpdateStudentBalance(string username, decimal newBalance)
        {
            var student = _users.OfType<Student>().FirstOrDefault(s => s.Username == username);
            if (student != null)
            {
                student.Balance = newBalance;
                UpdateUser(student);
            }
            else
            {
                MessageBox.Show($"Студента з логіном '{username}' не знайдено для оновлення балансу.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void DeleteUser(string userId)
        {
            var userToRemove = _users.FirstOrDefault(u => u.Id == userId);
            if (userToRemove != null)
            {
                _users.Remove(userToRemove);
                SaveUsers();
                Console.WriteLine($"Користувач з ID {userId} ({userToRemove.Username}) успішно видалений.");
            }
            else
            {
                MessageBox.Show($"Користувача з ID '{userId}' не знайдено для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Помилка: Користувача з ID {userId} не знайдено для видалення.");
            }
        }
    }
}