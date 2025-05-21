using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Models;
using System.Globalization;

namespace KabukiProject.ViewModels
{
    public class LessonDisplayViewModel : BaseViewModel // Може наслідувати BaseViewModel
    {
        public Lesson Lesson { get; }
        public string TeacherFullName { get; }

        public string Subject => Lesson.Subject;
        public DateTime DateTime => Lesson.DateTime;
        public decimal Price => Lesson.Price;
        public LessonStatus Status => Lesson.Status;

        public string FormattedDate => Lesson.DateTime.ToString("dd.MM.yyyy");
        public string FormattedTime => Lesson.DateTime.ToString("HH:mm");
        public string FormattedPrice => Lesson.Price.ToString("C", new CultureInfo("uk-UA")); // Форматування як валюта
        public string FormattedStatus => GetStatusDisplayName(Lesson.Status); // Метод для відображення статусу

        public LessonDisplayViewModel(Lesson lesson, Teacher teacher)
        {
            Lesson = lesson ?? throw new ArgumentNullException(nameof(lesson));
            TeacherFullName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : "Невідомий викладач";
        }
        private string GetStatusDisplayName(LessonStatus status)
        {
            switch (status)
            {
                case LessonStatus.Scheduled:
                    return "Заплановано";
                case LessonStatus.Completed:
                    return "Завершено";
                case LessonStatus.Cancelled:
                    return "Скасовано";
                case LessonStatus.Rescheduled:
                    return "Перенесено";
                default:
                    return status.ToString();
            }
        }
    }
}