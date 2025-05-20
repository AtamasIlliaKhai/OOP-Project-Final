using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KabukiProject.Enums; //UserRole
using KabukiProject.Models; //User Student Teacher model
using KabukiProject.Services; //Юзер скервіс
using KabukiProject.Views; //Навігація

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
            SelectedRole = "Учень"; // Default selection
        }

        private bool CanExecuteRegister(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Password == ConfirmPassword &&
                   !string.IsNullOrWhiteSpace(SelectedRole);
        }

        // ПЕРЕДАЄМО ВІКНО ЯК ПАРАМЕТР
        private void ExecuteRegister(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                if (Password != ConfirmPassword)
                {
                    MessageBox.Show("Паролі не співпадають!", "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (_userService.IsUsernameTaken(Username))
                {
                    MessageBox.Show("Ім'я користувача вже зайняте!", "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                User newUser;
                UserRole role = SelectedRole == "Учень" ? UserRole.Student : UserRole.Teacher; // Припускаємо, що SelectedRole завжди буде "Учень" або "Викладач"

                if (role == UserRole.Student)
                {
                    newUser = new Student { Username = Username, Password = Password, Role = UserRole.Student };
                }
                else // UserRole.Teacher
                {
                    newUser = new Teacher { Username = Username, Password = Password, Role = UserRole.Teacher, IsVerified = false };
                }

                if (_userService.RegisterUser(newUser))
                {
                    MessageBox.Show($"Користувач '{Username}' успішно зареєстрований як {SelectedRole}!", "Реєстрація успішна", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Перенаправлення на Login після успішної реєстрації
                    ExecuteNavigateToLogin(currentWindow); // Передаємо поточне вікно для закриття
                }
                else
                {
                    MessageBox.Show("Помилка реєстрації користувача.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ПЕРЕДАЄМО ВІКНО ЯК ПАРАМЕТР
        private void ExecuteNavigateToLogin(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                var loginView = new LoginView();
                loginView.Show();
                // Важливо: Оновити Application.Current.MainWindow на нове вікно
                Application.Current.MainWindow = loginView;
                currentWindow.Close(); // Закрити старе вікно
            }
        }
    }
}