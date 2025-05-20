using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KabukiProject.Interfaces
{
    public interface ITeacher : IUser
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string Description { get; set; }
        decimal PricePerHour { get; set; }
        string PhotoPath { get; set; }
        bool IsVerified { get; set; }
        List<string> Subjects { get; set; }
    }
}