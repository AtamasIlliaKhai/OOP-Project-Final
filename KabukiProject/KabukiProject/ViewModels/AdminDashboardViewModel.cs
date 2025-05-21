using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows; // MessageBox, Window, Application
using KabukiProject.Models; // User, Teacher
using KabukiProject.Services; // UserService
using KabukiProject.Views; // LoginView
using KabukiProject.Enums; // UserRole 

namespace KabukiProject.ViewModels
{
    public class AdminDashboardViewModel : BaseViewModel
    {
        private User _loggedInAdmin;
        public ObservableCollection<Teacher> Teachers { get; set; }

        private Teacher _selectedTeacher;
        public Teacher SelectedTeacher
        {
            get => _selectedTeacher;
            set
            {
                _selectedTeacher = value;
                OnPropertyChanged();
                VerifyTeacherCommand?.RaiseCanExecuteChanged();
                EditTeacherCommand?.RaiseCanExecuteChanged();
                DeleteTeacherCommand?.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand VerifyTeacherCommand { get; private set; }
        public RelayCommand EditTeacherCommand { get; private set; }
        public RelayCommand DeleteTeacherCommand { get; private set; }

        // КОНСТРУКТОР ДЛЯ РЕАЛЬНОГО ВИКОРИСТАННЯ (ПІСЛЯ ВХОДУ)
        public AdminDashboardViewModel(User loggedInAdmin)
        {
            if (loggedInAdmin == null || loggedInAdmin.Role != UserRole.Administrator)
            {
                MessageBox.Show("Помилка: Користувач не є адміністратором або не авторизований.", "Помилка доступу", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _loggedInAdmin = loggedInAdmin;

            LogoutCommand = new RelayCommand(ExecuteLogout);
            VerifyTeacherCommand = new RelayCommand(ExecuteVerifyTeacher, CanExecuteVerifyTeacher);
            EditTeacherCommand = new RelayCommand(ExecuteEditTeacher, CanExecuteEditTeacher);
            DeleteTeacherCommand = new RelayCommand(ExecuteDeleteTeacher, CanExecuteDeleteTeacher);

            Teachers = new ObservableCollection<Teacher>();

            LoadTeachers();
        }

        // КОНСТРУКТОР БЕЗ ПАРАМЕТРІВ (ДЛЯ ДИЗАЙНЕРА XAML)
        public AdminDashboardViewModel()
        {
            Teachers = new ObservableCollection<Teacher>
            {
                new Teacher
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "teacher_unverified",
                    FirstName = "Анна",
                    LastName = "Ковальчук",
                    IsVerified = false,
                    PricePerHour = 150.0m,
                    Description = "Викладач фізики, не верифікована.",
                    Subjects = new List<string> { "Фізика" }
                },
                new Teacher
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "teacher_verified",
                    FirstName = "Олег",
                    LastName = "Сидоренко",
                    IsVerified = true,
                    PricePerHour = 250.0m,
                    Description = "Верифікований викладач хімії.",
                    Subjects = new List<string> { "Хімія", "Біологія" }
                }
            };
            SelectedTeacher = Teachers.FirstOrDefault();

            LogoutCommand = new RelayCommand(ExecuteLogout);
            VerifyTeacherCommand = new RelayCommand(ExecuteVerifyTeacher, CanExecuteVerifyTeacher);
            EditTeacherCommand = new RelayCommand(ExecuteEditTeacher, CanExecuteEditTeacher);
            DeleteTeacherCommand = new RelayCommand(ExecuteDeleteTeacher, CanExecuteDeleteTeacher);
        }

        // МЕТОДИ ЛОГІКИ

        // Метод для завантаження всіх викладачів з UserService
        private void LoadTeachers()
        {
            Teachers.Clear();
            var allUsers = UserService.Instance.GetAllUsers();
            foreach (var user in allUsers)
            {
                if (user is Teacher teacher)
                {
                    Teachers.Add(teacher);
                }
            }
            VerifyTeacherCommand.RaiseCanExecuteChanged();
            EditTeacherCommand.RaiseCanExecuteChanged();
            DeleteTeacherCommand.RaiseCanExecuteChanged();
        }

        // КОМАНДИ

        // Логіка, коли кнопка "Верифікувати викладача"
        private bool CanExecuteVerifyTeacher(object parameter)
        {
            // Кнопка активна, якщо:
            // Вибрано викладача (`SelectedTeacher` не null) або викладач ще не верифікований (!SelectedTeacher.IsVerified)
            return SelectedTeacher != null && !SelectedTeacher.IsVerified;
        }

        // Логіка виконання верифікації викладача
        private void ExecuteVerifyTeacher(object parameter)
        {
            if (SelectedTeacher != null)
            {
                SelectedTeacher.IsVerified = true; // Змінюємо статус верифікації
                UserService.Instance.UpdateUser(SelectedTeacher); // Оновлюємо викладача в UserService (це також викличе SaveUsers())

                MessageBox.Show($"Викладач {SelectedTeacher.Username} успішно верифікований!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTeachers();
                SelectedTeacher = null; // Знімаємо вибір після дії, щоб запобігти повторним натисканням
            }
        }

        // Логіка, коли кнопка (Редагувати викладача) може бути активованою
        private bool CanExecuteEditTeacher(object parameter)
        {
            // Кнопка активна, якщо вибрано викладача
            return SelectedTeacher != null;
        }

        // Логіка виконання редагування викладача
        private void ExecuteEditTeacher(object parameter)
        {
            if (SelectedTeacher != null)
            {
                EditTeacherView editView = new EditTeacherView(SelectedTeacher); 
                editView.ShowDialog();
                LoadTeachers();
                SelectedTeacher = null;
            }
        }

        // Логіка, коли кнопка "Видалити викладача" може бути активованою
        private bool CanExecuteDeleteTeacher(object parameter)
        {
            // Кнопка активна, якщо вибрано викладача
            return SelectedTeacher != null;
        }

        // Логіка виконання видалення викладача
        private void ExecuteDeleteTeacher(object parameter)
        {
            if (SelectedTeacher != null)
            {
                var result = MessageBox.Show(
                    $"Ви впевнені, що хочете видалити викладача {SelectedTeacher.Username}?\n" +
                    "Цю дію неможливо буде скасувати.",
                    "Підтвердження видалення",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    UserService.Instance.DeleteUser(SelectedTeacher.Id);
                    MessageBox.Show($"Викладач {SelectedTeacher.Username} успішно видалений.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTeachers();
                    SelectedTeacher = null;
                }
            }
        }

        // Логіка виходу адміністратора
        private void ExecuteLogout(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                MessageBox.Show("Вихід з облікового запису адміністратора.", "Вихід", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginView = new LoginView();
                loginView.Show();
                Application.Current.MainWindow = loginView;
                currentWindow.Close();
            }
        }
    }
}