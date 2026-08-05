using System;

namespace Fm2ndParser.Character
{
    [Flags]
    public enum CommandStepType : byte
    {
        Press = 0,
        Repeat = 1,
        Charge = 2,
        Turn = 3,
    }
}