using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KabukiProject.Enums; // Забезпечити доступ до UserRole
using System.Globalization;
using System.Windows.Data; // Для IValueConverter

namespace KabukiProject.Converters
{
    public class UserRoleDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserRole role)
            {
                switch (role)
                {
                    case UserRole.Student:
                        return "Учень";
                    case UserRole.Teacher:
                        return "Викладач";
                    case UserRole.Administrator:
                        return "Адміністратор"; // Най буде для повноти, хоча адмінів не вибираємо
                    default:
                        return value.ToString(); // Поверне "Student", "Teacher" тощо
                }
            }
            // Якщо значення не UserRole, пробуємо повернути його строкове представлення так шо це може бути корисно для якшо привязка передає дивний тип
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Він міг би знадобитися, але на зараз - марний
            return value;
        }
    }
}