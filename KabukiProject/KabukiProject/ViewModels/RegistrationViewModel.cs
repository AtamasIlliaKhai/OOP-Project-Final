using KabukiProject.Enums; // Все одно потрібен для UserRole
using KabukiProject.Models;
using KabukiProject.Services;
using KabukiProject.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

        // Ці властивості тепер зберігають сирі паролі (не хешовані)
        private string _password;
        public string Password // ПРИВ'ЯЗУЄТЬСЯ ДО PasswordBox_PasswordChanged
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
        public string ConfirmPassword // ПРИВ'ЯЗУЄТЬСЯ ДО ConfirmPasswordBox_PasswordChanged
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }

        private string _selectedRole; // ЗМІНЕНО: Тепер це string
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

        public RegistrationViewModel()
        {
            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);

            SelectedRole = "Учень";
        }

        // Методи для обробки зміни паролів з PasswordBox
        // Ці методи тепер просто встановлюють значення, без хешування
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
            // Перевіряємо, чи всі поля заповнені та чи паролі співпадають
            bool passwordsMatch = Password == ConfirmPassword;
            bool isRoleSelected = !string.IsNullOrWhiteSpace(SelectedRole); // Перевіряємо, чи обрано роль

            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   passwordsMatch &&
                   isRoleSelected;
        }

        private void ExecuteRegister(object parameter)
        {
            if (UserService.Instance.IsUsernameTaken(Username))
            {
                MessageBox.Show("Користувач з таким іменем вже існує. Будь ласка, оберіть інше ім'я.", "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User newUser = null;
            UserRole roleToRegister;

            // --- ПОЧАТОК ЗМІН У ЦЬОМУ МЕТОДІ ---
            string extractedRoleContent = null;
            if (!string.IsNullOrWhiteSpace(SelectedRole))
            {
                // SelectedRole зараз виглядає як "System.Windows.Controls.ComboBoxItem: Учень"
                // Нам потрібно знайти двокрапку і пробіл після неї, а потім взяти частину рядка
                int colonIndex = SelectedRole.IndexOf(':');
                if (colonIndex != -1 && colonIndex + 2 < SelectedRole.Length) // Перевіряємо, чи є ":" і достатньо символів після нього
                {
                    // Витягуємо вміст після ": " і видаляємо зайві пробіли
                    extractedRoleContent = SelectedRole.Substring(colonIndex + 2).Trim();
                }
                else
                {
                    // Якщо формат несподіваний, або це просто "Учень" (менш ймовірно, але для безпеки),
                    // просто використовуємо весь рядок після Trim
                    extractedRoleContent = SelectedRole.Trim();
                }
            }
            // --- КІНЕЦЬ ЗМІН У ЦЬОМУ МЕТОДІ ---


            switch (extractedRoleContent) // Тепер switch працюватиме з "Учень" або "Викладач"
            {
                case "Учень":
                    roleToRegister = UserRole.Student;
                    newUser = new Student { Username = Username, Password = Password, Role = roleToRegister, Balance = 0 };
                    break;
                case "Викладач":
                    roleToRegister = UserRole.Teacher;
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
                default:
                    MessageBox.Show("Будь ласка, оберіть коректну роль (Учень або Викладач).", "Помилка реєстрації", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Виходимо, якщо роль не розпізнана
            }

            if (newUser != null)
            {
                UserService.Instance.RegisterUser(newUser);
                MessageBox.Show("Реєстрація успішна! Тепер ви можете увійти.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                ExecuteNavigateToLogin(parameter);
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