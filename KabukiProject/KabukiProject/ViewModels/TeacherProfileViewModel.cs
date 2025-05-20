using KabukiProject.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KabukiProject.ViewModels
{
    public class TeacherProfileViewModel : BaseViewModel
    {
        private Teacher _currentTeacher;

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
            }
        }

        // Властивості, що "проксі" до CurrentTeacher для зручності прив'язки в XAML
        public string FirstName => CurrentTeacher?.FirstName;
        public string LastName => CurrentTeacher?.LastName;
        public string Description => CurrentTeacher?.Description;
        public decimal PricePerHour => CurrentTeacher?.PricePerHour ?? 0m;
        public ObservableCollection<string> Subjects { get; set; } // Може бути List<string>, але ObservableCollection краще для UI
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
                // Коли дата змінюється, оновити доступні слоти часу (реалізувати пізніше)
                // LoadAvailableTimeSlots();
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
        // Приймає об'єкт Teacher, профіль якого буде відображатися.
        // <param name="teacher">Об'єкт Teacher для відображення.</param>

        public TeacherProfileViewModel(Teacher teacher)
        {
            if (teacher == null)
            {
                // Це не повинно відбуватися, якщо логіка вибору викладача коректна,
                // але це важлива перевірка.
                throw new ArgumentNullException(nameof(teacher), "Teacher object cannot be null for TeacherProfileViewModel.");
            }

            CurrentTeacher = teacher;

            // Ініціалізуємо Subject як ObservableCollection, копіюючи дані з Teacher
            Subjects = new ObservableCollection<string>(teacher.Subjects ?? new List<string>());

            // Ініціалізуємо властивості для запису на урок
            SelectedDate = DateTime.Today; // Початково встановлюємо сьогоднішню дату
            AvailableTimeSlots = new ObservableCollection<TimeSpan>();

            // Ініціалізуємо команди
            BookLessonCommand = new RelayCommand(ExecuteBookLesson, CanExecuteBookLesson);
            CloseCommand = new RelayCommand(ExecuteClose);

            // Завантажуємо початкові доступні слоти часу (поки що просто приклад)
            LoadDummyTimeSlots();
        }

        // Конструктор без параметрів для дизайнера XAML (не використовувати в реальному коді)
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
            Subjects = new ObservableCollection<string>(CurrentTeacher.Subjects);
            SelectedDate = DateTime.Today;
            AvailableTimeSlots = new ObservableCollection<TimeSpan>();
            LoadDummyTimeSlots(); // Завантажуємо фіктивні слоти
            BookLessonCommand = new RelayCommand(ExecuteBookLesson, CanExecuteBookLesson);
            CloseCommand = new RelayCommand(ExecuteClose);
        }

        // Методи для команд

        private bool CanExecuteBookLesson(object parameter)
        {
            // Перевіряємо, чи обрано дату і час, і чи обрана дата не в минулому
            return SelectedDate >= DateTime.Today && SelectedTime != default(TimeSpan);
        }

        private void ExecuteBookLesson(object parameter)
        {
            // Ця логіка буде реалізована пізніше.
            // Тут буде відправка запиту на бронювання уроку.
            MessageBox.Show($"Забронювати урок з {FirstName} {LastName} на {SelectedDate.ToShortDateString()} о {SelectedTime.ToString("hh\\:mm")}?",
                            "Підтвердження бронювання",
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Question);
            // Після успішного бронювання можна закрити вікно
            // ExecuteClose(parameter);
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

        // Тимчасовий метод для завантаження фіктивних слотів часу.
        // В майбутньому тут буде реальна логіка зчитування розкладу викладача.
        private void LoadDummyTimeSlots()
        {
            AvailableTimeSlots.Clear();
            // Припустимо, викладач доступний з 9:00 до 17:00 щогодини
            for (int hour = 9; hour <= 17; hour++)
            {
                AvailableTimeSlots.Add(new TimeSpan(hour, 0, 0));
            }
        }


        // TODO: Метод для завантаження реальних доступних слотів часу для обраної дати.
        // Потребуватиме інтеграції з розкладом викладача або сервісом.

        /* private void LoadAvailableTimeSlots()
         {
             AvailableTimeSlots.Clear();
              Тут буде логіка отримання розкладу викладача для SelectedDate
              Наприклад: var slots = _teacherService.GetTeacherAvailability(CurrentTeacher.Username, SelectedDate);
              foreach (var slot in slots)
              {
                  AvailableTimeSlots.Add(slot);
              }
         }
        */
    }
}