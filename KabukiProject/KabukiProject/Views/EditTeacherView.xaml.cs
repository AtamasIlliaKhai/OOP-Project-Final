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
using KabukiProject.Models;
using KabukiProject.ViewModels;

namespace KabukiProject.Views
{
    /// <summary>
    /// Interaction logic for EditTeacherView.xaml
    /// </summary>
    public partial class EditTeacherView : Window
    {
        public EditTeacherView(Teacher teacherToEdit)
        {
            InitializeComponent();
            var viewModel = new EditTeacherViewModel(teacherToEdit);
            this.DataContext = viewModel;
            // Підписуємося на подію RequestClose
            viewModel.RequestClose += () => this.Close();
        }

        // Цей конструктор потрібен лише для XAML-дизайнера, не викликається в реальному коді
        public EditTeacherView()
        {
            InitializeComponent();
            // DataContext вже визначений в XAML для дизайнера
            // var viewModel = new EditTeacherViewModel(new Teacher()); // Можна було б і так, але ми використали XAML
            // this.DataContext = viewModel;
            // viewModel.RequestClose += () => this.Close();
        }
    }
}