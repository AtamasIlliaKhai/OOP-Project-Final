using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KabukiProject.Enums;
using KabukiProject.Interfaces;
using KabukiProject.Models;
using System.IO;
using System.Windows;

namespace KabukiProject.Services
{
    public class UserService
    {
        public void SaveAllData()
        {
            _studentStorage.SaveData(_students, StudentsFilePath);
            _teacherStorage.SaveData(_teachers, TeachersFilePath);
            _administratorStorage.SaveData(_administrators, AdministratorsFilePath);
        }

        private readonly IJsonStorageService<Student> _studentStorage;
        private readonly IJsonStorageService<Teacher> _teacherStorage;
        private readonly IJsonStorageService<Administrator> _administratorStorage;

        private List<Student> _students;
        private List<Teacher> _teachers;
        private List<Administrator> _administrators;

        private const string DataDirectory = "Data";
        private const string StudentsFileName = "students.json";
        private const string TeachersFileName = "teachers.json";
        private const string AdministratorsFileName = "administrators.json";

        private string StudentsFilePath => Path.Combine(DataDirectory, StudentsFileName);
        private string TeachersFilePath => Path.Combine(DataDirectory, TeachersFileName);
        private string AdministratorsFilePath => Path.Combine(DataDirectory, AdministratorsFileName);

        public UserService()
        {
            _studentStorage = new JsonStorageService<Student>();
            _teacherStorage = new JsonStorageService<Teacher>();
            _administratorStorage = new JsonStorageService<Administrator>();

            // Завантажуємо дані при ініціалізації сервісу
            _students = _studentStorage.LoadData(StudentsFilePath);
            _teachers = _teacherStorage.LoadData(TeachersFilePath);
            _administrators = _administratorStorage.LoadData(AdministratorsFilePath);

            InitializeDummyDataIfEmpty();
        }

        private void InitializeDummyDataIfEmpty()
        {
            bool needsSave = false;

            // Додаємо фіктивного учня, якщо немає жодного
            if (!_students.Any())
            {
                _students.Add(new Student
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "student1",
                    Password = "123",
                    FirstName = "Олексій",
                    LastName = "Учень",
                    Email = "student1@example.com",
                    Balance = 500.00m
                });
                needsSave = true;
            }

