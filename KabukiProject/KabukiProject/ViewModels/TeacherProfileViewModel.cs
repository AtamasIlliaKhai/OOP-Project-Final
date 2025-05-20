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
            // Перевіряємо, чи обрано дату і час, чи обрана дата не в минулому,
            // і чи достатньо коштів у студента
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
                var result = MessageBox.Show($"Ви дійсно хочете забронювати урок з {FirstName} {LastName} на {SelectedDate.ToShortDateString()} о {SelectedTime.ToString("hh\\:mm")} за {PricePerHour:C} грн?",
                                     "Підтвердження бронювання",
                                     MessageBoxButton.YesNo,
                                     MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Логіка бронювання уроку
                    // Зменшуємо баланс студента
                    CurrentStudent.Balance -= CurrentTeacher.PricePerHour;
                    // ВИДАЛЕНО: CurrentTeacher.Balance += CurrentTeacher.PricePerHour;
                    // Якщо викладач має отримувати кошти в системі, додайте логіку для цього тут.
                    // Наприклад, запис транзакції, або оновлення якогось іншого поля заробітку викладача.

                    // Зберігаємо оновлені дані студента через UserService.Instance
                    // (Якщо викладач не має балансу, то його об'єкт не потребує збереження через оновлення балансу.
                    // Але якщо інші поля викладача можуть змінюватися в майбутньому (наприклад, доступність),
                    // то UpdateUser для CurrentTeacher може бути потрібним.)
                    UserService.Instance.UpdateUser(CurrentStudent);
                    // Якщо вам потрібно зберегти факт бронювання або оновити розклад викладача,
                    // ви можете викликати UserService.Instance.UpdateUser(CurrentTeacher); тут.
                    // Але для вирішення поточної помилки, рядок з CurrentTeacher.Balance був зайвим.

                    MessageBox.Show($"Урок з {FirstName} {LastName} успішно заброньовано!\nЗ вашого рахунку списано {PricePerHour:C}.\nВаш новий баланс: {CurrentStudent.Balance:C} грн.",
                                     "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Закриваємо вікно після успішного бронювання
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

        // Цей метод можна використовувати як заглушку, якщо немає реальної системи розкладу.
        private void LoadDummyTimeSlots()
        {
            AvailableTimeSlots.Clear();
            // Припустимо, викладач доступний з 9:00 до 17:00 щогодини
            for (int hour = 9; hour <= 17; hour++)
            {
                AvailableTimeSlots.Add(new TimeSpan(hour, 0, 0));
            }
            // Вибираємо перший доступний слот за замовчуванням
            if (AvailableTimeSlots.Any())
            {
                SelectedTime = AvailableTimeSlots.First();
            }
        }


        // TODO: Реальний метод для завантаження доступних слотів часу.
        // Він повинен враховувати реальний розклад викладача та вже заброньовані уроки.
        private void LoadAvailableTimeSlots()
        {
            AvailableTimeSlots.Clear();

            // Приклад реалізації:
            // В реальному проекті ви б тут зверталися до сервісу розкладу,
            // який би повертав вільні слоти для CurrentTeacher на SelectedDate.

            // Наразі просто завантажуємо заглушку для демонстрації
            // Якщо SelectedDate - це майбутня дата, можна показувати більше слотів
            // Якщо SelectedDate - сьогодні, показуємо слоти, що ще не пройшли
            if (SelectedDate.Date == DateTime.Today.Date)
            {
                for (int hour = DateTime.Now.Hour + 1; hour <= 17; hour++) // З наступної години
                {
                    if (hour >= 9) // Переконаємось, що не раніше 9 ранку
                    {
                        AvailableTimeSlots.Add(new TimeSpan(hour, 0, 0));
                    }
                }
            }
            else if (SelectedDate.Date > DateTime.Today.Date)
            {
                // Для майбутніх дат показуємо всі можливі слоти
                for (int hour = 9; hour <= 17; hour++)
                {
                    AvailableTimeSlots.Add(new TimeSpan(hour, 0, 0));
                }
            }

            // Вибираємо перший доступний слот за замовчуванням, якщо вони є
            if (AvailableTimeSlots.Any())
            {
                SelectedTime = AvailableTimeSlots.First();
            }
            else
            {
                SelectedTime = default(TimeSpan); // Скидаємо час, якщо слотів немає
            }
        }
    }
}