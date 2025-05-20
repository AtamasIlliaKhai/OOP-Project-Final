using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KabukiProject.Interfaces
{
    public interface IStudent : IUser
    {
        string FirstName { get; set; }
        string LastName { get; set; }
    }
}