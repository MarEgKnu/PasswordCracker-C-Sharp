using PasswordCrackerServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    public class UncrackedLoginInfoDatabase
    {
        public UncrackedLoginInfoDatabase(string path)
        {
            LoadData(path);
        }
        public UncrackedLoginInfoDatabase()
        {
            _data = new Dictionary<SHA1Hash, List<LoginIdentifier>>();
        }
        Dictionary<SHA1Hash, List<LoginIdentifier>> _data;
        public Dictionary<SHA1Hash, List<LoginIdentifier>> GetAll() { return _data; }
        
        public void LoadData(string path)
        {
            Dictionary<SHA1Hash, List<LoginIdentifier>> newData = FileSerializer.GetUserPasswordHashesByHash(path);
            foreach (var item in newData)
            {
                if(!_data.TryAdd(item.Key, item.Value))
                {
                    _data[item.Key].AddRange(item.Value);
                }
            }
            
        }

    }
}
