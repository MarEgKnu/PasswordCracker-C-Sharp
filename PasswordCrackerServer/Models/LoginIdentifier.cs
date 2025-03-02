using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer.Models
{
    public class LoginIdentifier
    {
        public LoginIdentifier(string username)
        {
            Username = username;
        }
        public string Username { get; set; }

        public override bool Equals(object? obj)
        {
            if(obj is LoginIdentifier login)
            {
                return String.Equals(Username, login.Username);
            }
            return false;
            
        }
        public override int GetHashCode()
        {
            return Username.GetHashCode();
        }
        public override string ToString()
        {
            return Username;
        }
    }
}
