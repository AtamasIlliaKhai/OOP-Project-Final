using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KabukiProject.Views; // навігація LoginView
using KabukiProject.Models; // User та Student
using KabukiProject.Services; // UserService
using System.Collections.ObjectModel;

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
                SearchTeachersCommand.RaiseCanExecuteChanged();
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
                // Откривається вікно викладача
                if (_selectedTeacher != null)
                {
                    OpenTeacherProfile(_selectedTeacher);
                }
            }
        }

        // Команди
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand SearchTeachersCommand { get; private set; }

        private readonly UserService _userService;
        private Student _loggedInStudent;

        // Приватне поле для зберігання всіх викладачів, щоб не перешуковувати весь список кожного разу
        private List<Teacher> _allTeachers;

        public StudentDashboardViewModel(User loggedInUser)
        {
            _userService = new UserService();
            LogoutCommand = new RelayCommand(ExecuteLogout);
            SearchTeachersCommand = new RelayCommand(ExecuteSearchTeachers, CanExecuteSearchTeachers);

            FoundTeachers = new ObservableCollection<Teacher>();

            if (loggedInUser is Student student)
            {
                _loggedInStudent = student;
                LoadStudentProfile(student.Username);
            }
            else
            {
                MessageBox.Show("Помилка: Користувач не є учнем або дані відсутні.", "Помилка авторизації", MessageBoxButton.OK, MessageBoxImage.Error);
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
            }

            CurrentUserBalance = 1500.00m;

            // Завантажуємо всіх викладачів один раз при ініціалізації ViewModel і ЗРАЗУ виконуємо початковий пошук (щоб показати всіх)
            _allTeachers = _userService.GetAllTeachers();
            ExecuteSearchTeachers(null); // Виконуємо пошук при запуску, щоб відобразити всіх викладачів
        }

        public StudentDashboardViewModel()
        {
            _userService = new UserService();
            FoundTeachers = new ObservableCollection<Teacher>();
            SearchTeachersCommand = new RelayCommand(ExecuteSearchTeachers, CanExecuteSearchTeachers);

            CurrentUserName = "Ім'я Учня (Design)";
            CurrentUserBalance = 0.00m;
            LogoutCommand = new RelayCommand(ExecuteLogout);

            // Для дизайнера тож завантажуємо всіх викладачів, щоб відобразити дані
            _allTeachers = _userService.GetAllTeachers();
            ExecuteSearchTeachers(null);
        }

        // Логіка

        private void LoadStudentProfile(string username)
        {
            User user = _userService.GetUserByUsername(username);
            if (user is Student student)
            {
                _loggedInStudent = student;
                CurrentUserName = student.Username;
                FirstName = student.FirstName;
                LastName = student.LastName;
            }
            else
            {
                MessageBox.Show("Не вдалося завантажити деталі профілю учня.", "Помилка завантаження", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentUserName = "Unknown Student";
                FirstName = "";
                LastName = "";
            }
        }

        private bool CanExecuteSearchTeachers(object parameter)
        {
            // Завжди активна
            return true;
        }


        // Виконує пошук та фільтрацію репетиторів на основі SearchQuery.
        private void ExecuteSearchTeachers(object parameter)
        {
            // Очищаємо список попередніх результатів
            FoundTeachers.Clear();

            // Якщо немає завантажених викладачів, намагаємося завантажити їх (хоча вони мають бути завантажені в конструкторі, але все одно про всякий пожарний)
            if (_allTeachers == null)
            {
                _allTeachers = _userService.GetAllTeachers();
            }

            // Якщо _allTeachers все ще null після спроби завантаження, значить, немає даних
            if (_allTeachers == null || !_allTeachers.Any())
            {
                MessageBox.Show("Наразі немає доступних викладачів.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Застосовуємо фільтрацію, якщо SearchQuery не порожній
            IEnumerable<Teacher> filteredTeachers = _allTeachers;

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                // Приводимо пошуковий запит до нижнього регістру для регістронезалежного пошуку
                string lowerCaseQuery = SearchQuery.ToLower();

                // Фільтруємо викладачів за ім'ям, прізвищем або предметами
                filteredTeachers = _allTeachers.Where(t =>
                    // Пошук за ім'ям або прізвищем
                    (t.FirstName != null && t.FirstName.ToLower().Contains(lowerCaseQuery)) ||
                    (t.LastName != null && t.LastName.ToLower().Contains(lowerCaseQuery)) ||
                    // Пошук за предметами (Чек, чи є в списку предметів викладача хоча б один який містить пошуковий запит)
                    (t.Subjects != null && t.Subjects.Any(s => s.ToLower().Contains(lowerCaseQuery)))
                );
            }

            // Додаємо відфільтрованих викладачів до ObservableCollection
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
            // Створюєм новий TeacherProfileViewModel, передаючи йому обраного викладача
            var teacherProfileViewModel = new TeacherProfileViewModel(teacher);

            // Создаєм нове вікно TeacherProfileView
            var teacherProfileView = new TeacherProfileView
            {
                // Встановлюємо DataContext нового вікна на наш TeacherProfileViewModel
                DataContext = teacherProfileViewModel
            };

            // Показуємо вікно профілю
            teacherProfileView.ShowDialog(); // Локаєм вікно, шоб не клацали

            // Чисто для красоти, знімаєм виділення з викладача у списку
            SelectedTeacher = null;
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