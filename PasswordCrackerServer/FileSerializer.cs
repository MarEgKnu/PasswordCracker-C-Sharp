using PasswordCrackerServer.Models;
using ServerFramework.TCPServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    public static class FileSerializer
    {
        public static string[] GetWordsFromFile(string filePath)
        {
            return File.ReadAllLines(filePath);
        }
        public static List<byte[]> GetWordsBytesFromFile(string filePath)
        {
            List<byte[]> bytes = new List<byte[]>();
            string[] words = GetWordsFromFile(filePath);
            foreach(string word in words)
            {
                bytes.Add(Encoding.UTF8.GetBytes(word));
            }
            return bytes;
        }
        public static Dictionary<LoginIdentifier, SHA1Hash> GetUserPasswordHashesByLogin(string filePath)
        {
            Dictionary<LoginIdentifier, SHA1Hash> result = new Dictionary<LoginIdentifier, SHA1Hash>();
            foreach(string line in File.ReadLines(filePath))
            {
                string[] subStr = line.Split(':');
                result.Add(new LoginIdentifier(subStr[0]), new SHA1Hash(Convert.FromBase64String(subStr[1])));
            }
            return result;
        }
        public static Dictionary<SHA1Hash, List<LoginIdentifier>> GetUserPasswordHashesByHash(string filePath)
        {
            Dictionary<SHA1Hash, List<LoginIdentifier>> result = new Dictionary<SHA1Hash, List<LoginIdentifier>>();
            foreach (string line in File.ReadLines(filePath))
            {
                string[] subStr = line.Split(':');
                byte[] hash = Convert.FromBase64String(subStr[1]);
                SHA1Hash sha = new SHA1Hash(hash);
                LoginIdentifier identifier = new LoginIdentifier(subStr[0]);
                if(!result.TryAdd(sha, new List<LoginIdentifier>() {identifier })) {
                    result[sha].Add(identifier);
                }
            }
            return result;
        }
        public static void WriteCrackedPasswordToFile(string filePath, SHA1Hash hash, string plainTextPs, LoginIdentifier loginIdentifier)
        {
            using (FileStream file = File.Open(filePath, FileMode.Append))
            using (StreamWriter writer = new StreamWriter(file, Encoding.UTF8))
            {
                string result = string.Concat(loginIdentifier.Username, ";", plainTextPs, ";", Convert.ToHexString(hash.Hash));
                writer.WriteLine(result);
                                  
            }                    
        }
    }
}
