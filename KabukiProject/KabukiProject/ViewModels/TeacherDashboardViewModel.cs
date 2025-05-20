using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KabukiProject.Views; //навігація LoginView
using KabukiProject.Models; //доступ до моделі Teacher та User
using KabukiProject.Services; //доступ до UserService
using KabukiProject.Interfaces; // Додано для ITeacher на всяк випадок
using System.Collections.ObjectModel; //Subjects

namespace KabukiProject.ViewModels
{
    public class TeacherDashboardViewModel : BaseViewModel
    {
        //Властивості для UI (прив'язані до полів вводу)
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

        //ВЛАСТИВОСТІ ПРОФІЛЮ ВИКЛАДАЧА
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

        /*
         Для відображення списку предметів, які викладає вчитель. 
        ObservableCollection потрібна, щоб зміни в колекції (додавання/видалення) 
        автоматично оновлювали UI.
        */
        public ObservableCollection<string> Subjects { get; set; }

        //Команди
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand SaveProfileCommand { get; private set; }

        //Сервіси
        private readonly UserService _userService;
        private Teacher _loggedInTeacher; //Зберігаємо посилання на поточного викладача

        //КОНСТРУКТОР приймає об'єкт User (який має бути Teacher)
        public TeacherDashboardViewModel(User loggedInUser)
        {
            _userService = new UserService();
            LogoutCommand = new RelayCommand(ExecuteLogout);
            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile, CanExecuteSaveProfile);

            Subjects = new ObservableCollection<string>(); //колекція

            //переданий користувач є викладачем???
            if (loggedInUser is Teacher teacher)
            {
                _loggedInTeacher = teacher;
                LoadTeacherProfile(teacher.Username); //повний профіль за ім'ям
            }
            else
            {
                //Це не повинно статися, якщо логіка входу коректна, так шо на всяк пожарний
                MessageBox.Show("Помилка: Користувач не є викладачем.", "Помилка авторизації", MessageBoxButton.OK, MessageBoxImage.Error);
                //Можливо перенаправити назад на логін
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
                //Закриття поточного вікна буде оброблене у LoginViewModel, якщо воно відкривається як main.
                //Або потрібно передати поточне вікно сюди та закрити його.
            }

            //Приклад фіксованого балансу (може бути завантажений з БД для реального викладача)
            CurrentUserBalance = 2500.00m;
        }

        //Методи команд та логіки

        private void LoadTeacherProfile(string username)
        {
            //Завантажуємо повні дані викладача з UserService
            User user = _userService.GetUserByUsername(username);
            if (user is Teacher teacher)
            {
                _loggedInTeacher = teacher;
                //Оновлюємо властивості ViewModel даними з моделі
                CurrentUserName = teacher.Username; //логін
                FirstName = teacher.FirstName;
                LastName = teacher.LastName;
                Description = teacher.Description;
                PricePerHour = teacher.PricePerHour;
                PhotoPath = teacher.PhotoPath;

                Subjects.Clear(); //Очищаємо перед завантаженням нових
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
                //Якщо користувача не знайдено або він не викладач (після аутентифікації, це помилка)
                MessageBox.Show("Не вдалося завантажити профіль викладача.", "Помилка завантаження", MessageBoxButton.OK, MessageBoxImage.Error);
                //Можна ініціалізувати порожніми значеннями або перенаправити.
                FirstName = "";
                LastName = "";
                Description = "";
                PricePerHour = 0;
                PhotoPath = "";
                Subjects.Clear();
            }
        }

        private bool CanExecuteSaveProfile(object parameter)
        {
            //Умови, за яких кнопка "Зберегти" буде активною
            //Можете додати перевірки на PhotoPath, Subjects, якщо вони обов'язкові
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   PricePerHour > 0; //Ціна більше 0
        }

        private void ExecuteSaveProfile(object parameter)
        {
            if (_loggedInTeacher != null)
            {
                //дані моделі викладача з властивостей ViewModel
                _loggedInTeacher.FirstName = FirstName;
                _loggedInTeacher.LastName = LastName;
                _loggedInTeacher.Description = Description;
                _loggedInTeacher.PricePerHour = PricePerHour;
                _loggedInTeacher.PhotoPath = PhotoPath;
                _loggedInTeacher.Subjects = Subjects.ToList(); //ObservableCollection назад у List

                //Зберігаємо оновленого викладача через UserService
                _userService.UpdateUser(_loggedInTeacher);

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