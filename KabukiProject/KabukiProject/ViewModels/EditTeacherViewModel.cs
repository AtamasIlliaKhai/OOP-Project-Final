using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Models;
using KabukiProject.Services;
using System.Windows;

namespace KabukiProject.ViewModels
{
    public class EditTeacherViewModel : BaseViewModel
    {
        private Teacher _originalTeacher;

        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public decimal PricePerHour { get; set; }
        public string PhotoPath { get; set; } 
        public bool IsVerified { get; set; }

        private string _subjectsInput;
        public string SubjectsInput
        {
            get => _subjectsInput;
            set
            {
                _subjectsInput = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        public event Action RequestClose;

        public EditTeacherViewModel(Teacher teacherToEdit)
        {
            if (teacherToEdit == null)
            {
                throw new ArgumentNullException(nameof(teacherToEdit), "Викладач для редагування не може бути null.");
            }


            _originalTeacher = teacherToEdit; // Це ссилка на об'єкт у колекції Teachers

            Id = teacherToEdit.Id;
            Username = teacherToEdit.Username;
            Password = teacherToEdit.Password;
            FirstName = teacherToEdit.FirstName;
            LastName = teacherToEdit.LastName;
            Description = teacherToEdit.Description;
            PricePerHour = teacherToEdit.PricePerHour;
            PhotoPath = teacherToEdit.PhotoPath;
            IsVerified = teacherToEdit.IsVerified;
            SubjectsInput = string.Join(", ", teacherToEdit.Subjects ?? new List<string>());

            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private bool CanExecuteSave(object parameter)
        {
            // Проста валідація: ім'я, прізвище, логін та ціна не повинні бути порожніми
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   PricePerHour >= 0; // Ціна має бути невід'ємною
        }

        private void ExecuteSave(object parameter)
        {
            // Оновлюємо оригінальний об'єкт викладача даними з ViewModel
            _originalTeacher.Username = Username;
            _originalTeacher.Password = Password; // Знову ж, обережно з паролями
            _originalTeacher.FirstName = FirstName;
            _originalTeacher.LastName = LastName;
            _originalTeacher.Description = Description;
            _originalTeacher.PricePerHour = PricePerHour;
            _originalTeacher.PhotoPath = PhotoPath;
            _originalTeacher.IsVerified = IsVerified;
            // Розбиваємо рядок предметів назад у List<string>
            _originalTeacher.Subjects = SubjectsInput?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(s => s.Trim())
                                                      .ToList();

            // Зберігаємо оновленого викладача через UserService
            UserService.Instance.UpdateUser(_originalTeacher);
            MessageBox.Show("Дані викладача успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

            RequestClose?.Invoke(); // Повідомляємо View, що його можна закрити
        }

        private void ExecuteCancel(object parameter)
        {
            MessageBox.Show("Редагування скасовано.", "Відміна", MessageBoxButton.OK, MessageBoxImage.Information);
            RequestClose?.Invoke(); // Повідомляємо View, що його можна закрити
        }
    }
}