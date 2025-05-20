using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KabukiProject.Interfaces
{
    public interface ILesson
    {
        string Id { get; set; }
        string TeacherId { get; set; }
        string StudentId { get; set; }
        DateTime DateTime { get; set; }
        decimal Price { get; set; }
        LessonStatus Status { get; set; }
    }

    public enum LessonStatus
    {
        Scheduled,
        Completed,
        Canceled
    }
}