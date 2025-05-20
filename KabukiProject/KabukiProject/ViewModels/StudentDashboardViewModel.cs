using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KabukiProject.Views; //навігація LoginView
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KabukiProject.Models; // Для доступу до моделей User та Student
using KabukiProject.Services; // Для доступу до UserService

namespace KabukiProject.ViewModels
{
    public class StudentDashboardViewModel : BaseViewModel
    {
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

        //ВЛАСТИВОСТІ ПРОФІЛЮ УЧНЯ
        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); /* Мб SaveProfileCommand.RaiseCanExecuteChanged(); */ }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); /* Мб SaveProfileCommand.RaiseCanExecuteChanged(); */ }
        }

        /* Додати інші властивості учня пізніше
         private string _studentSpecificProperty;
         public string StudentSpecificProperty
         {
             get => _studentSpecificProperty;
             set { _studentSpecificProperty = value; OnPropertyChanged(); }
         }
        */

        public RelayCommand LogoutCommand { get; private set; }
        //public RelayCommand SaveProfileCommand { get; private set; } // Якщо буде функціонал збереження профілю учня

        private readonly UserService _userService;
        private Student _loggedInStudent; //Зберігаємо посилання на поточного учня

        //КОНСТРУКТОР приймає об'єкт User (який має бути Student)
        public StudentDashboardViewModel(User loggedInUser)
        {
            _userService = new UserService();
            LogoutCommand = new RelayCommand(ExecuteLogout);
            //SaveProfileCommand = new RelayCommand(ExecuteSaveProfile, CanExecuteSaveProfile); // Ініціалізуємо, якщо буде

            //Перевіряємо, чи переданий користувач є учнем
            if (loggedInUser is Student student)
            {
                _loggedInStudent = student;
                LoadStudentProfile(student.Username); // Завантажуємо повний профіль за ім'ям
            }
            else
            {
                //Це не повинно статися при коректній логіці входу, але на всяк пожарний
                MessageBox.Show("Помилка: Користувач не є учнем.", "Помилка авторизації", MessageBoxButton.OK, MessageBoxImage.Error);
                //Можливо, перенаправити назад на логін
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
            }

            //Приклад фіксованого балансу (буде завантажений з БД для реального учня)
            CurrentUserBalance = 1500.00m;
        }

        /* Конструктор без параметрів (для дизайнера або якщо не передаємо User)
         Залиште його, якщо вам потрібен дизайнер, але пам'ятайте,
         що він не завантажуватиме реальні дані користувача.*/
        public StudentDashboardViewModel()
        {
            /* Цей конструктор краще використовувати лише для Design-time DataContext
             або якщо реалізую завантаження користувача через Singleton/Dependency Injection.
             Для реального автентифікованого користувача юзати конструктор з User. */
            CurrentUserName = "Ім'я Учня (Design)";
            CurrentUserBalance = 0.00m; // Для дизайнера
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }


        //Методи логіки

        private void LoadStudentProfile(string username)
        {
            //Завантажую повні дані учня з UserService
            User user = _userService.GetUserByUsername(username);
            if (user is Student student)
            {
                _loggedInStudent = student;
                // Оновлюєм властивості ViewModel даними з моделі
                CurrentUserName = student.Username; //логін
                FirstName = student.FirstName;
                LastName = student.LastName;
            }
            else
            {
                //Якщо користувача не знайдено або він не учень (після аутентифікації, це помилка)
                MessageBox.Show("Не вдалося завантажити профіль учня.", "Помилка завантаження", MessageBoxButton.OK, MessageBoxImage.Error);
                //Можна ініціалізувати порожніми значеннями або перенаправити.
                CurrentUserName = "Unknown Student";
                FirstName = "";
                LastName = "";
            }
        }

        /*
        Метод для збереження профілю учня (якщо ви вирішите додати такий функціонал)
         private bool CanExecuteSaveProfile(object parameter)
         {
             // Умови, за яких кнопка "Зберегти" буде активною
             return !string.IsNullOrWhiteSpace(FirstName) &&
                    !string.IsNullOrWhiteSpace(LastName);
         }

         private void ExecuteSaveProfile(object parameter)
         {
             if (_loggedInStudent != null)
             {
                 // Оновлюємо дані моделі учня з властивостей ViewModel
                 _loggedInStudent.FirstName = FirstName;
                 _loggedInStudent.LastName = LastName;
                 // Оновіть інші властивості учня

                 _userService.UpdateUser(_loggedInStudent);
                 MessageBox.Show("Профіль учня успішно збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
             }
             else
             {
                 MessageBox.Show("Не вдалося зберегти профіль: користувач не визначений.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
             }
         } 
        */

        //Метод команди
        private void ExecuteLogout(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                MessageBox.Show("Вихід з облікового запису учня.", "Вихід", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginView = new LoginView();
                loginView.Show();
                //Важливо: Оновити Application.Current.MainWindow на нове вікно
                Application.Current.MainWindow = loginView;
                currentWindow.Close(); //Закрити поточне вікно дашборда
            }
        }
    }
}