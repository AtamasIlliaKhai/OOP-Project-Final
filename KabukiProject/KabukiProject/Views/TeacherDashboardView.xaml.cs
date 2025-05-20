using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KabukiProject.Models; //моделб Teacher
using KabukiProject.Services; //UserService
using KabukiProject.Views;
using KabukiProject.ViewModels; //ViewModel

namespace KabukiProject.Views
{
    /// <summary>
    /// Interaction logic for TeacherDashboardView.xaml
    /// </summary>
    public partial class TeacherDashboardView : Window
    {
        //Конструктор за замовчуванням (без параметрів)
        //Оцей конструктор може бути викликаний для дизайнера або якщо ViewModel без передачі даних
        public TeacherDashboardView()
        {
            InitializeComponent();
            //Цей конструктор НЕ отримує інформацію про loggedInUser,

        }

        //конструктор приймає об'єкт User
        //Передає автентифікованого користувача з LoginViewModel
        public TeacherDashboardView(User loggedInUser)
        {
            InitializeComponent();

            /*
            Встановлюємо DataContext для цього вікна на екземпляр ViewModel, передаючи йому об'єкт увійшовшого користувача.
            */
            this.DataContext = new TeacherDashboardViewModel(loggedInUser);
        }

    /*Примітка: Для PasswordBox PasswordChanged, зазвичай, обробник події знаходиться у Code-behind (тут), оскільки PasswordBox.Password не підтримує Binding.
    Но, на дашборді викладача, швидше за все, не буде PasswordBox для зміни пароля. Але подивимось

    Якщо б був PasswordBox для зміни пароля, це виглядало б так:
         private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
         {
             if (this.DataContext is TeacherDashboardViewModel viewModel)
             {
                 /* НЕМА прямої властивості "Password" у ViewModel для PasswordBox, можна создати власний Attached Property
                  АБО просто оновити властивість ViewModel тут.
    (поле для "NewPassword" viewModel.NewPassword = ((PasswordBox)sender).Password;)
             }
        } */
    }
}