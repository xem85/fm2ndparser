using System;

namespace Fm2ndParser
{
    [Flags]

    public enum ComDirection
    {
        Free,
        Point,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
        Up,
        UpRight,
        UpLeftDown,
        UpLeftRight,
        UpRightDown,
        DownLeftRight,
    }
}