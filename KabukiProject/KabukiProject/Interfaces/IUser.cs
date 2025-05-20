using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Enums;

namespace KabukiProject.Interfaces
{
    public interface IUser
    {
        string Id { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        UserRole Role { get; set; }
    }
}