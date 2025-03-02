using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordCrackerClient
{
    [Flags]
    public enum SkipWordVariations
    {
        SkipNone = 0x0,
        SkipUpper = 0x1,
        SkipLower = 0x2,
        SkipCapitalizeStartLetters = 0x4,
        SkipReverse = 0x8,
        SkipAddDigitsEnd = 0x10,
        SkipAddDigitsBeginning = 0x20,
        SkipItself = 0x40,

    }
}
