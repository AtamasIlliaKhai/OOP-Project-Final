using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Enums;
using KabukiProject.Interfaces;

namespace KabukiProject.Models
{
    public class Student : User, IStudent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public decimal Balance { get; set; }

        public Student()
        {
            Role = UserRole.Student;
        }
    }
}