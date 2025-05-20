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


namespace KabukiProject.Services
{
    public class UserService
    {
        // Не використовуємо IJsonStorageService<User> для окремих ролей, бо дані розділені
        private readonly IJsonStorageService<Student> _studentStorage;
        private readonly IJsonStorageService<Teacher> _teacherStorage;
        private readonly IJsonStorageService<Administrator> _administratorStorage; // Додаємо для консистентності


        // Шляхи для json файлів
        // Передивишся потфім, мб переробиш. Не впевнений, що воно константою має буть
        private const string StudentsFilePath = "Data/students.json";
        private const string TeachersFilePath = "Data/teachers.json";
        private const string AdministratorsFilePath = "Data/administrators.json";

        public UserService()
        {
            // Різні місця збереження для безпеки
            _studentStorage = new JsonStorageService<Student>();
            _teacherStorage = new JsonStorageService<Teacher>();
            _administratorStorage = new JsonStorageService<Administrator>(); // Ініціалізуємо
        }

        public User AuthenticateUser(string username, string password)
        {
            // Завантажити всіх юзерів з їх відповідних файлів, передаючи filePath
            List<Student> students = _studentStorage.LoadData(StudentsFilePath);
            List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
            List<Administrator> administrators = _administratorStorage.LoadData(AdministratorsFilePath);

            // Чекнути студентів
            User user = students.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null) return user;

            // Чекнути викладачів
            user = teachers.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null) return user;

            // Чекнути адмінів
            user = administrators.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null) return user;

            return null; // Користувач не знайдений
        }

        public bool RegisterUser(User newUser)
        {
            if (IsUsernameTaken(newUser.Username))
            {
                return false; // Username already exists
            }

            if (newUser.Role == UserRole.Student)
            {
                List<Student> students = _studentStorage.LoadData(StudentsFilePath);
                students.Add(newUser as Student);
                _studentStorage.SaveData(students, StudentsFilePath); // Передаємо filePath
            }
            else if (newUser.Role == UserRole.Teacher)
            {
                List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
                teachers.Add(newUser as Teacher);
                _teacherStorage.SaveData(teachers, TeachersFilePath); // Передаємо filePath
            }
            else if (newUser.Role == UserRole.Administrator)
            {
                List<Administrator> admins = _administratorStorage.LoadData(AdministratorsFilePath);
                admins.Add(newUser as Administrator);
                _administratorStorage.SaveData(admins, AdministratorsFilePath); // Передаємо filePath
            }
            return true;
        }

        public bool IsUsernameTaken(string username)
        {
            // Check in all user types
            List<Student> students = _studentStorage.LoadData(StudentsFilePath);
            if (students.Any(u => u.Username == username)) return true;

            List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
            if (teachers.Any(u => u.Username == username)) return true;

            List<Administrator> administrators = _administratorStorage.LoadData(AdministratorsFilePath);
            if (administrators.Any(u => u.Username == username)) return true;

            return false;
        }

        public List<Teacher> GetAllTeachers()
        {
            // Використовуємо наш _teacherStorage для завантаження даних
            return _teacherStorage.LoadData(TeachersFilePath);
        }

        /// <summary>
        /// Повертає об'єкт користувача (Student, Teacher або Administrator) за його ім'ям користувача.
        /// </summary>
        /// <param name="username">Ім'я користувача для пошуку.</param>
        /// <returns>Об'єкт User або null, якщо користувач не знайдений.</returns>
        public User GetUserByUsername(string username)
        {
            // Шукаємо серед студентів
            List<Student> students = _studentStorage.LoadData(StudentsFilePath);
            Student student = students.FirstOrDefault(s => s.Username == username);
            if (student != null) return student;

            // Шукаємо серед викладачів
            List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
            Teacher teacher = teachers.FirstOrDefault(t => t.Username == username);
            if (teacher != null) return teacher;

            // Шукаємо серед адміністраторів
            List<Administrator> administrators = _administratorStorage.LoadData(AdministratorsFilePath);
            Administrator administrator = administrators.FirstOrDefault(a => a.Username == username);
            if (administrator != null) return administrator;

            return null; // Користувач не знайдений
        }

        /// <summary>
        /// Оновлює дані існуючого користувача та зберігає їх у відповідному файлі.
        /// </summary>
        /// <param name="updatedUser">Об'єкт користувача з оновленими даними.</param>
        public void UpdateUser(User updatedUser)
        {
            if (updatedUser == null) return;

            if (updatedUser.Role == UserRole.Student)
            {
                List<Student> students = _studentStorage.LoadData(StudentsFilePath);
                var existingStudent = students.FirstOrDefault(s => s.Username == updatedUser.Username);
                if (existingStudent != null && updatedUser is Student newStudentData)
                {
                    // Оновлюємо властивості студента
                    existingStudent.Password = newStudentData.Password; // Якщо потрібно оновити пароль
                    // Додайте інші специфічні для студента властивості, які потрібно оновити
                }
                _studentStorage.SaveData(students, StudentsFilePath); // Передаємо filePath
            }
            else if (updatedUser.Role == UserRole.Teacher)
            {
                List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
                var existingTeacher = teachers.FirstOrDefault(t => t.Username == updatedUser.Username);
                if (existingTeacher != null && updatedUser is Teacher newTeacherData)
                {
                    // Оновлюємо властивості викладача
                    existingTeacher.Password = newTeacherData.Password; // Якщо потрібно оновити пароль
                    existingTeacher.FirstName = newTeacherData.FirstName;
                    existingTeacher.LastName = newTeacherData.LastName;
                    existingTeacher.Description = newTeacherData.Description;
                    existingTeacher.PricePerHour = newTeacherData.PricePerHour; // Виправлено на PricePerHour
                    existingTeacher.PhotoPath = newTeacherData.PhotoPath;
                    existingTeacher.IsVerified = newTeacherData.IsVerified;
                    existingTeacher.Subjects = newTeacherData.Subjects;
                    // Щось ще добавить
                }
                _teacherStorage.SaveData(teachers, TeachersFilePath); // Передаємо filePath
            }
            else if (updatedUser.Role == UserRole.Administrator)
            {
                List<Administrator> administrators = _administratorStorage.LoadData(AdministratorsFilePath);
                var existingAdmin = administrators.FirstOrDefault(a => a.Username == updatedUser.Username);
                if (existingAdmin != null && updatedUser is Administrator newAdminData)
                {
                    // Оновлюємо властивості адміністратора
                    existingAdmin.Password = newAdminData.Password; // Якщо потрібно оновити пароль
                    // Додайте інші специфічні для адміністратора властивості, які потрібно оновити
                }
                _administratorStorage.SaveData(administrators, AdministratorsFilePath); // Передаємо filePath
            }
        }
    }
}