using PasswordCrackerServer.Models;
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
        public HashSet<LoginIdentifier> CrackedUsers { get; set; } = new HashSet<LoginIdentifier>();

        public void AddOrUpdatePassword(SHA1Hash hash, string plainText)
        {
            CrackedPasswords[hash] = plainText;
        }
        public void AddRange(IDictionary<SHA1Hash, string> input)
        {
            foreach(var kvp in input)
            {
                AddOrUpdatePassword(kvp.Key, kvp.Value);
            }
        }
        public bool ContainsPassword(SHA1Hash hash)
        {
            return CrackedPasswords.ContainsKey(hash);
        }
        public bool AddUser(LoginIdentifier login)
        {
            return CrackedUsers.Add(login);
        }
        public bool ContainsUser(LoginIdentifier login)
        {
            return CrackedUsers.Contains(login);
        }
    }
}
