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
            return Username.Equals(obj);
        }
        public override int GetHashCode()
        {
            return Username.GetHashCode();
        }
    }
}
