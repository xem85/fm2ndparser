using System.Collections.Generic;

namespace Fm2ndParser
{
    public class CpuCommand
    {
        public string Name { get; set; }
        public byte Probability { get; set; }
        public ushort Close { get; set; }
        public ushort Far { get; set; }
        public ICollection<CpuCommandStep> Steps { get; set; }
        public bool CharacterInAir { get; set; }
        public bool EnemyInAir { get; set; }
    }
}