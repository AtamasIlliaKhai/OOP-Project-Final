using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Enums;
using KabukiProject.Interfaces;
using System;

namespace KabukiProject.Models
{
    public abstract class User : IUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
    }
}