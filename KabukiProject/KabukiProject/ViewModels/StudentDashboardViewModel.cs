using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KabukiProject.Views; //навігація LoginView
using KabukiProject.Models; //User та Student
using KabukiProject.Services; //UserService
using System.Collections.ObjectModel;


namespace KabukiProject.ViewModels
{
    public class StudentDashboardViewModel : BaseViewModel
    {
        //Властивості для відображення профілю учня та його баланса
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

        private decimal _currentUserBalance;
        public decimal CurrentUserBalance
        {
            get => _currentUserBalance;
            set
            {
                _currentUserBalance = value;
                OnPropertyChanged();
            }
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

        //Це нове, шоб репетитора шукать

        private string _searchQuery;
        // Властивість для пошукового запиту, введеного учнем.

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                //Це викликає перевірку, чи можна виконати команду, воно типу локає кнопку пошуку
                SearchTeachersCommand.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<Teacher> _foundTeachers;

// Колекція знайдених репетиторів для відображення у списку. Юзаєм ObservableCollection для автоматичного оновлення UI.

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

        // Властивість для обраного репетитора зі списку (якщо потрібно буде взаємодіяти з ним).

        public Teacher SelectedTeacher
        {
            get => _selectedTeacher;
            set
            {
                _selectedTeacher = value;
                OnPropertyChanged();
                // Тута можна ше логіку на вибір викладача жахнуть
            }
        }

        //Команди
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand SearchTeachersCommand { get; private set; } //Старто пошуку

        private readonly UserService _userService; //Сервіс для взаємодії з даними студіка
        private Student _loggedInStudent; //Ссилка на поточного студента

        /* Основний конструктор StudentDashboardViewModel, що приймає автентифікованого користувача.
        <param name="loggedInUser">Об'єкт User, який успішно пройшов аунтефікацію.</param>
        */
        public StudentDashboardViewModel(User loggedInUser)
        {
            _userService = new UserService(); //Сервис на стол

            //Команди активуємо
            LogoutCommand = new RelayCommand(ExecuteLogout);
            SearchTeachersCommand = new RelayCommand(ExecuteSearchTeachers, CanExecuteSearchTeachers);

            //Ініціація колекції для пошуку
            FoundTeachers = new ObservableCollection<Teacher>();

            //Перевірка типу юзера та лоад його профілю
            if (loggedInUser is Student student)
            {
                _loggedInStudent = student;
                LoadStudentProfile(student.Username); //Повний профіль студіка
            }
            else
            {
                //Якщо юзер не учень, то помилку видає
                MessageBox.Show("Помилка: Користувач не є учнем або дані відсутні.", "Помилка авторизації", MessageBoxButton.OK, MessageBoxImage.Error);
                //Назад логін і пароль віправляєм
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
                // currentWindow.Close(); //Не помню. Хай буде, якщо це для прошлого бага
            }

            //СТартовий баланс (може бути завантажений з даних користувача)
            CurrentUserBalance = 1500.00m;

            //Ствртовий пошук, щоб показати всіх викладачів при завантаженні дашборда (або лише при пустому SearchQuery)
            ExecuteSearchTeachers(null);
        }


        //Конструктор без параметрів (в основному для Design-time DataContext у XAML, да це важно).

        public StudentDashboardViewModel()
        {
            _userService = new UserService(); //Тут тоже сервіс ініцілізуєм
            FoundTeachers = new ObservableCollection<Teacher>(); //Коллекції
            SearchTeachersCommand = new RelayCommand(ExecuteSearchTeachers, CanExecuteSearchTeachers); //Команду

            //Дані для дизайнєра:
            CurrentUserName = "Ім'я Учня (Design)";
            CurrentUserBalance = 0.00m;
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        //Тута логіка. ЗВІДСИ І ДО КОНЦА

        /* Завантажує дані поточного учня з UserService.
         <param name="username">Ім'я користувача для завантаження.</param>
        */
        private void LoadStudentProfile(string username)
        {
            User user = _userService.GetUserByUsername(username);
            if (user is Student student)
            {
                //Повний об'єкт учня: імя фамілія "отчєства"
                _loggedInStudent = student;
                CurrentUserName = student.Username;
                FirstName = student.FirstName;
                LastName = student.LastName;
            }
            else
            {
                //Якщо юзера не знайдено, або він не студент, то оце робиться
                MessageBox.Show("Не вдалося завантажити деталі профілю учня.", "Помилка завантаження", MessageBoxButton.OK, MessageBoxImage.Error);
                CurrentUserName = "Unknown Student";
                FirstName = "";
                LastName = "";
            }
        }

        //Метод, що узнає чи можна виконати команду SearchTeachersCommand.
 
        private bool CanExecuteSearchTeachers(object parameter)
        {
            //Пошук можна завжди виконать і з порожнім запитом (щоб показати всіх)
            //ІЛІ, ІЛІ return !string.IsNullOrWhiteSpace(SearchQuery); //Якщо пошук має бути лише при введенні тексту
            return true;
        }

        /* Метод, що виконує логіку пошуку репетиторів. Ця частина буде реалізована на наступному кроці.
        */
        private void ExecuteSearchTeachers(object parameter)
        {
            /*Пока шо просто виведемо повідомлення. Далі тут буде логіка отримання всіх прєподів з UserService і фільтрація за SearchQuery.
            */
            MessageBox.Show("Логіка пошуку репетиторів буде реалізована тут!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /*метод, що виконує логіку виходу з системи
         *<param name="parameter">Поточне вікно, яке потрібно закрити.</param>
        */
        private void ExecuteLogout(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                MessageBox.Show("Вихід з облікового запису учня.", "Вихід", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView; //Оновить вікно застосунка
                currentWindow.Close(); //закрити вікно дашборда
            }
        }
    }
}