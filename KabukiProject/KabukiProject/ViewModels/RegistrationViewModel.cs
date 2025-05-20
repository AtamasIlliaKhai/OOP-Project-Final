using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KabukiProject.Enums; // UserRole
using KabukiProject.Models; // User, Student, Teacher models
using KabukiProject.Services; // UserService
using KabukiProject.Views; // Navigation views

namespace KabukiProject.ViewModels
{
    public class RegistrationViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _selectedRole;
        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand RegisterCommand { get; private set; }
        public RelayCommand NavigateToLoginCommand { get; private set; }

        private readonly UserService _userService;

        public RegistrationViewModel()
        {
            _userService = new UserService();
            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);
            SelectedRole = "Учень"; // Значення за замовчуванням
        }

        private bool CanExecuteRegister(object parameter)
        {
            // Перевіряємо, чи всі необхідні поля заповнені та паролі співпадають.
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Password == ConfirmPassword &&
                   !string.IsNullOrWhiteSpace(SelectedRole);
        }

        private void ExecuteRegister(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                // Перевірка на співпадіння паролів
                if (Password != ConfirmPassword)
                {
                    MessageBox.Show("Паролі не співпадають!", "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Логіка створення нового користувача відповідно до обраної ролі
                User newUser;
                // Визначаємо UserRole на основі SelectedRole з ComboBox
                UserRole role = SelectedRole == "Учень" ? UserRole.Student : UserRole.Teacher;

                if (role == UserRole.Student)
                {
                    // Створюємо новий об'єкт Student і ПОВНІСТЮ ініціалізуємо всі його властивості.
                    // Це КЛЮЧОВО для коректної серіалізації в JSON.
                    newUser = new Student
                    {
                        Username = Username,
                        Password = Password, // Паролі слід хешувати в реальному застосунку для безпеки!
                        Role = UserRole.Student,
                        FirstName = "",     // Ініціалізуємо порожнім рядком
                        LastName = "",      // Ініціалізуємо порожнім рядком
                        Email = "",         // Ініціалізуємо порожнім рядком
                        Balance = 0.00m     // Ініціалізуємо нулем
                    };
                }
                else // UserRole.Teacher (за припущенням, що інших ролей тут немає)
                {
                    // Створюємо новий об'єкт Teacher і ПОВНІСТЮ ініціалізуємо всі його властивості.
                    // Це КЛЮЧОВО для коректної серіалізації в JSON.
                    newUser = new Teacher
                    {
                        Username = Username,
                        Password = Password, // Паролі слід хешувати в реальному застосунку для безпеки!
                        Role = UserRole.Teacher,
                        IsVerified = false,  // За замовчуванням викладач не верифікований при реєстрації
                        FirstName = "",      // Ініціалізуємо порожнім рядком
                        LastName = "",       // Ініціалізуємо порожнім рядком
                        Description = "",    // Ініціалізуємо порожнім рядком
                        PricePerHour = 0.00m, // Ініціалізуємо нулем
                        Subjects = new List<string>(), // Ініціалізуємо порожнім списком
                        PhotoPath = ""       // Ініціалізуємо порожнім рядком
                    };
                }

                // Спроба реєстрації користувача через UserService
                if (_userService.RegisterUser(newUser))
                {
                    MessageBox.Show($"Користувач '{Username}' успішно зареєстрований як {SelectedRole}!", "Реєстрація успішна", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Після успішної реєстрації перенаправляємо на вікно входу
                    ExecuteNavigateToLogin(currentWindow); // Передаємо поточне вікно для закриття
                }
                // Якщо реєстрація не вдалася (наприклад, ім'я користувача вже зайняте), повідомлення про помилку показано з UserService.
            }
        }

        private void ExecuteNavigateToLogin(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                // Створюєм новоє вікно входу та відображаєм його
                var loginView = new LoginView();
                loginView.Show();

                // Оновлюєм глвнее вікно застосунку, для норм роботи та не дать проблемам з закриттям/управлінням вікнами.
                Application.Current.MainWindow = loginView;

                // Закриваємо поточне вікно реєстрації
                currentWindow.Close();
            }
        }
    }
}