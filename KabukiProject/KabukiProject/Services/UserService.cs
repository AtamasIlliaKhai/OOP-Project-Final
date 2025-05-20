using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using KabukiProject.Enums;
using KabukiProject.Interfaces;
using KabukiProject.Models; //User, Student, Teacher

namespace KabukiProject.Services
{
    public class UserService
    {
        private readonly IJsonStorageService<User> _userStorage;
        private readonly IJsonStorageService<Student> _studentStorage;
        private readonly IJsonStorageService<Teacher> _teacherStorage;

        //Шляхи для json ів
        private const string UsersFilePath = "Data/users.json"; //Типу якщо на все один
        private const string StudentsFilePath = "Data/students.json";
        private const string TeachersFilePath = "Data/teachers.json";
        private const string AdministratorsFilePath = "Data/administrators.json";

        public UserService()
        {
            //Різні місця збереження для безпеки
            _userStorage = new JsonStorageService<User>(); //Для юзерів в одному файлі
            _studentStorage = new JsonStorageService<Student>();
            _teacherStorage = new JsonStorageService<Teacher>();
            //Для адмінів:
            // _adminStorage = new JsonStorageService<Administrator>();
        }

        public User AuthenticateUser(string username, string password)
        {
            //Завантажити всіх юзерів з їх відповідних файлів
            List<Student> students = _studentStorage.LoadData(StudentsFilePath);
            List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
            List<Administrator> administrators = new JsonStorageService<Administrator>().LoadData(AdministratorsFilePath); //Типу окремий файл

            //Чекнути студентів
            User user = students.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null) return user;

            //Чекнути викладачів
            user = teachers.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null) return user;

            //Чекнути адмінів
            user = administrators.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null) return user;

            return null; //Користувач не знайдений
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
                //Краще це в адмінів засунути
                List<Administrator> admins = new JsonStorageService<Administrator>().LoadData(AdministratorsFilePath);
                admins.Add(newUser as Administrator);
                new JsonStorageService<Administrator>().SaveData(admins, AdministratorsFilePath);
            }
            return true;
        }

        public bool IsUsernameTaken(string username)
        {
            //Перевірити у всіх типах
            List<Student> students = _studentStorage.LoadData(StudentsFilePath);
            if (students.Any(u => u.Username == username)) return true;

            List<Teacher> teachers = _teacherStorage.LoadData(TeachersFilePath);
            if (teachers.Any(u => u.Username == username)) return true;

            List<Administrator> administrators = new JsonStorageService<Administrator>().LoadData(AdministratorsFilePath);
            if (administrators.Any(u => u.Username == username)) return true;

            return false;
        }
    }
}