using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Models;

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
        string Subject { get; set; }
    }
}