using KabukiProject.Enums;
using KabukiProject.Models;
using KabukiProject.Services;
using KabukiProject.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using KabukiProject.ViewModels;

namespace KabukiProject.ViewModels
{
    public class StudentDashboardViewModel : BaseViewModel
    {
        private Student _currentStudent;

        public Student CurrentStudent
        {
            get => _currentStudent;
            set
            {
                _currentStudent = value;
                OnPropertyChanged();
                CurrentUserName = _currentStudent?.Username;
                FirstName = _currentStudent?.FirstName;
                LastName = _currentStudent?.LastName;
                CurrentUserBalance = _currentStudent?.Balance ?? 0;
                LoadStudentLessons(); // Завантажуємо уроки при зміні студента
            }
        }

        private string _currentUserName;
        public string CurrentUserName
        {
            get => _currentUserName;
            set { _currentUserName = value; OnPropertyChanged(); }
        }

        private decimal _currentUserBalance;
        public decimal CurrentUserBalance
        {
            get => _currentUserBalance;
            set { _currentUserBalance = value; OnPropertyChanged(); }
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        // Властивості для пошуку репетиторів
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                ExecuteSearchTeachers(null); // Автоматичний пошук при зміні запиту
            }
        }

        private ObservableCollection<Teacher> _foundTeachers;
        public ObservableCollection<Teacher> FoundTeachers
        {
            get => _foundTeachers;
            set
            {
                _foundTeachers = value;
                OnPropertyChanged();
            }
        }

        private Teacher _selectedTeacher;
        public Teacher SelectedTeacher
        {
            get => _selectedTeacher;
            set
            {
                _selectedTeacher = value;
                OnPropertyChanged();
                if (_selectedTeacher != null)
                {
                    OpenTeacherProfile(_selectedTeacher);
                }
            }
        }

        // Для вкладки "Мої уроки" - ТЕПЕР ObservableCollection<LessonDisplayViewModel>
        private ObservableCollection<LessonDisplayViewModel> _studentLessons;
        public ObservableCollection<LessonDisplayViewModel> StudentLessons
        {
            get => _studentLessons;
            set
            {
                _studentLessons = value;
                OnPropertyChanged();
            }
        }

        // Команди
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand SearchTeachersCommand { get; private set; }
        public RelayCommand TopUpBalanceCommand { get; private set; }

        private List<Teacher> _allTeachers;

        // Основний конструктор для входу користувача
        public StudentDashboardViewModel(User loggedInUser)
        {
            // Ініціалізуємо властивості
            FoundTeachers = new ObservableCollection<Teacher>();
            // Виправлено: ініціалізуємо StudentLessons як LessonDisplayViewModel
            StudentLessons = new ObservableCollection<LessonDisplayViewModel>();

            // Ініціалізуємо команди
            LogoutCommand = new RelayCommand(ExecuteLogout);
            SearchTeachersCommand = new RelayCommand(ExecuteSearchTeachers, CanExecuteSearchTeachers);
            TopUpBalanceCommand = new RelayCommand(ExecuteTopUpBalance);

            if (loggedInUser is Student student)
            {
                CurrentStudent = student;
            }
            else
            {
                MessageBox.Show("Помилка: Користувач не є учнем. Будь ласка, увійдіть знову.", "Помилка авторизації", MessageBoxButton.OK, MessageBoxImage.Error);
                // Забезпечуємо перехід на LoginView
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
            }

            _allTeachers = UserService.Instance.GetAllTeachers();
            ExecuteSearchTeachers(null); // Виконуємо початковий пошук при завантаженні
        }


