using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using KabukiProject.Views; //навігація LoginView

namespace KabukiProject.ViewModels
{
    public class TeacherDashboardViewModel : BaseViewModel
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

        public RelayCommand LogoutCommand { get; private set; }

        public TeacherDashboardViewModel()
        {
            CurrentUserName = "Ім'я Викладача";
            CurrentUserBalance = 2500.00m;

            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        //ПЕРЕДАЄМО ВІКНО ЯК ПАРАМЕТР
        private void ExecuteLogout(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                MessageBox.Show("Вихід з облікового запису викладача.", "Вихід", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginView = new LoginView();
                loginView.Show();
                //Важливо: Оновити Application.Current.MainWindow на нове вікно
                Application.Current.MainWindow = loginView;
                currentWindow.Close(); //Закритб поточне вікно дашборда
            }
        }
    }
}