using System.Collections.Generic;

namespace Fm2ndParser
{
    public class Player : FMFile
    {
        public IList<Command> Commands { get; set; }
        public PlayerSettings Settings { get; set; }
        public StoryMode StoryMode { get; set; }
        public ICollection<CommonImage> CommonImages { get; set; }
        public ICollection<CpuCommand> Cpu { get; set; }
        public ICollection<HitJunctionSkills> HitJunctionsSkills { get; set; }
    }
}
