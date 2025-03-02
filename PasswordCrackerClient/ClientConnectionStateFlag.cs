using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerClient
{
    public enum ClientConnectionStateFlag
    {
        ReceivingHashes = 1,
        ReceivingWords = 2,
        NoTasksAvailable = 3,
    }
}
