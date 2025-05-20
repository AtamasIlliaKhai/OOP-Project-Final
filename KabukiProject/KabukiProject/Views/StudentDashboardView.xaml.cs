using KabukiProject.Models;
using KabukiProject.ViewModels;
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
using System.Collections.ObjectModel;

namespace KabukiProject.Views
{
    /// <summary>
    /// Interaction logic for StudentDashboardView.xaml
    /// </summary>
    public partial class StudentDashboardView : Window
    {
        public StudentDashboardView()
        {
            InitializeComponent();
        }

        public StudentDashboardView(User loggedInUser)
        {
            InitializeComponent();
            this.DataContext = new StudentDashboardViewModel(loggedInUser);
        }
    }
}
