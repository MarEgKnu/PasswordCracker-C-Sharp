using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PasswordCrackerServer
{
    public class SHA1Hash
    {
        public SHA1Hash()
        {
            _hash = new byte[20];
        }
        public SHA1Hash(byte[] hash)
        {
            if(hash is null)
            {
                throw new ArgumentNullException("hash");
            }
            else if (hash.Length != 20)
            {
                throw new ArgumentException($"Tried to create a SHA1 hash with {hash.Length}, but must be 20 bytes");
            }
            _hash = hash;
        }
        public byte this[int key]
        {
            get => _hash[key];
            set => _hash[key] = value;
        }
        private byte[] _hash;

        public byte[] Hash
        {
            get { return _hash; }
            set {
                if (value is null)
                {
                    throw new ArgumentNullException("hash");
                }
                else if (value.Length != 20)
                {
                    throw new ArgumentException($"Tried to create a SHA1 hash with {value.Length}, but must be 20 bytes");
                }
                _hash = value; }
            }
        public override bool Equals(object? obj)
        {
            if(obj is null || obj is not SHA1Hash sha || this.Hash.Length != sha.Hash.Length)
            {
                return false;
            }
            for(int i = 0; i < _hash.Length; i++)
            {
                if(this[i] != sha[i])
                {
                    return false;
                }
            }
            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int result = 0;
                for(int i = 0; i < _hash.Length;i++)
                {
                    result = (result * 31) ^ _hash[i];
                }
                return result;
            }
        }
    }
}
