using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KabukiProject.Views; // навігація LoginView
using KabukiProject.Models; // доступ до моделі Teacher та User, Lesson, Student
using KabukiProject.Services; // доступ до UserService, LessonService
using KabukiProject.Interfaces; // Додано для ITeacher на всяк випадок (якщо використовується)
using System.Collections.ObjectModel; // ObservableCollection

namespace KabukiProject.ViewModels
{
    public class TeacherDashboardViewModel : BaseViewModel
    {
        // Властивості для UI (прив'язані до полів вводу)
        private string _currentUserName;
        public string CurrentUserName
        {
            get => _currentUserName;
            set
            {
                _currentUserName = value;
                OnPropertyChanged();
            }
        }

        // ВЛАСТИВОСТІ ПРОФІЛЮ ВИКЛАДАЧА
        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); SaveProfileCommand.RaiseCanExecuteChanged(); }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); SaveProfileCommand.RaiseCanExecuteChanged(); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); SaveProfileCommand.RaiseCanExecuteChanged(); }
        }

        private decimal _pricePerHour;
        public decimal PricePerHour
        {
            get => _pricePerHour;
            set { _pricePerHour = value; OnPropertyChanged(); SaveProfileCommand.RaiseCanExecuteChanged(); }
        }

        private string _photoPath;
        public string PhotoPath
        {
            get => _photoPath;
            set { _photoPath = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Lesson> _teacherLessons;
        public ObservableCollection<Lesson> TeacherLessons
        {
            get => _teacherLessons;
            set
            {
                _teacherLessons = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasLessons)); // Додаємо OnPropertyChanged для нової властивості
            }
        }

        public bool HasLessons => TeacherLessons != null && TeacherLessons.Any();

        public ObservableCollection<string> Subjects { get; set; }

        // Команди
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand SaveProfileCommand { get; private set; }

        private Teacher _loggedInTeacher; // Зберігаємо посилання на поточного викладача

        // КОНСТРУКТОР приймає об'єкт User (який має бути Teacher)
        public TeacherDashboardViewModel(User loggedInUser)
        {
            LogoutCommand = new RelayCommand(ExecuteLogout);
            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile, CanExecuteSaveProfile);

            Subjects = new ObservableCollection<string>(); // Ініціалізуємо колекцію
            TeacherLessons = new ObservableCollection<Lesson>(); // Ініціалізуємо колекцію уроків

            // перевіряємо, чи переданий користувач є викладачем
            if (loggedInUser is Teacher teacher)
            {
                _loggedInTeacher = teacher;
                LoadTeacherProfile(teacher.Username); // Завантажуємо повний профіль за ім'ям

                // Тепер, коли _loggedInTeacher ініціалізовано, завантажуємо уроки
                LoadTeacherLessons();
            }
            else
            {
                // Це не повинно статися, якщо логіка входу коректна, але на всяк випадок
                MessageBox.Show("Помилка: Користувач не є викладачем.", "Помилка авторизації", MessageBoxButton.OK, MessageBoxImage.Error);
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
            }
        }

        // Конструктор без параметрів для дизайнера XAML
        public TeacherDashboardViewModel()
        {
            LogoutCommand = new RelayCommand(ExecuteLogout);
            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile, CanExecuteSaveProfile);
            Subjects = new ObservableCollection<string>();
            TeacherLessons = new ObservableCollection<Lesson>(); // Ініціалізуємо для дизайнера

            // Фіктивні дані для дизайнера
            CurrentUserName = "Ім'я Викладача (Design)";
            FirstName = "Олена (Design)";
            LastName = "Викладач (Design)";
            Description = "Досвідчений викладач математики (Design)";
            PricePerHour = 300.00m;
            PhotoPath = "";
            Subjects.Add("Математика");
            Subjects.Add("Фізика");

            // Фіктивні уроки для дизайнера
            TeacherLessons.Add(new Lesson
            {
                DateTime = DateTime.Now.AddDays(1).AddHours(10),
                Subject = "Математика",
                Status = LessonStatus.Scheduled,
                Student = new Student { Id = Guid.NewGuid().ToString(), Username = "Іван Петров", FirstName = "Іван", LastName = "Петров" }
            });
            TeacherLessons.Add(new Lesson
            {
                DateTime = DateTime.Now.AddDays(2).AddHours(14),
                Subject = "Фізика",
                Status = LessonStatus.Scheduled,
                Student = new Student { Id = Guid.NewGuid().ToString(), Username = "Ольга Сидорова", FirstName = "Ольга", LastName = "Сидорова" }
            });
            TeacherLessons.Add(new Lesson
            {
                DateTime = DateTime.Now.AddDays(-5).AddHours(11),
                Subject = "Хімія",
                Status = LessonStatus.Completed,
                Student = new Student { Id = Guid.NewGuid().ToString(), Username = "Сергій Коваленко", FirstName = "Сергій", LastName = "Коваленко" }
            });
        }


        // Методи команд та логіки

        private void LoadTeacherProfile(string username)
        {
            User user = UserService.Instance.GetUserByUsername(username);
            if (user is Teacher teacher)
            {
                _loggedInTeacher = teacher;
                CurrentUserName = teacher.Username;
                FirstName = teacher.FirstName;
                LastName = teacher.LastName;
                Description = teacher.Description;
                PricePerHour = teacher.PricePerHour;
                PhotoPath = teacher.PhotoPath;

                Subjects.Clear();
                if (teacher.Subjects != null)
                {
                    foreach (var subject in teacher.Subjects)
                    {
                        Subjects.Add(subject);
                    }
                }
            }
            else
            {
                MessageBox.Show("Не вдалося завантажити профіль викладача. Можливо, дані пошкоджені.", "Помилка завантаження", MessageBoxButton.OK, MessageBoxImage.Error);
                FirstName = "";
                LastName = "";
                Description = "";
                PricePerHour = 0;
                PhotoPath = "";
                Subjects.Clear();
            }
        }

        // НОВИЙ МЕТОД ДЛЯ ЗАВАНТАЖЕННЯ УРОКІВ
        private void LoadTeacherLessons()
        {
            if (_loggedInTeacher == null)
            {
                TeacherLessons.Clear();
                return;
            }

            var allLessons = LessonService.Instance.GetLessonsByTeacherId(_loggedInTeacher.Id)
                                                    .OrderBy(l => l.DateTime) // Сортуємо за датою/часом
                                                    .ToList();

            TeacherLessons.Clear(); // Очищаємо поточну колекцію перед оновленням

            foreach (var lesson in allLessons)
            {
                lesson.Student = UserService.Instance.GetUserById(lesson.StudentId) as Student;
                TeacherLessons.Add(lesson);
            }

            OnPropertyChanged(nameof(HasLessons)); // Оновлюємо стан для UI
        }


        private bool CanExecuteSaveProfile(object parameter)
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   PricePerHour > 0 &&
                   Subjects.Any();
        }

        private void ExecuteSaveProfile(object parameter)
        {
            if (_loggedInTeacher != null)
            {
                _loggedInTeacher.FirstName = FirstName;
                _loggedInTeacher.LastName = LastName;
                _loggedInTeacher.Description = Description;
                _loggedInTeacher.PricePerHour = PricePerHour;
                _loggedInTeacher.PhotoPath = PhotoPath;
                _loggedInTeacher.Subjects = Subjects.ToList();

                UserService.Instance.UpdateUser(_loggedInTeacher);

                MessageBox.Show("Профіль успішно збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Не вдалося зберегти профіль: користувач не визначений.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteLogout(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                MessageBox.Show("Вихід з облікового запису викладача.", "Вихід", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
                currentWindow.Close();
            }
        }
    }
}