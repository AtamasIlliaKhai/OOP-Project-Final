using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Enums;
using KabukiProject.Interfaces;

namespace KabukiProject.Models
{
    public class Administrator : User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Administrator()
        {
            Role = UserRole.Administrator;
        }
    }
}