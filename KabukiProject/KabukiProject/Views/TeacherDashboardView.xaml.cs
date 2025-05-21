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

    public partial class TeacherDashboardView : Window
    {

        public TeacherDashboardView()
        {
            InitializeComponent();

        }

        public TeacherDashboardView(User loggedInUser)
        {
            InitializeComponent();
            this.DataContext = new TeacherDashboardViewModel(loggedInUser);
        }

    }
}