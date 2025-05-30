﻿using KabukiProject.Enums;
using KabukiProject.Models; //Для юрез моделі
using KabukiProject.Services; //Потім буде юзер сервіс
using KabukiProject.Views; //Навігація
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KabukiProject.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                // Забезпечити оновлення стану команди при зміні полів
                LoginCommand.RaiseCanExecuteChanged();
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
                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand LoginCommand { get; private set; }
        public RelayCommand NavigateToRegisterCommand { get; private set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            NavigateToRegisterCommand = new RelayCommand(ExecuteNavigateToRegister);
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        //Передача вікна ЯК ПАРАМЕТРА
        private void ExecuteLogin(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                User authenticatedUser = UserService.Instance.AuthenticateUser(Username, Password);

                if (authenticatedUser != null)
                {
                    MessageBox.Show($"Вхід успішний як {authenticatedUser.Role}!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    Window nextWindow = null;
                    if (authenticatedUser.Role == UserRole.Student)
                    {
                        nextWindow = new StudentDashboardView(authenticatedUser);
                    }
                    else if (authenticatedUser.Role == UserRole.Teacher)
                    {
                        nextWindow = new TeacherDashboardView(authenticatedUser);
                    }
                    else if (authenticatedUser.Role == UserRole.Administrator)
                    {
                        nextWindow = new AdminDashboardView(authenticatedUser);
                    }

                    if (nextWindow != null)
                    {
                        nextWindow.Show();
                        Application.Current.MainWindow = nextWindow;
                        currentWindow.Close();
                    }
                    else
                    {
                        // Треба, але вже не буде треба
                        MessageBox.Show("Не вдалося відкрити панель. Невідома роль користувача або помилка ініціалізації.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Невірний логін або пароль.", "Помилка входу", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //ПЕРЕДАЄМО ВІКНО ЯК ПАРАМЕТР
        private void ExecuteNavigateToRegister(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                var registerView = new RegistrationView();
                registerView.Show();
                // Важливо: Оновити Application.Current.MainWindow на нове вікно
                Application.Current.MainWindow = registerView;
                currentWindow.Close(); // закрити старе вікно
            }
        }
    }
}