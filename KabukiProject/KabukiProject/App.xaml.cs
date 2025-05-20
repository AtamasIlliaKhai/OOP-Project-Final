using System.Configuration;
using System.Data;
using System.Windows;
using KabukiProject.Services;
using KabukiProject.Views;

namespace KabukiProject
{
    public partial class App : Application
    {
        private UserService _userService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _userService = new UserService();

            var loginView = new LoginView();
            loginView.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}