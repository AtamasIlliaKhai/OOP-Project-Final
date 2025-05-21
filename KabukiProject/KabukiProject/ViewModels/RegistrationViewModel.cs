using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using KabukiProject.Enums;
using KabukiProject.Models;
using KabukiProject.Services;
using KabukiProject.Views;

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

        // ЗМІНА ТИПУ SELECTEDROLE
        private UserRole _selectedRole; // ТИП UserRole
        public UserRole SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }
        // КІНЕЦЬ ЗМІНИ ТИПУ SELECTEDROLE


        // КОЛЕКЦІЯ РОЛЕЙ
        public ObservableCollection<UserRole> UserRoles { get; set; }


        public RelayCommand RegisterCommand { get; private set; }
        public RelayCommand NavigateToLoginCommand { get; private set; }

        public RegistrationViewModel()
        {
            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);

            UserRoles = new ObservableCollection<UserRole>(
                Enum.GetValues(typeof(UserRole)) // Отримуємо всі значення enum UserRole
                    .Cast<UserRole>()           // Перетворюємо їх на UserRole
                    .Where(role => role != UserRole.Administrator) // Administrator в мінус
            );

            // Роль за замовчуванням Student
            SelectedRole = UserRole.Student; // Встановлюємо Enum значення
        }

        public void OnPasswordChanged(string password)
        {
            Password = password;
        }

        public void OnConfirmPasswordChanged(string confirmPassword)
        {
            ConfirmPassword = confirmPassword;
        }

        private bool CanExecuteRegister(object parameter)
        {
            bool passwordsMatch = Password == ConfirmPassword;

            // Перевіряємо, що SelectedRole не є значенням за замовчуванням (0)
            // і не Administrator (якого ми вже виключили з UserRoles)
            bool isRoleSelectedAndValid = SelectedRole != default(UserRole) && SelectedRole != UserRole.Administrator;

            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   passwordsMatch &&
                   isRoleSelectedAndValid; // Нова перевірка
        }

        private void ExecuteRegister(object parameter)
        {
            if (UserService.Instance.IsUsernameTaken(Username))
            {
                MessageBox.Show("Користувач з таким іменем вже існує. Будь ласка, оберіть інше ім'я.", "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User newUser = null;
            UserRole roleToRegister = SelectedRole; // SelectedRole вже є потрібним Enum!

            // --- СПРОЩЕНИЙ SWITCH ---
            switch (roleToRegister)
            {
                case UserRole.Student:
                    newUser = new Student { Username = Username, Password = Password, Role = roleToRegister, Balance = 0 };
                    break;
                case UserRole.Teacher:
                    newUser = new Teacher
                    {
                        Username = Username,
                        Password = Password,
                        Role = roleToRegister,
                        FirstName = "",
                        LastName = "",
                        Description = "",
                        PricePerHour = 0,
                        PhotoPath = "",
                        IsVerified = false,
                        Subjects = new List<string>()
                    };
                    break;
                    // Default case не потрібен, якщо ми впевнені, що SelectedRole завжди буде Student або Teacher
                    // Якщо SelectedRole з якихось причин дорівнює Administrator (хоча ми його виключили),
                    // CanExecuteRegister вже мав би це заблокувати.
                    // Можна додати throw new InvalidOperationException() для неможливих станів,
                    // але для курсового, ймовірно, не обов'язково.
            }
            // --- КІНЕЦЬ СПРОЩЕНОГО SWITCH ---


            if (newUser != null)
            {
                UserService.Instance.RegisterUser(newUser);
                MessageBox.Show("Реєстрація успішна! Тепер ви можете увійти.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                ExecuteNavigateToLogin(parameter);
            }
            else
            {
                // Цей блок може спрацювати, якщо newUser не був ініціалізований
                // через неочікуване значення roleToRegister.
                MessageBox.Show("Не вдалося створити користувача. Будь ласка, перевірте обрану роль.", "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteNavigateToLogin(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
                currentWindow.Close();
            }
        }
    }
}