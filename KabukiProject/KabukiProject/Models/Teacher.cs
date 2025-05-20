using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Enums;
using KabukiProject.Interfaces;

namespace KabukiProject.Models
{
    public class Teacher : User, ITeacher
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public decimal PricePerHour { get; set; }
        public string PhotoPath { get; set; }
        public bool IsVerified { get; set; } = false;
        public List<string> Subjects { get; set; } = new List<string>();

        public Teacher()
        {
            Role = UserRole.Teacher;
        }
    }
}