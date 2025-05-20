using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Interfaces;
using System;

namespace KabukiProject.Models
{
    public class Review : IReview
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string LessonId { get; set; }
        public string StudentId { get; set; }
        public string TeacherId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string TeacherReply { get; set; }
        public DateTime DatePosted { get; set; } = DateTime.Now;
        public bool IsModerated { get; set; } = false;
    }
}