        public StudentDashboardViewModel()
        {
            FoundTeachers = new ObservableCollection<Teacher>();
            StudentLessons = new ObservableCollection<LessonDisplayViewModel>();

            LogoutCommand = new RelayCommand(ExecuteLogout);
            SearchTeachersCommand = new RelayCommand(ExecuteSearchTeachers, CanExecuteSearchTeachers);
            TopUpBalanceCommand = new RelayCommand(ExecuteTopUpBalance);

            CurrentStudent = new Student
            {
                Id = Guid.NewGuid().ToString(),
                Username = "test_student",
                FirstName = "Іван",
                LastName = "Студентенко",
                Role = UserRole.Student,
                Balance = 1500.00m
            };

            _allTeachers = UserService.Instance.GetAllTeachers();
            if (_allTeachers == null || !_allTeachers.Any())
            {
                _allTeachers = new List<Teacher>
                {
                    new Teacher { Id = "test_teacher_1", FirstName = "Анна", LastName = "Іванова", PricePerHour = 250m, Description = "Досвідчений викладач математики.", Subjects = new List<string> { "Математика", "Фізика" }, IsVerified = true },
                    new Teacher { Id = "test_teacher_2", FirstName = "Петро", LastName = "Сидоров", PricePerHour = 300m, Description = "Фахівець з програмування.", Subjects = new List<string> { "Програмування", "ІТ" }, IsVerified = true }
                };
            }
            ExecuteSearchTeachers(null);
        }

        private void LoadStudentLessons()
        {
            StudentLessons.Clear();
            if (CurrentStudent != null)
            {
                var lessons = LessonService.Instance.GetLessonsByStudentId(CurrentStudent.Id)
                                                     .OrderByDescending(l => l.DateTime);

                foreach (var lesson in lessons)
                {
                    var teacher = UserService.Instance.GetUserById(lesson.TeacherId) as Teacher;
                    StudentLessons.Add(new LessonDisplayViewModel(lesson, teacher));
                }
            }
        }

        private bool CanExecuteSearchTeachers(object parameter)
        {
            return true; // Пошук завжди доступний
        }

        private void ExecuteSearchTeachers(object parameter)
        {
            FoundTeachers.Clear();

            if (_allTeachers == null || !_allTeachers.Any())
            {
                _allTeachers = UserService.Instance.GetAllTeachers();
                if (_allTeachers == null || !_allTeachers.Any())
                {
                    MessageBox.Show("Наразі немає доступних верифікованих викладачів у системі.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            IEnumerable<Teacher> filteredTeachers = _allTeachers;

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string lowerCaseQuery = SearchQuery.ToLower().Trim();

                filteredTeachers = _allTeachers.Where(t =>
                    (t.FirstName != null && t.FirstName.ToLower().Contains(lowerCaseQuery)) ||
                    (t.LastName != null && t.LastName.ToLower().Contains(lowerCaseQuery)) ||
                    (t.Subjects != null && t.Subjects.Any(s => s.ToLower().Contains(lowerCaseQuery)))
                );
            }

            foreach (var teacher in filteredTeachers)
            {
                FoundTeachers.Add(teacher);
            }

            if (!FoundTeachers.Any() && !string.IsNullOrWhiteSpace(SearchQuery))
            {
                MessageBox.Show("На жаль, за вашим запитом не знайдено репетиторів.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenTeacherProfile(Teacher teacher)
        {
            var teacherProfileViewModel = new TeacherProfileViewModel(teacher, CurrentStudent);

            var teacherProfileView = new TeacherProfileView
            {
                DataContext = teacherProfileViewModel
            };

            teacherProfileViewModel.LessonBooked += () =>
            {
                var updatedStudent = UserService.Instance.GetUserById(CurrentStudent.Id) as Student;
                if (updatedStudent != null)
                {
                    CurrentStudent = updatedStudent; 
                }
                LoadStudentLessons();
            };

            teacherProfileView.ShowDialog();

            SelectedTeacher = null;
        }

        private void ExecuteTopUpBalance(object parameter)
        {
            MessageBox.Show($"Функціонал поповнення балансу ({CurrentUserBalance:C}) буде реалізовано пізніше.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteLogout(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                MessageBox.Show("Вихід з облікового запису учня.", "Вихід", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
                currentWindow.Close();
            }
        }
    }
}