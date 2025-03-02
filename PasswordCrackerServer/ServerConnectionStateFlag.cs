using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerServer
{
    enum ServerConnectionStateFlag
    {
        RequestTask = 1,
        SendCrackedPasswords = 2,
        RequestHashes = 3,

    }
}
