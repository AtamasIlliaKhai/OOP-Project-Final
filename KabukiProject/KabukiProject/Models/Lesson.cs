using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Interfaces;
using System;

namespace KabukiProject.Models
{
    public class Lesson : ILesson
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string TeacherId { get; set; }
        public string StudentId { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Price { get; set; }
        public LessonStatus Status { get; set; } = LessonStatus.Scheduled;
        public string Subject { get; set; }
    }
    public enum LessonStatus
    {
        Scheduled,
        Completed,
        Cancelled,
        Rescheduled
    }
}