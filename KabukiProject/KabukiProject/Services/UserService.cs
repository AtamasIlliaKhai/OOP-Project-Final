using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KabukiProject.Enums;
using KabukiProject.Interfaces;
using KabukiProject.Models; // User, Student, Teacher
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO; // Додано для Path.Combine та Directory


namespace KabukiProject.Services
{
    public class UserService
    {
        private readonly IJsonStorageService<Student> _studentStorage;
        private readonly IJsonStorageService<Teacher> _teacherStorage;
        private readonly IJsonStorageService<Administrator> _administratorStorage;

        // Шляхи для JSON файлів
        // Забезпечуємо, що шлях відносний до папки, де виконується EXE-файл.
        // const string - це добре, оскільки ці значення не змінюються.
        private const string DataDirectory = "Data"; // Нова константа для папки
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

            // Цей блок додає фіктивних користувачів лише при першому запуску,
            // якщо файли даних порожні. Видаліть його після тестування,
            // або коли ви будете мати реальні дані.
            InitializeDummyDataIfEmpty();
        }

        // Новий метод для початкової ініціалізації даних
        private void InitializeDummyDataIfEmpty()
        {
            List<Student> students = _studentStorage.LoadData(StudentsFilePath);
            List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
            List<Administrator> administrators = _administratorStorage.LoadData(AdministratorsFilePath);

            bool needsSave = false;

            // Додаємо фіктивного учня, якщо немає жодного
            if (!students.Any())
            {
                students.Add(new Student
                {
                    Username = "student1",
                    Password = "123", // Паролі повинні бути хешовані в реальному застосунку
                    FirstName = "Олексій",
                    LastName = "Учень",
                    Email = "student1@example.com",
                    Balance = 500.00m
                });
                needsSave = true;
            }

            // Додаємо фіктивних викладачів, якщо немає жодного
            if (!teachers.Any())
            {
                teachers.Add(new Teacher
                {
                    Username = "teacher1",
                    Password = "123",
                    FirstName = "Олена",
                    LastName = "Іванова",
                    Description = "Досвідчений викладач математики та фізики з 10-річним стажем.",
                    PricePerHour = 300.00m,
                    Subjects = new List<string> { "Математика", "Фізика" },
                    PhotoPath = "", // Шлях до фото, якщо воно буде
                    IsVerified = true
                });
                teachers.Add(new Teacher
                {
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
                teachers.Add(new Teacher
                {
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
            if (!administrators.Any())
            {
                administrators.Add(new Administrator
                {
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
                _studentStorage.SaveData(students, StudentsFilePath);
                _teacherStorage.SaveData(teachers, TeachersFilePath);
                _administratorStorage.SaveData(administrators, AdministratorsFilePath);
            }
        }


        public User AuthenticateUser(string username, string password)
        {
            // Завантажуємо дані щоразу, щоб працювати з актуальними файлами.
            // Хоча для кожного запиту це може бути неефективно на великих даних,
            // для JSON файлів це прийнятно.
            List<Student> students = _studentStorage.LoadData(StudentsFilePath);
            List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
            List<Administrator> administrators = _administratorStorage.LoadData(AdministratorsFilePath);

            User user = students.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
            if (user != null) return user;

            user = teachers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
            if (user != null) return user;

            user = administrators.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);
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

            if (newUser.Role == UserRole.Student)
            {
                List<Student> students = _studentStorage.LoadData(StudentsFilePath);
                students.Add(newUser as Student);
                _studentStorage.SaveData(students, StudentsFilePath);
            }
            else if (newUser.Role == UserRole.Teacher)
            {
                List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
                teachers.Add(newUser as Teacher);
                _teacherStorage.SaveData(teachers, TeachersFilePath);
            }
            else if (newUser.Role == UserRole.Administrator)
            {
                List<Administrator> admins = _administratorStorage.LoadData(AdministratorsFilePath);
                admins.Add(newUser as Administrator);
                _administratorStorage.SaveData(admins, AdministratorsFilePath);
            }
            return true;
        }

        public bool IsUsernameTaken(string username)
        {
            // Перевіряємо в усіх типах користувачів
            List<Student> students = _studentStorage.LoadData(StudentsFilePath);
            if (students.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))) return true;

            List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
            if (teachers.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))) return true;

            List<Administrator> administrators = _administratorStorage.LoadData(AdministratorsFilePath);
            if (administrators.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase))) return true;

            return false;
        }

        public List<Teacher> GetAllTeachers()
        {
            return _teacherStorage.LoadData(TeachersFilePath);
        }

        public User GetUserByUsername(string username)
        {
            // Шукаємо серед студентів
            List<Student> students = _studentStorage.LoadData(StudentsFilePath);
            Student student = students.FirstOrDefault(s => s.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (student != null) return student;

            // Шукаємо серед викладачів
            List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
            Teacher teacher = teachers.FirstOrDefault(t => t.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (teacher != null) return teacher;

            // Шукаємо серед адміністраторів
            List<Administrator> administrators = _administratorStorage.LoadData(AdministratorsFilePath);
            Administrator administrator = administrators.FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (administrator != null) return administrator;

            return null;
        }

        public void UpdateUser(User updatedUser)
        {
            if (updatedUser == null) return;

            if (updatedUser.Role == UserRole.Student)
            {
                List<Student> students = _studentStorage.LoadData(StudentsFilePath);
                var existingStudent = students.FirstOrDefault(s => s.Username == updatedUser.Username);
                if (existingStudent != null && updatedUser is Student newStudentData)
                {
                    // Оновлюємо всі властивості студента
                    existingStudent.Password = newStudentData.Password;
                    existingStudent.FirstName = newStudentData.FirstName;
                    existingStudent.LastName = newStudentData.LastName;
                    existingStudent.Email = newStudentData.Email;
                    existingStudent.Balance = newStudentData.Balance;
                }
                _studentStorage.SaveData(students, StudentsFilePath);
            }
            else if (updatedUser.Role == UserRole.Teacher)
            {
                List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
                var existingTeacher = teachers.FirstOrDefault(t => t.Username == updatedUser.Username);
                if (existingTeacher != null && updatedUser is Teacher newTeacherData)
                {
                    // Оновлюємо всі властивості викладача
                    existingTeacher.Password = newTeacherData.Password;
                    existingTeacher.FirstName = newTeacherData.FirstName;
                    existingTeacher.LastName = newTeacherData.LastName;
                    existingTeacher.Description = newTeacherData.Description;
                    existingTeacher.PricePerHour = newTeacherData.PricePerHour;
                    existingTeacher.PhotoPath = newTeacherData.PhotoPath;
                    existingTeacher.IsVerified = newTeacherData.IsVerified;
                    existingTeacher.Subjects = newTeacherData.Subjects;
                }
                _teacherStorage.SaveData(teachers, TeachersFilePath);
            }
            else if (updatedUser.Role == UserRole.Administrator)
            {
                List<Administrator> administrators = _administratorStorage.LoadData(AdministratorsFilePath);
                var existingAdmin = administrators.FirstOrDefault(a => a.Username == updatedUser.Username);
                if (existingAdmin != null && updatedUser is Administrator newAdminData)
                {
                    // Оновлюємо властивості адміністратора
                    existingAdmin.Password = newAdminData.Password;
                    existingAdmin.FirstName = newAdminData.FirstName;
                    existingAdmin.LastName = newAdminData.LastName;
                }
                _administratorStorage.SaveData(administrators, AdministratorsFilePath);
            }
        }
    }
}