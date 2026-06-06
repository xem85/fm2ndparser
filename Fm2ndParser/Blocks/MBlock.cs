using System;

namespace Fm2ndParser
{
    public class MBlock : Block
    {
        public short GravityX { get; set; }
        public short MoveX { get; set; }
        public short MoveY { get; set; }
        public short GravityY { get; set; }
        public bool Add { get; set; }
        public bool Replace { get { return !Add; } }

        public bool StopMoveX { get; set; }
        public bool StopMoveY { get; set; }
        public bool StopGravityX { get; set; }
        public bool StopGravityY { get; set; }
    }
}