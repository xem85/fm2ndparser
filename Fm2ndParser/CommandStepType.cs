using System;

namespace Fm2ndParser
{
    [Flags]
    public enum CommandStepType
    {
        Press = 0b0010,
        Repeat = 0b0110,
        Charge = 0b1010,
        Turn = 0b1110,
    }
}