using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KabukiProject.Interfaces
{
    public interface IJsonStorageService<T> where T : class
    {
        List<T> LoadData(string filePath);
        void SaveData(List<T> data, string filePath);
    }
}