            // Додаємо фіктивних викладачів, якщо немає жодного
            if (!_teachers.Any())
            {
                _teachers.Add(new Teacher
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "teacher1",
                    Password = "123",
                    FirstName = "Олена",
                    LastName = "Іванова",
                    Description = "Досвідчений викладач математики та фізики з 10-річним стажем.",
                    PricePerHour = 300.00m,
                    Subjects = new List<string> { "Математика", "Фізика" },
                    PhotoPath = "",
                    IsVerified = true
                });
                _teachers.Add(new Teacher
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "teacher2",
                    Password = "123",
                    FirstName = "Сергій",
                    LastName = "Петров",
                    Description = "Викладач програмування (C#, Python) для початківців.",
                    PricePerHour = 350.00m,
                    Subjects = new List<string> { "Програмування", "C#", "Python" },
                    PhotoPath = "",
                    IsVerified = true
                });
                _teachers.Add(new Teacher
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "teacher3",
                    Password = "123",
                    FirstName = "Анна",
                    LastName = "Коваленко",
                    Description = "Викладач англійської мови для всіх рівнів. Підготовка до ЗНО/ІЕЛТС.",
                    PricePerHour = 280.00m,
                    Subjects = new List<string> { "Англійська мова", "Граматика", "Розмовна практика" },
                    PhotoPath = "",
                    IsVerified = true
                });
                needsSave = true;
            }

            // Додаємо фіктивного адміністратора, якщо немає жодного
            if (!_administrators.Any())
            {
                _administrators.Add(new Administrator
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "admin1",
                    Password = "123",
                    FirstName = "Головний",
                    LastName = "Адмін"
                });
                needsSave = true;
            }

            // Зберігаємо дані, якщо були додані нові користувачі
            if (needsSave)
            {
                _studentStorage.SaveData(_students, StudentsFilePath);
                _teacherStorage.SaveData(_teachers, TeachersFilePath);
                _administratorStorage.SaveData(_administrators, AdministratorsFilePath);
            }
        }

        public User AuthenticateUser(string username, string password)
        {
            User user = _students.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
            if (user != null) return user;

            user = _teachers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
            if (user != null) return user;

            user = _administrators.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
            if (user != null) return user;

            return null;
        }

        public bool RegisterUser(User newUser)
        {
            if (IsUsernameTaken(newUser.Username))
            {
                MessageBox.Show("Користувач з таким ім'ям вже існує.", "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Встановлюємо ID для нового користувача, якщо його ще немає
            if (string.IsNullOrEmpty(newUser.Id))
            {
                newUser.Id = Guid.NewGuid().ToString();
            }

            if (newUser.Role == UserRole.Student)
            {
                if (newUser is Student studentToAdd)
                {
                    _students.Add(studentToAdd);
                    _studentStorage.SaveData(_students, StudentsFilePath);
                }
                else
                {
                    MessageBox.Show("Помилка: не вдалося зареєструвати студента. Неправильний тип об'єкта.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            else if (newUser.Role == UserRole.Teacher)
            {
                if (newUser is Teacher teacherToAdd)
                {
                    _teachers.Add(teacherToAdd);
                    _teacherStorage.SaveData(_teachers, TeachersFilePath);
                }
                else
                {
                    MessageBox.Show("Помилка: не вдалося зареєструвати викладача. Неправильний тип об'єкта.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            else if (newUser.Role == UserRole.Administrator)
            {
                if (newUser is Administrator adminToAdd)
                {
                    _administrators.Add(adminToAdd);
                    _administratorStorage.SaveData(_administrators, AdministratorsFilePath);
                }
                else
                {
                    MessageBox.Show("Помилка: не вдалося зареєструвати адміністратора. Неправильний тип об'єкта.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
        }

        public bool IsUsernameTaken(string username)
        {
            if (_students.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))) return true;
            if (_teachers.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))) return true;
            if (_administrators.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))) return true;

            return false;
        }

        public List<Teacher> GetAllTeachers()
        {
            return _teachers;
        }

        public User GetUserByUsername(string username)
        {
            User user = _students.FirstOrDefault(s => s.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user != null) return user;

            user = _teachers.FirstOrDefault(t => t.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user != null) return user;

            user = _administrators.FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user != null) return user;

            return null;
        }

        public void UpdateUser(User updatedUser)
        {
            if (updatedUser == null) return;

            if (updatedUser.Role == UserRole.Student)
            {
                var existingStudent = _students.FirstOrDefault(s => s.Username == updatedUser.Username);
                if (existingStudent != null && updatedUser is Student newStudentData)
                {
                    existingStudent.Password = newStudentData.Password;
                    existingStudent.FirstName = newStudentData.FirstName;
                    existingStudent.LastName = newStudentData.LastName;
                    existingStudent.Email = newStudentData.Email;
                    existingStudent.Balance = newStudentData.Balance;
                }
                _studentStorage.SaveData(_students, StudentsFilePath);
            }
            else if (updatedUser.Role == UserRole.Teacher)
            {
                var existingTeacher = _teachers.FirstOrDefault(t => t.Username == updatedUser.Username);
                if (existingTeacher != null && updatedUser is Teacher newTeacherData)
                {
                    existingTeacher.Password = newTeacherData.Password;
                    existingTeacher.FirstName = newTeacherData.FirstName;
                    existingTeacher.LastName = newTeacherData.LastName;
                    existingTeacher.Description = newTeacherData.Description;
                    existingTeacher.PricePerHour = newTeacherData.PricePerHour;
                    existingTeacher.PhotoPath = newTeacherData.PhotoPath;
                    existingTeacher.IsVerified = newTeacherData.IsVerified;
                    existingTeacher.Subjects = newTeacherData.Subjects;
                }
                _teacherStorage.SaveData(_teachers, TeachersFilePath);
            }
            else if (updatedUser.Role == UserRole.Administrator)
            {
                var existingAdmin = _administrators.FirstOrDefault(a => a.Username == updatedUser.Username);
                if (existingAdmin != null && updatedUser is Administrator newAdminData)
                {
                    existingAdmin.Password = newAdminData.Password;
                    existingAdmin.FirstName = newAdminData.FirstName;
                    existingAdmin.LastName = newAdminData.LastName;
                }
                _administratorStorage.SaveData(_administrators, AdministratorsFilePath);
            }
        }
    }
}