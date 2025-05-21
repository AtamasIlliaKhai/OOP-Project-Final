using KabukiProject.Interfaces;
using KabukiProject.Models;
using KabukiProject.Services;
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
        private Student _currentStudent;
        public event Action LessonBooked;

        // Об'єкт викладача, профіль якого переглядається.
        public Teacher CurrentTeacher
        {
            get => _currentTeacher;
            set
            {
                _currentTeacher = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(LastName));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(PricePerHour));
                OnPropertyChanged(nameof(Subjects));
                OnPropertyChanged(nameof(PhotoPath));
                BookLessonCommand?.RaiseCanExecuteChanged();
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
                BookLessonCommand?.RaiseCanExecuteChanged();
            }
        }

        // Властивості, що "проксі" до CurrentTeacher для зручності прив'язки в XAML
        public string FirstName => CurrentTeacher?.FirstName;
        public string LastName => CurrentTeacher?.LastName;
        public string Description => CurrentTeacher?.Description;
        public decimal PricePerHour => CurrentTeacher?.PricePerHour ?? 0m;
        public ObservableCollection<string> Subjects { get; set; }
        public string PhotoPath => CurrentTeacher?.PhotoPath;

        // Властивості для запису на урок
        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                LoadAvailableTimeSlots();
                BookLessonCommand?.RaiseCanExecuteChanged();
            }
        }

        private TimeSpan _selectedTime;
        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                OnPropertyChanged();
                BookLessonCommand?.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<TimeSpan> _availableTimeSlots;
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
        public RelayCommand CloseCommand { get; private set; }

        public TeacherProfileViewModel(Teacher teacher, Student student)
        {
            BookLessonCommand = new RelayCommand(ExecuteBookLesson, CanExecuteBookLesson);
            CloseCommand = new RelayCommand(ExecuteClose);

            if (teacher == null)
            {
                throw new ArgumentNullException(nameof(teacher), "Teacher object cannot be null for TeacherProfileViewModel.");
            }
            if (student == null)
            {
                throw new ArgumentNullException(nameof(student), "Student object cannot be null for TeacherProfileViewModel.");
            }

            _currentTeacher = teacher;
            _currentStudent = student;

            AvailableTimeSlots = new ObservableCollection<TimeSpan>();
            Subjects = new ObservableCollection<string>(teacher.Subjects ?? new List<string>());

            OnPropertyChanged(nameof(CurrentTeacher));
            OnPropertyChanged(nameof(CurrentStudent));
            OnPropertyChanged(nameof(Subjects));

            // Встановлюємо SelectedDate, що ВИКЛИЧЕ LoadAvailableTimeSlots(), тепер це безпечно
            SelectedDate = DateTime.Today;

            BookLessonCommand.RaiseCanExecuteChanged(); // Оновлюємо стан команди після ініціалізації
        }

        // Конструктор без параметрів для дизайнера XAML
        public TeacherProfileViewModel()
        {
            BookLessonCommand = new RelayCommand(ExecuteBookLesson, CanExecuteBookLesson);
            CloseCommand = new RelayCommand(ExecuteClose);

            _currentTeacher = new Teacher
            {
                FirstName = "Ім'я",
                LastName = "Викладача (Design)",
                Description = "Це фіктивний опис для дизайнера. Викладач спеціалізується на різних предметах і має багаторічний досвід.",
                PricePerHour = 250.00m,
                Subjects = new List<string> { "Математика", "Фізика", "Програмування" },
                PhotoPath = "/Assets/default_teacher_photo.png"
            };
            _currentStudent = new Student
            {
                Username = "test_student",
                Balance = 1000.00m
            };

            AvailableTimeSlots = new ObservableCollection<TimeSpan>();
            Subjects = new ObservableCollection<string>(CurrentTeacher.Subjects);

            OnPropertyChanged(nameof(CurrentTeacher));
            OnPropertyChanged(nameof(CurrentStudent));
            OnPropertyChanged(nameof(Subjects));

            SelectedDate = DateTime.Today;
            LoadDummyTimeSlots();
            BookLessonCommand.RaiseCanExecuteChanged();
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
                        Status = Models.LessonStatus.Scheduled
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
                if (CurrentStudent?.Balance < CurrentTeacher?.PricePerHour)
                {
                    MessageBox.Show("Недостатньо коштів на рахунку для бронювання цього уроку. Будь ласка, поповніть баланс.", "Помилка бронювання", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (SelectedTime == default(TimeSpan) || SelectedDate < DateTime.Today)
                {
                    MessageBox.Show("Будь ласка, оберіть коректну дату та час для уроку.", "Помилка бронювання", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Не вдалося забронювати урок. Перевірте всі дані.", "Помилка бронювання", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ExecuteClose(object parameter)
        {
            if (parameter is System.Windows.Window window)
            {
                window.Close();
            }
        }

        // Допоміжні методи для слотів часу
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
            // Цей метод тепер безпечний, оскільки AvailableTimeSlots ініціалізовано в конструкторі
            AvailableTimeSlots.Clear();

            int startHour = 9;
            if (SelectedDate.Date == DateTime.Today.Date)
            {
                startHour = Math.Max(9, DateTime.Now.Hour + 1);
            }

            if (SelectedDate.Date < DateTime.Today.Date)
            {
                SelectedTime = default(TimeSpan);
                return;
            }

            if (CurrentTeacher == null)
            {
                SelectedTime = default(TimeSpan);
                return;
            }

            var bookedLessons = LessonService.Instance.GetLessonsByTeacherId(CurrentTeacher.Id)
                                                     .Where(l => l.DateTime.Date == SelectedDate.Date)
                                                     .ToList();

            for (int hour = startHour; hour <= 17; hour++)
            {
                TimeSpan potentialSlot = new TimeSpan(hour, 0, 0);

                bool isSlotBooked = bookedLessons.Any(l => l.DateTime.TimeOfDay == potentialSlot);

                if (!isSlotBooked)
                {
                    AvailableTimeSlots.Add(potentialSlot);
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