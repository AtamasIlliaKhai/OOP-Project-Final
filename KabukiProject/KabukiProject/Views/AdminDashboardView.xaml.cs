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
using KabukiProject.Models; // Для передачі об'єкта User
using KabukiProject.ViewModels;

namespace KabukiProject.Views
{
    /// <summary>
    /// Interaction logic for AdminDashboardView.xaml
    /// </summary>
    public partial class AdminDashboardView : Window
    {
        public AdminDashboardView(User adminUser)
        {
            InitializeComponent();
            this.DataContext = new AdminDashboardViewModel(adminUser);
        }

        public AdminDashboardView()
        {
            InitializeComponent();
            // Встановлюємо DataContext з фіктивними даними для зручності дизайнера
            this.DataContext = new AdminDashboardViewModel();
        }
    }
}