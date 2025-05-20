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
using KabukiProject.ViewModels;

namespace KabukiProject.Views
{
    public partial class TeacherProfileView : Window
    {
        public TeacherProfileView()
        {
            InitializeComponent();
            // DataContext буде встановлено StudentDashboardViewModel при відкритті цього вікна.
        }
    }
}