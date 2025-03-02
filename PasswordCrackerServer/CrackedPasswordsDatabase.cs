using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    public class CrackedPasswordsDatabase
    {
        public CrackedPasswordsDatabase()
        {

        }
        public IDictionary<SHA1Hash, string> CrackedPasswords {  get; set; } = new ConcurrentDictionary<SHA1Hash, string>();

        public void Add(SHA1Hash hash, string plainText)
        {
            CrackedPasswords[hash] = plainText;
        }
        public void AddRange(IDictionary<SHA1Hash, string> input)
        {
            foreach(var kvp in input)
            {
                Add(kvp.Key, kvp.Value);
            }
        }
    }
}
