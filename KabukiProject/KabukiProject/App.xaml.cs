using System.Configuration;
using System.Data;
using System.Windows;
using KabukiProject.Services;
using KabukiProject.Views;

namespace KabukiProject
{
    public partial class App : Application
    {
        private UserService _userService; // Додаємо поле для UserService

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _userService = new UserService(); // Ініціалізуємо UserService.
                                              // Всі користувачі та dummy-дані (якщо потрібно) завантажаться в конструкторі UserService.

            var loginView = new LoginView();
            loginView.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // UserService вже керує збереженням даних, коли зміни відбуваються (наприклад, RegisterUser, UpdateUser).
            // Тому явний виклик SaveUsers() тут не потрібен, якщо ви впевнені,
            // що всі зміни зберігаються одразу.
            // Однак, якщо у вас є "брудні" дані, які не були збережені,
            // або ви хочете гарантувати, що все записано при виході, можна додати:
            // _userService?.SaveAllUsers(); // Якщо у вас є такий метод в UserService, який зберігає все

            base.OnExit(e);
        }
    }
}