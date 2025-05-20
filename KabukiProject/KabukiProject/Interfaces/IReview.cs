using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KabukiProject.Interfaces
{
    public interface IReview
    {
        string Id { get; set; }
        string LessonId { get; set; }
        string StudentId { get; set; }
        string TeacherId { get; set; }
        int Rating { get; set; }
        string Comment { get; set; }
        string TeacherReply { get; set; }
        DateTime DatePosted { get; set; }
        bool IsModerated { get; set; }
    }
}