using KabukiProject.Interfaces;
using KabukiProject.Models;
using KabukiProject.Services; // Додано для доступу до UserService
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace KabukiProject.ViewModels
{
    public class TeacherProfileViewModel : BaseViewModel
    {
        private Teacher _currentTeacher;
        private Student _currentStudent; // Додано для зберігання поточного студента
        public event Action LessonBooked;

        // Об'єкт викладача, профіль якого переглядається.
        public Teacher CurrentTeacher
        {
            get => _currentTeacher;
            set
            {
                _currentTeacher = value;
                OnPropertyChanged();
                // Оновлюємо всі залежні властивості, якщо Teacher змінюється
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(LastName));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(PricePerHour));
                OnPropertyChanged(nameof(Subjects));
                OnPropertyChanged(nameof(PhotoPath));
                BookLessonCommand.RaiseCanExecuteChanged(); // Оновлюємо стан команди при зміні викладача
            }
        }

        // Об'єкт поточного студента, який переглядає профіль викладача.
        public Student CurrentStudent
        {
            get => _currentStudent;
            set
            {
                _currentStudent = value;
                OnPropertyChanged();
                // Оновлюємо CanExecute команди, оскільки вона залежить від балансу студента
                BookLessonCommand.RaiseCanExecuteChanged();
            }
        }

        // Властивості, що "проксі" до CurrentTeacher для зручності прив'язки в XAML
        public string FirstName => CurrentTeacher?.FirstName;
        public string LastName => CurrentTeacher?.LastName;
        public string Description => CurrentTeacher?.Description;
        public decimal PricePerHour => CurrentTeacher?.PricePerHour ?? 0m;
        // Змінено на ObservableCollection<string> дляSubjects
        public ObservableCollection<string> Subjects { get; set; }
        public string PhotoPath => CurrentTeacher?.PhotoPath;

        // Властивості для запису на урок
        private DateTime _selectedDate;

        // Обрана учнем дата для уроку.
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                // Коли дата змінюється, оновити доступні слоти часу
                LoadAvailableTimeSlots(); // Викликаємо реальний метод
                BookLessonCommand.RaiseCanExecuteChanged(); // Можливо, впливає на доступність кнопки
            }
        }

        private TimeSpan _selectedTime;

        // Обраний учнем час для уроку.
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                OnPropertyChanged();
                BookLessonCommand.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<TimeSpan> _availableTimeSlots;

        // Колекція доступних слотів часу для обраної дати.
        public ObservableCollection<TimeSpan> AvailableTimeSlots
        {
            get => _availableTimeSlots;
            set
            {
                _availableTimeSlots = value;
                OnPropertyChanged();
            }
        }

        // Команди
        public RelayCommand BookLessonCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; } // Команда для закриття вікна профілю

        // Конструктор TeacherProfileViewModel.
        // Приймає об'єкт Teacher (чиї дані відображаються) та Student (хто бронює).
        public TeacherProfileViewModel(Teacher teacher, Student student)
        {
            if (teacher == null)
            {
                throw new ArgumentNullException(nameof(teacher), "Teacher object cannot be null for TeacherProfileViewModel.");
            }
            if (student == null)
            {
                throw new ArgumentNullException(nameof(student), "Student object cannot be null for TeacherProfileViewModel.");
            }

            CurrentTeacher = teacher;
            CurrentStudent = student; // Зберігаємо посилання на студента

            // Ініціалізуємо Subject як ObservableCollection, копіюючи дані з Teacher
            Subjects = new ObservableCollection<string>(teacher.Subjects ?? new List<string>());

            // Ініціалізуємо властивості для запису на урок
            SelectedDate = DateTime.Today; // Початково встановлюємо сьогоднішню дату
            AvailableTimeSlots = new ObservableCollection<TimeSpan>();

            // Ініціалізуємо команди
            BookLessonCommand = new RelayCommand(ExecuteBookLesson, CanExecuteBookLesson);
            CloseCommand = new RelayCommand(ExecuteClose);

            // Завантажуємо початкові доступні слоти часу
            LoadAvailableTimeSlots();
        }

        // Конструктор без параметрів для дизайнера XAML
        public TeacherProfileViewModel()
        {
            // Створюємо фіктивний об'єкт Teacher для відображення в дизайнері
            CurrentTeacher = new Teacher
            {
                FirstName = "Ім'я",
                LastName = "Викладача (Design)",
                Description = "Це фіктивний опис для дизайнера. Викладач спеціалізується на різних предметах і має багаторічний досвід.",
                PricePerHour = 250.00m,
                Subjects = new List<string> { "Математика", "Фізика", "Програмування" },
                PhotoPath = "/Assets/default_teacher_photo.png" // Шлях до фіктивного фото
            };
            // Створюємо фіктивний об'єкт Student для дизайнера
            CurrentStudent = new Student
            {
                Username = "test_student",
                Balance = 1000.00m // Фіктивний баланс студента
            };

            Subjects = new ObservableCollection<string>(CurrentTeacher.Subjects);
            SelectedDate = DateTime.Today;
            AvailableTimeSlots = new ObservableCollection<TimeSpan>();
            LoadDummyTimeSlots(); // Завантажуємо фіктивні слоти для дизайнера
            BookLessonCommand = new RelayCommand(ExecuteBookLesson, CanExecuteBookLesson);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        // Методи для команд

        private bool CanExecuteBookLesson(object parameter)
        {
            return CurrentTeacher != null &&
                   CurrentStudent != null &&
                   SelectedDate >= DateTime.Today &&
                   SelectedTime != default(TimeSpan) &&
                   CurrentStudent.Balance >= CurrentTeacher.PricePerHour;
        }

        private void ExecuteBookLesson(object parameter)
        {
            if (CanExecuteBookLesson(null))
            {
                if (CurrentTeacher.Subjects == null || !CurrentTeacher.Subjects.Any())
                {
                    MessageBox.Show("Викладач не має вказаних предметів для бронювання уроку.", "Помилка бронювання", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Ви дійсно хочете забронювати урок з {FirstName} {LastName} на {SelectedDate.ToShortDateString()} о {SelectedTime.ToString("hh\\:mm")} за {PricePerHour:C} грн?",
                                             "Підтвердження бронювання",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    CurrentStudent.Balance -= CurrentTeacher.PricePerHour;

                    UserService.Instance.UpdateUser(CurrentStudent);

                    var newLesson = new Lesson
                    {
                        Id = Guid.NewGuid().ToString(),
                        StudentId = CurrentStudent.Id,
                        TeacherId = CurrentTeacher.Id,
                        Subject = CurrentTeacher.Subjects.FirstOrDefault(),
                        DateTime = SelectedDate.Date + SelectedTime,
                        Price = CurrentTeacher.PricePerHour,
                        Status = LessonStatus.Scheduled 
                    };

                    LessonService.Instance.AddLesson(newLesson);

                    MessageBox.Show($"Урок з {FirstName} {LastName} успішно заброньовано!\n" +
                                    $"З вашого рахунку списано {PricePerHour:C}.\n" +
                                    $"Ваш новий баланс: {CurrentStudent.Balance:C} грн.",
                                    "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    LessonBooked?.Invoke();
                    ExecuteClose(parameter);
                }
            }
            else
            {
                if (CurrentStudent.Balance < CurrentTeacher.PricePerHour)
                {
                    MessageBox.Show("Недостатньо коштів на рахунку для бронювання цього уроку.", "Помилка бронювання", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Будь ласка, оберіть дату та час для уроку.", "Помилка бронювання", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ExecuteClose(object parameter)
        {
            // Метод для закриття вікна (якщо View передає Window як параметр)
            if (parameter is System.Windows.Window window)
            {
                window.Close();
            }
        }

        // Допоміжні методи

        private void LoadDummyTimeSlots()
        {
            AvailableTimeSlots.Clear();
            for (int hour = 9; hour <= 17; hour++)
            {
                AvailableTimeSlots.Add(new TimeSpan(hour, 0, 0));
            }
            if (AvailableTimeSlots.Any())
            {
                SelectedTime = AvailableTimeSlots.First();
            }
        }


        private void LoadAvailableTimeSlots()
        {
            AvailableTimeSlots.Clear();

            if (SelectedDate.Date == DateTime.Today.Date)
            {
                for (int hour = DateTime.Now.Hour + 1; hour <= 17; hour++)
                {
                    if (hour >= 9)
                    {
                        AvailableTimeSlots.Add(new TimeSpan(hour, 0, 0));
                    }
                }
            }
            else if (SelectedDate.Date > DateTime.Today.Date)
            {
                for (int hour = 9; hour <= 17; hour++)
                {
                    AvailableTimeSlots.Add(new TimeSpan(hour, 0, 0));
                }
            }

            if (AvailableTimeSlots.Any())
            {
                SelectedTime = AvailableTimeSlots.First();
            }
            else
            {
                SelectedTime = default(TimeSpan);
            }
        }
    }
}