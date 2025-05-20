using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KabukiProject.Views; // навігація LoginView, TeacherProfileView
using KabukiProject.Models; // User та Student, Teacher
using KabukiProject.Services; // UserService
using System.Collections.ObjectModel; // ObservableCollection

namespace KabukiProject.ViewModels
{
    public class StudentDashboardViewModel : BaseViewModel
    {
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

        // Властивості для пошуку репетиторів. МБ кінцеві
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                // Не викликаємо RaiseCanExecuteChanged, бо CanExecuteSearchTeachers завжди true
                // Натомість, можна автоматично виконувати пошук при зміні запиту, якщо це потрібно:
                // ExecuteSearchTeachers(null);
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
                // Відкривається вікно викладача
                if (_selectedTeacher != null)
                {
                    OpenTeacherProfile(_selectedTeacher);
                }
            }
        }

        // Команди
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand SearchTeachersCommand { get; private set; }

        // Більше не потрібне приватне поле _userService, так як ми використовуємо синглтон Instance
        // private readonly UserService _userService; // Цей рядок видаляємо або коментуємо

        private Student _loggedInStudent;

        // Приватне поле для зберігання всіх викладачів, щоб не перешуковувати весь список кожного разу
        private List<Teacher> _allTeachers;

        public StudentDashboardViewModel(User loggedInUser)
        {
            // !!! ВИДАЛЯЄМО: _userService = new UserService(); !!!
            // Тепер UserService завжди доступний через UserService.Instance
            LogoutCommand = new RelayCommand(ExecuteLogout);
            SearchTeachersCommand = new RelayCommand(ExecuteSearchTeachers, CanExecuteSearchTeachers);

            FoundTeachers = new ObservableCollection<Teacher>();

            if (loggedInUser is Student student)
            {
                _loggedInStudent = student;
                LoadStudentProfile(student.Username); // Завантажуємо дані студента при вході
            }
            else
            {
                // Якщо користувач не студент або дані відсутні, перенаправляємо на вхід
                MessageBox.Show("Помилка: Користувач не є учнем або дані відсутні. Будь ласка, увійдіть знову.", "Помилка авторизації", MessageBoxButton.OK, MessageBoxImage.Error);
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
            }

            // Завантажуємо всіх викладачів один раз при ініціалізації ViewModel
            _allTeachers = UserService.Instance.GetAllTeachers();
            // Виконуємо початковий пошук, щоб відобразити всіх доступних викладачів
            ExecuteSearchTeachers(null);
        }

        public StudentDashboardViewModel()
        {
            // Цей конструктор використовується для дизайнера.
            // !!! ВИДАЛЯЄМО: _userService = new UserService(); !!!
            FoundTeachers = new ObservableCollection<Teacher>();
            SearchTeachersCommand = new RelayCommand(ExecuteSearchTeachers, CanExecuteSearchTeachers);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            CurrentUserName = "Ім'я Учня (Design)";
            CurrentUserBalance = 1500.00m; // Приклад балансу для дизайну
            FirstName = "Іван (Design)";
            LastName = "Учень (Design)";

            // Для дизайнера також завантажуємо всіх викладачів, щоб відобразити дані
            _allTeachers = UserService.Instance.GetAllTeachers();
            ExecuteSearchTeachers(null); // Виконуємо початковий пошук для дизайну
        }

        // Логіка
        private void LoadStudentProfile(string username)
        {
            // Використовуємо єдиний екземпляр UserService.Instance
            User user = UserService.Instance.GetUserByUsername(username);
            if (user is Student student)
            {
                // Оновлюємо посилання на _loggedInStudent, якщо воно відрізняється,
                // або просто оновлюємо його властивості
                _loggedInStudent = student;
                CurrentUserName = student.Username;
                FirstName = student.FirstName;
                LastName = student.LastName;
                CurrentUserBalance = student.Balance; // Оновлюємо баланс з реального об'єкта студента
            }
            else
            {
                MessageBox.Show("Не вдалося завантажити деталі профілю учня. Можливо, дані пошкоджені.", "Помилка завантаження", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentUserName = "Unknown Student";
                FirstName = "";
                LastName = "";
                CurrentUserBalance = 0.00m; // Встановлюємо в 0 при помилці
            }
        }

        private bool CanExecuteSearchTeachers(object parameter)
        {
            // Пошук завжди можливий, навіть з порожнім запитом (що покаже всіх викладачів)
            return true;
        }

        // Виконує пошук та фільтрацію репетиторів на основі SearchQuery.
        private void ExecuteSearchTeachers(object parameter)
        {
            FoundTeachers.Clear();

            // Якщо _allTeachers ще не завантажено (хоча має бути з конструктора), завантажуємо
            if (_allTeachers == null || !_allTeachers.Any())
            {
                _allTeachers = UserService.Instance.GetAllTeachers();
                if (_allTeachers == null || !_allTeachers.Any())
                {
                    MessageBox.Show("Наразі немає доступних викладачів у системі.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            IEnumerable<Teacher> filteredTeachers = _allTeachers;

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string lowerCaseQuery = SearchQuery.ToLower().Trim(); // Trim для видалення зайвих пробілів

                filteredTeachers = _allTeachers.Where(t =>
                    // Пошук за ім'ям або прізвищем
                    (t.FirstName != null && t.FirstName.ToLower().Contains(lowerCaseQuery)) ||
                    (t.LastName != null && t.LastName.ToLower().Contains(lowerCaseQuery)) ||
                    // Пошук за предметами
                    (t.Subjects != null && t.Subjects.Any(s => s.ToLower().Contains(lowerCaseQuery)))
                );
            }

            foreach (var teacher in filteredTeachers)
            {
                FoundTeachers.Add(teacher);
            }

            // Якщо нікого не знайдено за запитом
            if (!FoundTeachers.Any() && !string.IsNullOrWhiteSpace(SearchQuery))
            {
                MessageBox.Show("На жаль, за вашим запитом не знайдено репетиторів.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenTeacherProfile(Teacher teacher)
        {
            // Створюємо новий TeacherProfileViewModel, передаючи йому обраного викладача ТА поточного студента
            // Це потрібно, якщо TeacherProfileView має функціонал, пов'язаний з діями студента (наприклад, бронювання уроку)
            var teacherProfileViewModel = new TeacherProfileViewModel(teacher, _loggedInStudent);

            var teacherProfileView = new TeacherProfileView
            {
                DataContext = teacherProfileViewModel
            };

            teacherProfileView.ShowDialog(); // Відкриваємо модально

            // Після закриття TeacherProfileView, оновлюємо дані студента,
            // оскільки його баланс міг змінитися після бронювання/оплати.
            // Повторно завантажуємо профіль з UserService.Instance
            if (_loggedInStudent != null)
            {
                LoadStudentProfile(_loggedInStudent.Username);
            }

            SelectedTeacher = null; // Знімаємо виділення з викладача у списку
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